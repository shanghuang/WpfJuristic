using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace WpfJuristic
{
    public class Evaluator
    {
        DateTime START_DATE = new DateTime(2015,1,10);

        public interface evaluate_func
        {
            bool sell(StockData stock_data, DateTime date);
            bool buy(StockData stock_data, DateTime date);
        }

        evaluate_func eval_func;

        public Evaluator(evaluate_func ev_func)
        {
            this.eval_func = ev_func;
        }
        public class Trade_Cmd
        {
            public bool is_buy { get; set; }
            public DateTime date { get; set; }
            public float price { get; set; }
            public float perf { get; set; }
        }

        public float simulate_transation(String stock_index, ArrayList trade_history )
        {
            StockData stock_data = new StockData(stock_index);
            stock_data.LoadAll();

            //ArrayList trade_history = new ArrayList();
            Boolean hold = false;
            DateTime date = START_DATE;
            while (!date.Equals(DateTime.Today))
            {
                if (TradingDays.IsTradingDay(date))
                {

                    if (hold)
                    {
                        if (eval_func.sell(stock_data, date))
                        {
                            trade_history.Add(new Trade_Cmd { is_buy = false, date = date });
                            hold = false;
                        }
                    }
                    else
                    {
                        if (eval_func.buy(stock_data, date))
                        {
                            trade_history.Add(new Trade_Cmd { is_buy = true, date = date });
                            hold = true;
                        }
                    }
                }
                date = date.AddDays(1);
            }

            return evaluate_transation(stock_data, trade_history);
        }

        float evaluate_transation(StockData stock_data, ArrayList history)
        {
            float value = 1;
            float share = 0;
            Trade_Cmd cmd;
            foreach (object obj in history)
            {
                cmd = (Trade_Cmd)obj;
                if (cmd.is_buy)
                {
                    StockTradeInfo trade_info = stock_data.getByDate(cmd.date);
                    float buy_price = trade_info.open;
                    share = value / buy_price;
                    value = 0;
                    cmd.price = buy_price;
                }
                else
                {
                    StockTradeInfo trade_info = stock_data.getByDate(cmd.date);
                    float sell_price = trade_info.open;
                    value = share * sell_price;
                    share = 0;
                    cmd.price = sell_price;
                    cmd.perf = value;
                }
            }
            if (share > 0)
            {
                StockTradeInfo trade_info = stock_data.getLatest();
                float sell_price = trade_info.open;
                value = share * sell_price;
            }
            return value;
        }
    }
}
