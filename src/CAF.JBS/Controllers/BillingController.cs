using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CAF.JBS.Data;
using CAF.JBS.Models;
using CAF.JBS.ViewModels;
using System.IO;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using System.Data;
using MySql.Data.MySqlClient;

namespace CAF.JBS.Controllers
{
    public class BillingController : Controller
    {
        private readonly JbsDbContext _jbsDB;
        private readonly  string TempFile;
        public BillingController(JbsDbContext context1)
        {
            _jbsDB = context1;
            TempFile = "./FileBilling/";
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Download(ViewModels.DownloadBillingVM dw)
        {
            if (ModelState.IsValid) { /*return RedirectToAction("Index"); */ }
            // download file CC Billing
            if (dw.BcaCC || dw.MandiriCC || dw.MegaCC || dw.BniCC) {
                if (dw.BcaCC && !(dw.MandiriCC || dw.MegaCC || dw.BniCC))
                {   // jika dipilih BCA saja
                    // semua data dikeluarkan dgn format BCA
                    BcaCCFile(0); // BCA semua
                }
                else if (dw.BcaCC && dw.MandiriCC && !(dw.MegaCC || dw.BniCC))
                {   // jika dipilih BCA dan Mandiri
                    // semua data kecuali mandiri dikeluarkan format BCA, dan Mandiri data sendiri
                    BcaCCFile(2); // BCA semua kecuali mandiri
                }
                else if (dw.BcaCC && dw.MandiriCC && dw.MegaCC && !dw.BniCC)
                {   // jika dipilih BCA,Mandiri dan Mega
                    // BCA data sendiri, Mandiri data sendiri, dan Selebihnya Mega Off Us
                    BcaCCFile(1); // BCA sendiri
                }
                else if (dw.BcaCC && dw.MandiriCC && dw.BniCC && !dw.MegaCC)
                {   // jika dipilih BCA,Mandiri dan BNI
                    // BCA data sendiri, Mandiri data sendiri, dan Selebihnya BNI
                    BcaCCFile(1); // BCA sendiri
                }
                else if (dw.BcaCC && dw.BniCC&& !(dw.MandiriCC || dw.MegaCC))
                {   // jika dipilih BCA dan BNI
                    // BCA data sendiri, dan Selebihnya BNI
                    BcaCCFile(1); // BCA sendiri
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async void DownloadFile(string source,string filename)
        {
            Response.Headers.Add("content-disposition", "attachment; filename=" + filename);
            byte[] arr = System.IO.File.ReadAllBytes(source);
            await Response.Body.WriteAsync(arr, 0, arr.Length);
        }

        public async void GenerateFile(string filename)
        {
            Response.Headers.Add("content-disposition", "attachment; filename=" + filename);
            //byte[] arr = System.IO.File.ReadAllBytes(source);
            //await Response.Body.WriteAsync(arr, 0, arr.Length);
        }

        protected void BcaCCFile(int id)
        {

            /* id
             * 1 = bca only
             */
            FileInfo FileName = new FileInfo("CAF" + DateTime.Now.ToString("ddMM") + ".prn");
            var files = Directory.GetFiles(".").Where(s => s.EndsWith(".prn"));

            foreach (string file in files) {
                if (FileName.ToString() == file) { continue; }
                System.IO.File.Delete(file);
            }
            
            if (FileName.Exists) { System.IO.File.Delete(FileName.ToString()); }
                

            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GenerateBillingBCA_sp ";
            cmd.Parameters.Add(new MySqlParameter("@BankCode", MySqlDbType.Int16) { Value = 0});
            cmd.Connection.Open();

            //await cmd.ExecuteNonQueryAsync();
            try
            {
                using (var result = cmd.ExecuteReader())
                {
                    using (FileStream fs = new FileStream(FileName.ToString(), FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(fs))
                        {
                            while (result.Read())
                            {
                                writer.Write(result["a"]);
                                writer.Write(result["b"]);
                                writer.Write(result["c"]);
                                writer.Write(result["d"]);
                                writer.Write(result["e"]);
                                writer.Write(result["f"]);
                                writer.Write(result["g"]);
                                writer.Write(result["h"]);
                                writer.Write(result["i"]);
                                writer.Write(result["j"]);
                                writer.Write(result["k"]);
                                writer.Write(result["l"]);
                                writer.WriteLine();
                            }
                        }
                    }
                }
            }
            finally
            {
                cmd.Connection.Close();
            }

            //DbCommand cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            //List<dummyDownload> dd = _jbsDB.dummyDownload
            //        .FromSql("GenerateBillingBCA_sp").ToList();
            //return dd;

        }

        protected void MandiriCCFile(int id)
        {

        }

        protected void MegaOnUsCCFile(int id)
        {

        }

        protected void MegaOffUsCCFile(int id)
        {

        }

        protected void BniCCFile(int id)
        {

        }
    }
}