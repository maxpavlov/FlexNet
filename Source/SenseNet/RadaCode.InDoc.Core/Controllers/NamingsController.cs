using System.Linq;
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

            //var viewModel = new ExistingNamingsModel()
            //                    {
            //                        ExistingNamings =
            //                            namings.Select(
            //                                namingApproach =>
            //                                new NamingViewModel()
            //                                    {
            //                                        TypeName = namingApproach.TypeName,
            //                                        NameBlocks = namingApproach.NameBlocks,
            //                                        ParamBlocks = namingApproach.ParamBlocks
            //                                    }).ToList()
            //                    };

            var viewModel =
                namings.Select(
                    namingApproach =>
                    new NamingViewModel()
                        {
                            TypeName = namingApproach.TypeName,
                            NameBlocks = namingApproach.NameBlocks,
                            ParamBlocks = namingApproach.ParamBlocks
                        }).ToList();

            return this.RenderRazorViewToString("ExistingNamings", viewModel);
        }
    }
}
