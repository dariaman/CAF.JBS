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

namespace CAF.JBS.Controllers
{
    public class PolicyBillingController : Controller
    {
        private readonly JbsDbContext _context;

        public PolicyBillingController(JbsDbContext context)
        {
            _context = context;    
        }

        // GET: PolicyBilling
        public IActionResult Index()
        {
            IEnumerable<PolicyBillingViewModel> PolicyBillingView;
            PolicyBillingView = (from cd in _context.PolicyBillingModel
                                 select new PolicyBillingViewModel()
                                 {
                                     policy_Id = cd.policy_Id,
                                     policy_no = cd.policy_no,
                                     payment_method = cd.payment_method,
                                     commence_dt = cd.commence_dt,
                                     due_dt = cd.due_dt,
                                     premium_mode = cd.premium_mode,
                                     //due_dt_pre = cd.due_dt_pre,
                                     //product_code = cd.product_code,
                                     //HolderName = cd.HolderName,
                                     //EmailHolder = cd.EmailHolder,
                                     //regular_premium = cd.regular_premium,
                                     Policy_status = cd.Policy_status,
                                     //cc_no = cd.cc_no,
                                     //cc_acquirer_bank_id = cd.cc_acquirer_bank_id,
                                     //cc_expiry = cd.cc_expiry,
                                     //cc_name = cd.cc_name,
                                     //cc_address = cd.cc_address,
                                     //cc_telephone = cd.cc_telephone,
                                     //acc_no = cd.acc_no,
                                     //acc_bank_id = cd.acc_bank_id,
                                     //acc_name = cd.acc_name,
                                     //acc_bank_branch = cd.acc_bank_branch,
                                     //VANo = cd.VANo,
                                     //VAName = cd.VAName,
                                     //last_recurring_seq = cd.last_recurring_seq,
                                     //last_payment_source = cd.last_payment_source,
                                     //last_receipt_id = cd.last_receipt_id,
                                     //last_receipt_date = cd.last_receipt_date,
                                     //last_acquirer_bank_id = cd.last_acquirer_bank_id,
                                     IsHoldBilling = cd.IsHoldBilling
                                 });
            return View(PolicyBillingView);
        }

        // GET: PolicyBilling/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var billingModel = await _context.BillingModel
                .SingleOrDefaultAsync(m => m.BillingID == id);
            if (billingModel == null)
            {
                return NotFound();
            }

            return View(billingModel);
        }

        // GET: PolicyBilling/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BillingID,policy_id,recurring_seq,BillingDate,due_date_pre,PeriodeBilling,BillingType,policy_regular_premium,DISC_REGULAR_PREMIUM,DISC_REGULAR_PREMIUM_PCT_Amount,TotalAmount,statusBilling,IsDownload,DownloadDate,ReceiptID,PaymentTransactionID,UserCrt,DateCrt,UserUpdate,DateUpdate")] BillingModel billingModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(billingModel);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(billingModel);
        }

        // GET: PolicyBilling/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var billingModel = await _context.BillingModel.SingleOrDefaultAsync(m => m.BillingID == id);
            if (billingModel == null)
            {
                return NotFound();
            }
            return View(billingModel);
        }

        // POST: PolicyBilling/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("BillingID,policy_id,recurring_seq,BillingDate,due_date_pre,PeriodeBilling,BillingType,policy_regular_premium,DISC_REGULAR_PREMIUM,DISC_REGULAR_PREMIUM_PCT_Amount,TotalAmount,statusBilling,IsDownload,DownloadDate,ReceiptID,PaymentTransactionID,UserCrt,DateCrt,UserUpdate,DateUpdate")] BillingModel billingModel)
        {
            if (id != billingModel.BillingID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(billingModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillingModelExists(billingModel.BillingID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(billingModel);
        }

        private bool BillingModelExists(string id)
        {
            return _context.BillingModel.Any(e => e.BillingID == id);
        }
    }
}
