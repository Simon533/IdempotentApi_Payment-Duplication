using IdempotentApi_Payment_Duplication.Data;
using IdempotentApi_Payment_Duplication.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static IdempotentApi_Payment_Duplication.Dtos.OrderDtos;

namespace IdempotentApi_Payment_Duplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }
        // 1. GET: Retrieve all orders (Naturally idempotent, no idempotency filter required)
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders.AsNoTracking().ToListAsync();
            return Ok(orders);
        }
        // 1. GET: Retrieve an order by ID (Naturally idempotent, no filter needed)
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { error = $"Order with ID {id} not found." });
            }

            return Ok(order);
        }
        // 2. POST: Create a new order (Protected by Idempotency Filter)
        [HttpPost]
        [ServiceFilter(typeof(IdempotentActionFilter))]

        //execution passes through your IdempotentActionFilter.OnActionExecutionAsync method:
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var order = new Order
            {
                CustomerName = dto.CustomerName,
                Amount = dto.Amount,
                Status = "Completed"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { order.Id, order.CustomerName, order.Amount, order.Status });
        }
        // 3. PUT: Update an existing order (Protected by Idempotency Filter)
        [HttpPut("{id:int}")]
        [ServiceFilter(typeof(IdempotentActionFilter))]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto dto)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound(new { error = $"Order with ID {id} not found." });
            }

            // Update fields from dtabase here
            order.CustomerName = dto.CustomerName;
            order.Amount = dto.Amount;
            order.Status = dto.Status;

            await _context.SaveChangesAsync();

            return Ok(new { order.Id, order.CustomerName, order.Amount, order.Status });
        }
    }

    }

