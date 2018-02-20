using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CAF.JBS.Data;
using CAF.JBS.Models;
using CAF.JBS.ViewModels;
using System.Collections.Generic;
using System;

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
        public IActionResult Index()
        {
            IEnumerable<BillingHoldViewModel> BillingHoldView;
            BillingHoldView = (from cd in _context.BillingHoldModel
                               join bk in _context.PolicyBillingModel on cd.policy_Id equals bk.policy_Id
                               select new BillingHoldViewModel()
                           {
                               policy_Id=cd.policy_Id,
                               policy_No=bk.policy_no,
                               ReleaseDate=cd.ReleaseDate,
                               Description=cd.Description
                           });
            return View(BillingHoldView);
        }

        // GET: BillingHold/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("policy_No,ReleaseDate,Description")] BillingHoldViewModel HoldViewModel)
        {
            var polisID = this.FindPolicyID(HoldViewModel.policy_No);
            var tgl = DateTime.Now.Date;
            if (HoldViewModel.ReleaseDate < tgl)
            {
                ModelState.AddModelError("ReleaseDate", " HoldDate harus minimal tgl sekarang ");
            }
            if (polisID == 0)
            {
                ModelState.AddModelError("policy_No", "PolisNo Tidak Valid");
            }

            if (ModelState.IsValid)
            {
                var model = new BillingHoldModel
                {
                    policy_Id = Convert.ToInt32(FindPolicyID(HoldViewModel.policy_No)),
                    ReleaseDate = HoldViewModel.ReleaseDate.AddDays(1),
                    Description = HoldViewModel.Description,
                    UserCrt= User.Identity.Name
                };
                _context.BillingHoldModel.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(HoldViewModel);
        }

        // GET: BillingHold/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var HoldModel = await _context.BillingHoldModel.SingleOrDefaultAsync(m => m.policy_Id == id);
            BillingHoldViewModel HoldViewModel = new BillingHoldViewModel();
            HoldViewModel.policy_Id = id;
            HoldViewModel.policy_No= this.FindPolicyNo(HoldModel.policy_Id);

            if (HoldModel == null) { return NotFound(); }
            if (HoldViewModel.policy_No == null) { return NotFound(); }
            else {
                HoldViewModel.ReleaseDate = HoldModel.ReleaseDate.AddDays(-1);
                HoldViewModel.Description = HoldModel.Description;
            }

            return PartialView(HoldViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("policy_Id,policy_No,ReleaseDate,Description")] BillingHoldViewModel HoldViewModel)
        {
            var tgl = DateTime.Now.Date;
            if (HoldViewModel.ReleaseDate < tgl)
                ModelState.AddModelError("ReleaseDate", " HoldDate harus minimal tgl sekarang ");

            if (ModelState.IsValid)
            {
                try
                {
                    //BillingHoldModel HoldModel = new BillingHoldModel();
                    //HoldModel.policy_Id = id;
                    var HoldModel = _context.BillingHoldModel.SingleOrDefault(m => m.policy_Id == id);
                    HoldModel.ReleaseDate = HoldViewModel.ReleaseDate.AddDays(1);
                    HoldModel.Description = HoldViewModel.Description;
                    HoldModel.UserUpdate = User.Identity.Name;
                    HoldModel.DateUpdate = DateTime.Now;
                    _context.Update(HoldModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillingHoldModelExists(HoldViewModel.policy_Id))
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
