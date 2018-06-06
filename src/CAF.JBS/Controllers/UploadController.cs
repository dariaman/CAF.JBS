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
using System.Data;
using System.IO;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using Vereyon.Web;

namespace CAF.JBS.Controllers
{
    public class UploadController : Controller
    {
        private readonly JbsDbContext _context;
        private readonly string FileResult;   //folder Backup File Result dari Bank
        private FileSettings filesettings;
        private readonly string ConsoleExecResult;
        private readonly string DirCommand;
        private IFlashMessage flashMessage;

        public UploadController(JbsDbContext context, IFlashMessage flash)
        {
            _context = context;
            filesettings = new FileSettings();
            FileResult = filesettings.UploadSchedule;
            ConsoleExecResult = filesettings.FileExecresult;
            DirCommand = filesettings.DirCommand;
            flashMessage = flash;
        }

        public IActionResult Index()
        {
            List<UploadResultIndexVM> StagingUploadx = new List<UploadResultIndexVM>();

            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"SELECT fp.`id`,fp.`deskripsi`,fp.`FileName`,fp.`tglProses`,(bs.`BillingCountDWD`+bs.`QuoteCountDWD`+bs.`OthersCountDWD`) AS BillingCountDWD
                                FROM `FileNextProcess` fp
                                LEFT JOIN `billing_download_summary` bs ON bs.`id`= fp.`id_billing_download`; ";
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    StagingUploadx.Add(new UploadResultIndexVM()
                    {
                        id = Convert.ToInt32(rd["id"]),
                        deskripsi = rd["deskripsi"].ToString(),
                        FileName = rd["FileName"].ToString(),
                        tglProses = (rd["tglProses"] == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(rd["tglProses"]),
                        billCountDwd = (rd["BillingCountDWD"] == DBNull.Value) ? 0 : Convert.ToInt32(rd["BillingCountDWD"])
                    });

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                cmd.Connection.Close();
            }

            return View(StagingUploadx);
        }

        [HttpGet]
        public ActionResult UploadResult(int id)
        {
            var up = _context.FileNextProcessModel.Where(x => x.id == id).FirstOrDefault();
            var dataUpload = new UploadResultSubmitVM();
            dataUpload.id = up.id;
            dataUpload.deskripsi = up.deskripsi;
            dataUpload.tglProses = DateTime.Now.Date;

            return View("UploadResult", dataUpload);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadResult(int id, [Bind("tglProses,FileName")] UploadResultSubmitVM UploadFile)
        {
            var tgl = DateTime.Now.Date;
            if (UploadFile.tglProses < tgl)
                ModelState.AddModelError("tglProses", " Tgl Proses harus mulai dari tanggal sekarang Atau setelahnya .... ");

            var FileNextProses = _context.FileNextProcessModel.SingleOrDefault(m => m.id == id);

            if (FileNextProses.FileName != null) ModelState.AddModelError("FileName", " File sudah pernah di upload, silahkan di remove dulu !");

            var fileUpload = new FileInfo(FileResult + FileNextProses.FileName);
            if (fileUpload.Exists) ModelState.AddModelError("FileName", " File dengan nama file tersebut sudah ada, silahkan ubah nama file Upload !");

            if (ModelState.IsValid)
            {
                FileNextProses.tglProses = UploadFile.tglProses;
                FileNextProses.FileName = UploadFile.FileName.FileName.ToString() + Guid.NewGuid().ToString().Substring(0, 8);
                _context.Update(FileNextProses);
                _context.SaveChanges();

                using (var fileStream = new FileStream(FileResult + FileNextProses.FileName, FileMode.Create))
                {
                    UploadFile.FileName.CopyTo(fileStream);
                }

                // Proses insert File by Console
                foreach (Process proc in Process.GetProcessesByName("ExecFileBilling")) { proc.Kill(); }
                try
                {
                    var process = new Process();
                    process.StartInfo.FileName = "dotnet";
                    process.StartInfo.WorkingDirectory = DirCommand;
                    process.StartInfo.Arguments = ConsoleExecResult + " upload " + id.ToString();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();
                    process.WaitForExit();

                    flashMessage.Confirmation("Sukses");
                }
                catch (Exception ex)
                {
                    var fileupload = new FileInfo(FileResult + FileNextProses.FileName);
                    if (fileupload.Exists) fileupload.Delete();

                    FileNextProses.tglProses = null;
                    FileNextProses.FileName = null;
                    _context.Update(FileNextProses);
                    _context.SaveChanges();

                    flashMessage.Danger(ex.Message);
                }

                try
                {
                    foreach (Process proc in Process.GetProcessesByName("ExecFileBilling")) { proc.Kill(); }
                }
                catch (Exception ex) { throw ex; }

                return RedirectToAction("index");
            }
            return View("UploadResult", UploadFile);
        }

        [HttpGet]
        public ActionResult RemoveFile(int id)
        {
            var FileNextProses = _context.FileNextProcessModel.SingleOrDefault(m => m.id == id);

            FileInfo filex = new FileInfo(FileResult + FileNextProses.FileName);
            if (filex.Exists) filex.Delete();

            var cmd = _context.Database.GetDbConnection().CreateCommand();
            try
            {
                cmd.CommandType = CommandType.Text;
                cmd.Connection.Open();
                if (id == 1 || id == 2)
                {
                    cmd.CommandText = @"DELETE up
                                FROM `UploadBcaCC` up
                                INNER JOIN `FileNextProcess` fp ON up.`FileName`=fp.`FileName`
                                WHERE fp.id=@idx ;";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new MySqlParameter("@idx", MySqlDbType.Int32) { Value = id });
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = @"DELETE FROM " + FileNextProses.stageTable + " ;";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { flashMessage.Danger(ex.Message); }
            finally { cmd.Connection.Close(); }

            try
            {
                FileNextProses.tglProses = null;
                FileNextProses.FileName = null;
                _context.Update(FileNextProses);
                _context.SaveChanges();
                flashMessage.Confirmation("Sukses");
            }
            catch (Exception ex) { flashMessage.Danger(ex.Message); }

            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult Execute(int id)
        {
            if (User.Identity.Name != "dariaman.siagian@jagadiri.co.id") return RedirectToAction("index");

            foreach (Process proc in Process.GetProcessesByName("ExecFileBilling")) { proc.Kill(); }
            try
            {
                var process = new Process();
                process.StartInfo.FileName = "dotnet";
                process.StartInfo.WorkingDirectory = DirCommand;
                process.StartInfo.Arguments = ConsoleExecResult + " exec " + id.ToString();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                process.WaitForExit();
                flashMessage.Confirmation("Sukses");
            }
            catch (Exception ex) { flashMessage.Danger(ex.Message); }
            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult ExecuteAll()
        {
            if (User.Identity.Name != "dariaman.siagian@jagadiri.co.id") return RedirectToAction("index");

            foreach (Process proc in Process.GetProcessesByName("ExecFileBilling")) { proc.Kill(); }
            try
            {
                var process = new Process();
                process.StartInfo.FileName = "dotnet";
                process.StartInfo.WorkingDirectory = DirCommand;
                process.StartInfo.Arguments = ConsoleExecResult + " exec ";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                process.WaitForExit();
                flashMessage.Confirmation("Sukses");
            }
            catch (Exception ex) { flashMessage.Danger(ex.Message); }
            return RedirectToAction("index");
        }

        public FileStreamResult DownloadFile(string fileName)
        {
            Response.Headers.Add("content-disposition", "attachment; filename=" + fileName);
            return File(new FileStream(FileResult + fileName, FileMode.Open), "application/octet-stream");
        }
    }
}
