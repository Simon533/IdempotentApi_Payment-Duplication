using IdempotentApi_Payment_Duplication.Data;
using IdempotentApi_Payment_Duplication.Filters;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddControllers();

// 2. Register Entity Framework Core with SQLite
builder.Services.AddDbContext<AppDbContext>(static options =>
    options.UseSqlite("Data Source=orders.db"));

// 3. Register our Custom Idempotency Action Filter
builder.Services.AddScoped<IdempotentActionFilter>();

// 4. Native OpenAPI configuration (.NET 9/10)
builder.Services.AddOpenApi();

var app = builder.Build();

// 5. Automatically create the SQLite database file on startup for local dev
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// 6. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Point Scalar to the generated OpenAPI document.
    app.MapScalarApiReference(options =>
    {
        //options.WithEndpoint("/openapi/v1.json");
        options.WithOpenApiRoutePattern("/openapi/v1.json");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "Idempotent API is running!");

app.Run();