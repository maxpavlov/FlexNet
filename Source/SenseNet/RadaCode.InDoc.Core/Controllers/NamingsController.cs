using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using RadaCode.InDoc.Core.Models;
using RadaCode.InDoc.Data.EF;

namespace RadaCode.InDoc.Core.Controllers
{
    public class NamingsController: ControllerBase
    {
        private InDocContext _context;

        public NamingsController(InDocContext context)
        {
            _context = context;
        }

        public string RenderExistingNamings()
        {
            var namings = _context.NamingApproaches.ToList();

            var viewModel = new ExistingNamingsModel(namings);

            return this.RenderRazorViewToString("ExistingNamings", viewModel);
        }
    }
}
