using El_buen_sabor.Components.Interface;
using El_buen_sabor.Components.Models;

namespace El_buen_sabor.Components.Service
{
    public class OperationService : IOperationService
    {
        private readonly ITableService _tableService;
        private readonly IFacturationService _facturationService;
        private readonly List<OrderFromTable> items = [];
        private Table? table;

        public OperationService(ITableService tableService, IFacturationService facturationService)
        {
            _tableService = tableService;
            _facturationService = facturationService;
        }

        public event Action? OnChange;

        public void AddProduct(Product product)
        {
            var item = items.FirstOrDefault(x => x.Producto.Id == product.Id);

            if (item == null)
            {
                items.Add(new OrderFromTable
                {
                    Producto = product,
                    Cantidad = 1
                });
            }
            else
            {
                item.Cantidad++;
            }

            OnChange?.Invoke();
        }

        public List<OrderFromTable> GetItems() => items;

        public Table? GetTable() => table;

        public void RemoveProduct(Guid productId)
        {
            var item = items.FirstOrDefault(x => x.Producto.Id == productId);
            if (item != null)
            {
                items.Remove(item);
                OnChange?.Invoke();
            }
        }

        public void Clear()
        {
            items.Clear();
            OnChange?.Invoke();
        }

        public void IncreaseQuantity(Guid productId)
        {
            var item = items.FirstOrDefault(x => x.Producto.Id == productId);
            if (item != null)
            {
                item.Cantidad++;
                OnChange?.Invoke();
            }
        }

        public void DecreaseQuantity(Guid productId)
        {
            var item = items.FirstOrDefault(x => x.Producto.Id == productId);
            if (item != null && item.Cantidad > 1)
            {
                item.Cantidad--;
                OnChange?.Invoke();
            }
        }

        public void SetTable(Table selectedTable)
        {
            table = selectedTable;
            OnChange?.Invoke();
        }

        public void ClearTable()
        {
            table = null;
            items.Clear();
            OnChange?.Invoke();
        }

        public void NotifyChanged()
        {
            OnChange?.Invoke();
        }

        public void AddNote(Guid productId, string note)
        {
            var item = items.FirstOrDefault(x => x.Producto.Id == productId);
            if (item != null)
            {
                item.Notes = note;
                OnChange?.Invoke();
            }
        }

        public async Task<OperationResultDto> SendOrderAsync()
        {
            if (table is null)
                return Fail("Seleccioná una mesa antes de confirmar la orden.");

            if (items.Count == 0)
                return Fail("Agregá al menos un producto antes de confirmar la orden.");

            var cartItems = items.ToList();
            var requestItems = cartItems.Select(item => new CreateOrderItemRequest
            {
                ProductId = item.Producto.Id,
                ProductType = item.Producto.ProductType,
                Quantity = item.Cantidad,
                Notes = item.Notes
            }).ToList();

            var activeOrder = await GetActiveOrderAsync();

            if (activeOrder?.Status == OrderStatuses.ReadyToClose)
                return Fail("La cuenta ya fue solicitada. No se pueden agregar productos.");

            OperationResultDto result;

            // Si la mesa ya tiene una orden cocinando (InProgress), creamos una orden NUEVA
            // para esa mesa en vez de agregar a la existente: cada orden se cocina y se entrega
            // por separado, y no rompemos la sincronización de la cocina.
            if (activeOrder is not null && activeOrder.Status == OrderStatuses.Open)
            {
                result = new OperationResultDto { Success = true, Message = "Productos agregados correctamente." };

                foreach (var item in requestItems)
                {
                    result = await _tableService.AddItemToOrderAsync(activeOrder.Id, item);
                    if (!result.Success)
                    {
                        OnChange?.Invoke();
                        return result;
                    }

                    var sentItem = items.FirstOrDefault(cartItem => cartItem.Producto.Id == item.ProductId);
                    if (sentItem is not null)
                        items.Remove(sentItem);
                }
            }
            else
            {
                result = await _tableService.CreateOrderAsync(new CreateOrderRequest
                {
                    TableId = table.Id,
                    Items = requestItems
                });
            }

            if (result.Success)
            {
                items.Clear();
                OnChange?.Invoke();
            }

            return result;
        }

        public async Task<OperationResultDto> RequestBillAsync() /////////////////////// aca 
        {
            var activeOrders = await GetActiveOrdersAsync();
            if (activeOrders.Count == 0)
                return Fail("No hay órdenes activas para esta mesa.");

            if (!activeOrders.All(AllItemsDeliveredOrCancelled))
                return Fail("Entregá todos los platos de todas las órdenes antes de pedir la cuenta.");

            OperationResultDto result = new() { Success = true, Message = "La cuenta ya fue solicitada." };

            var ordersToInvoice = new List<OrderToInvoiceDto>();


            foreach (var order in activeOrders)
            {
                if (order.Status == OrderStatuses.ReadyToClose)
                    continue;

                result = await _tableService.ChangeOrderStatusAsync(order.Id, OrderStatuses.ReadyToClose);
                if (!result.Success)
                {
                    OnChange?.Invoke();
                    return result;
                }

                ordersToInvoice.Add(new OrderToInvoiceDto
                {

                    TableNumber = order.TableNumber,
                    Items = order.OrderItems.Select(i => new OrderItemDto
                    {
                        ProductName = i.Producto.Name,
                        Quantity = i.Cantidad,
                        Price = i.Producto.Price
                    }).ToList()
                });

            }

            try
            {
                await _facturationService.ConfirmOrdersForInvoiceAsync(ordersToInvoice);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Factura falló: {ex.Message}");
            }

            result.Message = "Cuenta solicitada.";
            OnChange?.Invoke();
            return result;
        }

        public async Task<OperationResultDto> ReleaseTableAsync()
        {
            var activeOrders = await GetActiveOrdersAsync();
            if (activeOrders.Count == 0)
                return Fail("No hay órdenes activas para liberar.");

            if (!activeOrders.All(order => order.Status == OrderStatuses.ReadyToClose))
                return Fail("Primero solicitá la cuenta de todas las órdenes.");

            OperationResultDto result = new() { Success = true, Message = "Mesa liberada." };


            foreach (var order in activeOrders)
            {
                result = await _tableService.ChangeOrderStatusAsync(order.Id, OrderStatuses.Closed);

                if (!result.Success)
                    return result;

            }


            OnChange?.Invoke();
            return result;
        }

        private async Task<Order?> GetActiveOrderAsync()
        {
            if (table is null)
                return null;

            var activeSummary = await _tableService.GetActiveOrdersSummaryByTableAsync(table.Id);
            if (activeSummary is not null)
                return activeSummary.Orders.FirstOrDefault(IsActiveOrder);

            var orders = await _tableService.GetOrdersByTableAsync(table.Id);
            return orders.FirstOrDefault(IsActiveOrder);
        }

        private async Task<List<Order>> GetActiveOrdersAsync()
        {
            if (table is null)
                return [];

            var orders = await _tableService.GetOrdersByTableAsync(table.Id);
            return orders.Where(IsActiveOrder).ToList();
        }

        private static bool AllItemsDeliveredOrCancelled(Order order)
            => order.OrderItems.Count > 0
            && order.OrderItems.All(item => item.Status is OrderItemStatuses.Delivered or OrderItemStatuses.Cancelled);

        private static bool IsActiveOrder(Order order)
            => order.Status is not OrderStatuses.Closed and not OrderStatuses.Cancelled;

        private static OperationResultDto Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}
