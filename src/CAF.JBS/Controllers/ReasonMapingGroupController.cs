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
using System.IO;
using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace CAF.JBS.Controllers
{
    public class ReasonMapingGroupController : Controller
    {
        private readonly JbsDbContext _context;

        public ReasonMapingGroupController(JbsDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<UploadResultIndexVM> StagingUploadx = new List<UploadResultIndexVM>();

            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"SELECT fp.`id`,fp.`deskripsi`,fp.`FileName`,fp.`tglProses`,bs.`BillingCountDWD`
                                FROM `FileNextProcess` fp
                                LEFT JOIN `billing_download_summary` bs ON bs.`id`= fp.`id_billing_download`; ";
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    StagingUploadx.Add(new UploadResultIndexVM()
                    {
                        id = Convert.ToInt32(rd["id"]),
                        deskripsi= rd["deskripsi"].ToString(),
                        FileName = rd["FileName"].ToString(),
                        tglProses = (rd["tglProses"] == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(rd["tglProses"]),
                        billCountDwd = Convert.ToInt32(rd["BillingCountDWD"])
                    });

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

            return View(StagingUploadx);
        }
        
    }
}
