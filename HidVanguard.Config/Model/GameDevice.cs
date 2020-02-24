using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HidVanguard.Config.Model
{
    [DebuggerDisplay("{DeviceId} [{BusName}]")]
    public class GameDevice
    {
        public string DeviceId { get; set; }
        public string BusName { get; set; }
        public string[] HardwareIds { get; set; }
    }
}
