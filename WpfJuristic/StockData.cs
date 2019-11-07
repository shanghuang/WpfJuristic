using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace WpfJuristic
{
    public class StockData
    {
        String stock_index;

        StockTradeInfo[] array;
        Dictionary<DateTime, int> date_index = new Dictionary<DateTime, int>();

        public StockData(String stock_index)
        {
            this.stock_index = stock_index;
        }

        public void LoadAll()
        {
            array = DbSql.GetStockTradeHistory(stock_index, 10000, false);
            for(int i=0; i<array.Length; i++)
            {
                if(!date_index.ContainsKey(array[i].trans_date))
                    date_index.Add(array[i].trans_date, i);
            }
        }

        public StockTradeInfo getByDate(DateTime date)
        {
            int index = date_index[date];
            return array[index];
        }

        DateTime findLatest(DateTime date)
        {
            DateTime d = date;
            for(int i=0;i<30;i++)
            {
                if (date_index.ContainsKey(date))
                    break;
                d = d.AddDays(-1);
            }
            return d;
        }

        public StockTradeInfo[] getByDateConsecutive(DateTime date, int count)
        {
            ArrayList res = new ArrayList();
            if(date.Equals(new DateTime(2015 , 9 , 29)))
            {
                int a = 0;
            }
            if (!date_index.ContainsKey(date))
            {
                date = findLatest(date);
            }
            int index = date_index[date];
            for (int i = 0; i < count; i++) {
                if (index < i)
                    break;
                res.Add(array[index-i]);
            }

            return (StockTradeInfo[])res.ToArray(typeof(StockTradeInfo));
        }

        public StockTradeInfo getLatest()
        {
            int index = array.Length - 1;
            return array[index];
        }
    }
}
