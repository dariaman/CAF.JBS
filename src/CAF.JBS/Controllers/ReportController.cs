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

namespace CAF.JBS.Controllers
{
    public class ReportController : Controller
    {

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
        public IActionResult DailyReconcile(ReportViewModel rpt)
        {

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MonthlyBilling(ReportViewModel rpt)
        {

            return RedirectToAction("Index");
        }
    }
}
