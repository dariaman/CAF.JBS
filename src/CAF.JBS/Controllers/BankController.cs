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
    public class BankController : Controller
    {
        private readonly JbsDbContext _context;

        public BankController(JbsDbContext context)
        {
            _context = context;    
        }

        // GET: Bank
        public async Task<IActionResult> Index()
        {
            return View(await _context.BankModel.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("bank_id,bank_code")] BankModel BankModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(BankModel);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(BankModel);
        }

        private bool BankModelExists(int id)
        {
            return _context.BankModel.Any(e => e.bank_id == id);
        }
    }
}
