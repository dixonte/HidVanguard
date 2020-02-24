﻿using GalaSoft.MvvmLight.CommandWpf;
using HidVanguard.Config.Components.Services;
using HidVanguard.Config.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace HidVanguard.Config.UI.ViewModels
{
    public interface IMainViewModel
    {
    }

    public class MainViewModel : IMainViewModel
    {
        public ObservableCollection<GameDevice> GameDevices { get; set; }

        private IDeviceService deviceService;

        public MainViewModel(
            IDeviceService deviceService
        )
        {
            this.deviceService = deviceService;

            GameDevices = new ObservableCollection<GameDevice>(deviceService.GetGameDevices());
        }

        private ICommand _GameDeviceDoubleClickCommand;
        //public ICommand GameDeviceDoubleClickCommand => _GameDeviceDoubleClickCommand ?? (_GameDeviceDoubleClickCommand = new RelayCommand(() =>
        //{

        //}));
        public ICommand GameDeviceDoubleClickCommand => new RelayCommand(() =>
        {

        });
    }
}
