using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Collections;

namespace WpfJuristic
{
    class DbSql
    {

        MySql.Data.MySqlClient.MySqlConnection conn;


        public void DBConnect()
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

        public Boolean isConnected()
        {
            return (conn != null);
        }

        static public String StockDbName(String stock_index)
        {
            return "s" + stock_index;
        }

        public StockTradeInfo[] GetStockTradeHistory( String stock_index, int count)
        {
            ArrayList res = new ArrayList();

            try
            {
                String qstr = "SELECT * from " + StockDbName(stock_index) + " ORDER by trans_date DESC LIMIT " + String.Format("{0}", count) + ";";
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

    }
}
