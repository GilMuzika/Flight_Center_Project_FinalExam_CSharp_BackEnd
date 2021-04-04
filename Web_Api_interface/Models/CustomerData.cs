using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Api_interface.Models
{
    public class CustomerData
    {
        public long iD { get; set; }
        public string Image { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
    }
}