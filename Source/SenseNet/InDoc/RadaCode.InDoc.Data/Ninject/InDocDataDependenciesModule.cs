using Ninject.Modules;
using Ninject.Web.Common;
using RadaCode.InDoc.Data.EF;

namespace RadaCode.InDoc.Data.Ninject
{
    public class InDocDataDependenciesModule: NinjectModule
    {
        public override void Load()
        {
            Bind<InDocContext>().ToSelf().InRequestScope();
        }
    }
}
