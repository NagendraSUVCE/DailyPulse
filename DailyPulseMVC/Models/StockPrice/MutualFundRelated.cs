using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;


    class MF_Basic
    {
        //https://api.mfapi.in/mf/119598
        //https://www.amfiindia.com/spages/NAVAll.txt
        /*

        
108466;INF109K01BL4;-;ICICI Prudential Bluechip Fund - Growth;43.54;08-Nov-2019
103340;INF109K01VQ1;-;ICICI Prudential Liquid Fund - Growth;286.3338;10-Nov-2019
120586;INF109K016L0;-;ICICI Prudential Bluechip Fund - Direct Plan - Growth;46.21;08-Nov-2019

        
100868;INF179KB1HK0;-;HDFC Liquid Fund-GROWTH;3803.7072;10-Nov-2019
118989;INF179K01XQ0;-;HDFC Mid Cap Opportunities Fund -Direct Plan - Growth Option;55.364;08-Nov-2019
105758;INF179K01CR2;-;HDFC Mid-Cap Opportunities Fund - Growth Option;52.244;08-Nov-2019
102948;INF179K01AS4;-;HDFC Hybrid Equity Fund-Growth;54.278;08-Nov-2019
119062;INF179K01XZ1;-;HDFC Hybrid Equity -Direct Plan - Growth Option;56.734;08-Nov-2019

119598;INF200K01QX4;-;SBI BLUE CHIP FUND-DIRECT PLAN -GROWTH;43.7014;08-Nov-2019
103504;INF200K01180;-;SBI BLUE CHIP FUND-REGULAR PLAN GROWTH;41.1219;08-Nov-2019
         */
    }

    public class FundMetaData
    {
        public string fund_house { get; set; }
        public string scheme_type { get; set; }
        public string scheme_category { get; set; }
        public int scheme_code { get; set; }
        public string scheme_name { get; set; }
    }

    public class NAVData
    {
        public string fundid { get; set; }
        public string date { get; set; }
        public DateTime dateNav { get; set; }
        public string nav { get; set; }
    }

    public class RootObject
    {
        [JsonProperty("meta")]
        public FundMetaData fundData { get; set; }

        [JsonProperty("data")]
        public List<NAVData> navListData { get; set; }
        public string status { get; set; }
    }

