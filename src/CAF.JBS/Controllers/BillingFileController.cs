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
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;

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
        private readonly string EmailCAF, EmailPHS, EmailFA, EmailCS, EmailBilling;

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

            var builder = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");
            var Configuration = builder.Build();

            EmailCAF = Configuration.GetValue<string>("Email:EmailCAF");
            EmailPHS = Configuration.GetValue<string>("Email:EmailPHS");
            EmailFA = Configuration.GetValue<string>("Email:EmailFA");
            EmailCS = Configuration.GetValue<string>("Email:EmailCS");
            EmailBilling = Configuration.GetValue<string>("Email:EmailBilling");
        }

        [HttpGet]
        public ActionResult Index()
        {
            var tglSistem = CekJobJalan().Value;
            if (tglSistem == null) return View("error");
            if (tglSistem.Date < DateTime.Now.Date) return View("error");

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
            //if (dw.BcaRegularPremium) GenVA();

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

        public ActionResult DeleteFile(string Filename)
        {
            FileInfo filex = new FileInfo(DirBilling + Filename);
            if (filex.Exists) System.IO.File.Delete(filex.ToString());
            return RedirectToAction("Index");
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

                try
                {
                    UpdateDataFileBilling(2, FileName.Name);
                }
                catch (Exception ex)
                {
                    throw ex;
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

                try
                {
                    UpdateDataFileBilling(4, FileName.Name);
                }
                catch (Exception ex)
                {
                    throw ex;
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

                try
                {
                    UpdateDataFileBilling(5, FileName.Name);
                }
                catch (Exception ex)
                {
                    throw ex;
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

                    try
                    {
                        UpdateDataFileBilling(6, FileName.Name);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
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

        public FileStreamResult DownloadVA()
        {
            string[] files = Directory.GetFiles(DirResult, "VARegulerPremi*.xlsx", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                FileInfo FileName = new FileInfo(file);
                if (FileName.Exists) System.IO.File.Delete(FileName.ToString());
            }
            var fileName = "VARegulerPremi" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
            var fullePath = DirResult + fileName;

            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.Parameters.Clear();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = @"GenVARegulerPremi_sp";

            using (ExcelPackage package = new ExcelPackage(new FileInfo(fullePath)))
            {
                var sheet = package.Workbook.Worksheets.Add("sheet1");

                try
                {
                    cmd.Connection.Open();
                    using (var result = cmd.ExecuteReader())
                    {
                        sheet.Cells[1, 1].Value = "No Polis";
                        sheet.Cells[1, 2].Value = "Pemegang Polis";
                        sheet.Cells[1, 3].Value = "Premi";

                        var i = 2;
                        while (result.Read())
                        {
                            sheet.Cells[i, 1].Value = result[0].ToString();
                            sheet.Cells[i, 2].Value = result[1].ToString();
                            sheet.Cells[i, 3].Value = result[2].ToString();
                            i++;
                        }
                        sheet.Column(1).AutoFit();
                        sheet.Column(2).AutoFit();
                        sheet.Column(3).AutoFit();
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
                package.Save();
            }
            var mimeType = "application/vnd.ms-excel";
            return File(new FileStream(fullePath, FileMode.Open), mimeType, fileName);
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
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
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
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
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
                cmd.Connection.Close();
            }
            return pesan;
        }

        private void UpdateDataFileBilling(int id, string Filename)
        {
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.Parameters.Clear();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"
UPDATE `FileNextProcess` fl
SET fl.`FileBilling`=@filename
WHERE fl.`id`=@id";
            cmd.Parameters.Add(new MySqlParameter("@filename", MySqlDbType.VarChar) { Value = Filename });
            cmd.Parameters.Add(new MySqlParameter("@id", MySqlDbType.Int32) { Value = id });
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
                cmd.Connection.Close();
            }
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
                var cmdT = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmdT.CommandType = CommandType.Text;
                cmdT.CommandText = @"DELETE FROM `stagingupload`";
                try
                {
                    if (cmdT.Connection.State == ConnectionState.Closed) cmdT.Connection.Open();
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

                if (UploadBill.TranCode == "bcacc")
                { //  Path.GetFileNameWithoutExtension(fullPath)
                    if (Path.GetExtension(UploadBill.FileBill.FileName.ToString().ToLower()) != ".txt")
                        ModelState.AddModelError("FileBill", "ResultFile BCA CC harus File .txt");

                    var str = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName.ToString().ToLower());
                    if (!(str.Substring(str.Length - 1) == "a" || str.Substring(str.Length - 1) == "r"))
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
                    UploadBill.TranCode == "varealtime" || UploadBill.TranCode == "vadaily")
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
                    (UploadBill.TranCode == "varealtime") ||
                    (UploadBill.TranCode == "vadaily"))
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
            SubmitUpload.trancode = (string)TempData["ModeUpload"];
            List<StagingUploadVM> StagingUpload = new List<StagingUploadVM>();
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            _jbsDB.Database.OpenConnection();
            cmd.Parameters.Clear();
            if ((SubmitUpload.trancode == "varealtime") || (SubmitUpload.trancode == "vadaily"))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"
                                DELETE su
                                FROM `stagingupload` su
                                INNER JOIN `policy_billing` pb ON pb.`policy_no`=su.`polisNo`
                                INNER JOIN `billing` b ON b.`policy_id`=pb.`policy_Id`
                                WHERE b.`paid_date`=su.`tgl`";
                cmd.ExecuteNonQuery();

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = @"SubmitUploadVA";
            }
            else
                cmd.CommandText = @"CompareUploadDownload";
            try
            {

                var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    StagingUpload.Add(new StagingUploadVM()
                    {
                        id = Convert.ToInt32(rd["id"]),
                        polisNo = rd["polisNo"].ToString(),
                        BillCode = rd["BillCode"].ToString(),
                        tgl = (rd["tgl"] == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(rd["tgl"]),
                        amount = Convert.ToDecimal(rd["amount"]),
                        StatusPolis = rd["StatusPolis"].ToString(),
                        IsSuccess = Convert.ToBoolean(rd["IsSuccess"]),
                        PolicyId = (rd["policy_id"] == DBNull.Value) ? (int?)null : Convert.ToInt32(rd["policy_id"]),
                        BillingID = rd["BillingID"].ToString(),
                        ReqSeq = (rd["recurring_seq"] == DBNull.Value) ? (int?)null : Convert.ToInt32(rd["recurring_seq"]),
                        billAmount = (rd["TotalAmount"] == DBNull.Value) ? (Decimal?)null : Convert.ToInt32(rd["TotalAmount"]),
                        Due_Date_Pre = (rd["due_dt_pre"] == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(rd["due_dt_pre"])
                    });
                }
                SubmitUpload.StagingUploadVM = StagingUpload;

            }
            catch (Exception ex)
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
        public async Task<IActionResult> SubmitUpload([Bind("trancode")] SubmitUploadVM SubmitUpload)
        {
            var tglSekarang = DateTime.Now;
            var cmdT = _jbsDB.Database.GetDbConnection().CreateCommand();

            ///mulai eksekusi transaksi
            List<StagingUploadVM> StagingUploadx = new List<StagingUploadVM>();
            cmdT.CommandType = CommandType.Text;
            // Data yang diproses semua billing recurring (BillCode=B)
            // dan semua billing Other dan quote yang billingnya ditemukan
            cmdT.CommandText = @"SELECT * FROM `stagingupload` su WHERE su.`BillCode`='B' AND su.`IsSuccess`=1 AND su.`trancode`=@trcode
                            UNION ALL
                            SELECT * FROM `stagingupload` su WHERE su.`IsSuccess`=1 AND su.`BillCode`<>'B' AND su.`Billid` IS NOT NULL AND su.`trancode`=@trcode;";
            cmdT.Parameters.Add(new MySqlParameter("@trcode", MySqlDbType.VarChar) { Value = SubmitUpload.trancode });
            try
            {
                cmdT.Connection.Open();
                var rd = cmdT.ExecuteReader();
                while (rd.Read())
                {
                    StagingUploadx.Add(new StagingUploadVM()
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
                        StatusPolis = rd["StatusPolis"].ToString(),
                        BillingID = rd["Billid"].ToString(),
                        BillType = rd["BillType"].ToString(),
                        ReqSeq = (rd["seq"] == DBNull.Value) ? (int?)null : Convert.ToInt32(rd["seq"]),
                        Due_Date_Pre = (rd["due_date_pre"] == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(rd["due_date_pre"]),
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
            var cmdx = _jbsDB.Database;
            var cmdx2 = _life21.Database;
            var cmdx3 = _life21p.Database;

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
                Rcpt = new Receipt();
                lst.TglSkrg = tglSekarang;
                lst.PaymentSource = "CC";
                lst.StatusBilling = "P";
                switch (lst.trancode)
                {
                    case "bcacc":
                        lst.BankidPaid = 1;
                        Rcpt.bank_acc_id = 2;
                        break;
                    case "mandiricc":
                        lst.BankidPaid = 2;
                        Rcpt.bank_acc_id = 8;
                        break;
                    case "megaonus":
                    case "megaoffus":
                        lst.BankidPaid = 12;
                        Rcpt.bank_acc_id = 10;
                        break;
                    case "bnicc":
                        lst.BankidPaid = 3;
                        Rcpt.bank_acc_id = 11;
                        break;

                    case "bcaac":
                        lst.BankidPaid = 1;
                        lst.PaymentSource = "AC";
                        Rcpt.bank_acc_id = 1;
                        break;
                    case "mandiriac":
                        lst.BankidPaid = 2;
                        lst.PaymentSource = "AC";
                        Rcpt.bank_acc_id = 8;
                        break;

                    case "varealtime":
                    case "vadaily":
                        lst.BankidPaid = 1;
                        Rcpt.bank_acc_id = 3;
                        lst.PaymentSource = "VA";
                        break;
                }

                //Cek transaksi atas polis dengan tanggal yang sama
                if (lst.PaymentSource == "VA")
                {
                    var billva = (from b in _jbsDB.BillingModel
                                  join pb in _jbsDB.PolicyBillingModel on b.policy_id equals pb.policy_Id
                                  where pb.policy_no == lst.polisNo && b.PaymentSource == lst.PaymentSource &&
                                  b.paid_date == lst.tgl
                                  select b.BillingID.ToString()
                                   ).FirstOrDefault();
                    if (!string.IsNullOrEmpty(billva)) continue;
                }

                Life21Tran = new PolicyTransaction();
                if (lst.PaymentSource == "CC" || lst.PaymentSource == "VA")
                {
                    Life21Tran.policy_id = lst.PolicyId;
                    Life21Tran.transaction_dt = tglSekarang;
                    Life21Tran.amount = lst.amount;
                    Life21Tran.BankID = lst.BankidPaid;

                    Life21Tran.idTran = lst.life21TranID;
                    Life21Tran.result_status = lst.ApprovalCode;
                    Life21Tran.Remark = lst.Description;
                }
                else if (lst.PaymentSource == "AC")
                {
                    Life21Tran.policy_id = lst.PolicyId;
                    Life21Tran.transaction_dt = tglSekarang;
                    Life21Tran.update_dt = tglSekarang;
                    Life21Tran.amount = lst.amount;
                    Life21Tran.BankID = lst.BankidPaid;

                    Life21Tran.idTran = lst.life21TranID;
                    Life21Tran.result_status = lst.ApprovalCode;
                    Life21Tran.Remark = lst.Description;
                }

                try
                {
                    cmdx.OpenConnection(); cmdx.BeginTransaction(); // jbs
                    cmdx2.OpenConnection(); cmdx2.BeginTransaction(); // life21
                    cmdx3.OpenConnection(); cmdx3.BeginTransaction(); //life21p

                    if (lst.polisNo == null) throw new Exception("Submit Upload ==>> PolisNo null");
                    if (!((lst.BillCode == "B") && (lst.BillingID == string.Empty))) lst.PaymentTransactionID = InsertTransactionBank(ref cmd, lst); // transaksi histori di JBS
                    if (lst.IsSuccess) // transaksi sukses

                    {
                        if ((lst.BillCode != "B") && (lst.BillingID == string.Empty)) throw new Exception("Submit Upload (PolisNo " + lst.polisNo + ")==>> Billcode null");
                        if (lst.BillCode == "Q")
                        { // untuk Billing Quote 
                            UpdateQuote(ref cmd3, tglSekarang, lst.BankidPaid, Convert.ToInt32(lst.BillingID), lst.PaymentSource);
                            UpdateBillingQuoteJBS(ref cmd, lst);
                            await AsyncSendEmailThanksQuote(Convert.ToInt32(lst.BillingID), lst.amount);
                        }
                        else
                        {// transaksi sudah pasti bukan Quote
                            Rcpto = new ReceiptOther();
                            BillingModel bil;

                            if (lst.BillCode == "B")
                            { // Recurring >> insert Receipt
                                Rcpt.transaction_code = "RP"; // buat recurring
                                if (string.IsNullOrEmpty(lst.BillingID))
                                {
                                    if (!lst.IsSuccess) continue;

                                    // Jika proses create billing hanya untuk billing approve
                                    lst.BillingID = CreateBilling(ref cmd, lst.polisNo, lst.PaymentSource, lst.BankidPaid);
                                    if (string.IsNullOrEmpty(lst.BillingID)) throw new Exception("Billing Gagal Create");

                                    bil = _jbsDB.BillingModel.FirstOrDefault(c => c.BillingID == Convert.ToInt32(lst.BillingID));

                                    if (bil == null) throw new Exception("Submit Upload (PolisNo " + lst.polisNo + ")==>> row Billing null"); ;
                                    lst.PolicyId = bil.policy_id;
                                    lst.ReqSeq = bil.recurring_seq;
                                    lst.Due_Date_Pre = bil.due_dt_pre;

                                    lst.PaymentTransactionID = InsertTransactionBank(ref cmd, lst);
                                }
                                else bil = _jbsDB.BillingModel.FirstOrDefault(c => c.BillingID == Convert.ToInt32(lst.BillingID));
                                var polis = _jbsDB.PolicyBillingModel.FirstOrDefault(p => p.policy_Id == bil.policy_id);

                                if (bil.cashless_fee_amount > 0)
                                { // insert receipt other recurring untuk cashless
                                    Rcpto.policy_id = lst.PolicyId;
                                    Rcpto.receipt_amount = bil.cashless_fee_amount;
                                    Rcpto.receipt_date = tglSekarang;
                                    Rcpto.bank_acc_id = Rcpt.bank_acc_id;
                                    Rcpto.seq = lst.ReqSeq;
                                    Rcpto.receipt_source = lst.PaymentSource;
                                    Rcpto.type_id = 3; // untuk recurring type harus 3
                                    Rcpto.acquirer_bank_id = lst.BankidPaid;
                                    lst.receipt_other_id = InsertReceiptOther(ref cmd2, Rcpto);
                                }

                                Rcpt.receipt_policy_id = lst.PolicyId;
                                Rcpt.receipt_source = lst.PaymentSource;
                                Rcpt.receipt_date = tglSekarang;
                                Rcpt.receipt_amount = lst.amount - bil.cashless_fee_amount;
                                Rcpt.receipt_seq = lst.ReqSeq;
                                Rcpt.fund_type_id = 0;
                                Rcpt.status = "P";
                                Rcpt.acquirer_bank_id = lst.BankidPaid;
                                Rcpt.due_date_pre = lst.Due_Date_Pre;

                                lst.receipt_id = InsertReceipt(ref cmd2, Rcpt);

                                Life21Tran.policy_id = lst.PolicyId;
                                Life21Tran.ACC_Name = bil.AccName;
                                Life21Tran.ACC_No = bil.AccNo;
                                Life21Tran.CC_expiry = bil.cc_expiry;
                                Life21Tran.receipt_id = lst.receipt_id;
                                Life21Tran.Due_Date_Pre = lst.Due_Date_Pre;
                                Life21Tran.recurring_seq = lst.ReqSeq;
                                Life21Tran.transaction_type = "R";

                                if (lst.PaymentSource == "AC") lst.life21TranID = InsertACTransaction(ref cmd2, Life21Tran);
                                else if (lst.PaymentSource == "CC") lst.life21TranID = InsertCCTransaction(ref cmd2, Life21Tran);
                                else lst.life21TranID = null;

                                if (lst.PaymentSource == "VA")
                                {
                                    lst.ACCname = string.Empty;
                                    lst.ACCno = string.Empty;
                                    lst.CC_Expiry = string.Empty;
                                }
                                else
                                {
                                    if (lst.PaymentSource == "CC")
                                    {
                                        var polisCC = _jbsDB.PolicyCCModel.FirstOrDefault(p => p.PolicyId == bil.policy_id);
                                        if (polisCC != null)
                                        {
                                            lst.ACCname = bil.AccName ?? polisCC.cc_name;
                                            lst.ACCno = bil.AccNo ?? polisCC.cc_no;
                                            lst.CC_Expiry = bil.cc_expiry ?? polisCC.cc_expiry;
                                        }
                                    }
                                    else if (lst.PaymentSource == "AC")
                                    {
                                        var polisAC = _jbsDB.PolicyAcModel.FirstOrDefault(p => p.PolicyId == bil.policy_id);
                                        if (polisAC != null)
                                        {
                                            lst.ACCname = bil.AccName ?? polisAC.acc_name;
                                            lst.ACCno = bil.AccNo ?? polisAC.acc_no;
                                        }
                                    }
                                }
                                UpdateBillingJBS(ref cmd, lst);
                                UpdateLastTransJBS(ref cmd, lst);
                                await AsyncSendEmailThanksRecurring(Convert.ToInt32(lst.BillingID));
                            }
                            else
                            { // Billing Others >> insert Receipt Other (pasti CC) 
                              // untuk billing other, melakukan UPDATE CC transaction karena tagihan berasal dari Life21

                                Rcpto.policy_id = lst.PolicyId;
                                Rcpto.receipt_amount = lst.amount;
                                Rcpto.receipt_date = tglSekarang;
                                Rcpto.bank_acc_id = Rcpt.bank_acc_id;
                                Rcpto.receipt_source = lst.PaymentSource;
                                Rcpto.type_id = (lst.BillType == "A2" ? 1 : (lst.BillType == "A3" ? 2 : 0));
                                if (Rcpto.type_id == 0 || Rcpto.type_id == null) throw new Exception("Billing Type tidak terdefenisi");
                                Rcpto.acquirer_bank_id = lst.BankidPaid;

                                lst.receipt_other_id = InsertReceiptOther(ref cmd2, Rcpto);
                                Life21Tran.receipt_other_id = (lst.receipt_other_id ?? 0);

                                UpdateCCTransaction(ref cmd2, Life21Tran);
                                UpdateBillingOthersJBS(ref cmd, lst);
                                await AsyncSendEmailThanksEndorsemen(lst.BillingID);
                            }
                        }
                    }
                    //else // transaksi Gagal
                    //{
                    //    if (string.IsNullOrEmpty(lst.BillingID.Trim())) continue;
                    //    BukaFlagDownloadBilling(ref cmd, lst);
                    //    if (lst.PaymentSource == "AC") InsertPolisHold(ref cmd, lst.BillCode, lst.polisNo, DateTime.Now.AddDays(15));
                    //}

                    cmdx.CommitTransaction();
                    cmdx2.CommitTransaction();
                    cmdx3.CommitTransaction();
                }
                catch (Exception ex)
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
                    if (cmdx.CurrentTransaction != null) cmdx.RollbackTransaction();
                    if (cmdx2.CurrentTransaction != null) cmdx2.RollbackTransaction();
                    if (cmdx3.CurrentTransaction != null) cmdx3.RollbackTransaction();

                    cmdx.CloseConnection();
                    cmdx2.CloseConnection();
                    cmdx3.CloseConnection();
                }

            } // end foreach (var lst in StagingUploadx) 

            // setelah selesai looping transaksi approve, buka flag download yang reject
            cmdT.CommandType = CommandType.StoredProcedure;
            cmdT.CommandText = @"mapping_billing_reject";
            cmdT.Parameters.Clear();
            try
            {
                cmdT.Connection.Open();
                cmdT.ExecuteNonQuery();

                cmdT.CommandText = @"Recurring_Reject";
                cmdT.Parameters.Clear();
                cmdT.Parameters.Add(new MySqlParameter("@trancode", MySqlDbType.VarChar) { Value = SubmitUpload.trancode });
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


            hitungUlang();
            PindahFileDownload(SubmitUpload.trancode);

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

            string txfilename;
            decimal fileamount = 0; // amount dr file
            txfilename = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName);

            string xFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName).ToLower() +
                Guid.NewGuid().ToString().Substring(0, 8) + Path.GetExtension(UploadBill.FileBill.FileName);
            // Simpan File yang diUpload ke File Backup
            using (var fileStream = new FileStream(BackupResult + xFileName, FileMode.Create))
            {
                UploadBill.FileBill.CopyTo(fileStream);
            }

            StagingUpload st;
            using (var reader = new StreamReader(UploadBill.FileBill.OpenReadStream()))
            {
                int i = 0;
                while (reader.Peek() >= 0)
                {
                    i++;
                    st = new StagingUpload();
                    st.trancode = UploadBill.TranCode;
                    st.filename = xFileName;
                    st.BillCode = "B"; // untuk AC dan VA hanya transaksi recurring

                    var tmp = reader.ReadLine();
                    if (UploadBill.TranCode == "bcacc")
                    {
                        var panjang = tmp.Trim().Length;
                        if (panjang < 91) continue;

                        st.polisNo = tmp.Substring(9, 25).Trim();
                        st.ACCno = tmp.Substring(34, 16).Trim();
                        st.ACCname = tmp.Substring(65, 26).Trim();
                        if (!Decimal.TryParse(tmp.Substring(54, 9), out fileamount)) continue;
                        st.amount = fileamount;
                        st.Description = tmp.Substring(tmp.Length - 2);
                        st.ApprovalCode = st.Description;
                        if (st.Description == "00")
                        {
                            st.ApprovalCode = tmp.Substring(tmp.Length - 8).Substring(0, 6);
                            st.IsSuccess = true;
                        }

                        if (st.polisNo.Substring(0, 1) == "A") st.BillCode = "A";
                        else if (st.polisNo.Substring(0, 1) == "X")
                        {
                            st.polisNo = st.polisNo.Substring(1);
                            st.BillCode = "Q";
                        }
                        else st.BillCode = "B";

                    }
                    else if (UploadBill.TranCode == "bcaac")
                    {
                        var panjang = tmp.Length;
                        if (panjang < 205) continue;

                        st.polisNo = tmp.Substring(92, 15).Trim();
                        st.ApprovalCode = tmp.Substring(129, 9).Trim();
                        st.Description = tmp.Substring(138, 51).Trim();
                        st.IsSuccess = (st.ApprovalCode.ToLower() == "berhasil") ? true : false;
                        st.ACCno = tmp.Substring(37, 11).Trim();
                        st.ACCname = tmp.Substring(48, 26).Trim();
                        if (!Decimal.TryParse(tmp.Substring(74, 18), out fileamount)) continue;
                        st.amount = fileamount;
                        DateTime time;
                        if (!DateTime.TryParseExact(tmp.Substring(189, 16).Trim(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out time)) continue;
                        st.tgl = time;
                    }
                    else if (UploadBill.TranCode == "mandiriac")
                    {
                        var panjang = tmp.Length;
                        if (panjang < 720) continue;

                        st.polisNo = tmp.Substring(590, 40).Trim();
                        if (!Decimal.TryParse(tmp.Substring(634, 40), out fileamount)) continue;
                        st.amount = fileamount;
                        st.ApprovalCode = tmp.Substring(674, 46).Trim();

                        st.IsSuccess = (st.ApprovalCode.ToLower() == "success") ? true : false;
                        if (!st.IsSuccess) st.Description = tmp.Substring(720, tmp.Length - 720).Trim();

                        var acc = tmp.Substring(306, 244).Trim().Split('/');
                        if (acc.Length == 2)
                        {
                            st.ACCno = acc[0].Trim();
                            st.ACCname = acc[1].Replace("(IDR)", string.Empty).Trim();
                        }
                        DateTime time;
                        if (!DateTime.TryParseExact(tmp.Substring(0, 19).Trim(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out time)) continue;
                        st.tgl = time;
                    }
                    else if (UploadBill.TranCode == "varealtime")
                    {
                        var panjang = tmp.Length;
                        if (panjang < 195) continue;
                        st.IsSuccess = true;
                        if (!int.TryParse(tmp.Substring(0, 5), out i)) continue; // cek no urut
                        st.polisNo = tmp.Substring(11, 19).Trim();
                        st.ACCname = tmp.Substring(45, 31).Trim();
                        if (!Decimal.TryParse(tmp.Substring(112, 22), out fileamount)) continue;
                        st.amount = fileamount;
                        st.Description = tmp.Substring(158, 37).Trim();
                        DateTime time;
                        if (!DateTime.TryParseExact(tmp.Substring(136, 19).Trim(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out time)) continue;
                        st.tgl = time;

                    }
                    else if (UploadBill.TranCode == "vadaily")
                    {
                        var panjang = tmp.Length;
                        if (panjang < 133) continue;
                        st.IsSuccess = true;
                        if (!int.TryParse(tmp.Substring(1, 5), out i)) continue; // cek no urut
                        st.polisNo = tmp.Substring(8, 20).Trim();
                        st.ACCname = tmp.Substring(28, 18).Trim();
                        if (!Decimal.TryParse(tmp.Substring(52, 19), out fileamount)) continue;
                        st.amount = fileamount;
                        st.Description = tmp.Substring(100, 33).Trim();
                        DateTime time;
                        if (!DateTime.TryParseExact(tmp.Substring(73, 18).Trim(), "dd/MM/yy  HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out time)) continue;
                        st.tgl = time;

                    }
                    else throw new Exception("Transaksi CC, TranCode belum di defenisikan");

                    //if (st.polisNo.Substring(0, 1) == "A")
                    //{
                    //    //st.Billid = st.polisNo;
                    //    st.BillCode = "A";
                    //    //st.polisNo = "";
                    //}
                    //else if (st.polisNo.Substring(0, 1) == "X")
                    //{
                    //    st.polisNo = st.polisNo.Substring(1);
                    //    st.BillCode = "Q";
                    //    //st.polisNo = "";
                    //}
                    //else st.BillCode = "B";


                    try
                    {
                        st.id = i;
                        //InsertStagingTable(Convert.ToInt32(baris.Substring(baris.Length - 5)), polisTran, billcode, trandate, fileamount, isApprove, approvalCode, TranDesc, UploadBill.TranCode, xFileName, accNo);
                        InsertStagingTable(st);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message + st.Billid);
                    }

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
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cmd.Dispose();
                cmd.Connection.Close();
            }

            string txfilename;
            decimal fileamount = 0; // amount dr file
            txfilename = Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName);

            string xFileName = DateTime.Now.ToString("yyyyMMdd") + "_" + Path.GetFileNameWithoutExtension(UploadBill.FileBill.FileName).ToLower() +
                Guid.NewGuid().ToString().Substring(0, 8) + Path.GetExtension(UploadBill.FileBill.FileName);
            // Simpan File yang diUpload ke File Backup
            using (var fileStream = new FileStream(BackupResult + xFileName, FileMode.Create))
            {
                UploadBill.FileBill.CopyTo(fileStream);
            }

            StagingUpload st;
            byte[] file = System.IO.File.ReadAllBytes(BackupResult + xFileName);
            using (MemoryStream ms = new MemoryStream(file))
            {
                using (ExcelPackage package = new ExcelPackage(ms))
                {
                    ExcelWorkbook wb = package.Workbook;
                    if (UploadBill.TranCode != "bnicc" && wb.Worksheets.Count < 2) throw new Exception("File Result harus 2 Sheet");
                    for (int sht = 1; sht < 3; sht++) // looping sheet 1 & 2
                    {
                        st = new StagingUpload();
                        st.trancode = UploadBill.TranCode;
                        long tmpa = 0;
                        ExcelWorksheet ws = wb.Worksheets[sht];
                        for (int row = ws.Dimension.Start.Row; row <= ws.Dimension.End.Row; row++)
                        {

                            if (UploadBill.TranCode == "mandiricc")
                            {
                                if ((ws.Cells[row, 1].Value == null) || // Nourut
                                    (ws.Cells[row, 3].Value == null) || // Amount
                                    (ws.Cells[row, 4].Value == null) || // AppCode
                                    (ws.Cells[row, 5].Value == null) || // Desc
                                    (ws.Cells[row, 6].Value == null) || // No Polis / Bill
                                    (ws.Cells[row, 7].Value == null)) // ACC No
                                    continue;

                                if (sht == 1) // Sheet APPROVE
                                {
                                    if (ws.Cells[row, 6].Value == null) continue;
                                    if (!long.TryParse(ws.Cells[row, 6].Value.ToString().Trim().Substring(1), out tmpa)) continue;
                                    st.polisNo = ws.Cells[row, 6].Value.ToString().Trim();
                                    st.ApprovalCode = ws.Cells[row, 4].Value.ToString().Trim();
                                    st.Description = ws.Cells[row, 5].Value.ToString().Trim();
                                    st.IsSuccess = true;

                                }
                                else // Sheet REJECT
                                {
                                    if (ws.Cells[row, 4].Value == null) continue;
                                    if (!long.TryParse(ws.Cells[row, 4].Value.ToString().Trim().Substring(1), out tmpa)) continue;

                                    st.polisNo = ws.Cells[row, 4].Value.ToString().Trim();
                                    st.ApprovalCode = ws.Cells[row, 5].Value.ToString().Trim();
                                    st.Description = ws.Cells[row, 6].Value.ToString().Trim();
                                    st.IsSuccess = false;

                                }
                                if (!decimal.TryParse(ws.Cells[row, 3].Value.ToString().Trim(), out fileamount)) continue;
                                st.amount = fileamount;
                                st.ACCno = ws.Cells[row, 7].Value.ToString().Trim();
                                st.ACCname = ws.Cells[row, 2].Value.ToString().Trim();
                            }
                            else if (UploadBill.TranCode == "megaonus" || UploadBill.TranCode == "megaoffus")
                            {
                                if ((ws.Cells[row, 1].Value == null) || // Nourut
                                    (ws.Cells[row, 2].Value == null) || // Deskripsi yg berisi no polis
                                    (ws.Cells[row, 3].Value == null) || // Amount
                                    (ws.Cells[row, 4].Value == null) || // transaction date
                                    (ws.Cells[row, 5].Value == null) || // Decline code
                                    (ws.Cells[row, 6].Value == null)) // Flaging
                                    continue;
                                if (!long.TryParse(ws.Cells[row, 1].Value.ToString().Trim(), out tmpa)) continue;

                                var temp = ws.Cells[row, 2].Value.ToString().Trim();
                                st.polisNo = temp.Split('-').Last().Trim();
                                if (string.IsNullOrEmpty(st.polisNo)) continue;
                                if (!long.TryParse(st.polisNo.Substring(1), out tmpa)) continue;

                                st.ApprovalCode = ws.Cells[row, 5].Value.ToString().Trim();
                                st.Description = ws.Cells[row, 6].Value.ToString().Trim();
                                st.IsSuccess = (sht == 1) ? true : false;

                                if (!decimal.TryParse(ws.Cells[row, 3].Value.ToString().Trim(), out fileamount)) continue;
                                st.amount = fileamount;
                                DateTime time;
                                if (DateTime.TryParse(ws.Cells[row, 3].Value.ToString().Trim(), out time)) st.tgl = time;
                            }
                            else if (UploadBill.TranCode == "bnicc")
                            {
                                if ((ws.Cells[row, 1].Value == null) || // Nourut
                                    (ws.Cells[row, 4].Value == null) || // ACC No
                                    (ws.Cells[row, 7].Value == null) || // No Polis / Bill
                                    (ws.Cells[row, 8].Value == null) || // Amoun
                                    (ws.Cells[row, 9].Value == null) || // Approval Code code
                                    (ws.Cells[row, 10].Value == null)) // Desc
                                    continue;

                                // cek no urut harus angka
                                if (!long.TryParse(ws.Cells[row, 1].Value.ToString().Trim(), out tmpa)) continue;
                                if (ws.Cells[row, 7].Value == null) continue;
                                //cek NoPolis, hlangkan 1 karakter dikiri dan konversi ke angka
                                if (!long.TryParse(ws.Cells[row, 7].Value.ToString().Trim().Substring(1), out tmpa)) continue;
                                // amount
                                if (!decimal.TryParse(ws.Cells[row, 8].Value.ToString().Trim(), out fileamount)) continue;
                                st.amount = fileamount;
                                st.polisNo = ws.Cells[row, 7].Value.ToString().Trim();
                                st.ApprovalCode = ws.Cells[row, 9].Value.ToString().Trim();
                                st.Description = ws.Cells[row, 10].Value.ToString().Trim();
                                st.IsSuccess = (st.ApprovalCode == "") ? false : true;
                                st.ACCno = ws.Cells[row, 4].Value.ToString().Trim();
                                st.ACCname = ws.Cells[row, 5].Value.ToString().Trim();

                            } // END UploadBill.TranCode ==
                            else
                            {
                                throw new Exception("Transaksi CC, TranCode belum di defenisikan");
                            }

                            if (st.polisNo.Substring(0, 1) == "A")
                            {
                                st.Billid = st.polisNo;
                                st.BillCode = "A";
                            }
                            else if (st.polisNo.Substring(0, 1) == "X")
                            {
                                st.Billid = st.polisNo.Substring(1);
                                st.polisNo = st.Billid;
                                st.BillCode = "Q";
                            }
                            else st.BillCode = "B";

                            try
                            {
                                var baris = "0000" + row.ToString();
                                st.id = Convert.ToInt32(sht.ToString() + baris.Substring(baris.Length - 5));
                                InsertStagingTable(st);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message + st.Billid);
                            }

                        }// END for (row=ws.Dimension.Start.Row; row <= ws.Dimension.End.Row; row++)

                        if (UploadBill.TranCode == "bnicc") break; // BNI cma 1 Sheet (1x loop langsung break)
                    } // END for(int sht=0; sht < 2; sht++)
                } // END using (ExcelPackage package = new ExcelPackage(new FileInfo(xFileName)))
            }
        }

        #endregion

        //private void InsertStagingTable(int id, string polisno,string billcode,DateTime? tgl,decimal amount,Boolean isSukses,
        //    string appcode,string desc, string trancode, string filename, string accNo)
        private void InsertStagingTable(StagingUpload st)
        {
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.Parameters.Clear();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = @"InsertStagingUpload";
            cmd.Parameters.Add(new MySqlParameter("@id", MySqlDbType.Int32) { Value = st.id });
            cmd.Parameters.Add(new MySqlParameter("@polis", MySqlDbType.VarChar) { Value = st.polisNo });
            cmd.Parameters.Add(new MySqlParameter("@billCode", MySqlDbType.VarChar) { Value = st.BillCode });
            cmd.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = st.tgl });
            cmd.Parameters.Add(new MySqlParameter("@amt", MySqlDbType.Decimal) { Value = st.amount });
            cmd.Parameters.Add(new MySqlParameter("@IsSukses", MySqlDbType.Bit) { Value = st.IsSuccess });
            cmd.Parameters.Add(new MySqlParameter("@appcode", MySqlDbType.VarChar) { Value = st.ApprovalCode });
            cmd.Parameters.Add(new MySqlParameter("@description", MySqlDbType.VarChar) { Value = st.Description });
            cmd.Parameters.Add(new MySqlParameter("@trancode", MySqlDbType.VarChar) { Value = st.trancode });
            cmd.Parameters.Add(new MySqlParameter("@filename", MySqlDbType.VarChar) { Value = st.filename });
            cmd.Parameters.Add(new MySqlParameter("@AccNo", MySqlDbType.VarChar) { Value = st.ACCno });
            cmd.Parameters.Add(new MySqlParameter("@AccNama", MySqlDbType.VarChar) { Value = st.ACCname });

            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.Message);
                throw new Exception("InsertStagingTable => (BillingID = " + st.Billid + ") " + ex.Message);
            }
            finally
            {
                cmd.Connection.Close();
            }
        }

        // history JBS Transaksi
        private int InsertTransactionBank(ref System.Data.Common.DbCommand cm, StagingUploadVM tb)
        {
            cm.Parameters.Clear();
            cm.CommandType = CommandType.StoredProcedure;
            cm.CommandText = @"InsertTransactionBank;";
            cm.Parameters.Add(new MySqlParameter("@FileName", MySqlDbType.VarChar) { Value = tb.filename });
            cm.Parameters.Add(new MySqlParameter("@Trancode", MySqlDbType.VarChar) { Value = tb.trancode });
            cm.Parameters.Add(new MySqlParameter("@TranDate", MySqlDbType.DateTime) { Value = tb.tgl });
            cm.Parameters.Add(new MySqlParameter("@IsApprove", MySqlDbType.Bit) { Value = tb.IsSuccess });
            cm.Parameters.Add(new MySqlParameter("@policyID", MySqlDbType.Int32) { Value = tb.PolicyId });
            cm.Parameters.Add(new MySqlParameter("@IDBill", MySqlDbType.VarChar) { Value = tb.BillingID });
            cm.Parameters.Add(new MySqlParameter("@amount", MySqlDbType.Decimal) { Value = tb.amount });
            cm.Parameters.Add(new MySqlParameter("@approvalCode", MySqlDbType.VarChar) { Value = tb.ApprovalCode });
            cm.Parameters.Add(new MySqlParameter("@ErrCode", MySqlDbType.VarChar) { Value = tb.Description });
            cm.Parameters.Add(new MySqlParameter("@AccNo", MySqlDbType.VarChar) { Value = tb.ACCno });
            cm.Parameters.Add(new MySqlParameter("@AccNama", MySqlDbType.VarChar) { Value = tb.ACCno });
            var idTran = 0;
            try
            {
                var hasil = cm.ExecuteScalar();
                idTran = Convert.ToInt32(hasil);
            }
            catch (Exception ex)
            {
                throw new Exception("InsertTransactionBank => (BillingID = " + tb.BillingID + ") " + ex.Message);
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
	                                    Remark='APPROVED',
	                                    receipt_other_id=@receiptID,
	                                    update_dt=@dtupdate
                                        WHERE `policy_cc_tran_id`=@id;";
            cm.Parameters.Add(new MySqlParameter("@rstStatus", MySqlDbType.VarChar) { Value = pt.result_status });
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

        private int InsertCCTransaction(ref System.Data.Common.DbCommand cm, PolicyTransaction pt)
        {
            cm.Parameters.Clear();
            cm.CommandType = CommandType.Text;
            cm.CommandText = @"INSERT INTO `policy_cc_transaction`(`policy_id`,`transaction_dt`,`transaction_type`,
		                    `recurring_seq`,`count_times`,`currency`,`total_amount`,`due_date_pre`,`due_date_pre_period`,
		                    `cycle_date`,`acquirer_bank_id`,`status_id`,`remark`,`receipt_id`,`receipt_other_id`,`created_dt`,
                            `cc_no`,`cc_name`,`cc_expiry`,`update_dt`)
	                    SELECT @PolisID,@Transdate,@billType,@Seq,1,'IDR',@Amount,@DueDatePre,@Period,@CycleDate,@BankID,2,'APPROVED',@receiptID,NULLIF(@receiptOtherID,0),@Transdate,
                            @CCno, @CCName, @CCExpiry,@Transdate;
	                    SELECT LAST_INSERT_ID() AS TranId;";
            cm.Parameters.Add(new MySqlParameter("@PolisID", MySqlDbType.Int32) { Value = pt.policy_id });
            cm.Parameters.Add(new MySqlParameter("@Transdate", MySqlDbType.DateTime) { Value = pt.transaction_dt });
            cm.Parameters.Add(new MySqlParameter("@billType", MySqlDbType.VarChar) { Value = pt.transaction_type });
            cm.Parameters.Add(new MySqlParameter("@Seq", MySqlDbType.Int32) { Value = pt.recurring_seq });
            cm.Parameters.Add(new MySqlParameter("@Amount", MySqlDbType.Decimal) { Value = pt.amount });
            cm.Parameters.Add(new MySqlParameter("@DueDatePre", MySqlDbType.Date) { Value = pt.Due_Date_Pre });
            cm.Parameters.Add(new MySqlParameter("@Period", MySqlDbType.VarChar) { Value = String.Format("{0:MMMdd}", pt.Due_Date_Pre) });
            cm.Parameters.Add(new MySqlParameter("@CycleDate", MySqlDbType.Int32) { Value = String.Format("{0:dd}", pt.Due_Date_Pre) });
            cm.Parameters.Add(new MySqlParameter("@BankID", MySqlDbType.Int32) { Value = pt.BankID });
            cm.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = pt.receipt_id });
            cm.Parameters.Add(new MySqlParameter("@receiptOtherID", MySqlDbType.Int32) { Value = pt.receipt_other_id });

            cm.Parameters.Add(new MySqlParameter("@CCno", MySqlDbType.VarChar) { Value = pt.ACC_No });
            cm.Parameters.Add(new MySqlParameter("@CCName", MySqlDbType.VarChar) { Value = pt.ACC_Name });
            cm.Parameters.Add(new MySqlParameter("@CCExpiry", MySqlDbType.VarChar) { Value = pt.CC_expiry });

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

        private int InsertACTransaction(ref System.Data.Common.DbCommand cm, PolicyTransaction pt)
        {
            cm.Parameters.Clear();
            cm.CommandType = CommandType.Text;
            cm.CommandText = @"INSERT INTO policy_ac_transaction(`policy_id`,`transaction_dt`,`transaction_type`,`recurring_seq`,`count_times`,`currency`,`total_amount`,
	                        `due_date_pre`,`due_date_pre_period`,`cycle_date`,`bank_id`,`acc_no`,`acc_name`,`status_id`,`receipt_id`,`created_dt`,`update_dt`)
	                    SELECT @PolisID,@Transdate,@TransType,@Seq,1,'IDR',@Amount,@DueDatePre,@Period,@CycleDate,@BankID,@ACno,@ACName,2,@receiptID,@tgl,@tgl;
	                    SELECT LAST_INSERT_ID() AS TranId;";
            cm.Parameters.Add(new MySqlParameter("@PolisID", MySqlDbType.Int32) { Value = pt.policy_id });
            cm.Parameters.Add(new MySqlParameter("@Transdate", MySqlDbType.Date) { Value = pt.transaction_dt });
            cm.Parameters.Add(new MySqlParameter("@TransType", MySqlDbType.VarChar) { Value = pt.transaction_type });
            cm.Parameters.Add(new MySqlParameter("@Seq", MySqlDbType.Int32) { Value = pt.recurring_seq });
            cm.Parameters.Add(new MySqlParameter("@Amount", MySqlDbType.Decimal) { Value = pt.amount });
            cm.Parameters.Add(new MySqlParameter("@DueDatePre", MySqlDbType.Date) { Value = pt.Due_Date_Pre });
            cm.Parameters.Add(new MySqlParameter("@Period", MySqlDbType.VarChar) { Value = String.Format("{0:MMMdd}", pt.Due_Date_Pre) });
            cm.Parameters.Add(new MySqlParameter("@CycleDate", MySqlDbType.Int32) { Value = String.Format("{0:dd}", pt.Due_Date_Pre) });
            cm.Parameters.Add(new MySqlParameter("@BankID", MySqlDbType.Int32) { Value = pt.BankID });
            cm.Parameters.Add(new MySqlParameter("@ACno", MySqlDbType.VarChar) { Value = pt.ACC_No });
            cm.Parameters.Add(new MySqlParameter("@ACName", MySqlDbType.VarChar) { Value = pt.ACC_Name });
            cm.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = pt.receipt_id });
            cm.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = pt.update_dt });

            var ACTransID = 0;
            try
            {
                ACTransID = Convert.ToInt32(cm.ExecuteScalar());
            }
            catch (Exception ex)
            {
                throw new Exception("InsertACTransaction => (policyid = " + pt.policy_id.ToString() + ") " + ex.Message);
            }

            return ACTransID;
        }

        private int InsertReceipt(ref System.Data.Common.DbCommand cm, Receipt rc)
        {
            cm.Parameters.Clear();
            cm.CommandType = CommandType.Text;
            cm.CommandText = @"INSERT INTO `receipt`(
                                `receipt_date`,
                                `receipt_policy_id`, 
                                `receipt_fund_type_id`, 
                                `receipt_transaction_code`, 
                                `receipt_amount`,
                                `receipt_source`, 
                                `receipt_status`, 
                                `receipt_payment_date_time`, 
                                `receipt_seq`, 
                                `bank_acc_id`, 
                                `due_date_pre`,
                                `acquirer_bank_id`)
                            SELECT @receiptdate,
                                @policyId,
                                @fundTypeId,
                                @transactionCode,
                                @amount,
                                @source,
                                @receiptStatus,
                                @paymentDate,
                                @seq,
                                @bankAccId,
                                @due_date_pre,
                                @AcquirerBankId;
                        SELECT LAST_INSERT_ID();";
            cm.Parameters.Add(new MySqlParameter("@receiptdate", MySqlDbType.Date) { Value = rc.receipt_date });
            cm.Parameters.Add(new MySqlParameter("@policyId", MySqlDbType.Int32) { Value = rc.receipt_policy_id });
            cm.Parameters.Add(new MySqlParameter("@fundTypeId", MySqlDbType.Int32) { Value = rc.fund_type_id });
            cm.Parameters.Add(new MySqlParameter("@transactionCode", MySqlDbType.VarChar) { Value = rc.transaction_code });
            cm.Parameters.Add(new MySqlParameter("@amount", MySqlDbType.Decimal) { Value = rc.receipt_amount });
            cm.Parameters.Add(new MySqlParameter("@receiptStatus", MySqlDbType.VarChar) { Value = rc.status });
            cm.Parameters.Add(new MySqlParameter("@source", MySqlDbType.VarChar) { Value = rc.receipt_source });
            cm.Parameters.Add(new MySqlParameter("@paymentDate", MySqlDbType.DateTime) { Value = rc.receipt_date });
            cm.Parameters.Add(new MySqlParameter("@seq", MySqlDbType.Int32) { Value = rc.receipt_seq });
            cm.Parameters.Add(new MySqlParameter("@bankAccId", MySqlDbType.Int32) { Value = rc.bank_acc_id });
            cm.Parameters.Add(new MySqlParameter("@due_date_pre", MySqlDbType.Date) { Value = rc.due_date_pre });
            cm.Parameters.Add(new MySqlParameter("@AcquirerBankId", MySqlDbType.Int32) { Value = rc.acquirer_bank_id });

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
            cm.CommandType = CommandType.Text;
            cm.CommandText = @"INSERT INTO `receipt_other`(
                                `receipt_date`,
                                `policy_id`,
                                `receipt_type_id`,
                                `receipt_amount`,
                                `receipt_source`,
                                `receipt_payment_date`,
                                `receipt_seq`,
                                `bank_acc_id`,
                                `acquirer_bank_id`) 
                            SELECT @receiptdate,
                                @policyId,
                                @receiptTypeId,
                                @amount,
                                @source,
                                @paymentDate,
                                @seq,
                                @bankAccId,
                                @AcquirerBankId;
                            SELECT LAST_INSERT_ID();";
            cm.Parameters.Add(new MySqlParameter("@receiptdate", MySqlDbType.DateTime) { Value = ro.receipt_date });
            cm.Parameters.Add(new MySqlParameter("@policyId", MySqlDbType.Int32) { Value = ro.policy_id });
            cm.Parameters.Add(new MySqlParameter("@receiptTypeId", MySqlDbType.Int32) { Value = ro.type_id });
            cm.Parameters.Add(new MySqlParameter("@amount", MySqlDbType.Decimal) { Value = ro.receipt_amount });
            cm.Parameters.Add(new MySqlParameter("@source", MySqlDbType.VarChar) { Value = ro.receipt_source });
            cm.Parameters.Add(new MySqlParameter("@paymentDate", MySqlDbType.DateTime) { Value = ro.receipt_date });
            cm.Parameters.Add(new MySqlParameter("@seq", MySqlDbType.Int32) { Value = ro.seq });
            cm.Parameters.Add(new MySqlParameter("@bankAccId", MySqlDbType.Int32) { Value = ro.bank_acc_id });
            cm.Parameters.Add(new MySqlParameter("@AcquirerBankId", MySqlDbType.Int32) { Value = ro.acquirer_bank_id });

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

        private void BukaFlagDownloadBilling(ref System.Data.Common.DbCommand cm, StagingUploadVM st)
        { // hanya buka flag download, untuk transaksi Reject
            cm.Parameters.Clear();
            cm.CommandType = CommandType.Text;
            if (st.BillCode == "B")
            {// Transaksi Billing Rucurring
                cm.CommandText = @"UPDATE `billing` SET IsDownload=0, LastUploadDate=@uploadDate,PaymentTransactionID=@ptd WHERE `BillingID`=@billid";
                cm.Parameters.Add(new MySqlParameter("@billid", MySqlDbType.Int32) { Value = Convert.ToInt32(st.BillingID) });
            }
            else if (st.BillCode == "Q")
            {// Transaksi Billing Quote
                cm.CommandText = @"UPDATE `quote_billing` SET IsDownload=0, LastUploadDate=@uploadDate,PaymentTransactionID=@ptd WHERE `quote_id`=@billid";
                cm.Parameters.Add(new MySqlParameter("@billid", MySqlDbType.Int32) { Value = Convert.ToInt32(st.BillingID) });
            }
            else
            {// transaksi Billing Others
                cm.CommandText = @"UPDATE `billing_others` SET IsDownload=0, LastUploadDate=@uploadDate,PaymentTransactionID=@ptd WHERE `BillingID`=@billid";
                cm.Parameters.Add(new MySqlParameter("@billid", MySqlDbType.VarChar) { Value = st.BillingID });
            }
            cm.Parameters.Add(new MySqlParameter("@uploadDate", MySqlDbType.DateTime) { Value = st.TglSkrg });
            cm.Parameters.Add(new MySqlParameter("@ptd", MySqlDbType.Int32) { Value = st.PaymentTransactionID });
            try
            {
                cm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("BukaFlagDownloadBilling => (BillingID = " + st.BillingID + ") " + ex.Message);
            }
        }

        private void UpdateQuote(ref System.Data.Common.DbCommand cm, DateTime tgl, int bankID, int QuoteID, string PaymentSource)
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
                                        SET prospect_convert_flag=2,prospect_appr_code=@appCode,
                                        updated_dt=@tgl,
                                        acquirer_bank_id=@bankid
                                        WHERE `quote_id`=@quoteID;";
                cm.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = tgl });
                cm.Parameters.Add(new MySqlParameter("@bankid", MySqlDbType.Int32) { Value = bankID });
                cm.Parameters.Add(new MySqlParameter("@quoteID", MySqlDbType.Int32) { Value = QuoteID });
                cm.Parameters.Add(new MySqlParameter("@appCode", MySqlDbType.VarChar) { Value = (PaymentSource == "VA" ? "VA" : "UP4Y1") });
                cm.ExecuteNonQuery();

                cm.Parameters.Clear();
                cm.CommandText = @"UPDATE `quote_edc`
                                        SET status_id=1,
                                        reason='',
                                        appr_code=@appCode
                                        WHERE `quote_id`=@quoteID;";
                cm.Parameters.Add(new MySqlParameter("@quoteID", MySqlDbType.Int32) { Value = QuoteID });
                cm.Parameters.Add(new MySqlParameter("@appCode", MySqlDbType.VarChar) { Value = (PaymentSource == "VA" ? "VA" : "UP4Y1") });
                cm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateQuote => (QuoteID = " + QuoteID.ToString() + ") " + ex.Message);
            }
        }

        private void UpdateBillingQuoteJBS(ref System.Data.Common.DbCommand cm, StagingUploadVM bm)
        {
            cm.Parameters.Clear();
            cm.CommandType = CommandType.Text;
            cm.CommandText = @"UPDATE `quote_billing` SET `IsDownload`=0,
			                                            `IsClosed`=1,
			                                            `status`='P',
                                                        `PaymentSource`=@PaymentSource,
                                                        `PaidAmount`=@PaidAmount,
                                                        paid_dt=@tglPaid,
                                                        BankIdPaid=@bankid,
			                                            `LastUploadDate`=@tgl,
			                                            `PaymentTransactionID`=@uid,
                                                        UserUpload=@user
		                                            WHERE `quote_id`=@idBill;";
            cm.Parameters.Add(new MySqlParameter("@PaymentSource", MySqlDbType.VarChar) { Value = bm.PaymentSource });
            cm.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.Int32) { Value = Convert.ToInt32(bm.BillingID) });
            cm.Parameters.Add(new MySqlParameter("@PaidAmount", MySqlDbType.Decimal) { Value = bm.amount });
            cm.Parameters.Add(new MySqlParameter("@bankid", MySqlDbType.Int32) { Value = bm.BankidPaid });
            cm.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = bm.TglSkrg });
            cm.Parameters.Add(new MySqlParameter("@tglPaid", MySqlDbType.DateTime) { Value = bm.tgl });
            cm.Parameters.Add(new MySqlParameter("@uid", MySqlDbType.Int32) { Value = bm.PaymentTransactionID });
            cm.Parameters.Add(new MySqlParameter("@user", MySqlDbType.VarChar) { Value = User.Identity.Name });
            try
            {
                cm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateBillingQuoteJBS => (QuoteID = " + bm.BillingID.ToString() + ") " + ex.Message);
            }
        }

        private void UpdateBillingOthersJBS(ref System.Data.Common.DbCommand cm, StagingUploadVM bm)
        {
            cm.Parameters.Clear();
            cm.CommandType = CommandType.Text;
            cm.CommandText = @"UPDATE `billing_others` SET `IsDownload`=0,
			                                            `IsClosed`=1,
			                                            `status_billing`='P',
                                                        `PaymentSource`=@PaymentSource,
			                                            `LastUploadDate`=@tgl,
                                                        BankIdPaid=@bankid,
                                                        paid_date=@tglPaid,
                                                        `PaidAmount`=@PaidAmount,
                                                        Life21TranID=@TransactionID,
			                                            `ReceiptOtherID`=@receiptID,
			                                            `PaymentTransactionID`=@uid,UserUpload=@user
		                                            WHERE `BillingID`=@idBill;";
            cm.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.VarChar) { Value = bm.BillingID });
            cm.Parameters.Add(new MySqlParameter("@PaymentSource", MySqlDbType.VarChar) { Value = bm.PaymentSource });
            cm.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = bm.TglSkrg });
            cm.Parameters.Add(new MySqlParameter("@tglPaid", MySqlDbType.DateTime) { Value = bm.tgl });
            cm.Parameters.Add(new MySqlParameter("@PaidAmount", MySqlDbType.Decimal) { Value = bm.amount });
            cm.Parameters.Add(new MySqlParameter("@bankid", MySqlDbType.Int32) { Value = bm.BankidPaid });
            cm.Parameters.Add(new MySqlParameter("@TransactionID", MySqlDbType.Int32) { Value = bm.life21TranID });
            cm.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = bm.receipt_other_id });
            cm.Parameters.Add(new MySqlParameter("@uid", MySqlDbType.Int32) { Value = bm.PaymentTransactionID });
            cm.Parameters.Add(new MySqlParameter("@user", MySqlDbType.VarChar) { Value = User.Identity.Name });
            try
            {
                cm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateBillingOthersJBS => (BillingID = " + bm.BillingID.ToString() + ") " + ex.Message);
            }
        }

        private void UpdateBillingJBS(ref System.Data.Common.DbCommand cm, StagingUploadVM bm)
        {
            /// update billing jadi closed 
            /// untuk payment sukses aja
            cm.Parameters.Clear();
            cm.CommandType = CommandType.Text;
            cm.CommandText = @"UPDATE `billing` SET `IsDownload`=0,
                                        `IsClosed`=1,
                                        `BillingDate`=case when @PaymentSource in ('VA','AC') then @tgl 
                                                            else COALESCE(`BillingDate`,@tgl) end,
                                        `status_billing`=@statusBil,
                                        `PaymentSource`=@PaymentSource,
                                        `LastUploadDate`=@tgl,
                                        `BankIdPaid`=@bankid,
                                        `paid_date`=@tglPaid,
                                        `PaidAmount`=@PaidAmount,
                                        `Life21TranID`=@TransactionID,
                                        `ReceiptID`=@receiptID,
                                        `ReceiptOtherID`=@ReceiptOtherID,
                                        `PaymentTransactionID`=@uid,
                                        `ACCname`=@ACCname,
                                        `ACCno`=@ACCno,
                                        `cc_expiry`=@cc_expiry,
                                        `PolisRefundId`=@RefundId,
                                        `UserUpload`=@userupload
                                    WHERE `BillingID`=@idBill;";
            cm.Parameters.Add(new MySqlParameter("@PaymentSource", MySqlDbType.VarChar) { Value = bm.PaymentSource });
            cm.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = bm.TglSkrg });
            cm.Parameters.Add(new MySqlParameter("@statusBil", MySqlDbType.VarChar) { Value = bm.StatusBilling });
            cm.Parameters.Add(new MySqlParameter("@TransactionID", MySqlDbType.Int32) { Value = bm.life21TranID });
            cm.Parameters.Add(new MySqlParameter("@uid", MySqlDbType.Int32) { Value = bm.PaymentTransactionID });
            cm.Parameters.Add(new MySqlParameter("@bankid", MySqlDbType.Int32) { Value = bm.BankidPaid });
            cm.Parameters.Add(new MySqlParameter("@tglPaid", MySqlDbType.DateTime) { Value = bm.tgl });
            cm.Parameters.Add(new MySqlParameter("@PaidAmount", MySqlDbType.Decimal) { Value = bm.amount });
            cm.Parameters.Add(new MySqlParameter("@receiptID", MySqlDbType.Int32) { Value = bm.receipt_id });
            cm.Parameters.Add(new MySqlParameter("@ReceiptOtherID", MySqlDbType.Int32) { Value = bm.receipt_other_id });

            cm.Parameters.Add(new MySqlParameter("@ACCname", MySqlDbType.VarChar) { Value = bm.ACCname });
            cm.Parameters.Add(new MySqlParameter("@ACCno", MySqlDbType.VarChar) { Value = bm.ACCno });
            cm.Parameters.Add(new MySqlParameter("@cc_expiry", MySqlDbType.VarChar) { Value = bm.CC_Expiry });

            cm.Parameters.Add(new MySqlParameter("@userupload", MySqlDbType.VarChar) { Value = User.Identity.Name });
            cm.Parameters.Add(new MySqlParameter("@RefundId", MySqlDbType.Int32) { Value = bm.PolisRefundId });
            cm.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.Int32) { Value = bm.BillingID });

            try
            {
                cm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateBillingJBS => (BillingID = " + bm.BillingID.ToString() + ") " + ex.Message);
            }
        }

        private void UpdateLastTransJBS(ref System.Data.Common.DbCommand cm, StagingUploadVM bm)
        {
            cm.Parameters.Clear();
            cm.CommandType = CommandType.Text;
            cm.CommandText = @"INSERT INTO `policy_last_trans`(`policy_Id`,`BillingID`,`BillingDate`,`recurring_seq`,`due_dt_pre`,`source`,`receipt_id`,`receipt_date`,`bank_id`,`UserCrt`)
                            SELECT @policyID, bx.`BillingID`,bx.`BillingDate`,bx.`recurring_seq`,bx.`due_dt_pre`,bx.`PaymentSource`,bx.`ReceiptID`,@tgl,bx.`BankIdDownload`,@usercrt
                            FROM `policy_last_trans` AS pt
                            INNER JOIN `billing` AS bx ON bx.policy_id=pt.policy_Id
                            WHERE pt.policy_Id=@policyID AND bx.BillingID=@idBill
                            ON DUPLICATE KEY UPDATE `BillingID`=bx.`BillingID`,
	                            `BillingDate`=bx.`BillingDate`,
	                            `recurring_seq`=bx.`recurring_seq`,
	                            `due_dt_pre`=bx.`due_dt_pre`,
	                            `source`=bx.`PaymentSource`,
	                            `receipt_id`=bx.`ReceiptID`,
	                            `receipt_date`=@tgl,
	                            `bank_id`=bx.`BankIdDownload`,
	                            `UserCrt`=@usercrt;";
            cm.Parameters.Add(new MySqlParameter("@policyID", MySqlDbType.Int32) { Value = bm.PolicyId });
            cm.Parameters.Add(new MySqlParameter("@idBill", MySqlDbType.Int32) { Value = bm.BillingID });
            cm.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.DateTime) { Value = bm.TglSkrg });
            cm.Parameters.Add(new MySqlParameter("@usercrt", MySqlDbType.VarChar) { Value = User.Identity.Name });

            try
            {
                cm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateLastTransJBS => (BillingID = " + bm.BillingID.ToString() + ") " + ex.Message);
            }
        }

        private void InsertPolisHold(ref System.Data.Common.DbCommand cmd, string billCode, string polisNo, DateTime releaseDate)
        {
            cmd.Parameters.Clear();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = @"InsertPolisHold";
            cmd.Parameters.Add(new MySqlParameter("@Billcode", MySqlDbType.VarChar) { Value = billCode });
            cmd.Parameters.Add(new MySqlParameter("@polisno", MySqlDbType.VarChar) { Value = polisNo });
            cmd.Parameters.Add(new MySqlParameter("@realeaseDate", MySqlDbType.DateTime) { Value = releaseDate });

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("InsertPolisHold => (polisNo = " + polisNo + ") " + ex.Message);
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
                        stg.CountUpload = (result["TotalUpload"] == DBNull.Value) ? 0 : Convert.ToInt32(result["TotalUpload"]);
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
                        }
                        else if (result["BillCode"].ToString() == "B")
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
        }

        private void PindahFileDownload(string trancode)
        {
            string fileSearch = "";
            string payMeth = "";
            int bankid = 0;

            if (trancode == "bcacc")
            {
                fileSearch = "CAF*.prn";
                bankid = 1;
                payMeth = "CC";
            }
            else if (trancode == "mandiricc")
            {
                fileSearch = "Mandiri_*.xls";
                bankid = 2;
                payMeth = "CC";
            }
            else if (trancode == "megaonus")
            {
                fileSearch = "CAF*_MegaOnUs.bpmt";
                bankid = 3;
                payMeth = "CC";
            }
            else if (trancode == "megaoffus")
            {
                fileSearch = "CAF*_MegaOffUs.bpmt";
                bankid = 4;
                payMeth = "CC";
            }
            else if (trancode == "bnicc")
            {
                fileSearch = "BNI_*.xlsx";
                bankid = 5;
                payMeth = "CC";
            }
            else if (trancode == "bcaac")
            {
                fileSearch = "BCAac*.xls";
                bankid = 1;
                payMeth = "AC";
            }
            else if (trancode == "mandiriac")
            {
                fileSearch = "MandiriAc*.csv";
                bankid = 2;
                payMeth = "AC";
            }
            else
            {
                return;
            }


            string[] files = Directory.GetFiles(DirBilling, fileSearch, SearchOption.TopDirectoryOnly);

            var validasi = CekDataDownload(bankid, payMeth);
            if (validasi != "")
            {
                foreach (string file in files)
                {
                    FileInfo filex = new FileInfo(file);
                    string xFileName = Path.GetFileNameWithoutExtension(filex.Name) + Guid.NewGuid().ToString().Substring(0, 8) + Path.GetExtension(filex.Name);
                    if (filex.Exists) filex.MoveTo(BackupFile + xFileName);
                    //if (filex.Exists) System.IO.File.Delete(filex.ToString());
                }
            }
        }

        private string CreateBilling(ref System.Data.Common.DbCommand cmd, string polisNo, string payMeth, int bankid)
        {
            if (payMeth == string.Empty) return null;
            var query = QureyCreateBilling(payMeth);
            string sql = query + @"SELECT b.`BillingID` FROM `billing` b
                            INNER JOIN `policy_billing` pb ON pb.`policy_Id`=b.`policy_id`
                            WHERE pb.`policy_no`=@polisNo
                            ORDER BY b.`recurring_seq` DESC
                            LIMIT 1; ";
            cmd.Parameters.Clear();
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add(new MySqlParameter("@polisNo", MySqlDbType.VarChar) { Value = polisNo });
            cmd.Parameters.Add(new MySqlParameter("@Usercrt", MySqlDbType.VarChar) { Value = User.Identity.Name });
            cmd.CommandText = sql;


            try
            {
                return cmd.ExecuteScalar().ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("CreateBilling => (polisNo = " + polisNo + ") " + ex.Message);
            }
        }

        private string QureyCreateBilling(string paymentType)
        {
            // CC atau AC
            string query = "";
            if (paymentType == "CC")
            {
                query = @"INSERT INTO `billing`(`BillingDate`,`policy_id`,`recurring_seq`,`due_dt_pre`,`policy_regular_premium`,`cashless_fee_amount`,`TotalAmount`,`AccNo`,`AccName`,`cc_expiry`,UserCrt)
SELECT CURDATE(),
	pb.`policy_Id`, 
	COALESCE(b.`recurring_seq`,pt.`recurring_seq`,0)+1 AS seq,
	DATE_ADD(COALESCE(b.`due_dt_pre`,pt.`due_dt_pre`,pb.`commence_dt`),INTERVAL pb.`premium_mode` MONTH) AS date_pre,
	pb.`regular_premium`,
	pb.`cashless_fee_amount`,
	(pb.`regular_premium`+pb.`cashless_fee_amount`) AS total,
	pc.`cc_no`,
	pc.`cc_name`,
	pc.`cc_expiry`,@Usercrt
FROM `policy_billing` pb
INNER JOIN `policy_cc` pc ON pc.`PolicyId`=pb.`policy_Id`
LEFT JOIN `policy_last_trans` pt ON pt.`policy_Id`=pb.`policy_Id`
LEFT JOIN `billing` b ON b.`policy_id`=pb.`policy_Id`
WHERE pb.`policy_no`=@polisNo
ORDER BY b.`recurring_seq` DESC
LIMIT 1;";
            }
            else if (paymentType == "AC")
            {
                query = @"INSERT INTO `billing`(`BillingDate`,`policy_id`,`recurring_seq`,`due_dt_pre`,`policy_regular_premium`,`cashless_fee_amount`,`TotalAmount`,`AccNo`,`AccName`,UserCrt)
SELECT CURDATE(),
	pb.`policy_Id`, 
	COALESCE(b.`recurring_seq`,pt.`recurring_seq`,0)+1 AS seq,
	DATE_ADD(COALESCE(b.`due_dt_pre`,pt.`due_dt_pre`,pb.`commence_dt`),INTERVAL pb.`premium_mode` MONTH) AS date_pre,
	pb.`regular_premium`,
	pb.`cashless_fee_amount`,
	(pb.`regular_premium`+pb.`cashless_fee_amount`) AS total,
	pa.`acc_no`,
	pa.`acc_name`,@Usercrt
FROM `policy_billing` pb
INNER JOIN `policy_ac` pa ON pa.`PolicyId`=pb.`policy_Id`
LEFT JOIN `policy_last_trans` pt ON pt.`policy_Id`=pb.`policy_Id`
LEFT JOIN `billing` b ON b.`policy_id`=pb.`policy_Id`
WHERE pb.`policy_no`=@polisNo
ORDER BY b.`recurring_seq` DESC
LIMIT 1;";
            }
            else
            {
                query = @"INSERT INTO `billing`(`BillingDate`,`policy_id`,`recurring_seq`,`due_dt_pre`,`policy_regular_premium`,`cashless_fee_amount`,`TotalAmount`,UserCrt)
SELECT CURDATE(),
	pb.`policy_Id`, 
	COALESCE(b.`recurring_seq`,pt.`recurring_seq`,0)+1 AS seq,
	DATE_ADD(COALESCE(b.`due_dt_pre`,pt.`due_dt_pre`,pb.`commence_dt`),INTERVAL pb.`premium_mode` MONTH) AS date_pre,
	pb.`regular_premium`,
	pb.`cashless_fee_amount`,
	(pb.`regular_premium`+pb.`cashless_fee_amount`) AS total,@Usercrt
FROM `policy_billing` pb
LEFT JOIN `policy_last_trans` pt ON pt.`policy_Id`=pb.`policy_Id`
LEFT JOIN `billing` b ON b.`policy_id`=pb.`policy_Id`
WHERE pb.`policy_no`=@polisNo
ORDER BY b.`recurring_seq` DESC
LIMIT 1;";
            }
            return query;
        }

        private int InsertPolisRefund(ref System.Data.Common.DbCommand cmd, PolicyRefundVM pf)
        {
            cmd.Parameters.Clear();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"INSERT INTO `policy_refund`(`policy_id`,`policy_commence_dt`,
`refund_date`,`refund_type`,`policy_regular_premium`,`policy_single_premium`,`total_amount`,`receipt_id`,`receipt_other_id`)
SELECT @policy_id, @commence_dt, @refund_date, @refund_type, @regular_premium, @single_premium, @total_amount, @receipt_id, @receipt_other_id;
SELECT LAST_INSERT_ID();";
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add(new MySqlParameter("@policy_id", MySqlDbType.Int32) { Value = pf.PolicyId });
            cmd.Parameters.Add(new MySqlParameter("@commence_dt", MySqlDbType.Date) { Value = pf.commenceDate });
            cmd.Parameters.Add(new MySqlParameter("@refund_date", MySqlDbType.Date) { Value = pf.refundDate });
            cmd.Parameters.Add(new MySqlParameter("@refund_type", MySqlDbType.Int32) { Value = pf.refundType });
            cmd.Parameters.Add(new MySqlParameter("@regular_premium", MySqlDbType.Decimal) { Value = pf.regularPremium });
            cmd.Parameters.Add(new MySqlParameter("@single_premium", MySqlDbType.Decimal) { Value = pf.singlePremium });
            cmd.Parameters.Add(new MySqlParameter("@total_amount", MySqlDbType.Decimal) { Value = pf.totalAmount });
            cmd.Parameters.Add(new MySqlParameter("@receipt_id", MySqlDbType.Int32) { Value = pf.receiptId });
            cmd.Parameters.Add(new MySqlParameter("@receipt_other_id", MySqlDbType.Int32) { Value = pf.receiptOtherId });
            int hasil = 0;
            try
            {
                hasil = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                throw new Exception("InsertPolisRefund => (polisNo = " + pf.PolicyId + ") " + ex.Message);
                //return 0
            }
            return hasil;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            var ColReceiver = email.Split(',');
            foreach (string mls in ColReceiver) emailMessage.To.Add(new MailboxAddress(mls.Trim()));

            emailMessage.From.Add(new MailboxAddress("JAGADIRI", EmailCAF));
            //emailMessage.To.Add(new MailboxAddress(email));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = message;
            //bodyBuilder.TextBody = "This is some plain text";

            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                //client.LocalDomain = "some.domain.com";
                await client.ConnectAsync("mail.caf.co.id", 25, SecureSocketOptions.None).ConfigureAwait(false);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
        public async Task SendEmailAsync(string email, string subject, string message, string bcc)
        {
            var emailMessage = new MimeMessage();

            var ColBcc = bcc.Split(',');
            foreach (string mls in ColBcc) emailMessage.Bcc.Add(new MailboxAddress(mls.Trim()));

            emailMessage.From.Add(new MailboxAddress("JAGADIRI", EmailCAF));
            emailMessage.To.Add(new MailboxAddress(email));

            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = message;
            //bodyBuilder.TextBody = "This is some plain text";

            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                //client.LocalDomain = "some.domain.com";
                await client.ConnectAsync("mail.caf.co.id", 25, SecureSocketOptions.None).ConfigureAwait(false);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
        public async Task AsyncSendEmailThanksRecurring(int BillID)
        {
            EmailThanksRecurringVM EmailThanks;
            EmailThanks = await (from b in _jbsDB.BillingModel
                                 join pb in _jbsDB.PolicyBillingModel on b.policy_id equals pb.policy_Id
                                 join ci in _jbsDB.CustomerInfo on pb.holder_id equals ci.CustomerId
                                 join pd in _jbsDB.Product on pb.product_id equals pd.product_id
                                 where b.BillingID == BillID
                                 select new EmailThanksRecurringVM()
                                 {
                                     PolicyNo = pb.policy_no,
                                     CustomerName = ci.CustomerName,
                                     Salam = (ci.IsLaki == true) ? "Bapak" : "Ibu",
                                     CustomerEmail = ci.Email,
                                     ProductName = pd.product_description,
                                     PremiAmount = b.TotalAmount
                                 }).SingleOrDefaultAsync();
            string SubjectEmail = string.Format(@"JAGADIRI: Penerimaan Premi Regular {0} {1} {2}", EmailThanks.ProductName, EmailThanks.PolicyNo, EmailThanks.CustomerName.ToUpper());
            string BodyMessage = string.Format(@"Salam hangat {0} {1},<br>
<p style='text-align:justify'>Bersama surat ini kami ingin mengucapkan terima kasih atas pembayaran Premi Regular untuk Polis {2} dengan nomor polis {3} sejumlah IDR {4} yang telah kami terima. Pembayaran Premi tersebut secara otomatis akan membuat Polis Asuransi Anda tetap aktif dan memberikan manfaat perlindungan maksimal bagi Anda dan keluarga.</p>
<br>Sukses selalu,
<br>JAGADIRI ", EmailThanks.Salam, EmailThanks.CustomerName.ToUpper(), EmailThanks.ProductName, EmailThanks.PolicyNo, EmailThanks.PremiAmount.ToString("#,###"));
            try
            {
                await SendEmailAsync(EmailThanks.CustomerEmail, SubjectEmail, BodyMessage, EmailPHS);
            }
            catch (Exception ex)
            {
                throw new Exception("AsyncSendEmailThanksRecurring => (BillID = " + BillID.ToString() + ") " + ex.Message);
            }
        }
        public async Task AsyncSendEmailThanksQuote(int Quoteid, Decimal jlhBayar)
        {
            var emailQ = await (from qb in _jbsDB.QuoteBillingModel
                                join q in _jbsDB.Quote on qb.quote_id equals q.quote_id
                                join pd in _jbsDB.Product on qb.product_id equals pd.product_id
                                where qb.quote_id == Quoteid
                                select new EmailThanksQuoteVM()
                                {
                                    QuoteID = q.quote_id,
                                    RefNo = qb.ref_no,
                                    Email = q.email,
                                    Gender = (q.IsLaki ? "Pria" : "Wanita"),
                                    Sapaan = (q.IsLaki ? "Bapak" : "Ibu"),
                                    CustName = q.prospect_name,
                                    POB = q.POB,
                                    DOB = q.DOB,
                                    MobileNo = q.mobile_phone,
                                    ProductName = pd.product_description,
                                    Insured = q.sum_insured,
                                    DurasiTahun = q.duration,
                                    DurasiHari = q.duration_days,
                                    PremiAmount = qb.prospect_amount,
                                    CetakPolisAmount = qb.paper_print_fee,
                                    FrekuensiBayar = (q.premium_mode == 0 ? "Sekaligus" : (q.premium_mode == 1 ? "Bulanan" : (q.premium_mode == 3 ? "Triwulanan" : (q.premium_mode == 6 ? "Semesteran" : (q.premium_mode == 12 ? "Tahunan" : "-"))))),
                                    PaymentMeth = q.payment_method,
                                    PaymentAmount = jlhBayar
                                }).SingleOrDefaultAsync();
            if (emailQ == null) throw new Exception("AsyncSendEmailThanksQuote (QuoteID = " + Quoteid.ToString() + ") Email Qoute gagal");

            string SubjectEmail = string.Format(@"JAGADIRI: Nomor Quotation: {0} TERBAYAR", emailQ.RefNo);
            string cetakPolis = "";
            if ((emailQ.CetakPolisAmount != null) && (emailQ.CetakPolisAmount > 0))
            {
                cetakPolis = string.Format(@"<tr><td>Biaya Cetak Polis</td>  <td>: IDR {0}</td></tr>", Convert.ToDecimal(emailQ.CetakPolisAmount).ToString("#,###"));
            }
            string BodyMessage = string.Format(@"Dengan Hormat {0} {1},
<p style='text-align:justify'>Terima kasih atas Pembayaran Asuransi Anda. Permohonan Asuransi Anda akan segera kami proses dan kami akan informasikan Anda kembali via email </p>
<table>
<tr><td></td><td></td></tr>
    <tr><td>No Quote</td>               <td>: {2}</td></tr>
    <tr><td>Nama</td>                   <td>: {1}</td></tr>
    <tr><td>Jenis Kelamin</td>          <td>: {3}</td></tr>
    <tr><td>Tempat/Tanggal Lahir</td>   <td>: {4}/{5}</td></tr>
    <tr><td>Email</td>                  <td>: {6}</td></tr>
    <tr><td>Mobile</td>                 <td>: {7}</td></tr>
    <tr><td>Nama Product</td>           <td>: {8}</td></tr>
    <tr><td>Uang Pertanggungan</td>     <td>: IDR {9}</td></tr>
    <tr><td>Durasi (tahun)</td>         <td>: {10} tahun</td></tr>
    <tr><td>Durasi (hari)</td>          <td>: {11} hari</td></tr>
    <tr><td>Total Premi</td>            <td>: IDR {12}</td></tr>" +
    cetakPolis +
    @"<tr><td>Frekuensi Bayar</td>        <td>: {13}</td></tr>
<tr><td><br></td><td></td></tr>
    <tr><td>Pembayaran</td><td></td></tr>
    <tr><td>Metode Pembayaran</td>      <td>: {14}</td></tr>
    <tr><td>Jumlah Pembayaran</td>      <td>: IDR {15}</td></tr>
    <tr><td>Status</td>                 <td>: TERDAFTAR</td></tr>
</table>
<br><br>Sukses selalu,
<br>JAGADIRI ", emailQ.Sapaan,
emailQ.CustName.ToUpper(),
emailQ.RefNo,
emailQ.Gender,
emailQ.POB,
emailQ.DOB == null ? "" : Convert.ToDateTime(emailQ.DOB).ToString("dd MMM yyyy"),
emailQ.Email,
emailQ.MobileNo,
emailQ.ProductName.ToString(),
emailQ.Insured == null ? "" : Convert.ToDecimal(emailQ.Insured).ToString("#,###"),
emailQ.DurasiTahun,
emailQ.DurasiHari,
emailQ.PremiAmount == null ? "" : Convert.ToDecimal(emailQ.PremiAmount).ToString("#,###"),
emailQ.FrekuensiBayar,
emailQ.PaymentMeth,
emailQ.PaymentAmount == null ? "" : Convert.ToDecimal(emailQ.PaymentAmount).ToString("#,###")
);
            try
            {
                await SendEmailAsync(emailQ.Email, SubjectEmail, BodyMessage, EmailPHS);
            }
            catch (Exception ex)
            {
                throw new Exception("AsyncSendEmailThanksQuote => (Quoteid = " + Quoteid.ToString() + ") " + ex.Message);
            }
        }
        public async Task AsyncSendEmailThanksEndorsemen(string BillID)
        {
            EmailThanksBillOthersVM EmailThanks;
            EmailThanks = await (from b in _jbsDB.BillingOtherModel
                                 join pb in _jbsDB.PolicyBillingModel on b.policy_id equals pb.policy_Id
                                 join ci in _jbsDB.CustomerInfo on pb.holder_id equals ci.CustomerId
                                 join pd in _jbsDB.Product on pb.product_id equals pd.product_id
                                 where b.BillingID == BillID.ToString()
                                 select new EmailThanksBillOthersVM()
                                 {
                                     PolicyNo = pb.policy_no,
                                     CustomerName = ci.CustomerName,
                                     Salam = (ci.IsLaki == true) ? "Bapak" : "Ibu",
                                     CustomerEmail = ci.Email,
                                     ProductName = pd.product_description,
                                     ProductType = (b.BillingType == "A2" ? "Endorsemen Cetak Polis Fisik" : (b.BillingType == "A3" ? "Cetak Kartu" : "")),
                                     PremiAmount = b.TotalAmount
                                 }).SingleOrDefaultAsync();
            if (string.IsNullOrEmpty(EmailThanks.ProductType)) return;

            string SubjectEmail = string.Format(@"JAGADIRI: Pembayaran {0}", EmailThanks.ProductType);
            string BodyMessage = string.Format(@"Salam hangat {0} {1},<br>
<p style='text-align:justify'>Bersama surat ini kami ingin mengucapkan terima kasih atas pembayaran {2} untuk Polis {3} 
dengan nomor polis {4} sejumlah {5} yang telah kami terima. Pembayaran Premi tersebut secara otomatis akan membuat Polis Asuransi Anda tetap aktif dan memberikan manfaat perlindungan maksimal bagi Anda dan keluarga.</p>
<br>Sukses selalu,
<br>JAGADIRI ", EmailThanks.Salam, EmailThanks.CustomerName.ToUpper(), EmailThanks.ProductType, EmailThanks.ProductName, EmailThanks.PolicyNo, EmailThanks.PremiAmount.ToString("#,###"));
            try
            {
                await SendEmailAsync(EmailThanks.CustomerEmail, SubjectEmail, BodyMessage, EmailPHS);
            }
            catch (Exception ex)
            {
                throw new Exception("AsyncSendEmailThanksEndorsemen => (BillID = " + BillID + ") " + ex.Message);
            }
        }
        public async Task AsyncSendEmailRefundCancelRecurring(int BillID)
        {
            var EmailRefund = await (from b in _jbsDB.BillingModel
                                     join pb in _jbsDB.PolicyBillingModel on b.policy_id equals pb.policy_Id
                                     join ci in _jbsDB.CustomerInfo on pb.holder_id equals ci.CustomerId
                                     join pd in _jbsDB.Product on pb.product_id equals pd.product_id
                                     where b.BillingID == BillID
                                     select new EmailRefundVM()
                                     {
                                         PolicyNo = pb.policy_no,
                                         CustomerName = ci.CustomerName,
                                         ProductName = pd.product_description,
                                         PolicyStatus = pb.Policy_status

                                     }).SingleOrDefaultAsync();
            string SubjectEmail = string.Format(@"JAGADIRI: SUCCESS RECURRING CANCEL POLICY");
            string BodyMessage = string.Format(@"<table><tr><td>PolicyNo</td><td>: {0}</td></tr>
<tr><td>Holder Name</td><td>: {1}</td></tr>
<tr><td>Product Description</td><td>: {2}</td></tr>
<tr><td>Policy Status</td><td>: {3}</td></tr></table>",
EmailRefund.PolicyNo, EmailRefund.CustomerName, EmailRefund.ProductName, EmailRefund.PolicyStatus);
            string receiver = EmailCS + "," + EmailFA + "," + EmailPHS + "," + EmailBilling;
            try
            {
                await SendEmailAsync(receiver, SubjectEmail, BodyMessage);
            }
            catch (Exception ex)
            {
                throw new Exception("AsyncSendEmailRefundCancelRecurring => (BillID = " + BillID + ") " + ex.Message);
            }
        }
        public ActionResult FileSetting()
        {
            FileStringVM fls = new FileStringVM();
            foreach (String fs in filesettings.s)
            {
                fls.files = fls.files + fs.ToString();
            }

            return View(fls);
        }

        private DateTime? CekJobJalan()
        {
            DateTime? tgl = null;
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"SELECT g.`Tgl` FROM `GeneralSetting` g";
            try
            {
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                tgl = Convert.ToDateTime(cmd.ExecuteScalar());
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally { cmd.Connection.Close(); }
            return tgl;
        }

    }
}