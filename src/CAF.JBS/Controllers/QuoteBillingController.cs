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
using System.Text.RegularExpressions;
using DataTables.AspNet.Core;
using DataTables.AspNet.AspNetCore;

namespace CAF.JBS.Controllers
{
    public class QuoteBillingController : Controller
    {
        private readonly JbsDbContext _context;

        public QuoteBillingController(JbsDbContext context)
        {
            _context = context;
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

            List<QuoteBillingVM> BillingOthers = new List<QuoteBillingVM>();
            BillingOthers = GetPageData(request.Start, request.Length, sort, sqlFilter, ref jlhFilter, ref jlh);

            var filteredData = BillingOthers;

            var response = DataTablesResponse.Create(request, jlh, jlhFilter, filteredData);

            return new DataTablesJsonResult(response);
        }

        private string GenerateFilter(IDataTablesRequest request, ref string sort)
        {
            string FilterSql = "";
            DateTime tgl = DateTime.Now.Date;

            string paternAngka = @"[^0-9,%]";
            //string paternAngkaHuruf = @"[^0-9a-zA-Z,%]";
            string paternHuruf = @"[^a-zA-Z,%]";

            int i = 0;
            foreach (var req in request.Columns)
            {
                i++;
                if (req.Sort != null) sort = string.Format(" {0} {1} ", i, req.Sort.Direction.ToString().ToLower() == "ascending" ? "ASC" : "DESC");

                if (req.Search == null) continue;
                if (req.Search.Value == null) continue;

                if (req.Field == "quote_id" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND q.`quote_id` like '" + tmp + "'";

                }
                else if (req.Field == "ref_no" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND q.`ref_no` like '" + tmp + "'";
                }
                else if (req.Field == "policy_id" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND q.`policy_id` like '" + tmp + "'";
                }
                else if (req.Field == "policy_no" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pb.`policy_no` like '" + tmp + "'";
                }
                else if (req.Field == "holder_Name" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternHuruf, "");
                    FilterSql += " AND q.`Holder_Name` like '" + tmp + "'";
                }
                else if (req.Field == "status" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, "[^a-zA-Z]", "");
                    FilterSql += " AND q.`status`='" + tmp + "'";
                }
                else if (req.Field == "lastUploadDate" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    if (DateTime.TryParse(req.Search.Value, out tgl))
                    {
                        FilterSql += " AND q.`LastUploadDate` >= '" + tgl.ToString("yyyy-MM-dd") + "'";
                        FilterSql += " AND q.`LastUploadDate` <  '" + tgl.AddDays(1).ToString("yyyy-MM-dd") + "'";
                    }
                }
            }

            return FilterSql;
        }

        private List<QuoteBillingVM> GetPageData(int rowStart, int limitData, string orderString, string FilterWhere, ref int jlhdataFilter, ref int jlhData)
        {
            FilterWhere = string.Concat(" WHERE 1=1 ", FilterWhere);
            string order = (orderString == "" ? "" : string.Format(" ORDER BY {0} ", orderString));
            string limit = string.Format(" LIMIT {0},{1} ", rowStart, limitData);
            QuoteBillingVM dt = new QuoteBillingVM();
            List<QuoteBillingVM> ls = new List<QuoteBillingVM>();

            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = QueryPaging(GetDataSelect(), FilterWhere, order, limit);

            try
            {
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    ls.Add(new QuoteBillingVM()
                    {
                        quote_id = rd["quote_id"].ToString(),
                        ref_no = rd["ref_no"].ToString(),
                        policy_id = rd["policy_Id"].ToString(),
                        policy_no= rd["policy_no"].ToString(),
                        Holder_Name = rd["Holder_Name"].ToString(),
                        prospect_amount = Convert.ToDecimal(rd["prospect_amount"]),
                        paper_print_fee = Convert.ToDecimal(rd["paper_print_fee"]),
                        cashless_fee = Convert.ToDecimal(rd["cashless_fee"]),
                        TotalAmount = Convert.ToDecimal(rd["TotalAmount"]),
                        status= rd["status"].ToString(),

                        LastUploadDate = (rd["LastUploadDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["LastUploadDate"])),
                        cancel_date = (rd["cancel_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["cancel_date"])),
                        paid_dt = (rd["paid_dt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["paid_dt"])),
                        DateCrt = (rd["DateCrt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["DateCrt"])),

                        acc_no= rd["acc_no"].ToString(),
                        acc_name= rd["acc_name"].ToString(),
                        cc_expiry= rd["cc_expiry"].ToString(),
                        bank_code= rd["bank_code"].ToString(),
                        ApprovalCode = rd["ApprovalCode"].ToString(),
                        Description = rd["Description"].ToString(),
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
                    FROM `quote_billing` q
                    LEFT JOIN `policy_billing` pb ON pb.`policy_Id`=q.`policy_id`
                    LEFT JOIN bank b ON b.`bank_id`=q.`acc_bankid` 
                    LEFT JOIN `transaction_bank` tb ON q.`PaymentTransactionID`=tb.`id` " +
                    where + order + limit;

            return sql;
        }

        private string GetDataSelect()
        {
            string select = @"q.`quote_id`,
                            q.`ref_no`,
                            q.`policy_id`,
                            pb.`policy_no`,
                            q.`Holder_Name`,
                            q.`prospect_amount`,
                            q.`paper_print_fee`,
                            q.`cashless_fee`,
                            q.`TotalAmount`,
                            q.`status`,
                            q.`LastUploadDate`,
                            q.`cancel_date`,
                            q.`paid_dt`,
                            q.`DateCrt`,
                            q.`acc_no`,
                            q.`acc_name`,
                            q.`cc_expiry`,
                            b.`bank_code`,tb.`ApprovalCode`,tb.`Description` ";
            return select;
        }
    }
}
