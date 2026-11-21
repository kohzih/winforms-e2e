$ErrorActionPreference = 'Stop'

Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;

public static class DisplaySettings
{
    public const int CurrentSettings = -1;

    // DEVMODEW uses a union. These fields describe its display branch.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DisplayMode
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DeviceName;
        public ushort SpecVersion, DriverVersion, Size, DriverExtra;
        public uint Fields;
        public int PositionX, PositionY;
        public uint DisplayOrientation, DisplayFixedOutput;
        public short Color, Duplex, YResolution, TTOption, Collate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string FormName;
        public ushort LogPixels;
        public uint BitsPerPel, Width, Height, DisplayFlags, DisplayFrequency;
        public uint ICMMethod, ICMIntent, MediaType, DitherType, Reserved1, Reserved2;
        public uint PanningWidth, PanningHeight;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "EnumDisplaySettingsW", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplaySettings(string deviceName, int modeIndex, ref DisplayMode mode);

    [DllImport("user32.dll", EntryPoint = "ChangeDisplaySettingsW", ExactSpelling = true)]
    public static extern int ChangeDisplaySettings(ref DisplayMode mode, uint flags);

    public static DisplayMode? Read(int modeIndex)
    {
        var mode = new DisplayMode { Size = (ushort)Marshal.SizeOf(typeof(DisplayMode)) };
        return EnumDisplaySettings(null, modeIndex, ref mode) ? mode : (DisplayMode?)null;
    }
}
'@

$current = [DisplaySettings]::Read([DisplaySettings]::CurrentSettings)
if (-not $current) { throw 'Could not read the primary display mode.' }
Write-Host "Primary display before change: $($current.Width) x $($current.Height) pixels"

if ($current.Width -ne 1920 -or $current.Height -ne 1080) {
    $modes = for ($index = 0; $mode = [DisplaySettings]::Read($index); $index++) { $mode }
    $availableSizes = ($modes | ForEach-Object { "$($_.Width)x$($_.Height)" } | Sort-Object -Unique) -join ', '
    Write-Host "Available display resolutions: $availableSizes"

    $fullHd = $modes | Where-Object { $_.Width -eq 1920 -and $_.Height -eq 1080 -and $_.BitsPerPel -eq $current.BitsPerPel } |
        Sort-Object -Property DisplayFrequency -Descending | Select-Object -First 1
    if (-not $fullHd) { throw 'The primary display driver does not report a 1920 x 1080 mode at the current color depth.' }

    # Change the current session's mode without writing display settings to the registry.
    $result = [DisplaySettings]::ChangeDisplaySettings([ref]$fullHd, 0)
    if ($result -ne 0) { throw "ChangeDisplaySettingsW returned $result while applying 1920 x 1080." }
}

$actual = [DisplaySettings]::Read([DisplaySettings]::CurrentSettings)
if ($actual.Width -ne 1920 -or $actual.Height -ne 1080) {
    throw "Expected 1920 x 1080, got $($actual.Width) x $($actual.Height)."
}
Write-Host "Primary display verified: $($actual.Width) x $($actual.Height) pixels"
