using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Timers;
using System.Collections;
using MongoDB.Bson;
using System.Diagnostics;

namespace WpfJuristic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<BsonDocument> financial_reports;
        private ArrayList company_infos;
        public MainWindow()
        {
            InitializeComponent();
            combo_stock_index.SelectedIndex = 0;

            DbSql.DBConnect();
            company_infos = DbSql.LoadCompanyList();
            foreach(object obj in company_infos)
            {
                PageEquityList.EquityInfo ci = (PageEquityList.EquityInfo)obj;
                combo_stock_index.Items.Add(ci.index);
            }
        }

        private void btn_query_Click(object sender, RoutedEventArgs e)
        {
            String stock_selected = (String)combo_stock_index.SelectedItem;
            //StockTradeInfo[] hist = sql.GetStockTradeHistory(stock_selected, 50);
            //show_kd(kd_canvas, hist);

            DbMongo mongo = new DbMongo();
            mongo.connect();
            financial_reports = mongo.FinancialReport_FindAll(stock_selected);
            show_financial(financial_reports);
        }

        public class Range_Mapper
        {
            public double upper{get; set; }
            public double lower { get; set; }
            double range;
            public double targer_size { get; set; }

            /*public Range_Mapper(double upper, double lower)
            {
                this.upper = upper;
                this.lower = lower;
            }*/
            public void UpdateRange(StockTradeInfo[] hist)
            {
                upper = Double.MinValue;
                lower = Double.MaxValue;

                foreach (StockTradeInfo stock in hist)
                {
                    if (stock.high > upper)
                        upper = stock.high;
                    if (stock.low < lower)
                        lower = stock.low;
                }
                range = upper - lower;
            }

            public double map(double x)
            {
                return targer_size * (x - lower) / range;
            }
        }

        double flip_y(double y_height, double input)
        {
            return y_height - input;
        }

        private void show_kd(Canvas kd_canvas, StockTradeInfo[] hist)
        {
            double left_line_x = 50;
            double buttom_space_y = 30;
            double x_step = (kd_canvas.ActualWidth - left_line_x) / hist.Length;
            double kd_height = kd_canvas.ActualHeight - buttom_space_y;

            Range_Mapper mapper = new Range_Mapper();
            mapper.UpdateRange(hist);
            mapper.targer_size = kd_height;

            for (int i = 0; i < hist.Length; i++)
            {
                StockTradeInfo si = hist[i];
                double x_center = left_line_x + (49-i) * x_step + x_step / 2;
                SolidColorBrush color = si.close > si.open ? Brushes.Red : Brushes.Green;

                Line high_low = new Line();
                high_low.Stroke = color;
                high_low.X1 = x_center;
                high_low.X2 = x_center;
                high_low.Y1 = flip_y(kd_height, mapper.map(si.low));
                high_low.Y2 = flip_y(kd_height, mapper.map(si.high));
                kd_canvas.Children.Add(high_low);

                Rectangle open_close = new Rectangle();
                Canvas.SetLeft(open_close, x_center - x_step / 2);
                Canvas.SetTop(open_close, flip_y(kd_height, mapper.map(Math.Max(si.open, si.close))));
                open_close.Width = x_step;
                open_close.Height = Math.Abs(mapper.map(si.open) - mapper.map(si.close));
                open_close.Stroke = color;
                kd_canvas.Children.Add(open_close);

            }
        }

        

        public void show_financial(List<BsonDocument> financial)
        {
            BsonDocument latest = financial[0];
            foreach (BsonElement category in latest)
            {
                /*String t = category.Value.GetType().ToString();
                if (!category.Value.GetType().ToString().Contains("BsonDocument"))
                    continue;
                BsonDocument elements = category.Value.AsBsonDocument;
                String category_name = category.Name;
                foreach (BsonElement element in elements)
                {*/
                    String tt = category.Value.GetType().ToString();
                    if (!category.Value.GetType().ToString().Contains("BsonString"))
                        continue;
                this.Financial_items.Items.Add(new FinancialItem { Item = category.Name.ToString(), Value = category.Value.AsString });
                Debug.WriteLine("new FinancialItem {  Item = \"" + category.Name.ToString() + "\" ,Value=null },");

                //this.Financial_items.Items.Add(new FinancialItem { Category = category_name, Item = category.Name.ToString(), Value = category.Value.AsString });
                //Debug.WriteLine("new FinancialItem { Category =\"" + category_name + "\", Item = \"" + category.Name.ToString() + "\" ,Value=null },");
                //}
            }
        }

        private void Financial_items_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.ListView list = (System.Windows.Controls.ListView)sender;
            FinancialItem selectedObject = (FinancialItem)list.SelectedItem;
            show_financial_history(selectedObject);
        }

        private void show_financial_history(FinancialItem selectedItem)
        {
            ArrayList values = new ArrayList();
            foreach(BsonDocument financial in financial_reports)
            {
                BsonDocument group = financial[selectedItem.Category].AsBsonDocument;
                long value = group[selectedItem.Item].ToInt64();
                values.Add(value);
            }
            long[] value_aray = (long[])values.ToArray(typeof(long));
            long max = value_aray.Max();
            long min = value_aray.Min();
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            Downloader dm = new Downloader();
            //dm.getLastUpdateDate();

            ThreadStart ts = new ThreadStart(dm.DownloadToLatest);
            Thread t = new Thread(ts);
            t.Start();
        }

        private void DownloadStockName_Click(object sender, RoutedEventArgs e)
        {
            Downloader dm = new Downloader();
            dm.DownloadEquityinfo();

            MessageBox.Show("Update company list done!", "Update company list", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Evaluate_Click(object sender, RoutedEventArgs e)
        {
            Evaluator eva = new Evaluator(new StrategyForeignConsecutive());
            float ratio = eva.simulate_transation("2379");
        }
    }
}
