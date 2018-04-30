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
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Diagnostics;

namespace CAF.JBS.Controllers
{
    public class PolicyBillingController : Controller
    {
        private readonly JbsDbContext _context;
        private readonly Life21DbContext _contextLife21;
        private IFlashMessage flashMessage;

        private readonly string EmailCAF, EmailPHS, EmailFA, EmailCS, EmailBilling;

        private FileSettings filesettings;
        private readonly string DirCommand;
        private readonly string ConsoleExecResult;

        public PolicyBillingController(JbsDbContext context, Life21DbContext Life21, IFlashMessage flash)
        {
            filesettings = new FileSettings();
            _context = context;
            _contextLife21 = Life21;
            this.flashMessage = flash;

            var builder = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");
            var Configuration = builder.Build();

            EmailCAF = Configuration.GetValue<string>("Email:EmailCAF");
            EmailPHS = Configuration.GetValue<string>("Email:EmailPHS");
            EmailFA = Configuration.GetValue<string>("Email:EmailFA");
            EmailCS = Configuration.GetValue<string>("Email:EmailCS");
            EmailBilling = Configuration.GetValue<string>("Email:EmailBilling");
            DirCommand = filesettings.DirCommand;
            ConsoleExecResult = filesettings.FileExecresult;
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
	WHERE bl.`status_billing`<>'P' AND bl.`policy_id`=@polis
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
        public ActionResult AddPayment(int id, [Bind("PolicyId,BillingID,PaidDate,SourcePayment,PaidAmount")] PolicyAddPaymentSave polisVM)
        {
            Boolean retval = false;
            string message = "";

            var cmdx = _context.Database;
            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmdx.OpenConnection(); cmdx.BeginTransaction(); // jbs

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "add_payment_recurring";
            cmd.Parameters.Clear();
            cmd.Parameters.Add(new MySqlParameter("@polis_idx", MySqlDbType.Int32) { Value = polisVM.PolicyId });
            cmd.Parameters.Add(new MySqlParameter("@paid_datex", MySqlDbType.DateTime) { Value = polisVM.PaidDate });
            cmd.Parameters.Add(new MySqlParameter("@source_paymentx", MySqlDbType.VarChar) { Value = polisVM.SourcePayment });
            cmd.Parameters.Add(new MySqlParameter("@user_life21", MySqlDbType.VarChar) { Value = "2000" });
            cmd.Parameters.Add(new MySqlParameter("@user_jbs", MySqlDbType.VarChar) { Value = User.Identity.Name });
            try
            {
                int bill;
                var billing_id = cmd.ExecuteScalar().ToString();
                if (int.TryParse(billing_id, out bill))
                {
                    systemEmailQueueModel emailSent = new systemEmailQueueModel();
                    SetEmailThanksRecurring(bill, "UploadCCResult", ref emailSent);

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.CommandText = @"INSERT INTO `prod_life21`.`system_email_queue`(`email_to`, `email_subject`, `email_body`,`email_created_dt`, `email_status`, `email_bcc`, `email_type`)
                    VALUES (@email_to, @email_subject, @email_body, @tgl, 'P', @email_bcc, 'UploadCCResult')";
                    cmd.Parameters.Add(new MySqlParameter("@email_to", MySqlDbType.VarChar) { Value = emailSent.email_to });
                    cmd.Parameters.Add(new MySqlParameter("@email_subject", MySqlDbType.VarChar) { Value = emailSent.email_subject });
                    cmd.Parameters.Add(new MySqlParameter("@email_body", MySqlDbType.Text) { Value = emailSent.email_body });
                    cmd.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = DateTime.Now });
                    cmd.Parameters.Add(new MySqlParameter("@email_bcc", MySqlDbType.VarChar) { Value = emailSent.email_bcc });
                    cmd.ExecuteNonQuery();

                    cmdx.CommitTransaction();
                    retval = true;
                    message = "sukses";
                }
                else
                {
                    throw new Exception("Salah billing id");
                }
            }
            catch (Exception ex)
            {
                retval = false;
                message = ex.Message;
                if (cmdx.CurrentTransaction != null) cmdx.RollbackTransaction();
            }
            finally
            {
                if (cmdx.CurrentTransaction != null) cmdx.RollbackTransaction();
                cmd.Connection.Close();
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

        public void SetEmailThanksRecurring(int BillID, string TipeEmail, ref systemEmailQueueModel emailSent)
        {
            EmailThanksRecurringVM EmailThanks;
            EmailThanks = (from b in _context.BillingModel
                           join pb in _context.PolicyBillingModel on b.policy_id equals pb.policy_Id
                           join ci in _context.CustomerInfo on pb.holder_id equals ci.CustomerId
                           join pd in _context.Product on pb.product_id equals pd.product_id
                           where b.BillingID == BillID
                           select new EmailThanksRecurringVM()
                           {
                               PolicyNo = pb.policy_no,
                               CustomerName = ci.CustomerName,
                               Salam = (ci.IsLaki == true) ? "Bapak" : "Ibu",
                               CustomerEmail = ci.Email,
                               ProductName = pd.product_description,
                               PremiAmount = b.TotalAmount
                           }).SingleOrDefault();
            string SubjectEmail = string.Format(@"JAGADIRI: Penerimaan Premi Regular {0} {1} {2}", EmailThanks.ProductName, EmailThanks.PolicyNo, EmailThanks.CustomerName.ToUpper());
            string BodyMessage = string.Format(@"Salam hangat {0} {1},<br>
<p style='text-align:justify'>Bersama surat ini kami ingin mengucapkan terima kasih atas pembayaran Premi Regular untuk Polis {2} dengan nomor polis {3} sejumlah IDR {4} yang telah kami terima. Pembayaran Premi tersebut secara otomatis akan membuat Polis Asuransi Anda tetap aktif dan memberikan manfaat perlindungan maksimal bagi Anda dan keluarga.</p>
<br>Sukses selalu,
<br>JAGADIRI ", EmailThanks.Salam, EmailThanks.CustomerName.ToUpper(), EmailThanks.ProductName, EmailThanks.PolicyNo, EmailThanks.PremiAmount.ToString("#,###"));
            try
            {
                emailSent.email_body = BodyMessage;
                emailSent.email_subject = SubjectEmail;
                emailSent.email_to = EmailThanks.CustomerEmail;
                emailSent.email_bcc = this.EmailPHS;
                emailSent.email_type = TipeEmail;
                emailSent.email_status = "P";
                emailSent.email_created_dt = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw new Exception("SetEmailThanksRecurring => (BillID = " + BillID.ToString() + ") " + ex.Message);
            }
        }
    }
}
