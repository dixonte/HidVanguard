using HidVanguard.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HidVanguard.Config.Components.Services
{
    public interface IWhitelistService
    {
        void Refresh();

        bool GetHidGuardianInstalled();
        bool GetHidVanguardInstalled();

        void SetDeviceHidden(GameDevice device, bool hidden);
        bool GetDeviceHidden(GameDevice device);

        IEnumerable<AllowedProcess> GetAllowedProcesses();
        void SetAllowedProcesses(IEnumerable<AllowedProcess> allowedProcesses);
    }

    public class WhitelistService : IWhitelistService
    {
        private IDeviceService deviceService;

        private List<string> _affectedDevices;

        public WhitelistService(
            IDeviceService deviceService
        )
        {
            this.deviceService = deviceService;

            Refresh();
        }

        public void Refresh()
        {
            var affectedDevices = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HidGuardian\Parameters", "AffectedDevices", null) as string[];

            if (affectedDevices == null)
                affectedDevices = new string[] { };

            _affectedDevices = new List<string>(affectedDevices);
        }

        public bool GetHidGuardianInstalled()
        {
            return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HidGuardian", "ImagePath", null) != null;
        }

        public bool GetHidVanguardInstalled()
        {
            return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HidVanguard", "ImagePath", null) != null;
        }

        public bool GetDeviceHidden(GameDevice device)
        {
            return device.HardwareIds.All(id => _affectedDevices.Contains(id, StringComparer.InvariantCultureIgnoreCase));
        }

        public void SetDeviceHidden(GameDevice device, bool hidden)
        {
            if (hidden)
                _affectedDevices = _affectedDevices.Union(device.HardwareIds, StringComparer.InvariantCultureIgnoreCase).ToList();
            else
                _affectedDevices = _affectedDevices.Except(device.HardwareIds, StringComparer.InvariantCultureIgnoreCase).ToList();

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HidGuardian\Parameters", "AffectedDevices", _affectedDevices.ToArray(), RegistryValueKind.MultiString);

            // Toggle device
            deviceService.DisableDevice(hwid =>
            {
                //if (hwid.Length == 0)
                //    return false;

                //var hidA = hwid.Where(i => i.StartsWith("HID\\"));
                //var hidB = device.HardwareIds.Where(i => i.StartsWith("HID\\"));

                //return hwid.Length > 0 && hwid.Length == device.HardwareIds.Length && hwid.Zip(device.HardwareIds).All(z => z.First.Equals(z.Second));

                return hwid == device.DeviceId;
            }, true, true);
        }

        public IEnumerable<AllowedProcess> GetAllowedProcesses()
        {
            var allowed = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HidVanguard\Parameters", "AllowedProcesses", null) as string[];

            foreach(var allowedProcessString in allowed)
            {
                yield return AllowedProcess.FromString(allowedProcessString);
            }
        }

        public void SetAllowedProcesses(IEnumerable<AllowedProcess> allowedProcesses)
        {
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HidVanguard\Parameters", "AllowedProcesses", allowedProcesses.Select(p => p.ToString()).ToArray(), RegistryValueKind.MultiString);

            // TODO: Restart service
        }
    }
}
