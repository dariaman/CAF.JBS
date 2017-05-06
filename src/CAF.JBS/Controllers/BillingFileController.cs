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
using System.Net.Http;
using System.Text;

//using NPOI.SS.UserModel;
//using NPOI.HSSF.UserModel;
//using NPOI.XSSF.UserModel;
//using OfficeOpenXml;
//using System.Runtime.InteropServices;
using System.Diagnostics;
using OfficeOpenXml;



//using Npoi.Core.SS.UserModel;
//using Npoi.Core.SS.Util;
//using Npoi.Core.HSSF.UserModel;
//using Npoi.Core.HSSF.Util;
//using Excel = Microsoft.Office.Interop.Excel;
//using System.Runtime.InteropServices;


namespace CAF.JBS.Controllers
{
    public class BillingFileController : Controller
    {
        private readonly JbsDbContext _jbsDB;
        private readonly  string DirBilling;      //folder Billing yang standby hari ini
        private readonly string BackupFile;     //folder Backup billing hari2 sebelumnya
        private readonly string Template;       //folder template billing

        private readonly string BCAccFile;
        private readonly string MandiriccFile;
        private readonly string MegaOnUsccFile;
        private readonly string MegaOfUsccFile;
        private readonly string BNIccFile;

        private readonly string BCAacFile;
        private readonly string MandiriAcFile;

        private readonly string TempBniFile;
        private readonly string TempMandiriFile;
        private readonly string TempBCAacFile;
        public BillingFileController(JbsDbContext context1)
        {
            _jbsDB = context1;
            DirBilling = "./FileBilling/";
            BackupFile = "./BillingBackup/";
            Template = "./Template/";

            BCAccFile = "CAF" + DateTime.Now.ToString("ddMM") + ".prn";
            MandiriccFile = "Mandiri_" + DateTime.Now.ToString("ddMMyyyy") + ".xls";
            MegaOnUsccFile = "CAF"+DateTime.Now.ToString("yyyyMMdd")+"_MegaOnUs.bpmt";
            MegaOfUsccFile = "CAF" + DateTime.Now.ToString("yyyyMMdd") + "_MegaOffUs.bpmt";
            BNIccFile = "BNI_" + DateTime.Now.ToString("ddMMyyyy")  + ".xlsx";

            BCAacFile = "BCAacFile" + DateTime.Now.ToString("yyyyMMdd") + ".xls";
            MandiriAcFile = "MandiriAcFile" + DateTime.Now.ToString("yyyyMMdd") + ".csv";

            TempBniFile = "./Template/BniCC.xlsx";
            TempMandiriFile = "./Template/MandiriCC.xls";
            TempBCAacFile = "./Template/BcaAc.xls";

            // Move(backup) existing file BCA => dilakukan pada saat upload result
            // file tidak akan hilang jika data result tidak hilang
            //var files = Directory.GetFiles(TempFile);
            //foreach (string file in files)
            //{
            //    FileInfo FileName = new FileInfo(file);
            //    if ((FileName.ToString() == TempFile + BCAFile) ||      // File BCA
            //        (FileName.ToString() == TempFile + MandiriFile) ||  // File Mandiri
            //        (FileName.ToString() == TempFile + MegaOnUsFile) || // File MegaOnUs
            //        (FileName.ToString() == TempFile + MegaOfUsFile) || // File MegaOffUs
            //        (FileName.ToString() == TempFile + BNIFile)         // File BNI
            //        )
            //    { continue; }

            //    FileInfo filex = new FileInfo(BackupFile + FileName.Name);
            //    if (filex.Exists) System.IO.File.Delete(filex.ToString());
            //    FileName.MoveTo(BackupFile + FileName.Name);
            //}
        }

