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
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using OfficeOpenXml;
using System.Globalization;

namespace CAF.JBS.Controllers
{
    public class BillingFileController : Controller
    {
        private readonly JbsDbContext _jbsDB;
        private readonly Life21DbContext _life21;
        private readonly Life21pDbContext _life21p;
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

        private FileSettings filesettings;
        //private IConfigurationRoot Configuration { get; set; }

        public BillingFileController(JbsDbContext context1, Life21DbContext context2, Life21pDbContext context4, UserDbContext context3)
        {
            filesettings = new FileSettings();
            _jbsDB = context1;
            _life21 = context2;
            _life21p = context4;
            _user = context3;

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
            DownloadBillVM.BillingSummary = (from cd in _jbsDB.BillingSummary
                                             select new BillingSummary()
                                             {
                                                 id = cd.id,
                                                 Judul = cd.Judul,
                                                 TotalCountDWD = cd.TotalCountDWD ?? 0,
                                                 TotalAmountDWD = cd.TotalAmountDWD ?? 0,
                                                 BillingAmountDWD = cd.BillingAmountDWD ?? 0,
                                                 BillingCountDWD = cd.BillingCountDWD ?? 0,
                                                 OthersAmountDWD = cd.OthersAmountDWD ?? 0,
                                                 OthersCountDWD = cd.OthersCountDWD ?? 0,
                                                 QuoteAmountDWD = cd.QuoteAmountDWD ?? 0,
                                                 QuoteCountDWD = cd.QuoteCountDWD ?? 0,
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
             * BCA harus paling atas, karena pengaruh untuk produk Flexy Link
            */

            //if (ModelState.IsValid) { /*return RedirectToAction("Index"); */ }
            // download file CC Billing
            if (dw.BcaCC || dw.MandiriCC || dw.MegaCC || dw.BniCC)
            {
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
                    GenMegaOnUsCCFile(); // MegaOn 3
                    GenMegaOffUsCCFile(2); // MegaOff 4 (<> 1,2,3)
                    GenMandiriCCFile(); // Mandiri 2
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
            return File(new FileStream(DirBilling + fileName, FileMode.Open), "application/octet-stream");
        }

        #region GenerateFileDownloadCC
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
            FileInfo FileName = new FileInfo(this.MegaOfUsccFile);
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
        #endregion

        #region GenerateFileDownloadAC
        protected void GenBcaAcFile()
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
        #endregion

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
                                        b.LastUploadDate=null,
                                        b.paid_date=null,
                                        b.ReceiptID=null,
                                        b.PaymentTransactionID=null,
                                        b.`BillingDate`= null,
                                        b.Source_download=null; ";
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"UPDATE `billing_download_summary` AS bs
                                    SET bs.`BillingCountDWD`=0,
                                    bs.`BillingAmountDWD`=0,
                                    bs.`OthersCountDWD`=0,
                                    bs.`OthersAmountDWD`=0,
                                    bs.`QuoteCountDWD`=0,
                                    bs.`QuoteAmountDWD`=0,
                                    bs.`TotalCountDWD`=0,
                                    bs.`TotalAmountDWD`=0; ";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"UPDATE `billing_others` as b 
                                        SET b.`IsDownload`=0,
                                        b.`BankIdDownload`=null,
                                        b.BankID_Source=null,
                                        b.LastUploadDate=null,
                                        b.IsClosed=0,
                                        b.status_billing='A',
                                        b.paid_date=null,
                                        b.ReceiptOtherID=null,
                                        b.PaymentTransactionID=null,
                                        b.`BillingDate`= null,
                                        b.Source_download=null; ";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"UPDATE `quote_billing` as b 
                                        SET b.`IsDownload`=0,
                                        b.`BankIdDownload`=null,
                                        b.BankID_Source=null,
                                        b.LastUploadDate=null,
                                        b.IsClosed=0,
                                        b.status='A',
                                        b.paid_dt=null,
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
            catch (Exception ex)
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
            string pesan = "", jenisTransaksi = "";
            // Proses cek jlh data yg didownload, jika 0 maka file yang sudah terbentuk harus di hapus
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CheckDownloadData";

            if (sourceDownload == "CC")
            {
                if (trancode == 1) //BCA CC
                {
                    jenisTransaksi = "BCA CC";
                    cmd.Parameters.Add(new MySqlParameter("@bankDWD", MySqlDbType.Int32) { Value = 1 });
                    cmd.Parameters.Add(new MySqlParameter("@bankSrc", MySqlDbType.Int32) { Value = 0 });
                    cmd.Parameters.Add(new MySqlParameter("@SrcDWD", MySqlDbType.VarChar) { Value = "CC" });
                }
                else if (trancode == 2) // Mandiri CC
                {
                    jenisTransaksi = "Mandiri CC";
                    cmd.Parameters.Add(new MySqlParameter("@bankDWD", MySqlDbType.Int32) { Value = 2 });
                    cmd.Parameters.Add(new MySqlParameter("@bankSrc", MySqlDbType.Int32) { Value = 2 });
                    cmd.Parameters.Add(new MySqlParameter("@SrcDWD", MySqlDbType.VarChar) { Value = "CC" });
                }
                else if (trancode == 3) //Mega On Us
                {
                    jenisTransaksi = "MegaOnUs CC";
                    cmd.Parameters.Add(new MySqlParameter("@bankDWD", MySqlDbType.Int32) { Value = 12 });
                    cmd.Parameters.Add(new MySqlParameter("@bankSrc", MySqlDbType.Int32) { Value = 12 });
                    cmd.Parameters.Add(new MySqlParameter("@SrcDWD", MySqlDbType.VarChar) { Value = "CC" });
                }
                else if (trancode == 4)// Mega Off Us
                {
                    jenisTransaksi = "MegaOffUs CC";
                    cmd.Parameters.Add(new MySqlParameter("@bankDWD", MySqlDbType.Int32) { Value = 12 });
                    cmd.Parameters.Add(new MySqlParameter("@bankSrc", MySqlDbType.Int32) { Value = 0 });
                    cmd.Parameters.Add(new MySqlParameter("@SrcDWD", MySqlDbType.VarChar) { Value = "CC" });
                }
                else if (trancode == 5) // BNI CC
                {
                    jenisTransaksi = "BNI CC";
                    cmd.Parameters.Add(new MySqlParameter("@bankDWD", MySqlDbType.Int32) { Value = 3 });
                    cmd.Parameters.Add(new MySqlParameter("@bankSrc", MySqlDbType.Int32) { Value = 0 });
                    cmd.Parameters.Add(new MySqlParameter("@SrcDWD", MySqlDbType.VarChar) { Value = "CC" });
                }

            }
            else if (sourceDownload == "AC")
            {
                if (trancode == 1) // BCA AC
                {
                    jenisTransaksi = "BCA AC";
                    cmd.Parameters.Add(new MySqlParameter("@bankDWD", MySqlDbType.Int32) { Value = 1 });
                    cmd.Parameters.Add(new MySqlParameter("@bankSrc", MySqlDbType.Int32) { Value = 0 });
                    cmd.Parameters.Add(new MySqlParameter("@SrcDWD", MySqlDbType.VarChar) { Value = "AC" });
                }
                else if (trancode == 2) // Mandiri AC
                {
                    jenisTransaksi = "Mandiri AC";
                    cmd.Parameters.Add(new MySqlParameter("@bankDWD", MySqlDbType.Int32) { Value = 2 });
                    cmd.Parameters.Add(new MySqlParameter("@bankSrc", MySqlDbType.Int32) { Value = 0 });
                    cmd.Parameters.Add(new MySqlParameter("@SrcDWD", MySqlDbType.VarChar) { Value = "AC" });
                }
            }

            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (!rd.HasRows)
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
        public ActionResult UploadResult(string TranCode)
        {
            string layout = "UploadResult";
            UploadResultBillingVM UploadBill = new UploadResultBillingVM();
            UploadBill.TranCode = TranCode;
            switch (TranCode)
            {
                case "bcacc": UploadBill.Description = "BCA CC"; break;
                case "mandiricc": UploadBill.Description = "Mandiri CC"; break;
                case "megaonus": UploadBill.Description = "MegaOnUs CC"; break;
                case "megaoffus": UploadBill.Description = "MegaOffUs CC"; break;
                case "bnicc": UploadBill.Description = "BNI CC"; break;
                case "bcaac": UploadBill.Description = "BCA AC"; break;
                case "mandiriac": UploadBill.Description = "Mandiri AC"; break;
                case "varealtime": UploadBill.Description = "Virtual Account Realtime Transaction"; break;
                case "vadaily": UploadBill.Description = "Virtual Account Daily Transaction"; break;
            }
            return View(layout, UploadBill);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadResult(string TranCode, [Bind("TranCode,FileBill")] UploadResultBillingVM UploadBill)
        {
            if (UploadBill.FileBill != null)
            { //validasi file manual
                if (UploadBill.TranCode == "bcacc")
                { //  Path.GetFileNameWithoutExtension(fullPath)
                    if (Path.GetExtension(UploadBill.FileBill.FileName.ToString().ToLower()) != ".txt")
                        ModelState.AddModelError("FileBill", "ResultFile BCA CC harus File .txt");

                    var str = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName.ToString().ToLower());
                    if (!(str.Substring(str.Length-1) =="a" || str.Substring(str.Length - 1) == "r"))
                    {
                        ModelState.AddModelError("FileBill", "ResultFile BCA CC, FileName harus berakhiran A atau R");
                    }
                }
                else if (UploadBill.TranCode == "megaonus")
                { //  Path.GetFileNameWithoutExtension(fullPath)
                    if (UploadBill.FileBill.FileName.ToString().ToLower().Substring(UploadBill.FileBill.FileName.Length - 12) != "s1.bret.xlsx")
                        ModelState.AddModelError("FileBill", "ResultFile harus File *s1.bret.xlsx");
                }
                else if (UploadBill.TranCode == "megaoffus")
                { //  Path.GetFileNameWithoutExtension(fullPath)
                    if (UploadBill.FileBill.FileName.ToString().ToLower().Substring(UploadBill.FileBill.FileName.Length - 12) != "s2.bret.xlsx")
                        ModelState.AddModelError("FileBill", "ResultFile harus File *s2.bret.xlsx");
                }
                else if (UploadBill.TranCode == "bcaac" || UploadBill.TranCode == "mandiriac" ||
                    UploadBill.TranCode == "vabcarealtime" || UploadBill.TranCode == "vabcadaily")
                {
                    if (Path.GetExtension(UploadBill.FileBill.FileName.ToString().ToLower()) != ".txt")
                        ModelState.AddModelError("FileBill", "ResultFile harus File .txt");
                }
                else
                {
                    if (Path.GetExtension(UploadBill.FileBill.FileName.ToString().ToLower()) != ".xlsx")
                        ModelState.AddModelError("FileBill", "ResultFile harus File .xlsx");
                }
                if (UploadBill.FileBill.Length < 1)
                {
                    ModelState.AddModelError("FileBill", "Data File kosong");
                }
            }

            // Jika data sudah valid  =====================
            if (ModelState.IsValid)
            {
                // Proses baca result Mandiri CC
                if (UploadBill.TranCode == "mandiricc") ResultCC(UploadBill);

                // Proses baca result MegaOnUs CC
                if (UploadBill.TranCode == "megaonus") ResultCC(UploadBill);

                // Proses baca result MegaOffUs CC
                if (UploadBill.TranCode == "megaoffus") ResultCC(UploadBill);

                // Proses baca result BNI CC
                if (UploadBill.TranCode == "bnicc") ResultCC(UploadBill);

                // Proses baca result bcaCC AC VA
                if ((UploadBill.TranCode == "bcacc") || 
                    (UploadBill.TranCode == "bcaac") || 
                    (UploadBill.TranCode == "mandiriac") || 
                    (UploadBill.TranCode == "vabcarealtime") || 
                    (UploadBill.TranCode == "vabcadaily"))
                    ResultTextFile(UploadBill);

                TempData["ModeUpload"] = UploadBill.TranCode;

                return RedirectToAction("SubmitUpload");
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

        [HttpGet]
        public ActionResult SubmitUpload()
        {
            if (TempData["ModeUpload"] == null)
            {
                TempData["terimaError"] = "Sesion sudah habis, Silahkan Upload ulang file result....";
            }

            SubmitUploadVM SubmitUpload = new SubmitUploadVM();
            List<StagingUploadVM> StagingUpload = new List<StagingUploadVM>();
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = @"CompareUploadDownload";
            try
            {
                _jbsDB.Database.OpenConnection();
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    StagingUpload.Add(new StagingUploadVM() { id= Convert.ToInt32(rd["id"]),
                        polisNo = rd["polisNo"].ToString(),
                        BillCode= rd["BillCode"].ToString(),
                        tgl=(rd["tgl"] == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(rd["tgl"]),
                        amount = Convert.ToDecimal(rd["amount"]),
                        IsSuccess =Convert.ToBoolean(rd["IsSuccess"]),
                        policy_id= rd["policy_id"].ToString(),
                        BillingID= rd["BillingID"].ToString(),
                        ReqSeq= (rd["recurring_seq"] == DBNull.Value) ? (int?)null : Convert.ToInt32(rd["recurring_seq"]),
                        billAmount= (rd["TotalAmount"] == DBNull.Value) ? (Decimal?)null : Convert.ToInt32(rd["TotalAmount"]),
                        Due_Date_Pre= (rd["due_dt_pre"] == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(rd["due_dt_pre"])
                    });
                }
                SubmitUpload.StagingUploadVM = StagingUpload;
                SubmitUpload.trancode= (string)TempData["ModeUpload"];
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _jbsDB.Database.CloseConnection();
            }

            ////// Cek detail summary
            SummaryData(ref SubmitUpload);

            return View(SubmitUpload);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitUpload([Bind("trancode")] SubmitUploadVM SubmitUpload)
        {
            var tglSekarang = DateTime.Now;
            var cmdT = _jbsDB.Database.GetDbConnection().CreateCommand();
            if (SubmitUpload.trancode == null)
            {
                    cmdT.CommandType = CommandType.Text;
                    cmdT.CommandText = @"DELETE FROM `stagingupload`";
                    try
                    {
                        cmdT.Connection.Open();
                        cmdT.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        cmdT.Connection.Close();
                    }
                TempData["terimaError"] = "Sesion sudah habis, Silahkan Upload ulang file result....";
                return RedirectToAction("SubmitUpload");
            }

            ///mulai eksekusi transaksi
            List<StagingUpload> StagingUploadx = new List<StagingUpload>();
            cmdT.CommandType = CommandType.Text;
            cmdT.CommandText = @"SELECT * FROM `stagingupload` su WHERE su.`Billid` IS NOT NULL AND su.`trancode`=@trcode;";
            cmdT.Parameters.Add(new MySqlParameter("@trcode", MySqlDbType.VarChar) { Value = SubmitUpload.trancode });
            try
            {
                cmdT.Connection.Open();
                var rd = cmdT.ExecuteReader();
                while (rd.Read())
                {
                    StagingUploadx.Add(new StagingUpload()
                    {
                        id = Convert.ToInt32(rd["id"]),
                        polisNo = rd["polisNo"].ToString(),
                        BillCode = rd["BillCode"].ToString(),
                        tgl = (rd["tgl"] == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(rd["tgl"]),
                        amount = Convert.ToDecimal(rd["amount"]),
                        IsSuccess = Convert.ToBoolean(rd["IsSuccess"]),
                        ApprovalCode = rd["ApprovalCode"].ToString(),
                        Description = rd["Description"].ToString(),
                        ACCno = rd["ACCno"].ToString(),
                        filename = rd["filename"].ToString(),
                        life21TranID = (rd["Life21TranID"] == DBNull.Value) ? (int?)null : Convert.ToInt32(rd["Life21TranID"]),
                        PolicyId = (rd["PolicyId"] == DBNull.Value) ? (int?)null : Convert.ToInt32(rd["PolicyId"]),
                        Billid = rd["Billid"].ToString(),
                        recurring_seq = (rd["seq"] == DBNull.Value) ? (int?)null : Convert.ToInt32(rd["seq"]),
                        due_dt_pre = (rd["due_date_pre"] == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(rd["due_date_pre"]),
                        trancode = SubmitUpload.trancode
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cmdT.Connection.Close();
            }

            /// Transaction===============================================================================
            var cmdx = _jbsDB.Database;  cmdx.SetCommandTimeout(60); // jbs
            var cmdx2 = _life21.Database;  cmdx2.SetCommandTimeout(60); // life21
            var cmdx3 = _life21p.Database;  cmdx3.SetCommandTimeout(60); //life21p

            var cmd = cmdx.GetDbConnection().CreateCommand(); // jbs
            var cmd2 = cmdx2.GetDbConnection().CreateCommand(); // life21
            var cmd3 = cmdx3.GetDbConnection().CreateCommand(); //life21p

            //TransactionBank tbank;
            Receipt Rcpt;
            ReceiptOther Rcpto;
            PolicyTransaction Life21Tran;
            //BillingOthersVM bom;
            foreach (var lst in StagingUploadx)
            {
                var BankID = 0;
                var TransactionBankID = 0;
                var ReciptID = 0;
                var ReciptOtherID = 0;
                var IDLife21Tran = 0;

                Rcpt = new Receipt();
                

                switch (lst.trancode)
                {
                    case "bcacc":
                        BankID = 1;
                        Rcpt.receipt_source = "CC";
                        break;
                    case "mandiricc":
                        BankID = 2;
                        Rcpt.receipt_source = "CC";
                        break;
                    case "megaonus":
                    case "megaoffus":
                        BankID = 12;
                        Rcpt.receipt_source = "CC";
                        break;
                    case "bnicc":
                        BankID = 3;
                        Rcpt.receipt_source = "CC";
                        break;
                }

                Life21Tran = new PolicyTransaction();
                if (Rcpt.receipt_source=="CC")
                {
                    Rcpt.receipt_date = tglSekarang;
                    Rcpt.receipt_policy_id = lst.PolicyId;
                    Rcpt.receipt_amount = lst.amount;
                    Rcpt.receipt_seq = lst.recurring_seq;
                    Rcpt.bank_acc_id = BankID;
                    Rcpt.due_date_pre = lst.due_dt_pre;

                    Life21Tran.policy_id = lst.PolicyId;
                    Life21Tran.transaction_dt = tglSekarang;
                    Life21Tran.recurring_seq = lst.recurring_seq;
                    Life21Tran.amount = lst.amount;
                    Life21Tran.Due_Date_Pre = lst.due_dt_pre;
                    Life21Tran.BankID = BankID;
                    Life21Tran.ACC_No = lst.ACCno;
                    Life21Tran.transaction_type = "R";
                    Life21Tran.idTran = lst.life21TranID;
                    Life21Tran.result_status = lst.ApprovalCode;
                    Life21Tran.Remark = lst.Description;
                }

                try
                {
                    cmdx.OpenConnection(); cmdx.BeginTransaction(); // jbs
                    cmdx2.OpenConnection(); cmdx2.BeginTransaction(); // life21
                    cmdx3.OpenConnection(); cmdx3.BeginTransaction(); //life21p

                    TransactionBankID = InsertTransactionBank(ref cmd, lst); // transaksi histori di JBS
                    if (lst.IsSuccess) // transaksi sukses
                    {
                        if (lst.BillCode == "Q")
                        { // untuk Billing Quote 
                            UpdateQuote(ref cmd3,tglSekarang,BankID,Convert.ToInt32(lst.Billid));
                        }else
                        {// transaksi sudah pasti bukan Quote
                            lst.TglSkrg = tglSekarang;
                            lst.PaymentTransactionID = TransactionBankID;

                            if (lst.BillCode == "B")
                            { // Recurring >> insert Receipt
                                ReciptID=InsertReceipt(ref cmd2,Rcpt);
                                Life21Tran.receipt_id = ReciptID;
                                IDLife21Tran = InsertCCTransaction(ref cmd2, Life21Tran);
                                lst.receipt_id = ReciptID;
                                lst.life21TranID = IDLife21Tran;

                                UpdateBillingJBS(ref cmd, lst);
                                UpdateLastTransJBS(ref cmd, lst);
                            }
                            else
                            { // Billing Others >> insert Receipt Other (pasti CC)
                                Rcpto = new ReceiptOther();
                                Rcpto.policy_id = lst.PolicyId;
                                Rcpto.receipt_amount = lst.amount;
                                Rcpto.receipt_date = tglSekarang;
                                Rcpto.bank_acc_id = BankID;
                                Rcpto.receipt_source = "CC";

                                ReciptOtherID = InsertReceiptOther(ref cmd2, Rcpto);
                                Life21Tran.receipt_other_id = ReciptOtherID;
                                lst.receipt_other_id = ReciptOtherID;

                                UpdateCCTransaction(ref cmd2, Life21Tran);
                                UpdateBillingOthersJBS(ref cmd,lst);
                            }
                        }
                    }
                    else // transaksi Gagal
                    {
                        BukaFlagDownloadBilling(ref cmd,lst.BillCode,lst.Billid);
                    }

                    cmdx.RollbackTransaction();
                    cmdx2.RollbackTransaction();
                    cmdx3.RollbackTransaction();
                }
                catch(Exception ex)
                {
                    cmdx.RollbackTransaction();
                    cmdx2.RollbackTransaction();
                    cmdx3.RollbackTransaction();
                    //throw new Exception(ex.Message);

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.CommandText = @"INSERT INTO `log_error_upload_result`(TranCode,line,FileName,exceptionApp)
                                        SELECT @TranCode,@line,@FileName,@exceptionApp";
                    cmd.Parameters.Add(new MySqlParameter("@TranCode", MySqlDbType.VarChar) { Value = SubmitUpload.trancode });
                    cmd.Parameters.Add(new MySqlParameter("@line", MySqlDbType.Int32) { Value = lst.id });
                    cmd.Parameters.Add(new MySqlParameter("@FileName", MySqlDbType.VarChar) { Value = lst.filename });
                    cmd.Parameters.Add(new MySqlParameter("@exceptionApp", MySqlDbType.VarChar) { Value = ex.Message.Substring(0, ex.Message.Length < 255 ? ex.Message.Length : 253) });
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    cmdx.CloseConnection();
                    cmdx2.CloseConnection();
                    cmdx3.CloseConnection();
                }

            } // end foreach (var lst in StagingUploadx)
            TempData["pesanSukses"] = "Upload File Sukses";
            return RedirectToAction("Index");
        }

        #region UploadFileResult

        // Proses baca File Result Bank dlm bentuk teks
        // BCACC, AC, VA
        private void ResultTextFile(UploadResultBillingVM UploadBill)
        {
            // Delete file stagingUpload dulu
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.Parameters.Clear();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"DELETE FROM `stagingupload`;";
            try
            {
                _jbsDB.Database.OpenConnection();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cmd.Dispose();
                _jbsDB.Database.CloseConnection();
            }

            string approvalCode="", TranDesc="", txfilename,
                policyNo = "",
                accNo = "",
                BillOthers = "";
            int Billqoute = -1;
            DateTime? trandate = null;
            decimal fileamount = 0; // amount dr file
            bool isApprove=false;
            txfilename = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName);

            string xFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName).ToLower() +
                Guid.NewGuid().ToString().Substring(0, 8) +"."+ Path.GetExtension(UploadBill.FileBill.FileName);
            // Simpan File yang diUpload ke File Backup
            using (var fileStream = new FileStream(BackupResult + xFileName, FileMode.Create))
            {
                UploadBill.FileBill.CopyTo(fileStream);
            }

            using (var reader = new StreamReader(UploadBill.FileBill.OpenReadStream()))
            {
                int i = 1;
                while (reader.Peek() >= 0)
                {
                    var tmp = reader.ReadLine();
                    if (UploadBill.TranCode == "bcacc")
                    {
                        policyNo = tmp.Substring(9, 25).Trim();
                        if (Decimal.TryParse(tmp.Substring(54, 9), out fileamount)) continue;
                        TranDesc= tmp.Substring(tmp.Length - 8).Substring(0, 6);
                        approvalCode = tmp.Substring(tmp.Length - 2);
                        if (approvalCode == "00") isApprove = true;
                    }
                    else if (UploadBill.TranCode == "bcaac")
                    {

                    }
                    else if (UploadBill.TranCode == "mandiriac")
                    {

                    }
                    else throw new Exception("Transaksi CC, TranCode belum di defenisikan");

                    if (policyNo.Substring(0, 1) == "A")
                    {
                        BillOthers = policyNo;
                        policyNo = "";
                    }
                    else if (policyNo.Substring(0, 1) == "X")
                    {
                        Billqoute = Convert.ToInt32(policyNo.Substring(1));
                        policyNo = "";
                    }

                    var billcode = (policyNo == "") ? ((Billqoute < 1) ? "A" : "Q") : "B";
                    var polisTran = (billcode == "B") ? policyNo : ((billcode == "A") ? BillOthers : Billqoute.ToString());

                    try
                    {
                        var baris = "0000" + i.ToString();
                        InsertStagingTable(Convert.ToInt32(baris.Substring(baris.Length - 5)), polisTran, billcode, trandate, fileamount, isApprove, approvalCode, TranDesc, UploadBill.TranCode, xFileName, accNo);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message + billcode);
                    }

                    trandate = null;
                    polisTran = "";
                    accNo = "";
                    fileamount = 0;
                    Billqoute = 0;
                    BillOthers = "";
                    policyNo = "";
                    approvalCode = null;
                    TranDesc = null;
                    isApprove = false;
                    i++;
                } // END while (reader.Peek() >= 0)
            } // END using (var reader = new StreamReader(UploadBill.FileBill.OpenReadStream()))
        }

        // proses baca file result dari bank dlm bentuk excel (xlsx only)
        // * Mandiri, Mega, BNI (CC Only)
        // Untuk yg format 2 sheet, sheet1=approve dan sheet2=reject
        // untuk yg 1 sheet, data approve -> yang memiliki approval code
        // Proses baca data adalah untuk memasukkan ke table staging
        private void ResultCC(UploadResultBillingVM UploadBill)
        {
            // Delete file stagingUpload dulu
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.Parameters.Clear();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"DELETE FROM `stagingupload`;";
            try
            {
                _jbsDB.Database.OpenConnection();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cmd.Dispose();
                _jbsDB.Database.CloseConnection();
            }

            string approvalCode, TranDesc, txfilename,
                policyNo = "",
                accNo="",
                BillOthers = "";
            int Billqoute = -1;
            DateTime? trandate=null;
            decimal fileamount = 0; // amount dr file
            bool isApprove;
            txfilename = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName);

            string xFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName).ToLower() + 
                Guid.NewGuid().ToString().Substring(0, 8) + "." + Path.GetExtension(UploadBill.FileBill.FileName);
            // Simpan File yang diUpload ke File Backup
            using (var fileStream = new FileStream(BackupResult + xFileName, FileMode.Create))
            {
                UploadBill.FileBill.CopyTo(fileStream);
            }

            byte[] file = System.IO.File.ReadAllBytes(BackupResult + xFileName);
            using (MemoryStream ms = new MemoryStream(file))
            {
                using (ExcelPackage package = new ExcelPackage(ms))
                {
                    ExcelWorkbook wb = package.Workbook;
                    if (UploadBill.TranCode != "bnicc" && wb.Worksheets.Count < 2) throw new Exception("File Result harus 2 Sheet");
                    for (int sht = 1; sht < 3; sht++) // looping sheet 1 & 2
                    {
                        long tmpa=0;
                        ExcelWorksheet ws = wb.Worksheets[sht];
                        for (int row = ws.Dimension.Start.Row; row <= ws.Dimension.End.Row; row++)
                        {
                            if (UploadBill.TranCode == "mandiricc")
                            {
                                if (sht == 1) // Sheet APPROVE
                                {
                                    if (ws.Cells[row, 6].Value == null) continue;
                                    if (! long.TryParse(ws.Cells[row, 6].Value.ToString().Trim().Substring(1), out tmpa)) continue;

                                    policyNo = ws.Cells[row, 6].Value.ToString().Trim();
                                    approvalCode = ws.Cells[row, 4].Value.ToString().Trim();
                                    TranDesc = ws.Cells[row, 5].Value.ToString().Trim();
                                    isApprove = true;
                                }
                                else // Sheet REJECT
                                {
                                    if (ws.Cells[row, 4].Value == null) continue;
                                    if (! long.TryParse(ws.Cells[row, 4].Value.ToString().Trim().Substring(1), out tmpa)) continue;

                                    policyNo = ws.Cells[row, 4].Value.ToString().Trim();
                                    approvalCode = ws.Cells[row, 5].Value.ToString().Trim();
                                    TranDesc = ws.Cells[row, 6].Value.ToString().Trim();
                                    isApprove = false;
                                }
                                if (! decimal.TryParse(ws.Cells[row, 3].Value.ToString().Trim(), out fileamount)) continue;
                            }
                            else if (UploadBill.TranCode == "megaonus" || UploadBill.TranCode == "megaoffus")
                            {
                                //cek no uruk di kolom 1
                                if ((ws.Cells[row, 1].Value == null) || // Nourut
                                    (ws.Cells[row, 2].Value == null) || // Deskripsi yg berisi no polis
                                    (ws.Cells[row, 3].Value == null) || // Amount
                                    (ws.Cells[row, 4].Value == null) || // transaction date
                                    (ws.Cells[row, 5].Value == null) || // Decline code
                                    (ws.Cells[row, 6].Value == null)) // Flaging
                                    continue;
                                if (!long.TryParse(ws.Cells[row, 1].Value.ToString().Trim(), out tmpa)) continue;
                                
                                var temp = ws.Cells[row, 2].Value.ToString().Trim();
                                policyNo = temp.Split('-').Last().Trim();
                                if (policyNo == "") continue;
                                if (!long.TryParse(policyNo.Substring(1), out tmpa)) continue;

                                approvalCode = ws.Cells[row, 5].Value.ToString().Trim();
                                TranDesc = ws.Cells[row, 6].Value.ToString().Trim();
                                isApprove = (sht == 1) ? true : false;

                                if (! decimal.TryParse(ws.Cells[row, 3].Value.ToString().Trim(), out fileamount)) continue;
                                DateTime time;
                                if(DateTime.TryParse(ws.Cells[row, 3].Value.ToString().Trim(), out time)) trandate=time;
                            }
                            else if (UploadBill.TranCode == "bnicc")
                            {
                                // cek no urut harus angka
                                if (!long.TryParse(ws.Cells[row, 1].Value.ToString().Trim(), out tmpa)) continue;
                                if (ws.Cells[row, 7].Value == null) continue;
                                //cek NoPolis, hlangkan 1 karakter dikiri dan konversi ke angka
                                if (! long.TryParse(ws.Cells[row, 7].Value.ToString().Trim().Substring(1), out tmpa)) continue;
                                // amount
                                if (!decimal.TryParse(ws.Cells[row, 8].Value.ToString().Trim(), out fileamount)) continue;

                                policyNo = ws.Cells[row, 7].Value.ToString().Trim();
                                approvalCode = ws.Cells[row, 9].Value.ToString().Trim();
                                TranDesc = ws.Cells[row, 10].Value.ToString().Trim();
                                isApprove = (approvalCode == "") ? false : true;
                                accNo = ws.Cells[row, 4].Value.ToString().Trim();
                                trandate = null;
                            } // END UploadBill.TranCode ==
                            else
                            {
                                throw new Exception("Transaksi CC, TranCode belum di defenisikan");
                            }

                            if (policyNo.Substring(0, 1) == "A")
                            {
                                BillOthers = policyNo;
                                policyNo = "";
                            }
                            else if (policyNo.Substring(0, 1) == "X")
                            {
                                Billqoute = Convert.ToInt32(policyNo.Substring(1));
                                policyNo = "";
                            }

                            var billcode = (policyNo == "") ? ((Billqoute < 1) ? "A" : "Q") : "B";
                            var polisTran = (billcode=="B") ? policyNo : ((billcode == "A") ? BillOthers : Billqoute.ToString());
                            try
                            {
                                var baris ="0000" + row.ToString();
                                InsertStagingTable(Convert.ToInt32(sht.ToString() + baris.Substring(baris.Length-5)),polisTran, billcode, trandate, fileamount, isApprove, approvalCode, TranDesc, UploadBill.TranCode, xFileName, accNo);
                            }
                            catch(Exception ex)
                            {
                                throw new Exception(ex.Message + billcode);
                            }

                            trandate = null;
                            polisTran = "";
                            accNo = "";
                            fileamount = 0;
                            Billqoute = 0;
                            BillOthers = "";
                            policyNo = "";
                            approvalCode = null;
                            TranDesc = null;
                            isApprove = false;
                        }// END for (row=ws.Dimension.Start.Row; row <= ws.Dimension.End.Row; row++)

                        if (UploadBill.TranCode == "bnicc") break; // BNI cma 1 Sheet (1x loop langsung break)
                    } // END for(int sht=0; sht < 2; sht++)
                } // END using (ExcelPackage package = new ExcelPackage(new FileInfo(xFileName)))
            }
        }

        private void ResultAC(UploadResultBillingVM UploadBill)
        {// File Result txt
            string tmp,
                approvalCode,
                TranDesc,
                txfilename,
                policyNo = "",
                Period = "",
                ACno = "",
                acName = "",
                BillOthers = "",
                fileBillSearch = "";
            int PolicyID = -1, BillingID = -1, recurring_seq = -1, Life21TranID = -1;
            int CycleDate = 0, bankID = 0, sumCode = 1;
            DateTime DueDatePre = new DateTime(2000, 1, 1), BillDate = new DateTime(2000, 1, 1);
            decimal BillAmount = 0, fileamount=0;
            txfilename = DateTime.Now.ToString("yyyyMMdd") + "_" + Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName).ToLower() + ".txt";
            bool isApprove;

            switch (UploadBill.TranCode)
            {
                case "bcaac":
                    bankID = 1;
                    fileBillSearch = "BCAac*.xls";
                    sumCode = 6;
                    break;
                case "mandiriac":
                    bankID = 2;
                    fileBillSearch = "MandiriAc*.csv";
                    sumCode = 7;
                    break;
            }

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
                int i = 0;
                long tempx;
                while (reader.Peek() >= 0)
                {
                    i++;
                    tmp = reader.ReadLine();
                    if (tmp.Length < 40) continue; // Jika karakter cma 40, skip karena akan error utk diolah
                    if (i < 2) continue; // baca mulai baris ke 2

                    if (UploadBill.TranCode == "bcaac")
                    {
                        var tempa = tmp.Substring(92, 15).Trim();
                        if (! long.TryParse(tempa.Substring(1), out tempx)) continue;
                        if (! decimal.TryParse(tmp.Substring(74, 17).Trim(), out fileamount)) continue;

                        if (tempa.Substring(0,1) == "A") BillOthers = tempa;
                        else policyNo = tempa;
                        approvalCode = tmp.Substring(129, 9).Trim();
                        TranDesc = tmp.Substring(138, 50).Trim();
                        isApprove = (approvalCode.ToLower() == "berhasil") ? true : false;
                    }
                    else if (UploadBill.TranCode == "mandiriac")
                    {
                        var tempa = tmp.Substring(590, 40).Trim();
                        if (! long.TryParse(tempa.Substring(1), out tempx)) continue;
                        if (! decimal.TryParse(tmp.Substring(633, 10).Trim(), out fileamount)) continue;

                        if (tempa.Substring(0, 1).Trim() == "A") BillOthers = tempa;
                        else policyNo = tempa;
                        approvalCode = tmp.Substring(674, 45).Trim();
                        TranDesc = tmp.Substring(720).Trim();
                        isApprove = (approvalCode.ToLower() == "success") ? true : false;
                    }
                    else
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

                                if (policyNo != "") // jika transaksi billing Recuring
                                {
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
                                            Life21TranID = rd["Life21TranID"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(rd["Life21TranID"]);
                                            BillOthers = "";
                                        }

                                        if (PolicyID < 1 || BillingID < 1 || recurring_seq < 1)
                                        {
                                            throw new Exception("Polis {"+policyNo+"} tidak ditemukan,mungkin billingnya tidak dalam status download atau terdapat kesalahan pada data file Upload...");
                                        }
                                    }
                                }
                                else if (BillOthers != "") // jika transaksi Billing Others
                                {
                                    cmd.CommandText = @"FindPolisBillOthersAC";
                                    cmd.Parameters.Add(new MySqlParameter("@BillOthersNo", MySqlDbType.VarChar) { Value = BillOthers });

                                    using (var rd = cmd.ExecuteReader())
                                    {
                                        while (rd.Read())
                                        {
                                            PolicyID = Convert.ToInt32(rd["policy_id"]);
                                            policyNo = rd["policy_no"].ToString();
                                            //BillOthers = rd["BillingID"].ToString();
                                            BillDate = Convert.ToDateTime(rd["BillingDate"]);
                                            BillAmount = Convert.ToDecimal(rd["TotalAmount"]);
                                            ACno = rd["acc_no"].ToString();
                                            acName = rd["acc_name"].ToString();
                                            Life21TranID = rd["Life21TranID"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(rd["Life21TranID"]);
                                        }

                                        if (PolicyID < 1)
                                        {
                                            throw new Exception("BillingOthers {" + BillOthers + "} tidak ditemukan,mungkin billingnya tidak dalam status download atau terdapat kesalahan pada data file Upload...");
                                        }
                                    }
                                }

                                cmd.Parameters.Clear();
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = @"InsertTransactionBank;";
                                cmd.Parameters.Add(new MySqlParameter("@FileName", MySqlDbType.VarChar) { Value = xFileName });
                                cmd.Parameters.Add(new MySqlParameter("@Trancode", MySqlDbType.VarChar) { Value = UploadBill.TranCode }); // hardCode BCA AC
                                cmd.Parameters.Add(new MySqlParameter("@IsApprove", MySqlDbType.Bit) { Value = isApprove });
                                cmd.Parameters.Add(new MySqlParameter("@policyID", MySqlDbType.VarChar) { Value = PolicyID });
                                cmd.Parameters.Add(new MySqlParameter("@Seq", MySqlDbType.VarChar) { Value = (BillOthers == "") ? recurring_seq : 0 });
                                cmd.Parameters.Add(new MySqlParameter("@IDBill", MySqlDbType.VarChar) { Value = (BillOthers == "") ? BillingID.ToString() : BillOthers });
                                cmd.Parameters.Add(new MySqlParameter("@amount", MySqlDbType.Decimal) { Value = fileamount });
                                cmd.Parameters.Add(new MySqlParameter("@approvalCode", MySqlDbType.VarChar) { Value = approvalCode });
                                cmd.Parameters.Add(new MySqlParameter("@BankID", MySqlDbType.Int32) { Value = 0 }); // Bukan BCA CC (jangan pake bankID)
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
                                    cmd2.Parameters.Add(new MySqlParameter("@recurring_seq", MySqlDbType.Int32) { Value = (BillOthers == "") ? recurring_seq : 0 });
                                    cmd2.Parameters.Add(new MySqlParameter("@bank_acc_id", MySqlDbType.Int32) { Value = bankID });
                                    cmd2.Parameters.Add(new MySqlParameter("@due_dt_pre", MySqlDbType.Date) { Value = (BillOthers == "") ? DueDatePre : BillDate });
                                    var receiptID = cmd2.ExecuteScalar().ToString();

                                    // ============================ Proses Insert/Update AC Transaction Life21 ===========================
                                    if (Life21TranID < 1)
                                    {
                                        cmd2.Parameters.Clear();
                                        cmd2.CommandType = CommandType.StoredProcedure;
                                        cmd2.CommandText = @"InsertPolistransAC";
                                        cmd2.Parameters.Add(new MySqlParameter("@PolisID", MySqlDbType.Int32) { Value = PolicyID });
                                        cmd2.Parameters.Add(new MySqlParameter("@Transdate", MySqlDbType.Date) { Value = BillDate });
                                        cmd2.Parameters.Add(new MySqlParameter("@TransType", MySqlDbType.VarChar) { Value = "R" });
                                        cmd2.Parameters.Add(new MySqlParameter("@Seq", MySqlDbType.Int32) { Value = (BillOthers == "") ? recurring_seq : 0 });
                                        cmd2.Parameters.Add(new MySqlParameter("@Amount", MySqlDbType.Decimal) { Value = BillAmount });
                                        cmd2.Parameters.Add(new MySqlParameter("@DueDatePre", MySqlDbType.Date) { Value = (BillOthers == "") ? DueDatePre : BillDate });
                                        cmd2.Parameters.Add(new MySqlParameter("@Period", MySqlDbType.VarChar) { Value = Period });
                                        cmd2.Parameters.Add(new MySqlParameter("@CycleDate", MySqlDbType.Int32) { Value = (BillOthers == "") ? CycleDate : 0 });
                                        cmd2.Parameters.Add(new MySqlParameter("@BankID", MySqlDbType.Int32) { Value = bankID });
                                        cmd2.Parameters.Add(new MySqlParameter("@ACno", MySqlDbType.VarChar) { Value = ACno });
                                        cmd2.Parameters.Add(new MySqlParameter("@ACName", MySqlDbType.VarChar) { Value = acName });
                                        cmd2.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = receiptID });
                                        Life21TranID = Convert.ToInt32(cmd2.ExecuteScalar());
                                    }
                                    else
                                    {
                                        cmd2.Parameters.Clear();
                                        cmd2.CommandType = CommandType.Text;
                                        cmd2.CommandText = @"UPDATE policy_ac_transaction
                                                                status_id=2,
	                                                            SET result_status=@rstStatus,
	                                                            Remark=@remark,
	                                                            receipt_id=@receiptID,
	                                                            update_dt=@tgl
                                                                WHERE `policy_ac_tran_id`=@id";
                                        cmd2.Parameters.Add(new MySqlParameter("@rstStatus", MySqlDbType.VarChar) { Value = approvalCode });
                                        cmd2.Parameters.Add(new MySqlParameter("@remark", MySqlDbType.VarChar) { Value = TranDesc });
                                        cmd2.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = receiptID });
                                        cmd2.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.Date) { Value = DateTime.Now });
                                        cmd2.Parameters.Add(new MySqlParameter("@id", MySqlDbType.Int32) { Value = Life21TranID });
                                        cmd.ExecuteNonQuery();
                                    }


                                    // Update table billing
                                    cmd.Parameters.Clear();
                                    cmd.CommandType = CommandType.Text;
                                    if (BillOthers == "")
                                    {
                                        cmd.CommandText = @"UPDATE `billing` SET `IsDownload`=0,
			                                                `IsClosed`=1,
			                                                `status_billing`='P',
			                                                `LastUploadDate`=@tgl,
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
			                                                `LastUploadDate`=@tgl,
			                                                `paid_date`=@billDate,
                                                            Life21TranID=@TransactionID,
			                                                `ReceiptID`=@receiptID,
			                                                `PaymentTransactionID`=@uid
		                                                WHERE `BillingID`=@idBill;";
                                        cmd.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.VarChar) { Value = BillOthers });
                                    }
                                    cmd.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = DateTime.Now });
                                    cmd.Parameters.Add(new MySqlParameter("@billDate", MySqlDbType.DateTime) { Value = BillDate });
                                    cmd.Parameters.Add(new MySqlParameter("@TransactionID", MySqlDbType.Int32) { Value = Life21TranID });
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
                                    cmd.CommandType = CommandType.Text;
                                    cmd.Parameters.Clear();
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
                        BillAmount = 0;
                        policyNo = "";
                        BillOthers = "";
                        PolicyID = -1;
                        BillingID = -1;
                        recurring_seq = -1;
                        approvalCode = null;
                        TranDesc = null;
                        Life21TranID = -1;
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
                cmd.CommandText = @"SELECT `rowCountDownload` FROM `billing_download_summary` WHERE id=@id;";
                cmd.Parameters.Add(new MySqlParameter("@id", MySqlDbType.Int32) { Value = sumCode });
                var sumdata = Convert.ToInt32(cmd.ExecuteScalar().ToString());

                if (sumdata <= 0)
                {
                    // Jika data download sudah semua dpt result, pindahkan file billing ke folder backup
                    string[] files;
                    files = Directory.GetFiles(DirBilling, fileBillSearch, SearchOption.TopDirectoryOnly);
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
        #endregion

        private void ResultVA(UploadResultBillingVM UploadBill)
        {
            string tmp,
                //approvalCode,
                TranDesc = "",
                txfilename,
                policyNo = "",
                Period = "",
                PaymentMeth = "",
                ACno = "",
                acName = "";
            //BillOthers = "",
            //fileBillSearch = "";
            int PolicyID = -1, BillingID = -1, recurring_seq = -1, Life21TranID = -1;
            //int CycleDate = 0, bankID = 0, sumCode = 1;
            DateTime DueDatePre = new DateTime(2000, 1, 1), BillDate = new DateTime(2000, 1, 1),tglTransaksi= new DateTime(2000, 1, 1);
            decimal BillAmount = 0;
            txfilename = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName);
            bool isApprove=true;


            // File Upload dalam bentuk txt
            string xFileName = DateTime.Now.ToString("yyyyMMdd")+ Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName).ToLower() +
                Path.GetRandomFileName().Replace(".", "").Substring(0, 8).ToLower() + ".txt";

            // Simpan File yang diUpload ke File Backup
            using (var fileStream = new FileStream(BackupResult + xFileName, FileMode.Create))
            {
                UploadBill.FileBill.CopyToAsync(fileStream);
            }

            // mulai baca file upload
            using (var reader = new StreamReader(UploadBill.FileBill.OpenReadStream()))
            {
                int i = 1,j=0;
                while (reader.Peek() >= 0)
                {
                    i++;
                    tmp = reader.ReadLine();
                    if (tmp.Length < 10) continue;
                    try
                    {
                        if (UploadBill.TranCode == "vabcarealtime")
                        {
                            // jika 5 karakter pertama tidak bisa di convert ke int
                            if (! int.TryParse(tmp.Substring(0, 5).Trim(),out j)) continue;
                            policyNo = tmp.Substring(11, 19).Trim();
                            if (!decimal.TryParse(tmp.Substring(113, 21).Trim(), out BillAmount)) continue;
                            tglTransaksi = DateTime.ParseExact(tmp.Substring(136, 19).Trim(), "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                            TranDesc = tmp.Substring(158).Trim();
                        }
                        else if (UploadBill.TranCode == "vabcadaily")
                        {
                            if (!int.TryParse(tmp.Substring(0, 6).Trim(), out j)) continue;
                            policyNo = tmp.Substring(8, 20).Trim();
                            if(! decimal.TryParse(tmp.Substring(56, 17).Trim(),out BillAmount)) continue;
                            tglTransaksi = DateTime.ParseExact(tmp.Substring(73, 19).Trim(), "dd/MM/yy  HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                            TranDesc = tmp.Substring(93).Trim();
                        }
                    }catch(Exception ex)
                    {
                        throw ex;
                    }

                    var cmdx = _jbsDB.Database;
                    cmdx.OpenConnection();
                    var cmdx2 = _life21.Database;
                    cmdx2.OpenConnection();

                    var cmd = cmdx.GetDbConnection().CreateCommand();
                    var cmd2 = cmdx2.GetDbConnection().CreateCommand();

                    using (var dbTrans = cmdx.BeginTransaction())
                    {
                        using (var dbTrans2 = cmdx2.BeginTransaction())
                        {
                            try
                            {
                                string isCheck="";
                                cmd.Parameters.Clear();
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = @"CheckTranVA";
                                cmd.Parameters.Add(new MySqlParameter("@polisNo", MySqlDbType.VarChar) { Value = policyNo });
                                cmd.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = tglTransaksi });
                                isCheck = (string)cmd.ExecuteScalar();
                                if (isCheck == "1") continue; // jika transaksi sudah pernah insert, jgn insert lg

                                cmd.Parameters.Clear();
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = @"FindPolisTranVA";
                                cmd.Parameters.Add(new MySqlParameter("@NoPolis", MySqlDbType.VarChar) { Value = policyNo });
                                using (var rd = cmd.ExecuteReader())
                                {
                                    while (rd.Read())
                                    {
                                        PolicyID = Convert.ToInt32(rd["policy_id"]);
                                        BillingID = Convert.ToInt32(rd["BillingID"]);
                                        //ACno=rd["ACC_NO"].ToString();
                                        //acName= rd["ACC_NAME"].ToString();
                                        PaymentMeth = rd["payment_method"].ToString().ToUpper();
                                        recurring_seq = Convert.ToInt32(rd["recurring_seq"]);
                                        DueDatePre = Convert.ToDateTime(rd["due_dt_pre"]);
                                        Period = rd["PeriodeBilling"].ToString();
                                        //Life21TranID = rd["Life21TranID"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(rd["Life21TranID"]);
                                        int.TryParse(rd["Life21TranID"].ToString().Trim(), out Life21TranID);
                                    }
                                    if (PolicyID < 1 )
                                    {
                                        throw new Exception("Billing dengan Polis {"+ policyNo+"} tidak ditemukan ");
                                    }
                                }

                                cmd.Parameters.Clear();
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = @"InsertTransactionBank;";
                                cmd.Parameters.Add(new MySqlParameter("@FileName", MySqlDbType.VarChar) { Value = xFileName });
                                cmd.Parameters.Add(new MySqlParameter("@Trancode", MySqlDbType.VarChar) { Value = UploadBill.TranCode }); 
                                cmd.Parameters.Add(new MySqlParameter("@IsApprove", MySqlDbType.Bit) { Value = isApprove });
                                cmd.Parameters.Add(new MySqlParameter("@policyID", MySqlDbType.VarChar) { Value = PolicyID });
                                cmd.Parameters.Add(new MySqlParameter("@Seq", MySqlDbType.VarChar) { Value = recurring_seq });
                                cmd.Parameters.Add(new MySqlParameter("@IDBill", MySqlDbType.VarChar) { Value = BillingID.ToString() });
                                cmd.Parameters.Add(new MySqlParameter("@approvalCode", MySqlDbType.VarChar) { Value = "-" }); // karena dari VA, gak pake approval code
                                cmd.Parameters.Add(new MySqlParameter("@BankID", MySqlDbType.Int32) { Value = 0 }); // Bukan BCA CC (jangan pake bankID)
                                cmd.Parameters.Add(new MySqlParameter("@ErrCode", MySqlDbType.VarChar) { Value = TranDesc });
                                var uid = cmd.ExecuteScalar().ToString();

                                cmd2.Parameters.Clear();
                                cmd2.CommandType = CommandType.StoredProcedure;
                                cmd2.CommandText = @"ReceiptInsert";
                                cmd2.Parameters.Add(new MySqlParameter("@BillingDate", MySqlDbType.Date) { Value = tglTransaksi });
                                cmd2.Parameters.Add(new MySqlParameter("@policy_id", MySqlDbType.Int32) { Value = PolicyID });
                                cmd2.Parameters.Add(new MySqlParameter("@receipt_amount", MySqlDbType.Decimal) { Value = BillAmount });
                                cmd2.Parameters.Add(new MySqlParameter("@Source_download", MySqlDbType.VarChar) { Value = "VA" });
                                cmd2.Parameters.Add(new MySqlParameter("@recurring_seq", MySqlDbType.Int32) { Value = recurring_seq });
                                cmd2.Parameters.Add(new MySqlParameter("@bank_acc_id", MySqlDbType.Int32) { Value = 1 }); // karena VA BCA
                                cmd2.Parameters.Add(new MySqlParameter("@due_dt_pre", MySqlDbType.Date) { Value = DueDatePre });
                                var receiptID = cmd2.ExecuteScalar().ToString();

                                if(PaymentMeth=="AC" )
                                {
                                    cmd2.Parameters.Clear();
                                    cmd2.CommandType = CommandType.Text;
                                    cmd2.CommandText = @"UPDATE `policy_ac_transaction` pa
                                                        SET pa.`status_id`=5 AND pa.`remark`='VA'
                                                        WHERE pa.`policy_id`=@policy_id AND pa.`status_id`=1 LIMIT 1";
                                }
                                else if (PaymentMeth == "CC" )
                                {
                                    cmd2.Parameters.Clear();
                                    cmd2.CommandType = CommandType.Text;
                                    cmd2.CommandText = @"UPDATE `policy_cc_transaction` pa
                                                        SET pa.`status_id`=5 AND pa.`remark`='VA'
                                                        WHERE pa.`policy_id`=@policy_id AND pa.`status_id`=1 LIMIT 1";
                                }
                                cmd2.Parameters.Add(new MySqlParameter("@policy_id", MySqlDbType.Int32) { Value = PolicyID });
                                cmd2.ExecuteNonQuery();

                                // Update table billing
                                cmd.Parameters.Clear();
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = @"UPDATE `billing` SET `IsDownload`=0,
			                                                `IsClosed`=1,
			                                                `status_billing`='P',
			                                                `LastUploadDate`=@tgl,
			                                                `paid_date`=@billDate,
                                                            Source_download='VA',
                                                            BankIdDownload=1,
			                                                `ReceiptID`=@receiptID,
			                                                `PaymentTransactionID`=@uid
		                                                WHERE `BillingID`=@idBill;";
                                cmd.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.Int32) { Value = BillingID });
                                cmd.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = DateTime.Now });
                                cmd.Parameters.Add(new MySqlParameter("@billDate", MySqlDbType.DateTime) { Value = tglTransaksi });
                                cmd.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = receiptID });
                                cmd.Parameters.Add(new MySqlParameter("@uid", MySqlDbType.VarChar) { Value = uid }); 
                                cmd.ExecuteNonQuery();

                                dbTrans.Commit();
                                dbTrans2.Commit();
                            }
                            catch(Exception ex)
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
                        }// end using (var dbTrans2 = cmdx2.BeginTransaction())
                    }// End using (var dbTrans = cmdx.BeginTransaction())

                    policyNo = "";
                    BillAmount = 0;
                    tglTransaksi = new DateTime(2000, 1, 1);
                    TranDesc = "";
                }// end while (reader.Peek() >= 0)
            } // end using (var reader = new StreamReader(UploadBill.FileBill.OpenReadStream()))
        }

        private void InsertStagingTable(int id, string polisno,string billcode,DateTime? tgl,decimal amount,Boolean isSukses,
            string appcode,string desc, string trancode, string filename, string accNo)
        {
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.Parameters.Clear();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = @"InsertStagingUpload";
            cmd.Parameters.Add(new MySqlParameter("@id", MySqlDbType.Int32) { Value = id });
            cmd.Parameters.Add(new MySqlParameter("@polis", MySqlDbType.VarChar) { Value = polisno });
            cmd.Parameters.Add(new MySqlParameter("@billCode", MySqlDbType.VarChar) { Value = billcode });
            cmd.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = tgl });
            cmd.Parameters.Add(new MySqlParameter("@amt", MySqlDbType.Decimal) { Value = amount });
            cmd.Parameters.Add(new MySqlParameter("@IsSukses", MySqlDbType.Bit) { Value = isSukses });
            cmd.Parameters.Add(new MySqlParameter("@appcode", MySqlDbType.VarChar) { Value = appcode.Trim()});
            cmd.Parameters.Add(new MySqlParameter("@description", MySqlDbType.VarChar) { Value = desc});
            cmd.Parameters.Add(new MySqlParameter("@trancode", MySqlDbType.VarChar) { Value = trancode});
            cmd.Parameters.Add(new MySqlParameter("@filename", MySqlDbType.VarChar) { Value = filename});
            cmd.Parameters.Add(new MySqlParameter("@AccNo", MySqlDbType.VarChar) { Value = accNo});

            try
            {
                _jbsDB.Database.OpenConnection();
                cmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cmd.Dispose();
                _jbsDB.Database.CloseConnection();
            }
        }

        private int InsertTransactionBank(ref System.Data.Common.DbCommand cm, StagingUpload tb)
        { // history JBS Transaksi
            cm.Parameters.Clear();
            cm.CommandType = CommandType.StoredProcedure;
            cm.CommandText = @"InsertTransactionBank;";
            cm.Parameters.Add(new MySqlParameter("@FileName", MySqlDbType.VarChar) { Value = tb.filename });
            cm.Parameters.Add(new MySqlParameter("@Trancode", MySqlDbType.VarChar) { Value = tb.trancode });
            cm.Parameters.Add(new MySqlParameter("@TranDate", MySqlDbType.VarChar) { Value = tb.tgl });
            cm.Parameters.Add(new MySqlParameter("@IsApprove", MySqlDbType.Bit) { Value = tb.IsSuccess });
            cm.Parameters.Add(new MySqlParameter("@policyID", MySqlDbType.Int32) { Value = tb.PolicyId});
            cm.Parameters.Add(new MySqlParameter("@IDBill", MySqlDbType.VarChar) { Value = tb.Billid});
            cm.Parameters.Add(new MySqlParameter("@amount", MySqlDbType.Decimal) { Value = tb.amount });
            cm.Parameters.Add(new MySqlParameter("@approvalCode", MySqlDbType.VarChar) { Value = tb.ApprovalCode });
            cm.Parameters.Add(new MySqlParameter("@ErrCode", MySqlDbType.VarChar) { Value = tb.Description });
            cm.Parameters.Add(new MySqlParameter("@AccNo", MySqlDbType.VarChar) { Value = tb.ACCno });
            var idTran = 0;
            try
            {
                var hasil = cm.ExecuteScalar();
                idTran = Convert.ToInt32(hasil);
            }
            catch (Exception ex)
            {
                throw new Exception("InsertTransactionBank => (BillingID = " + tb.Billid + ") " + ex.Message);
            }
            return idTran;

        }

        private void UpdateCCTransaction(ref System.Data.Common.DbCommand cm, PolicyTransaction pt)
        { // untuk yang billing Other
            cm.Parameters.Clear();
            cm.CommandType = CommandType.Text;
            cm.CommandText = @"UPDATE policy_cc_transaction
                                        SET status_id=2,
	                                    result_status=@rstStatus,
	                                    Remark=@remark,
	                                    receipt_other_id=@receiptID,
	                                    update_dt=@dtupdate
                                        WHERE `policy_cc_tran_id`=@id;";
            cm.Parameters.Add(new MySqlParameter("@rstStatus", MySqlDbType.VarChar) { Value = pt.result_status });
            cm.Parameters.Add(new MySqlParameter("@remark", MySqlDbType.VarChar) { Value = pt.Remark });
            cm.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = pt.receipt_other_id });
            cm.Parameters.Add(new MySqlParameter("@dtupdate", MySqlDbType.DateTime) { Value = pt.transaction_dt });
            cm.Parameters.Add(new MySqlParameter("@id", MySqlDbType.Int32) { Value = pt.idTran });
            cm.ExecuteNonQuery();
            try
            {
                cm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateCCTransaction => (policyid = " + pt.policy_id.ToString() + ") " + ex.Message);
            }
        }
        private int InsertCCTransaction(ref System.Data.Common.DbCommand cm,PolicyTransaction pt)
        {
            cm.Parameters.Clear();
            cm.CommandType = CommandType.StoredProcedure;
            cm.CommandText = @"InsertPolistransCC";
            cm.Parameters.Add(new MySqlParameter("@PolisID", MySqlDbType.Int32) { Value = pt.policy_id });
            cm.Parameters.Add(new MySqlParameter("@Transdate", MySqlDbType.Date) { Value = pt.transaction_dt });
            cm.Parameters.Add(new MySqlParameter("@Seq", MySqlDbType.Int32) { Value = pt.recurring_seq });
            cm.Parameters.Add(new MySqlParameter("@billType", MySqlDbType.VarChar) { Value = pt.transaction_type });
            cm.Parameters.Add(new MySqlParameter("@Amount", MySqlDbType.Decimal) { Value = pt.amount });
            cm.Parameters.Add(new MySqlParameter("@DueDatePre", MySqlDbType.Date) { Value = pt.Due_Date_Pre });
            cm.Parameters.Add(new MySqlParameter("@Period", MySqlDbType.VarChar) { Value = String.Format("{0:MMMdd}", pt.Due_Date_Pre) });
            cm.Parameters.Add(new MySqlParameter("@CycleDate", MySqlDbType.Int32) { Value = String.Format("{0:dd}", pt.Due_Date_Pre) });
            cm.Parameters.Add(new MySqlParameter("@BankID", MySqlDbType.Int32) { Value = pt.BankID });
            cm.Parameters.Add(new MySqlParameter("@CCno", MySqlDbType.VarChar) { Value = pt.ACC_No });
            cm.Parameters.Add(new MySqlParameter("@CCExpiry", MySqlDbType.VarChar) { Value = "" });
            cm.Parameters.Add(new MySqlParameter("@CCName", MySqlDbType.VarChar) { Value = "" });
            cm.Parameters.Add(new MySqlParameter("@CCAddrs", MySqlDbType.VarChar) { Value = "" });
            cm.Parameters.Add(new MySqlParameter("@CCtelp", MySqlDbType.VarChar) { Value = "" });
            cm.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = pt.receipt_id });
            cm.Parameters.Add(new MySqlParameter("@receiptOtherID", MySqlDbType.Int32) { Value = pt.receipt_other_id });

            var CCTransID = 0;
            try
            {
                CCTransID = Convert.ToInt32(cm.ExecuteScalar());
            }
            catch (Exception ex)
            {
                throw new Exception("InsertCCTransaction => (policyid = " + pt.policy_id.ToString() + ") " + ex.Message);
            }

            return CCTransID;
        }

        private int InsertReceipt(ref System.Data.Common.DbCommand cm,Receipt rc)
        {
            cm.Parameters.Clear();
            cm.CommandType = CommandType.StoredProcedure;
            cm.CommandText = @"ReceiptInsert";
            cm.Parameters.Add(new MySqlParameter("@BillingDate", MySqlDbType.Date) { Value = rc.receipt_date });
            cm.Parameters.Add(new MySqlParameter("@policy_id", MySqlDbType.Int32) { Value = rc.receipt_policy_id });
            cm.Parameters.Add(new MySqlParameter("@receipt_amount", MySqlDbType.Decimal) { Value = rc.receipt_amount });
            cm.Parameters.Add(new MySqlParameter("@Source_download", MySqlDbType.VarChar) { Value = rc.receipt_source });
            cm.Parameters.Add(new MySqlParameter("@recurring_seq", MySqlDbType.Int32) { Value = rc.receipt_seq });
            cm.Parameters.Add(new MySqlParameter("@bank_acc_id", MySqlDbType.Int32) { Value = rc.bank_acc_id });
            cm.Parameters.Add(new MySqlParameter("@due_dt_pre", MySqlDbType.Date) { Value = rc.due_date_pre });
            var receiptID = 0;
            try
            {
                receiptID = Convert.ToInt32(cm.ExecuteScalar());
            }
            catch (Exception ex)
            {
                throw new Exception("InsertReceipt => (policyid = " + rc.receipt_policy_id.ToString() + ") " + ex.Message);
            }
            return receiptID;
        }

        private int InsertReceiptOther(ref System.Data.Common.DbCommand cm, ReceiptOther ro)
        {
            cm.Parameters.Clear();
            cm.CommandType = CommandType.StoredProcedure;
            cm.CommandText = @"ReceiptOtherInsert_sp";
            cm.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = ro.receipt_date });
            cm.Parameters.Add(new MySqlParameter("@policy_id", MySqlDbType.Int32) { Value = ro.policy_id });
            cm.Parameters.Add(new MySqlParameter("@receipt_amount", MySqlDbType.Decimal) { Value = ro.receipt_amount });
            cm.Parameters.Add(new MySqlParameter("@Source_download", MySqlDbType.VarChar) { Value = ro.receipt_source });
            cm.Parameters.Add(new MySqlParameter("@bankid", MySqlDbType.Int32) { Value = ro.bank_acc_id });

            var receiptOther = 0;
            try
            {
                receiptOther = Convert.ToInt32(cm.ExecuteScalar());
            }
            catch (Exception ex)
            {
                throw new Exception("InsertReceiptOther => (policyid = " + ro.policy_id + ") " + ex.Message);
            }
            return receiptOther;
        }

        private void BukaFlagDownloadBilling(ref System.Data.Common.DbCommand cm, string billCode, string BillingID)
        { // hanya buka flag download, untuk transaksi Reject
            cm.Parameters.Clear();
            cm.CommandType = CommandType.Text;
            if (billCode == "B")
            {// Transaksi Billing Rucurring
                cm.CommandText = @"UPDATE `billing` SET IsDownload=0 WHERE `BillingID`=@billid";
                cm.Parameters.Add(new MySqlParameter("@billid", MySqlDbType.Int32) { Value = Convert.ToInt32(BillingID) });
            }
            else if (billCode == "Q")
            {// Transaksi Billing Quote
                cm.CommandText = @"UPDATE `quote_billing` SET IsDownload=0 WHERE `quote_id`=@billid";
                cm.Parameters.Add(new MySqlParameter("@billid", MySqlDbType.Int32) { Value = Convert.ToInt32(BillingID) });
            }
            else
            {// transaksi Billing Others
                cm.CommandText = @"UPDATE `billing_others` SET IsDownload=0 WHERE `BillingID`=@billid";
                cm.Parameters.Add(new MySqlParameter("@billid", MySqlDbType.VarChar) { Value = BillingID });
            }
            try
            {
                cm.ExecuteNonQuery();
            }catch(Exception ex)
            {
                throw new Exception("BukaFlagDownloadBilling => (BillingID = "+ BillingID+") " + ex.Message);
            }
        }

        private void UpdateQuote(ref System.Data.Common.DbCommand cm, DateTime tgl, int bankID, int QuoteID)
        { // Update Quote di Life21P
            try
            {
                cm.Parameters.Clear();
                cm.CommandText = @"UPDATE `quote` q
                                        SET q.`quote_status`='P',
                                        quote_submitted_dt=@tgl
                                        WHERE q.`quote_id`=@quoteID;";
                cm.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = tgl });
                cm.Parameters.Add(new MySqlParameter("@quoteID", MySqlDbType.Int32) { Value = QuoteID });
                cm.ExecuteNonQuery();

                cm.Parameters.Clear();
                cm.CommandText = @"UPDATE `prospect_billing`
                                        SET prospect_convert_flag=2,prospect_appr_code='UP4Y1',
                                        updated_dt=@tgl,
                                        acquirer_bank_id=@bankid
                                        WHERE `quote_id`=@quoteID;";
                cm.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = tgl });
                cm.Parameters.Add(new MySqlParameter("@bankid", MySqlDbType.Int32) { Value = bankID });
                cm.Parameters.Add(new MySqlParameter("@quoteID", MySqlDbType.Int32) { Value = QuoteID });
                cm.ExecuteNonQuery();

                cm.Parameters.Clear();
                cm.CommandText = @"UPDATE `quote_edc`
                                        SET status_id=1,
                                        reason='',
                                        appr_code='UP4Y1'
                                        WHERE `quote_id`=@quoteID;";
                cm.Parameters.Add(new MySqlParameter("@quoteID", MySqlDbType.Int32) { Value = QuoteID });
                cm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateQuote => (QuoteID = " + QuoteID.ToString() + ") " + ex.Message);
            }
        }

        private void UpdateBillingOthersJBS(ref System.Data.Common.DbCommand cm, StagingUpload bm)
        {
            cm.Parameters.Clear();
            cm.CommandType = CommandType.Text;
            cm.CommandText = @"UPDATE `billing_others` SET `IsDownload`=0,
			                                            `IsClosed`=1,
			                                            `status_billing`='P',
			                                            `LastUploadDate`=@tgl,
			                                            `paid_date`=@billDate,
                                                        Life21TranID=@TransactionID,
			                                            `ReceiptOtherID`=@receiptID,
			                                            `PaymentTransactionID`=@uid
		                                            WHERE `BillingID`=@idBill;";
            cm.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.VarChar) { Value = bm.Billid });
            cm.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = bm.TglSkrg });
            cm.Parameters.Add(new MySqlParameter("@billDate", MySqlDbType.DateTime) { Value = bm.TglSkrg });
            cm.Parameters.Add(new MySqlParameter("@TransactionID", MySqlDbType.Int32) { Value = bm.life21TranID });
            cm.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = bm.receipt_other_id });
            cm.Parameters.Add(new MySqlParameter("@uid", MySqlDbType.Int32) { Value = bm.PaymentTransactionID });
            try
            {
                cm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateBillingJBS => (BillingID = " + bm.Billid.ToString() + ") " + ex.Message);
            }
        }

        private void UpdateBillingJBS(ref System.Data.Common.DbCommand cm, StagingUpload bm)
        {/// update billing jadi closed
            cm.Parameters.Clear();
            cm.CommandType = CommandType.Text;
            cm.CommandText = @"UPDATE `billing` SET `IsDownload`=0,
			                                        `IsClosed`=1,
			                                        `status_billing`='P',
			                                        `LastUploadDate`=@tgl,
			                                        `paid_date`=@billDate,
                                                    Life21TranID=@TransactionID,
			                                        `ReceiptID`=@receiptID,
			                                        `PaymentTransactionID`=@uid
		                                        WHERE `BillingID`=@idBill;";
            cm.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.Int32) { Value = bm.Billid });
            cm.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = bm.TglSkrg });
            cm.Parameters.Add(new MySqlParameter("@billDate", MySqlDbType.DateTime) { Value = bm.TglSkrg });
            cm.Parameters.Add(new MySqlParameter("@TransactionID", MySqlDbType.Int32) { Value = bm.life21TranID });
            cm.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = bm.receipt_id });
            cm.Parameters.Add(new MySqlParameter("@uid", MySqlDbType.Int32) { Value = bm.PaymentTransactionID });
            try
            {
                cm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateBillingJBS => (BillingID = " + bm.Billid.ToString() + ") " + ex.Message);
            }
        }

        private void UpdateLastTransJBS(ref System.Data.Common.DbCommand cm, StagingUpload bm)
        {
            cm.Parameters.Clear();
            cm.CommandType = CommandType.Text;
            cm.CommandText = @"UPDATE `policy_last_trans` AS pt
		                                INNER JOIN `billing` AS bx ON bx.policy_id=pt.policy_Id
			                                SET pt.BillingID=bx.BillingID,
			                                pt.recurring_seq=bx.recurring_seq,
			                                pt.due_dt_pre=bx.due_dt_pre,
			                                pt.source=bx.Source_download,
			                                pt.receipt_id=bx.`ReceiptID`,
			                                pt.receipt_date=bx.BillingDate,
			                                pt.bank_id=bx.BankIdDownload
		                                WHERE pt.policy_Id=@policyID AND bx.BillingID=@idBill;";
            cm.Parameters.Add(new MySqlParameter("@policyID", MySqlDbType.Int32) { Value = bm.PolicyId });
            cm.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.Int32) { Value = bm.Billid });

            try
            {
                cm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateLastTransJBS => (BillingID = " + bm.Billid.ToString() + ") " + ex.Message);
            }
        }

        private void SummaryData(ref SubmitUploadVM stg)
        {
            string sql = @"SELECT bd.`BillingCountDWD`,bd.`BillingAmountDWD`,bd.`OthersCountDWD`,bd.`OthersAmountDWD`,
	                        bd.`QuoteCountDWD`,bd.`QuoteAmountDWD`,bd.`TotalCountDWD`,bd.`TotalAmountDWD`
                        FROM `billing_download_summary` bd 
                        WHERE bd.`trancode`=@trancode;";
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add(new MySqlParameter("@trancode", MySqlDbType.VarChar) { Value = stg.trancode });
            cmd.CommandText = sql;

            ///// Data download
            try
            {
                cmd.Connection.Open();
                using (var result = cmd.ExecuteReader())
                {
                    while (result.Read())
                    {
                        stg.BCountDw = Convert.ToInt32(result["BillingCountDWD"]);
                        stg.BSumDw = Convert.ToDecimal(result["BillingAmountDWD"]);
                        stg.ACountDw = Convert.ToInt32(result["OthersCountDWD"]);
                        stg.ASumDw = Convert.ToDecimal(result["OthersAmountDWD"]);
                        stg.QCountDw = Convert.ToInt32(result["QuoteCountDWD"]);
                        stg.QSumDw = Convert.ToDecimal(result["QuoteAmountDWD"]);
                        stg.TCountDownload = Convert.ToInt32(result["TotalCountDWD"]);
                        stg.TSumDownload = Convert.ToDecimal(result["TotalAmountDWD"]);
                    }
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally { cmd.Connection.Close(); }

            ///////// Hitung total Upload
            sql = @"SELECT COUNT(1) AS TotalUpload,SUM(su.`amount`) AS totalAmount
                    FROM `stagingupload` su WHERE su.`trancode`=@trancode;";
            cmd.CommandText = sql;
            try
            {
                cmd.Connection.Open();
                using (var result = cmd.ExecuteReader())
                {
                    while (result.Read())
                    {
                        stg.CountUpload = (result["TotalUpload"] == DBNull.Value) ?0:Convert.ToInt32(result["TotalUpload"]);
                        stg.SumUpload = (result["totalAmount"] == DBNull.Value) ? 0 : Convert.ToDecimal(result["totalAmount"]);
                    }
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally { cmd.Connection.Close(); }

            ///////// Hitung total Approve
            sql = @"SELECT COUNT(1) AS TotalUpload,SUM(su.`amount`) AS totalAmount
                    FROM `stagingupload` su WHERE su.`trancode`=@trancode AND su.`IsSuccess`=1;";
            cmd.CommandText = sql;
            try
            {
                cmd.Connection.Open();
                using (var result = cmd.ExecuteReader())
                {
                    while (result.Read())
                    {
                        stg.CountApprove = (result["TotalUpload"] == DBNull.Value) ? 0 : Convert.ToInt32(result["TotalUpload"]);
                        stg.SumApprove = (result["totalAmount"] == DBNull.Value) ? 0 : Convert.ToDecimal(result["totalAmount"]);
                    }
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally { cmd.Connection.Close(); }

            ///////// Hitung total Reject
            sql = @"SELECT COUNT(1) AS TotalUpload,SUM(su.`amount`) AS totalAmount
                    FROM `stagingupload` su WHERE su.`trancode`=@trancode AND su.`IsSuccess`=0;";
            cmd.CommandText = sql;
            try
            {
                cmd.Connection.Open();
                using (var result = cmd.ExecuteReader())
                {
                    while (result.Read())
                    {
                        stg.CountReject = (result["TotalUpload"] == DBNull.Value) ? 0 : Convert.ToInt32(result["TotalUpload"]);
                        stg.SumReject = (result["totalAmount"] == DBNull.Value) ? 0 : Convert.ToDecimal(result["totalAmount"]);
                    }
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally { cmd.Connection.Close(); }

            /////////// Hitung Total Upload 
            sql = @"SELECT su.`BillCode`, COUNT(1) AS jlhUpload, SUM(su.`amount`) AS totalAmount
                    FROM `stagingupload` su 
                    WHERE su.`trancode`=@trancode
                    GROUP BY su.`BillCode`;";
            cmd.CommandText = sql;
            try
            {
                cmd.Connection.Open();
                using (var result = cmd.ExecuteReader())
                {
                    while (result.Read())
                    {
                        if (result["BillCode"].ToString() == "A")
                        {
                            stg.ACountUp = (result["jlhUpload"] == DBNull.Value) ? 0 : Convert.ToInt32(result["jlhUpload"]);
                            stg.ASumUp = (result["totalAmount"] == DBNull.Value) ? 0 : Convert.ToDecimal(result["totalAmount"]);
                        }else if (result["BillCode"].ToString() == "B")
                        {
                            stg.BCountUp = (result["jlhUpload"] == DBNull.Value) ? 0 : Convert.ToInt32(result["jlhUpload"]);
                            stg.BSumUp = (result["totalAmount"] == DBNull.Value) ? 0 : Convert.ToDecimal(result["totalAmount"]);
                        }
                        else if (result["BillCode"].ToString() == "Q")
                        {
                            stg.QCountUp = (result["jlhUpload"] == DBNull.Value) ? 0 : Convert.ToInt32(result["jlhUpload"]);
                            stg.QSumUp = (result["totalAmount"] == DBNull.Value) ? 0 : Convert.ToDecimal(result["totalAmount"]);
                        }

                    }
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally { cmd.Connection.Close(); }

            ////// Hitung Jlh Approve
            sql = @"SELECT su.`BillCode`, COUNT(1) AS jlhUpload, SUM(su.`amount`) AS totalAmount
                        FROM `stagingupload` su 
                        WHERE su.`trancode`=@trancode AND su.`IsSuccess`=1
                        GROUP BY su.`BillCode`;";
            cmd.CommandText = sql;
            try
            {
                cmd.Connection.Open();
                using (var result = cmd.ExecuteReader())
                {
                    while (result.Read())
                    {
                        if (result["BillCode"].ToString() == "A")
                        {
                            stg.ACountUpAp = (result["jlhUpload"] == DBNull.Value) ? 0 : Convert.ToInt32(result["jlhUpload"]);
                            stg.ASumUpAp = (result["totalAmount"] == DBNull.Value) ? 0 : Convert.ToDecimal(result["totalAmount"]);
                        }
                        else if (result["BillCode"].ToString() == "B")
                        {
                            stg.BCountUpAp = (result["jlhUpload"] == DBNull.Value) ? 0 : Convert.ToInt32(result["jlhUpload"]);
                            stg.BSumUpAp = (result["totalAmount"] == DBNull.Value) ? 0 : Convert.ToDecimal(result["totalAmount"]);
                        }
                        else if (result["BillCode"].ToString() == "Q")
                        {
                            stg.QCountUpAp = (result["jlhUpload"] == DBNull.Value) ? 0 : Convert.ToInt32(result["jlhUpload"]);
                            stg.QSumUpAp = (result["totalAmount"] == DBNull.Value) ? 0 : Convert.ToDecimal(result["totalAmount"]);
                        }

                    }
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally { cmd.Connection.Close(); }

            ////// Hitung Jlh Reject
            sql = @"SELECT su.`BillCode`, COUNT(1) AS jlhUpload, SUM(su.`amount`) AS totalAmount
                        FROM `stagingupload` su 
                        WHERE su.`trancode`=@trancode AND su.`IsSuccess`=0
                        GROUP BY su.`BillCode`;";
            cmd.CommandText = sql;
            try
            {
                cmd.Connection.Open();
                using (var result = cmd.ExecuteReader())
                {
                    while (result.Read())
                    {
                        if (result["BillCode"].ToString() == "A")
                        {
                            stg.ACountUpRj = (result["jlhUpload"] == DBNull.Value) ? 0 : Convert.ToInt32(result["jlhUpload"]);
                            stg.ASumUpRj = (result["totalAmount"] == DBNull.Value) ? 0 : Convert.ToDecimal(result["totalAmount"]);
                        }
                        else if (result["BillCode"].ToString() == "B")
                        {
                            stg.BCountUpRj = (result["jlhUpload"] == DBNull.Value) ? 0 : Convert.ToInt32(result["jlhUpload"]);
                            stg.BSumUpRj = (result["totalAmount"] == DBNull.Value) ? 0 : Convert.ToDecimal(result["totalAmount"]);
                        }
                        else if (result["BillCode"].ToString() == "Q")
                        {
                            stg.QCountUpRj = (result["jlhUpload"] == DBNull.Value) ? 0 : Convert.ToInt32(result["jlhUpload"]);
                            stg.QSumUpRj = (result["totalAmount"] == DBNull.Value) ? 0 : Convert.ToDecimal(result["totalAmount"]);
                        }

                    }
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally { cmd.Connection.Close(); }


            //try
            //{
            //    cmd.Connection.Open();
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message);
            //}
            //finally
            //{
            //    cmd.Connection.Close();
            //}

        }
    }
}