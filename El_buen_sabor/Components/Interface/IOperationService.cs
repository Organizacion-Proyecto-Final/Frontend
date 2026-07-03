namespace El_buen_sabor.Components.Interface
{
    using Components.Models;
    public interface IOperationService
    {

        event Action? OnChange;
        void AddProduct(Product product);
        List<OrderFromTable> GetItems();
        Table? GetTable();
        void SetTable(Table t);
        void ClearTable();
        void NotifyChanged();
        void RemoveProduct(Guid productId);
        void Clear();
        void IncreaseQuantity(Guid productId);
        void DecreaseQuantity(Guid productId);
        public void AddNote(Guid productId, string note);
        public Task<OperationResultDto> SendOrderAsync();
        public Task<OperationResultDto> RequestBillAsync();
        public Task<OperationResultDto> ConfirmTablePaymentAsync();
        public Task<OperationResultDto> ReleaseTableAsync();


    }
}
