using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAF.JBS.Data;
using CAF.JBS.Models;
using CAF.JBS.ViewModels;
using System.Diagnostics;
using System.Data;
using MySql.Data.MySqlClient;

namespace CAF.JBS.Controllers
{
    public class BillingOtherController : Controller
    {
        private readonly JbsDbContext _context;

        public BillingOtherController(JbsDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            IEnumerable<BillingOthersVM> BillingOthers;
            BillingOthers = (from cd in _context.BillingOtherModel
                           join bk in _context.PolicyBillingModel on cd.policy_id equals bk.policy_Id
                           orderby cd.DateCrt descending
                    select new BillingOthersVM()
                     {
                        BillingID=cd.BillingID,
                        PolicyNo=bk.policy_no,
                        BillingDate=cd.BillingDate,
                        BillingType=cd.BillingType,
                        TotalAmount=cd.TotalAmount,
                        status_billing=cd.status_billing,
                        IsDownload=cd.IsDownload,
                        BankIdDownload=cd.BankIdDownload,
                        DateCrt=cd.DateCrt,
                        LastUploadDate=cd.LastUploadDate,
                        cancel_date=cd.cancel_date,
                        paid_date=cd.paid_date
                     });
            return View(BillingOthers);
        }
        
    }
}
