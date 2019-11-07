using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace WpfJuristic
{
    public class PageForeign : Page
    {
        string table_name = "juristic";

        public PageForeign()
            : base("foreign_")
        {
        }


        public override String getPageUrl(DateTime date)
        {
            int tw_year = date.Year - 1911;
            string url_string = @"https://www.twse.com.tw/fund/TWT38U?response=json&date=" + string.Format("{0:D4}{1:D2}{2:D2}", date.Year, date.Month, date.Day) + "&_=1232123123123";
            return url_string;
        }

        public override ArrayList ParseHtml( string source, DateTime date)
        {
            Logger.Log("Parsing Foreign Page:" + date.ToString());
            JObject obj = JObject.Parse(source);
            ArrayList stocks = new ArrayList();

            if (!IsValidJson(source))
            {
                return stocks;
            }

            if (obj == null || obj["data"] == null)
            {
                return stocks;
            }

            foreach (JArray stk in obj["data"])
            {
                //String stock_str = stk.ToString();
                //stock_str = stock_str.Substring(1, stock_str.Length - 2);   //remove []
                JToken[] token_ary = stk.ToArray();
                String[] stk_strings = new string[token_ary.Length];
                for (int i = 0; i < token_ary.Length; i++)
                    stk_strings[i] = token_ary[i].ToString();

                JuristicTradeInfo stock_info = new JuristicTradeInfo(stk_strings, date);
                stocks.Add(stock_info);
            }

            return stocks;
        }

            

        /*public DateTime getLastDBDate(DbSql db)
        {
            DateTime res = new DateTime(2004, 12, 18);        //first date
                                                                
            String qstr = "SELECT * from " + table_name + "  ORDER by trans_date DESC LIMIT 1;";
            MySqlCommand cmd = new MySqlCommand(qstr, db.getConnection());
            MySqlDataReader rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
            {
                rdr.Read();
                res = (DateTime)rdr["trans_date"];
            }
            rdr.Close();
            return res;
        }*/

        public override void DBSave( ArrayList stock_data)
        {
            DbSql.StockData_SaveForeign(stock_data);
        }
    }
}
