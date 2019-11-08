using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Collections;
using System.Diagnostics;
using MongoDB.Bson;

namespace WpfJuristic
{
    public class Downloader
    {
        private String TargetDirectory = @"C:\htmldwl\";
        private DateTime StartDate = new DateTime(2015, 1, 4);
        private String current_download_status;

        /*public void Download_PerDay(DateTime date)
        {
            dynamic[] equility_pages = new dynamic[] { new PageStock(), new PageForeign() };//, new PageJuristic("selfoper_"), new PageJuristic("investtrust_"), };

            foreach (dynamic page in equility_pages)
            {
                current_download_status = "Downloading " + page.getFilePrefix() + date.ToShortDateString();
                Logger.Log(current_download_status);
                page.download_page(page.getPageUrl(date),   date);
                Thread.Sleep(5000);
            }
        }*/

        public void BatchDownloadStockTrading()
        {
            dynamic[] equility_pages = new dynamic[] { new PageStock(), new PageForeign() };

            //DateTime date = checkDownload ? StartDate : equility_pages[0].getLastDBDate(sql_db, "2379");
            DateTime date = DbSql.getLastDBDate( "2379");
            date = date.CompareTo(StartDate) > 0 ? date : StartDate;

            while (!date.Equals(DateTime.Today))
            {
                date = date.AddDays(1);
                //Logger.v("Downloading data starting from " + String.Format("{0}", date));
                if (!TradingDays.IsTradingDay(date))
                    continue;
                //Download_PerDay(date);
                foreach (dynamic page in equility_pages)
                {
                    String content = String.Empty;
                    if (!File.Exists(TargetDirectory + page.getFileName(date)))
                    {
                        current_download_status = "Downloading " + page.getFilePrefix() + date.ToShortDateString();
                        Logger.Log(current_download_status);
                        content = page.download_page(page.getPageUrl(date), Encoding.GetEncoding("utf-8"));
                        Thread.Sleep(5000);
                        using (StreamWriter sw = new StreamWriter(TargetDirectory + page.getFileName(date), false, Encoding.GetEncoding("utf-8")))
                        {
                            sw.Write(content);
                            sw.Close();
                        }
                    }
                    else
                    {
                        using (StreamReader sr = new StreamReader(TargetDirectory + page.getFileName(date), Encoding.GetEncoding("utf-8")))
                        {
                            content = sr.ReadToEnd();
                        }
                    }
                    ArrayList stock_data = page.ParseHtml(content, date);
                    if (stock_data.Count > 0)
                    {
                        page.DBSave(stock_data);
                    }
                }
                //Thread.Sleep(5000);
            }
        }

        public void BatchDownloadStockTrading_FixMissing()
        {
            dynamic[] equility_pages = new dynamic[] { new PageStock(), new PageForeign() };

            //DateTime date = checkDownload ? StartDate : equility_pages[0].getLastDBDate(sql_db, "2379");
            DateTime date = StartDate;

            while (!date.Equals(DateTime.Today))
            {
                date = date.AddDays(1);
                //Logger.v("Downloading data starting from " + String.Format("{0}", date));
                if (!TradingDays.IsTradingDay(date))
                    continue;
                //Download_PerDay(date);
                foreach (dynamic page in equility_pages)
                {
                    String content = String.Empty;
                    if (!File.Exists(TargetDirectory + page.getFileName(date)))
                    {
                        current_download_status = "Downloading " + page.getFilePrefix() + date.ToShortDateString();
                        Logger.Log(current_download_status);
                        content = page.download_page(page.getPageUrl(date), Encoding.GetEncoding("utf-8"));
                        Thread.Sleep(5000);
                        using (StreamWriter sw = new StreamWriter(TargetDirectory + page.getFileName(date), false, Encoding.GetEncoding("utf-8")))
                        {
                            sw.Write(content);
                            sw.Close();
                        }
                    }
                    else
                    {
                        using (StreamReader sr = new StreamReader(TargetDirectory + page.getFileName(date), Encoding.GetEncoding("utf-8")))
                        {
                            content = sr.ReadToEnd();
                        }
                    }
                    ArrayList stock_data = page.ParseHtml(content, date);
                    if (stock_data.Count > 0)
                    {
                        page.DBSave(stock_data);
                    }
                }
                //Thread.Sleep(5000);
            }
        }

        public void DownloadToLatest()
        {
            BatchDownloadStockTrading();
            BatchDownloadFinancialReport();
           //BatchDownloadStockTrading_FixMissing();
        }

        /*
         * Season start with 1.
         * .Seasonal report is available 45 days after the season
         */
        public bool SeasonalReportReady(int year, int season)
        {
            int avail_year, avail_month;
            if (season == 4)
            {
                avail_year = year + 1;
                avail_month = 2;
            }
            else
            {
                avail_year = year;
                avail_month = (season) * 3 + 2;
            }
            DateTime avail = new DateTime(avail_year + 1911, avail_month, 16);

            return avail.CompareTo(DateTime.Now) < 0;

        }

        public void BatchDownloadFinancialReport()
        {
            DbMongo mongo = new DbMongo();
            mongo.connect();

            String[] stocks = new string[] { "2379" };
            foreach (String stock in stocks)
            {
                YearSeason yearseason = mongo.FinancialReport_FindLatest(stock);
                yearseason = (yearseason == null) ? new YearSeason(104, 1) : yearseason.next();
                for (int year = 104; year <= 108; year++)
                    for (int season = 1; season <= 4; season++)
                    {
                        if (SeasonalReportReady(year, season))
                        {
                            int try_count = 1;
                            Boolean download_success = false;
                            do
                            {
                                download_success = DownloadSeasonalReport(stock, year, season);
                                Thread.Sleep(1000 * try_count);
                            } while (!download_success && (try_count < 10));
                        }
                    }
            }
        }

        public Boolean DownloadSeasonalReport(String stock_index, int year, int season)
        {
            Boolean result = false;
            PageFinancial page = new PageFinancial();

            String path = TargetDirectory + page.getFileName(stock_index, year, season);
            if (File.Exists(path) == false)
            {
                Logger.Log("Download Financial Report:" + stock_index +" "+ year.ToString() +"/"+ season.ToString() );
                String content = page.download_page(page.getPageUrl(stock_index, year, season), stock_index, year, season);
                using (StreamWriter sw = new StreamWriter(path, false, Encoding.GetEncoding("utf-8")))
                {
                    sw.Write(content);
                    sw.Close();
                }
            }

            using (StreamReader sr = new StreamReader(path, Encoding.GetEncoding("utf-8")))
            {
                Debug.Write("Parse Financial Report:" + stock_index + "," + year.ToString() + "/" + season.ToString());
                Logger.Log("Parse Financial Report:" + stock_index + "," + year.ToString() + "/" + season.ToString());
                BsonDocument financial = page.ParseHtml(sr.ReadToEnd());
                if (financial.ElementCount != 0)
                {
                    result = true;
                    page.DBSave(financial, stock_index, year, season);
                }
            }
            if (result == false)
            {
                File.Delete(path);
            }
            return result;
        }

        public void DownloadEquityinfo()
        {
            DbSql.SetupEquityInfo();
            String[] pages = new String[] { "2", "4" };
            foreach (String pageidx in pages)
            {
                PageEquityList page = new PageEquityList(pageidx);

                String content = page.download_page(page.getPageUrl(DateTime.Now), Encoding.GetEncoding("big5"));
                ArrayList stock_data = page.ParseHtml(content, DateTime.Now);
                if (stock_data.Count > 0)
                {
                    page.DBSave(stock_data);
                }
            }
        }
    }
}
