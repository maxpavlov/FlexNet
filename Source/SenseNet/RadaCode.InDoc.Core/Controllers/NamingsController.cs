using System.Linq;
using RadaCode.InDoc.Core.Models;
using RadaCode.InDoc.Data.DocumentNaming.SpecialNamings;
using RadaCode.InDoc.Data.EF;
using System.Web.Mvc;

namespace RadaCode.InDoc.Core.Controllers
{
    public class NamingsController: ControllerBase
    {
        private InDocContext _context;

        public NamingsController(InDocContext context)
        {
            _context = context;
        }

        //[HttpGet]
        //public JsonResult GetAllAvailableSpecialCodes()
        //{
        //    var res = SpecialNamingsFactory.ListAllNamingProcessorCodes();
        //    return Json(res, JsonRequestBehavior.AllowGet);
        //}

        public string RenderExistingNamings()
        {
            var namings = _context.NamingApproaches.ToList();
            var codes = SpecialNamingsFactory.ListAllNamingProcessorCodes();

            var viewModel = new ExistingNamingsViewModel
                                {
                                    Namings = namings.Select(
                                        namingApproach =>
                                        new NamingViewModel(
                                            namingApproach.TypeName, 
                                            namingApproach.NameBlocks, 
                                            namingApproach.ParamBlocks)).ToList(),
                                    Codes = codes.Select( 
                                        code => 
                                        new SpecialCodeViewModel()
                                            {
                                                 Code = code.Key,
                                                 HasValue = code.Value
                                            }).ToList()
                                };

            return RenderRazorViewToString("ExistingNamings", viewModel);
        }
    }
}
