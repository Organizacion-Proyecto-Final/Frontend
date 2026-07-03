using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using El_buen_sabor.Components.Models;
using El_buen_sabor.Components.Interface;
using static El_buen_sabor.Components.Pages.SectionsAdmin.FacturationSection;


namespace El_buen_sabor.Components.Service
{

    public class FacturationService : IFacturationService
    {
        private readonly HttpClient _http;

        public FacturationService(HttpClient http)
        {
            _http = http;
        }

        public async Task<FacturePagedResponseDto<FacturaDto>> GetFacturasAsync(
            int pageNumber,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate,
            PaymentFilter filter)
        {
            var url =
                $"api/v1/orders/facturas?PageNumber={pageNumber}" +
                $"&PageSize={pageSize}" +
                $"&Filter={(int)filter}";

            if (fromDate.HasValue)
                url += $"&FromDate={fromDate.Value:O}";

            if (toDate.HasValue)
                url += $"&ToDate={toDate.Value:O}";


            var result = await _http.GetFromJsonAsync<FacturePagedResponseDto<FacturaDto>>(url);

            return result ?? new FacturePagedResponseDto<FacturaDto>();
        }

        public async Task ConfirmPaymentAsync(int facturaId)
        {
            var response = await _http.PutAsync(
                $"api/v1/orders/facturas/{facturaId}/pay",
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task ConfirmOrdersForInvoiceAsync(List<OrderToInvoiceDto> orders)
        {
            Console.WriteLine(orders == null
                ? "Orders es NULL"
                : $"Orders tiene {orders.Count} órdenes");

            var request = new CreateInvoiceRequestDto
            {
                Orders = orders
            };

            var response = await _http.PostAsJsonAsync(
                "api/v1/orders/facturas/from-orders",
                request);

            Console.WriteLine($"RESPUESTA FACTURA: {response.StatusCode}");

            response.EnsureSuccessStatusCode();
        }

    }
}
