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
    public class cctypeController : Controller
    {
        private readonly JbsDbContext _context;

        public cctypeController(JbsDbContext context)
        {
            _context = context;    
        }

        // GET: cctype
        public async Task<IActionResult> Index()
        {
            return View(await _context.cctypeModel.ToListAsync());
        }

        // GET: cctype/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TypeCard")] cctypeModel cctypeModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cctypeModel);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(cctypeModel);
        }
        

        private bool cctypeModelExists(int id)
        {
            return _context.cctypeModel.Any(e => e.Id == id);
        }
    }
}
