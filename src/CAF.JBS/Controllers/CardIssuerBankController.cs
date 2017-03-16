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
            //(from ct in _context.cctypeModel orderby ct.TypeCard
            //                 select new { ct.Id, ct.TypeCard}).ToList();            
            return View();
            //return PartialView("index", cards);
        }

        [HttpGet]
        public ActionResult Ajax()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GridPartial()
        {

            IEnumerable<CardIssuerBankViewModel> cards;
            cards = (from cd in _context.CardIssuerBankModel
                     join bk in _context.BankModel on cd.acquirer_bank_id equals bk.bank_id into bx
                     from bankx in bx.DefaultIfEmpty()
                     join ct in _context.cctypeModel on cd.Type equals ct.Id.ToString() into cx
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

            return PartialView("_IndexGrid", cards);
        }

        [HttpGet]
        public IActionResult Create()
        {
            TempData["cType"] = _context.cctypeModel.ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CardIssuerBankModel card)
        {
            if (ModelState.IsValid)
            {
                _context.CardIssuerBankModel.Add(card);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(card);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cardIssuerBankModel = await _context.CardIssuerBankModel.SingleOrDefaultAsync(m => m.card_issuer_bank_id == id);
            if (cardIssuerBankModel == null)
            {
                return NotFound();
            }
            return View(cardIssuerBankModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("card_issuer_bank_id,Type,Prefix,Description,acquirer_bank_id")] CardIssuerBankModel cardIssuerBankModel)
        {
            if (id != cardIssuerBankModel.card_issuer_bank_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cardIssuerBankModel);
                    await _context.SaveChangesAsync();
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
            return _context.CardIssuerBankModel.Any(e => e.card_issuer_bank_id == id);
        }


    }
}