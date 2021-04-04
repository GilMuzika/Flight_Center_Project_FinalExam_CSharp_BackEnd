using Flight_Center_Project_FinalExam_DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Api_interface.Models
{
    public class AlikeQueryParameters
    {
        public string searchQuery { get; set; }
        public SearchFlightsCriterion searchCriterion { get; set; }
        public SortFlightsCriterion sortingCriterion { get; set; }
    }
}