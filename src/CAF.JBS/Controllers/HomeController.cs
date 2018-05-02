using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CAF.JBS.ViewModels;
using CAF.JBS.Data;
using Microsoft.EntityFrameworkCore;

namespace CAF.JBS.Controllers
{
    public class HomeController : Controller
    {
        private readonly JbsDbContext _jbsDB;
        public HomeController(JbsDbContext context1)
        {
            _jbsDB = context1;
        }
        public IActionResult Index()
        {
            var periode = DateTime.Now.ToString("yyyyMM");
            List<BillingSumMonthlyVM> bs = new List<BillingSumMonthlyVM>();
            BillingSumMonthlyVM bil;

            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = @"SELECT td.`DashName`,bs.*
                                FROM `trancode_dashboard` td
                                LEFT JOIN `billing_sum_monthly` bs ON td.`TranCode`=bs.`trancode` AND bs.`Periode`='" + periode + @"'
                                order by bs.`trancode` ";
            try
            {
                cmd.Connection.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        bil = new BillingSumMonthlyVM()
                        {
                            DashName = rd["DashName"].ToString(),
                            Periode = rd["Periode"].ToString(),
                            PaidCount = rd["PaidCount"] == DBNull.Value ? 0 : Convert.ToInt32(rd["PaidCount"]) ,
                            PaidAmount = rd["PaidAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["PaidAmount"]) ,
                            UnPaidCount = rd["UnPaidCount"] == DBNull.Value ? 0 : Convert.ToInt32(rd["UnPaidCount"]) ,
                            UnPaidAmount = rd["UnPaidAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["UnPaidAmount"]) ,
                            TotalCount = rd["TotalCount"] == DBNull.Value ? 0 : Convert.ToInt32(rd["TotalCount"]) ,
                            TotalAmount = rd["TotalAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["TotalAmount"]) ,
                            DateUpdate = rd["DateUpdate"] == DBNull.Value ? (rd["DateCrt"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(rd["DateCrt"]) ) : Convert.ToDateTime(rd["DateUpdate"]) 
                        };
                        bs.Add(bil);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cmd.Connection.Close();
            }
            return View(bs);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
