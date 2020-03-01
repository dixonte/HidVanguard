using GalaSoft.MvvmLight.Command;
using HidVanguard.Config.Components.Services;
using HidVanguard.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
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

        public ObservableCollection<AllowedProcess> AllowedProcesses { get; set; }
        public AllowedProcess SelectedProcess { get; set; }

        public bool HidGuardianInstalled { get; set; }
        public bool HidVanguardInstalled { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private IDeviceService deviceService;
        private IWhitelistService whitelistSerice;

        public MainViewModel(
            IDeviceService deviceService,
            IWhitelistService whitelistSerice
        )
        {
            this.deviceService = deviceService;
            this.whitelistSerice = whitelistSerice;

            Load();
        }

        private void Refresh()
        {
            whitelistSerice.Refresh();

            Load();
        }

        private void Load()
        {
            GameDevices = new ObservableCollection<GameDevice>(deviceService.GetGameDevices());
            AllowedProcesses = new ObservableCollection<AllowedProcess>(whitelistSerice.GetAllowedProcesses());
            HidGuardianInstalled = whitelistSerice.GetHidGuardianInstalled();
            HidVanguardInstalled = whitelistSerice.GetHidVanguardInstalled();

            UpdateGameDevicesHidden();
        }

        private void UpdateGameDevicesHidden()
        {
            foreach (var gameDevice in GameDevices)
            {
                gameDevice.Hidden = whitelistSerice.GetDeviceHidden(gameDevice);
            }
        }

        private ICommand _gameDeviceToggleHiddenCommand;

        public ICommand GameDeviceToggleHiddenCommand => _gameDeviceToggleHiddenCommand ?? (_gameDeviceToggleHiddenCommand = new RelayCommand<GameDevice>(gameDevice =>
        {
            if (HidGuardianInstalled)
            {
                whitelistSerice.SetDeviceHidden(gameDevice, !gameDevice.Hidden);

                UpdateGameDevicesHidden();
            }
            else
            {
                MessageBox.Show("As HidGuardian is not installed, devices cannot be hidden.", "HidGuardian / HidVanguard Configuration", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }));

        private ICommand _refreshCommand;
        public ICommand RefreshCommand => _refreshCommand ?? (_refreshCommand = new RelayCommand(Refresh));
    }
}
