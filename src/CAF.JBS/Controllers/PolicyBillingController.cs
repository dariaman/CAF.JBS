using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAF.JBS.Data;
using CAF.JBS.Models;
using CAF.JBS.ViewModels;
using System.Data;
using System.Text.RegularExpressions;
using DataTables.AspNet.Core;
using DataTables.AspNet.AspNetCore;

namespace CAF.JBS.Controllers
{
    public class PolicyBillingController : Controller
    {
        private readonly JbsDbContext _context;

        public PolicyBillingController(JbsDbContext context)
        {
            _context = context;
        }

        // GET: PolicyBilling
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            var polis = await (from p in _context.PolicyBillingModel
                               join c in _context.CustomerInfo on p.holder_id equals c.CustomerId
                               join pd in _context.Product on p.product_id equals pd.product_id
                               where p.policy_Id.Equals(id)
                               select new PolicyCycleDateVM()
                               {
                                   policy_Id = p.policy_Id,
                                   cycleDate = p.cycleDate,
                                   CylceDateNotes=p.CylceDateNotes,
                                   policy_no = p.policy_no,
                                   commence_dt = p.commence_dt,
                                   payment_method = p.payment_method,
                                   premium_mode = p.premium_mode,
                                   regular_premium = p.regular_premium,
                                   Status = p.Policy_status,
                                   product_Name = pd.product_description,
                                   HolderName = c.CustomerName
                               }).SingleOrDefaultAsync();
            if (polis == null) return NotFound();

