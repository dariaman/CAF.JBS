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
using Vereyon.Web;
using System.Net;

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

        private readonly string CIMBOnUsccFile;
        private readonly string CIMBOffUsccFile;

        private readonly string BCAacFile;
        private readonly string MandiriAcFile;
        private readonly string VaRegulerPremi;

        private readonly string TempBniFile;
        private readonly string TempMandiriFile;
        private readonly string TempBCAacFile;

        private readonly string GenerateXls;
        private readonly string EmailCAF, EmailPHS, EmailFA, EmailCS, EmailBilling;

        private IFlashMessage flashMessage;
        private FileSettings filesettings;

        public BillingFileController(JbsDbContext context1, Life21DbContext context2,  UserDbContext context3, IFlashMessage flash)
        {
            filesettings = new FileSettings();
            _jbsDB = context1;
            _life21 = context2;
            _user = context3;
            flashMessage = flash;

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

            CIMBOnUsccFile = filesettings.CIMBOnUsccFile;
            CIMBOffUsccFile = filesettings.CIMBOffUsccFile;

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
            List<DownloadBillingVM> DownloadBillVM = new List<DownloadBillingVM>();
            DownloadBillingVM dwd = new DownloadBillingVM();
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"SELECT bd.`row_span`,bd.`group_name`,bd.`group_code`,bd.`id`,bd.`file_download`,bd.`Judul`,
                        bd.`TotalCountDWD`,bd.`TotalAmountDWD`,bd.`BillingAmountDWD`,bd.`BillingCountDWD`,bd.`OthersAmountDWD`,
                        bd.`OthersCountDWD`,bd.`QuoteAmountDWD`,bd.`QuoteCountDWD`
                        FROM `billing_download_summary` bd
                        ORDER BY bd.`group_order`,bd.`id`; ";
            try
            {
                cmd.Connection.Open();

                using (var result = cmd.ExecuteReader())
                {
                    while (result.Read())
                    {
                        dwd = new DownloadBillingVM();
                        dwd.id = result["id"].ToString().Trim() == "" ? 0 : Convert.ToInt32(result["id"]);
                        dwd.file_download = result["file_download"].ToString().Trim();
                        dwd.judul = result["Judul"].ToString().Trim();
                        dwd.group_name = result["group_name"].ToString().Trim();
                        dwd.group_code = result["group_code"].ToString().Trim();
                        dwd.row_span = result["row_span"].ToString().Trim();

                        dwd.total_count_billing = result["TotalCountDWD"].ToString() == "" ? 0 : Convert.ToInt32(result["TotalCountDWD"]);

                        dwd.recurring_count_billing = result["BillingCountDWD"].ToString() == "" ? 0 : Convert.ToInt32(result["BillingCountDWD"]);
                        dwd.other_count_billing = result["OthersCountDWD"].ToString() == "" ? 0 : Convert.ToInt32(result["OthersCountDWD"]);
                        dwd.quote_count_billing = result["QuoteCountDWD"].ToString() == "" ? 0 : Convert.ToInt32(result["QuoteCountDWD"]);

                        dwd.total_amount_billing = result["TotalAmountDWD"].ToString() == "" ? 0 : Convert.ToDecimal(result["TotalAmountDWD"]);
                        dwd.recurring_amount_billing = result["BillingAmountDWD"].ToString() == "" ? 0 : Convert.ToDecimal(result["BillingAmountDWD"]);
                        dwd.other_amount_billing = result["OthersAmountDWD"].ToString() == "" ? 0 : Convert.ToDecimal(result["OthersAmountDWD"]);
                        dwd.quote_amount_billing = result["QuoteAmountDWD"].ToString() == "" ? 0 : Convert.ToDecimal(result["QuoteAmountDWD"]);

                        DownloadBillVM.Add(dwd);
                    }
                }
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
            return View(DownloadBillVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Download(ViewModels.DownloadBillingVM dw)
        {
            /*
             * kode bank sbg info di keterangan
             * 0. Bank Lain-lain
             * 1. BCA
             * 2. Mandiri
             * 3. Mega
             * 4. CIMB
             * BCA harus paling atas, karena pengaruh untuk produk Flexy Link
            */

            // download file CC Billing
            if (dw.BcaCC || dw.MandiriCC || dw.MegaCC || dw.BniCC || dw.CimbCC)
            {
                // ================== Pilihan 1 bank ========================================
                if (dw.BcaCC && !(dw.MandiriCC || dw.MegaCC || dw.BniCC || dw.CimbCC))
                {   // BCA saja
                    GenBcaCCFile(0); // BCA 1 2 3 4 0 (ALL)
                }
                else if (dw.MandiriCC && !(dw.BcaCC || dw.MegaCC || dw.BniCC || dw.CimbCC))
                {   // Mandiri saja
                    GenMandiriCCFile(); // Mandiri 2
                }
                else if (dw.MegaCC && !(dw.BcaCC || dw.MandiriCC || dw.BniCC || dw.CimbCC))
                {   // Mega saja
                    GenMegaOnUsCCFile(); // MegaOn 3
                    GenMegaOffUsCCFile(0); // MegaOff 1 2 4 0(ALL <>3)
                }
                else if (dw.BniCC && !(dw.BcaCC || dw.MandiriCC || dw.MegaCC || dw.CimbCC))
                {   // BNI aja
                    GenBniCCFile(0); // BNI 1 2 3 4 0 (ALL)
                }
                else if (dw.CimbCC && !(dw.BcaCC || dw.MandiriCC || dw.MegaCC || dw.BniCC))
                {   // CIMB aja
                    GenCimbNiagaOnUsCCFile(); // CIMB 4
                    GenCimbNiagaOffUsCCFile(0); // CIMB 1 2 3  0 (ALL <> 4)
                }

                // ==================== Pilihan 2 Bank ==================================================
                else if (dw.BcaCC && dw.MandiriCC && !(dw.MegaCC || dw.BniCC || dw.CimbCC))
                {   // jika dipilih BCA dan Mandiri
                    GenBcaCCFile(2); // BCA 1 3 4 (<> 2)
                    GenMandiriCCFile(); // Mandiri 2
                }
                else if (dw.BcaCC && dw.MegaCC && !(dw.MandiriCC || dw.BniCC || dw.CimbCC))
                {   // jika dipilih BCA dan Mega
                    GenBcaCCFile(1); // BCA 1 
                    GenMegaOnUsCCFile(); // MegaOn 3
                    GenMegaOffUsCCFile(1); // MegaOff 2 4 (<> 1 3)
                }
                else if (dw.MandiriCC && dw.MegaCC && !(dw.BcaCC || dw.BniCC || dw.CimbCC))
                {   // jika dipilih Mandiri dan Mega
                    GenMandiriCCFile(); // Mandiri 2
                    GenMegaOnUsCCFile(); // MegaOn 3
                    GenMegaOffUsCCFile(3); // MegaOff 1 4 (<> 2 3)
                }
                else if (dw.BcaCC && dw.BniCC && !(dw.MandiriCC || dw.MegaCC || dw.CimbCC))
                {   // jika dipilih BCA dan BNI
                    GenBcaCCFile(1); // BCA 1
                    GenBniCCFile(1); // BNI 2 3 4 (<> 1)
                }
                else if (dw.MandiriCC && dw.BniCC && !(dw.BcaCC || dw.MegaCC || dw.CimbCC))
                {   // jika dipilih Mandiri dan BNI
                    GenMandiriCCFile(); // Mandiri 2
                    GenBniCCFile(2); // BNI 1 3 4 (<> 2)
                }
                else if (dw.BcaCC && dw.CimbCC && !(dw.MegaCC || dw.MandiriCC || dw.BniCC))
                {   // jika dipilih BCA dan CIMB
                    GenBcaCCFile(1); // BCA 1
                    GenCimbNiagaOnUsCCFile(); // CIMB 4
                    GenCimbNiagaOffUsCCFile(1); // CIMB 2 3  0 (ALL <> 1 4)
                }
                else if (dw.MandiriCC && dw.CimbCC && !(dw.MegaCC || dw.BcaCC || dw.BniCC))
                {   // jika dipilih Mandiri dan CIMB
                    GenMandiriCCFile(); // Mandiri 2
                    GenCimbNiagaOnUsCCFile(); // CIMB 4
                    GenCimbNiagaOffUsCCFile(3); // CIMB 1 3  0 (ALL <> 2 4)
                }

                /* ================Pilihan 3 Bank====================================*/
                else if (dw.BcaCC && dw.MandiriCC && dw.MegaCC && !(dw.BniCC || dw.CimbCC))
                {   // jika dipilih BCA,Mandiri dan Mega                    
                    GenBcaCCFile(1); // BCA 1
                    GenMandiriCCFile(); // Mandiri 2
                    GenMegaOnUsCCFile(); // MegaOn 3
                    GenMegaOffUsCCFile(2); // MegaOff 4 0 (<> 1,2,3)
                }
                else if (dw.BcaCC && dw.MandiriCC && dw.BniCC && !(dw.MegaCC || dw.CimbCC))
                {   // jika dipilih BCA,Mandiri dan BNI
                    GenBcaCCFile(1); // BCA 1
                    GenMandiriCCFile(); // Mandiri 2
                    GenBniCCFile(3); //BNI 3 4 0 (<> 1,2)
                }
                else if (dw.BcaCC && dw.MandiriCC && dw.CimbCC && !(dw.MegaCC || dw.BniCC))
                {   // jika dipilih BCA,Mandiri dan BNI
                    GenBcaCCFile(1); // BCA 1
                    GenMandiriCCFile(); // Mandiri 2
                    GenCimbNiagaOnUsCCFile(); // CIMB 4
                    GenCimbNiagaOffUsCCFile(2); // CIMB 3 0 (ALL <> 1 2 4)
                }
            }
            if (dw.MandiriAC) GenMandiriAcFile();
            if (dw.BcaAC) GenBcaAcFile();

            if (dw.BcaCC || dw.MandiriCC || dw.MegaCC || dw.BniCC || dw.BcaAC || dw.MandiriAC || dw.CimbCC)
            { // Jika ada aktifitas generate file tuk siap di download
                hitungUlang();

                // Validasi Data Kosong, agar File yg terbentuk dgn data kosong dihapus
                string validasi = "";
                if (dw.BcaCC) // Cek Bca CC
                {
                    validasi = CekDataDownload(1);
                    if (validasi != "")
                    {
                        reset_data_download(1);
                        FileInfo filex = new FileInfo(BCAccFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        flashMessage.Danger(validasi);
                    }
                }

                if (dw.MandiriCC) // Cek mandiri CC
                {
                    validasi = "";
                    validasi = CekDataDownload(2);
                    if (validasi != "")
                    {
                        reset_data_download(2);
                        FileInfo filex = new FileInfo(MandiriccFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        flashMessage.Danger(validasi);
                    }
                }

                if (dw.MegaCC) // Cek Mega CC
                {
                    validasi = "";
                    validasi = CekDataDownload(3);
                    if (validasi != "")
                    {
                        reset_data_download(3);
                        FileInfo filex = new FileInfo(MegaOnUsccFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        flashMessage.Danger(validasi);
                    }

                    validasi = "";
                    validasi = CekDataDownload(4);
                    if (validasi != "")
                    {
                        reset_data_download(4);
                        FileInfo filex = new FileInfo(MegaOfUsccFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        flashMessage.Danger(validasi);
                    }
                }

                if (dw.CimbCC) // Cek CIMB CC
                {
                    validasi = "";
                    validasi = CekDataDownload(8);
                    if (validasi != "")
                    {
                        reset_data_download(8);
                        FileInfo filex = new FileInfo(CIMBOnUsccFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        flashMessage.Danger(validasi);
                    }

                    validasi = "";
                    validasi = CekDataDownload(9);
                    if (validasi != "")
                    {
                        reset_data_download(9);
                        FileInfo filex = new FileInfo(CIMBOffUsccFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        flashMessage.Danger(validasi);
                    }
                }

                if (dw.BniCC) // Cek BNI CC
                {
                    validasi = "";
                    validasi = CekDataDownload(5);
                    if (validasi != "")
                    {
                        reset_data_download(5);
                        FileInfo filex = new FileInfo(BNIccFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        flashMessage.Danger(validasi);
                    }
                }
                if (dw.BcaAC) // Cek BCA CC
                {
                    validasi = "";
                    validasi = CekDataDownload(6);
                    if (validasi != "")
                    {
                        reset_data_download(6);
                        FileInfo filex = new FileInfo(BCAacFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        flashMessage.Danger(validasi);
                    }
                }
                if (dw.MandiriAC) // Cek Mandiri CC
                {
                    validasi = "";
                    validasi = CekDataDownload(7);
                    if (validasi != "")
                    {
                        reset_data_download(7);
                        FileInfo filex = new FileInfo(MandiriAcFile);
                        if (filex.Exists) System.IO.File.Delete(filex.ToString());
                        flashMessage.Danger(validasi);
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult DownloadFile(string fileName)
        {
            FileInfo fl = new FileInfo(DirBilling + fileName);
            if (!fl.Exists)
            {
                flashMessage.Danger("(" + fileName + ") File Not Found");
                return RedirectToAction("Index");
            }
            else
            {
                Response.Headers.Add("content-disposition", "attachment; filename=" + fileName);
                return File(new FileStream(DirBilling + fileName, FileMode.Open), "application/octet-stream");
            }
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
                cmd.CommandText = "BillingBCAcc_sp";
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
                    flashMessage.Danger(ex.Message);
                }
                finally
                {
                    cmd.Dispose();
                    cmd.Connection.Close();
                }

                try
                {
                    UpdateDataFileBilling(1, FileName.Name);
                }
                catch (Exception ex)
                {
                    flashMessage.Danger(ex.Message);
                }
            }
        }
        protected void GenMandiriCCFile()
        {
            FileInfo FileName = new FileInfo(this.MandiriccFile);
            if (!FileName.Exists)
            {
                FileName = new FileInfo(TempMandiriFile);
                FileName.CopyTo(this.MandiriccFile);
                FileName = new FileInfo(this.MandiriccFile);

                using (ExcelPackage package = new ExcelPackage(new FileInfo(this.MandiriccFile)))
                {
                    ExcelWorkbook workBook = package.Workbook;
                    var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "BillingMandiriCC_sp";
                    try
                    {
                        ExcelWorksheet ws = workBook.Worksheets.SingleOrDefault(w => w.Name == "sheet1");
                        cmd.Connection.Open();
                        int j = 1;
                        int i = 16;
                        using (var result = cmd.ExecuteReader())
                        {
                            while (result.Read())
                            {
                                ws.Cells[i, 3].Value = result["a"];
                                ws.Cells[i, 5].Value = result["b"];
                                ws.Cells[i, 7].Value = result["c"];
                                ws.Cells[i, 9].Value = result["d"];
                                ws.Cells[i, 11].Value = result["e"];
                                ws.Cells[i, 13].Value = result["f"];
                                i++;
                                j++;
                            }
                        }
                        //Header file
                        ws.Cells[8, 5].Value = DateTime.Now.ToString("ddMMyyyy");
                        ws.Cells[12, 5].Formula = "=COUNT(G15:G" + (i - 1).ToString() + ")";
                        ws.Cells[13, 5].Formula = "=SUM(G15:G" + (i - 1).ToString() + ")";
                        package.Save();
                    }
                    catch (Exception ex)
                    {
                        if (FileName.Exists) FileName.Delete();
                        flashMessage.Danger(ex.Message);
                        return;
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
                        flashMessage.Danger(ex.Message);
                    }
                }
            }
        }

        protected void GenMegaOnUsCCFile()
        {
            FileInfo FileName = new FileInfo(this.MegaOnUsccFile);
            if (!FileName.Exists) //jika file belum ada akan di generate tp jika sudah ada maka akan pake file exist
            {
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "BillingMegaOnUsCC_sp";
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
                    flashMessage.Danger(ex.Message);
                }
                finally
                {
                    cmd.Dispose();
                    cmd.Connection.Close();
                }

                try
                {
                    UpdateDataFileBilling(3, FileName.Name);
                }
                catch (Exception ex)
                {
                    flashMessage.Danger(ex.Message);
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
                cmd.CommandText = "BillingMegaOffUsCC_sp";
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
                    flashMessage.Danger(ex.Message);
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
                    flashMessage.Danger(ex.Message);
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
                    cmd.CommandText = "BillingBNIcc_sp";
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
                        flashMessage.Danger(ex.Message);
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
                        flashMessage.Danger(ex.Message);
                    }
                }
            }
        }

        protected void GenCimbNiagaOnUsCCFile()
        {
            FileInfo FileName = new FileInfo(this.CIMBOnUsccFile);
            if (!FileName.Exists) //jika file belum ada akan di generate tp jika sudah ada maka akan pake file exist
            {
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "billing_cimb_cc_onus_sp";
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
                                    if (result[0].ToString() == "") break;
                                    writer.Write(result[0]);
                                    writer.WriteLine();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    flashMessage.Danger(ex.Message);
                }
                finally
                {
                    cmd.Dispose();
                    cmd.Connection.Close();
                }

                try
                {
                    UpdateDataFileBilling(8, FileName.Name);
                }
                catch (Exception ex)
                {
                    flashMessage.Danger(ex.Message);
                }
            }
        }
        protected void GenCimbNiagaOffUsCCFile(int id)
        {
            FileInfo FileName = new FileInfo(this.CIMBOffUsccFile);
            if (!FileName.Exists) //jika file belum ada akan di generate tp jika sudah ada maka akan pake file exist
            {
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "billing_cimb_cc_offus_sp";
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
                                    if (result[0].ToString() == "") break;
                                    writer.Write(result[0]);
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
                    UpdateDataFileBilling(9, FileName.Name);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        #endregion

        #region GenerateFileDownloadAC
        protected void GenBcaAcFile()
        {
            FileInfo FileName = new FileInfo(this.BCAacFile);
            if (!FileName.Exists)
            {
                FileName = new FileInfo(TempBCAacFile);
                FileName.CopyTo(this.BCAacFile);
                FileName = new FileInfo(this.BCAacFile);

                using (ExcelPackage package = new ExcelPackage(new FileInfo(this.BCAacFile)))
                {
                    ExcelWorkbook workBook = package.Workbook;
                    var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "BillingBcaAC_sp ";
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
                        flashMessage.Danger(ex.Message);
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
                        flashMessage.Danger(ex.Message);
                    }
                }
            }
        }
        protected void GenMandiriAcFile()
        {
            FileInfo FileName = new FileInfo(this.MandiriAcFile);
            if (!FileName.Exists) //jika file belum ada akan di generate tp jika sudah ada maka tidak akan terjadi apa2
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

                try
                {
                    UpdateDataFileBilling(7, FileName.Name);
                }
                catch (Exception ex)
                {
                    flashMessage.Danger(ex.Message);
                }
            }
        }
        #endregion

        public FileStreamResult DownloadVA()
        {
            string[] files = Directory.GetFiles(DirBilling, "VARegulerPremi*.xlsx", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                FileInfo FileName = new FileInfo(file);
                if (FileName.Exists) System.IO.File.Delete(FileName.ToString());
            }
            var fileName = "VARegulerPremi" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
            var fullePath = DirBilling + fileName;

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

        public void reset_data_download(int trancode)
        {
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"UPDATE `billing_download_summary` bd SET bd.`file_download`=NULL
                                WHERE bd.`id`=@id AND bd.`TotalAmountDWD`<1 AND bd.`TotalCountDWD`<1; ";
            cmd.Parameters.Clear();
            cmd.Parameters.Add(new MySqlParameter("@id", MySqlDbType.Int32) { Value = trancode });
            try
            {
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                flashMessage.Danger(ex.Message);
            }
            finally
            {
                cmd.Connection.Close();
            }
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

        private string CekDataDownload(int trancode)
        {
            string pesan = "", jenisTransaksi = "";
            // Proses cek jlh data yg didownload, jika 0 maka file yang sudah terbentuk harus di hapus
            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CheckDownloadData";
            cmd.Parameters.Add(new MySqlParameter("@trancode", MySqlDbType.Int32) { Value = trancode });

            if (trancode == 1) jenisTransaksi = "BCA CC";
            else if (trancode == 2) jenisTransaksi = "Mandiri CC";
            else if (trancode == 3) jenisTransaksi = "MegaOnUs CC";
            else if (trancode == 4) jenisTransaksi = "MegaOffUs CC";
            else if (trancode == 5) jenisTransaksi = "BNI CC";
            else if (trancode == 6) jenisTransaksi = "BCA AC";
            else if (trancode == 7) jenisTransaksi = "Mandiri AC";
            else if (trancode == 8) jenisTransaksi = "CIMBOnUs CC";
            else if (trancode == 9) jenisTransaksi = "CIMBOffUs CC";

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
            cmd.CommandText = @"UPDATE `billing_download_summary` fl SET fl.`file_download`=@filename WHERE fl.`id`=@id; ";
            cmd.Parameters.Add(new MySqlParameter("@filename", MySqlDbType.VarChar) { Value = Filename });
            cmd.Parameters.Add(new MySqlParameter("@id", MySqlDbType.Int32) { Value = id });
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                flashMessage.Danger(ex.Message);
            }
            finally
            {
                cmd.Connection.Close();
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

        //public ActionResult reset()
        //{
        //    var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
        //    cmd.CommandType = CommandType.Text;
        //    cmd.CommandText = @"UPDATE `billing` AS b SET b.`IsDownload`=0; ";
        //    try
        //    {
        //        cmd.Connection.Open();
        //        cmd.ExecuteNonQuery();
        //        cmd.CommandText = @"UPDATE `billing_download_summary` AS bs
        //                            SET bs.`file_download`=NULL,
        //                            bs.`BillingCountDWD`=0,
        //                            bs.`BillingAmountDWD`=0,
        //                            bs.`OthersCountDWD`=0,
        //                            bs.`OthersAmountDWD`=0,
        //                            bs.`QuoteCountDWD`=0,
        //                            bs.`QuoteAmountDWD`=0,
        //                            bs.`TotalCountDWD`=0,
        //                            bs.`TotalAmountDWD`=0; ";
        //        cmd.ExecuteNonQuery();

        //        cmd.CommandText = @"UPDATE `billing_others` AS b SET b.`IsDownload`=0; ";
        //        cmd.ExecuteNonQuery();

        //        cmd.CommandText = @"UPDATE `quote_billing` AS b SET b.`IsDownload`=0; ";
        //        cmd.ExecuteNonQuery();

        //        var files = Directory.GetFiles(DirBilling);
        //        foreach (string file in files)
        //        {
        //            FileInfo fileBill = new FileInfo(file);
        //            fileBill.Delete();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        cmd.Connection.Close();
        //    }

        //    return RedirectToAction("Index");
        //}
    }
}