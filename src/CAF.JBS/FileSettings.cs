using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace CAF.JBS
{
    public class FileSettings
    {
        public  string FileBilling { get; private set; }
        public  string BackupBilling { get; private set; }
        public  string Result { get; private set; }
        public  string BackupResult { get; private set; }
        public  string Template { get; private set; }
        public  string TempBNIcc { get; private set; }
        public  string TempMandiriCC { get; private set; }
        public  string TempBCAac { get; private set; }

        public string BCAcc { get; private set; }
        public string MandiriCC { get; private set; }
        public string MegaonUsCC { get; private set; }
        public string MegaOffUsCC { get; private set; }
        public string BNIcc { get; private set; }

        public string BCAac { get; private set; }
        public string MandiriAC { get; private set; }

        public string BCAva { get; private set; }

        private IConfigurationRoot Configuration { get; set; }

        public FileSettings()
        {
            var builder = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");
            Configuration = builder.Build();

            FileBilling = Configuration.GetValue<string>("FileSetting:FileBilling");
            BackupBilling = Configuration.GetValue<string>("FileSetting:BackupBilling");
            Result = Configuration.GetValue<string>("FileSetting:Result");
            BackupResult = Configuration.GetValue<string>("FileSetting:BackupResult");
            Template = Configuration.GetValue<string>("FileSetting:TemplateFile");

            TempBNIcc = Configuration.GetValue<string>("FileSetting:TemplateBNIcc");
            TempMandiriCC = Configuration.GetValue<string>("FileSetting:TemplateMandiriCC");
            TempBCAac = Configuration.GetValue<string>("FileSetting:TemplateBCAac");

            BCAcc = "CAF" + DateTime.Now.ToString("ddMM") + ".prn";
            MandiriCC = "Mandiri_" + DateTime.Now.ToString("ddMMyyyy") + ".xls";
            MegaonUsCC = "CAF" + DateTime.Now.ToString("yyyyMMdd") + "_MegaOnUs.bpmt";
            MegaOffUsCC = "CAF" + DateTime.Now.ToString("yyyyMMdd") + "_MegaOffUs.bpmt";
            BNIcc = "BNI_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx";

            BCAac = Configuration.GetValue<string>("FileSetting:TemplateBNIcc");
            MandiriAC = Configuration.GetValue<string>("FileSetting:TemplateMandiriCC");
            BCAva = Configuration.GetValue<string>("FileSetting:TemplateBCAac");
        }

    }
}
