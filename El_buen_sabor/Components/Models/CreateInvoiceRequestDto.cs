namespace El_buen_sabor.Components.Models
{
    public class CreateInvoiceRequestDto
    {
        public List<OrderToInvoiceDto> Orders { get; set; } = [];
    }
}
