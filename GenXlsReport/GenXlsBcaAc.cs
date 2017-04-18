using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using MySql.Data.MySqlClient;
using Microsoft.Office.Interop.Excel;


namespace GenXlsReport
{
    class GenXlsBcaAc
    {
        static string constring = ConfigurationSettings.AppSettings["DefaultDB"];

        static void Main(string[] args)
        {
            MySqlConnection con = new MySqlConnection(constring);
            con.Open(); // connection must be openned for command
            MySqlCommand cmd = new MySqlCommand(@"BillingBcaAC_sp", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            MySqlDataReader reader = cmd.ExecuteReader();
        }
    }
}
