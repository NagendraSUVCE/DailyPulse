using YahooFinanceApi;
public class YahooFinanceService
{
    public async Task GetHistoricalPriceGivenStock(string stockSymbol, DateTime startDate, DateTime endDate)
    {
        // You should be able to query data from various markets including US, HK, TW
        // The startTime & endTime here defaults to EST timezone
        var history = await Yahoo.GetHistoricalAsync("MSFT", new DateTime(2025, 1, 1), new DateTime(2025, 6, 13), Period.Daily);

        foreach (var candle in history)
        {
            Console.WriteLine($"DateTime: {candle.DateTime}, Open: {candle.Open}, High: {candle.High}, Low: {candle.Low}, Close: {candle.Close}, Volume: {candle.Volume}, AdjustedClose: {candle.AdjustedClose}");
        }
    }
}