using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Text;

namespace HidVanguard.Config.Components.Installers
{
    public class DefaultInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Classes.FromAssemblyContaining<CompositionRoot>().InSameNamespaceAs<CompositionRoot>().WithService.DefaultInterfaces().LifestyleTransient());
        }
    }
}
