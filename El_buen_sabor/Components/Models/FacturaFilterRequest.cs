using static El_buen_sabor.Components.Pages.SectionsAdmin.FacturationSection;

namespace El_buen_sabor.Components.Models
{
    public class FacturaFilterRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public PaymentFilter PaymentFilter { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
