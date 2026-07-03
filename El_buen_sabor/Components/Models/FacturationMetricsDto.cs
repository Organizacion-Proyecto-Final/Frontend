namespace El_buen_sabor.Components.Models;

public sealed class FacturationMetricsDto
{
    public decimal TodayTotal { get; set; }
    public int TodayCount { get; set; }
    public decimal MonthTotal { get; set; }
    public int MonthCount { get; set; }
    public List<ProductMetricDto> TopProducts { get; set; } = [];
    public List<ProductMetricDto> TopRevenueProducts { get; set; } = [];
    public List<HourlyMetricDto> HourlyConcurrency { get; set; } = [];
    public List<WeekdayMetricDto> WeeklyConcurrency { get; set; } = [];
}

public sealed class ProductMetricDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class HourlyMetricDto
{
    public int Hour { get; set; }
    public int InvoiceCount { get; set; }
}

public sealed class WeekdayMetricDto
{
    public int DayOfWeek { get; set; }
    public int InvoiceCount { get; set; }
}
