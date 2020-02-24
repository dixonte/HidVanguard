using HidVanguard.Config.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HidVanguard.Config.Components
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IApp
    {
        private IMainViewModel mainViewModel;

        public App() : base() { }

        public App(
            IMainViewModel mainViewModel
        )
        {
            this.mainViewModel = mainViewModel;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

            MainWindow = new UI.MainWindow();
            MainWindow.DataContext = mainViewModel;
            MainWindow.Show();
        }
    }
}
