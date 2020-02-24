using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace JoystickGremlinWhitelister
{
    public class Worker : BackgroundService
    {
        private const string TARGET_INSTANCE_NAME = "joystick_gremlin.exe";
        private const uint POLL_INTERVAL_SEC = 2;

        private const string INPUTDEV_CLASS = "{745a17a0-74d3-11d0-b6fe-00a0c90f57da}";

        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("JoystickGremlinWhitelister starting...");

            var creationWatcher = new ManagementEventWatcher(@"\\.\root\CIMV2", $"SELECT * FROM __InstanceCreationEvent WITHIN {POLL_INTERVAL_SEC} WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = '{TARGET_INSTANCE_NAME}'");
            creationWatcher.EventArrived += CreationWatcher_EventArrived;
            creationWatcher.Start();
            var deletionWatcher = new ManagementEventWatcher(@"\\.\root\CIMV2", $"SELECT * FROM __InstanceDeletionEvent WITHIN {POLL_INTERVAL_SEC} WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = '{TARGET_INSTANCE_NAME}'");
            deletionWatcher.EventArrived += DeletionWatcher_EventArrived;
            deletionWatcher.Start();

            _logger.LogInformation("WMI watchers started.");

            using (var currentQuery = new ManagementObjectSearcher(@"\\.\root\CIMV2", $"SELECT * FROM Win32_Process WHERE Name = '{TARGET_INSTANCE_NAME}'"))
            using (var currentCol = currentQuery.Get())
            {
                foreach (var targetInstance in currentCol)
                {
                    if (ValidateTargetInstance(targetInstance))
                    {
                        var pid = GetTargetInstancePID(targetInstance);

                        _logger.LogInformation($"Trusted process {TARGET_INSTANCE_NAME} found already running with PID {pid}, adding to whitelist.");

                        WhitelistPid(pid, true);
                    }
                }
            }

            _logger.LogInformation("Waiting for shutdown command.");

            await Task.Delay(-1, stoppingToken);

            creationWatcher.Dispose();
            deletionWatcher.Dispose();

            _logger.LogInformation("JoystickGremlinWhitelister stopping...");
        }

        private void CreationWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (e.NewEvent.GetPropertyValue("TargetInstance") is ManagementBaseObject targetInstance && ValidateTargetInstance(targetInstance))
            {
                var pid = GetTargetInstancePID(targetInstance);

                _logger.LogInformation($"Trusted process {TARGET_INSTANCE_NAME} found with PID {pid}, adding to whitelist.");

                WhitelistPid(pid, true);
            }
        }

        private void DeletionWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (e.NewEvent.GetPropertyValue("TargetInstance") is ManagementBaseObject targetInstance && ValidateTargetInstance(targetInstance)) {
                var pid = GetTargetInstancePID(targetInstance);

                _logger.LogInformation($"Trusted process {TARGET_INSTANCE_NAME} with PID {pid} closed, removing from whitelist.");

                WhitelistPid(pid, false);
            }
        }

        private void WhitelistPid(UInt32 pid, bool whitelist)
        {
            // Add to or remove from whitelist
            try
            {
                if (whitelist)
                    Registry.LocalMachine.CreateSubKey(@$"SYSTEM\CurrentControlSet\Services\HidGuardian\Parameters\Whitelist\{pid}").Dispose();
                else
                    Registry.LocalMachine.DeleteSubKey(@$"SYSTEM\CurrentControlSet\Services\HidGuardian\Parameters\Whitelist\{pid}", false);
            }
            catch (Exception ex)
            {
                if (whitelist)
                    _logger.LogError(ex, "Could not add process to whitelist.");
                else
                    _logger.LogError(ex, "Could not remove process from whitelist.");
            }

            // Toggle a device to make sure Joystick Gremlin sees the change
            try
            {
                if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HidGuardian\Parameters", "AffectedDevices", null) is string[] affectedDevices)
                {
                    _logger.LogInformation($"Loaded {affectedDevices.Length} affected device(s).");

                    if (affectedDevices.Length > 0)
                    {
                        string toggleVictim = null;

                        using (var q = new ManagementObjectSearcher(@"\\.\root\CIMV2", $"SELECT * FROM Win32_PnPEntity WHERE ClassGuid = '{INPUTDEV_CLASS}'"))
                        using (var c = q.Get())
                        {
                            foreach (var o in c)
                            {
                                if (o.GetPropertyValue("HardwareID") is string[] hids && hids.Any(h => affectedDevices.Contains(h)))
                                {
                                    using (var qn = new ManagementObjectSearcher(@"\\.\root\CIMV2", $"SELECT * FROM Win32_PnPEntity WHERE DeviceID = '{o.GetPropertyValue("DeviceID").ToString().Replace(@"HID\", @"USB\").Replace(@"\", @"\\")}'"))
                                    using (var cn = qn.Get())
                                    {
                                        foreach (var on in cn)
                                        {

                                        }
                                    }
                                    
                                    using (var cls = new ManagementClass(o.ClassPath))
                                    {
                                        cls.InvokeMethod("Reset", null);
                                    }
                                }
                            }
                        }

                        //Contrib.DisableHardware.DisableDevice((hids, desc) =>
                        //{
                        //    var hid = hids.Split('\0').Where(x => x.StartsWith("HID")).OrderByDescending(x => x.Length).FirstOrDefault();

                        //    if (string.IsNullOrWhiteSpace(hid))
                        //        return false;

                        //    if (affectedDevices.Contains(hid))
                        //    {
                        //        _logger.LogInformation($"Toggling affected device: {hid}");

                        //        toggleVictim = hids;
                        //        return true;
                        //    }

                        //    return false;
                        //}, true);

                        //if (toggleVictim != null)
                        //{
                        //    Contrib.DisableHardware.DisableDevice((s, desc) => s == toggleVictim, false);
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not read affectedDevices.");
            }
        }

        private bool ValidateTargetInstance(ManagementBaseObject targetInstance)
        {
            if (targetInstance == null)
                throw new ArgumentNullException(nameof(targetInstance));

            var path = targetInstance.GetPropertyValue("ExecutablePath") as string;

            // TODO: Check this path in some way
            return true;
        }

        private UInt32 GetTargetInstancePID(ManagementBaseObject targetInstance)
        {
            if (targetInstance == null)
                throw new ArgumentNullException(nameof(targetInstance));

            if (targetInstance.GetPropertyValue("ProcessId") is UInt32 processId)
                return processId;
            else
                return 0;
        }
    }
}
