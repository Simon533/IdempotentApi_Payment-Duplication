using Azure;
using Azure.Core;
using IdempotentApi_Payment_Duplication.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IdempotentApi_Payment_Duplication.Filters
{
    public class IdempotentActionFilter : IAsyncActionFilter
    {
        private readonly AppDbContext _dbContext;

        public IdempotentActionFilter(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        // check first first checks if the Idempotency-Key header exists and is not empty. 
        //If it's missing, it immediately stops execution and returns a 400 Bad Request ("Missing required 'Idempotency-Key' header.").
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var keyValues) ||
                string.IsNullOrWhiteSpace(keyValues))
            {
                context.Result = new BadRequestObjectResult(new { error = "Missing required 'Idempotency-Key' header." });
                return;
            }

            var key = keyValues.ToString();

            // 1. Check if response is already cached
            var cached = await _dbContext.IdempotencyRecords.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key);
            if (cached != null)
            {
                var deserializedData = JsonSerializer.Deserialize<object>(cached.ResponseBody);
                context.Result = new ObjectResult(deserializedData) { StatusCode = cached.StatusCode };
                return;
            }

            // 2. Execute endpoint action 
            //If it does NOT exist (First time request): 
            //The filter allows the request to proceed to your controller using await next(). 
            //Your controller processes the business logic, creates the order in the database, 
            //and returns a 201 Created status code along with the order details.
            var executedContext = await next();

            // 3. Cache successful results
            if (executedContext.Result is ObjectResult objectResult)
            {
                var jsonBody = JsonSerializer.Serialize(objectResult.Value);
                var record = new IdempotencyRecord
                {
                    Key = key,
                    StatusCode = objectResult.StatusCode ?? 200,
                    ResponseBody = jsonBody
                };

                _dbContext.IdempotencyRecords.Add(record);

                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // Race condition catch: concurrent requests with identical keys
                    var latest = await _dbContext.IdempotencyRecords.AsNoTracking().FirstAsync(x => x.Key == key);
                    //If it DOES exist(A retry or duplicate request): 
                    //The filter skips your controller entirely. 
                    //It takes the ResponseBody and StatusCode that were saved from the original successful run, 
                    //deserializes them, and returns the exact same cached response instantly. This ensures your business logic runs only once.
                    executedContext.Result = new ObjectResult(JsonSerializer.Deserialize<object>(latest.ResponseBody))
                    {
                        StatusCode = latest.StatusCode
                    };
                }
            }
        }
      

    }
}
