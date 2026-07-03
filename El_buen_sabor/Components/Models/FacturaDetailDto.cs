namespace El_buen_sabor.Components.Models
{
    public class FacturaDetailDto
    {
        public int Quantity { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
