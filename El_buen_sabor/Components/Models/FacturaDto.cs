namespace El_buen_sabor.Components.Models
{
    public class FacturaDto
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public DateTime Date { get; set; }
        public bool IsPaid { get; set; }
        public decimal Total { get; set; }

        public List<FacturaDetailDto> Details { get; set; } = new();
    }
}
