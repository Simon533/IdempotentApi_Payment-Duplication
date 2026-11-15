namespace IdempotentApi_Payment_Duplication.Dtos
{
    public class OrderDtos
    {
        public record CreateOrderDto(string CustomerName, decimal Amount);
        public record UpdateOrderDto(string CustomerName, decimal Amount, string Status);
    }
}
