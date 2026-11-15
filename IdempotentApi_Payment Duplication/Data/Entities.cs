namespace IdempotentApi_Payment_Duplication.Data
{
    public class IdempotencyRecord
    {
        public string Key { get; set; } = null!;
        public int StatusCode { get; set; }
        public string ResponseBody { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
