namespace El_buen_sabor.Components.Models
{
    public class FacturePagedResponseDto<T>
    {
        public List<T> Items { get; set; } = new();

        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; } = new();
    }
}
