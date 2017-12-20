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
using MySql.Data.MySqlClient;

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

        public ActionResult AddPayment(int id)
        {
            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = @"
SELECT pb.`policy_Id`,pb.`policy_no`,pb.`premium_mode`,pb.`commence_dt`,pb.`due_dt`,pb.`Policy_status`,
pd.`product_description`,ci.`CustomerName`,
b.`BillingID`, b.`due_dt_pre`,b.`policy_regular_premium`,b.`cashless_fee_amount`,b.`TotalAmount`,
pb.`cashless_fee_amount` AS PolisCashless,pb.`regular_premium` AS polisPremi,pt.`due_dt_pre` AS lastDueDatePre
FROM `policy_billing` pb
INNER JOIN `product` pd ON pd.`product_id`=pb.`product_id`
INNER JOIN `customer_info` ci ON ci.`CustomerId`=pb.`holder_id`
LEFT JOIN (
	SELECT * FROM `billing` bl
	WHERE bl.`status_billing`='A' AND bl.`policy_id`=@polis
	ORDER BY bl.`recurring_seq` 
	LIMIT 1
)b ON pb.`policy_Id`=b.policy_id
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
                        polix = new PolicyAddPayment() {
                            BillingID= rd["BillingID"].ToString(),
                            PolicyId = rd["policy_Id"].ToString(),
                            policy_no = rd["policy_no"].ToString(),
                            StatusPolis = rd["Policy_status"].ToString(),
                            CommenceDate = Convert.ToDateTime(rd["commence_dt"]),
                            DueDate = Convert.ToDateTime(rd["due_dt"]),
                            ProductDesc = rd["product_description"].ToString(),
                            PremiumMode = rd["premium_mode"].ToString(),
                            SourcePayment = "Bank Transfer",
                            HolderName = rd["CustomerName"].ToString(),
                            PaidAmount= Convert.ToDecimal(rd["TotalAmount"]),
                            PaidDate=DateTime.Now.Date,
                            BillingDate = DateTime.Now.Date,
                            CashLess = rd["BillingID"] == DBNull.Value ? Convert.ToDecimal(rd["PolisCashless"]) : 
                                Convert.ToDecimal(rd["cashless_fee_amount"]),
                            Premi = rd["BillingID"] == DBNull.Value ? Convert.ToDecimal(rd["regular_premium"]) :
                                Convert.ToDecimal(rd["policy_regular_premium"]),
                            Due_date_pre = rd["BillingID"] == DBNull.Value ? Convert.ToDateTime(rd["lastDueDatePre"]) :
                                Convert.ToDateTime(rd["due_dt_pre"]),
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
            var polis = this.findPolicyModel(id);
            var billing = this.findBillingAktif(polisVM.BillingID ?? 0);

            if (billing == null)
            {
                var PolisBill = this.findPolisBillingAktif(polisVM.PolicyId);
                // Create Billing and Paid
                if (PolisBill == null) CreateBilling(polisVM.PolicyId);
                billing = this.findPolisBillingAktif(polisVM.PolicyId);

                if (billing == null) throw new Exception("Gagal Create Billing");

                billing.paid_date = polisVM.PaidDate;
                billing.BillingDate= polisVM.PaidDate.Date;
                billing.IsDownload = false;
                billing.IsClosed = true;
                billing.status_billing = "P";
                billing.cancel_date = null;
                billing.PaymentSource = "BT";
                billing.BankIdPaid = 1;
                billing.PaidAmount = polisVM.PaidAmount;

                billing.ReceiptID = 0;
                billing.ReceiptOtherID = 0;
                billing.PaymentTransactionID = 0;

                billing.UserUpdate = "";
                billing.DateUpdate = DateTime.Now;

            }
            else
            {
                // Update Billing Paid

            }
            return PartialView();
        }

        private void CreateBilling(int polisID)
        {
            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"INSERT INTO `billing`(`BillingID`,`policy_id`,`recurring_seq`,`due_dt_pre`,`policy_regular_premium`,`cashless_fee_amount`,`TotalAmount`)
    SELECT 
		1 AS bill_id,
		pb.`policy_Id`,
		1 AS rec_seq,
		CASE WHEN DAY(pb.`commence_dt`)=31 
			THEN LAST_DAY(DATE_ADD(COALESCE(bx.`due_dt_pre`,pt.`due_dt_pre`,pb.`commence_dt`), INTERVAL pb.premium_mode MONTH)) 
		     WHEN DAY(pb.`commence_dt`)>28 AND MONTH(DATE_ADD(COALESCE(bx.`due_dt_pre`,pt.`due_dt_pre`,pb.`commence_dt`), INTERVAL pb.premium_mode MONTH))=2
			THEN LAST_DAY(DATE_ADD(COALESCE(bx.`due_dt_pre`,pt.`due_dt_pre`,pb.`commence_dt`), INTERVAL pb.premium_mode MONTH))
		ELSE DATE_ADD(COALESCE(bx.`due_dt_pre`,pt.`due_dt_pre`,pb.`commence_dt`), INTERVAL pb.premium_mode MONTH)
		END AS due_dt_pre,
		COALESCE(pp.premium_amount,pb.`regular_premium`) AS regular_premium,
		pb.`cashless_fee_amount`,
		COALESCE(pp.premium_amount,pb.`regular_premium`) + pb.`cashless_fee_amount`
	FROM `policy_billing` pb
	INNER JOIN `product` pd ON pd.`product_id`=pb.`product_id`
	LEFT JOIN `policy_prerenewal` pp ON pp.policy_Id=pb.policy_Id 
		AND LAST_DAY(pp.after_commence_dt) <= LAST_DAY(NOW())
		AND pp.premium_amount <> pb.`regular_premium`
	LEFT JOIN `policy_last_trans` pt ON pt.policy_Id=pb.policy_Id
	LEFT JOIN(
		SELECT MAX(b.`recurring_seq`) AS recurring_seq, b.`policy_id`
		FROM `billing` b
		LEFT JOIN `policy_billing` pb ON pb.`policy_Id`=b.`policy_id`
		WHERE pb.`policy_Id`=@polisID
		GROUP BY pb.`policy_Id`
	)a ON a.policy_id=pb.`policy_Id`
	LEFT JOIN `billing` bx ON bx.`recurring_seq`=a.recurring_seq AND pb.`policy_Id`=bx.`policy_id`
	WHERE pb.`policy_Id`=@polisID;";
            cmd.Parameters.Clear();
            cmd.Parameters.Add(new MySqlParameter("@polisID", MySqlDbType.Int32) { Value = polisID });

            if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
            try
            {
                cmd.ExecuteNonQuery();

            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally { cmd.Connection.Close(); }
        }

        private bool PolicyModelExists(int id)
        {
            return _context.PolicyBillingModel.Any(e => e.policy_Id == id);
        }

        private PolicyBillingModel findPolicyModel(int id)
        {
            return _context.PolicyBillingModel.SingleOrDefault(m => m.policy_Id == id); ;
        }

        private BillingModel findBillingAktif(int id)
        {
            return _context.BillingModel.FirstOrDefault(m => m.BillingID == id && m.status_billing!="P");
        }

        private BillingModel findPolisBillingAktif(int id)
        {
            return _context.BillingModel
                .OrderBy(b => new {b.policy_id, b.recurring_seq })
                .SingleOrDefault(m => m.policy_id == id && m.status_billing != "P");
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
