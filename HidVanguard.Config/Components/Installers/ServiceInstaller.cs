using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using HidVanguard.Config.Components.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace HidVanguard.Config.Components.Installers
{
    public class ServiceInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Classes.FromAssemblyContaining<CompositionRoot>().InSameNamespaceAs<WhitelistService>().WithService.DefaultInterfaces().LifestyleSingleton());
        }
    }
}
