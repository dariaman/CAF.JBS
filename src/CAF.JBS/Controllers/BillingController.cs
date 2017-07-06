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
                        //policy_id=cd.policy_id,
                        PolicyNo=bk.policy_no,
                        recurring_seq=cd.recurring_seq,
                        BillingDate=cd.BillingDate,
                        due_dt_pre = cd.due_dt_pre,
                        PeriodeBilling=cd.PeriodeBilling,
                        IsPending=cd.IsPending,
                        policy_regular_premium=cd.TotalAmount,
                        status_billing=cd.status_billing,
                        IsDownload=cd.IsDownload,
                        BankIdDownload=cd.BankIdDownload,
                        ReceiptID=cd.ReceiptID
                     });
            return View(BillingView);
        }

        public IActionResult SyncData()
        {
            //var billingModel = await 0;
            foreach (Process proc in Process.GetProcessesByName("JBSGenExcel")) { proc.Kill(); }

            try
            {
                var process = new Process();
                process.StartInfo.FileName = ConsoleFile;
                process.StartInfo.Arguments = @" sync /c";

                process.EnableRaisingEvents = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();
                process.WaitForExit();

            }
            catch (Exception ex) { throw ex; }

            try
            {
                foreach (Process proc in Process.GetProcessesByName("JBSGenExcel")) { proc.Kill(); }
            }
            catch (Exception ex) { throw ex; }
            return RedirectToAction("Index");
        }

        //// GET: Billing/Details/5
        //public async Task<IActionResult> Details(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var billingModel = await _context.BillingModel
        //        .SingleOrDefaultAsync(m => m.BillingID == id);
        //    if (billingModel == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(billingModel);
        //}

        //// GET: Billing/Edit/5
        //public async Task<IActionResult> Edit(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var billingModel = await _context.BillingModel.SingleOrDefaultAsync(m => m.BillingID == id);
        //    if (billingModel == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(billingModel);
        //}

        //// POST: Billing/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(string id, [Bind("BillingID,policy_id,recurring_seq,BillingDate,due_date_pre,PeriodeBilling,BillingType,policy_regular_premium,DISC_REGULAR_PREMIUM,DISC_REGULAR_PREMIUM_PCT_Amount,TotalAmount,statusBilling,IsDownload,DownloadDate,ReceiptID,PaymentTransactionID,UserCrt,DateCrt,UserUpdate,DateUpdate")] BillingModel billingModel)
        //{
        //    if (id != billingModel.BillingID)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(billingModel);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!BillingModelExists(billingModel.BillingID))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction("Index");
        //    }
        //    return View(billingModel);
        //}

        //private bool BillingModelExists(string id)
        //{
        //    return _context.BillingModel.Any(e => e.BillingID == id);
        //}
    }
}
