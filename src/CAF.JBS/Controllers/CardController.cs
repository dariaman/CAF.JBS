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
    public class CardController : Controller
    {
        private readonly JbsDbContext _jbsDB;
        private readonly Life21DbContext _Life21DB;

        public CardController(JbsDbContext context1, Life21DbContext contex2)
        {
            _jbsDB = context1;
            _Life21DB = contex2;
        }

        // GET: Card
        public async Task<IActionResult> Index()
        {
            return View(await _Life21DB.CardIssuerBankModel.ToListAsync());
        }

        // GET: Card/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cardIssuerBankModel = await _Life21DB.CardIssuerBankModel
                .SingleOrDefaultAsync(m => m.card_issuer_bank_id == id);
            if (cardIssuerBankModel == null)
            {
                return NotFound();
            }

            return View(cardIssuerBankModel);
        }

        // GET: Card/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Card/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("card_issuer_bank_id,Type,Prefix,Description,acquirer_bank_id")] CardIssuerBankModel cardIssuerBankModel)
        {
            if (ModelState.IsValid)
            {
                _Life21DB.Add(cardIssuerBankModel);
                await _Life21DB.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(cardIssuerBankModel);
        }

        // GET: Card/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cardIssuerBankModel = await _Life21DB.CardIssuerBankModel.SingleOrDefaultAsync(m => m.card_issuer_bank_id == id);
            if (cardIssuerBankModel == null)
            {
                return NotFound();
            }
            return View(cardIssuerBankModel);
        }

        // POST: Card/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Card/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null){
                return NotFound();
            }

            var cardIssuerBankModel = await _Life21DB.CardIssuerBankModel
                .SingleOrDefaultAsync(m => m.card_issuer_bank_id == id);
            if (cardIssuerBankModel == null){
                return NotFound();
            }

            return View(cardIssuerBankModel);
        }

        // POST: Card/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cardIssuerBankModel = await _Life21DB.CardIssuerBankModel.SingleOrDefaultAsync(m => m.card_issuer_bank_id == id);
            _Life21DB.CardIssuerBankModel.Remove(cardIssuerBankModel);
            await _Life21DB.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool CardIssuerBankModelExists(int id)
        {
            return _Life21DB.CardIssuerBankModel.Any(e => e.card_issuer_bank_id == id);
        }
    }
}
