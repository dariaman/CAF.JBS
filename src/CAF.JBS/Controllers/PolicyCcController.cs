using CAF.JBS.Data;
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

namespace CAF.JBS.Controllers
{
    public class PolicyCcController : Controller
    {
        private readonly JbsDbContext _context;

        public PolicyCcController(JbsDbContext context)
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

            List<PolicyCcVM> polisCC = new List<PolicyCcVM>();
            polisCC = GetPageData(request.Start, request.Length, sort, sqlFilter, ref jlhFilter, ref jlh);
            var response = DataTablesResponse.Create(request, jlh, jlhFilter, polisCC);

            return new DataTablesJsonResult(response);
        }

        private string GenerateFilter(IDataTablesRequest request, ref string sort)
        {
            string FilterSql = "";
            DateTime tgl = DateTime.Now.Date;

            string paternAngka = @"[^0-9,%]";

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
                    FilterSql += " AND pc.`PolicyId` like '" + tmp + "'";
                }
                else if (req.Field == "policy_no" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pb.`policy_no` like '" + tmp + "'";
                }
                else if (req.Field == "cc_no" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pc.`cc_no` like '" + tmp + "'";
                }
                else if (req.Field == "cc_name" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value.Trim(), "[^a-zA-Z %]", "");
                    FilterSql += " AND pc.`cc_name` like '" + tmp + "'";
                }
                else if (req.Field == "bank_code" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value.Trim(), "[^a-zA-Z %]", "");
                    FilterSql += " AND b.`bank_code` like '" + tmp + "'";
                }
                else if (req.Field == "cc_expiry" && !string.IsNullOrEmpty(req.Search.Value))
                {
                    var tmp = Regex.Replace(req.Search.Value, paternAngka, "");
                    FilterSql += " AND pc.`cc_expiry` like '" + tmp + "'";
                }
            }

            return FilterSql;
        }

        private List<PolicyCcVM> GetPageData(int rowStart, int limitData, string orderString, string FilterWhere, ref int jlhdataFilter, ref int jlhData)
        {
            FilterWhere = string.Concat(" WHERE 1=1 ", FilterWhere);
            string order = (orderString == "" ? "" : string.Format(" ORDER BY {0} ", orderString));
            string limit = string.Format(" LIMIT {0},{1} ", rowStart, limitData);
            PolicyCcVM dt = new PolicyCcVM();
            List<PolicyCcVM> ls = new List<PolicyCcVM>();

            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = QueryPaging(GetDataSelect(), FilterWhere, order, limit);

            try
            {
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    ls.Add(new PolicyCcVM()
                    {
                        PolicyId = rd["PolicyId"].ToString(),
                        policy_no = rd["policy_no"].ToString(),
                        cc_no = rd["cc_no"].ToString(),
                        cc_name = rd["cc_name"].ToString(),
                        cc_expiry = rd["cc_expiry"].ToString(),
                        bank_code = rd["bank_code"].ToString(),
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
                    FROM `policy_cc` pc
                    LEFT JOIN `policy_billing` pb ON pb.`policy_Id`=pc.`PolicyId`
                    LEFT JOIN `bank` b ON b.`bank_id`=pc.`bank_id` " +
                    where + order + limit;

            return sql;
        }

        private string GetDataSelect()
        {
            string select = @"pc.`PolicyId`,
                            pb.`policy_no`,
                            pc.`cc_no`,
                            pc.`cc_name`,
                            pc.`cc_expiry`,
                            b.`bank_code`,
                            pc.`DateCrt`,
                            pc.`DateUpdate`";
            return select;
        }

    }
}
