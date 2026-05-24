// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Tests.Services.Linux;

/// <summary>
/// Provides raw text content of mock Linux system files stored in the TestData/ subfolder.
/// The files are expected to be copied to the output directory (Copy to Output Directory = Copy if newer).
/// </summary>
public static class TestData
{
    private static readonly string BaseDir =
        Path.Combine(AppContext.BaseDirectory, "Services/Linux/TestData");

    /// <summary>
    /// /proc/cpuinfo
    /// </summary>
    public static string CpuInfo() => ReadFile("cpuinfo");

    /// <summary>
    /// /proc/stat
    /// </summary>
    public static string Stat1() => ReadFile("stat1");

    /// <summary>
    /// /proc/stat
    /// </summary>
    public static string Stat2() => ReadFile("stat2");

    /// <summary>
    /// /sys/class/powercap/intel-rapl:0/energy_uj
    /// </summary>
    public static string EnergyUj1() => ReadFile("energy_uj1");

    /// <summary>
    /// /sys/class/powercap/intel-rapl:0/energy_uj
    /// </summary>
    public static string EnergyUj2() => ReadFile("energy_uj2");

    /// <summary>
    /// /proc/cpuinfo for small two-logical-processor test fixtures
    /// </summary>
    public static string CpuInfo2Core() => ReadFile("cpuinfo-2core");

    /// <summary>
    /// /sys/class/hwmon/hwmon0 for an Intel coretemp sensor
    /// </summary>
    public static string HwmonIntelName() => ReadFile("hwmon/intel/hwmon0/name");
    public static string HwmonIntelTemp1Input() => ReadFile("hwmon/intel/hwmon0/temp1_input");
    public static string HwmonIntelTemp1Label() => ReadFile("hwmon/intel/hwmon0/temp1_label");
    public static string HwmonIntelTemp2Input() => ReadFile("hwmon/intel/hwmon0/temp2_input");
    public static string HwmonIntelTemp2Label() => ReadFile("hwmon/intel/hwmon0/temp2_label");
    public static string HwmonIntelTemp3Input() => ReadFile("hwmon/intel/hwmon0/temp3_input");
    public static string HwmonIntelTemp3Label() => ReadFile("hwmon/intel/hwmon0/temp3_label");

    /// <summary>
    /// /sys/class/hwmon/hwmon0 for an AMD k10temp sensor
    /// </summary>
    public static string HwmonAmdName() => ReadFile("hwmon/amd/hwmon0/name");
    public static string HwmonAmdTemp1Input() => ReadFile("hwmon/amd/hwmon0/temp1_input");
    public static string HwmonAmdTemp1Label() => ReadFile("hwmon/amd/hwmon0/temp1_label");
    public static string HwmonAmdTemp2Input() => ReadFile("hwmon/amd/hwmon0/temp2_input");
    public static string HwmonAmdTemp2Label() => ReadFile("hwmon/amd/hwmon0/temp2_label");
    public static string HwmonAmdTemp3Input() => ReadFile("hwmon/amd/hwmon0/temp3_input");
    public static string HwmonAmdTemp3Label() => ReadFile("hwmon/amd/hwmon0/temp3_label");

    /// <summary>
    /// /proc/meminfo
    /// </summary>
    public static string MemInfo() => ReadFile("meminfo");

    /// <summary>
    /// /proc/sys/kernel/osrelease
    /// </summary>
    public static string OsRelease() => ReadFile("osrelease");

    /// <summary>
    /// /proc/sys/kernel/hostname
    /// </summary>
    public static string Hostname() => ReadFile("hostname");

    /// <summary>
    /// /proc/uptime
    /// </summary>
    public static string Uptime() => ReadFile("uptime");

    /// <summary>
    /// /proc/loadavg
    /// </summary>
    public static string LoadAvg() => ReadFile("loadavg");

    /// <summary>
    /// df -T -B1 output
    /// </summary>
    public static string DfOutput() => ReadFile("df");

    /// <summary>
    /// /proc/diskstats first snapshot
    /// </summary>
    public static string DiskStats1() => ReadFile("diskstats1");

    /// <summary>
    /// /proc/diskstats second snapshot
    /// </summary>
    public static string DiskStats2() => ReadFile("diskstats2");

    /// <summary>
    /// /proc/net/dev first snapshot
    /// </summary>
    public static string NetDev1() => ReadFile("proc_net_dev1");

    /// <summary>
    /// /proc/net/dev second snapshot
    /// </summary>
    public static string NetDev2() => ReadFile("proc_net_dev2");

    /// <summary>
    /// /sys/class/net/eth0/operstate
    /// </summary>
    public static string OperstateEth0() => ReadFile("operstate_eth0");

    private static string ReadFile(string filename)
    {
        string path = Path.Combine(BaseDir, filename);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Test data file not found: {path}. Ensure it is set to 'Copy to Output Directory'.");
        return File.ReadAllText(path);
    }
}
