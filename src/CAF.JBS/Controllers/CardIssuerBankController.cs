using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CAF.JBS.Data;
using CAF.JBS.Models;

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
            //var banks = _context.cctypeModel.OrderBy(c => c.CountryName).Select(x => new { Id = x.Code, Value = x.Name });

            //var model = new HomeViewModel();
            return View(_context.CardIssuerBankModel.ToList());
        }

        [HttpGet]
        public ActionResult GridPartial()
        {
            return PartialView("_IndexGrid", _context.CardIssuerBankModel.ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            //List<cctypeModel>
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