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
using System.Data;
using MySql.Data.MySqlClient;

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
                     join bk in _jbsDB.BankModel on cd.bank_id equals bk.bank_id into bx
                     from bankx in bx.DefaultIfEmpty()
                     join ct in _jbsDB.cctypeModel on cd.Type equals ct.Id into cx
                     from cardx in cx.DefaultIfEmpty()
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
            bankList = _jbsDB.BankModel.Select(x => new SelectListItem { Value = x.bank_id.ToString(), Text = x.bank_code }).Where(r => r.Value != "0");
            JenisKartuList = _jbsDB.cctypeModel.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.TypeCard });
            cards.banks = bankList;
            cards.CCtypes = JenisKartuList;
            return View(cards);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(prefixcardModel card)
        {
            var cmdx = _jbsDB.Database;
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            if (ModelState.IsValid)
            {
                try
                {
                    cmdx.OpenConnection(); cmdx.BeginTransaction();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "insert_bin_number";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new MySqlParameter("@prefix", MySqlDbType.Int32) { Value = card.Prefix });
                    cmd.Parameters.Add(new MySqlParameter("@tipe", MySqlDbType.Int32) { Value = card.Type });
                    cmd.Parameters.Add(new MySqlParameter("@bank_idx", MySqlDbType.Int32) { Value = card.bank_id });
                    cmd.Parameters.Add(new MySqlParameter("@desk", MySqlDbType.VarChar) { Value = card.Description });

                    await cmd.ExecuteNonQueryAsync();
                    cmdx.CommitTransaction();
                }
                catch (Exception ex)
                {
                    cmdx.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
                finally
                {
                    if (cmdx.CurrentTransaction != null) cmdx.RollbackTransaction();
                    cmdx.CloseConnection();
                }

                //_jbsDB.prefixcardModel.Add(card);
                //_jbsDB.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(card);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cardIssuerBankModel = await _jbsDB.prefixcardModel.SingleOrDefaultAsync(m => m.Prefix == id);
            if (cardIssuerBankModel == null)
            {
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
            if (id != prefixcardModel.Prefix) return NotFound();
            if (ModelState.IsValid)
            {
                var cmdx = _jbsDB.Database;
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                try
                {
                    cmdx.OpenConnection(); cmdx.BeginTransaction();

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "update_bin_number";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new MySqlParameter("@bin_number", MySqlDbType.Int32) { Value = prefixcardModel.Prefix });
                    cmd.Parameters.Add(new MySqlParameter("@bank_idx", MySqlDbType.Int32) { Value = prefixcardModel.bank_id });
                    cmd.Parameters.Add(new MySqlParameter("@note", MySqlDbType.VarChar) { Value = prefixcardModel.Description });
                    cmd.Parameters.Add(new MySqlParameter("@tipe_card", MySqlDbType.Int32) { Value = prefixcardModel.Type });

                    await cmd.ExecuteNonQueryAsync();
                    cmdx.CommitTransaction();
                }
                catch (Exception ex)
                {
                    cmdx.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
                finally
                {
                    if (cmdx.CurrentTransaction != null) cmdx.RollbackTransaction();
                    cmdx.CloseConnection();
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
            var cmdx = _jbsDB.Database;
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            try
            {
                cmdx.OpenConnection(); cmdx.BeginTransaction();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "delete_bin_number";
                cmd.Parameters.Clear();
                cmd.Parameters.Add(new MySqlParameter("@prefix", MySqlDbType.Int32) { Value = id });

                await cmd.ExecuteNonQueryAsync();
                cmdx.CommitTransaction();
            }
            catch (Exception ex)
            {
                cmdx.RollbackTransaction();
                throw new Exception(ex.Message);
            }
            finally
            {
                if (cmdx.CurrentTransaction != null) cmdx.RollbackTransaction();
                cmdx.CloseConnection();
            }

            return RedirectToAction("Index");
        }

        private bool prefixcardModelExists(int id)
        {
            return _jbsDB.prefixcardModel.Any(e => e.Prefix == id);
        }


    }
}