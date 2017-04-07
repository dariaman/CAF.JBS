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
using OfficeOpenXml;
using System.Text;
using Npoi.Core.SS.UserModel;
using Npoi.Core.SS.Util;
using Npoi.Core.HSSF.UserModel;
using Npoi.Core.HSSF.Util;

namespace CAF.JBS.Controllers
{
    public class BillingController : Controller
    {
        private readonly JbsDbContext _jbsDB;
        private readonly  string TempFile;      //folder Billing yang standby hari ini
        private readonly string BackupFile;     //folder Backup billing hari2 sebelumnya
        private readonly string Template;       //folder template billing

        private readonly string BCAFile;
        private readonly string MandiriFile;
        private readonly string MegaOnUsFile;
        private readonly string MegaOfUsFile;
        private readonly string BNIFile;
        private readonly string TempBniFile;
        private readonly string TempMandiriFile;
        public BillingController(JbsDbContext context1)
        {
            _jbsDB = context1;
            TempFile = "./FileBilling/";
            BackupFile = "./BillingBackup/";
            Template = "./Template/";

            BCAFile = "CAF" + DateTime.Now.ToString("ddMM") + ".prn";
            MandiriFile = "Mandiri_" + DateTime.Now.ToString("ddMMyyyy") + ".xls";
            MegaOnUsFile = "CAF"+DateTime.Now.ToString("yyyyMMdd")+"_MegaOnUs.bpmt";
            MegaOfUsFile = "CAF" + DateTime.Now.ToString("yyyyMMdd") + "_MegaOffUs.bpmt";
            BNIFile = "BNI_" + DateTime.Now.ToString("ddMMyyyy")  + ".xlsx";
            TempBniFile = "./Template/BNI.xlsx";
            TempMandiriFile = "./Template/MANDIRI.xls";

            //string pattern = "CAF*.prn";
            //string[] matches = Directory.GetFiles(TempFile, pattern);
            //foreach (FileInfo fInfo in matches.ToString())
            //{
            //    if (!fInfo.Name.StartsWith("AAA") ||
            //        !fInfo.Name.EndsWith("BBB"))
            //    {
            //        fInfo.Delete();
            //    }
            //}

            // Move(backup) existing file BCA
            var files = Directory.GetFiles(TempFile);
            foreach (string file in files)
            {
                FileInfo FileName = new FileInfo(file);
                if ((FileName.ToString() == TempFile + BCAFile) ||      // File BCA
                    (FileName.ToString() == TempFile + MandiriFile) ||  // File Mandiri
                    (FileName.ToString() == TempFile + MegaOnUsFile) || // File MegaOnUs
                    (FileName.ToString() == TempFile + MegaOfUsFile) || // File MegaOffUs
                    (FileName.ToString() == TempFile + BNIFile)         // File BNI
                    )
                { continue; }

                FileInfo filex = new FileInfo(BackupFile + FileName.Name);
                if (filex.Exists) System.IO.File.Delete(filex.ToString());
                FileName.MoveTo(BackupFile + FileName.Name);
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            // cek file BCA CC Per Tgl Skrg tuk ditampilkan di web interface
            FileInfo FileName = new FileInfo(TempFile + this.BCAFile);
            if (FileName.Exists) ViewBag.BCACC = BCAFile;

            FileName = new FileInfo(TempFile + this.MandiriFile);
            if (FileName.Exists) ViewBag.MandiriCC = MandiriFile;

            FileName = new FileInfo(TempFile + this.MegaOnUsFile);
            if (FileName.Exists) ViewBag.MegaOnUs = MegaOnUsFile;

            FileName = new FileInfo(TempFile + this.MegaOfUsFile);
            if (FileName.Exists) ViewBag.MegaOfUs = MegaOfUsFile;

            FileName = new FileInfo(TempFile + this.BNIFile);
            if (FileName.Exists) ViewBag.BNICC = BNIFile;

            //string[] files = Directory.GetFiles(TempFile);
            //foreach (string file in files)
            //{
            //    string[] files += file;
            //}

            //for (int i = 0; i < files.Length; i++)
            //{
            //    files[i] = Path.GetFileName(files[i]);
            //}

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
                    MandiriCCFile();
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
                    BniCCFile(0);
                }
                else if (dw.BcaCC && dw.MegaCC && !(dw.MandiriCC || dw.BniCC))
                {   // jika dipilih BCA dan Mega
                    // BCA data sendiri, dan Selebihnya BNI
                    BcaCCFile(1); // BCA sendiri
                    MegaOnUsCCFile();
                    MegaOffUsCCFile(0);
                }
            }
            return RedirectToAction("Index");
        }

