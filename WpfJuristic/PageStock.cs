using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Collections;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MongoDB.Bson;

namespace WpfJuristic
{
    public class PageStock : Page
    {
        string stock_db_name = "tw_stock";
        DateTime format_2015_start = new DateTime(2015, 1, 1);

        public PageStock() : base("stock_")
        {

        }

        public override String getPageUrl(DateTime date)
        {
            string url_string = @"https://www.twse.com.tw/exchangeReport/MI_INDEX?response=json&date=" +
                string.Format("{0:D4}{1:D2}{2:D2}", date.Year, date.Month, date.Day) +
                @"&type=ALLBUT0999&_=1568988527622";
            return url_string;
        }

        public override ArrayList ParseHtml( string source, DateTime date)
        {
            Logger.Log("Parsing Stock Page:"+date.ToString());

            ArrayList stocks = new ArrayList();

            if (!IsValidJson(source))
            {
                return stocks;
            }

            JObject obj = JObject.Parse(source);

            if (obj == null || obj["data9"] == null)
            {
                return stocks;
            }

            foreach (JArray stk in obj["data9"])
            {
                //String stock_str = stk.ToString();
                //stock_str = stock_str.Substring(1, stock_str.Length - 2);   //remove []
                JToken[] token_ary = stk.ToArray();
                String[] stk_strings = new string[token_ary.Length];
                for (int i = 0; i < token_ary.Length; i++)
                    stk_strings[i] = token_ary[i].ToString();

                StockTradeInfo stock_info = new StockTradeInfo(stk_strings, date);
                stocks.Add(stock_info);
            }
            return stocks;
        }


        public override void DBSave( ArrayList stock_data)
        {
            DbSql.StockData_Save(stock_data);
        }
    }
}
