using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HidVanguard.Config.Components
{
    public interface ICompositionRoot
    {
        int Run(string[] args);
    }

    public class CompositionRoot : ICompositionRoot
    {
        private IApp _app;

        public CompositionRoot(
            IApp app
        )
        {
            _app = app;
        }
        public int Run(string[] args)
        {
            try
            {
                _app.InitializeComponent();

                return _app.Run();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("General error:\r\n\r\n" + ex.ToString(), "HidVanguard Configuration", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return -1;
            }
        }
    }
}
