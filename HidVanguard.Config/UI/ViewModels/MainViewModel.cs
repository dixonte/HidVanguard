using GalaSoft.MvvmLight.Command;
using HidVanguard.Config.Components.Services;
using HidVanguard.Config.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace HidVanguard.Config.UI.ViewModels
{
    public interface IMainViewModel
    {
    }

    public class MainViewModel : IMainViewModel, INotifyPropertyChanged
    {
        public ObservableCollection<GameDevice> GameDevices { get; set; }
        public GameDevice SelectedDevice { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private IDeviceService deviceService;

        public MainViewModel(
            IDeviceService deviceService
        )
        {
            this.deviceService = deviceService;

            GameDevices = new ObservableCollection<GameDevice>(deviceService.GetGameDevices());
        }

        private ICommand _gameDeviceToggleHiddenCommand;

        public ICommand GameDeviceToggleHiddenCommand => _gameDeviceToggleHiddenCommand ?? (_gameDeviceToggleHiddenCommand = new RelayCommand<GameDevice>(gameDevice =>
        {
            gameDevice.Hidden = !gameDevice.Hidden;
        }));
    }
}
