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

        private bool BankModelExists(int id)
        {
            return _context.BankModel.Any(e => e.bank_id == id);
        }
    }
}
