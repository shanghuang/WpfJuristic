using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Collections;

/*
 * Fix MySql chinese charset problem:https://www.itread01.com/content/1547002469.html
 */

namespace WpfJuristic
{
    public static class DbSql
    {

        static MySql.Data.MySqlClient.MySqlConnection conn;


        public static void DBConnect()
        {
            string myConnectionString = "server=localhost;uid=shang;" +
                "pwd=king3697;database=twstock;";

            try
            {
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = myConnectionString;
                conn.Open();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                try
                {
                    //setupUser();
                    //SetupDatabase();
                    conn.Open();
                }
                catch (MySql.Data.MySqlClient.MySqlException ex_in)
                {
                }
            }

            /*StockPage sp = new StockPage();
            PageJuristic jp = new PageJuristic("foreign_");
            sp.SetupDatebase(conn);
            jp.SetupDatebase(conn);
            CompanyProfilePage cpp = new CompanyProfilePage();
            cpp.SetupDatebase(conn);
            cpp.SetupEarningDatebase(conn);

            UserManager userManager = new UserManager();
            userManager.SetupDatebase(conn);*/

        }

        public static Boolean isConnected()
        {
            return (conn != null);
        }

        static public String StockDbName(String stock_index)
        {
            return "s" + stock_index;
        }

        public static StockTradeInfo[] GetStockTradeHistory( String stock_index, int count, bool desc)
        {
            ArrayList res = new ArrayList();

            try
            {
                String qstr = "SELECT * from " + StockDbName(stock_index) + " ORDER by trans_date"+ (desc?"DESC":" ") + "LIMIT " + String.Format("{0}", count) + ";";
                MySqlCommand cmd = new MySqlCommand(qstr, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    res.Add(new StockTradeInfo(rdr));
                }

                rdr.Close();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                //MessageBox.Show("Error " + ex.Number + " has occurred: " + ex.Message,
                //    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {

            }
            return (StockTradeInfo[])res.ToArray(typeof(StockTradeInfo));
        }

        public static DateTime getLastDBDate( String stock_index)
        {
            DateTime res = new DateTime();        //first date
            //string stock_idx = "1101";  //"台泥"
            string stock_db_name = StockDbName(stock_index);

            try
            {
                String qstr = "SELECT * from " + stock_db_name + "  ORDER by trans_date DESC;";
                MySqlCommand cmd = new MySqlCommand(qstr, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    rdr.Read();
                    res = (DateTime)rdr["trans_date"];
                }
                rdr.Close();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                //MessageBox.Show("Error " + ex.Number + " has occurred: " + ex.Message,
                //    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {

            }
            return res;
        }



