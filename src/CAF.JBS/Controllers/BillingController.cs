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
    public class BillingController : Controller
    {

        private readonly string ConsoleFile;
        private FileSettings filesettings;

        private readonly JbsDbContext _context;

        public BillingController(JbsDbContext context)
        {
            _context = context;
            filesettings = new FileSettings();
            ConsoleFile = filesettings.GenFileXls;
        }

        // GET: Billing
        public IActionResult Index()
        {
            IEnumerable<BillingViewModel> BillingView;
            BillingView = (from cd in _context.BillingModel
                           join bk in _context.PolicyBillingModel on cd.policy_id equals bk.policy_Id
                    select new BillingViewModel()
                     {
                        BillingID=cd.BillingID,
                        policy_id = cd.policy_id,
                        PolicyNo =bk.policy_no,
                        recurring_seq=cd.recurring_seq,
                        BillingDate=cd.BillingDate,
                        due_dt_pre = cd.due_dt_pre,
                        PeriodeBilling=cd.PeriodeBilling,
                        PayMeth=bk.payment_method,
                        cancel_date=cd.cancel_date,
                        paid_date=cd.paid_date,
                        IsPending=cd.IsPending,
                        cashless_fee_amount=cd.cashless_fee_amount,
                        policy_regular_premium=cd.policy_regular_premium,
                        status_billing=cd.status_billing,
                        IsDownload=cd.IsDownload,
                        BankIdDownload=cd.BankIdDownload,
                        ReceiptID=cd.ReceiptID,
                        LastUploadDate=cd.LastUploadDate
                     });
            return View(BillingView);
        }

        public IActionResult SyncData()
        {
            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.Parameters.Clear();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = @"SycnDataBill";

            try
            {
                _context.Database.OpenConnection();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cmd.Dispose();
                _context.Database.CloseConnection();
            }
            return RedirectToAction("Index");
        }
    }
}
