namespace El_buen_sabor.Components.Models
{
    public class OrderToInvoiceDto
    {
        public string TableNumber { get; set; }
        public List<OrderItemDto> Items { get; set; } = [];
    }
}
