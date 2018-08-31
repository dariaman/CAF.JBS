using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CAF.JBS.Data;
using CAF.JBS.ViewModels;
using System.Data;
using DataTables.AspNet.Core;
using System.Text.RegularExpressions;
using DataTables.AspNet.AspNetCore;
using Vereyon.Web;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using CAF.JBS.Models;

namespace CAF.JBS.Controllers
{
    public class ReasonMapingGroupController : Controller
    {
        private readonly JbsDbContext _context;
        private IFlashMessage flashMessage;

        public ReasonMapingGroupController(JbsDbContext context, IFlashMessage flash)
        {
            _context = context;
            flashMessage = flash;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult PageData(IDataTablesRequest request)
        {
            int jlh = 0, jlhFilter = 0;
            string sort = "";
            var sqlFilter = GenerateFilter(request, ref sort);

            List<ReasonMapingGroupVM> RejectMapping = new List<ReasonMapingGroupVM>();
            RejectMapping = GetPageData(request.Start, request.Length, sort, sqlFilter, ref jlhFilter, ref jlh);
            var response = DataTablesResponse.Create(request, jlh, jlhFilter, RejectMapping);

            return new DataTablesJsonResult(response);
        }

        private string GenerateFilter(IDataTablesRequest request, ref string sort)
        {
            string FilterSql = "";
            DateTime tgl = DateTime.Now.Date;

            string paternAngka = @"[^0-9,%]";
            string paternAngkaHuruf = @"[^0-9a-zA-Z,%]";

            int i = 0;
            foreach (var req in request.Columns)
            {
                i++;
                if (req.Sort != null) sort = string.Format(" {0} {1} ", i, req.Sort.Direction.ToString().ToLower() == "ascending" ? "ASC" : "DESC");

                if (req.Search == null) continue;
                if (req.Search.Value == null) continue;

                if (req.Field == "id" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND rm.`id` like '" + tmp + "'";
                }
                else if (req.Field == "bank" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND rm.`bank_id` like '" + tmp + "'";
                }
                else if (req.Field == "rejectCode" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngkaHuruf, "");
                    FilterSql += " AND rm.`RejectCode` like '" + tmp + "'";
                }
                else if (req.Field == "rejectReason" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngkaHuruf, "");
                    FilterSql += " AND rm.`RejectReason` like '" + tmp + "'";
                }
                else if (req.Field == "groupReject" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngkaHuruf, "");
                    FilterSql += " AND gm.`GroupRejectReason` like '" + tmp + "'";
                }
                else if (req.Field == "note" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngkaHuruf, "");
                    FilterSql += " AND rm.`note` like '" + tmp + "'";
                }
            }

            return FilterSql;
        }

        private List<ReasonMapingGroupVM> GetPageData(int rowStart, int limitData, string orderString, string FilterWhere, ref int jlhdataFilter, ref int jlhData)
        {
            FilterWhere = string.Concat(" WHERE 1=1 ", FilterWhere);
            string order = (orderString == "" ? "" : string.Format(" ORDER BY {0} ", orderString));
            string limit = string.Format(" LIMIT {0},{1} ", rowStart, limitData);
            ReasonMapingGroupVM dt = new ReasonMapingGroupVM();
            List<ReasonMapingGroupVM> ls = new List<ReasonMapingGroupVM>();

            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = QueryPaging(GetDataSelect(), FilterWhere, order, limit);

            try
            {
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    ls.Add(new ReasonMapingGroupVM()
                    {
                        id = rd["id"].ToString(),
                        bank = rd["bank_code"].ToString(),
                        RejectCode = rd["RejectCode"].ToString(),
                        RejectReason = rd["RejectReason"].ToString(),
                        GroupReject = rd["GroupRejectReason"].ToString(),
                        note = rd["note"].ToString(),
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Connection.Close();
            }

            try
            {
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                cmd.CommandText = QueryPaging(" COUNT(1) ", FilterWhere, "", "");
                jlhdataFilter = Convert.ToInt32(cmd.ExecuteScalar().ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Connection.Close();
            }

            try
            {
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                cmd.CommandText = QueryPaging(" COUNT(1) ", "", "", "");
                jlhData = Convert.ToInt32(cmd.ExecuteScalar().ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return ls;
        }

        private string QueryPaging(string SelectData, string where, string order, string limit)
        {
            string sql = "";
            sql = @"SELECT " + SelectData + @"
                    FROM `reason_maping_group` rm
                    LEFT JOIN `bank` b ON b.`bank_id`=rm.`bank_id`
                    LEFT JOIN `GroupRejectMapping` gm ON gm.`id`=rm.`GroupRejectMappingID` " +
                    where + order + limit;

            return sql;
        }

        private string GetDataSelect()
        {
            string select = @"rm.`id`,
                              rm.`bank_id`,
                              b.`bank_code`,
                              rm.`RejectCode`,
                              rm.`RejectReason`,
                              rm.`GroupRejectMappingID`,
                              gm.`GroupRejectReason`,
                              rm.`note`";
            return select;
        }

        public async Task<IActionResult> Edit(int id)
        {
            ReasonMapingGroupEditVM rejectMap = new ReasonMapingGroupEditVM();
            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"SELECT rm.`id`,rm.`bank_id`,b.`bank_code`,rm.`RejectCode`,rm.`RejectReason`,rm.`GroupRejectMappingID`,gm.`GroupRejectReason`,rm.`note`
                                FROM `reason_maping_group` rm
                                LEFT JOIN `bank` b ON b.`bank_id`=rm.`bank_id`
                                LEFT JOIN `GroupRejectMapping` gm ON gm.`id`=rm.`id`
                                WHERE rm.`id`=@idx LIMIT 1 ; ";
            cmd.Parameters.Clear();
            cmd.Parameters.Add(new MySqlParameter("@idx", MySqlDbType.Int16) { Value = id });
            try
            {
                cmd.Connection.Open();
                var rd = await cmd.ExecuteReaderAsync();
                while (rd.Read())
                {
                    rejectMap.id = rd["id"].ToString();
                    rejectMap.bank_id = rd["bank_id"].ToString();
                    rejectMap.bank_name = rd["bank_code"].ToString();
                    rejectMap.RejectCode = rd["RejectCode"].ToString();
                    rejectMap.RejectReason = rd["RejectReason"].ToString();
                    rejectMap.GroupRejectMappingID = rd["GroupRejectMappingID"].ToString();
                    rejectMap.GroupReject_Description = rd["GroupRejectReason"].ToString();
                    rejectMap.note = rd["note"].ToString();
                }
            }
            catch (Exception ex) { flashMessage.Danger(ex.Message); }
            finally { cmd.Connection.Close(); }

            if (rejectMap == null) return NotFound();

            IEnumerable<SelectListItem> bankList;
            IEnumerable<SelectListItem> GroupRejectList;

            // List bank collector pendebetan CC
            List<int> bankIds = new List<int>();
            bankIds.Add(1);
            bankIds.Add(2);
            bankIds.Add(3);
            bankIds.Add(12);
            bankIds.Add(14);

            bankList = _context.BankModel.Where(b => bankIds.Contains(b.bank_id))
                .Select(x => new SelectListItem { Value = x.bank_id.ToString(), Text = x.bank_code }).Where(r => r.Value != "0");
            GroupRejectList = _context.GroupRejectMappingModel.Select(x => new SelectListItem { Value = x.id.ToString(), Text = x.GroupRejectReason });
            rejectMap.banks = bankList;
            rejectMap.GroupReject = GroupRejectList;

            return PartialView(rejectMap);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind("id,bank_id,RejectCode,RejectReason,GroupRejectMappingID,note")] ReasonMapingGroupEditVM ReasonMapingUpdate)
        {
            Boolean retval = false;
            string message = "";

            var reasonGroupModel = this.findRejectModel(id);

            if (reasonGroupModel == null)
            {
                retval = false;
                message = "Data tidak ditemukan";
                return Json(new { data = retval, message = message });
            }

            int bankid;

            if (!int.TryParse(ReasonMapingUpdate.bank_id ?? "0", out bankid))
            {
                retval = false;
                message = "Data tidak ditemukan";
                return Json(new { data = retval, message = message });
            }

            try
            {
                reasonGroupModel.bank_id = (bankid == 0 ? (int?)null : bankid);
                reasonGroupModel.RejectCode = ReasonMapingUpdate.RejectCode;
                reasonGroupModel.RejectReason = ReasonMapingUpdate.RejectReason;
                reasonGroupModel.GroupRejectMappingID = int.Parse(ReasonMapingUpdate.GroupRejectMappingID);
                reasonGroupModel.note = ReasonMapingUpdate.note;
                _context.Update(reasonGroupModel);
                _context.SaveChanges();
                retval = true;
                message = "sukses";
            }
            catch (Exception ex)
            {
                retval = false;
                message = ex.Message;
            }

            return Json(new { data = retval, message = message });
        }

        public async Task<IActionResult> Create()
        {
            ReasonMapingGroupEditVM rejectMap = new ReasonMapingGroupEditVM();

            IEnumerable<SelectListItem> bankList;
            IEnumerable<SelectListItem> GroupRejectList;

            // List bank collector pendebetan CC
            List<int> bankIds = new List<int>();
            bankIds.Add(1); // BCA
            bankIds.Add(2); // Mandiri
            bankIds.Add(3); // BNI
            bankIds.Add(12); // Mega
            bankIds.Add(14); // CIMB

            bankList = _context.BankModel.Where(b => bankIds.Contains(b.bank_id))
                .Select(x => new SelectListItem { Value = x.bank_id.ToString(), Text = x.bank_code }).Where(r => r.Value != "0");
            GroupRejectList = _context.GroupRejectMappingModel.Select(x => new SelectListItem { Value = x.id.ToString(), Text = x.GroupRejectReason });
            rejectMap.banks = bankList;
            rejectMap.GroupReject = GroupRejectList;

            return View(rejectMap);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReasonMapingGroupEditVM ReasonMapVM)
        {
            var ReasonMapModel = new ReasonMapingGroupModel();

            int bankid, groupid;

            if (int.TryParse(ReasonMapVM.GroupRejectMappingID, out groupid))
                ReasonMapModel.GroupRejectMappingID = groupid;
            else
                ModelState.AddModelError("GroupRejectMappingID", " Group Reject belum dipilih ");

            if (string.IsNullOrEmpty(ReasonMapVM.RejectReason))
                ModelState.AddModelError("RejectReason", " Reaject Reason harus diisi ");

            if (ModelState.IsValid)
            {

                if (int.TryParse(ReasonMapVM.bank_id, out bankid))
                    ReasonMapModel.bank_id = bankid;

                if (ReasonMapModel.bank_id == null) ReasonMapModel.RejectCode = null;
                else ReasonMapModel.RejectCode = ReasonMapVM.RejectCode;

                ReasonMapModel.RejectReason = ReasonMapVM.RejectReason;

                ReasonMapModel.note = ReasonMapVM.note;
                ReasonMapModel.user_crt = User.Identity.Name;
                ReasonMapModel.DateCrt = DateTime.Now;



                _context.ReasonMapingGroupModel.Add(ReasonMapModel);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            IEnumerable<SelectListItem> bankList;
            IEnumerable<SelectListItem> GroupRejectList;

            // List bank collector pendebetan CC
            List<int> bankIds = new List<int>();
            bankIds.Add(1); // BCA
            bankIds.Add(2); // Mandiri
            bankIds.Add(3); // BNI
            bankIds.Add(12); // Mega
            bankIds.Add(14); // CIMB

            bankList = _context.BankModel.Where(b => bankIds.Contains(b.bank_id))
                .Select(x => new SelectListItem { Value = x.bank_id.ToString(), Text = x.bank_code }).Where(r => r.Value != "0");
            GroupRejectList = _context.GroupRejectMappingModel.Select(x => new SelectListItem { Value = x.id.ToString(), Text = x.GroupRejectReason });
            ReasonMapVM.banks = bankList;
            ReasonMapVM.GroupReject = GroupRejectList;
            return View(ReasonMapVM);
        }

        private ReasonMapingGroupModel findRejectModel(int id)
        {
            return _context.ReasonMapingGroupModel.SingleOrDefault(m => m.id == id);
        }
    }
}