using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using HidVanguard.Config.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace HidVanguard.Config.Components.Installers
{
    public class ViewModelInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Classes.FromAssemblyContaining<MainViewModel>().InSameNamespaceAs<MainViewModel>().WithService.DefaultInterfaces().LifestyleTransient());
        }
    }
}
