using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Collections;
using MongoDB.Bson;
using HtmlAgilityPack;

using System.Diagnostics;

namespace WpfJuristic
{
    public class PageFinancial
    {
        public String getPageUrl(String stock_index, int year, int season)
        {
            String path = string.Format(@"https://mops.twse.com.tw/server-java/t164sb01?step=1&CO_ID={0}&SYEAR={1:D4}&SSEASON={2:D2}&REPORT_ID=C", stock_index, year+1911, season);
            return path;
        }

        public String getFileName(String stock_index, int year, int season)
        {
            String path = string.Format(@"FinReport_{0}_{1:D4}_{2:D2}.html", stock_index, year, season);
            return path;
        }

        public string download_page(string url, String stock_index, int year, int season)
        {
            string strResult = string.Empty;

            WebResponse objResponse;
            WebRequest objRequest = System.Net.HttpWebRequest.Create(url);

            try
            {
                objResponse = objRequest.GetResponse();

                using (StreamReader sr = new StreamReader(objResponse.GetResponseStream(), Encoding.GetEncoding("big5"))) //, Encoding.Unicode
                {
                    strResult = sr.ReadToEnd();
                    // Close and clean up the StreamReader
                    sr.Close();
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
            }
            return strResult;
        }


        public BsonDocument ParseHtml(string source)
        {
            int[] subtable_start = new int[] { 4, 14, 26, 38, 46, 49, 54, 59 };

            //BsonDocument stocks = new BsonDocument();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(source);

            HtmlNodeCollection tables = htmlDoc.DocumentNode.SelectNodes("//table");
            if ((tables == null) || (tables.Count < 2))
                return new BsonDocument();

            BsonDocument financial = new BsonDocument();

            for (int table_index = 0; table_index < 4; table_index++)
            {
                HtmlNodeCollection trs = tables[table_index].SelectNodes("tr");

                /*HtmlNodeCollection th_span = trs[0].SelectNodes("th/span");
                if(th_span.Count > 0)
                {
                    String test = th_span[0].InnerText;
                }*/
                if (trs == null || trs.Count < 10)
                    continue;

                //String subtable_key = "";
                BsonDocument section = new BsonDocument();
                for (int tr_index = 2; tr_index < trs.Count; tr_index++)
                {

                    HtmlNodeCollection tds = trs[tr_index].SelectNodes("td");
                    if (tds == null || tds.Count < 2)
                        continue;
                    String name, value;
                    HtmlNodeCollection spans = tds[1].SelectNodes("span");
                    if(spans==null || spans.Count == 0)
                    {
                        name = tds[0].InnerText.Replace(" ", "").TrimStart();
                        value = tds[1].InnerText.Replace(",", "").Trim();
                    }
                    else
                    {
                        name = spans[0].InnerText.Replace(" ", "").TrimStart();
                        value = tds[2].InnerText.Replace(",", "").Trim();
                    }

                    String name_utf8 = Util.Big5ToUtf8(name);
                    String value_utf8 = Util.Big5ToUtf8(value);
                    if (!financial.Contains(name_utf8))
                        financial.Add(name_utf8, value_utf8);

                    Debug.Write(table_index.ToString() + " " + tr_index.ToString() + " ");

                    for (int td_index = 0; td_index < tds.Count; td_index++)
                    {
                        Debug.Write(tds[td_index].InnerText + " ");
                    }

                    Debug.WriteLine("");
                }
                //financial.Add(subtable_key, section);
            }

            return financial;
        }

        public void DBSave(BsonDocument financial, String stock_index, int year, int season)
        {
            DbMongo db = new DbMongo();
            db.connect();
            db.FinancialReport_save(financial, stock_index, year, season);
        }
    }
}
