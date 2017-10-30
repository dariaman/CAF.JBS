using CAF.JBS.Data;
using CAF.JBS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Controllers
{

    public class RerateController : Controller
    {
        private readonly JbsDbContext _context;
        private FileSettings filesettings;
        private readonly string tempFile;
        public RerateController(JbsDbContext context)
        {
            _context = context;
            filesettings = new FileSettings();
            tempFile = filesettings.Result;
        }

        public IActionResult Index()
        {
            IEnumerable<RerateVM> Rerate;
            Rerate = (from cd in _context.PolicyPrerenewalModel
                      join bk in _context.PolicyBillingModel on cd.policy_Id equals bk.policy_Id
                      orderby cd.policy_Id
                            select new RerateVM()
                            {
                                policy_Id=cd.policy_Id,
                                policy_No=bk.policy_no,
                                history_date=cd.history_date,
                                premium_amount=cd.premium_amount
                            });
            return View(Rerate);
        }

        public FileStreamResult Download()
        {
            // period = yyyyMM
            // kosongkan folder tmp
            string[] files = Directory.GetFiles(tempFile, "Rerate*.xlsx", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                FileInfo FileName = new FileInfo(file);
                if (FileName.Exists) System.IO.File.Delete(FileName.ToString());
            }
            var fileName = "Rerate" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
            var fullePath = tempFile + fileName;

            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.Parameters.Clear();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"SELECT pb.`policy_no`, pr.`premium_amount`,pr.`history_date`
                                FROM `policy_prerenewal` pr
                                INNER JOIN `policy_billing` pb ON pb.`policy_Id`= pr.`policy_Id`; ";

            using (ExcelPackage package = new ExcelPackage(new FileInfo(fullePath)))
            {
                var sheet = package.Workbook.Worksheets.Add("sheet1");

                try
                {
                    cmd.Connection.Open();
                    using (var result = cmd.ExecuteReader())
                    {
                        sheet.Cells[1, 1].Value ="Policy No";
                        sheet.Cells[1, 2].Value = "Premi Amount";
                        sheet.Cells[1, 3].Value = "History Date";
                        

                        var i = 2;
                        while (result.Read())
                        {
                            sheet.Cells[i, 1].Value = result[0];
                            sheet.Cells[i, 2].Value = result[1];
                            sheet.Cells[i, 3].Value = Convert.ToDateTime(result[2]).ToString("dd/MM/yyyy");
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

    }
}
