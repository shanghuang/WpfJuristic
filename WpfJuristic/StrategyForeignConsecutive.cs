using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfJuristic
{
    public class StrategyForeignConsecutive : Evaluator.evaluate_func
    {

        bool Evaluator.evaluate_func.buy(StockData stock_data, DateTime date)
        {
            StockTradeInfo[] last5days = stock_data.getByDateConsecutive(date, 5);
            long d1 = last5days[0].foreign_total;
            long d2 = last5days.Length >= 2 ? last5days[1].foreign_total : 0;
            long d3 = last5days.Length >= 3 ? last5days[2].foreign_total : 0;
            long d4 = last5days.Length >= 4 ? last5days[3].foreign_total : 0;

            bool val = ((d1 > 0) && (d2 < 0) && (d3 < 0) && (d4 < 0));
            return ((d1 > 0) && (d2 < 0) && (d3 < 0) && (d4 < 0));

        }

        bool Evaluator.evaluate_func.sell(StockData stock_data, DateTime date)
        {
            StockTradeInfo[] last5days = stock_data.getByDateConsecutive(date, 5);
            long d1 = last5days[0].foreign_total;
            long d2 = last5days.Length >= 2 ? last5days[1].foreign_total : 0;
            long d3 = last5days.Length >= 3 ? last5days[2].foreign_total : 0;
            long d4 = last5days.Length >= 4 ? last5days[3].foreign_total : 0;

            bool val = ((d1 < 0) && (d2 > 0) && (d3 > 0) && (d4 > 0));
            return ((d1 < 0) && (d2 > 0) && (d3 > 0) && (d4 > 0));
        }
    }
}
