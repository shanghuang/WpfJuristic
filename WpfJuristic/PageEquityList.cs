using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

using System.Diagnostics;

namespace WpfJuristic
{
    public class PageEquityList : Page
    {
        public class EquityInfo
        {
            public String name { get; set; }
            public String index { get; set; }
            public String ipo_date { get; set; }

            public EquityInfo(String name, String index,String ipo_date)
            {
                this.name = name;
                this.index = index;
                this.ipo_date = ipo_date;
            }
        }

        String page_index;

        public PageEquityList(String page_index) : base("equityList_")
        {
            this.page_index = page_index;
        }

        public override void DBSave(ArrayList stock_data)
        {
            foreach(Object obj in stock_data)
            {
                EquityInfo info = (EquityInfo)obj;
                DbSql.AddEquityInfo(info);
            }
        }

        public override string getPageUrl(DateTime d)
        {
            return @"https://isin.twse.com.tw/isin/C_public.jsp?strMode=" + page_index;
        }

        public override ArrayList ParseHtml(string source, DateTime date)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(source);

            HtmlNodeCollection trs = htmlDoc.DocumentNode.SelectNodes("//table/tr");
            if(trs!=null && trs.Count < 10)
            {
                return new ArrayList();
            }
            ArrayList res = new ArrayList();
            for (int i = 0; i < trs.Count; i++)
            {
                HtmlNodeCollection tds = trs[i].SelectNodes("td");
                if (tds.Count < 5)
                    continue;

                String[] segs = tds[0].InnerText.Split(new char[] { ' ', '　' });
                if (segs.Length < 2)
                    continue;
                String date_str = tds[2].InnerText;
                int n;
                if (int.TryParse(segs[0], out n) && n<10000 && segs[0].Length == 4)
                {
                    res.Add(new EquityInfo(segs[1], segs[0], date_str));
                }
            }

            return res;
        }
    }
}
