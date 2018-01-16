using CAF.JBS.Data;
using CAF.JBS.Models;
using CAF.JBS.ViewModels;
using DataTables.AspNet.AspNetCore;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vereyon.Web;

namespace CAF.JBS.Controllers
{
    public class PolicyAcController : Controller
    {
        private readonly JbsDbContext _context;
        //private IFlashMessage flashMessage;

        public PolicyAcController(JbsDbContext context)
        {
            _context = context;
            //flashMessage = flash;
        }

        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> Edit(int id)
        {
            var polis = await (from p in _context.PolicyBillingModel
                               join pa in _context.PolicyAcModel on p.policy_Id equals pa.PolicyId
                               join c in _context.CustomerInfo on p.holder_id equals c.CustomerId
                               join pd in _context.Product on p.product_id equals pd.product_id
                               join b in _context.BankModel on pa.bank_id equals b.bank_id
                               where p.policy_Id.Equals(id)
                               select new PolicyCycleDateVM()
                               {
                                   policy_Id = p.policy_Id,
                                   cycleDate = pa.cycleDate ?? 0 ,
                                   CylceDateNotes = pa.cycleDateNote,
                                   policy_no = p.policy_no,
                                   commence_dt = p.commence_dt,
                                   payment_method = p.payment_method,
                                   premium_mode = p.premium_mode,
                                   regular_premium = p.regular_premium,
                                   Status = p.Policy_status,
                                   product_Name = pd.product_description,
                                   HolderName = c.CustomerName,
                                   acc_no = pa.acc_no,
                                   acc_name = pa.acc_name,
                                   BankName = b.bank_code,
                                   IsSkdr = pa.IsSKDR
                               }).SingleOrDefaultAsync();
            if (polis == null) return NotFound();

            return PartialView(polis);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind("policy_Id,cycleDate,CylceDateNotes")] PolicyCycleDateVM polisVM)
        {
            Boolean retval = false;
            string message = "";

            if (id != polisVM.policy_Id)
            {
                return NotFound();
            }

            if (polisVM.cycleDate < 0 || polisVM.cycleDate > 31)
            {
                message = " cycleDate harus diantara 0 - 31 ! ";
                return Json(new { data = retval, message = message });
            }

            var polisAC = this.findPolicyModel(id);
            if (polisAC == null)
            {
                message = " Polis Tidak ditemukan ! ";
                return Json(new { data = retval, message = message });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    polisAC.cycleDate = polisVM.cycleDate;
                    if (polisVM.CylceDateNotes != "") polisAC.cycleDateNote = polisVM.CylceDateNotes;
                    _context.Update(polisAC);
                    _context.SaveChanges();
                    retval = true;
                    message = "sukses";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    retval = false;
                    message = ex.Message;
                }
            }
            return Json(new { data = retval, message = message });
        }

        private PolicyAcModel findPolicyModel(int id)
        {
            return _context.PolicyAcModel.SingleOrDefault(m => m.PolicyId == id); ;
        }

        public IActionResult PageData(IDataTablesRequest request)
        {
            int jlh = 0, jlhFilter = 0;
            string sort = "";
            var sqlFilter = GenerateFilter(request, ref sort);

            List<PolicyAcVM> polisAC = new List<PolicyAcVM>();
            polisAC = GetPageData(request.Start, sort, sqlFilter, ref jlhFilter, ref jlh);
            var response = DataTablesResponse.Create(request, jlh, jlhFilter, polisAC);

            return new DataTablesJsonResult(response);
        }

        private string GenerateFilter(IDataTablesRequest request, ref string sort)
        {
            string FilterSql = "";
            DateTime tgl = DateTime.Now.Date;

            string paternAngka = @"[^0-9,%]";
            //string paternAngkaHuruf = @"[^0-9a-zA-Z,%]";
            string paternHuruf = @"[^0-9a-zA-Z ,%]";

            int i = 0;
            foreach (var req in request.Columns)
            {
                i++;
                if (req.Sort != null) sort = string.Format(" {0} {1} ", i, req.Sort.Direction.ToString().ToLower() == "ascending" ? "ASC" : "DESC");

                if (req.Search == null) continue;
                if (req.Search.Value == null) continue;

                if (req.Field == "policyId" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pa.`PolicyId` like '" + tmp + "'";
                }
                else if (req.Field == "policy_no" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pb.`policy_no` like '" + tmp + "'";
                }
                else if (req.Field == "acc_no" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pa.`acc_no` like '" + tmp + "'";
                }
                else if (req.Field == "acc_name" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value.Trim(), paternHuruf, "");
                    FilterSql += " AND pa.`acc_name` like '" + tmp + "'";
                }
                else if (req.Field == "bank_code" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value.Trim(), "[^a-zA-Z %]", "");
                    FilterSql += " AND b.`bank_code` like '" + tmp + "'";
                }
                else if (req.Field == "cycleDate" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pa.`cycleDate` like '" + tmp + "'";
                }
                else if (req.Field == "isSKDR" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pa.`IsSKDR`='" + tmp + "'";
                }

            }

            return FilterSql;
        }

        private List<PolicyAcVM> GetPageData(int rowStart, string orderString, string FilterWhere, ref int jlhdataFilter, ref int jlhData)
        {
            FilterWhere = string.Concat(" WHERE 1=1 ", FilterWhere);
            string order = (orderString == "" ? "" : string.Format(" ORDER BY {0} ", orderString));
            string limit = string.Format(" LIMIT {0},10 ", rowStart);
            PolicyAcVM dt = new PolicyAcVM();
            List<PolicyAcVM> ls = new List<PolicyAcVM>();

            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = QueryPaging(GetDataSelect(), FilterWhere, order, limit);

            try
            {
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    ls.Add(new PolicyAcVM()
                    {
                        PolicyId = rd["PolicyId"].ToString(),
                        policy_no = rd["policy_no"].ToString(),
                        acc_no = rd["acc_no"].ToString(),
                        acc_name = rd["acc_name"].ToString(),
                        bank_code = rd["bank_code"].ToString(),
                        cycleDate = rd["cycleDate"].ToString(),
                        cycleDateNote = rd["cycleDateNote"].ToString(),
                        IsSKDR = Convert.ToBoolean(rd["IsSKDR"]),
                        DateCrt = rd["DateCrt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["DateCrt"]),
                        DateUpdate = rd["DateUpdate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["DateUpdate"]),
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
                    FROM `policy_ac` pa
                    LEFT JOIN `policy_billing` pb ON pb.`policy_Id`=pa.`PolicyId`
                    LEFT JOIN `bank` b ON b.`bank_id`=pa.`bank_id` " +
                    where + order + limit;

            return sql;
        }

        private string GetDataSelect()
        {
            string select = @"pa.`PolicyId`,
                            pb.`policy_no`,
                            pa.`acc_no`,
                            pa.`acc_name`,
                            b.`bank_code`,
                            COALESCE(pa.`cycleDate`,0) as cycleDate,
                            pa.`cycleDateNote`,
                            pa.`IsSKDR`,
                            pa.`DateCrt`,
                            pa.`DateUpdate`";
            return select;
        }
    }
}
