using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAF.JBS.Data;
using CAF.JBS.Models;

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
        public async Task<IActionResult> Index()
        {
            return View(await _context.BillingModel.ToListAsync());
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
