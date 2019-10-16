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
using MongoDB.Bson;
using System.Diagnostics;

namespace WpfJuristic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            combo_stock_index.Items.Add("2377");
            combo_stock_index.Items.Add("2379");
            combo_stock_index.SelectedIndex = 0;

        }

        private void btn_query_Click(object sender, RoutedEventArgs e)
        {
            String stock_selected = (String)combo_stock_index.SelectedItem;
            DbSql sql = new DbSql();
            sql.DBConnect();
            StockTradeInfo[] hist = sql.GetStockTradeHistory(stock_selected, 50);
            draw_kd(kd_canvas, hist);
            //kd_canvas.rect
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

        private void draw_kd(Canvas kd_canvas, StockTradeInfo[] hist)
        {
            double left_line_x = 24;
            double x_step = (kd_canvas.ActualWidth - left_line_x) / hist.Length;
            Range_Mapper mapper = new Range_Mapper();
            mapper.UpdateRange(hist);
            mapper.targer_size = kd_canvas.ActualHeight;// - 10;
            for (int i = 0; i < hist.Length; i++)
            {
                StockTradeInfo si = hist[i];
                double x_center = i * x_step + x_step / 2;
                SolidColorBrush color = si.close > si.open ? Brushes.Red : Brushes.Green;

                double high_map = mapper.map(si.high);
                double low_map = mapper.map(si.low);
                double open_map = mapper.map(si.open);
                double close_map = mapper.map(si.close);
                double height_map = Math.Abs(mapper.map(si.open) - mapper.map(si.close));

                Line high_low = new Line();
                high_low.Stroke = color;
                high_low.X1 = x_center;
                high_low.X2 = x_center;
                high_low.Y1 = mapper.map(si.low);
                high_low.Y2 = mapper.map(si.high);
                kd_canvas.Children.Add(high_low);

                Rectangle open_close = new Rectangle();
                Canvas.SetLeft(open_close, i * x_step);
                Canvas.SetTop(open_close, mapper.map(Math.Min(si.open, si.close)));
                open_close.Width = x_step;
                
                open_close.Height = Math.Abs(mapper.map(si.open) - mapper.map(si.close));
                open_close.Stroke = color;
                kd_canvas.Children.Add(open_close);

            }
        }
    }
}
