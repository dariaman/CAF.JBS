using CAF.JBS.Data;
using CAF.JBS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Mvc.JQuery.DataTables;
using Mvc.JQuery.DataTables.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Controllers
{
    public class PolicyAcController : Controller
    {
        private readonly JbsDbContext _context;

        public PolicyAcController(JbsDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var getDataUrl = Url.Action(nameof(PolicyAcController.GetData));
            var vm = DataTablesHelper.DataTableVm<PolicyAcVM>("idForTableElement", getDataUrl);

            vm.Filter = true; 
            vm.ShowFilterInput = true;
            vm.UseColumnFilterPlugin = true;

            //vm
            //    .FilterOn("polis", new { sSelector = "#custom-filter-placeholder-position" }, new { sSearch = "Tester" }).Select("Engineer", "Tester", "Manager")
            //    .FilterOn("AccNo").NumberRange();
                //.FilterOn("Salary", new { sSelector = "#custom-filter-placeholder-salary" }).NumberRange();

            vm.JsOptions.Add("fnCreatedRow", new Raw(@"function( nRow, aData, iDataIndex ) {
                    $(nRow).attr('data-id', aData[0]);
                }"));
            vm.StateSave = true;
            vm.LengthMenu = LengthMenuVm.Default();
            vm.LengthMenu.RemoveAll(t => t.Item2 == 5);
            vm.PageLength = 10;
            vm.ColVis = true;
            vm.ShowVisibleColumnPicker = true;

            return View(vm);
        }

        public DataTablesResult<PolicyAcVM> GetData([FromForm] DataTablesParam dataTableParam)
        {
            return DataTablesResult.Create(_context.PolicyAcModel.Select(pa => new PolicyAcVM()
            {
                PolicyId = pa.PolicyId,
                acc_no = pa.acc_no
            }), dataTableParam,row => new
            {
                polis = "<b>" + row.PolicyId + "</b>",
                AccNo = row.acc_no
            });

        }
    }
}
