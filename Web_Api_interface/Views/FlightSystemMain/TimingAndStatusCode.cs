using AutoMapper;
using Flight_Center_Project_FinalExam_DAL;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using System.Windows.Data;

namespace Web_Api_interface.Views.FlightSystemMain
{
    /// <summary>
    /// This classs creates time delays fo estimated arrival or departure time for flights,
    /// and also creates stat
    /// </summary>
    public class TimingAndStatusCode
    {
        private Random _rnd = new Random();

        public DateTime EstimatedTime { get; set; }
        public string StatusMessage { get; set; }
        public string StatusBoxColor { get; set; }

        private DateTime _estimatedTime;

        private Flight _flight;
        private string _pageKind;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flight"></param>
        /// <param name="pageKind">If I seek for Departure or Landing flights</param>
        public TimingAndStatusCode(Flight flight, string pageKind)
        {
            _flight = flight;
            _pageKind = pageKind;

            SetDefaultValues();

            if(pageKind == "Departures" || pageKind == "Landings")
                GetEstimatedTime();
        }

        public void SetDefaultValues()
        {
            //Estimated arrival time 
            //TimeSpan estimateTime = flight.LANDING_TIME.Subtract(flight.DEPARTURE_TIME);
            _estimatedTime = _flight.DEPARTURE_TIME;
            this.EstimatedTime = _estimatedTime;

            this.StatusMessage = "---------";
            this.StatusBoxColor = "#77ca50;";
        }

        public void GetEstimatedTime()
        {
            if (_pageKind == "Departures")
            {
                this.StatusMessage = "ON TIME";

                if (_rnd.Next(1, 10) > 8)
                {
                    this.StatusMessage = "DELAYED";
                    this.StatusBoxColor = "red;";

                    int delayingHours = _rnd.Next(0, 4);
                    int delayingMinutes = 0;
                    if (delayingHours == 0) { delayingMinutes = 30; }

                    TimeSpan delayingTime = new TimeSpan(delayingHours, delayingMinutes, 0);
                    this.EstimatedTime = _estimatedTime.Add(delayingTime);
                }
            }


            if (_pageKind == "Landings")
            {
                TimeSpan tspn = DateTime.Now.Subtract(_flight.LANDING_TIME);
                TimeSpan minutes15 = new TimeSpan(0, 15, 0);
                if (DateTime.Now.Subtract(_flight.LANDING_TIME) <= new TimeSpan(0, 15, 0) && DateTime.Now.Subtract(_flight.LANDING_TIME) >= new TimeSpan(0, 0, 0))
                { this.StatusMessage = "LANDING"; this.StatusBoxColor = "#f3850e;"; }
                if (DateTime.Now.Subtract(_flight.LANDING_TIME) <= new TimeSpan(0, 0, 0))
                {
                    this.StatusMessage = "LANDED";


                }
                if (_flight.DEPARTURE_TIME < DateTime.Now && _flight.LANDING_TIME > DateTime.Now)
                {

                    this.StatusMessage = "NOT FINAL";


                    if (_rnd.Next(1, 10) == 1 || _rnd.Next(1, 10) <= 2)
                    {
                        this.StatusMessage = "LANDING"; this.StatusBoxColor = "#f3850e;";
                        TimeSpan addToEstTime = new TimeSpan(0, _rnd.Next(30, 240), 0);
                        _estimatedTime += addToEstTime;
                        this.EstimatedTime = _estimatedTime;
                    }

                }
                //var r = flight.LANDING_TIME.Subtract(DateTime.Now);
                if (_flight.LANDING_TIME.Subtract(DateTime.Now) <= new TimeSpan(2, 0, 0) && _flight.LANDING_TIME.Subtract(DateTime.Now) > new TimeSpan(0, 15, 0))
                { this.StatusMessage = "FINAL"; }

            }
        }
    }
}