using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CAF.JBS.Data;
using CAF.JBS.Models;
using CAF.JBS.ViewModels;

namespace CAF.JBS.Controllers
{    
    public class CardIssuerBankController : Controller
    {
        private ApplicationDbContext _context;

        public CardIssuerBankController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GridPartial()
        {
            return PartialView("_IndexGrid", _context.CardIssuerBankModel.ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            //List<bankModel> BankList = new List<bankModel>();
            //BankList = (from bm in _context.BankModel select bm).ToList();
            //BankList.Insert(0, new bankModel { bank_id = 0, bank_code = "select Bank" });
            //ViewBag.Bank = BankList;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CardIssuerBankModel author)
        {
            if (ModelState.IsValid)
            {
                _context.CardIssuerBankModel.Add(author);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(author);
        }

        // GET: CardIssuerBank/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CardIssuerBank/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: CardIssuerBank/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }
    }
}