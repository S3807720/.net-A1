using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBA.Models
{
    class Customer
    {
        public int customerId { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string postCode { get; set; }
        public List<Account> accounts { get; set; } 
    }
}