        //public async void Download(string fileName)
        //{
        //    ActionContext context = new ActionContext();
        //    var filepath = $"{fileName}";
        //    byte[] fileBytes = System.IO.File.ReadAllBytes(filepath);
        //    //await Response.Body.WriteAsync(fileBytes, 0, fileBytes.Length);
        //    using (var fileStream = new FileStream(filepath, FileMode.Open))
        //    {
        //        await fileStream.CopyToAsync(context.HttpContext.Response.Body);
        //    }
        //    //return File(fileBytes, "application/x-msdownload", fileName);
        //}

        //public async Task Download3(string fileName)
        //{
        //    using (HttpClient client = new HttpClient())
        //    {

        //    }
        //}
        
        public FileStreamResult DownloadFile(string fileName)
        {
            Response.Headers.Add("content-disposition", "attachment; filename=" + fileName);
            return File(new FileStream(TempFile + fileName, FileMode.Open),"application/octet-stream"); 
        }

        protected void BcaCCFile(int id)
        {
            /* id
             * 0 = All data
             * 1 = bca only
             */
            FileInfo FileName = new FileInfo(TempFile + this.BCAFile);
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
                    cmd.CommandText = "GenerateBillingBCA_sp ";
                    cmd.Parameters.Add(new MySqlParameter("@BankCode", MySqlDbType.Int16) { Value = 0 });
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

        protected void MandiriCCFile()
        {
            FileInfo FileName = new FileInfo(TempFile + this.BNIFile);
            if (!FileName.Exists)
            {

                FileName = new FileInfo(TempMandiriFile);
                FileName.CopyTo(TempFile + this.MandiriFile);
                FileName = new FileInfo(TempFile + this.MandiriFile);

                using (ExcelPackage package = new ExcelPackage(FileName))
                {
                    ExcelWorkbook workBook = package.Workbook;
                    ExcelWorksheet ws = workBook.Worksheets.SingleOrDefault(w => w.Name == "sheet1");
                    var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "BillingMandiri_sp ";
                    cmd.Connection.Open();
                    try
                    {
                        using (var result = cmd.ExecuteReader())
                        {
                            var i = 16;
                            while (result.Read())
                            {
                                ws.Cells[i, 3].Value = result["a"];
                                ws.Cells[i, 5].Value = result["b"];
                                ws.Cells[i, 7].Value = result["c"];
                                ws.Cells[i, 9].Value = result["d"];
                                ws.Cells[i,11].Value = result["e"];
                                ws.Cells[i, 13].Value = result["f"];
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
                        cmd.Dispose();
                        cmd.Connection.Close();
                    }
                    package.Save();
                }
            }
            //FileInfo FileName = new FileInfo(TempFile + this.MandiriFile);
            //if (!FileName.Exists)
            //{
            //    FileName = new FileInfo(TempMandiriFile);
            //    FileName.CopyTo(TempFile + this.MandiriFile);
            //    FileName = new FileInfo(TempFile + this.MandiriFile);

            //    try
            //    {
            //        var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        cmd.CommandText = "BillingMandiri_sp ";
            //        cmd.Connection.Open();

            //        HSSFWorkbook wb;
            //        using (var fs = new FileStream(FileName.ToString(), FileMode.Create, FileAccess.ReadWrite))
            //        {
            //            wb = new HSSFWorkbook(fs);
            //            //HSSFSheet sh = new HSSFSheet(wb);

            //            ISheet sh = wb.GetSheetAt(0);

            //            //sh = (HSSFSheet)wb.CreateSheet("sheet1");
            //            using (var result = cmd.ExecuteReader())
            //            {
            //                var i = 16;
            //                while (result.Read())
            //                {
            //                    var row = (HSSFRow)sh.CreateRow(5);
            //                    //var row = sh.CreateRow(i);
            //                    row.CreateCell(3).SetCellValue(result["b"].ToString());

            //                    //row.CreateCell(1).SetCellValue("Eve Paradise"); // Column B
            //                    //row.CreateCell(2).SetCellValue(4);
            //                    //row.Cells.Add()

            //                    //row = sheet.CreateRow(rowIndex);
            //                    //row.CreateCell(0).SetCellValue(account.UserName);

            //                    //cell = row.CreateCell(0);
            //                    //cell.SetCellValue("Total:");
            //                    //cell.CellStyle = detailSubtotalCellStyle;

            //                    //ws.Cells[i, 5].Value = result["b"];
            //                    //ws.Cells[i, 7].Value = result["c"];
            //                    //ws.Cells[i, 9].Value = result["d"];
            //                    //ws.Cells[i, 11].Value = result["e"];
            //                    //ws.Cells[i, 13].Value = result["f"];
            //                    i++;
            //                }
            //            }

            //            wb.Write(fs);
            //        }


            //    }
            //    catch (Exception ex)
            //    {
            //        System.IO.File.Delete(FileName.ToString());
            //        throw ex;
            //    }

            //    //for (int row = 0; row <= sheet.LastRowNum; row++)
            //    //{
            //    //    string cellValue = sheet.GetRow(row).GetCell(0).ToString().Trim(); 
            //    //    string cellValue2 = sheet.GetRow(row).GetCell(0).StringCellValue.Trim();
            //    //}
            //    //hssfwb = new HSSFWorkbook(file);

            //    ///*HSSFWorkbook*/ hssfwb = new HSSFWorkbook(file);
            //    //}

            //    //sheet = hssfwb.GetSheet("");

            //    //using (ExcelPackage package = new ExcelPackage(FileName))
            //    //{
            //    //    ExcelWorkbook workBook = package.Workbook;
            //    //    ExcelWorksheet ws = workBook.Worksheets.SingleOrDefault(w => w.Name == "sheet1");

            //    //    try
            //    //    {

            //    //    }
            //    //    catch (Exception ex)
            //    //    {
            //    //        System.IO.File.Delete(FileName.ToString());
            //    //        throw ex;
            //    //    }
            //    //    finally
            //    //    {
            //    //        cmd.Connection.Close();
            //    //    }
            //    //    package.Save();
            //    //}
            //}
        }

        protected void MegaOnUsCCFile()
        {
            FileInfo FileName = new FileInfo(TempFile + this.MegaOnUsFile);
            if (!FileName.Exists) //jika file belum ada akan di generate tp jika sudah ada maka akan pake file exist
            {
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "BillingMegaOnUs_sp ";
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

        protected void MegaOffUsCCFile(int id)
        {
            FileInfo FileName = new FileInfo(TempFile + this.MegaOfUsFile);
            if (!FileName.Exists) //jika file belum ada akan di generate tp jika sudah ada maka akan pake file exist
            {
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "BillingMegaOffUs_sp ";
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

        protected void BniCCFile(int id)
        {
            FileInfo FileName = new FileInfo(TempFile + this.BNIFile);
            if (!FileName.Exists)
            {

                FileName = new FileInfo(TempBniFile);
                FileName.CopyTo(TempFile + this.BNIFile);
                FileName = new FileInfo(TempFile + this.BNIFile);

                using (ExcelPackage package = new ExcelPackage(FileName))
                {
                    ExcelWorkbook workBook = package.Workbook;
                    ExcelWorksheet ws = workBook.Worksheets.SingleOrDefault(w => w.Name == "sheet1");
                    var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "BillingBNI_sp ";
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
                                ws.Cells[i,10].Value = result["j"];
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
                        cmd.Connection.Close();
                    }
                    package.Save();
                }
            }
        }
        
    }
}