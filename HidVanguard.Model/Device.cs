using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HidVanguard.Model
{
    [DebuggerDisplay("{DeviceId} [{BusName}] -> {ParentId}")]
    public class Device
    {
        public string DeviceId { get; set; }
        public string BusName { get; set; }
        public string Description { get; set; }
        public string[] HardwareIds { get; set; }
        public string ParentId { get; set; }
        public Guid? ClassGuid { get; set; }
        public bool Present { get; set; }
    }
}
