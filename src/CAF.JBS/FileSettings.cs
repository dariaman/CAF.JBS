using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace CAF.JBS
{
    public class FileSettings
    {
        public string FileBilling { get; private set; }
        public string BackupBilling { get; private set; }
        public string Result { get; private set; }
        public string BackupResult { get; private set; }
        public string UploadSchedule { get; private set; }
        public string Template { get; private set; }
        public string TempBNIcc { get; private set; }
        public string TempMandiriCC { get; private set; }
        public string TempBCAac { get; private set; }

        public string BCAcc { get; private set; }
        public string MandiriCC { get; private set; }
        public string MegaonUsCC { get; private set; }
        public string MegaOffUsCC { get; private set; }
        public string BNIcc { get; private set; }

        public string BCAac { get; private set; }
        public string MandiriAC { get; private set; }

        public string BCAva { get; private set; }

        public string GenFileXls { get; private set; }
        public string FileExecresult { get; private set; }

        private IConfigurationRoot Configuration { get; set; }
        private IHostingEnvironment _env;
        public string[] s { get; private set; }
        public FileSettings()
        {
            var builder = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");
            Configuration = builder.Build();

            GenFileXls = Configuration.GetValue<string>("FileSetting:FileGenExcel");
            FileExecresult = Configuration.GetValue<string>("FileSetting:FileExecresult");

            FileBilling = Configuration.GetValue<string>("FileSetting:FileBilling");
            BackupBilling = Configuration.GetValue<string>("FileSetting:BackupBilling");
            Result = Configuration.GetValue<string>("FileSetting:Result");
            BackupResult = Configuration.GetValue<string>("FileSetting:BackupResult");
            UploadSchedule = Configuration.GetValue<string>("FileSetting:UploadSchedule");
            Template = Configuration.GetValue<string>("FileSetting:TemplateFile");

            TempBNIcc = Configuration.GetValue<string>("FileSetting:TemplateBNIcc");
            TempMandiriCC = Configuration.GetValue<string>("FileSetting:TemplateMandiriCC");
            TempBCAac = Configuration.GetValue<string>("FileSetting:TemplateBCAac");

            BCAcc = FileBilling + "CAF" + DateTime.Now.ToString("ddMM") + ".prn";
            MandiriCC = FileBilling + "Mandiri_" + DateTime.Now.ToString("ddMMyyyy") + ".xls";
            MegaonUsCC = FileBilling + "CAF" + DateTime.Now.ToString("yyyyMMdd") + "_MegaOnUs.bpmt";
            MegaOffUsCC = FileBilling + "CAF" + DateTime.Now.ToString("yyyyMMdd") + "_MegaOffUs.bpmt";
            BNIcc = FileBilling + "BNI_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx";

            BCAac = FileBilling + "BCAac" + DateTime.Now.ToString("yyyyMMdd") + ".xls";
            MandiriAC = FileBilling + "MandiriAc" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
            BCAva = FileBilling + "VARegulerPremi" + DateTime.Now.ToString("yyyyMMdd") + ".xls";
            s = System.IO.File.ReadAllLines("appsettings.json");
        }

    }
}
