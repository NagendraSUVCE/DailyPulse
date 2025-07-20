
public class StockPurchase
{
    public string Symbol { get; set; }
    public string StockName { get; set; }
    public decimal? Quantity { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }

    public string Currency { get; set; } = "USD"; // Default currency is USD
    public string Notes { get; set; } = string.Empty; // Optional notes about the purchase
    public string PurchaseType { get; set; } = "Direct"; // Default purchase type is Direct
    public string PurchaseSource { get; set; } = "Brokerage Account"; // Default source of purchase
    public string PurchaseMethod { get; set; } = "Online"; // Default method of purchase
}
