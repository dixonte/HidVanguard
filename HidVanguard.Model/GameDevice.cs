using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace HidVanguard.Model
{
    [DebuggerDisplay("{DeviceId} [{BusName}]")]
    public class GameDevice : INotifyPropertyChanged
    {
        public string DeviceId { get; set; }
        public string Description { get; set; }
        public string BusName { get; set; }
        public string[] HardwareIds { get; set; }
        public bool Hidden { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public bool Present { get; set; }

        public string DisplayName => BusName ?? Description;
    }
}
