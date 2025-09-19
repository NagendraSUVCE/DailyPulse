using YahooFinanceApi;
public class YahooFinanceService
{
    public async Task<Candle> GetLatestPriceGivenStock(string stockSymbol)
    {
        var securities = await GetHistoricalPriceGivenStock(stockSymbol, DateTime.Now.AddDays(-5), DateTime.Now, Period.Daily);
        if (securities == null || securities.Count == 0)
        {
            throw new Exception($"No data found for stock symbol: {stockSymbol}");
        }
        return securities.Last();
    }
    public async Task<Candle> GetPriceGivenStockOnGivenDate(string stockSymbol, DateTime givenDate)
    {
        var securities = await GetHistoricalPriceGivenStock(stockSymbol, givenDate.AddDays(-5), givenDate, Period.Daily);
        if (securities == null || securities.Count == 0)
        {
            throw new Exception($"No data found for stock symbol: {stockSymbol}");
        }
        return securities.Last();
    }

    public async Task<List<Candle>> GetHistoricalPriceGivenStock(string stockSymbol, DateTime startDate, DateTime endDate, Period per)
    {
        // You should be able to query data from various markets including US, HK, TW
        // The startTime & endTime here defaults to EST timezone
        var history = await Yahoo.GetHistoricalAsync(stockSymbol, startDate.Date, endDate.Date, per);

        foreach (var candle in history)
        {

            if (candle.DateTime.Month == 1 && candle.DateTime.Day >= 1 && candle.DateTime.Day <= 5)
            {
                Console.WriteLine($"stockSymbol: {stockSymbol}, period : {per.ToString()}, DateTime: {candle.DateTime.Date}, Open: {candle.Open}, High: {candle.High}, Low: {candle.Low}, Close: {candle.Close}, Volume: {candle.Volume}, AdjustedClose: {candle.AdjustedClose}");
            }
        }
        return history.ToList();
    }
    
    public decimal CalculateCAGR(Candle startCandle, Candle endCandle)
    {
        if (startCandle == null || endCandle == null)
        {
            throw new ArgumentNullException("Candles cannot be null.");
        }

        if (startCandle.DateTime >= endCandle.DateTime)
        {
            throw new ArgumentException("Start candle date must be earlier than end candle date.");
        }

        // Calculate the number of years between the two dates
        double years = (endCandle.DateTime - startCandle.DateTime).TotalDays / 365.0;

        if (years <= 0)
        {
            throw new ArgumentException("Invalid time period for CAGR calculation.");
        }

        // Calculate CAGR using the formula: CAGR = (EndValue / StartValue)^(1 / Years) - 1
        decimal startValue = startCandle.AdjustedClose > 0 ? startCandle.AdjustedClose : startCandle.Close;
        decimal endValue = endCandle.AdjustedClose > 0 ? endCandle.AdjustedClose : endCandle.Close;

        if (startValue <= 0 || endValue <= 0)
        {
            throw new ArgumentException("Candle close values must be greater than zero.");
        }

        decimal cagr = (decimal)Math.Pow((double)(endValue / startValue), 1 / years) - 1;
        return cagr;
    }
}