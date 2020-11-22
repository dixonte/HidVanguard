using GalaSoft.MvvmLight.Command;
using HidVanguard.Config.Components.Services;
using HidVanguard.Config.Contrib;
using HidVanguard.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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

        public TrulyObservableCollection<AllowedProcess> AllowedProcesses { get; set; }
        public AllowedProcess SelectedProcess { get; set; }
        public bool ProcessListDirty { get; set; }

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

            AllowedProcesses = new TrulyObservableCollection<AllowedProcess>();
            AllowedProcesses.ItemChanged += AllowedProcesses_ItemChanged;

            Load();
        }

        private void AllowedProcesses_ItemChanged(object sender, ItemChangedEventArgs<AllowedProcess> e)
        {
            if (e.PropertyName != nameof(AllowedProcess.Dirty))
            {
                ProcessListDirty = true;
            }
        }

        private void Refresh()
        {
            whitelistSerice.Refresh();

            Load();
        }

        private void Load()
        {
            GameDevices = new ObservableCollection<GameDevice>(deviceService.GetGameDevices());
            AllowedProcesses.Clear();
            AllowedProcesses.AddRange(whitelistSerice.GetAllowedProcesses());
            ProcessListDirty = false;
            HidGuardianInstalled = whitelistSerice.GetHidGuardianInstalled();
            HidVanguardInstalled = whitelistSerice.GetHidVanguardInstalled();

            UpdateGameDevicesHidden();
        }

        private void UpdateGameDevicesHidden()
        {
            foreach (var gameDevice in GameDevices)
            {
                var hidden = whitelistSerice.GetDeviceHidden(gameDevice);

                // If a device's hidden status changed, toggle it for the benefit of other programs
                if (gameDevice.Hidden != hidden)
                {
                    deviceService.DisableDevice(devId => devId == gameDevice.DeviceId, toggle: true);

                    gameDevice.Hidden = hidden;
                }
            }
        }

        private ICommand _gameDeviceToggleHiddenCommand;

        public ICommand GameDeviceToggleHiddenCommand => _gameDeviceToggleHiddenCommand ?? (_gameDeviceToggleHiddenCommand = new RelayCommand<GameDevice>(gameDevice =>
        {
            if (gameDevice == null)
                return;

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

        private ICommand _addProcessCommand;
        public ICommand AddProcessCommand => _addProcessCommand ?? (_addProcessCommand = new RelayCommand(() =>
        {
            SelectedProcess = new AllowedProcess
            {
                Name = "<new process>"
            };
            AllowedProcesses.Add(SelectedProcess);
        }));

        private ICommand _deleteProcessCommand;
        public ICommand DeleteProcessCommand => _deleteProcessCommand ?? (_deleteProcessCommand = new RelayCommand(() =>
        {
            if (SelectedProcess != null)
            {
                AllowedProcesses.Remove(SelectedProcess);
                SelectedProcess = null;
                ProcessListDirty = true;
            }
        }));

        private ICommand _saveProcessCommand;
        public ICommand SaveProcessCommand => _saveProcessCommand ?? (_saveProcessCommand = new RelayCommand(() =>
        {
            foreach (var process in AllowedProcesses)
                process.Dirty = false;

            whitelistSerice.SetAllowedProcesses(AllowedProcesses);
        }));

        private ICommand _reloadProcessesCommand;
        public ICommand ReloadProcessesCommand => _reloadProcessesCommand ?? (_reloadProcessesCommand = new RelayCommand(() =>
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Reload and lose any unsaved changes?", "HidVanguard", MessageBoxButton.YesNo, MessageBoxImage.Question))
                Refresh();
        }));

        private ICommand _selectProcessLocationCommand;
        public ICommand SelectProcessLocationCommand => _selectProcessLocationCommand ?? (_selectProcessLocationCommand = new RelayCommand(() =>
        {
            var ofd = new OpenFileDialog
            {
                Title = "Select Application",
                Filter = "Executable files (*.exe)|*.exe",
                CheckFileExists = true,
                CheckPathExists = true,
                DereferenceLinks = true
            };

            if (ofd.ShowDialog() == true)
            {
                SelectedProcess.Name = ofd.SafeFileName;
                SelectedProcess.DirPath = Path.GetDirectoryName(ofd.FileName);

                SelectedProcess.PopulateHash();
            }
        }));
    }
}
