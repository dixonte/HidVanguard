using HidVanguard.Config.Components.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HidVanguard.Config.UI.ViewModels
{
    public interface IMainViewModel
    {

    }

    public class MainViewModel : IMainViewModel
    {
        private IDeviceService deviceService;

        public MainViewModel(
            IDeviceService deviceService
        )
        {
            this.deviceService = deviceService;

            var devices = deviceService.GetGameDevices().ToList();
        }
    }
}
