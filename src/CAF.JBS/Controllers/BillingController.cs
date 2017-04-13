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
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace CAF.JBS.Controllers
{
    public class BillingController : Controller
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
        public BillingController(JbsDbContext context1)
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
            // cek file BCA CC Per Tgl Skrg tuk ditampilkan di web interface
            FileInfo FileName = new FileInfo(DirBilling + this.BCAccFile);
            if (FileName.Exists) ViewBag.BCACC = BCAccFile;

            FileName = new FileInfo(DirBilling + this.MandiriccFile);
            if (FileName.Exists) ViewBag.MandiriCC = MandiriccFile;

            FileName = new FileInfo(DirBilling + this.MegaOnUsccFile);
            if (FileName.Exists) ViewBag.MegaOnUs = MegaOnUsccFile;

            FileName = new FileInfo(DirBilling + this.MegaOfUsccFile);
            if (FileName.Exists) ViewBag.MegaOfUs = MegaOfUsccFile;

            FileName = new FileInfo(DirBilling + this.BNIccFile);
            if (FileName.Exists) ViewBag.BNICC = BNIccFile;

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
                    GenBcaCCFile(0); // BCA semua
                }
                else if (dw.BcaCC && dw.MandiriCC && !(dw.MegaCC || dw.BniCC))
                {   // jika dipilih BCA dan Mandiri
                    // semua data kecuali mandiri dikeluarkan format BCA, dan Mandiri data sendiri
                    GenBcaCCFile(2); // BCA semua kecuali mandiri
                    GenMandiriCCFile();
                }
                else if (dw.BcaCC && dw.MandiriCC && dw.MegaCC && !dw.BniCC)
                {   // jika dipilih BCA,Mandiri dan Mega
                    // BCA data sendiri, Mandiri data sendiri, dan Selebihnya Mega Off Us
                    GenBcaCCFile(1); // BCA sendiri
                }
                else if (dw.BcaCC && dw.MandiriCC && dw.BniCC && !dw.MegaCC)
                {   // jika dipilih BCA,Mandiri dan BNI
                    // BCA data sendiri, Mandiri data sendiri, dan Selebihnya BNI
                    GenBcaCCFile(1); // BCA sendiri
                }
                else if (dw.BcaCC && dw.BniCC&& !(dw.MandiriCC || dw.MegaCC))
                {   // jika dipilih BCA dan BNI
                    // BCA data sendiri, dan Selebihnya BNI
                    GenBcaCCFile(1); // BCA sendiri
                    GenBniCCFile(0);
                }
                else if (dw.BcaCC && dw.MegaCC && !(dw.MandiriCC || dw.BniCC))
                {   // jika dipilih BCA dan Mega
                    // BCA data sendiri, dan Selebihnya BNI
                    GenBcaCCFile(1); // BCA sendiri
                    GenMegaOnUsCCFile();
                    GenMegaOffUsCCFile(0);
                }
            }
            GenBcaAcFile();
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

        protected void GenMandiriCCFile()
        {
            FileInfo FileName = new FileInfo(DirBilling + this.MandiriccFile);
            if (!FileName.Exists)
            {

                FileName = new FileInfo(TempMandiriFile);
                FileName.CopyTo(DirBilling + this.MandiriccFile);
                FileName = new FileInfo(DirBilling + this.MandiriccFile);

                using (ExcelPackage package = new ExcelPackage(FileName))
                {
                    ExcelWorkbook workBook = package.Workbook;
                    ExcelWorksheet ws = workBook.Worksheets.SingleOrDefault(w => w.Name == "sheet1");
                    var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "BillingMandiriCC_sp ";
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

                using (ExcelPackage package = new ExcelPackage(FileName))
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

        protected void GenBcaAcFile()
        {
            FileInfo FileName = new FileInfo(DirBilling + this.TempBCAacFile);
            if (!FileName.Exists)
            {

                FileName = new FileInfo(TempBCAacFile);
                Excel.Application xlApp = new Excel.Application();
                Excel.Range range;
                Excel.Workbook wb;
                Excel.Worksheet ws;
                var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
                wb = xlApp.Workbooks.Open(FileName.FullName.ToString(), 0, true, 5, "", "", true, Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, false,false);
                ws = (Excel.Worksheet)wb.Worksheets.get_Item(1);
                

                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "BillingBcaAC_sp";
                    
                    xlApp.Visible = false;
                    
                    range = ws.UsedRange;
                    cmd.Connection.Open();

                    using (var result = cmd.ExecuteReader())
                    {
                        var baris = 2;
                        while (result.Read())
                        {
                            ws.Cells[baris, "A"] = "ID Number";
                            //range = (Range)ws.Cells[baris, 1];
                            //range.Value2 = "test";
                            baris++;
                        }

                    }

                    FileName = new FileInfo(DirBilling + this.BCAacFile);
                    wb.SaveAs(FileName.FullName.ToString());
                    //wb.SaveAs(FileName.FullName.ToString(), Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,
                        //false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
                        //Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    cmd.Connection.Close();

                    wb.Close();
                    xlApp.Quit();

                    xlApp = null;
                    ws = null;
                    wb = null;
                    range = null;
                }
            }
        }

        //public ActionResult Download2()
        //{
        //    try
        //    {
        //        var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.CommandText = "BillingBcaAC_sp";
        //        cmd.Connection.Open();
        //        //var reader = cmd.ExecuteReader();

        //        Application xlApp = new Application();
        //        xlApp.Visible = false;
        //        Workbook wb = xlApp.Workbooks.Open(@"c:\temp\testfile.xls", 0, false, 5, "", "", true, XlPlatform.xlWindows, "\t", false, false, 0, true, false, false);
        //        //Workbook wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
        //        Worksheet ws = (Worksheet)wb.Worksheets[1];
        //        Range range;

        //        int i = 2;
        //        using (var result = cmd.ExecuteReader())
        //        {
                    
        //            //while (result.Read())
        //            //{
        //            //    ws.Cells[i, 1] = result["a"].ToString();
        //            //    i++;
        //            //}
        //            for (i=2; i <= 5; i++)
        //            {
        //                //range = ws.get_Range("A" + i.ToString(), "D" + i.ToString());

        //                //System.Array myvalues = (System.Array)range.Cells.Value2;
        //                //string[] strArray = ConvertToStringArray(myvalues);
        //                //range = ws.get_Range("B6", "H20");
        //                range = ws.UsedRange;
        //                ws.Cells.set_Item(1, i, "asd");
        //                //ws.Cells[i, 1] = "asdfasdf";
        //                //range.Value2 = "aaaaaaa";

        //            }
        //        }
                
        //        wb.SaveAs(@"c:\temp\testfile2.xls", Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,
        //            false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
        //            Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

        //        wb.Close();
        //        xlApp.Quit();

        //        xlApp = null;
        //        ws = null;
        //        wb = null;
        //        range = null;
        //        //Marshal.ReleaseComObject(ws);
        //        //Marshal.ReleaseComObject(wb);
        //        //Marshal.ReleaseComObject(xlApp);
        //        //Filex.MoveTo("C:\\tmp\\tmp\\" + Filex.Name);
        //        //cmd.Connection.Close();
        //        //ws = null;
        //        //wb = null;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //    }
        //        return RedirectToAction("Index");
        //}

        //public ActionResult Download3()
        //{
        //    string FullfileName = @"c:\temp\testfile2.xls";
        //    FileInfo Filex = new FileInfo(FullfileName);
        //    try
        //    {
        //        //var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
        //        //cmd.CommandType = CommandType.StoredProcedure;
        //        //cmd.CommandText = "BillingBcaAC_sp";
        //        //cmd.Connection.Open();

        //        //var reader = cmd.ExecuteReader();

        //        Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
        //        xlApp.Visible = false;
        //        Range range;
        //        Workbook wb;
        //        Worksheet ws;
        //        object misValue = System.Reflection.Missing.Value;
                

        //        wb = xlApp.Workbooks.Add(misValue);
        //        ws = (Worksheet)wb.Worksheets.get_Item(1);

        //        //int i = 2;
        //        //while (reader.Read())
        //        //{
        //        //    ws.Cells[i, 1] = reader["a"].ToString();
        //        //    ws.Cells[i, 2] = reader["b"].ToString();
        //        //    ws.Cells[i, 3] = reader["c"].ToString();
        //        //    ws.Cells[i, 4] = reader["d"].ToString();
        //        //    ws.Cells[i, 5] = reader["e"].ToString();
        //        //    ws.Cells[i, 6] = reader["f"].ToString();
        //        //    ws.Cells[i, 7] = reader["g"].ToString();
        //        //    ws.Cells[i, 8] = reader["i"].ToString();
        //        //    ws.Cells[i, 9] = reader["j"].ToString();
        //        //    i++;
        //        //}
        //        ws.Range["A1","D1"].Value= "asdasd";
                

        //        //ws.Cells["A1"] = "ID";
        //        //ws.Cells[1, 2] = "Name";
        //        //ws.Cells[2, 1] = "1";
        //        //ws.Cells[2, 2] = "One";
        //        //ws.Cells[3, 1] = "2";
        //        //ws.Cells[3, 2] = "Two";

        //        wb.SaveAs(FullfileName.ToString(), XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
        //        wb.Close(true, misValue, misValue);
        //        xlApp.Quit();

        //        Marshal.ReleaseComObject(ws);
        //        Marshal.ReleaseComObject(wb);
        //        Marshal.ReleaseComObject(xlApp);

        //        //wb.Close();
        //        //xlApp.Quit();
        //        //Filex.MoveTo("C:\\tmp\\tmp\\" + Filex.Name);
        //        //cmd.Connection.Close();

        //        //xlApp = null;
        //        //ws = null;
        //        //wb = null;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //        return RedirectToAction("Index");
        //}

        //private string[] ConvertToStringArray(System.Array values)
        //{
        //    // create a new string array
        //    string[] theArray = new string[values.Length];

        //    // loop through the 2-D System.Array and populate the 1-D String Array
        //    for (int i = 1; i <= values.Length; i++)
        //    {
        //        if (values.GetValue(1, i) == null) theArray[i - 1] = "";
        //        else theArray[i - 1] = (string)values.GetValue(1, i).ToString();
        //    }
        //    return theArray;
        //}
    }
}