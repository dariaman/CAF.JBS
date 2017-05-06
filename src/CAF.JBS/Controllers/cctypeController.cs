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

        // GET: cctype/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cctypeModel = await _context.cctypeModel
                .SingleOrDefaultAsync(m => m.Id == id);
            if (cctypeModel == null)
            {
                return NotFound();
            }

            return View(cctypeModel);
        }

        // GET: cctype/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: cctype/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: cctype/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cctypeModel = await _context.cctypeModel.SingleOrDefaultAsync(m => m.Id == id);
            if (cctypeModel == null)
            {
                return NotFound();
            }
            return View(cctypeModel);
        }

        // POST: cctype/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TypeCard")] cctypeModel cctypeModel)
        {
            if (id != cctypeModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cctypeModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!cctypeModelExists(cctypeModel.Id))
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
            return View(cctypeModel);
        }

        // GET: cctype/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cctypeModel = await _context.cctypeModel
                .SingleOrDefaultAsync(m => m.Id == id);
            if (cctypeModel == null)
            {
                return NotFound();
            }

            return View(cctypeModel);
        }

        // POST: cctype/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cctypeModel = await _context.cctypeModel.SingleOrDefaultAsync(m => m.Id == id);
            _context.cctypeModel.Remove(cctypeModel);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool cctypeModelExists(int id)
        {
            return _context.cctypeModel.Any(e => e.Id == id);
        }
    }
}
