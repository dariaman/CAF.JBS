using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CAF.JBS.ViewModels;
using CAF.JBS.Data;

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
            List<BillingSumMonthlyVM> bs ;
            bs = (from bl in _jbsDB.BillingSumMonthly
                  join td in _jbsDB.TrancodeDashboard on bl.TranCode equals td.TranCode
                  select new BillingSumMonthlyVM()
                  {
                      DashName = td.DashName,
                      Periode = periode,
                      PaidCount = bl.PaidCount,
                      PaidAmount = bl.PaidAmount,
                      UnPaidCount = bl.UnPaidCount,
                      UnPaidAmount = bl.UnPaidAmount,
                      TotalCount = bl.TotalCount,
                      TotalAmount = bl.TotalAmount,
                      DateUpdate = bl.DateUpdate == null ? bl.DateCrt : bl.DateUpdate
                  }).Where(z => z.Periode == periode).ToList();
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
