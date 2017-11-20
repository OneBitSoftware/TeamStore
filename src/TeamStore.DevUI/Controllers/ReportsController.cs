using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TeamStore.DevUI.Controllers
{
    public class ReportsController : Controller
    {
        public ReportsController()
        {

        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        // GET: Reports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            return View();
        }
    }
}
