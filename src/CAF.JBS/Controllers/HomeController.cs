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
            cmd.CommandText = @"SELECT td.* FROM `trancode_dashboard` td order by td.`TranCode`;";
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
                            PaidCount = rd["count_paid"] == DBNull.Value ? 0 : Convert.ToInt32(rd["count_paid"]) ,
                            PaidAmount = rd["amount_paid"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["amount_paid"]) ,
                            UnPaidCount = rd["count_unpaid"] == DBNull.Value ? 0 : Convert.ToInt32(rd["count_unpaid"]) ,
                            UnPaidAmount = rd["amount_unpaid"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["amount_unpaid"]) ,
                            CancelCount = rd["count_cancel"] == DBNull.Value ? 0 : Convert.ToInt32(rd["count_cancel"]),
                            CancelAmount = rd["amount_cancel"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["amount_cancel"]),
                            //TotalCount = rd["count_total"] == DBNull.Value ? 0 : Convert.ToInt32(rd["count_total"]) ,
                            //TotalAmount = rd["amount_total"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["amount_total"]) ,
                            DateUpdate = rd["DateUpdate"] == DBNull.Value ? (rd["DateCrt"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(rd["DateCrt"]) ) : Convert.ToDateTime(rd["DateUpdate"]) 
                        };
                        bil.TotalCount = bil.PaidCount+bil.UnPaidCount+bil.CancelCount;
                        bil.TotalAmount = bil.PaidAmount+bil.UnPaidAmount+bil.CancelAmount;
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
