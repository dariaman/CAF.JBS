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
        private readonly ApplicationDbContext _context;

        public CardController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Card
        public async Task<IActionResult> Index()
        {
            return View(await _context.CardIssuerBankModel.ToListAsync());
        }

        // GET: Card/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cardIssuerBankModel = await _context.CardIssuerBankModel
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
                _context.Add(cardIssuerBankModel);
                await _context.SaveChangesAsync();
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

            var cardIssuerBankModel = await _context.CardIssuerBankModel.SingleOrDefaultAsync(m => m.card_issuer_bank_id == id);
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

        // GET: Card/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cardIssuerBankModel = await _context.CardIssuerBankModel
                .SingleOrDefaultAsync(m => m.card_issuer_bank_id == id);
            if (cardIssuerBankModel == null)
            {
                return NotFound();
            }

            return View(cardIssuerBankModel);
        }

        // POST: Card/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cardIssuerBankModel = await _context.CardIssuerBankModel.SingleOrDefaultAsync(m => m.card_issuer_bank_id == id);
            _context.CardIssuerBankModel.Remove(cardIssuerBankModel);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool CardIssuerBankModelExists(int id)
        {
            return _context.CardIssuerBankModel.Any(e => e.card_issuer_bank_id == id);
        }
    }
}
