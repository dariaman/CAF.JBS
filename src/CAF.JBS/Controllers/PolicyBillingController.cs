using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CAF.JBS.Data;
using CAF.JBS.Models;
using CAF.JBS.ViewModels;
using System.Data;
using System.Text.RegularExpressions;
using DataTables.AspNet.Core;
using DataTables.AspNet.AspNetCore;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Authorization;
using Vereyon.Web;

namespace CAF.JBS.Controllers
{
    public class PolicyBillingController : Controller
    {
        private readonly JbsDbContext _context;
        private IFlashMessage flashMessage;

        public PolicyBillingController(JbsDbContext context, IFlashMessage flash)
        {
            _context = context;
            this.flashMessage = flash;
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
                                   CylceDateNotes = p.CylceDateNotes,
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
        //[Authorize(User  = "Administrator, PowerUser")]
        public ActionResult Edit(int id, [Bind("policy_Id,cycleDate,CylceDateNotes")] PolicyCycleDateVM polisVM)
        {
            Boolean retval = false;
            string message = "";

            if (id != polisVM.policy_Id) return NotFound();

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
                    if (polisVM.CylceDateNotes != "") polis.CylceDateNotes = polisVM.CylceDateNotes;
                    _context.Update(polis);
                    _context.SaveChanges();
                    retval = true;
                    message = "sukses";
                    //flashMessage.Confirmation("Sukses");
                }
                catch (Exception ex)
                {
                    retval = false;
                    message = ex.Message;
                    //flashMessage.Danger(ex.Message);
                }
            }
            return Json(new { data = retval, message = message });
        }

