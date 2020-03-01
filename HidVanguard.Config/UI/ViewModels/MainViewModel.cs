﻿using GalaSoft.MvvmLight.Command;
using HidVanguard.Config.Components.Services;
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

        public ObservableCollection<AllowedProcess> AllowedProcesses { get; set; }
        public AllowedProcess SelectedProcess { get; set; }
        public AllowedProcess EditProcess { get; set; }

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

        private ICommand _addProcessCommand;
        public ICommand AddProcessCommand => _addProcessCommand ?? (_addProcessCommand = new RelayCommand(() =>
        {
            EditProcess = new AllowedProcess
            {
                Name = "<new process>"
            };
            AllowedProcesses.Add(EditProcess);
            SelectedProcess = EditProcess;
        }));

        private ICommand _editProcessCommand;
        public ICommand EditProcessCommand => _editProcessCommand ?? (_editProcessCommand = new RelayCommand(() =>
        {
            EditProcess = SelectedProcess;
        }));

        private ICommand _deleteProcessCommand;
        public ICommand DeleteProcessCommand => _deleteProcessCommand ?? (_deleteProcessCommand = new RelayCommand(() =>
        {
            if (SelectedProcess != null)
                AllowedProcesses.Remove(SelectedProcess);
            EditProcess = null;
        }));

        private ICommand _saveProcessCommand;
        public ICommand SaveProcessCommand => _saveProcessCommand ?? (_saveProcessCommand = new RelayCommand(() =>
        {
            if (EditProcess != null)
                EditProcess.Dirty = false;
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
                EditProcess.Name = ofd.SafeFileName;
                EditProcess.DirPath = Path.GetDirectoryName(ofd.FileName);

                EditProcess.PopulateHash();
            }
        }));
    }
}
