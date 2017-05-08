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
    public class BillingController : Controller
    {
        private readonly JbsDbContext _context;

        public BillingController(JbsDbContext context)
        {
            _context = context;    
        }

        // GET: Billing
        public async Task<IActionResult> Index()
        {
            IEnumerable<BillingViewModel> BillingView;
            BillingView = (from cd in _context.BillingModel
                           join bk in _context.PolicyBillingModel on cd.policy_id equals bk.policy_Id
                    select new BillingViewModel()
                     {
                         BillingID=cd.BillingID,
                         policy_id=cd.policy_id,
                         PolicyNo=bk.policy_no,
                         recurring_seq=cd.recurring_seq,
                         BillingDate=cd.BillingDate,
                         due_date_pre=cd.due_date_pre,
                         PeriodeBilling=cd.PeriodeBilling,
                         BillingType=cd.BillingType,
                         policy_regular_premium=cd.policy_regular_premium,
                         TotalAmount=cd.TotalAmount,
                         status_billing=cd.status_billing,
                         IsDownload=cd.IsDownload,
                         BankIdDownload=cd.BankIdDownload,
                         ReceiptID=cd.ReceiptID
                     });
            return View(BillingView);
        }

        // GET: Billing/Details/5
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

        // GET: Billing/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Billing/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Billing/Edit/5
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

        // POST: Billing/Edit/5
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
