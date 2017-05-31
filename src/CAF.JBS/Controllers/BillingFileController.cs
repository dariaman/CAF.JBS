using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CAF.JBS.Data;
using CAF.JBS.Models;
using CAF.JBS.ViewModels;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using OfficeOpenXml;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace CAF.JBS.Controllers
{
    public class BillingFileController : Controller
    {
        private readonly JbsDbContext _jbsDB;
        private readonly Life21DbContext _life21;
        private readonly UserDbContext _user;
        private readonly string DirBilling;     //folder Billing yang standby hari ini
        private readonly string BackupFile;     //folder Backup billing hari2 sebelumnya
        private readonly string BackupResult;   //folder Backup File Result dari Bank
        private readonly string Template;       //folder template billing
        private readonly string DirResult;      //Folder Result tmp

        private readonly string BCAccFile;
        private readonly string MandiriccFile;
        private readonly string MegaOnUsccFile;
        private readonly string MegaOfUsccFile;
        private readonly string BNIccFile;

        private readonly string BCAacFile;
        private readonly string MandiriAcFile;
        private readonly string VaRegulerPremi;

        private readonly string TempBniFile;
        private readonly string TempMandiriFile;
        private readonly string TempBCAacFile;

        private readonly string GenerateXls;

        private FileSettings filesettings { get; set; }
        //private IConfigurationRoot Configuration { get; set; }

        public BillingFileController(JbsDbContext context1, Life21DbContext context2, UserDbContext context3)
        {
            filesettings = new FileSettings();
            _jbsDB = context1;
            _life21 = context2;
            _user= context3;

            GenerateXls = filesettings.GenFileXls;
            BackupResult = filesettings.BackupResult;
            DirResult = filesettings.Result;

            DirBilling = filesettings.FileBilling;
            BackupFile = filesettings.BackupBilling;
            Template = filesettings.Template;

            BCAccFile = filesettings.BCAcc;
            MandiriccFile = filesettings.MandiriCC;
            MegaOnUsccFile = filesettings.MegaonUsCC;
            MegaOfUsccFile = filesettings.MegaOffUsCC;
            BNIccFile = filesettings.BNIcc;

            BCAacFile = filesettings.BCAac;
            MandiriAcFile = filesettings.MandiriAC;

            VaRegulerPremi = filesettings.BCAva;

            TempBniFile = filesettings.TempBNIcc;
            TempMandiriFile = filesettings.TempMandiriCC;
            TempBCAacFile = filesettings.TempBCAac;

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
            DownloadBillingVM DownloadBillVM = new DownloadBillingVM();
            DownloadBillVM.BillingSummary =(from cd in _jbsDB.BillingSummary
                                            select new BillingSummary()
                                            {
                                                id=cd.id,
                                                Judul=cd.Judul,
                                                rowCountDownload=cd.rowCountDownload,
                                                AmountDownload=cd.AmountDownload
                                            }).ToList();
            
            // cek file BCA CC
            string[] files = Directory.GetFiles(DirBilling, "CAF*.prn", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                ViewBag.BCACC = new FileInfo(files[0]).Name.ToString();
            }

            // cek file Mandiri CC
            files = Directory.GetFiles(DirBilling, "Mandiri_*.xls", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                ViewBag.MandiriCC = new FileInfo(files[0]).Name.ToString();
            }

            // cek file MegaOnUs CC
            files = Directory.GetFiles(DirBilling, "CAF*_MegaOnUs.bpmt", SearchOption.TopDirectoryOnly);
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
            files = Directory.GetFiles(DirBilling, "BNI_*.xlsx", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                ViewBag.BNICC = new FileInfo(files[0]).Name.ToString();
            }

            // cek file BCA AC
            files = Directory.GetFiles(DirBilling, "BCAac*.xls", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                ViewBag.BcaAC = new FileInfo(files[0]).Name.ToString();
            }

            // cek file Mandiri AC
            files = Directory.GetFiles(DirBilling, "MandiriAc*.csv", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                ViewBag.MandiriAC = new FileInfo(files[0]).Name.ToString();
            }

            // cek file VA Reguler Premi
            files = Directory.GetFiles(DirBilling, "VARegulerPremi*.xls", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                ViewBag.VA = new FileInfo(files[0]).Name.ToString();
            }

            return View(DownloadBillVM);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Download(ViewModels.DownloadBillingVM dw)
        {
            /*
             * kode bank sbg info di keterangan
             * 1. BCA
             * 2. Mandiri
             * 3. Mega
             * 4. Bank Lain
            */

            //if (ModelState.IsValid) { /*return RedirectToAction("Index"); */ }
            // download file CC Billing
            if (dw.BcaCC || dw.MandiriCC || dw.MegaCC || dw.BniCC) {
                if (dw.BcaCC && !(dw.MandiriCC || dw.MegaCC || dw.BniCC))
                {   // BCA saja
                    GenBcaCCFile(0); // BCA 1 2 3 4
                }
                else if (dw.MandiriCC && !(dw.BcaCC || dw.MegaCC || dw.BniCC))
                {   // Mandiri saja
                    GenMandiriCCFile(); // Mandiri 2
                }
                else if (dw.MegaCC && !(dw.BcaCC || dw.MandiriCC || dw.BniCC))
                {   // Mega saja
                    GenMegaOnUsCCFile(); // MegaOn 3
                    GenMegaOffUsCCFile(0); // MegaOff 1 2 4 (ALL <>3)
                }
                else if (dw.BniCC && !(dw.BcaCC || dw.MandiriCC || dw.MegaCC))
                {   // BNI aja
                    GenBniCCFile(0); // BNI 1 2 3 4
                }
                else if (dw.BcaCC && dw.MandiriCC && !(dw.MegaCC || dw.BniCC))
                {   // jika dipilih BCA dan Mandiri
                    GenBcaCCFile(2); // BCA 1 3 4 (<> 2)
                    GenMandiriCCFile(); // Mandiri 2
                }
                else if (dw.BcaCC && dw.MegaCC && !(dw.MandiriCC || dw.BniCC))
                {   // jika dipilih BCA dan Mega
                    GenBcaCCFile(1); // BCA 1 
                    GenMegaOnUsCCFile(); // MegaOn 3
                    GenMegaOffUsCCFile(1); // MegaOff 2 4 (<> 1 3)
                }
                else if (dw.MandiriCC && dw.MegaCC && !(dw.BcaCC || dw.BniCC))
                {   // jika dipilih Mandiri dan Mega
                    GenMandiriCCFile(); // Mandiri 2
                    GenMegaOnUsCCFile(); // MegaOn 3
                    GenMegaOffUsCCFile(3); // MegaOff 1 4 (<> 2 3)
                }
                else if (dw.BcaCC && dw.BniCC && !(dw.MandiriCC || dw.MegaCC))
                {   // jika dipilih BCA dan BNI
                    GenBcaCCFile(1); // BCA 1
                    GenBniCCFile(1); // BNI 2 3 4 (<> 1)
                }
                else if (dw.MandiriCC && dw.BniCC && !(dw.BcaCC || dw.MegaCC))
                {   // jika dipilih Mandiri dan BNI
                    GenMandiriCCFile(); // Mandiri 2
                    GenBniCCFile(2); // BNI 1 3 4 (<> 2)
                }
                else if (dw.BcaCC && dw.MandiriCC && dw.MegaCC && !dw.BniCC)
                {   // jika dipilih BCA,Mandiri dan Mega
                    GenBcaCCFile(1); // BCA 1
                    GenMandiriCCFile(); // Mandiri 2
                    GenMegaOnUsCCFile(); // MegaOn 3
                    GenMegaOffUsCCFile(2); // MegaOff 4 (<> 1,2,3)
                }
                else if (dw.BcaCC && dw.MandiriCC && dw.BniCC && !dw.MegaCC)
                {   // jika dipilih BCA,Mandiri dan BNI
                    GenBcaCCFile(1); // BCA 1
                    GenMandiriCCFile(); // Mandiri 2
                    GenBniCCFile(3); //BNI 3 4 (<> 1,2)
                }
            }
            if (dw.MandiriAC) GenMandiriAcFile();
            if (dw.BcaAC) GenBcaAcFile();
            if (dw.BcaRegularPremium) GenVA();

            if (dw.BcaCC || dw.MandiriCC || dw.MegaCC || dw.BniCC || dw.BcaAC || dw.MandiriAC)
            { // Jika ada aktifitas generate file tuk siap di download
                hitungUlang();

                // Validasi Data Kosong, agar File yg terbentuk dgn data kosong dihapus
                string validasi = "";
                List<string> errorstate = new List<string>();

                if (dw.BcaCC) // Cek Bca CC
                {
                    validasi = CekDataDownload(1, "CC");
                    if (validasi != "")
                    {
                        FileInfo filex = new FileInfo(BCAccFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        errorstate.Add(validasi);
                    }
                }

                if (dw.MandiriCC) // Cek mandiri CC
                {
                    validasi = "";
                    validasi = CekDataDownload(2, "CC");
                    if (validasi != "")
                    {
                        FileInfo filex = new FileInfo(MandiriccFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        errorstate.Add(validasi);
                    }
                }

                if (dw.MegaCC) // Cek Mega CC
                {
                    validasi = "";
                    validasi = CekDataDownload(3, "CC");
                    if (validasi != "")
                    {
                        FileInfo filex = new FileInfo(MegaOnUsccFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        errorstate.Add(validasi);
                    }

                    validasi = "";
                    validasi = CekDataDownload(4, "CC");
                    if (validasi != "")
                    {
                        FileInfo filex = new FileInfo(MegaOfUsccFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        errorstate.Add(validasi);
                    }
                }

                if (dw.BniCC) // Cek BNI CC
                {
                    validasi = "";
                    validasi = CekDataDownload(5, "CC");
                    if (validasi != "")
                    {
                        FileInfo filex = new FileInfo(BNIccFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        errorstate.Add(validasi);
                    }
                }
                if (dw.BcaAC) // Cek BNI CC
                {
                    validasi = "";
                    validasi = CekDataDownload(1, "AC");
                    if (validasi != "")
                    {
                        FileInfo filex = new FileInfo(BCAacFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        errorstate.Add(validasi);
                    }
                }
                if (dw.MandiriAC) // Cek BNI CC
                {
                    validasi = "";
                    validasi = CekDataDownload(2, "AC");
                    if (validasi != "")
                    {
                        FileInfo filex = new FileInfo(MandiriAcFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        errorstate.Add(validasi);
                    }
                }

                if (errorstate.Count > 0) TempData["ModelState"] = errorstate;
                // End Validasi Data download
            }

            return RedirectToAction("Index");
        }

        public FileStreamResult DownloadFile(string fileName)
        {
            Response.Headers.Add("content-disposition", "attachment; filename=" + fileName);
            return File(new FileStream(DirBilling + fileName, FileMode.Open),"application/octet-stream"); 
        }

        protected void GenBcaCCFile(int id)
        {
            FileInfo FileName = new FileInfo(this.BCAccFile);

            if (!FileName.Exists) //jika file belum ada akan di generate tp jika sudah ada maka akan pake file exist
            {
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "BillingBCAcc_sp ";
                cmd.Parameters.Add(new MySqlParameter("@BankCode", MySqlDbType.Int16) { Value = id });
                try
                {
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

        protected void GenMandiriCCFile()
        {
            foreach (Process proc in Process.GetProcessesByName("JBSGenExcel")) { proc.Kill(); }

            try
            {
                var process = new Process();
                process.StartInfo.FileName = GenerateXls;
                process.StartInfo.Arguments = @" mandiricc /c";

                process.EnableRaisingEvents = true;

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();
                process.WaitForExit();

            }
            catch (Exception ex) { throw ex; }

            try
            {
                foreach (Process proc in Process.GetProcessesByName("JBSGenExcel")) { proc.Kill(); }
            }
            catch (Exception ex) { throw ex; }
        }

        protected void GenMegaOnUsCCFile()
        {
            FileInfo FileName = new FileInfo(this.MegaOnUsccFile);
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
            FileInfo FileName = new FileInfo( this.MegaOfUsccFile);
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
            FileInfo FileName = new FileInfo(this.BNIccFile);
            if (!FileName.Exists)
            {
                FileName = new FileInfo(TempBniFile);
                FileName.CopyTo(this.BNIccFile);
                FileName = new FileInfo(this.BNIccFile);

                using (ExcelPackage package = new ExcelPackage(new FileInfo(this.BNIccFile)))
                {
                    ExcelWorkbook workBook = package.Workbook;
                    var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "BillingBNIcc_sp ";
                    cmd.Parameters.Add(new MySqlParameter("@BankCode", MySqlDbType.Int16) { Value = id });

                    try
                    {
                        ExcelWorksheet ws = workBook.Worksheets.SingleOrDefault(w => w.Name == "sheet1");
                        cmd.Connection.Open();

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
                        package.Save();
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
        }

        protected void GenBcaAcFile()
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName("JBSGenExcel")){ proc.Kill(); }
            }
            catch (Exception ex) { throw ex; }

            try
            {
                var process = new Process();
                process.StartInfo.FileName = GenerateXls;
                process.StartInfo.Arguments = @" bcaac /c";
                process.EnableRaisingEvents = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex) { throw ex; }

            try
            {
                foreach (Process proc in Process.GetProcessesByName("JBSGenExcel")) { proc.Kill(); }
            }
            catch (Exception ex) { throw ex; }
        }

        protected void GenMandiriAcFile()
        {
            FileInfo FileName = new FileInfo(this.MandiriAcFile);
            if (!FileName.Exists) //jika file belum ada akan di generate tp jika sudah ada maka akan pake file exist
            {
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "BillingMandiriAC_sp ";
                cmd.Connection.Open();

                try
                {
                    using (var result = cmd.ExecuteReader())
                    {
                        using (FileStream fs = new FileStream(FileName.ToString(), FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            using (StreamWriter writer = new StreamWriter(fs))
                            {
                                int i = 1;
                                while (result.Read())
                                {
                                    if (i == 1)
                                    {
                                        writer.Write(result["a"]);
                                        writer.Write("," + result["b"]);
                                    }
                                    else if (i == 2)
                                    {
                                        writer.Write(result["a"]);
                                        writer.Write("," + result["b"]);
                                        writer.Write("," + result["c"]);
                                        writer.Write("," + result["d"]);
                                        writer.Write("," + result["e"]);
                                    }
                                    else
                                    {
                                        writer.Write(result["a"]);
                                        writer.Write("," + result["b"]);
                                        writer.Write("," + result["c"]);
                                        writer.Write("," + result["d"]);
                                        writer.Write("," + result["e"]);
                                        writer.Write("," + result["f"]);
                                        writer.Write("," + result["g"]);
                                        writer.Write("," + result["h"]);
                                    }                                    
                                    writer.WriteLine();
                                    i++;
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

        protected void GenVA()
        {

            try
            {
                foreach (Process proc in Process.GetProcessesByName("JBSGenExcel")) { proc.Kill(); }
            }
            catch (Exception ex) { throw ex; }

            try
            {
                var process = new Process();
                process.StartInfo.FileName = GenerateXls;
                process.StartInfo.Arguments = @" va /c";

                process.EnableRaisingEvents = true;

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex) { throw ex; }
        }

        public ActionResult reset()
        {
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"UPDATE `billing` as b 
                                        SET b.`IsDownload`=0,
                                        b.`BankIdDownload`=null,
                                        b.BankID_Source=null,
                                        b.IsClosed=0,
                                        b.status_billing='A',
                                        b.status_billing_dateUpdate=null,
                                        b.paid_date=null,
                                        b.Life21TranID=null,
                                        b.ReceiptID=null,
                                        b.PaymentTransactionID=null,
                                        b.`BillingDate`= null,
                                        b.Source_download=null; ";
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"UPDATE `billing_download_summary` AS bs
                                    SET bs.`AmountDownload`=0,
                                    bs.`rowCountDownload`=0; ";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"UPDATE `billing_others` as b 
                                        SET b.`IsDownload`=0,
                                        b.`BankIdDownload`=null,
                                        b.BankID_Source=null,
                                        b.IsClosed=0,
                                        b.status_billing='A',
                                        b.paid_date=null,
                                        b.Life21TranID=null,
                                        b.ReceiptID=null,
                                        b.PaymentTransactionID=null,
                                        b.`BillingDate`= null,
                                        b.Source_download=null; ";
                cmd.ExecuteNonQuery();

                var files = Directory.GetFiles(DirBilling);
                foreach (string file in files)
                {
                    FileInfo fileBill = new FileInfo(file);
                    fileBill.Delete();
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

            return RedirectToAction("Index");
        }

        public ActionResult Recalculate()
        {
            hitungUlang();
            return RedirectToAction("Index");
        }

        public void hitungUlang()
        {
            // Proses Hitung Ulang summary billing yang di Download
            // efeknya ketika upload result, apakah sudah semua data yg di download dikasi result
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = @"UpdateBillSum";
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
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

        private string CekDataDownload(int trancode, string sourceDownload)
        {
            string pesan="",jenisTransaksi="";
            // Proses cek jlh data yg didownload, jika 0 maka file yang sudah terbentuk harus di hapus
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;

            if (sourceDownload == "CC")
            {
                if (trancode == 1) //BCA CC
                {
                    jenisTransaksi = "BCA CC";
                    cmd.CommandText = @"SELECT 1 AS val FROM `billing` b
                        WHERE b.`IsDownload`=1 AND b.`BankIdDownload`=1 AND b.`Source_download`='CC' LIMIT 1;";
                }
                else if (trancode == 2) // Mandiri CC
                {
                    jenisTransaksi = "Mandiri CC";
                    cmd.CommandText = @"SELECT 1 AS val FROM `billing` b
                        WHERE b.`IsDownload`=1 AND b.`BankIdDownload`=2 AND b.`Source_download`='CC' LIMIT 1";
                }
                else if (trancode == 3) //Mega On Us
                {
                    jenisTransaksi = "MegaOnUs CC";
                    cmd.CommandText = @"SELECT 1 AS val FROM `billing` b
                        WHERE b.`IsDownload`=1 AND b.`BankIdDownload`=12 AND IFNULL(b.`BankID_Source`,0)=12 AND b.`Source_download`='CC' LIMIT 1";
                }
                else if (trancode == 4)// Mega Off Us
                {
                    jenisTransaksi = "MegaOffUs CC";
                    cmd.CommandText = @"SELECT 1 AS val FROM `billing` b
                        WHERE b.`IsDownload`=1 AND b.`BankIdDownload`=12 AND IFNULL(b.`BankID_Source`,0)<>12 AND b.`Source_download`='CC' LIMIT 1";
                }
                else if (trancode == 5) // BNI CC
                {
                    jenisTransaksi = "BNI CC";
                    cmd.CommandText = @"SELECT 1 AS val FROM `billing` b
                        WHERE b.`IsDownload`=1 AND b.`BankIdDownload`=3 AND b.`Source_download`='CC' LIMIT 1";
                }

            }
            else if (sourceDownload == "AC")
            {
                if (trancode == 1) // BCA AC
                {
                    jenisTransaksi = "BCA AC";
                    cmd.CommandText = @"SELECT 1 AS val FROM `billing` b
                        WHERE b.`IsDownload`=1 AND b.`BankIdDownload`=1 AND b.`Source_download`='AC' LIMIT 1";
                }
                else if (trancode == 2) // Mandiri AC
                {
                    jenisTransaksi = "Mandiri AC";
                    cmd.CommandText = @"SELECT 1 AS val FROM `billing` b
                        WHERE b.`IsDownload`=1 AND b.`BankIdDownload`=2 AND b.`Source_download`='AC' LIMIT 1";
                }
            }
                
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if(!rd.HasRows)
                {// Jika Data kosong
                    pesan = String.Format("({0}) Data Kosong", jenisTransaksi);
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
            return pesan;
        }

        [HttpGet]
        public ActionResult UploadResult( string TranCode)
        {
            string layout = "UploadResult";
            UploadResultBillingVM UploadBill = new UploadResultBillingVM();
            UploadBill.TranCode = TranCode;
            switch(TranCode)
            {
                case "bcacc": UploadBill.Description = "BCA CC"; break;
                case "mandiricc": UploadBill.Description = "Mandiri CC"; break;
                case "megaonus": UploadBill.Description = "MegaOnUs CC"; break;
                case "megaoffus": UploadBill.Description = "MegaOffUs CC"; break;
                case "bnicc": UploadBill.Description = "BNI CC"; break;
                case "bcaac": UploadBill.Description = "BCA AC"; break;
                case "mandiriac": UploadBill.Description = "Mandiri AC"; break;
            }
            return View(layout,UploadBill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadResult(string TranCode, [Bind("TranCode,FileBill")] UploadResultBillingVM UploadBill)
        {
            if (UploadBill.FileBill != null)
            { //validasi file manual
                if (UploadBill.TranCode == "bcacc")
                { //  Path.GetFileNameWithoutExtension(fullPath)
                    if (Path.GetExtension( UploadBill.FileBill.FileName.ToString().ToLower()) != ".txt")
                        ModelState.AddModelError("FileBill", "ResultFile BCA CC harus File .txt");
                }
                else if (UploadBill.TranCode == "megaonus")
                { //  Path.GetFileNameWithoutExtension(fullPath)
                    if (UploadBill.FileBill.FileName.ToString().ToLower().Substring(UploadBill.FileBill.FileName.Length-11) != "s1.bret.xls")
                        ModelState.AddModelError("FileBill", "ResultFile harus File *s1.bret.xls");
                }
                else if (UploadBill.TranCode == "megaoffus")
                { //  Path.GetFileNameWithoutExtension(fullPath)
                    if (UploadBill.FileBill.FileName.ToString().ToLower().Substring(UploadBill.FileBill.FileName.Length - 11) != "s2.bret.xls")
                        ModelState.AddModelError("FileBill", "ResultFile harus File *s2.bret.xls");
                }
                else if (UploadBill.TranCode == "bcaac" || UploadBill.TranCode == "mandiriac")
                {
                    if (Path.GetExtension(UploadBill.FileBill.FileName.ToString().ToLower()) != ".txt")
                        ModelState.AddModelError("FileBill", "ResultFile harus File .txt");
                }
                else
                {
                    if (Path.GetExtension(UploadBill.FileBill.FileName.ToString().ToLower()) != ".xls")
                        ModelState.AddModelError("FileBill", "ResultFile harus File .xls");
                }
                if(UploadBill.FileBill.Length <1)
                {
                    ModelState.AddModelError("FileBill", "Data File kosong");
                }
            }
            if (ModelState.IsValid)
            {
                // Proses baca result BCA CC
                if(UploadBill.TranCode=="bcacc") ResultBCACC(UploadBill);

                // Proses baca result Mandiri CC
                if (UploadBill.TranCode == "mandiricc") ResultMandiriCC(UploadBill);
                
                // Proses baca result MegaOnUs CC
                if (UploadBill.TranCode == "megaonus") ResultMegaOnUsCC(UploadBill);

                // Proses baca result MegaOffUs CC
                if (UploadBill.TranCode == "megaoffus") ResultMegaOffUsCC(UploadBill);

                // Proses baca result BNI CC
                if (UploadBill.TranCode == "bnicc") ResultBNICC(UploadBill);

                // Proses baca result BCA CC
                if (UploadBill.TranCode == "bcaac") ResultBCAAC(UploadBill);
                
                // Proses baca result Mandiri AC
                if (UploadBill.TranCode == "mandiriac") ResultMandiriAC(UploadBill);

                //ModelState.AddModelError("FileBill","Baris ke-"+ errorKode.ToString() + " gak match dengan data download");
                return RedirectToAction("Index");
            }
            switch (TranCode)
            {
                case "bcacc": UploadBill.Description = "BCA CC"; break;
                case "mandiricc": UploadBill.Description = "Mandiri CC"; break;
                case "megaonus": UploadBill.Description = "MegaOnUs CC"; break;
                case "megaoffus": UploadBill.Description = "MegaOffUs CC"; break;
                case "bnicc": UploadBill.Description = "BNI CC"; break;
                case "bcaac": UploadBill.Description = "BCA AC"; break;
                case "mandiriac": UploadBill.Description = "Mandiri AC"; break;
            }
            return View(UploadBill);
        }

        private void ResultBCACC(UploadResultBillingVM UploadBill)
        {
            string tmp, approvalCode, TranDesc,txfilename, policyNo="",Period = "", CCno = "", CCexp = "", ccName = "", addr="",telp="",BillOthers="";
            int? PolicyID=-1, BillingID=-1, recurring_seq=-1;
            int CycleDate=0;
            DateTime DueDatePre = new DateTime(2000, 1, 1), BillDate = new DateTime(2000,1,1);
            decimal BillAmount=0;
            txfilename = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName);
            bool isApprove = (txfilename.Substring(txfilename.Length-1) =="A" ? true : false);

            string xFileName= Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName).ToLower() + 
                Path.GetRandomFileName().Replace(".", "").Substring(0, 8).ToLower() + ".txt";

            // Simpan File yang diUpload ke File Backup
            using (var fileStream = new FileStream(BackupResult + xFileName, FileMode.Create))
            {
                UploadBill.FileBill.CopyToAsync(fileStream);
            }

            using (var reader = new StreamReader(UploadBill.FileBill.OpenReadStream()))
            {
                int i = 1;
                while (reader.Peek() >= 0)
                {
                    tmp = reader.ReadLine();
                    if (tmp.Length < 40) continue; // Jika karakter cma 40, skip karena akan error utk diolah

                    if(tmp.Substring(9, 1)=="A") BillOthers = tmp.Substring(9, 25).Trim(); 
                    else policyNo = tmp.Substring(9, 25).Trim();

                    approvalCode = tmp.Substring(tmp.Length - 8).Substring(0, 6);
                    TranDesc = tmp.Substring(tmp.Length - 2);

                    var cmdx = _jbsDB.Database;
                    cmdx.OpenConnection();
                    var cmdx2 = _life21.Database;
                    cmdx2.OpenConnection();

                    var cmd = cmdx.GetDbConnection().CreateCommand();
                    var cmd2 = cmdx2.GetDbConnection().CreateCommand();

                    using (var dbTrans = cmdx.BeginTransaction())// pake userDB hanya utk koneksi aja biar gak sama dgn transaction
                    {
                        using (var dbTrans2 = cmdx2.BeginTransaction())
                        {
                            try
                            {
                                cmd.Parameters.Clear();
                                cmd.CommandType = CommandType.StoredProcedure;
                                if (policyNo != "") // jika transaksi billing Recuring
                                {
                                    cmd.CommandText = @"FindPolisCCGetBillSeq";
                                    cmd.Parameters.Add(new MySqlParameter("@NoPolis", MySqlDbType.VarChar) { Value = policyNo });

                                    using (var rd = cmd.ExecuteReader())
                                    {
                                        while (rd.Read())
                                        {
                                            PolicyID = Convert.ToInt32(rd["policy_id"]);
                                            BillingID = Convert.ToInt32(rd["BillingID"]);
                                            recurring_seq = Convert.ToInt32(rd["recurring_seq"]);
                                            BillDate = Convert.ToDateTime(rd["BillingDate"]);
                                            DueDatePre = Convert.ToDateTime(rd["due_dt_pre"]);
                                            BillAmount = Convert.ToDecimal(rd["TotalAmount"]);
                                            Period = rd["PeriodeBilling"].ToString();
                                            CycleDate = Convert.ToInt32(rd["cycleDate"]);
                                            CCno = rd["cc_no"].ToString();
                                            CCexp = rd["cc_expiry"].ToString();
                                            ccName = rd["cc_name"].ToString();
                                            addr = rd["cc_address"].ToString();
                                            telp = rd["cc_telephone"].ToString();
                                            BillOthers = "";
                                        }

                                        if (PolicyID < 1 || BillingID < 1 || recurring_seq < 1)
                                        {
                                            throw new Exception("Polis tidak ditemukan,mungkin billingnya tidak dalam status download atau terdapat kesalahan pada data file Upload...");
                                        }
                                    }
                                }
                                else if (BillOthers != "") // jika transaksi Billing Others
                                {
                                    cmd.CommandText = @"FindPolisBillOthers";
                                    cmd.Parameters.Add(new MySqlParameter("@BillOthersNo", MySqlDbType.VarChar) { Value = BillOthers });

                                    using (var rd = cmd.ExecuteReader())
                                    {
                                        while (rd.Read())
                                        {
                                            PolicyID = Convert.ToInt32(rd["policy_id"]);
                                            policyNo= rd["policy_no"].ToString();
                                            BillOthers= rd["BillingID"].ToString();
                                            BillDate = Convert.ToDateTime(rd["BillingDate"]);

                                            BillAmount = Convert.ToDecimal(rd["TotalAmount"]);
                                            CCno = rd["cc_no"].ToString();
                                            CCexp = rd["cc_expiry"].ToString();
                                            ccName = rd["cc_name"].ToString();
                                            addr = rd["cc_address"].ToString();
                                            telp = rd["cc_telephone"].ToString();
                                        }

                                        if (PolicyID < 1 )
                                        {
                                            throw new Exception("BillingOthersID tidak ditemukan,mungkin billingnya tidak dalam status download atau terdapat kesalahan pada data file Upload...");
                                        }
                                    }
                                }

                                cmd.Parameters.Clear();
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = @"InsertTransactionBank;";
                                cmd.Parameters.Add(new MySqlParameter("@FileName", MySqlDbType.VarChar) { Value = xFileName });
                                cmd.Parameters.Add(new MySqlParameter("@Trancode", MySqlDbType.VarChar) { Value = "bcacc" }); // hardCode BCA CC
                                cmd.Parameters.Add(new MySqlParameter("@IsApprove", MySqlDbType.Bit) { Value = isApprove });
                                cmd.Parameters.Add(new MySqlParameter("@policyID", MySqlDbType.VarChar) { Value = PolicyID });
                                cmd.Parameters.Add(new MySqlParameter("@Seq", MySqlDbType.VarChar) { Value = (BillOthers == "")?recurring_seq : 0});
                                cmd.Parameters.Add(new MySqlParameter("@IDBill", MySqlDbType.VarChar) { Value = (BillOthers=="")? BillingID.ToString():BillOthers });
                                cmd.Parameters.Add(new MySqlParameter("@approvalCode", MySqlDbType.VarChar) { Value = (isApprove) ? approvalCode : TranDesc });
                                cmd.Parameters.Add(new MySqlParameter("@BankID", MySqlDbType.Int32) { Value = 1 }); // hardCode BCA
                                cmd.Parameters.Add(new MySqlParameter("@ErrCode", MySqlDbType.VarChar) { Value = TranDesc });
                                var uid = cmd.ExecuteScalar().ToString();

                                if (isApprove) // jika transaksi d approve bank, ada flag approve di file
                                {// ============================ Proses Insert Received ===========================
                                    cmd2.Parameters.Clear();
                                    cmd2.CommandType = CommandType.StoredProcedure;
                                    cmd2.CommandText = @"ReceiptInsert";
                                    cmd2.Parameters.Add(new MySqlParameter("@BillingDate", MySqlDbType.Date) { Value = BillDate });
                                    cmd2.Parameters.Add(new MySqlParameter("@policy_id", MySqlDbType.Int32) { Value = PolicyID });
                                    cmd2.Parameters.Add(new MySqlParameter("@receipt_amount", MySqlDbType.Decimal) { Value = BillAmount });
                                    cmd2.Parameters.Add(new MySqlParameter("@Source_download", MySqlDbType.VarChar) { Value = "CC" });
                                    cmd2.Parameters.Add(new MySqlParameter("@recurring_seq", MySqlDbType.Int32) { Value = (BillOthers == "") ? recurring_seq:0 });
                                    cmd2.Parameters.Add(new MySqlParameter("@bank_acc_id", MySqlDbType.Int32) { Value = 1 });
                                    cmd2.Parameters.Add(new MySqlParameter("@due_dt_pre", MySqlDbType.Date) { Value = (BillOthers == "") ? DueDatePre:BillDate });
                                    var receiptID = cmd2.ExecuteScalar().ToString();

                                    // ============================ Proses Insert Pilis CC Transaction Life21 ===========================
                                    cmd2.Parameters.Clear();
                                    cmd2.CommandType = CommandType.StoredProcedure;
                                    cmd2.CommandText = @"InsertPolistransCC";
                                    cmd2.Parameters.Add(new MySqlParameter("@PolisID", MySqlDbType.Int32) { Value = PolicyID });
                                    cmd2.Parameters.Add(new MySqlParameter("@Transdate", MySqlDbType.Date) { Value = BillDate });
                                    cmd2.Parameters.Add(new MySqlParameter("@Seq", MySqlDbType.Int32) { Value = (BillOthers == "") ? recurring_seq : 0 });
                                    cmd2.Parameters.Add(new MySqlParameter("@Amount", MySqlDbType.Decimal) { Value = BillAmount });
                                    cmd2.Parameters.Add(new MySqlParameter("@DueDatePre", MySqlDbType.Date) { Value = (BillOthers == "") ? DueDatePre : BillDate });
                                    cmd2.Parameters.Add(new MySqlParameter("@Period", MySqlDbType.VarChar) { Value = Period });
                                    cmd2.Parameters.Add(new MySqlParameter("@CycleDate", MySqlDbType.Int32) { Value = (BillOthers == "") ? CycleDate : 0});
                                    cmd2.Parameters.Add(new MySqlParameter("@BankID", MySqlDbType.Int32) { Value = 1 });
                                    cmd2.Parameters.Add(new MySqlParameter("@CCno", MySqlDbType.VarChar) { Value = CCno });
                                    cmd2.Parameters.Add(new MySqlParameter("@CCExpiry", MySqlDbType.VarChar) { Value = CCexp});
                                    cmd2.Parameters.Add(new MySqlParameter("@CCName", MySqlDbType.VarChar) { Value = ccName});
                                    cmd2.Parameters.Add(new MySqlParameter("@CCAddrs", MySqlDbType.VarChar) { Value = addr});
                                    cmd2.Parameters.Add(new MySqlParameter("@CCtelp", MySqlDbType.VarChar) { Value = telp});
                                    cmd2.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = receiptID });
                                    var CCTransID = cmd2.ExecuteScalar().ToString();

                                    // Update table billing
                                    cmd.Parameters.Clear();
                                    cmd.CommandType = CommandType.Text;
                                    if (BillOthers == "")
                                    {
                                        cmd.CommandText = @"UPDATE `billing` SET `IsDownload`=0,
			                                                `IsClosed`=1,
			                                                `status_billing`='P',
			                                                `status_billing_dateUpdate`=@tgl,
			                                                `paid_date`=@billDate,
                                                            Life21TranID=@TransactionID,
			                                                `ReceiptID`=@receiptID,
			                                                `PaymentTransactionID`=@uid
		                                                WHERE `BillingID`=@idBill;";
                                        cmd.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.Int32) { Value = BillingID });
                                    }
                                    else
                                    {
                                        cmd.CommandText = @"UPDATE `billing_others` SET `IsDownload`=0,
			                                                `IsClosed`=1,
			                                                `status_billing`='P',
			                                                `status_billing_dateUpdate`=@tgl,
			                                                `paid_date`=@billDate,
                                                            Life21TranID=@TransactionID,
			                                                `ReceiptID`=@receiptID,
			                                                `PaymentTransactionID`=@uid
		                                                WHERE `BillingID`=@idBill;";
                                        cmd.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.VarChar) { Value = BillOthers });
                                    }
                                    
                                    cmd.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = DateTime.Now });
                                    cmd.Parameters.Add(new MySqlParameter("@billDate", MySqlDbType.DateTime) { Value = BillDate });
                                    cmd.Parameters.Add(new MySqlParameter("@TransactionID", MySqlDbType.Int32) { Value = CCTransID });
                                    cmd.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = receiptID });
                                    cmd.Parameters.Add(new MySqlParameter("@uid", MySqlDbType.VarChar) { Value = uid });                                    
                                    cmd.ExecuteNonQuery();

                                    // Update Polis Last Transaction
                                    if (BillOthers == "") // Hanya untuk billing recurring
                                    {
                                        cmd.Parameters.Clear();
                                        cmd.CommandType = CommandType.Text;
                                        cmd.CommandText = @"UPDATE `policy_last_trans` AS pt
		                                                INNER JOIN `billing` AS bx ON bx.policy_id=pt.policy_Id
			                                                SET pt.BillingID=bx.BillingID,
			                                                pt.recurring_seq=bx.recurring_seq,
			                                                pt.due_dt_pre=bx.due_dt_pre,
			                                                pt.source=bx.Source_download,
			                                                pt.receipt_id=bx.`ReceiptID`,
			                                                pt.receipt_date=bx.BillingDate,
			                                                pt.bank_id=bx.BankIdDownload
		                                                WHERE pt.policy_Id=@policyID AND bx.BillingID=@idBill;";
                                        cmd.Parameters.Add(new MySqlParameter("@policyID", MySqlDbType.Int32) { Value = PolicyID });
                                        cmd.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.Int32) { Value = BillingID });
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                else // jika transaksi d reject bank
                                {//billing hanya ganti flag download, kolom lain tetap sbg status terakhir
                                    cmd.Parameters.Clear();
                                    cmd.CommandType = CommandType.Text;
                                    if (BillOthers == "")
                                    {// Transaksi Billing Rucurring
                                        cmd.CommandText = @"UPDATE `billing` SET IsDownload=0 WHERE `BillingID`=@billid";
                                        cmd.Parameters.Add(new MySqlParameter("@billid", MySqlDbType.Int32) { Value = BillingID });
                                    }
                                    else
                                    {// transaksi Billing Others
                                        cmd.CommandText = @"UPDATE `billing_others` SET IsDownload=0 WHERE `BillingID`=@billid";
                                        cmd.Parameters.Add(new MySqlParameter("@billid", MySqlDbType.VarChar) { Value = BillOthers });
                                    }
                                    cmd.ExecuteNonQuery();
                                }
                                dbTrans.Commit();
                                dbTrans2.Commit();
                            }
                            catch (Exception ex)
                            {
                                dbTrans.Rollback();
                                dbTrans2.Rollback();
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Clear();
                                cmd.CommandText = @"INSERT INTO `log_error_upload_result`(TranCode,line,FileName,exceptionApp)
                                            SELECT @TranCode,@line,@FileName,@exceptionApp";
                                cmd.Parameters.Add(new MySqlParameter("@TranCode", MySqlDbType.VarChar) { Value = UploadBill.TranCode });
                                cmd.Parameters.Add(new MySqlParameter("@line", MySqlDbType.Int32) { Value = i });
                                cmd.Parameters.Add(new MySqlParameter("@FileName", MySqlDbType.VarChar) { Value = xFileName });
                                cmd.Parameters.Add(new MySqlParameter("@exceptionApp", MySqlDbType.VarChar) { Value = ex.Message.Substring(0, ex.Message.Length < 255 ? ex.Message.Length : 253) });
                                cmd.ExecuteNonQuery();
                            }
                            finally
                            {
                                dbTrans.Dispose();
                                dbTrans2.Dispose();
                                cmdx.CloseConnection();
                                cmdx2.CloseConnection();
                            }
                        }
                    }
                    BillAmount = 0;
                    BillOthers = "";
                    policyNo = "";
                    PolicyID = -1;
                    BillingID = -1;
                    recurring_seq = -1;
                    approvalCode = null;
                    TranDesc = null;
                    i++;
                }// End While
            }

            // cek data downlod, jika sudah nol maka data billingDownload pindahkan ke Backup Billing
            try
            {
                hitungUlang();
                _jbsDB.Database.OpenConnection();
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"SELECT `rowCountDownload` FROM `billing_download_summary` WHERE id=1;";
                var sumdata = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                if (sumdata <= 0)
                {
                    string[] files = Directory.GetFiles(DirBilling, "CAF*.prn", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        var FileBil = new FileInfo(files[0]);
                        FileBil.MoveTo(BackupFile + FileBil.Name.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _jbsDB.Database.CloseConnection();
            }
        }
        private void ResultMandiriCC(UploadResultBillingVM UploadBill)
        {
            string xFileName = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName).ToLower() +
               Path.GetRandomFileName().Replace(".", "").Substring(0, 8).ToLower() + ".xls";

            if (System.IO.File.Exists(DirResult + xFileName)) System.IO.File.Delete(DirResult + xFileName);
            using (var fileStream = new FileStream(DirResult + xFileName, FileMode.Create)) UploadBill.FileBill.CopyTo(fileStream);

            try
            {
                foreach (Process proc in Process.GetProcessesByName("JBSGenExcel")) { proc.Kill(); }
            }
            catch (Exception ex) { throw ex; }

            try
            {
                var process = new Process();
                process.StartInfo.FileName = GenerateXls;
                process.StartInfo.Arguments = @" resultmandiricc " + xFileName + " /c";
                process.EnableRaisingEvents = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex) { throw ex; }

            try
            {
                foreach (Process proc in Process.GetProcessesByName("JBSGenExcel")) { proc.Kill(); }
            }
            catch (Exception ex) { throw ex; }

            //Backup alias pindah File Result
            var FileBilResult = new FileInfo(DirResult + xFileName);
            try
            {
                FileBilResult.MoveTo(BackupResult + FileBilResult.Name.ToString());
            }
            catch (Exception ex) { throw ex; }

            // cek data downlod, jika sudah nol maka data billingDownload pindahkan ke Backup Billing
            try
            {
                hitungUlang();
                _jbsDB.Database.OpenConnection();
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"SELECT `rowCountDownload` FROM `billing_download_summary` WHERE id=2;";
                var sumdata = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                if (sumdata <= 0)
                {
                    string[] files = Directory.GetFiles(DirBilling, "Mandiri_*.xls", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        var FileBil = new FileInfo(files[0]);
                        FileBil.MoveTo(BackupFile + FileBil.Name.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _jbsDB.Database.CloseConnection();
            }
        }
        private void ResultMegaOnUsCC(UploadResultBillingVM UploadBill)
        {
            string xFileName = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName).ToLower() +
               Path.GetRandomFileName().Replace(".", "").Substring(0, 8).ToLower() + ".xls";

            if (System.IO.File.Exists(DirResult + xFileName)) System.IO.File.Delete(DirResult + xFileName);
            using (var fileStream = new FileStream(DirResult + xFileName, FileMode.Create)) UploadBill.FileBill.CopyTo(fileStream);

            try
            {
                foreach (Process proc in Process.GetProcessesByName("JBSGenExcel")) { proc.Kill(); }
            }
            catch (Exception ex) { throw ex; }

            try
            {
                var process = new Process();
                process.StartInfo.FileName = GenerateXls;
                process.StartInfo.Arguments = @" resultmegaonuscc " + xFileName + " /c";
                process.EnableRaisingEvents = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex) { throw ex; }

            try
            {
                foreach (Process proc in Process.GetProcessesByName("JBSGenExcel")) { proc.Kill(); }
            }
            catch (Exception ex) { throw ex; }

            //Backup alias pindah File Result
            var FileBilResult = new FileInfo(DirResult + xFileName);
            try
            {
                FileBilResult.MoveTo(BackupResult + FileBilResult.Name.ToString());
            }
            catch (Exception ex) { throw ex; }

            // cek data downlod, jika sudah nol maka data billingDownload pindahkan ke Backup Billing
            try
            {
                hitungUlang();
                _jbsDB.Database.OpenConnection();
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"SELECT `rowCountDownload` FROM `billing_download_summary` WHERE id=3;";
                var sumdata = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                if (sumdata <= 0)
                {
                    string[] files = Directory.GetFiles(DirBilling, "CAF*_MegaOnUs.bpmt", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        var FileBil = new FileInfo(files[0]);
                        FileBil.MoveTo(BackupFile + FileBil.Name.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _jbsDB.Database.CloseConnection();
            }
        }
        private void ResultMegaOffUsCC(UploadResultBillingVM UploadBill)
        {
            string xFileName = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName).ToLower() +
               Path.GetRandomFileName().Replace(".", "").Substring(0, 8).ToLower() + ".xls";

            if (System.IO.File.Exists(DirResult + xFileName)) System.IO.File.Delete(DirResult + xFileName);
            using (var fileStream = new FileStream(DirResult + xFileName, FileMode.Create)) UploadBill.FileBill.CopyTo(fileStream);

            try
            {
                foreach (Process proc in Process.GetProcessesByName("JBSGenExcel")) { proc.Kill(); }
            }
            catch (Exception ex) { throw ex; }

            try
            {
                var process = new Process();
                process.StartInfo.FileName = GenerateXls;
                process.StartInfo.Arguments = @" resultmegaoffuscc " + xFileName + " /c";
                process.EnableRaisingEvents = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex) { throw ex; }

            try
            {
                foreach (Process proc in Process.GetProcessesByName("JBSGenExcel")) { proc.Kill(); }
            }
            catch (Exception ex) { throw ex; }

            //Backup alias pindah File Result
            var FileBilResult = new FileInfo(DirResult + xFileName);
            try
            {
                FileBilResult.MoveTo(BackupResult + FileBilResult.Name.ToString());
            }
            catch (Exception ex) { throw ex; }

            // cek data downlod, jika sudah nol maka data billingDownload pindahkan ke Backup Billing
            try
            {
                hitungUlang();
                _jbsDB.Database.OpenConnection();
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"SELECT `rowCountDownload` FROM `billing_download_summary` WHERE id=4;";
                var sumdata = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                if (sumdata <= 0)
                {
                    string[] files = Directory.GetFiles(DirBilling, "CAF*_MegaOffUs.bpmt", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        var FileBil = new FileInfo(files[0]);
                        FileBil.MoveTo(BackupFile + FileBil.Name.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _jbsDB.Database.CloseConnection();
            }
        }
        private void ResultBNICC(UploadResultBillingVM UploadBill)
        {
            string xFileName = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName).ToLower() +
               Path.GetRandomFileName().Replace(".", "").Substring(0, 8).ToLower() + ".xls";

            if (System.IO.File.Exists(DirResult + xFileName)) System.IO.File.Delete(DirResult + xFileName);
            using (var fileStream = new FileStream(DirResult + xFileName, FileMode.Create)) UploadBill.FileBill.CopyTo(fileStream);

            try
            {
                foreach (Process proc in Process.GetProcessesByName("JBSGenExcel")) { proc.Kill(); }
            }
            catch (Exception ex) { throw ex; }

            try
            {
                var process = new Process();
                process.StartInfo.FileName = GenerateXls;
                process.StartInfo.Arguments = @" resultbnicc " + xFileName + " /c";
                process.EnableRaisingEvents = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex) { throw ex; }

            try
            {
                foreach (Process proc in Process.GetProcessesByName("JBSGenExcel")) { proc.Kill(); }
            }
            catch (Exception ex) { throw ex; }

            //Backup File Result
            var FileBilResult = new FileInfo(DirResult + xFileName);
            try
            {
                FileBilResult.MoveTo(BackupResult + FileBilResult.Name.ToString());
            }
            catch (Exception ex) { throw ex; }

            // cek data downlod, jika sudah nol maka data billingDownload pindahkan ke Backup Billing
            try
            {
                hitungUlang();
                _jbsDB.Database.OpenConnection();
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"SELECT `rowCountDownload` FROM `billing_download_summary` WHERE id=5;";
                var sumdata = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                if (sumdata <= 0)
                {
                    string[] files = Directory.GetFiles(DirBilling, "BNI_*.xlsx", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        var FileBil = new FileInfo(files[0]);
                        FileBil.MoveTo(BackupFile + FileBil.Name.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _jbsDB.Database.CloseConnection();
            }

        }
        private void ResultBCAAC(UploadResultBillingVM UploadBill)
        {
            string tmp, approvalCode, TranDesc, txfilename, policyNo, Period = "", ACno = "", acName = "" ;
            int? PolicyID = -1, BillingID = -1, recurring_seq = -1;
            int CycleDate = 0;
            DateTime DueDatePre = new DateTime(2000, 1, 1), BillDate = new DateTime(2000, 1, 1);
            decimal BillAmount = 0;
            txfilename = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName);
            bool isApprove;

            string xFileName = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName).ToLower() +
                Path.GetRandomFileName().Replace(".", "").Substring(0, 8).ToLower() + ".txt";

            // Simpan File yang diUpload ke File Backup
            using (var fileStream = new FileStream(BackupResult + xFileName, FileMode.Create))
            {
                UploadBill.FileBill.CopyToAsync(fileStream);
            }

            using (var reader = new StreamReader(UploadBill.FileBill.OpenReadStream()))
            {
                int i = 1;
                while (reader.Peek() >= 0)
                {
                    tmp = reader.ReadLine();
                    if (tmp.Length < 40) continue; // Jika karakter cma 40, skip karena akan error utk diolah
                    if (i < 2) continue; // baca mulai baris ke 2

                    policyNo = tmp.Substring(92, 15).Trim();
                    approvalCode = tmp.Substring(129,9).Trim();
                    TranDesc = tmp.Substring(138,50).Trim();
                    isApprove = (approvalCode.ToLower() =="berhasil") ? true : false;

                    var cmdx = _jbsDB.Database;
                    cmdx.OpenConnection();
                    var cmdx2 = _life21.Database;
                    cmdx2.OpenConnection();

                    var cmd = cmdx.GetDbConnection().CreateCommand();
                    var cmd2 = cmdx2.GetDbConnection().CreateCommand();

                    using (var dbTrans = cmdx.BeginTransaction()) // pake userDB hanya utk koneksi aja biar gak sama dgn transaction
                    {
                        using (var dbTrans2 = cmdx2.BeginTransaction())
                        {
                            try
                            {
                                cmd.Parameters.Clear();
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = @"FindPolisACGetBillSeq";
                                cmd.Parameters.Add(new MySqlParameter("@NoPolis", MySqlDbType.VarChar) { Value = policyNo });
                                using (var rd = cmd.ExecuteReader())
                                {
                                    while (rd.Read())
                                    {
                                        PolicyID = Convert.ToInt32(rd["policy_id"]);
                                        BillingID = Convert.ToInt32(rd["BillingID"]);
                                        recurring_seq = Convert.ToInt32(rd["recurring_seq"]);
                                        BillDate = Convert.ToDateTime(rd["BillingDate"]);
                                        DueDatePre = Convert.ToDateTime(rd["due_dt_pre"]);
                                        BillAmount = Convert.ToDecimal(rd["TotalAmount"]);

                                        Period = rd["PeriodeBilling"].ToString();
                                        CycleDate = Convert.ToInt32(rd["cycleDate"]);
                                        ACno = rd["acc_no"].ToString();
                                        acName = rd["acc_name"].ToString();
                                    }

                                    if (PolicyID < 1 || BillingID < 1 || recurring_seq < 1)
                                    {
                                        throw new Exception("Polis tidak ditemukan,mungkin billingnya tidak dalam status download atau terdapat kesalahan pada data textfile...");
                                    }
                                }

                                cmd.Parameters.Clear();
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = @"InsertTransactionBank;";
                                cmd.Parameters.Add(new MySqlParameter("@FileName", MySqlDbType.VarChar) { Value = xFileName });
                                cmd.Parameters.Add(new MySqlParameter("@Trancode", MySqlDbType.VarChar) { Value = UploadBill.TranCode }); // hardCode BCA AC
                                cmd.Parameters.Add(new MySqlParameter("@IsApprove", MySqlDbType.Bit) { Value = isApprove });
                                cmd.Parameters.Add(new MySqlParameter("@policyID", MySqlDbType.VarChar) { Value = PolicyID });
                                cmd.Parameters.Add(new MySqlParameter("@Seq", MySqlDbType.VarChar) { Value = recurring_seq });
                                cmd.Parameters.Add(new MySqlParameter("@IDBill", MySqlDbType.VarChar) { Value = BillingID });
                                cmd.Parameters.Add(new MySqlParameter("@approvalCode", MySqlDbType.VarChar) { Value = approvalCode });
                                cmd.Parameters.Add(new MySqlParameter("@BankID", MySqlDbType.Int32) { Value = 0 }); // Bukan BCA CC
                                cmd.Parameters.Add(new MySqlParameter("@ErrCode", MySqlDbType.VarChar) { Value = TranDesc });
                                var uid = cmd.ExecuteScalar().ToString();

                                if (isApprove) // jika transaksi d approve bank, ada flag approve di file
                                {// ============================ Proses Insert Received ===========================
                                    cmd2.Parameters.Clear();
                                    cmd2.CommandType = CommandType.StoredProcedure;
                                    cmd2.CommandText = @"ReceiptInsert";
                                    cmd2.Parameters.Add(new MySqlParameter("@BillingDate", MySqlDbType.Date) { Value = BillDate });
                                    cmd2.Parameters.Add(new MySqlParameter("@policy_id", MySqlDbType.Int32) { Value = PolicyID });
                                    cmd2.Parameters.Add(new MySqlParameter("@receipt_amount", MySqlDbType.Decimal) { Value = BillAmount });
                                    cmd2.Parameters.Add(new MySqlParameter("@Source_download", MySqlDbType.VarChar) { Value = "AC" });
                                    cmd2.Parameters.Add(new MySqlParameter("@recurring_seq", MySqlDbType.Int32) { Value = recurring_seq });
                                    cmd2.Parameters.Add(new MySqlParameter("@bank_acc_id", MySqlDbType.Int32) { Value = 1 });
                                    cmd2.Parameters.Add(new MySqlParameter("@due_dt_pre", MySqlDbType.Date) { Value = DueDatePre });
                                    var receiptID = cmd2.ExecuteScalar().ToString();

                                    // ============================ Proses Insert Pilis CC Transaction Life21 ===========================
                                    cmd2.Parameters.Clear();
                                    cmd2.CommandType = CommandType.StoredProcedure;
                                    cmd2.CommandText = @"InsertPolistransAC";
                                    cmd2.Parameters.Add(new MySqlParameter("@PolisID", MySqlDbType.Int32) { Value = PolicyID });
                                    cmd2.Parameters.Add(new MySqlParameter("@Transdate", MySqlDbType.Date) { Value = BillDate });
                                    cmd2.Parameters.Add(new MySqlParameter("@TransType", MySqlDbType.VarChar) { Value = "R" });
                                    cmd2.Parameters.Add(new MySqlParameter("@Seq", MySqlDbType.Int32) { Value = recurring_seq });
                                    cmd2.Parameters.Add(new MySqlParameter("@Amount", MySqlDbType.Decimal) { Value = BillAmount });
                                    cmd2.Parameters.Add(new MySqlParameter("@DueDatePre", MySqlDbType.Date) { Value = DueDatePre });
                                    cmd2.Parameters.Add(new MySqlParameter("@Period", MySqlDbType.VarChar) { Value = Period });
                                    cmd2.Parameters.Add(new MySqlParameter("@CycleDate", MySqlDbType.Int32) { Value = CycleDate });
                                    cmd2.Parameters.Add(new MySqlParameter("@BankID", MySqlDbType.Int32) { Value = 1 });
                                    cmd2.Parameters.Add(new MySqlParameter("@ACno", MySqlDbType.VarChar) { Value = ACno });
                                    cmd2.Parameters.Add(new MySqlParameter("@ACName", MySqlDbType.VarChar) { Value = acName });
                                    cmd2.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = receiptID });
                                    var CCTransID = cmd2.ExecuteScalar().ToString();

                                    // Update table billing
                                    cmd.Parameters.Clear();
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = @"UPDATE `billing` SET `IsDownload`=0,
			                                                `IsClosed`=1,
			                                                `status_billing`='P',
			                                                `status_billing_dateUpdate`=@tgl,
			                                                `paid_date`=@billDate,
                                                            Life21TranID=@TransactionID,
			                                                `ReceiptID`=@receiptID,
			                                                `PaymentTransactionID`=@uid
		                                                WHERE `BillingID`=@idBill;";
                                    cmd.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = DateTime.Now });
                                    cmd.Parameters.Add(new MySqlParameter("@billDate", MySqlDbType.DateTime) { Value = BillDate });
                                    cmd.Parameters.Add(new MySqlParameter("@TransactionID", MySqlDbType.Int32) { Value = CCTransID });
                                    cmd.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = receiptID });
                                    cmd.Parameters.Add(new MySqlParameter("@uid", MySqlDbType.VarChar) { Value = uid });
                                    cmd.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.Int32) { Value = BillingID });
                                    cmd.ExecuteNonQuery();

                                    // Update Polis Last Transaction
                                    cmd.Parameters.Clear();
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = @"UPDATE `policy_last_trans` AS pt
		                                                INNER JOIN `billing` AS bx ON bx.policy_id=pt.policy_Id
			                                                SET pt.BillingID=bx.BillingID,
			                                                pt.recurring_seq=bx.recurring_seq,
			                                                pt.due_dt_pre=bx.due_dt_pre,
			                                                pt.source=bx.Source_download,
			                                                pt.receipt_id=bx.`ReceiptID`,
			                                                pt.receipt_date=bx.BillingDate,
			                                                pt.bank_id=bx.BankIdDownload
		                                                WHERE pt.policy_Id=@policyID AND bx.BillingID=@idBill;";
                                    cmd.Parameters.Add(new MySqlParameter("@policyID", MySqlDbType.Int32) { Value = PolicyID });
                                    cmd.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.Int32) { Value = BillingID });
                                    cmd.ExecuteNonQuery();

                                }
                                else // jika transaksi d reject bank
                                {//billing hanya ganti flag download, kolom lain tetap sbg status terakhir
                                    cmd.CommandType = CommandType.Text;
                                    cmd.Parameters.Clear();
                                    cmd.CommandText = @"UPDATE `billing` SET IsDownload=0 WHERE `BillingID`=@billid";
                                    cmd.Parameters.Add(new MySqlParameter("@billid", MySqlDbType.Int32) { Value = BillingID });
                                    cmd.ExecuteNonQuery();
                                }

                                dbTrans.Commit();
                                dbTrans2.Commit();
                            }
                            catch (Exception ex)
                            {
                                dbTrans.Rollback();
                                dbTrans2.Rollback();
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Clear();
                                cmd.CommandText = @"INSERT INTO `log_error_upload_result`(TranCode,line,FileName,exceptionApp)
                                            SELECT @TranCode,@line,@FileName,@exceptionApp";
                                cmd.Parameters.Add(new MySqlParameter("@TranCode", MySqlDbType.VarChar) { Value = UploadBill.TranCode });
                                cmd.Parameters.Add(new MySqlParameter("@line", MySqlDbType.Int32) { Value = i });
                                cmd.Parameters.Add(new MySqlParameter("@FileName", MySqlDbType.VarChar) { Value = xFileName });
                                cmd.Parameters.Add(new MySqlParameter("@exceptionApp", MySqlDbType.VarChar) { Value = ex.Message.Substring(0, ex.Message.Length < 255 ? ex.Message.Length : 253) });
                                cmd.ExecuteNonQuery();
                            }
                            finally
                            {
                                dbTrans.Dispose();
                                dbTrans2.Dispose();
                                cmdx.CloseConnection();
                                cmdx2.CloseConnection();
                            }
                        }
                        BillAmount = 0;
                        policyNo = null;
                        PolicyID = -1;
                        BillingID = -1;
                        recurring_seq = -1;
                        approvalCode = null;
                        TranDesc = null;
                        i++;
                    }
                }
            }

            // cek data downlod, jika sudah nol maka data billingDownload pindahkan ke Backup Billing
            try
            {
                hitungUlang();
                _jbsDB.Database.OpenConnection();
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"SELECT `rowCountDownload` FROM `billing_download_summary` WHERE id=6;";
                var sumdata = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                if (sumdata <= 0)
                {
                    string[] files;
                    files = Directory.GetFiles(DirBilling, "BCAac*.xls", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        var FileBil = new FileInfo(files[0]);
                        FileBil.MoveTo(BackupFile + FileBil.Name.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _jbsDB.Database.CloseConnection();
            }
        }
        private void ResultMandiriAC(UploadResultBillingVM UploadBill)
        {
            string tmp, approvalCode, TranDesc, txfilename, policyNo, Period = "", ACno = "", acName = "";
            int? PolicyID = -1, BillingID = -1, recurring_seq = -1;
            int CycleDate = 0;
            DateTime DueDatePre = new DateTime(2000, 1, 1), BillDate = new DateTime(2000, 1, 1);
            decimal BillAmount = 0;
            txfilename = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName);
            bool isApprove;

            string xFileName = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName).ToLower() +
                Path.GetRandomFileName().Replace(".", "").Substring(0, 8).ToLower() + ".txt";

            // Simpan File yang diUpload ke File Backup
            using (var fileStream = new FileStream(BackupResult + xFileName, FileMode.Create))
            {
                UploadBill.FileBill.CopyToAsync(fileStream);
            }

            using (var reader = new StreamReader(UploadBill.FileBill.OpenReadStream()))
            {
                int i = 1;
                while (reader.Peek() >= 0)
                {
                    tmp = reader.ReadLine();
                    if (tmp.Length < 40) continue; // Jika karakter cma 40, skip karena akan error utk diolah
                    if (i<2) continue; // baca mulai baris ke 2

                    policyNo = tmp.Substring(590, 40).Trim();
                    approvalCode = tmp.Substring(674, 45).Trim();
                    TranDesc = tmp.Substring(720).Trim();
                    isApprove = (approvalCode.ToLower() == "success") ? true : false;

                    var cmdx = _jbsDB.Database;
                    cmdx.OpenConnection();
                    var cmdx2 = _life21.Database;
                    cmdx2.OpenConnection();

                    var cmd = cmdx.GetDbConnection().CreateCommand();
                    var cmd2 = cmdx2.GetDbConnection().CreateCommand();

                    using (var dbTrans = cmdx.BeginTransaction())// pake userDB hanya utk koneksi aja biar gak sama dgn transaction
                    {
                        using (var dbTrans2 = cmdx2.BeginTransaction())
                        {
                            try
                            {
                                cmd.Parameters.Clear();
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = @"FindPolisACGetBillSeq";
                                cmd.Parameters.Add(new MySqlParameter("@NoPolis", MySqlDbType.VarChar) { Value = policyNo });
                                using (var rd = cmd.ExecuteReader())
                                {
                                    while (rd.Read())
                                    {
                                        PolicyID = Convert.ToInt32(rd["policy_id"]);
                                        BillingID = Convert.ToInt32(rd["BillingID"]);
                                        recurring_seq = Convert.ToInt32(rd["recurring_seq"]);
                                        BillDate = Convert.ToDateTime(rd["BillingDate"]);
                                        DueDatePre = Convert.ToDateTime(rd["due_dt_pre"]);
                                        BillAmount = Convert.ToDecimal(rd["TotalAmount"]);

                                        Period = rd["PeriodeBilling"].ToString();
                                        CycleDate = Convert.ToInt32(rd["cycleDate"]);
                                        ACno = rd["acc_no"].ToString();
                                        acName = rd["acc_name"].ToString();
                                    }

                                    if (PolicyID < 1 || BillingID < 1 || recurring_seq < 1)
                                    {
                                        throw new Exception("Polis tidak ditemukan,mungkin billingnya tidak dalam status download atau terdapat kesalahan pada data textfile...");
                                    }
                                }

                                cmd.Parameters.Clear();
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = @"InsertTransactionBank;";
                                cmd.Parameters.Add(new MySqlParameter("@FileName", MySqlDbType.VarChar) { Value = xFileName });
                                cmd.Parameters.Add(new MySqlParameter("@Trancode", MySqlDbType.VarChar) { Value = UploadBill.TranCode }); // hardCode BCA AC
                                cmd.Parameters.Add(new MySqlParameter("@IsApprove", MySqlDbType.Bit) { Value = isApprove });
                                cmd.Parameters.Add(new MySqlParameter("@policyID", MySqlDbType.VarChar) { Value = PolicyID });
                                cmd.Parameters.Add(new MySqlParameter("@Seq", MySqlDbType.VarChar) { Value = recurring_seq });
                                cmd.Parameters.Add(new MySqlParameter("@IDBill", MySqlDbType.VarChar) { Value = BillingID });
                                cmd.Parameters.Add(new MySqlParameter("@approvalCode", MySqlDbType.VarChar) { Value = approvalCode });
                                cmd.Parameters.Add(new MySqlParameter("@BankID", MySqlDbType.Int32) { Value = 0 }); // Bukan BCA CC
                                cmd.Parameters.Add(new MySqlParameter("@ErrCode", MySqlDbType.VarChar) { Value = TranDesc });
                                var uid = cmd.ExecuteScalar().ToString();

                                if (isApprove) // jika transaksi d approve bank, ada flag approve di file
                                {// ============================ Proses Insert Received ===========================
                                    cmd2.Parameters.Clear();
                                    cmd2.CommandType = CommandType.StoredProcedure;
                                    cmd2.CommandText = @"ReceiptInsert";
                                    cmd2.Parameters.Add(new MySqlParameter("@BillingDate", MySqlDbType.Date) { Value = BillDate });
                                    cmd2.Parameters.Add(new MySqlParameter("@policy_id", MySqlDbType.Int32) { Value = PolicyID });
                                    cmd2.Parameters.Add(new MySqlParameter("@receipt_amount", MySqlDbType.Decimal) { Value = BillAmount });
                                    cmd2.Parameters.Add(new MySqlParameter("@Source_download", MySqlDbType.VarChar) { Value = "AC" });
                                    cmd2.Parameters.Add(new MySqlParameter("@recurring_seq", MySqlDbType.Int32) { Value = recurring_seq });
                                    cmd2.Parameters.Add(new MySqlParameter("@bank_acc_id", MySqlDbType.Int32) { Value = 1 });
                                    cmd2.Parameters.Add(new MySqlParameter("@due_dt_pre", MySqlDbType.Date) { Value = DueDatePre });
                                    var receiptID = cmd2.ExecuteScalar().ToString();

                                    // ============================ Proses Insert Pilis CC Transaction Life21 ===========================
                                    cmd2.Parameters.Clear();
                                    cmd2.CommandType = CommandType.StoredProcedure;
                                    cmd2.CommandText = @"InsertPolistransAC";
                                    cmd2.Parameters.Add(new MySqlParameter("@PolisID", MySqlDbType.Int32) { Value = PolicyID });
                                    cmd2.Parameters.Add(new MySqlParameter("@Transdate", MySqlDbType.Date) { Value = BillDate });
                                    cmd2.Parameters.Add(new MySqlParameter("@TransType", MySqlDbType.VarChar) { Value = "R" });
                                    cmd2.Parameters.Add(new MySqlParameter("@Seq", MySqlDbType.Int32) { Value = recurring_seq });
                                    cmd2.Parameters.Add(new MySqlParameter("@Amount", MySqlDbType.Decimal) { Value = BillAmount });
                                    cmd2.Parameters.Add(new MySqlParameter("@DueDatePre", MySqlDbType.Date) { Value = DueDatePre });
                                    cmd2.Parameters.Add(new MySqlParameter("@Period", MySqlDbType.VarChar) { Value = Period });
                                    cmd2.Parameters.Add(new MySqlParameter("@CycleDate", MySqlDbType.Int32) { Value = CycleDate });
                                    cmd2.Parameters.Add(new MySqlParameter("@BankID", MySqlDbType.Int32) { Value = 2 });
                                    cmd2.Parameters.Add(new MySqlParameter("@ACno", MySqlDbType.VarChar) { Value = ACno });
                                    cmd2.Parameters.Add(new MySqlParameter("@ACName", MySqlDbType.VarChar) { Value = acName });
                                    cmd2.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = receiptID });
                                    var CCTransID = cmd2.ExecuteScalar().ToString();

                                    // Update table billing
                                    cmd.Parameters.Clear();
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = @"UPDATE `billing` SET `IsDownload`=0,
			                                                `IsClosed`=1,
			                                                `status_billing`='P',
			                                                `status_billing_dateUpdate`=@tgl,
			                                                `paid_date`=@billDate,
                                                            Life21TranID=@TransactionID,
			                                                `ReceiptID`=@receiptID,
			                                                `PaymentTransactionID`=@uid
		                                                WHERE `BillingID`=@idBill;";
                                    cmd.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = DateTime.Now });
                                    cmd.Parameters.Add(new MySqlParameter("@billDate", MySqlDbType.DateTime) { Value = BillDate });
                                    cmd.Parameters.Add(new MySqlParameter("@TransactionID", MySqlDbType.Int32) { Value = CCTransID });
                                    cmd.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = receiptID });
                                    cmd.Parameters.Add(new MySqlParameter("@uid", MySqlDbType.VarChar) { Value = uid });
                                    cmd.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.Int32) { Value = BillingID });
                                    cmd.ExecuteNonQuery();

                                    // Update Polis Last Transaction
                                    cmd.Parameters.Clear();
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = @"UPDATE `policy_last_trans` AS pt
		                                                INNER JOIN `billing` AS bx ON bx.policy_id=pt.policy_Id
			                                                SET pt.BillingID=bx.BillingID,
			                                                pt.recurring_seq=bx.recurring_seq,
			                                                pt.due_dt_pre=bx.due_dt_pre,
			                                                pt.source=bx.Source_download,
			                                                pt.receipt_id=bx.`ReceiptID`,
			                                                pt.receipt_date=bx.BillingDate,
			                                                pt.bank_id=bx.BankIdDownload
		                                                WHERE pt.policy_Id=@policyID AND bx.BillingID=@idBill;";
                                    cmd.Parameters.Add(new MySqlParameter("@policyID", MySqlDbType.Int32) { Value = PolicyID });
                                    cmd.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.Int32) { Value = BillingID });
                                    cmd.ExecuteNonQuery();

                                }
                                else // jika transaksi d reject bank
                                {//billing hanya ganti flag download, kolom lain tetap sbg status terakhir
                                    cmd.CommandType = CommandType.Text;
                                    cmd.Parameters.Clear();
                                    cmd.CommandText = @"UPDATE `billing` SET IsDownload=0 WHERE `BillingID`=@billid";
                                    cmd.Parameters.Add(new MySqlParameter("@billid", MySqlDbType.Int32) { Value = BillingID });
                                    cmd.ExecuteNonQuery();
                                }

                                dbTrans.Commit();
                                dbTrans2.Commit();
                            }
                            catch (Exception ex)
                            {
                                dbTrans.Rollback();
                                dbTrans2.Rollback();
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Clear();
                                cmd.CommandText = @"INSERT INTO `log_error_upload_result`(TranCode,line,FileName,exceptionApp)
                                            SELECT @TranCode,@line,@FileName,@exceptionApp";
                                cmd.Parameters.Add(new MySqlParameter("@TranCode", MySqlDbType.VarChar) { Value = UploadBill.TranCode });
                                cmd.Parameters.Add(new MySqlParameter("@line", MySqlDbType.Int32) { Value = i });
                                cmd.Parameters.Add(new MySqlParameter("@FileName", MySqlDbType.VarChar) { Value = xFileName });
                                cmd.Parameters.Add(new MySqlParameter("@exceptionApp", MySqlDbType.VarChar) { Value = ex.Message.Substring(0, ex.Message.Length < 255 ? ex.Message.Length : 253) });
                                cmd.ExecuteNonQuery();
                            }
                            finally
                            {
                                dbTrans.Dispose();
                                dbTrans2.Dispose();
                                cmdx.CloseConnection();
                                cmdx2.CloseConnection();
                            }
                        }
                        BillAmount = 0;
                        policyNo = null;
                        PolicyID = -1;
                        BillingID = -1;
                        recurring_seq = -1;
                        approvalCode = null;
                        TranDesc = null;
                        i++;
                    }
                }
            }

            // cek data downlod, jika sudah nol maka data billingDownload pindahkan ke Backup Billing
            try
            {
                hitungUlang();
                _jbsDB.Database.OpenConnection();
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"SELECT `rowCountDownload` FROM `billing_download_summary` WHERE id=7;";
                var sumdata = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                if (sumdata <= 0)
                {
                    string[] files;
                    files = Directory.GetFiles(DirBilling, "MandiriAc*.csv", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        var FileBil = new FileInfo(files[0]);
                        FileBil.MoveTo(BackupFile + FileBil.Name.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _jbsDB.Database.CloseConnection();
            }
        }
        private void ResultAC(UploadResultBillingVM UploadBill)
        {
            string tmp, approvalCode, TranDesc, txfilename, policyNo="", Period = "", ACno = "", acName = "", BillOthers = "";
            int? PolicyID = -1, BillingID = -1, recurring_seq = -1;
            int CycleDate = 0;
            DateTime DueDatePre = new DateTime(2000, 1, 1), BillDate = new DateTime(2000, 1, 1);
            decimal BillAmount = 0;
            txfilename = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName);
            bool isApprove;

            // File Upload dalam bentuk txt
            string xFileName = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName).ToLower() +
                Path.GetRandomFileName().Replace(".", "").Substring(0, 8).ToLower() + ".txt";

            // Simpan File yang diUpload ke File Backup
            using (var fileStream = new FileStream(BackupResult + xFileName, FileMode.Create))
            {
                UploadBill.FileBill.CopyToAsync(fileStream);
            }

            using (var reader = new StreamReader(UploadBill.FileBill.OpenReadStream()))
            {
                int i = 1;
                while (reader.Peek() >= 0)
                {
                    tmp = reader.ReadLine();
                    if (tmp.Length < 40) continue; // Jika karakter cma 40, skip karena akan error utk diolah
                    if (i < 2) continue; // baca mulai baris ke 2


                    if (UploadBill.TranCode == "bcaac")
                    {
                        if (tmp.Substring(92, 1).Trim() == "A") BillOthers = tmp.Substring(92, 15).Trim();
                        else policyNo = tmp.Substring(92, 15).Trim();
                        //policyNo = tmp.Substring(92, 15).Trim();
                        approvalCode = tmp.Substring(129, 9).Trim();
                        TranDesc = tmp.Substring(138, 50).Trim();
                        isApprove = (approvalCode.ToLower() == "berhasil") ? true : false;
                    }
                    else if (UploadBill.TranCode == "mandiriac")
                    {
                        if (tmp.Substring(590, 1).Trim() == "A") BillOthers = tmp.Substring(590, 40).Trim();
                        else policyNo = tmp.Substring(590, 40).Trim();
                        //policyNo = tmp.Substring(590, 40).Trim();
                        approvalCode = tmp.Substring(674, 45).Trim();
                        TranDesc = tmp.Substring(720).Trim();
                        isApprove = (approvalCode.ToLower() == "success") ? true : false;
                    }else
                    {
                        throw new Exception("Transaksi AC, TranCode belum di defenisikan");
                    }

                    var cmdx = _jbsDB.Database;
                    cmdx.OpenConnection();
                    var cmdx2 = _life21.Database;
                    cmdx2.OpenConnection();

                    var cmd = cmdx.GetDbConnection().CreateCommand();
                    var cmd2 = cmdx2.GetDbConnection().CreateCommand();

                    using (var dbTrans = cmdx.BeginTransaction())// pake userDB hanya utk koneksi aja biar gak sama dgn transaction
                    {
                        using (var dbTrans2 = cmdx2.BeginTransaction())
                        {
                            try
                            {
                                cmd.Parameters.Clear();
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = @"FindPolisACGetBillSeq";
                                cmd.Parameters.Add(new MySqlParameter("@NoPolis", MySqlDbType.VarChar) { Value = policyNo });
                                using (var rd = cmd.ExecuteReader())
                                {
                                    while (rd.Read())
                                    {
                                        PolicyID = Convert.ToInt32(rd["policy_id"]);
                                        BillingID = Convert.ToInt32(rd["BillingID"]);
                                        recurring_seq = Convert.ToInt32(rd["recurring_seq"]);
                                        BillDate = Convert.ToDateTime(rd["BillingDate"]);
                                        DueDatePre = Convert.ToDateTime(rd["due_dt_pre"]);
                                        BillAmount = Convert.ToDecimal(rd["TotalAmount"]);

                                        Period = rd["PeriodeBilling"].ToString();
                                        CycleDate = Convert.ToInt32(rd["cycleDate"]);
                                        ACno = rd["acc_no"].ToString();
                                        acName = rd["acc_name"].ToString();
                                    }

                                    if (PolicyID < 1 || BillingID < 1 || recurring_seq < 1)
                                    {
                                        throw new Exception("Polis tidak ditemukan,mungkin billingnya tidak dalam status download atau terdapat kesalahan pada data textfile...");
                                    }
                                }

                                cmd.Parameters.Clear();
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = @"InsertTransactionBank;";
                                cmd.Parameters.Add(new MySqlParameter("@FileName", MySqlDbType.VarChar) { Value = xFileName });
                                cmd.Parameters.Add(new MySqlParameter("@Trancode", MySqlDbType.VarChar) { Value = UploadBill.TranCode }); // hardCode BCA AC
                                cmd.Parameters.Add(new MySqlParameter("@IsApprove", MySqlDbType.Bit) { Value = isApprove });
                                cmd.Parameters.Add(new MySqlParameter("@policyID", MySqlDbType.VarChar) { Value = PolicyID });
                                cmd.Parameters.Add(new MySqlParameter("@Seq", MySqlDbType.VarChar) { Value = recurring_seq });
                                cmd.Parameters.Add(new MySqlParameter("@IDBill", MySqlDbType.VarChar) { Value = BillingID });
                                cmd.Parameters.Add(new MySqlParameter("@approvalCode", MySqlDbType.VarChar) { Value = approvalCode });
                                cmd.Parameters.Add(new MySqlParameter("@BankID", MySqlDbType.Int32) { Value = 0 }); // Bukan BCA CC
                                cmd.Parameters.Add(new MySqlParameter("@ErrCode", MySqlDbType.VarChar) { Value = TranDesc });
                                var uid = cmd.ExecuteScalar().ToString();

                                if (isApprove) // jika transaksi d approve bank, ada flag approve di file
                                {// ============================ Proses Insert Received ===========================
                                    cmd2.Parameters.Clear();
                                    cmd2.CommandType = CommandType.StoredProcedure;
                                    cmd2.CommandText = @"ReceiptInsert";
                                    cmd2.Parameters.Add(new MySqlParameter("@BillingDate", MySqlDbType.Date) { Value = BillDate });
                                    cmd2.Parameters.Add(new MySqlParameter("@policy_id", MySqlDbType.Int32) { Value = PolicyID });
                                    cmd2.Parameters.Add(new MySqlParameter("@receipt_amount", MySqlDbType.Decimal) { Value = BillAmount });
                                    cmd2.Parameters.Add(new MySqlParameter("@Source_download", MySqlDbType.VarChar) { Value = "AC" });
                                    cmd2.Parameters.Add(new MySqlParameter("@recurring_seq", MySqlDbType.Int32) { Value = recurring_seq });
                                    cmd2.Parameters.Add(new MySqlParameter("@bank_acc_id", MySqlDbType.Int32) { Value = 1 });
                                    cmd2.Parameters.Add(new MySqlParameter("@due_dt_pre", MySqlDbType.Date) { Value = DueDatePre });
                                    var receiptID = cmd2.ExecuteScalar().ToString();

                                    // ============================ Proses Insert Pilis CC Transaction Life21 ===========================
                                    cmd2.Parameters.Clear();
                                    cmd2.CommandType = CommandType.StoredProcedure;
                                    cmd2.CommandText = @"InsertPolistransAC";
                                    cmd2.Parameters.Add(new MySqlParameter("@PolisID", MySqlDbType.Int32) { Value = PolicyID });
                                    cmd2.Parameters.Add(new MySqlParameter("@Transdate", MySqlDbType.Date) { Value = BillDate });
                                    cmd2.Parameters.Add(new MySqlParameter("@TransType", MySqlDbType.VarChar) { Value = "R" });
                                    cmd2.Parameters.Add(new MySqlParameter("@Seq", MySqlDbType.Int32) { Value = recurring_seq });
                                    cmd2.Parameters.Add(new MySqlParameter("@Amount", MySqlDbType.Decimal) { Value = BillAmount });
                                    cmd2.Parameters.Add(new MySqlParameter("@DueDatePre", MySqlDbType.Date) { Value = DueDatePre });
                                    cmd2.Parameters.Add(new MySqlParameter("@Period", MySqlDbType.VarChar) { Value = Period });
                                    cmd2.Parameters.Add(new MySqlParameter("@CycleDate", MySqlDbType.Int32) { Value = CycleDate });
                                    cmd2.Parameters.Add(new MySqlParameter("@BankID", MySqlDbType.Int32) { Value = 2 });
                                    cmd2.Parameters.Add(new MySqlParameter("@ACno", MySqlDbType.VarChar) { Value = ACno });
                                    cmd2.Parameters.Add(new MySqlParameter("@ACName", MySqlDbType.VarChar) { Value = acName });
                                    cmd2.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = receiptID });
                                    var CCTransID = cmd2.ExecuteScalar().ToString();

                                    // Update table billing
                                    cmd.Parameters.Clear();
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = @"UPDATE `billing` SET `IsDownload`=0,
			                                                `IsClosed`=1,
			                                                `status_billing`='P',
			                                                `status_billing_dateUpdate`=@tgl,
			                                                `paid_date`=@billDate,
                                                            Life21TranID=@TransactionID,
			                                                `ReceiptID`=@receiptID,
			                                                `PaymentTransactionID`=@uid
		                                                WHERE `BillingID`=@idBill;";
                                    cmd.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = DateTime.Now });
                                    cmd.Parameters.Add(new MySqlParameter("@billDate", MySqlDbType.DateTime) { Value = BillDate });
                                    cmd.Parameters.Add(new MySqlParameter("@TransactionID", MySqlDbType.Int32) { Value = CCTransID });
                                    cmd.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = receiptID });
                                    cmd.Parameters.Add(new MySqlParameter("@uid", MySqlDbType.VarChar) { Value = uid });
                                    cmd.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.Int32) { Value = BillingID });
                                    cmd.ExecuteNonQuery();

                                    // Update Polis Last Transaction
                                    cmd.Parameters.Clear();
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = @"UPDATE `policy_last_trans` AS pt
		                                                INNER JOIN `billing` AS bx ON bx.policy_id=pt.policy_Id
			                                                SET pt.BillingID=bx.BillingID,
			                                                pt.recurring_seq=bx.recurring_seq,
			                                                pt.due_dt_pre=bx.due_dt_pre,
			                                                pt.source=bx.Source_download,
			                                                pt.receipt_id=bx.`ReceiptID`,
			                                                pt.receipt_date=bx.BillingDate,
			                                                pt.bank_id=bx.BankIdDownload
		                                                WHERE pt.policy_Id=@policyID AND bx.BillingID=@idBill;";
                                    cmd.Parameters.Add(new MySqlParameter("@policyID", MySqlDbType.Int32) { Value = PolicyID });
                                    cmd.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.Int32) { Value = BillingID });
                                    cmd.ExecuteNonQuery();

                                }
                                else // jika transaksi d reject bank
                                {//billing hanya ganti flag download, kolom lain tetap sbg status terakhir
                                    cmd.CommandType = CommandType.Text;
                                    cmd.Parameters.Clear();
                                    cmd.CommandText = @"UPDATE `billing` SET IsDownload=0 WHERE `BillingID`=@billid";
                                    cmd.Parameters.Add(new MySqlParameter("@billid", MySqlDbType.Int32) { Value = BillingID });
                                    cmd.ExecuteNonQuery();
                                }

                                dbTrans.Commit();
                                dbTrans2.Commit();
                            }
                            catch (Exception ex)
                            {
                                dbTrans.Rollback();
                                dbTrans2.Rollback();
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Clear();
                                cmd.CommandText = @"INSERT INTO `log_error_upload_result`(TranCode,line,FileName,exceptionApp)
                                            SELECT @TranCode,@line,@FileName,@exceptionApp";
                                cmd.Parameters.Add(new MySqlParameter("@TranCode", MySqlDbType.VarChar) { Value = UploadBill.TranCode });
                                cmd.Parameters.Add(new MySqlParameter("@line", MySqlDbType.Int32) { Value = i });
                                cmd.Parameters.Add(new MySqlParameter("@FileName", MySqlDbType.VarChar) { Value = xFileName });
                                cmd.Parameters.Add(new MySqlParameter("@exceptionApp", MySqlDbType.VarChar) { Value = ex.Message.Substring(0, ex.Message.Length < 255 ? ex.Message.Length : 253) });
                                cmd.ExecuteNonQuery();
                            }
                            finally
                            {
                                dbTrans.Dispose();
                                dbTrans2.Dispose();
                                cmdx.CloseConnection();
                                cmdx2.CloseConnection();
                            }
                        }
                        BillAmount = 0;
                        policyNo = null;
                        PolicyID = -1;
                        BillingID = -1;
                        recurring_seq = -1;
                        approvalCode = null;
                        TranDesc = null;
                        i++;
                    }
                }
            }

            // cek data downlod, jika sudah nol maka data billingDownload pindahkan ke Backup Billing
            try
            {
                hitungUlang();
                _jbsDB.Database.OpenConnection();
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"SELECT `rowCountDownload` FROM `billing_download_summary` WHERE id=7;";
                var sumdata = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                if (sumdata <= 0)
                {
                    string[] files;
                    files = Directory.GetFiles(DirBilling, "MandiriAc*.csv", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        var FileBil = new FileInfo(files[0]);
                        FileBil.MoveTo(BackupFile + FileBil.Name.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _jbsDB.Database.CloseConnection();
            }
        }
    }
}