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
    public class BillingHoldController : Controller
    {
        private readonly JbsDbContext _context;

        public BillingHoldController(JbsDbContext context)
        {
            _context = context;    
        }

        // GET: BillingHold
        public async Task<IActionResult> Index()
        {
            return View(await _context.BillingHoldModel.ToListAsync());
        }

        // GET: BillingHold/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("policy_No,ReleaseDate")] BillingHoldViewModel BillingHoldViewModel)
        {
            //var polisID = this.PolicyExists(billingHoldModel.policy_Id.ToString());
            //billingHoldModel.policy_Id = polisID;
            if (ModelState.IsValid)
            {
                var model = new BillingHoldModel
                {
                    policy_Id = this.FindPolicyID(BillingHoldViewModel.policy_No),
                    ReleaseDate = BillingHoldViewModel.ReleaseDate.AddDays(1)
                };
                _context.BillingHoldModel.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(BillingHoldViewModel);
        }

        // GET: BillingHold/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var HoldModel = await _context.BillingHoldModel.SingleOrDefaultAsync(m => m.policy_Id == id);
            BillingHoldViewModel HoldViewModel = new BillingHoldViewModel();
            HoldViewModel.OldpolicyID = id;
            HoldViewModel.policy_No= this.FindPolicyNo(HoldModel.policy_Id);

            if (HoldModel == null) { return NotFound(); }
            if (HoldViewModel.policy_No == null) { return NotFound(); }
            else { HoldViewModel.ReleaseDate = HoldModel.ReleaseDate.AddDays(-1); }

            return PartialView(HoldViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BillingHoldViewModel HoldViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    BillingHoldModel HoldModel = new BillingHoldModel();
                    HoldModel.policy_Id = HoldViewModel.OldpolicyID;
                    HoldModel.ReleaseDate = HoldViewModel.ReleaseDate.AddDays(1);
                    _context.Update(HoldModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillingHoldModelExists(HoldViewModel.OldpolicyID))
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
            return View(HoldViewModel);
        }


        // GET: aaaaaa/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var billingHoldModel = await _context.BillingHoldModel
                .SingleOrDefaultAsync(m => m.policy_Id == id);
            if (billingHoldModel == null)
            {
                return NotFound();
            }

            return View(billingHoldModel);
        }

        // POST: aaaaaa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var billingHoldModel = await _context.BillingHoldModel.SingleOrDefaultAsync(m => m.policy_Id == id);
            _context.BillingHoldModel.Remove(billingHoldModel);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool BillingHoldModelExists(int id)
        {
            return _context.BillingHoldModel.Any(e => e.policy_Id == id);
        }

        private int FindPolicyID(string policy_no)
        {
            int polisID;
            polisID = _context.PolicyBillingModel.Where(x => x.policy_no == policy_no)
                .Select(x => x.policy_Id).FirstOrDefault();
            return polisID;
        }

        private string FindPolicyNo(int policy_id)
        {
            string polisID;
            polisID = _context.PolicyBillingModel.Where(x => x.policy_Id == policy_id)
                .Select(x => x.policy_no).FirstOrDefault();
            return polisID;
        }
    }
}
