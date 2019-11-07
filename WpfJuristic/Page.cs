using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Collections;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace WpfJuristic
{
    abstract public class Page
    {
        String fileNameStart;

        public Page(String fn)
        {
            fileNameStart = fn;
        }

        public String getFilePrefix()
        {
            return fileNameStart;
        }

        abstract public String getPageUrl(DateTime date);
        abstract public ArrayList ParseHtml( string source, DateTime date);
        abstract public void DBSave( ArrayList stock_data);

        virtual public String getFileName(DateTime date)
        {
            String path = string.Format(@"\{0}{1:D4}_{2:D2}_{3:D2}.html", fileNameStart, date.Year, date.Month, date.Day);
            return path;
        }

        virtual public string download_page(string url, Encoding encoding)
        {
            string strResult = string.Empty;

            WebResponse objResponse;
            WebRequest objRequest = System.Net.HttpWebRequest.Create(url);

            try
            {
                objResponse = objRequest.GetResponse();

                using (StreamReader sr = new StreamReader(objResponse.GetResponseStream(), encoding/*Encoding.GetEncoding("utf-8")*/)) //, Encoding.Unicode
                {
                    strResult = sr.ReadToEnd();
                    // Close and clean up the StreamReader
                    sr.Close();
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
            }
            return strResult;
        }

        public static bool IsValidJson(string value)
        {
            try
            {
                var json = JContainer.Parse(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
