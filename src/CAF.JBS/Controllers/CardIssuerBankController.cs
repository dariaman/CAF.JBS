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
            //var customerlist = (from Cust in orderdb.Customers
            //                join Ord in orderdb.Orders on Cust.CustomerID equals Ord.CustomerID
            //                select new { Cust.Name, Cust.Mobileno, Cust.Address, Ord.OrderDate, Ord.OrderPrice}).ToList();

            ///*CardIssuerBankViewModel cards = new CardIssuerBankViewModel();

            //cards = from ci in _context.CardIssuerBankModel
            //         join b in _context.BankModel on ci.card_issuer_bank_id equals b.bank_id into bx from b in bx.DefaultIfEmpty()
            //         join ct in _context.cctypeModel on ci.Type equals ct.Id
            //         select new { ci.Prefix, ct.TypeCard, b.bank_code,ci.Description};

            //var InnerJoinOutput =
            //         from emp in Employees 
            //         join dep in Departments on emp.DepartmentId equals dep.Id
            //         join proj in Projects on emp.ProjectId equals proj.Id
            //         select new
            //         {
            //             Emp_Name = emp.Name,
            //             Dep_Name = dep.Name,
            //             Proj_Name = proj.Name
            //         };

            //var LeftJoinOutput =
            //    from emp in Employees
            //    join dep in Departments on emp.DepartmentId equals dep.Id into temp from j in temp.DefaultIfEmpty()
            //    join proj in Projects on emp.ProjectId equals proj.Id into temp1 from j1 in temp1.DefaultIfEmpty()
            //    select new
            //    {
            //        Emp_Name = emp.Name,
            //        Dep_Name = j == null ? "No Dep" : j.Name,
            //        Proj_Name = j1 == null ? "No Proj" : j1.Name
            //    };

            IEnumerable<CardIssuerBankViewModel> cards;            
            cards = (from cd in _context.CardIssuerBankModel
                        join bk in _context.BankModel on cd.acquirer_bank_id equals bk.bank_id into bx
                        from bankx in bx.DefaultIfEmpty()
                        join ct in _context.cctypeModel on cd.Type equals ct.Id.ToString() into cx
                        from cardx in cx.DefaultIfEmpty()
                        select new CardIssuerBankViewModel()
                        {
                            Prefix = cd.Prefix,
                            //TypeCard = cardx.TypeCard,
                            //card_issuer_bank_id = cd.card_issuer_bank_id,
                            BankName = bankx.bank_code,
                            Description = cd.Description
                        });
            
            return View(cards);
        }

        //[HttpGet]
        //public ActionResult GridPartial()
        //{
        //    return PartialView("_IndexGrid", _context.CardIssuerBankModel.ToList());
        //}

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.banks = _context.BankModel.ToList();
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