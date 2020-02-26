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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
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
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString(), "HidVanguard.Config Main Exception", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return -1;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show(e.ExceptionObject.ToString(), "HidVanguard.Config Unhandled Exception", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}
