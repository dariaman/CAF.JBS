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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CAF.JBS.Controllers
{    
    public class PrefixcardController : Controller
    {
        private readonly JbsDbContext _jbsDB;

        public PrefixcardController(JbsDbContext context1)
        {
            _jbsDB = context1;
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
            PrefixcardViewModel cards = new PrefixcardViewModel();
            IEnumerable<SelectListItem> bankList;
            IEnumerable<SelectListItem> JenisKartuList;
            bankList = _jbsDB.BankModel.Select(x=> new SelectListItem { Value = x.bank_id.ToString(),Text=x.bank_code}).Where(r=> r.Value != "0");
            JenisKartuList = _jbsDB.cctypeModel.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.TypeCard });
            cards.banks = bankList;
            cards.CCtypes = JenisKartuList;
            return View(cards);
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cardIssuerBankModel = await _jbsDB.prefixcardModel.SingleOrDefaultAsync(m => m.Prefix == id);
            if (cardIssuerBankModel == null) {
                return NotFound();
            }

            PrefixcardViewModel cards = new PrefixcardViewModel();
            IEnumerable<SelectListItem> bankList;
            IEnumerable<SelectListItem> JenisKartuList;
            bankList = _jbsDB.BankModel.Select(x => new SelectListItem { Value = x.bank_id.ToString(), Text = x.bank_code }).Where(r => r.Value != "0");
            JenisKartuList = _jbsDB.cctypeModel.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.TypeCard });
            cards.banks = bankList;
            cards.CCtypes = JenisKartuList;

            cards.Prefix = cardIssuerBankModel.Prefix;
            cards.PrefixCopy = cardIssuerBankModel.Prefix;
            cards.Type = cardIssuerBankModel.Type;
            cards.bank_id = cardIssuerBankModel.bank_id;
            cards.Description = cardIssuerBankModel.Description;

            return View(cards);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Prefix,bank_id,Description,Type")] prefixcardModel prefixcardModel)
        {
            if (id != prefixcardModel.Prefix)
            {
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

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prefixcardModel = await _jbsDB.prefixcardModel
                .SingleOrDefaultAsync(m => m.Prefix == id);
            if (prefixcardModel == null)
            {
                return NotFound();
            }

            return View(prefixcardModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prefixcardModel = await _jbsDB.prefixcardModel.SingleOrDefaultAsync(m => m.Prefix == id);
            _jbsDB.prefixcardModel.Remove(prefixcardModel);
            await _jbsDB.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool prefixcardModelExists(int id)
        {
            return _jbsDB.prefixcardModel.Any(e => e.Prefix == id);
        }


    }
}