        [HttpGet]
        public ActionResult Index()
        {
            // cek file BCA CC
            string[] files = System.IO.Directory.GetFiles(DirBilling, "CAF*.prn", System.IO.SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                ViewBag.BCACC = new FileInfo(files[0]).Name.ToString();
            }

            // cek file Mandiri CC
            files = System.IO.Directory.GetFiles(DirBilling, "Mandiri_*.xls", System.IO.SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                ViewBag.MandiriCC = new FileInfo(files[0]).Name.ToString();
            }

            // cek file MegaOnUs CC
            files = System.IO.Directory.GetFiles(DirBilling, "CAF*_MegaOnUs.bpmt", System.IO.SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                ViewBag.MegaOnUs = new FileInfo(files[0]).Name.ToString();
            }

            // cek file MegaOffUs CC
            files = System.IO.Directory.GetFiles(DirBilling, "CAF*_MegaOffUs.bpmt", System.IO.SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                ViewBag.MegaOfUs = new FileInfo(files[0]).Name.ToString();
            }

            // cek file BNI CC
            files = System.IO.Directory.GetFiles(DirBilling, "BNI_*.xlsx", System.IO.SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                ViewBag.BNICC = new FileInfo(files[0]).Name.ToString();
            }

            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Download(ViewModels.DownloadBillingVM dw)
        {
            /*
             * kode bank utk sbg info di keterangan
             * 1. BCA
             * 2. Mandiri
             * 3. Mega
             * 4. Bank Lain
            */

            if (ModelState.IsValid) { /*return RedirectToAction("Index"); */ }
            // download file CC Billing
            if (dw.BcaCC || dw.MandiriCC || dw.MegaCC || dw.BniCC) {
                if (dw.BcaCC && !(dw.MandiriCC || dw.MegaCC || dw.BniCC))
                {   // BCA saja
                    GenBcaCCFile(0); // BCA 1 3 4
                }
                else if (dw.MandiriCC && !(dw.BcaCC || dw.MegaCC || dw.BniCC))
                {   // Mandiri saja
                    GenMandiriCCFile(); // Mandiri 2
                }
                else if (dw.MegaCC && !(dw.BcaCC || dw.MandiriCC || dw.BniCC))
                {   // Mega saja
                    GenMegaOnUsCCFile(); // MegaOn 3
                    GenMegaOffUsCCFile(0); // MegaOff 1 4
                }
                else if (dw.BniCC && !(dw.BcaCC || dw.MandiriCC || dw.MegaCC))
                {   // BNI aja
                    GenBniCCFile(0); // BNI 1 3 4
                }
                else if (dw.BcaCC && dw.MandiriCC && !(dw.MegaCC || dw.BniCC))
                {   // jika dipilih BCA dan Mandiri
                    GenBcaCCFile(0); // BCA 1 3 4
                    GenMandiriCCFile(); // Mandiri 2
                }
                else if (dw.BcaCC && dw.MegaCC && !(dw.MandiriCC || dw.BniCC))
                {   // jika dipilih BCA dan Mega
                    GenBcaCCFile(1); // BCA 1
                    GenMegaOnUsCCFile(); // MegaOn 3
                    GenMegaOffUsCCFile(1); // MegaOff 4
                }
                else if (dw.BcaCC && dw.BniCC && !(dw.MandiriCC || dw.MegaCC))
                {   // jika dipilih BCA dan BNI
                    GenBcaCCFile(1); // BCA 1
                    GenBniCCFile(1); // BNI 3 4
                }
                else if (dw.BniCC && !(dw.BcaCC || dw.MandiriCC || dw.MegaCC))
                {   // jika dipilih Mandiri dan BNI
                    GenMandiriCCFile(); // Mandiri 2
                    GenBniCCFile(0); // BNI 1 3 4
                }
                else if (dw.BcaCC && dw.MandiriCC && dw.MegaCC && !dw.BniCC)
                {   // jika dipilih BCA,Mandiri dan Mega
                    GenBcaCCFile(1); // BCA 1
                    GenMandiriCCFile(); // Mandiri 2
                    GenMegaOnUsCCFile(); // MegaOn 3
                    GenMegaOffUsCCFile(1); // MegaOff 4
                }
                else if (dw.BcaCC && dw.MandiriCC && dw.BniCC && !dw.MegaCC)
                {   // jika dipilih BCA,Mandiri dan BNI
                    GenBcaCCFile(1); // BCA 1
                    GenMandiriCCFile(); // Mandiri 2
                    GenBniCCFile(1); //BNI 3 4
                }
            }
            //GenBcaAcFile();
            return RedirectToAction("Index");
        }

        public FileStreamResult DownloadFile(string fileName)
        {
            Response.Headers.Add("content-disposition", "attachment; filename=" + fileName);
            return File(new FileStream(DirBilling + fileName, FileMode.Open),"application/octet-stream"); 
        }

        protected void GenBcaCCFile(int id)
        {
            /* id
             * 0 = All data
             * 1 = bca only
             */
            FileInfo FileName = new FileInfo(DirBilling + this.BCAccFile);
            //var files = Directory.GetFiles(TempFile).Where(s => s.EndsWith(".prn"));

            //foreach (string file in files) {
            //    if (FileName.ToString() == file) { continue; }
            //    System.IO.File.Delete(file);
            //}

            if (!FileName.Exists) //jika file belum ada akan di generate tp jika sudah ada maka akan pake file exist
            {
                //System.IO.File.Delete(FileName.ToString());                
                try
                {
                    var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "BillingBCAcc_sp ";
                    cmd.Parameters.Add(new MySqlParameter("@BankCode", MySqlDbType.Int16) { Value = id });
                    cmd.Connection.Open();
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
                    cmd.Connection.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        protected void GenMandiriCCFile()
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("taskkill", "/F /T /IM conhost.exe")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                Process.Start(processStartInfo);
            }
            catch (Exception ex) { throw ex; }

            try
            {
                ProcessStartInfo p = new ProcessStartInfo(@"./GenFile/JBSGenExcel.exe", @"mandiricc /c");
                var process = new Process();
                process.StartInfo.FileName = @"./GenFile/JBSGenExcel.exe ";
                process.StartInfo.Arguments = @" mandiricc /c";

                process.EnableRaisingEvents = true;

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();
                process.WaitForExit();

            }
            catch (Exception ex) { throw ex; }
        }

        protected void GenMegaOnUsCCFile()
        {
            FileInfo FileName = new FileInfo(DirBilling + this.MegaOnUsccFile);
            if (!FileName.Exists) //jika file belum ada akan di generate tp jika sudah ada maka akan pake file exist
            {
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "BillingMegaOnUsCC_sp ";
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
                                    writer.WriteLine();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    cmd.Dispose();
                    cmd.Connection.Close();
                }
            }

        }

        protected void GenMegaOffUsCCFile(int id)
        {
            FileInfo FileName = new FileInfo(DirBilling + this.MegaOfUsccFile);
            if (!FileName.Exists) //jika file belum ada akan di generate tp jika sudah ada maka akan pake file exist
            {
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "BillingMegaOffUsCC_sp ";
                cmd.Parameters.Add(new MySqlParameter("@BankCode", MySqlDbType.Int16) { Value = id });
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
                                    writer.WriteLine();
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    cmd.Dispose();
                    cmd.Connection.Close();
                }
            }
        }

