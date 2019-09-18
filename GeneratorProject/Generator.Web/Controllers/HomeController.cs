using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Generator.Web.ViewModels;
using System.Data.Entity;
using Generator.Web.Models;

namespace Generator.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly Web.Models.Context _context = new Web.Models.Context();
        public ActionResult Index()
        {
            var model = new PersonViewModel();
            var query = _context.People.Include("Employee")
                .Where(p => p.PersonType == "EM");

            model.Persons = query.ToList();
            
            return View(model);
        } 
    }
}