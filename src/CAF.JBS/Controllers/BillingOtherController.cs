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
using DataTables.AspNet.Core;
using System.Text.RegularExpressions;

namespace CAF.JBS.Controllers
{
    public class BillingOtherController : Controller
    {
        private readonly JbsDbContext _context;

        public BillingOtherController(JbsDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult PageData(IDataTablesRequest request)
        {
            int jlh=0,jlhFilter=0;
            string sort="";
            var sqlFilter = GenerateFilter(request,ref sort);

            List<BillingOthersVM> BillingOthers = new List<BillingOthersVM>();
            BillingOthers = GetPageData(request.Start, request.Length, sort, sqlFilter,ref jlhFilter,ref jlh);
            var response = DataTablesResponse.Create(request, jlh, jlhFilter, BillingOthers);

            return new DataTablesJsonResult(response);
        }

        private string GenerateFilter(IDataTablesRequest request,ref string sort)
        {
            string FilterSql = "";
            DateTime tgl = DateTime.Now.Date;

            string paternAngka = @"[^0-9,%]";
            string paternAngkaHuruf = @"[^0-9a-zA-Z,%]";

            int i = 0;
            foreach (var req in request.Columns)
            {
                i++;
                if (req.Sort != null)sort = string.Format(" {0} {1} ",i, req.Sort.Direction.ToString().ToLower() == "ascending" ? "ASC" : "DESC");

                if (req.Search == null) continue;
                if (req.Search.Value == null) continue;

                if(req.Field == "billingID" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngkaHuruf, "");
                    FilterSql += " AND b.`BillingID` like '" + tmp  + "'";

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
                else if (req.Field == "billingType" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngkaHuruf, "");
                    FilterSql += " AND b.`BillingType`='" + tmp + "'";
                }
                else if (req.Field == "status_billing" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, "[^a-zA-Z]", "");
                    FilterSql += " AND b.`status_billing`='" + tmp + "'";
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

        private List<BillingOthersVM> GetPageData(int rowStart, int limitData, string orderString, string FilterWhere,ref int jlhdataFilter, ref int jlhData)
        {
            FilterWhere = string.Concat(" WHERE 1=1 ", FilterWhere);
            string order = (orderString=="" ? "" : string.Format(" ORDER BY {0} " , orderString));
            string limit = string.Format(" LIMIT {0},{1} ", rowStart, limitData);
            BillingOthersVM dt = new BillingOthersVM();
            List<BillingOthersVM> ls = new List<BillingOthersVM>();

            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = QueryPaging(GetDataSelect(), FilterWhere,order,limit);

            try
            {
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    ls.Add(new BillingOthersVM()
                    {
                        BillingID = rd["BillingID"].ToString(),
                        policy_id = Convert.ToInt32(rd["policy_Id"]),
                        PolicyNo = rd["policy_no"].ToString(),
                        BillingDate = rd["BillingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["BillingDate"]),
                        BillingType = rd["BillingType"].ToString() =="A2" ? "Cetak Polis" : (rd["BillingType"].ToString() == "A3" ? "Cetak Kartu" : (rd["BillingType"].ToString() == "A1" ? "Cashless Fee" : "-")),
                        TotalAmount = Convert.ToDecimal(rd["TotalAmount"]),
                        status_billing = rd["status_billing"].ToString(),
                        //IsDownload = Convert.ToBoolean(rd["IsDownload"]),
                        //BankIdDownload = rd["BankIdDownload"] == DBNull.Value ? (Int32?)null : Convert.ToInt32(rd["BankIdDownload"]),
                        //BankDownload = rd["bank_code"].ToString(),
                        DateCrt = rd["DateCrt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["DateCrt"]),
                        LastUploadDate = rd["LastUploadDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["LastUploadDate"]),
                        cancel_date = rd["cancel_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["cancel_date"]),
                        paid_date = rd["paid_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["paid_date"]),

                        ApprovalCode = rd["ApprovalCode"].ToString(),
                        deskripsi_reject = rd["deskripsi_reject"].ToString(),
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
            sql = @"SELECT "+ SelectData + @"
                    FROM `billing_others` b
                    INNER JOIN `policy_billing` pb ON pb.`policy_Id`=b.`policy_id`
                    LEFT JOIN `bank` bk ON bk.`bank_id`=b.`BankIdDownload` 
                    LEFT JOIN `transaction_bank` tb ON b.`PaymentTransactionID`=tb.id " + 
                    where + order + limit;

            return sql;
        }

        private string GetDataSelect()
        {
            string select = @"b.`BillingID`,
	                    pb.`policy_Id`,
	                    pb.`policy_no`,
	                    b.`BillingDate`,
	                    b.`BillingType`,
	                    b.`TotalAmount`,
	                    b.`status_billing`,
	                    b.`DateCrt`,
	                    b.`LastUploadDate`,
	                    b.`cancel_date`,
	                    b.`paid_date`,tb.`ApprovalCode`,tb.`Description` AS deskripsi_reject ";
            return select;
        }
    }
}