        protected void GenBniCCFile(int id)
        {
            FileInfo FileName = new FileInfo(DirBilling + this.BNIccFile);
            if (!FileName.Exists)
            {
                FileName = new FileInfo(TempBniFile);
                FileName.CopyTo(DirBilling + this.BNIccFile);
                FileName = new FileInfo(DirBilling + this.BNIccFile);

                using (ExcelPackage package = new ExcelPackage(new FileInfo(DirBilling + this.BNIccFile)))
                {
                    ExcelWorkbook workBook = package.Workbook;
                    ExcelWorksheet ws = workBook.Worksheets.SingleOrDefault(w => w.Name == "sheet1");
                    var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "BillingBNIcc_sp ";
                    cmd.Parameters.Add(new MySqlParameter("@BankCode", MySqlDbType.Int16) { Value = id });
                    cmd.Connection.Open();
                    try
                    {
                        using (var result = cmd.ExecuteReader())
                        {
                            var i = 2;
                            while (result.Read())
                            {
                                ws.Cells[i, 1].Value = result["a"];
                                ws.Cells[i, 2].Value = result["b"];
                                ws.Cells[i, 3].Value = result["c"];
                                ws.Cells[i, 4].Value = result["d"];
                                ws.Cells[i, 5].Value = result["e"];
                                ws.Cells[i, 6].Value = result["f"];
                                ws.Cells[i, 7].Value = result["g"];
                                ws.Cells[i, 8].Value = result["h"];
                                ws.Cells[i, 9].Value = result["i"];
                                ws.Cells[i, 10].Value = result["j"];
                                i++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if(cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Close();
                    }
                    package.Save();
                }
            }
        }

        protected void GenBcaAcFile()
        {

        }

    }
}