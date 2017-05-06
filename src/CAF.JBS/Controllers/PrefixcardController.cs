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
    public class PrefixcardController : Controller
    {
        private readonly JbsDbContext _jbsDB;
        private readonly Life21DbContext _Life21DB;

        public PrefixcardController(JbsDbContext context1, Life21DbContext contex2)
        {
            _jbsDB = context1;
            _Life21DB = contex2;
        }

        [HttpGet]
        public ActionResult Index()
        {
            IEnumerable<PrefixcardViewModel> cards;
            cards = (from cd in _jbsDB.prefixcardModel
                     join bk in _jbsDB.BankModel on cd.bank_id equals bk.bank_id into bx from bankx in bx.DefaultIfEmpty()
                     join ct in _jbsDB.cctypeModel on cd.Type equals ct.Id into cx from cardx in cx.DefaultIfEmpty()
                     select new PrefixcardViewModel()
                     {
                         Prefix = cd.Prefix,
                         TypeCard = cardx == null ? string.Empty : cardx.TypeCard,
                         BankName = bankx == null ? string.Empty : bankx.bank_code,
                         Description = cd.Description
                     });
            return View(cards);
        }

        [HttpGet]
        public IActionResult Create()
        {
            //IEnumerable<PrefixcardViewModel> cards;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(prefixcardModel card)
        {
            if (ModelState.IsValid)
            {
                _jbsDB.prefixcardModel.Add(card);
                _jbsDB.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(card);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) {
                return NotFound();
            }

            var cardIssuerBankModel = await _jbsDB.prefixcardModel.SingleOrDefaultAsync(m => m.Prefix == id);
            if (cardIssuerBankModel == null) {
                return NotFound();
            }
            return View(cardIssuerBankModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("card_issuer_bank_id,Type,Prefix,Description,acquirer_bank_id")] prefixcardModel prefixcardModel)
        {
            if (id != prefixcardModel.Prefix) {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _jbsDB.Update(prefixcardModel);
                    await _jbsDB.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!prefixcardModelExists(prefixcardModel.Prefix))
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
            return View(prefixcardModel);
        }

        private bool prefixcardModelExists(int id)
        {
            return _jbsDB.prefixcardModel.Any(e => e.Prefix == id);
        }


    }
}