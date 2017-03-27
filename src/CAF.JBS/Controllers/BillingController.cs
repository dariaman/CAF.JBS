using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CAF.JBS.Data;
using CAF.JBS.Models;
using CAF.JBS.ViewModels;

namespace CAF.JBS.Controllers
{
    public class BillingController : Controller
    {
        private readonly JbsDbContext _jbsDB;
        public BillingController(JbsDbContext context1)
        {
            _jbsDB = context1;
        }

        [HttpGet]
        public ActionResult Index()
        {
            //IEnumerable<DownloadBillingVM> bill;
            //bill = (from b in _jbsDB.BillingModel);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("card_issuer_bank_id,Type,Prefix,Description,acquirer_bank_id")] BillingModel BillingModels)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }            
            return View();
        }
    }
}