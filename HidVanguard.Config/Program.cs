using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using HidVanguard.Config.Components;
using HidVanguard.Config.Components.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace HidVanguard.Config
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            using (var container = new WindsorContainer())
            {
                // Registration
                container.Install(FromAssembly.Containing<CompositionRoot>());
                

                // Bootstrap
                var root = container.Resolve<ICompositionRoot>();

                return root.Run(args);
            }
        }
    }
}
