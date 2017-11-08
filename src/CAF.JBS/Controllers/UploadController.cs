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

namespace CAF.JBS.Controllers
{
    public class UploadController : Controller
    {
        private readonly JbsDbContext _context;
        private readonly string FileResult;   //folder Backup File Result dari Bank
        private FileSettings filesettings;

        public UploadController(JbsDbContext context)
        {
            _context = context;
            filesettings = new FileSettings();
            FileResult = filesettings.UploadSchedule;
        }

        public IActionResult Index()
        {
            List<UploadResultIndexVM> StagingUploadx = new List<UploadResultIndexVM>();

            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"SELECT fp.`id`,fp.`deskripsi`,fp.`FileName`,fp.`tglProses`,bs.`BillingCountDWD`
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
                        deskripsi= rd["deskripsi"].ToString(),
                        FileName = rd["FileName"].ToString(),
                        tglProses = (rd["tglProses"] == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(rd["tglProses"]),
                        billCountDwd = Convert.ToInt32(rd["BillingCountDWD"])
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

            if (ModelState.IsValid)
            {
                var FileNextProses = _context.FileNextProcessModel.SingleOrDefault(m => m.id == id);
                FileNextProses.tglProses = UploadFile.tglProses;
                FileNextProses.FileName = UploadFile.FileName.FileName.ToString();
                _context.Update(FileNextProses);
                _context.SaveChanges();

                using (var fileStream = new FileStream(FileResult + FileNextProses.FileName, FileMode.Create))
                {
                    UploadFile.FileName.CopyTo(fileStream);
                }
                return RedirectToAction("index");
            }
            return View("UploadResult", UploadFile);
        }

        [HttpGet]
        public ActionResult RemoveFile(int id)
        {
            var FileNextProses = _context.FileNextProcessModel.SingleOrDefault(m => m.id == id);

            FileInfo filex = new FileInfo(FileResult + FileNextProses.FileName);
            if (filex.Exists) System.IO.File.Delete(filex.ToString());

            FileNextProses.tglProses = null;
            FileNextProses.FileName = null;
            _context.Update(FileNextProses);
            _context.SaveChanges();

            return RedirectToAction("index");
        }

        public FileStreamResult DownloadFile(string fileName)
        {
            Response.Headers.Add("content-disposition", "attachment; filename=" + fileName);
            return File(new FileStream(FileResult + fileName, FileMode.Open), "application/octet-stream");
        }
    }
}
