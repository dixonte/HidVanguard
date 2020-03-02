// https://stackoverflow.com/questions/4097000/how-do-i-disable-a-system-device-programmatically

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HidVanguard.Service.Contrib
{
    public static class DisableHardware
    {
        #region Imports and declarations
        const uint DIF_PROPERTYCHANGE = 0x12;
        const uint DICS_ENABLE = 1;
        const uint DICS_DISABLE = 2;  // disable device
        const uint DICS_FLAG_GLOBAL = 1; // not profile-specific
        const uint DIGCF_DEFAULT = 0x1;
        const uint DIGCF_PRESENT = 0x2;
        const uint DIGCF_ALLCLASSES = 0x4;
        const uint DIGCF_DEVICEINTERFACE = 0x10;
        const uint ERROR_INVALID_DATA = 13;
        const uint ERROR_NO_MORE_ITEMS = 259;
        const uint ERROR_NOT_FOUND = 1168;

        public enum SPDRP : uint
        {
            /// <summary>
            /// DeviceDesc (R/W)
            /// </summary>
            SPDRP_DEVICEDESC = 0x00000000,

            /// <summary>
            /// HardwareID (R/W)
            /// </summary>
            SPDRP_HARDWAREID = 0x00000001,

            /// <summary>
            /// CompatibleIDs (R/W)
            /// </summary>
            SPDRP_COMPATIBLEIDS = 0x00000002,

            /// <summary>
            /// unused
            /// </summary>
            SPDRP_UNUSED0 = 0x00000003,

            /// <summary>
            /// Service (R/W)
            /// </summary>
            SPDRP_SERVICE = 0x00000004,

            /// <summary>
            /// unused
            /// </summary>
            SPDRP_UNUSED1 = 0x00000005,

            /// <summary>
            /// unused
            /// </summary>
            SPDRP_UNUSED2 = 0x00000006,

            /// <summary>
            /// Class (R--tied to ClassGUID)
            /// </summary>
            SPDRP_CLASS = 0x00000007,

            /// <summary>
            /// ClassGUID (R/W)
            /// </summary>
            SPDRP_CLASSGUID = 0x00000008,

            /// <summary>
            /// Driver (R/W)
            /// </summary>
            SPDRP_DRIVER = 0x00000009,

            /// <summary>
            /// ConfigFlags (R/W)
            /// </summary>
            SPDRP_CONFIGFLAGS = 0x0000000A,

            /// <summary>
            /// Mfg (R/W)
            /// </summary>
            SPDRP_MFG = 0x0000000B,

            /// <summary>
            /// FriendlyName (R/W)
            /// </summary>
            SPDRP_FRIENDLYNAME = 0x0000000C,

            /// <summary>
            /// LocationInformation (R/W)
            /// </summary>
            SPDRP_LOCATION_INFORMATION = 0x0000000D,

            /// <summary>
            /// PhysicalDeviceObjectName (R)
            /// </summary>
            SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000E,

            /// <summary>
            /// Capabilities (R)
            /// </summary>
            SPDRP_CAPABILITIES = 0x0000000F,

            /// <summary>
            /// UiNumber (R)
            /// </summary>
            SPDRP_UI_NUMBER = 0x00000010,

            /// <summary>
            /// UpperFilters (R/W)
            /// </summary>
            SPDRP_UPPERFILTERS = 0x00000011,

            /// <summary>
            /// LowerFilters (R/W)
            /// </summary>
            SPDRP_LOWERFILTERS = 0x00000012,

            /// <summary>
            /// BusTypeGUID (R)
            /// </summary>
            SPDRP_BUSTYPEGUID = 0x00000013,

            /// <summary>
            /// LegacyBusType (R)
            /// </summary>
            SPDRP_LEGACYBUSTYPE = 0x00000014,

            /// <summary>
            /// BusNumber (R)
            /// </summary>
            SPDRP_BUSNUMBER = 0x00000015,

            /// <summary>
            /// Enumerator Name (R)
            /// </summary>
            SPDRP_ENUMERATOR_NAME = 0x00000016,

            /// <summary>
            /// Security (R/W, binary form)
            /// </summary>
            SPDRP_SECURITY = 0x00000017,

            /// <summary>
            /// Security (W, SDS form)
            /// </summary>
            SPDRP_SECURITY_SDS = 0x00000018,

            /// <summary>
            /// Device Type (R/W)
            /// </summary>
            SPDRP_DEVTYPE = 0x00000019,

            /// <summary>
            /// Device is exclusive-access (R/W)
            /// </summary>
            SPDRP_EXCLUSIVE = 0x0000001A,

            /// <summary>
            /// Device Characteristics (R/W)
            /// </summary>
            SPDRP_CHARACTERISTICS = 0x0000001B,

            /// <summary>
            /// Device Address (R)
            /// </summary>
            SPDRP_ADDRESS = 0x0000001C,

            /// <summary>
            /// UiNumberDescFormat (R/W)
            /// </summary>
            SPDRP_UI_NUMBER_DESC_FORMAT = 0X0000001D,

            /// <summary>
            /// Device Power Data (R)
            /// </summary>
            SPDRP_DEVICE_POWER_DATA = 0x0000001E,

            /// <summary>
            /// Removal Policy (R)
            /// </summary>
            SPDRP_REMOVAL_POLICY = 0x0000001F,

            /// <summary>
            /// Hardware Removal Policy (R)
            /// </summary>
            SPDRP_REMOVAL_POLICY_HW_DEFAULT = 0x00000020,

            /// <summary>
            /// Removal Policy Override (RW)
            /// </summary>
            SPDRP_REMOVAL_POLICY_OVERRIDE = 0x00000021,

            /// <summary>
            /// Device Install State (R)
            /// </summary>
            SPDRP_INSTALL_STATE = 0x00000022,

            /// <summary>
            /// Device Location Paths (R)
            /// </summary>
            SPDRP_LOCATION_PATHS = 0x00000023,
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SP_CLASSINSTALL_HEADER
        {
            public UInt32 cbSize;
            public UInt32 InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SP_PROPCHANGE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader;
            public UInt32 StateChange;
            public UInt32 Scope;
            public UInt32 HwProfile;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVINFO_DATA
        {
            public UInt32 cbSize;
            public Guid classGuid;
            public UInt32 devInst;
            public IntPtr reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVICE_INTERFACE_DATA
        {
            public Int32 cbSize;
            public Guid interfaceClassGuid;
            public Int32 flags;
            private IntPtr reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DEVPROPKEY
        {
            public Guid fmtid;
            public UInt32 pid;
        }

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr SetupDiGetClassDevs(
            [In] ref Guid ClassGuid,
            [MarshalAs(UnmanagedType.LPWStr)] string Enumerator,
            IntPtr parent,
            UInt32 flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool SetupDiDestroyDeviceInfoList(IntPtr handle);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool SetupDiEnumDeviceInfo(
            IntPtr deviceInfoSet,
            UInt32 memberIndex,
            [Out] out SP_DEVINFO_DATA deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool SetupDiSetClassInstallParams(
            IntPtr deviceInfoSet,
            [In] ref SP_DEVINFO_DATA deviceInfoData,
            [In] ref SP_PROPCHANGE_PARAMS classInstallParams,
            UInt32 ClassInstallParamsSize);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool SetupDiChangeState(
            IntPtr deviceInfoSet,
            [In] ref SP_DEVINFO_DATA deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool SetupDiGetDeviceProperty(
                IntPtr deviceInfoSet,
                [In] ref SP_DEVINFO_DATA DeviceInfoData,
                [In] ref DEVPROPKEY propertyKey,
                [Out] out UInt32 propertyType,
                IntPtr propertyBuffer,
                UInt32 propertyBufferSize,
                out UInt32 requiredSize,
                UInt32 flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool SetupDiGetDeviceRegistryProperty(
          IntPtr DeviceInfoSet,
          [In] ref SP_DEVINFO_DATA DeviceInfoData,
          [MarshalAs(UnmanagedType.U4)] SPDRP Property,
          [Out] out UInt32 PropertyRegDataType,
          IntPtr PropertyBuffer,
          UInt32 PropertyBufferSize,
          [In, Out] ref UInt32 RequiredSize
        );

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern int CM_Get_Parent(
            out UInt32 pdnDevInst,
            UInt32 dnDevInst,
            int ulFlags
        );

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern int CM_Get_Device_ID_Size(out int pulLen, UInt32 dnDevInst, int flags = 0);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern int CM_Get_Device_ID(
           UInt32 dnDevInst,
           IntPtr buffer,
           int bufferLen,
           int flags = 0
        );
        #endregion

        public static void DisableDevice(Func<string, bool> filter, bool disable = true, bool toggle = false)
        {
            IntPtr info = IntPtr.Zero;
            Guid NullGuid = Guid.Empty;
            try
            {
                info = SetupDiGetClassDevs(
                    ref NullGuid,
                    null,
                    IntPtr.Zero,
                    DIGCF_ALLCLASSES);
                CheckError("SetupDiGetClassDevs");

                // Get first device matching device criterion.
                SP_DEVINFO_DATA devdata;
                for (uint i = 0; ; i++)
                {
                    devdata = new SP_DEVINFO_DATA();
                    devdata.cbSize = (UInt32)Marshal.SizeOf(devdata);

                    SetupDiEnumDeviceInfo(info, i, out devdata);
                    if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
                        throw new ApplicationException("No device matching that Hardware Id found.");
                    CheckError("SetupDiEnumDeviceInfo");

                    var hardwareIds = GetStringPropertyForDevice(info, devdata, propId: SPDRP.SPDRP_HARDWAREID);

                    if (hardwareIds != null && filter(hardwareIds))
                    {
                        try
                        {
                            SP_CLASSINSTALL_HEADER header = new SP_CLASSINSTALL_HEADER();
                            header.cbSize = (UInt32)Marshal.SizeOf(header);
                            header.InstallFunction = DIF_PROPERTYCHANGE;

                            SP_PROPCHANGE_PARAMS propchangeparams = new SP_PROPCHANGE_PARAMS();
                            propchangeparams.ClassInstallHeader = header;
                            propchangeparams.StateChange = disable ? DICS_DISABLE : DICS_ENABLE;
                            propchangeparams.Scope = DICS_FLAG_GLOBAL;
                            propchangeparams.HwProfile = 0;

                            SetupDiSetClassInstallParams(info,
                                ref devdata,
                                ref propchangeparams,
                                (UInt32)Marshal.SizeOf(propchangeparams));
                            CheckError("SetupDiSetClassInstallParams");

                            SetupDiChangeState(
                                info,
                                ref devdata);
                            CheckError("SetupDiChangeState");

                            if (toggle)
                            {
                                propchangeparams.StateChange = !disable ? DICS_DISABLE : DICS_ENABLE;

                                SetupDiSetClassInstallParams(info,
                                    ref devdata,
                                    ref propchangeparams,
                                    (UInt32)Marshal.SizeOf(propchangeparams));
                                CheckError("SetupDiSetClassInstallParams");

                                SetupDiChangeState(
                                    info,
                                    ref devdata);
                                CheckError("SetupDiChangeState");
                            }

                            break;
                        }
                        catch { }
                    }
                }
            }
            finally
            {
                if (info != IntPtr.Zero)
                    SetupDiDestroyDeviceInfoList(info);
            }
        }
        private static void CheckError(string message, int lasterror = -1)
        {

            int code = lasterror == -1 ? Marshal.GetLastWin32Error() : lasterror;
            if (code != 0)
                throw new ApplicationException(
                    String.Format("Error disabling hardware device (Code {0}): {1}",
                        code, message));
        }

        private static string GetStringPropertyForDevice(IntPtr info, SP_DEVINFO_DATA devdata, SPDRP? propId = null, DEVPROPKEY? propKey = null)
        {
            if ((propId == null) == (propKey == null))
                throw new ArgumentException("Must pass only one of propId and propKey.");

            uint proptype, outsize;
            IntPtr buffer = IntPtr.Zero;
            try
            {
                uint buflen = 512;
                buffer = Marshal.AllocHGlobal((int)buflen);
                outsize = 0;

                if (propId != null)
                {
                    SetupDiGetDeviceRegistryProperty(
                        info,
                        ref devdata,
                        propId.Value,
                        out proptype,
                        buffer,
                        buflen,
                        ref outsize);
                }
                if (propKey != null)
                {
                    var key = propKey.Value;

                    SetupDiGetDeviceProperty(
                        info,
                        ref devdata,
                        ref key,
                        out proptype,
                        buffer,
                        buflen,
                        out outsize,
                        0);
                }

                int errcode = Marshal.GetLastWin32Error();
                if (errcode == ERROR_INVALID_DATA || errcode == ERROR_NOT_FOUND)
                    return null;

                byte[] lbuffer = new byte[outsize];
                Marshal.Copy(buffer, lbuffer, 0, (int)outsize);

                CheckError("SetupDiGetDeviceProperty", errcode);
                return Encoding.Unicode.GetString(lbuffer);
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(buffer);
            }
        }

    }
}
