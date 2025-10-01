
public class StockPurchase
{
    public string StockSymbol { get; set; }
    public string StockName { get; set; }
    public string YahooStockId { get; set; }
    public decimal? Quantity { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal? TotalPurchaseValue { get; set; }

    //public string Currency { get; set; } = "USD"; // Default currency is USD
    //public string Notes { get; set; } = string.Empty; // Optional notes about the purchase
    //public string PurchaseType { get; set; } = "Direct"; // Default purchase type is Direct
    //public string PurchaseSource { get; set; } = "Brokerage Account"; // Default source of purchase
    //public string PurchaseMethod { get; set; } = "Online"; // Default method of purchase
}