        public static void SetupDatebase(MySqlConnection conn)//, String stock_index)
        {
            String[] stocks = new string[] { "2370", "2371", "2372", "2373", "2374", "2375", "2376", "2377", "2378", "2379" };

            foreach (String stock_index in stocks)
            {
                try
                {
                    //string sql_create_db = "CREATE TABLE AsyncSampleTable (index int)";
                    string sql_create_db = "CREATE TABLE IF NOT EXISTS " + "S" + stock_index + " (" +
                                        "idx INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                                            "trans_date     DATE, " +
                                            "trans_volume   INT, " +
                                            "trans_count    INT, " +
                                            "trans_value    INT(64), " +
                                            "open           DECIMAL(6,2), " +
                                            "high           DECIMAL(6,2), " +
                                            "low            DECIMAL(6,2), " +
                                            "close          DECIMAL(6,2), " +
                                            "diff           DECIMAL(6,2), " +
                                            "close_buy_price    DECIMAL(6,2), " +
                                            "close_buy_volume   INT, " +
                                            "close_sell_price   DECIMAL(6,2), " +
                                            "close_sell_volume  INT, " +
                                            "foreign_buy    INT, " +
                                            "foreign_sell    INT, " +
                                            "foreign_total    INT " +
                                            ") ";

                    MySqlCommand cmd_create = new MySqlCommand(sql_create_db, conn);
                    cmd_create.ExecuteNonQuery();
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    //MessageBox.Show("Error " + ex.Number + " has occurred: " + ex.Message,
                    //    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public static void StockData_Save(ArrayList data)
        {
            foreach (Object obj in data)
            {
                StockTradeInfo stock_info = (StockTradeInfo)obj;
                String stock_db_name = stock_info.stock_index;

                if (stock_db_name.StartsWith("237") == false)   //debug
                    continue;

                string sql_ins = "INSERT INTO " + StockDbName(stock_db_name) + "( trans_date, trans_volume, trans_count, trans_value, open, high, low ," +
                                                                "close, diff, close_buy_price, close_buy_volume, close_sell_price, close_sell_volume) VALUE(" +
                                                                "'" + Util.convertDate2mysql(stock_info.trans_date) + "'" + "," + stock_info.trans_volume.ToString() + "," + stock_info.trans_count.ToString() + "," + stock_info.trans_value.ToString() + "," +
                                                               Util.double2dec(stock_info.open, 6, 2) + "," + Util.double2dec(stock_info.high, 6, 2) + "," + Util.double2dec(stock_info.low, 5, 2) + "," + Util.double2dec(stock_info.close, 5, 2) + "," +
                                                                Util.double2dec(stock_info.diff, 5, 2) + "," + Util.double2dec(stock_info.close_buy_price, 5, 2) + "," + stock_info.close_buy_volume.ToString() + "," +
                                                                Util.double2dec(stock_info.close_sell_price, 5, 2) + "," + stock_info.close_sell_volume.ToString() + ");";
                MySqlCommand cmd_insert = new MySqlCommand(sql_ins, conn);
                try
                {
                    cmd_insert.ExecuteNonQuery();
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    //MessageBox.Show("Error " + ex.Number + " has occurred: " + ex.Message,
                    //    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public static void StockData_SaveForeign(ArrayList data)
        {
            foreach (Object obj in data)
            {
                JuristicTradeInfo stock_info = (JuristicTradeInfo)obj;
                String stock_db_name = stock_info.stock_index;

                if (stock_db_name.StartsWith("237") == false)   //debug
                    continue;
                /*string sql_update = "UPDATE @stock_db_name " +
                                " SET foreign_buy=@buy_volume, foreign_sell=@sell_volume, foreign_total=@total_volume " +
                                " WHERE trans_date =@trans_date;";*/
                string sql_update = "UPDATE " + StockDbName(stock_db_name) +
                " SET foreign_buy=@buy_volume, foreign_sell=@sell_volume, foreign_total=@total_volume " +
                " WHERE trans_date =@trans_date;";

                MySqlCommand cmd_insert = new MySqlCommand(sql_update, conn);
                cmd_insert.Parameters.AddWithValue("@stock_db_name", StockDbName(stock_db_name));
                cmd_insert.Parameters.AddWithValue("@buy_volume", stock_info.buy_volume);
                cmd_insert.Parameters.AddWithValue("@sell_volume", stock_info.sell_volume);
                cmd_insert.Parameters.AddWithValue("@total_volume", stock_info.total_volume);
                cmd_insert.Parameters.AddWithValue("@trans_date", Util.convertDate2mysql(stock_info.trans_date));

                try
                {
                    cmd_insert.ExecuteNonQuery();
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    //MessageBox.Show("Error " + ex.Number + " has occurred: " + ex.Message,
                    //    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public static void SetupLoggerDb()
        {
            try
            {
                //string sql_create_db = "CREATE TABLE AsyncSampleTable (index int)";
                string sql_create_db = "CREATE TABLE IF NOT EXISTS " + "Log (" +
                                    "idx INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                                        "trans_date     DATE, " +
                                        "message   VARCHAR(256) " +
                                        ") ";

                MySqlCommand cmd_create = new MySqlCommand(sql_create_db, conn);
                cmd_create.ExecuteNonQuery();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                //MessageBox.Show("Error " + ex.Number + " has occurred: " + ex.Message,
                //    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void Log(String message)
        {
            string sql_ins = "INSERT INTO Log ( trans_date, message) VALUE(" + Util.convertDate2mysql(DateTime.Now) + ", \"" + message + "\")";

            MySqlCommand cmd_insert = new MySqlCommand(sql_ins, conn);
            try
            {
                cmd_insert.ExecuteNonQuery();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                //MessageBox.Show("Error " + ex.Number + " has occurred: " + ex.Message,
                //    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void SetupEquityInfo()
        {
            try
            {
                //string sql_create_db = "CREATE TABLE AsyncSampleTable (index int)";
                string sql_create_db = "CREATE TABLE IF NOT EXISTS `StockInfo` (" +
                                    "idx INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                                        "`name`   VARCHAR(32), " +
                                        "`index`  VARCHAR(16), " +
                                        "`ipo_date`  DATE " +
                                        ") ";

                MySqlCommand cmd_create = new MySqlCommand(sql_create_db, conn);
                cmd_create.ExecuteNonQuery();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                //MessageBox.Show("Error " + ex.Number + " has occurred: " + ex.Message,
                //    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void AddEquityInfo(PageEquityList.EquityInfo info)
        {
            string sql_ins = "INSERT INTO StockInfo ( `name`, `index`, `ipo_date`) VALUES (\"" +  info.name +"\",\"" +info.index +"\",\"" + info.ipo_date + "\")";

            MySqlCommand cmd_insert = new MySqlCommand(sql_ins, conn);

            try
            {
                cmd_insert.ExecuteNonQuery();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                //MessageBox.Show("Error " + ex.Number + " has occurred: " + ex.Message,
                //    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static ArrayList LoadCompanyList()
        {
            ArrayList res = new ArrayList();
            try
            {
                String qstr = "SELECT * from StockInfo";
                MySqlCommand cmd = new MySqlCommand(qstr, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    res.Add(new PageEquityList.EquityInfo(
                        (String) rdr["name"],
                        (String) rdr["index"],
                        ((DateTime) rdr["ipo_date"]).ToString()));
                }
                rdr.Close();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                //MessageBox.Show("Error " + ex.Number + " has occurred: " + ex.Message,
                //    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {

            }
            return res;
        }
    }
}
