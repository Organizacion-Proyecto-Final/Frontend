using El_buen_sabor.Components.Models;
using static El_buen_sabor.Components.Pages.SectionsAdmin.FacturationSection;

namespace El_buen_sabor.Components.Interface
{
    public interface IFacturationService
    {
        Task<FacturePagedResponseDto<FacturaDto>> GetFacturasAsync(
            int pageNumber,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate,
            PaymentFilter filter);

        Task ConfirmPaymentAsync(int facturaId);
        Task ConfirmOrdersForInvoiceAsync(List<OrderToInvoiceDto> orders);
    }
}
