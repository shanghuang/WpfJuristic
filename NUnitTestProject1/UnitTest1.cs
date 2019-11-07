using NUnit.Framework;

using WpfJuristic;
using System;
using System.IO;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;
using MongoDB.Bson;
using System.Collections;

namespace NUnitTestProject1
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            DbSql.DBConnect();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [Test]
        public void TestParsePageFinancial()
        {
            String srcDir = @"C:\htmldwl\";

            PageFinancial page = new PageFinancial();

            String path = srcDir + page.getFileName("2379", 108, 1);
            if (File.Exists(path) == false)
            {
                String content = page.download_page(page.getPageUrl("2379", 108, 1), "2379", 108, 1);
                using (StreamWriter sw = new StreamWriter(path, false, Encoding.GetEncoding("big5")))
                {
                    sw.Write(content);
                    sw.Close();
                }
            }

            using (StreamReader sr = new StreamReader(path, Encoding.GetEncoding("big5")))
            {
                BsonDocument res = page.ParseHtml(sr.ReadToEnd());
            }

            Assert.Pass();
        }


        [Test]
        public void Test1()
        {
            Downloader dm = new Downloader();
            DateTime date = new DateTime(2019, 1, 3);
            //dm.Download_PerDay(date);
            Assert.Pass();
        }

        [Test]
        public void TestParseStockHtml()
        {
            PageStock page = new PageStock();


            DateTime date = new DateTime(2019, 1, 3);
            String path = page.getFileName(date);

            String srcDir = @"C:\Temp\htmldwl";
            if (File.Exists(srcDir + path) == false)
            {
                //Logger.e("File " + srcDir + path + " not found!!");
                //continue;
            }

            using (StreamReader sr = new StreamReader(srcDir + path, Encoding.GetEncoding("utf-8")))
            {
                page.ParseHtml(sr.ReadToEnd(), date);
            }

            Assert.Pass();
        }

        [Test]
        public void TestParseForeignHtml()
        {
            PageForeign page = new PageForeign();


            DateTime date = new DateTime(2019, 1, 3);
            String path = page.getFileName(date);

            String srcDir = @"C:\Temp\htmldwl";
            if (File.Exists(srcDir + path) == false)
            {
                //Logger.e("File " + srcDir + path + " not found!!");
                //continue;
            }

            using (StreamReader sr = new StreamReader(srcDir + path, Encoding.GetEncoding("utf-8")))
            {
                page.ParseHtml(sr.ReadToEnd(), date);
            }

            Assert.Pass();
        }

        [Test]
        public void TestParseFinancialReport()
        {
            String srcDir = @"C:\Temp\htmldwl\";

            PageFinancial page = new PageFinancial();

            String path = srcDir + page.getFileName("2379", 108, 1);
            if (File.Exists(path) == false)
            {
                String content = page.download_page(page.getPageUrl("2379", 108, 1), "2379", 108, 1);
                using (StreamWriter sw = new StreamWriter(path, false, Encoding.GetEncoding("utf-8")))
                {
                    sw.Write(content);
                    sw.Close();
                }
            }

            using (StreamReader sr = new StreamReader(path, Encoding.GetEncoding("utf-8")))
            {
                BsonDocument res = page.ParseHtml(sr.ReadToEnd());
            }

            Assert.Pass();
        }

        [Test]
        public void TestParseFinancial2()
        {
            String srcDir = @"C:\Temp\htmldwl\";

            PageFinancial page = new PageFinancial();

            String path = srcDir + page.getFileName("2379", 108, 1);
            if (File.Exists(path) == false)
            {
                String content = page.download_page(page.getPageUrl("2379", 108, 1), "2379", 108, 1);
                using (StreamWriter sw = new StreamWriter(path, false, Encoding.GetEncoding("utf-8")))
                {
                    sw.Write(content);
                    sw.Close();
                }
            }

            using (StreamReader sr = new StreamReader(path, Encoding.GetEncoding("utf-8")))
            {
                BsonDocument res = page.ParseHtml(sr.ReadToEnd());
            }

            Assert.Pass();
        }

        [Test]
        public void TestMongoConnection()
        {
            DbMongo mongo = new DbMongo();
            mongo.connect();

            PageFinancial page = new PageFinancial();

            String path = @"C:\Temp\htmldwl\" + page.getFileName("2379", 108, 1);
            using (StreamReader sr = new StreamReader(path, Encoding.GetEncoding("utf-8")))
            {
                BsonDocument financial = page.ParseHtml(sr.ReadToEnd());
                mongo.FinancialReport_save(financial, "2379", 108, 1);
            }

        }

        [Test]
        public void TestMongo_FinancialReport_GetLatest()
        {
            DbMongo mongo = new DbMongo();
            mongo.connect();
            mongo.FinancialReport_FindLatest("2371");
        }


        [Test]
        public void TestSQLSaveStock()
        {
            PageStock page = new PageStock();
            DateTime date = new DateTime(2019, 1, 3);
            String path = page.getFileName(date);

            using (StreamReader sr = new StreamReader(@"C:\Temp\htmldwl" + path, Encoding.GetEncoding("utf-8")))
            {
                ArrayList stock_data = page.ParseHtml(sr.ReadToEnd(), date);
                DbSql.StockData_Save(stock_data);
            }
        }

        [Test]
        public void TestSQLSaveForeign()
        {
            PageForeign page = new PageForeign();
            DateTime date = new DateTime(2019, 1, 3);
            String path = page.getFileName(date);

            using (StreamReader sr = new StreamReader(@"C:\Temp\htmldwl" + path, Encoding.GetEncoding("utf-8")))
            {
                ArrayList stock_data = page.ParseHtml(sr.ReadToEnd(), date);
                DbSql.StockData_SaveForeign(stock_data);
            }
        }

        [Test]
        public void TestGetLatestDownload()
        {
            DateTime latest = DbSql.getLastDBDate("2379");
        }

        [Test]
        public void TestSeasonalReportReady()
        {
            Downloader dm = new Downloader();

            Boolean res = dm.SeasonalReportReady(108, 3);
            Boolean res2 = dm.SeasonalReportReady(107, 4);
        }

        [Test]
        public void TestTradingDay()
        {
            Boolean d1 = TradingDays.IsTradingDay(new DateTime(2019, 10, 8));
            Boolean d2 = TradingDays.IsTradingDay(new DateTime(2019, 10, 10));
            Boolean d3 = TradingDays.IsTradingDay(new DateTime(2018, 12, 22));

            Boolean d4 = TradingDays.IsTradingDay(new DateTime(2015, 10, 8));

            /*TradingDays td = new TradingDays();
            Boolean d1 = td.IsTradingDay(new DateTime(2019, 10, 8));
            Boolean d2 = td.IsTradingDay(new DateTime(2019, 10, 10));*/

        }

        [Test]
        public void TestDbLogger()
        {
            Logger.SetupDb();
            Logger.Log("test logger!");
        }

        [Test]
        public void TestLoadCompany()
        {
            ArrayList companys = DbSql.LoadCompanyList();
            Assert.Pass();
        }

        [Test]
        public void TestEncoding()
        {
            String big5 = "ด๚ธี";
            byte[] big5_chars = Encoding.GetEncoding("big5").GetBytes(big5);
            byte[] utf8_chars = Encoding.Convert(Encoding.GetEncoding("big5"), Encoding.UTF8, big5_chars);

            String name_utf8 = Encoding.UTF8.GetString(utf8_chars);
            Assert.Pass();
        }
    }
}