            return PartialView(polis);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind("policy_Id,cycleDate,CylceDateNotes")] PolicyCycleDateVM polisVM)
        {
            Boolean retval =false;
            string message = "";

            if (id != polisVM.policy_Id)
            {
                return NotFound();
            }

            if (polisVM.cycleDate < 1 || polisVM.cycleDate > 31)
            {
                message = " cycleDate harus diantara 1 - 31 ! ";
                return Json(new { data = retval, message = message });
            }

            var polis = this.findPolicyModel(id);
            if (polis == null)
            {
                message = " Polis Tidak ditemukan ! ";
                return Json(new { data = retval, message = message });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    polis.cycleDate = polisVM.cycleDate;
                    if(polisVM.CylceDateNotes != "") polis.CylceDateNotes = polisVM.CylceDateNotes;
                    _context.Update(polis);
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

        private bool PolicyModelExists(int id)
        {
            return _context.PolicyBillingModel.Any(e => e.policy_Id == id);
        }

        private PolicyBillingModel findPolicyModel(int id)
        {
            return _context.PolicyBillingModel.SingleOrDefault(m => m.policy_Id == id); ;
        }

        public IActionResult PageData(IDataTablesRequest request)
        {
            int jlh = 0, jlhFilter = 0;
            string sort = "";
            var sqlFilter = GenerateFilter(request, ref sort);

            List<PolicyBillingViewModel> Polis = new List<PolicyBillingViewModel>();
            Polis = GetPageData(request.Start, sort, sqlFilter, ref jlhFilter, ref jlh);
            var response = DataTablesResponse.Create(request, jlh, jlhFilter, Polis);

            return new DataTablesJsonResult(response);
        }

        private string GenerateFilter(IDataTablesRequest request, ref string sort)
        {
            string FilterSql = "";
            DateTime tgl = DateTime.Now.Date;

            string paternAngkaLike = @"[^0-9,%]";
            string paternAngka = @"[^0-9]";
            string paternHurufLike = @"[^a-zA-Z,%]";
            string paternHuruf = @"[^a-zA-Z]";

            int i = 0;
            foreach (var req in request.Columns)
            {
                i++;
                if (req.Sort != null) sort = string.Format(" {0} {1} ", i, req.Sort.Direction.ToString().ToLower() == "ascending" ? "ASC" : "DESC");

                if (req.Search == null) continue;
                if (req.Search.Value == null) continue;

                if (req.Field == "policy_Id" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngkaLike, "");
                    FilterSql += " AND pb.`policy_Id` like '" + tmp + "'";
                }
                else if (req.Field == "policy_no" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngkaLike, "");
                    FilterSql += " AND pb.`policy_no` like '" + tmp + "'";
                }
                else if (req.Field == "payment_method" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternHuruf, "");
                    FilterSql += " AND pb.`payment_method` = '" + tmp + "'";
                }
                else if (req.Field == "premium_mode" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pb.`premium_mode`='" + tmp + "'";
                }
                else if (req.Field == "cycleDate" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pb.`cycleDate`='" + tmp + "'";
                }
                else if (req.Field == "cylceDateNotes" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternHurufLike, "");
                    FilterSql += " AND pb.`CylceDateNotes` like '" + tmp + "'";
                }
                else if (req.Field == "product_description" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value.Trim(), "[^a-zA-Z ,%]", "");
                    FilterSql += " AND pd.`product_description` like '" + tmp + "'";
                }
                else if (req.Field == "customerName" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value.Trim(), "[^a-zA-Z ,%]", "");
                    FilterSql += " AND ci.`CustomerName` like '" + tmp + "'";
                }
                else if (req.Field == "policy_status" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternHurufLike, "");
                    FilterSql += " AND pb.`Policy_status` like '" + tmp + "'";
                }
                else if (req.Field == "isHoldBilling" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pb.`IsHoldBilling`='" + tmp + "'";
                }
                else if (req.Field == "isWatchList" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pb.`IsWatchList`='" + tmp + "'";
                }
                else if (req.Field == "isRenewal" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pb.`IsRenewal`='" + tmp + "'";
                }
                else if (req.Field == "worksite_org_name" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternHurufLike, "");
                    FilterSql += " AND pb.`worksite_org_name` like '" + tmp + "'";
                }
            }

            return FilterSql;
        }

        private List<PolicyBillingViewModel> GetPageData(int rowStart, string orderString, string FilterWhere, ref int jlhdataFilter, ref int jlhData)
        {
            FilterWhere = string.Concat(" WHERE 1=1 ", FilterWhere);
            string order = (orderString == "" ? "" : string.Format(" ORDER BY {0} ", orderString));
            string limit = string.Format(" LIMIT {0},10 ", rowStart);
            PolicyBillingViewModel dt = new PolicyBillingViewModel();
            List<PolicyBillingViewModel> ls = new List<PolicyBillingViewModel>();

            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = QueryPaging(GetDataSelect(), FilterWhere, order, limit);

            try
            {
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    ls.Add(new PolicyBillingViewModel()
                    {
                        policy_Id = rd["policy_Id"].ToString(),
                        policy_no = rd["policy_no"].ToString(),
                        commence_dt = Convert.ToDateTime(rd["commence_dt"]),
                        due_dt = Convert.ToDateTime(rd["due_dt"]),
                        payment_method = rd["payment_method"].ToString(),

                        premium_mode = rd["premium_mode"].ToString(),
                        cycleDate = rd["cycleDate"].ToString(),
                        CylceDateNotes = rd["CylceDateNotes"].ToString(),
                        product_description = rd["product_description"].ToString(),

                        CustomerName = rd["CustomerName"].ToString(),
                        regular_premium = Convert.ToDecimal(rd["regular_premium"]),
                        cashless_fee_amount = Convert.ToDecimal(rd["cashless_fee_amount"]),
                        Policy_status = rd["Policy_status"].ToString(),

                        IsHoldBilling = Convert.ToBoolean(rd["IsHoldBilling"]),
                        IsWatchList = Convert.ToBoolean(rd["IsWatchList"]),
                        IsRenewal = Convert.ToBoolean(rd["IsRenewal"]),
                        worksite_org_name = rd["worksite_org_name"].ToString(),
                        DateCrt = rd["DateCrt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["DateCrt"])
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
                    FROM `policy_billing` pb
                    LEFT JOIN `product` pd ON pd.`product_id`=pb.`product_id`
                    LEFT JOIN `customer_info` ci ON ci.`CustomerId`=pb.`holder_id` " +
                    where + order + limit;

            return sql;
        }

        private string GetDataSelect()
        {
            string select = @"pb.`policy_Id`,
                            pb.`policy_no`,
                            pb.`commence_dt`,
                            pb.`due_dt`,
                            pb.`payment_method`,
                            pb.`premium_mode`,
                            pb.`cycleDate`,
                            pb.`CylceDateNotes`,
                            pd.`product_description`,
                            ci.`CustomerName`,
                            pb.`regular_premium`,
                            pb.`cashless_fee_amount`,
                            pb.`Policy_status`,
                            pb.`IsHoldBilling`,
                            pb.`IsWatchList`,
                            pb.`IsRenewal`,
                            pb.`worksite_org_name`,pb.`DateCrt`";
            return select;
        }
    }
}
