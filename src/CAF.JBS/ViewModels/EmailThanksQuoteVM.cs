using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class EmailThanksQuoteVM
    {
        public int QuoteID { get; set; }
        public String RefNo { get; set; }
        public String Sapaan { get; set; }
        public String CustName { get; set; }
        public String Gender { get; set; }
        public String POB { get; set; }
        public DateTime DOB { get; set; }
        public String TempatLahir { get; set; }
        public String Email { get; set; }
        public String MobileNo { get; set; }
        public String ProductName { get; set; }

        public Decimal PremiAmount { get; set; }
        public Decimal CetakPolisAmount { get; set; }
        public Decimal Insured { get; set; }
        public String FrekuensiBayar { get; set; }
        public String PaymentMeth { get; set; }
        public Decimal PaymentAmount { get; set; }
        public String Status { get; set; }
        public int DurasiTahun { get; set; }
        public int DurasiHari { get; set; }
    }
}