        public ActionResult CekUserAdmin()
        {
            Boolean retval = false;
            string message = "";
            if (User.Identity.Name == "dariaman.siagian@jagadiri.co.id")
            {
                retval = true;
                message = "";
            }
            else
            {
                retval = false;
                message = "Action belum ready !!";
            }
            return Json(new { data = retval, message = message });
        }
        public ActionResult AddPayment(int id)
        {

            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = @"
SELECT pb.`policy_Id`,pb.`policy_no`,pb.`premium_mode`,pb.`commence_dt`,pb.`due_dt`,pb.`Policy_status`,pd.`product_description`,ci.`CustomerName`,
b.`BillingID`, 
COALESCE(b.`due_dt_pre`,DATE_ADD(COALESCE(b2.`due_dt_pre`,pt.`due_dt_pre`,pb.`commence_dt`), INTERVAL pb.`premium_mode` MONTH)) AS due_dt_pre,
b.`policy_regular_premium`,b.`cashless_fee_amount`,b.`TotalAmount`,
pb.`cashless_fee_amount` AS PolisCashless,pb.`regular_premium` AS polisPremi

FROM `policy_billing` pb
INNER JOIN `product` pd ON pd.`product_id`=pb.`product_id`
INNER JOIN `customer_info` ci ON ci.`CustomerId`=pb.`holder_id`
LEFT JOIN (
    SELECT bl.`policy_id`,
		bl.`BillingID`, 
		bl.`due_dt_pre`,
		bl.`policy_regular_premium`,
		bl.`cashless_fee_amount`,
		bl.`TotalAmount`
	FROM `billing` bl
	WHERE bl.`status_billing`='A' AND bl.`policy_id`=@polis
	ORDER BY bl.`due_dt_pre` ASC
	LIMIT 1
)b ON pb.`policy_Id`=b.policy_id
LEFT JOIN (
	SELECT bz.`policy_id`,
		bz.`due_dt_pre`
	FROM `billing` bz
	WHERE bz.`policy_id`=@polis
	ORDER BY bz.`due_dt_pre`  DESC
	LIMIT 1
)b2 ON pb.`policy_Id`=b2.policy_id
LEFT JOIN `policy_last_trans` pt ON pt.`policy_Id`=pb.`policy_Id`
WHERE pb.`policy_Id`=@polis";
            cmd.Parameters.Add(new MySqlParameter("@polis", MySqlDbType.Int32) { Value = id });
            var polix = new PolicyAddPayment();
            try
            {
                cmd.Connection.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        polix = new PolicyAddPayment()
                        {
                            BillingID = rd["BillingID"].ToString(),
                            PolicyId = rd["policy_Id"].ToString(),
                            policy_no = rd["policy_no"].ToString(),
                            StatusPolis = rd["Policy_status"].ToString(),
                            CommenceDate = Convert.ToDateTime(rd["commence_dt"]),
                            DueDate = Convert.ToDateTime(rd["due_dt"]),
                            ProductDesc = rd["product_description"].ToString(),
                            PremiumMode = rd["premium_mode"].ToString(),
                            SourcePayment = "Bank Transfer",
                            HolderName = rd["CustomerName"].ToString(),
                            PaidAmount = rd["BillingID"] == DBNull.Value
                                    ? Convert.ToDecimal(rd["polisPremi"]) + Convert.ToDecimal(rd["PolisCashless"])
                                    : Convert.ToDecimal(rd["TotalAmount"]),
                            PaidDate = DateTime.Now.Date,
                            BillingDate = DateTime.Now.Date,
                            CashLess = rd["BillingID"] == DBNull.Value ? Convert.ToDecimal(rd["PolisCashless"]) :
                                Convert.ToDecimal(rd["cashless_fee_amount"]),
                            Premi = rd["BillingID"] == DBNull.Value ? Convert.ToDecimal(rd["polisPremi"]) :
                                Convert.ToDecimal(rd["policy_regular_premium"]),
                            Due_date_pre = rd["due_dt_pre"] == DBNull.Value ? DateTime.Now.Date : Convert.ToDateTime(rd["due_dt_pre"]),
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cmd.Connection.Close();
            }

            return PartialView(polix);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPayment(int id, [Bind("PolicyId,BillingID,PaidDate,Premi,CashLess,PaidAmount")] PolicyAddPaymentSave polisVM)
        {
            if (User.Identity.Name != "dariaman.siagian@jagadiri.co.id")
            {
                flashMessage.Danger("Proses tidak dapat akses");
                return RedirectToAction("index");
            }

            Boolean retval = false;
            string message = "";
            var polis = this.findPolicyModel(id);
            var billing = this.findBillingAktif(polis.policy_Id);

            if (billing == null)
            {
                // Create New Billing
                var cmd = _context.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CreateNewBillingRecurring";
                cmd.Parameters.Add(new MySqlParameter("@polisId", MySqlDbType.Int32) { Value = polis.policy_Id });
                try
                {
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    retval = false;
                    message = ex.Message;
                    //flashMessage.Danger("ex.Message");
                }
                finally { cmd.Connection.Close(); }
            }

            billing = this.findBillingAktif(polis.policy_Id);
            // Proses Update pembayaran, setelah create billing
            if (billing == null)
            {
                retval = false;
                message = "Proses Error Setelah Create Billing";
            }

            try
            {
                billing.paid_date = polisVM.PaidDate;
                billing.LastUploadDate = polisVM.PaidDate;
                billing.PaidAmount = polisVM.PaidAmount;
                billing.BankIdPaid = 1; // dianggap bayar ke Akun BCA (Transfer Manual)
                billing.status_billing = "P";
                billing.IsClosed = true;
                billing.IsDownload = false;
                billing.IsPending = false;
                billing.BillingDate = polisVM.PaidDate;
                billing.PaymentSource = "BT";
                _context.Update(billing);
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

        private bool PolicyModelExists(int id)
        {
            return _context.PolicyBillingModel.Any(e => e.policy_Id == id);
        }

        private PolicyBillingModel findPolicyModel(int id)
        {
            return _context.PolicyBillingModel.FirstOrDefault(m => m.policy_Id == id); ;
        }

        private BillingModel findBillingAktif(int polisid)
        {
            // Cari Billing yang belum paid
            return _context.BillingModel
                .OrderBy(b => new { b.policy_id, b.recurring_seq })
                .FirstOrDefault(m => m.policy_id == polisid && m.status_billing != "P");
        }

        //private BillingModel findPolisBillingAktif(int id)
        //{
        //    return _context.BillingModel
        //        .OrderBy(b => new {b.policy_id, b.recurring_seq })
        //        .SingleOrDefault(m => m.policy_id == id && m.status_billing != "P");
        //}

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
