using System;
using System.Collections.Generic;
using System.Text;

namespace HidVanguard.Config.Components
{
    public interface IApp
    {
        void InitializeComponent();
        int Run();
    }
}
