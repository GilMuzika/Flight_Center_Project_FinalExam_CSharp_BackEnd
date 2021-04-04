using Flight_Center_Project_FinalExam_DAL;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Api_interface.Models
{
    /// <summary>
    /// this is a model "poco" class  that bears information about flight 
    /// returned to the user
    /// </summary>
    public class FlightData
    {
        public long iD { get; set; }
        public string Adorning { get; set; }
        public string Image { get; set; }
        public string AirlineName { get; set; }
        public string DepartureCountryName { get; set; }
        public string DestinationCountryName { get; set; }
        public string EstimatedTime { get; set; }
        public string StatusMessage { get; set; }
        public string StatusColor { get; set; }
        public decimal Price { get; set; }
        public int RemainingTickets { get; set; }

        public DateTime DEPARTURE_TIME { get; set; }
        public DateTime LANDING_TIME { get; set; }
        public TimeSpan FlightDuration { get; set; }

        public AirlineCompany AirlineCompany { get; set; }
        public Country DepartureCountry { get; set; }
        public Country DestinationCountry { get; set; }


    }
}