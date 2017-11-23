﻿using CAF.JBS.Data;
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
    public class PolicyAcController : Controller
    {
        private readonly JbsDbContext _context;

        public PolicyAcController(JbsDbContext context)
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
                            pa.`cycleDate`,
                            pa.`IsSKDR`,
                            pa.`DateCrt`,
                            pa.`DateUpdate`";
            return select;
        }
    }
}
