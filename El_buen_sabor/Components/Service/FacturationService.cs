using System.Globalization;
using System.Net.Http.Json;
using El_buen_sabor.Components.Interface;
using El_buen_sabor.Components.Models;
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
            var queryParameters = new List<string>
            {
                $"PageNumber={pageNumber.ToString(CultureInfo.InvariantCulture)}",
                $"PageSize={pageSize.ToString(CultureInfo.InvariantCulture)}",
                $"Filter={Uri.EscapeDataString(filter.ToString())}"
            };

            if (fromDate.HasValue)
            {
                queryParameters.Add(
                    $"FromDate={Uri.EscapeDataString(fromDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
            }

            if (toDate.HasValue)
            {
                queryParameters.Add(
                    $"ToDate={Uri.EscapeDataString(toDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
            }

            queryParameters.Add($"_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            var url = $"api/v1/orders/facturas?{string.Join("&", queryParameters)}";
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

        public async Task ConfirmTablePaymentAsync(string tableNumber)
        {
            var response = await _http.PutAsync(
                $"api/v1/orders/facturas/table/{Uri.EscapeDataString(tableNumber)}/pay",
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task<FacturationMetricsDto> GetMetricsAsync()
        {
            var result = await _http.GetFromJsonAsync<FacturationMetricsDto>(
                $"api/v1/orders/facturas/metrics?_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");

            return result ?? new FacturationMetricsDto();
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
