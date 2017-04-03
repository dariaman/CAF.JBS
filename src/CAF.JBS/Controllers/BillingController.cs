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

        private readonly string BCAFile;
        private readonly string Mandiriile;
        private readonly string MegaFile;
        private readonly string BNIFile;
        private List<string> DownloadFile;
        public BillingController(JbsDbContext context1)
        {
            _jbsDB = context1;
            TempFile = "./FileBilling/";

            BCAFile = "CAF" + DateTime.Now.ToString("ddMM") + ".prn";
            Mandiriile = "";
            MegaFile = "";
            BNIFile = "";
            DownloadFile = new List<string>();
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

            foreach (var filex in this.DownloadFile)
            {
                FileInfo FileName = new FileInfo(filex);
                if (FileName.Exists) {
                    Download(FileName.ToString());
                    //Download2(FileName.ToString());
                }
            }

            return RedirectToAction("Index");
        }

        public async void Download(string fileName)
        {
            ActionContext context = new ActionContext();
            var filepath = $"{fileName}";
            byte[] fileBytes = System.IO.File.ReadAllBytes(filepath);
            //await Response.Body.WriteAsync(fileBytes, 0, fileBytes.Length);
            using (var fileStream = new FileStream(filepath, FileMode.Open))
            {
                await fileStream.CopyToAsync(context.HttpContext.Response.Body);
            }
            //return File(fileBytes, "application/x-msdownload", fileName);
        }

        public FileStreamResult Download2(string fileName)
        {
            Response.Headers.Add("content-disposition", "attachment; filename=" + fileName);
            return File(new FileStream(fileName, FileMode.Open),"application/octet-stream"); 
        }

        protected void BcaCCFile(int id)
        {

            /* id
             * 0 = All data
             * 1 = bca only
             */
            FileInfo FileName = new FileInfo("./tempFile/" + this.BCAFile);
            var files = Directory.GetFiles("./tempFile/").Where(s => s.EndsWith(".prn"));

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
                        this.DownloadFile.Add(FileName.ToString());
                    }
                }
            }
            finally
            {
                cmd.Connection.Close();
            }
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