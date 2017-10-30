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
    public class QuoteBillingController : Controller
    {
        private readonly JbsDbContext _context;

        public QuoteBillingController(JbsDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            IEnumerable<QuoteBillingVM> QuoteBilling;
            QuoteBilling = (from cd in _context.QuoteBillingModel
                            orderby cd.DateCrt descending
                            select new QuoteBillingVM()
                            {
                                quote_id = cd.quote_id,
                                ref_no = cd.ref_no,
                                CustomerName=cd.Holder_Name,
                                prospect_amount = cd.prospect_amount,
                                paper_print_fee = cd.paper_print_fee,
                                cashless_fee = cd.cashless_fee,
                                status = cd.status,
                                LastUploadDate = cd.LastUploadDate,
                                DateCrt = cd.DateCrt,
                                cancel_date=cd.cancel_date,
                                paid_dt = cd.paid_dt
                            });
            return View(QuoteBilling);
        }

    }
}
