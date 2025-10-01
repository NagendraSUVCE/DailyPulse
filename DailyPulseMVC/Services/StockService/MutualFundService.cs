using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class MutualFundService
{
    private string _allFundsLatestNAV = string.Empty;
    public MutualFundService()
    {
        if (_allFundsLatestNAV == string.Empty)
        {
            try
            {
                // _allFundsLatestNAV = GetLatestNAVForAllFunds().Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching latest NAV: {ex.Message}");
            }
        }
    }

    public async Task<List<NAVData>> GetMutualFundNAV()
    {
        /*
          WHEN StockName ='HDFC Hybrid Equity Fund-Dir-Growth-(formerly HDFC Premier Multi-Cap Fund, erstwhile HDFC Balanced Fund merged)'	 THEN 	119062
           WHEN StockName ='HDFC Hybrid Equity Fund-Growth-(formerly HDFC Premier Multi-Cap Fund, erstwhile HDFC Balanced Fund merged)'	 THEN 	102948
           WHEN StockName ='HDFC Liquid Fund-Growth'	 THEN 	100868
           WHEN StockName ='HDFC Mid-Cap Opportunities Fund-DG'	 THEN 	118989
           WHEN StockName ='Bluechip Fund - Direct Plan Growth-(formerly ICICI Prudential Focused Bluechip Equity Fund)'	 THEN 	120586
           WHEN StockName ='Bluechip Fund - Growth-(formerly ICICI Prudential Focused Bluechip Equity Fund)'	 THEN 	108466
           WHEN StockName ='Liquid Fund - Growth-(formerly ICICI Prudential Liquid Plan)'	 THEN 	103340
           WHEN StockName ='Value Discovery Fund - DP Growth'	 THEN 	120323
           WHEN StockName ='Value Discovery Fund - Growth'	 THEN 	102594
           WHEN StockName ='SBI Blue Chip Fund Dir Plan-G'	 THEN 	119598
           WHEN StockName ='SBI Blue Chip Fund Reg Plan-G'	 THEN 	103504
           WHEN StockName ='HDFC Index Fund-NIFTY 50 Plan-Dir-(formerly HDFC Index Fund  - Nifty Plan)' THEN 119063
           WHEN StockName ='HDFC Liquid-DP-Growth Option' THEN 119091

            BFG
            GFOFT
            PREGT
            PREG
            LFGN
            LFGTN
            MCOGT
            INNPT
            8042
            1191
            1565
            8176
            DFG
            D103G
            103G
         */
        //string fundidstring = "100868";
        List<String> findidsstring = new List<string>() {
            /*"100868","102594",
                "102948",
                "103340",
                "103504",
                "108466",*/
                "118989",
                /*"119062",
                "119598",
                "120323",
                "120586","119063","119091"*/

            };

        bool bDeleteFirst = true;
        List<RootObject> lstRootObject = new List<RootObject>();
        List<FundMetaData> lstFundMetaData = new List<FundMetaData>();
        List<NAVData> lstNAVData = new List<NAVData>();

        CultureInfo provider = CultureInfo.InvariantCulture;
        DataSet ds = new DataSet();
        foreach (var fundidstring in findidsstring)
        {
            RootObject navDataOfFund = await GetNAVGivenFundId(fundidstring);

            lstRootObject.Add(navDataOfFund);
            lstFundMetaData.Add(navDataOfFund.fundData);
            foreach (var item in navDataOfFund.navListData)
            {
                item.fundid = fundidstring;
                item.dateNav = DateTime.ParseExact(item.date, "dd-MM-yyyy", provider);
            }
            lstNAVData.AddRange(navDataOfFund.navListData);
        }
        return lstNAVData;
    }

    private async Task<RootObject> GetNAVGivenFundId(string fundId)
    {
        string url = "https://www.amfiindia.com/spages/NAVAll.txt";
        url = $"https://api.mfapi.in/mf/{fundId}";
        string contents = await GetWebsiteContents(url);
        RootObject valueSet = JsonConvert.DeserializeObject<RootObject>(contents);

        return valueSet;
    }
    private async Task<decimal> GetLatestNAVGivenFundId(string fundId)
    {
        if (_allFundsLatestNAV == string.Empty)
        {
            _allFundsLatestNAV = GetLatestNAVForAllFunds().Result;
        }
        string[] lines = _allFundsLatestNAV.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            string[] columns = line.Split(';');
            if (columns.Length > 0 && columns[0] == fundId)
            {
                if (decimal.TryParse(columns[4], out decimal nav))
                {
                    return nav;
                }
            }
        }
        return 0;
    }

    private async Task<string> GetLatestNAVForAllFunds()
    {
        string url = "https://www.amfiindia.com/spages/NAVAll.txt";
        string contents = await GetWebsiteContents(url);
        return contents;

    }

    private Task<String> GetWebsiteContents(string url)
    {
        return Task.Run(() =>
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
        });
    }
}