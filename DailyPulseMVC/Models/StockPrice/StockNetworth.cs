
public class StockNetworth
{
    public string Symbol { get; set; }
    public string StockName { get; set; }
    public decimal? TotalQuantityOnGivenDate { get; set; }
    public DateTime? NetworthGivendate { get; set; }
    public decimal? TotalPurchasePrice { get; set; }
    public decimal? TotalNetworthOnGivenDate { get; set; }
    public decimal? TotalNetworthOnGivenDateInDollars { get; set; }
    public decimal? PriceOfStockOnGivenDate { get; set; }
    public decimal? PriceOfStockOnGiveDateInDollars { get; set; }
}
