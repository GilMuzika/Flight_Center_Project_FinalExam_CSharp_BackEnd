using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web_Api_interface.MvcControllers
{
    public class FlightSystemMainController : Controller
    {
        // GET: FlightSystemMain
        public ActionResult FlightSystemMainView()
        {            
            ViewBag.PageKind = "Departures";
            ViewBag.PageKindIconSrc = "departures.gif";
            return View("FlightSystemMainView");
            //return View();
        }

        public ActionResult Departures(string str)
        {
            ViewBag.Title = "Departures";
            ViewBag.PageKind = "Departures";
            ViewBag.PageKindIconSrc = "departures.gif";
            return View("FlightSystemMainView");
        }

        public ActionResult AllFlights(string str)
        {
            ViewBag.Title = "All Flights";
            ViewBag.PageKind = "AllFlights";
            ViewBag.PageKindIconSrc = "departures.gif";
            return View("FlightSystemMainView");
        }

        public ActionResult Landings(string str)
        {
            ViewBag.Title = "Landings";
            ViewBag.PageKind = "Landings";
            ViewBag.PageKindIconSrc = "arrivings.gif";
            return View("FlightSystemMainView");
        }

        public ActionResult SearchPage(string str)
        {
            ViewBag.Title = "Search";
            ViewBag.PageKind = "Search";
            ViewBag.PageKindIconSrc = "search_small.gif";
            return View();
        }

        public ActionResult Search()
        {
            return new FilePathResult("~/Views/FlightSystemMain/search.html", "text/html");
        }

        public ActionResult EditCustomers()
        {
            return new FilePathResult("~/Views/FlightSystemMain/edit.html", "text/html");
        }

        public ActionResult  ViewCustomers()
        {
            return new FilePathResult("~/Views/FlightSystemMain/view_clients.html", "text/html");
        }

        public ActionResult index_test_page()
        {
            return new FilePathResult("~/Views/FlightSystemMain/index_test_page.html", "text/html");
        }

        public ActionResult WebClientSideJobDoer_test()
        {
            return View();
        }

        public ActionResult DavidNochovich_flightsSearch()
        {
            return new FilePathResult("~/Views/FlightSystemMain/DavidNochovich_flightsSearch.html", "text/html");
        }
    }
}