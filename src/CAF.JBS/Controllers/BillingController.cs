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
using System.Diagnostics;
using System.Data;
using MySql.Data.MySqlClient;
using DataTables.AspNet.AspNetCore;
using System.Text.RegularExpressions;
using DataTables.AspNet.Core;

namespace CAF.JBS.Controllers
{
    public class BillingController : Controller
    {

        private readonly string ConsoleFile;
        private FileSettings filesettings;

        private readonly JbsDbContext _context;

        public BillingController(JbsDbContext context)
        {
            _context = context;
            filesettings = new FileSettings();
            ConsoleFile = filesettings.GenFileXls;
        }

        // GET: Billing
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult PageData(IDataTablesRequest request)
        {
            int jlh = 0, jlhFilter = 0;
            string sort = "";
            var sqlFilter = GenerateFilter(request, ref sort);

            List<BillingViewModel> Billing= new List<BillingViewModel>();
            Billing = GetPageData(request.Start, sort, sqlFilter, ref jlhFilter, ref jlh);

            var filteredData = Billing;

            var response = DataTablesResponse.Create(request, jlh, jlhFilter, filteredData);

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

                if (req.Field == "billingID" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngkaHuruf, "");
                    FilterSql += " AND b.`BillingID` like '" + tmp + "'";

                }
                else if (req.Field == "policy_id" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pb.`policy_Id` like '" + tmp + "'";
                }
                else if (req.Field == "policyNo" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pb.`policy_no` like '" + tmp + "'";
                }
                else if (req.Field == "payment_method" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, "[^a-zA-Z]", "");
                    FilterSql += (tmp == "x" ? " AND pb.`payment_method` NOT IN ('AC','CC') " : " AND pb.`payment_method`='" + tmp + "'");
                }
                else if (req.Field == "recurring_seq" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, "[^0-9]", "");
                    FilterSql += " AND b.`recurring_seq`='" + tmp + "'";
                }
                else if (req.Field == "status_billing" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, "[^a-zA-Z]", "");
                    FilterSql += " AND b.`status_billing`='" + tmp + "'";
                }
                else if (req.Field == "paymentSource" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, "[^a-zA-Z]", "");
                    FilterSql += " AND b.`PaymentSource`='" + tmp + "'";
                }
                else if (req.Field == "isHold" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, "[^0-1]", "");
                    FilterSql += " AND COALESCE(NULLIF(b.`IsPending`,0),pb.`IsHoldBilling`)='" + tmp + "'";
                }
                else if (req.Field == "lastUploadDate" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    if (DateTime.TryParse(req.Search.Value, out tgl))
                    {
                        FilterSql += " AND b.`LastUploadDate` >= '" + tgl.ToString("yyyy-MM-dd") + "'";
                        FilterSql += " AND b.`LastUploadDate` <  '" + tgl.AddDays(1).ToString("yyyy-MM-dd") + "'";
                    }
                }
            }

            return FilterSql;
        }

        private List<BillingViewModel> GetPageData(int rowStart, string orderString, string FilterWhere, ref int jlhdataFilter, ref int jlhData)
        {
            FilterWhere = string.Concat(" WHERE 1=1 ", FilterWhere);
            string order = (orderString == "" ? "" : string.Format(" ORDER BY {0} ", orderString));
            string limit = string.Format(" LIMIT {0},10 ", rowStart);
            BillingViewModel dt = new BillingViewModel();
            List<BillingViewModel> ls = new List<BillingViewModel>();

            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = QueryPaging(GetDataSelect(), FilterWhere, order, limit);

            try
            {
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    ls.Add(new BillingViewModel()
                    {
                        BillingID = rd["BillingID"].ToString(),
                        policy_id = rd["policy_id"].ToString(),
                        PolicyNo = rd["policy_no"].ToString(),
                        payment_method= rd["payment_method"].ToString(),
                        recurring_seq= rd["recurring_seq"].ToString(),
                        BillingDate = rd["BillingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["BillingDate"]),
                        due_dt_pre = Convert.ToDateTime(rd["due_dt_pre"]),
                        policy_regular_premium= Convert.ToDecimal(rd["policy_regular_premium"]),
                        cashless_fee_amount = Convert.ToDecimal(rd["cashless_fee_amount"]),
                        TotalAmount = Convert.ToDecimal(rd["TotalAmount"]),
                        status_billing = rd["status_billing"].ToString(),
                        PaymentSource = rd["PaymentSource"].ToString(),
                        IsHold = Convert.ToBoolean(Convert.ToInt16(rd["IsHold"])),
                        DateCrt = rd["DateCrt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["DateCrt"]),
                        LastUploadDate = rd["LastUploadDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["LastUploadDate"]),
                        cancel_date = rd["cancel_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["cancel_date"]),
                        paid_date = rd["paid_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["paid_date"])
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
                    FROM `billing` b
                    INNER JOIN `policy_billing` pb ON pb.`policy_Id`=b.`policy_id` " +
                    where + order + limit;

            return sql;
        }

        private string GetDataSelect()
        {
            string select = @"b.`BillingID`,
                            b.`policy_id`,
                            pb.`policy_no`,
                            pb.`payment_method`,
                            b.`recurring_seq`,
                            b.`BillingDate`,
                            b.`due_dt_pre`,
                            b.`policy_regular_premium`,
                            b.`cashless_fee_amount`,
                            b.`TotalAmount`,
                            b.`status_billing`,
                            b.`LastUploadDate`,
                            b.`cancel_date`,
                            b.`paid_date`,
                            b.`PaymentSource`,
                            COALESCE(NULLIF(b.`IsPending`,0),pb.`IsHoldBilling`) AS IsHold,
                            b.`DateCrt`";
            return select;
        }
    }
}
