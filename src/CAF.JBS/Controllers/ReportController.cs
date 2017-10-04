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
using OfficeOpenXml;
using System.IO;
using System.Data;
using MySql.Data.MySqlClient;

namespace CAF.JBS.Controllers
{
    public class ReportController : Controller
    {
        private readonly JbsDbContext _jbsDB;
        private FileSettings filesettings;
        private readonly string tempFile;
        public ReportController(JbsDbContext context1)
        {
            _jbsDB = context1;
            filesettings = new FileSettings();
            tempFile = filesettings.Result;
        }
        public IActionResult Index()
        {
            var bulan = DateTime.Now.ToString("MM");
            var tahun = DateTime.Now.ToString("yyyy");
            var tahunPrev = DateTime.Now.AddYears(-1).Year.ToString();

            ReportViewModel rpt = new ReportViewModel();
            rpt.blnList = new List<SelectListItem> {
                new SelectListItem() {Text = "Januari", Value="01"},
                new SelectListItem() {Text = "Februari", Value="02"},
                new SelectListItem() {Text = "Maret", Value="03"},
                new SelectListItem() {Text = "April", Value="04"},
                new SelectListItem() {Text = "Mei", Value="05"},
                new SelectListItem() {Text = "Juni", Value="06"},
                new SelectListItem() {Text = "Juli", Value="07"},
                new SelectListItem() {Text = "Agustus", Value="08"},
                new SelectListItem() {Text = "September", Value="09"},
                new SelectListItem() {Text = "Oktober", Value="10"},
                new SelectListItem() {Text = "November", Value="11"},
                new SelectListItem() {Text = "Desember", Value="12"} };
            rpt.thnList = new List<SelectListItem> {
                new SelectListItem() {Text = tahunPrev, Value=tahunPrev},
                new SelectListItem() {Text = tahun, Value=tahun}
            };
            rpt.bln = bulan;
            rpt.thn = tahun;
            rpt.tgl = DateTime.Now;

            return View(rpt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public FileResult DailyReconcile(ReportViewModel rpt)
        {
            return BillingReconcile(rpt.tgl);
            //return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MonthlyBilling(ReportViewModel rpt)
        {
            return MonthlyBilling(rpt.thn + rpt.bln);
            //var fileName = "MonthlyBilling" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
            //return RedirectToAction("Index");
        }

        public FileStreamResult BillingReconcile(DateTime tgl)
        {
            // kosongkan folder tmp
            var files = Directory.GetFiles(tempFile);
            foreach (string file in files)
            {
                FileInfo FileName = new FileInfo(file);
                if (FileName.Exists) System.IO.File.Delete(FileName.ToString());
            }

            var fileName = "DailyReconcile" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
            var fullePath = tempFile + fileName;

            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "DailyReconcile_sp ";
            cmd.Parameters.Add(new MySqlParameter("@tgl", MySqlDbType.VarChar) { Value = tgl.ToString("yyyyMMdd") });

            using (ExcelPackage package = new ExcelPackage(new FileInfo(fullePath)))
            {
                var sheet = package.Workbook.Worksheets.Add("sheet1");

                try
                {
                    cmd.Connection.Open();
                    using (var result = cmd.ExecuteReader())
                    {
                        sheet.Cells[1, 1].Value = result.GetName(0);
                        sheet.Cells[1, 2].Value = result.GetName(1);
                        sheet.Cells[1, 3].Value = result.GetName(2);
                        sheet.Cells[1, 4].Value = result.GetName(3);
                        sheet.Cells[1, 5].Value = result.GetName(4);
                        sheet.Cells[1, 6].Value = result.GetName(5);
                        sheet.Cells[1, 7].Value = result.GetName(6);
                        sheet.Cells[1, 8].Value = result.GetName(7);
                        sheet.Cells[1, 9].Value = result.GetName(8);
                        sheet.Cells[1, 10].Value = result.GetName(9);
                        sheet.Cells[1, 11].Value = result.GetName(10);
                        sheet.Cells[1, 12].Value = result.GetName(11);
                        sheet.Cells[1, 13].Value = result.GetName(12);
                        sheet.Cells[1, 14].Value = result.GetName(13);
                        sheet.Cells[1, 15].Value = result.GetName(14);
                        sheet.Cells[1, 16].Value = result.GetName(15);
                        sheet.Cells[1, 17].Value = result.GetName(16);
                        sheet.Cells[1, 18].Value = result.GetName(17);
                        sheet.Cells[1, 19].Value = result.GetName(18);
                        sheet.Cells[1, 20].Value = result.GetName(19);
                        sheet.Cells[1, 21].Value = result.GetName(20);
                        sheet.Cells[1, 22].Value = result.GetName(21);

                        var i = 2;
                        while (result.Read())
                        {
                            sheet.Cells[i, 1].Value = result[0];
                            sheet.Cells[i, 2].Value = result[1];
                            sheet.Cells[i, 3].Value = result[2];
                            sheet.Cells[i, 4].Value = result[3];
                            sheet.Cells[i, 5].Value = result[4];
                            sheet.Cells[i, 6].Value = result[5];
                            sheet.Cells[i, 7].Value = result[6];
                            sheet.Cells[i, 8].Value = result[7];
                            sheet.Cells[i, 9].Value = result[8];
                            sheet.Cells[i, 10].Value = result[9];
                            sheet.Cells[i, 11].Value = result[10];
                            sheet.Cells[i, 12].Value = result[11];
                            sheet.Cells[i, 13].Value = result[12];
                            sheet.Cells[i, 14].Value = result[13];
                            sheet.Cells[i, 15].Value = result[14];
                            sheet.Cells[i, 16].Value = result[15];
                            sheet.Cells[i, 17].Value = result[16];
                            sheet.Cells[i, 18].Value = result[17];
                            sheet.Cells[i, 19].Value = result[18];
                            sheet.Cells[i, 20].Value = result[19];
                            sheet.Cells[i, 21].Value = result[20];
                            sheet.Cells[i, 22].Value = result[21];
                            i++;
                        }
                        sheet.Column(1).AutoFit();
                        sheet.Column(2).AutoFit();
                        sheet.Column(3).AutoFit();
                        sheet.Column(4).AutoFit();
                        sheet.Column(5).AutoFit();
                        sheet.Column(6).AutoFit();
                        sheet.Column(7).AutoFit();
                        sheet.Column(8).AutoFit();
                        sheet.Column(9).AutoFit();
                        sheet.Column(10).AutoFit();
                        sheet.Column(11).AutoFit();
                        sheet.Column(12).AutoFit();
                        sheet.Column(13).AutoFit();
                        sheet.Column(14).AutoFit();
                        sheet.Column(15).AutoFit();
                        sheet.Column(16).AutoFit();
                        sheet.Column(17).AutoFit();
                        sheet.Column(18).AutoFit();
                        sheet.Column(19).AutoFit();
                        sheet.Column(20).AutoFit();
                        sheet.Column(21).AutoFit();
                        sheet.Column(22).AutoFit();
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

        public FileStreamResult MonthlyBilling(string period)
        {
            // period = yyyyMM
            // kosongkan folder tmp
            var files = Directory.GetFiles(tempFile);
            foreach (string file in files)
            {
                FileInfo FileName = new FileInfo(file);
                if (FileName.Exists) System.IO.File.Delete(FileName.ToString());
            }

            var fileName = "MonthlyBilling" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
            var fullePath = tempFile + fileName;

            var cmd = _jbsDB.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "MonthlyBilling_sp ";
            cmd.Parameters.Add(new MySqlParameter("@period", MySqlDbType.VarChar) { Value = period });

            using (ExcelPackage package = new ExcelPackage(new FileInfo(fullePath)))
            {
                //var worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == "Attempts");
                //var ws = package.Workbook.Worksheets.Add("Sample1");
                var sheet = package.Workbook.Worksheets.Add("sheet1");
                //worksheet = package.Workbook.Worksheets.Add("Assessment Attempts");

                try
                {
                    cmd.Connection.Open();
                    using (var result = cmd.ExecuteReader())
                    {
                        sheet.Cells[1, 1].Value = result.GetName(0);
                        sheet.Cells[1, 2].Value = result.GetName(1);
                        sheet.Cells[1, 3].Value = result.GetName(2);
                        sheet.Cells[1, 4].Value = result.GetName(3);
                        sheet.Cells[1, 5].Value = result.GetName(4);
                        sheet.Cells[1, 6].Value = result.GetName(5);
                        sheet.Cells[1, 7].Value = result.GetName(6);
                        sheet.Cells[1, 8].Value = result.GetName(7);
                        sheet.Cells[1, 9].Value = result.GetName(8);
                        sheet.Cells[1, 10].Value = result.GetName(9);
                        sheet.Cells[1, 11].Value = result.GetName(10);
                        sheet.Cells[1, 12].Value = result.GetName(11);
                        sheet.Cells[1, 13].Value = result.GetName(12);
                        sheet.Cells[1, 14].Value = result.GetName(13);
                        sheet.Cells[1, 15].Value = result.GetName(14);
                        sheet.Cells[1, 16].Value = result.GetName(15);
                        sheet.Cells[1, 17].Value = result.GetName(16);
                        sheet.Cells[1, 18].Value = result.GetName(17);
                        sheet.Cells[1, 19].Value = result.GetName(18);
                        sheet.Cells[1, 20].Value = result.GetName(19);
                        sheet.Cells[1, 21].Value = result.GetName(20);
                        sheet.Cells[1, 22].Value = result.GetName(21);
                        sheet.Cells[1, 23].Value = result.GetName(22);
                        sheet.Cells[1, 24].Value = result.GetName(23);
                        sheet.Cells[1, 25].Value = result.GetName(24);
                        sheet.Cells[1, 26].Value = result.GetName(25);

                        var i = 2;
                        while (result.Read())
                        {
                            sheet.Cells[i, 1].Value = result[0];
                            sheet.Cells[i, 2].Value = result[1];
                            sheet.Cells[i, 3].Value = result[2];
                            sheet.Cells[i, 4].Value = result[3];
                            sheet.Cells[i, 5].Value = result[4];
                            sheet.Cells[i, 6].Value = result[5];
                            sheet.Cells[i, 7].Value = result[6];
                            sheet.Cells[i, 8].Value = result[7];
                            sheet.Cells[i, 9].Value = result[8];
                            sheet.Cells[i, 10].Value = result[9];
                            sheet.Cells[i, 11].Value = result[10];
                            sheet.Cells[i, 12].Value = result[11];
                            sheet.Cells[i, 13].Value = result[12];
                            sheet.Cells[i, 14].Value = result[13];
                            sheet.Cells[i, 15].Value = result[14];
                            sheet.Cells[i, 16].Value = result[15];
                            sheet.Cells[i, 17].Value = result[16];
                            sheet.Cells[i, 18].Value = result[17];
                            sheet.Cells[i, 19].Value = result[18];
                            sheet.Cells[i, 20].Value = result[19];
                            sheet.Cells[i, 21].Value = result[20];
                            sheet.Cells[i, 22].Value = result[21];
                            sheet.Cells[i, 23].Value = result[22];
                            sheet.Cells[i, 24].Value = result[23];
                            sheet.Cells[i, 25].Value = result[24];
                            sheet.Cells[i, 26].Value = result[25];
                            i++;
                        }
                        sheet.Column(1).AutoFit();
                        sheet.Column(2).AutoFit();
                        sheet.Column(3).AutoFit();
                        sheet.Column(4).AutoFit();
                        sheet.Column(5).AutoFit();
                        sheet.Column(6).AutoFit();
                        sheet.Column(7).AutoFit();
                        sheet.Column(8).AutoFit();
                        sheet.Column(9).AutoFit();
                        sheet.Column(10).AutoFit();
                        sheet.Column(11).AutoFit();
                        sheet.Column(12).AutoFit();
                        sheet.Column(13).AutoFit();
                        sheet.Column(14).AutoFit();
                        sheet.Column(15).AutoFit();
                        sheet.Column(16).AutoFit();
                        sheet.Column(17).AutoFit();
                        sheet.Column(18).AutoFit();
                        sheet.Column(19).AutoFit();
                        sheet.Column(20).AutoFit();
                        sheet.Column(21).AutoFit();
                        sheet.Column(22).AutoFit();
                        sheet.Column(23).AutoFit();
                        sheet.Column(24).AutoFit();
                        sheet.Column(25).AutoFit();
                        sheet.Column(26).AutoFit();
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

    }
}
