using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using HidVanguard.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace HidVanguard.Service
{
    public class Worker : BackgroundService
    {
        private const uint POLL_INTERVAL_SEC = 2;

        private readonly ILogger<Worker> _logger;

        private List<AllowedProcess> allowedProcesses;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HidVanguard.Service starting...");

            allowedProcesses = GetAllowedProcesses().ToList();

            _logger.LogDebug($"Loaded {allowedProcesses.Count} allowed processes:\r\n{string.Join("\r\n", allowedProcesses.Select(a => $"{a.Name} in {a.DirPath ?? "<null>"} with hash {a.Hash ?? "<null>"}"))}");

            var eventFilter = string.Join(" OR ", allowedProcesses.Select(a => $"TargetInstance.Name = '{a.Name.Replace("'", "''")}'"));
            var win32ProcessFilter = string.Join(" OR ", allowedProcesses.Select(a => $"Name = '{a.Name.Replace("'", "''")}'"));

            var creationWatcher = new ManagementEventWatcher(@"\\.\root\CIMV2", $"SELECT * FROM __InstanceCreationEvent WITHIN {POLL_INTERVAL_SEC} WHERE TargetInstance ISA 'Win32_Process' AND ({eventFilter})");
            creationWatcher.EventArrived += CreationWatcher_EventArrived;
            creationWatcher.Start();
            var deletionWatcher = new ManagementEventWatcher(@"\\.\root\CIMV2", $"SELECT * FROM __InstanceDeletionEvent WITHIN {POLL_INTERVAL_SEC} WHERE TargetInstance ISA 'Win32_Process' AND ({eventFilter})");
            deletionWatcher.EventArrived += DeletionWatcher_EventArrived;
            deletionWatcher.Start();

            _logger.LogInformation("WMI watchers started.");

            using (var currentQuery = new ManagementObjectSearcher(@"\\.\root\CIMV2", $"SELECT * FROM Win32_Process WHERE {win32ProcessFilter}"))
            using (var currentCol = currentQuery.Get())
            {
                foreach (var targetInstance in currentCol)
                {
                    if (ValidateTargetInstance(targetInstance))
                    {
                        var pid = GetTargetInstancePID(targetInstance);
                        var name = GetTargetInstanceName(targetInstance);

                        _logger.LogInformation($"Trusted process {name} found already running with PID {pid}, adding to whitelist.");

                        WhitelistPid(pid, true);
                    }
                }
            }

            _logger.LogInformation("Waiting for shutdown command.");

            await Task.Delay(-1, stoppingToken);

            _logger.LogInformation("HidVanguard.Service stopping...");

            creationWatcher.Dispose();
            deletionWatcher.Dispose();
        }

        private IEnumerable<AllowedProcess> GetAllowedProcesses()
        {
            var allowed = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HidVanguard\Parameters", "AllowedProcesses", null) as string[];

            foreach (var allowedProcessString in allowed)
            {
                yield return AllowedProcess.FromString(allowedProcessString);
            }
        }

        private void CreationWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (e.NewEvent.GetPropertyValue("TargetInstance") is ManagementBaseObject targetInstance && ValidateTargetInstance(targetInstance))
            {
                var pid = GetTargetInstancePID(targetInstance);
                var name = GetTargetInstanceName(targetInstance);

                _logger.LogInformation($"Trusted process {name} found with PID {pid}, adding to whitelist.");

                WhitelistPid(pid, true);
            }
        }

        private void DeletionWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (e.NewEvent.GetPropertyValue("TargetInstance") is ManagementBaseObject targetInstance && ValidateTargetInstance(targetInstance)) {
                var pid = GetTargetInstancePID(targetInstance);
                var name = GetTargetInstanceName(targetInstance);

                _logger.LogInformation($"Trusted process {name} with PID {pid} closed, removing from whitelist.");

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

            // Toggle a device to make sure apps sees the change
            try
            {
                if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HidGuardian\Parameters", "AffectedDevices", null) is string[] affectedDevices)
                {
                    _logger.LogInformation($"Loaded {affectedDevices.Length} affected device(s).");

                    if (affectedDevices.Length > 0)
                    {
                        Contrib.DisableHardware.DisableDevice((hids) =>
                        {
                            var hid = hids.Split('\0').Where(x => x.StartsWith("HID")).OrderByDescending(x => x.Length).FirstOrDefault();

                            if (string.IsNullOrWhiteSpace(hid))
                                return false;

                            if (affectedDevices.Contains(hid))
                            {
                                _logger.LogInformation($"Toggling affected device: {hid}");

                                return true;
                            }

                            return false;
                        }, true, true);
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

            var name = Path.GetFileName(path);
            var dir = Path.GetDirectoryName(path);

            var matches = allowedProcesses.Where(p => name.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase) && (string.IsNullOrEmpty(p.DirPath) || dir.Equals(p.DirPath, StringComparison.InvariantCultureIgnoreCase)));
            var valid = matches.Any(m => m.LooseValidate());

            return valid;
        }

        private string GetTargetInstanceName(ManagementBaseObject targetInstance)
        {
            if (targetInstance == null)
                throw new ArgumentNullException(nameof(targetInstance));

            if (targetInstance.GetPropertyValue("Name") is string instanceName)
                return instanceName;
            else
                return null;
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
