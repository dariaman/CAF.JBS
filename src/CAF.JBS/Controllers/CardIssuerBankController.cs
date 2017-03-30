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
        private readonly JbsDbContext _jbsDB;
        private readonly Life21DbContext _Life21DB;

        public CardIssuerBankController(JbsDbContext context1, Life21DbContext contex2)
        {
            _jbsDB = context1;
            _Life21DB = contex2;
        }

        [HttpGet]
        public ActionResult Index()
        {
            IEnumerable<CardIssuerBankViewModel> cards;
            cards = (from cd in _jbsDB.CardIssuerBankModel
                     join bk in _jbsDB.BankModel on cd.acquirer_bank_id equals bk.bank_id into bx
                     from bankx in bx.DefaultIfEmpty()
                     join ct in _jbsDB.cctypeModel on cd.Type equals ct.Id.ToString() into cx
                     from cardx in cx.DefaultIfEmpty()
                     orderby cd.acquirer_bank_id
                     select new CardIssuerBankViewModel()
                     {
                         Prefix = cd.Prefix,
                         TypeCard = cardx == null ? string.Empty : cardx.TypeCard,
                         card_issuer_bank_id = cd.card_issuer_bank_id,
                         BankName = bankx == null ? string.Empty : bankx.bank_code,
                         Description = cd.Description
                     });
            return View(cards);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CardIssuerBankModel card)
        {
            if (ModelState.IsValid)
            {
                _jbsDB.CardIssuerBankModel.Add(card);
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

            var cardIssuerBankModel = await _Life21DB.CardIssuerBankModel.SingleOrDefaultAsync(m => m.card_issuer_bank_id == id);
            if (cardIssuerBankModel == null) {
                return NotFound();
            }
            return View(cardIssuerBankModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("card_issuer_bank_id,Type,Prefix,Description,acquirer_bank_id")] CardIssuerBankModel cardIssuerBankModel)
        {
            if (id != cardIssuerBankModel.card_issuer_bank_id) {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _Life21DB.Update(cardIssuerBankModel);
                    await _Life21DB.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CardIssuerBankModelExists(cardIssuerBankModel.card_issuer_bank_id))
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
            return View(cardIssuerBankModel);
        }

        private bool CardIssuerBankModelExists(int id)
        {
            return _Life21DB.CardIssuerBankModel.Any(e => e.card_issuer_bank_id == id);
        }


    }
}