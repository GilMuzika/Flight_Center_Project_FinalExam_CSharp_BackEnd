using Flight_Center_Project_FinalExam_DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Api_interface.Models
{
    public class AirlineCompanyData
    {
        public long iD { get; set; }
        public string Adorning { get; set; }
        public string Image { get; set; }
        public string AirlineName { get; set; }
        public string BaseCountryName { get; set; }

        public Utility_class_User AirlineAsUtilityClassUser { get; set; }
    }
}