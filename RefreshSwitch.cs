using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

static class Native
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmDeviceName;
        public ushort dmSpecVersion, dmDriverVersion, dmSize, dmDriverExtra;
        public uint dmFields;
        public int dmPositionX, dmPositionY;
        public uint dmDisplayOrientation, dmDisplayFixedOutput;
        public short dmColor, dmDuplex, dmYResolution, dmTTOption, dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmFormName;
        public ushort dmLogPixels;
        public uint dmBitsPerPel, dmPelsWidth, dmPelsHeight, dmDisplayFlags, dmDisplayFrequency;
        public uint dmICMMethod, dmICMIntent, dmMediaType, dmDitherType, dmReserved1, dmReserved2, dmPanningWidth, dmPanningHeight;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAY_DEVICE
    {
        public int cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceString;
        public int StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceKey;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern bool EnumDisplayDevices(string device, uint devNum, ref DISPLAY_DEVICE dd, uint flags);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern bool EnumDisplaySettings(string device, int mode, ref DEVMODE dm);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int ChangeDisplaySettingsEx(string device, ref DEVMODE dm, IntPtr hwnd, uint flags, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int ChangeDisplaySettingsEx(string device, IntPtr dm, IntPtr hwnd, uint flags, IntPtr lParam);
    [DllImport("user32.dll")]
    public static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

    public const uint DM_POSITION = 0x20, DM_BITSPERPEL = 0x40000, DM_PELSWIDTH = 0x80000, DM_PELSHEIGHT = 0x100000;
    public const uint CDS_NORESET = 0x10000000;
    public const int ENUM_CURRENT_SETTINGS = -1;
    public const uint DM_DISPLAYFREQUENCY = 0x400000;
    public const uint CDS_UPDATEREGISTRY = 1;
    public const int ATTACHED = 1, PRIMARY = 4;
}

class Monitor
{
    public string Device, Name;
    public bool Primary, Active = true;
    public uint Current;
    public List<uint> Rates = new List<uint>();

    static Native.DEVMODE NewMode()
    {
        var dm = new Native.DEVMODE();
        dm.dmSize = (ushort)Marshal.SizeOf(typeof(Native.DEVMODE));
        return dm;
    }

    // real monitor models (e.g. "DELL U2723QE") keyed by PnP code, from WMI; empty if WMI fails
    static Dictionary<string, string> FriendlyNames()
    {
        var d = new Dictionary<string, string>();
        try
        {
            using (var q = new System.Management.ManagementObjectSearcher("root\\wmi", "SELECT InstanceName, UserFriendlyName, ManufacturerName FROM WmiMonitorID"))
                foreach (System.Management.ManagementObject o in q.Get())
                {
                    string inst = (o["InstanceName"] ?? "").ToString();
                    var parts = inst.Split('\\');
                    if (parts.Length < 2) continue;
                    string model = Decode(o["UserFriendlyName"] as ushort[]);
                    if (model.Length == 0) model = Decode(o["ManufacturerName"] as ushort[]);
                    if (model.Length > 0) d[parts[1].ToUpperInvariant()] = model;
                }
        }
        catch { }
        return d;
    }

    static string Decode(ushort[] a)
    {
        if (a == null) return "";
        var sb = new System.Text.StringBuilder();
        foreach (var c in a) { if (c == 0) break; sb.Append((char)c); }
        return sb.ToString().Trim();
    }

    public static List<Monitor> All()
    {
        var list = new List<Monitor>();
        var names = FriendlyNames();
        for (uint i = 0; ; i++)
        {
            var dd = new Native.DISPLAY_DEVICE(); dd.cb = Marshal.SizeOf(dd);
            if (!Native.EnumDisplayDevices(null, i, ref dd, 0)) break;
            bool attached = (dd.StateFlags & Native.ATTACHED) != 0;
            var m = new Monitor { Device = dd.DeviceName, Primary = (dd.StateFlags & Native.PRIMARY) != 0, Active = attached };
            var mon = new Native.DISPLAY_DEVICE(); mon.cb = Marshal.SizeOf(mon);
            bool hasMon = Native.EnumDisplayDevices(dd.DeviceName, 0, ref mon, 0);
            string baseName = hasMon ? mon.DeviceString : dd.DeviceString;
            string friendly = null;
            if (hasMon && mon.DeviceID != null)
            {
                var parts = mon.DeviceID.Split('\\');
                if (parts.Length > 1) names.TryGetValue(parts[1].ToUpperInvariant(), out friendly);
            }
            string num = dd.DeviceName.StartsWith("\\\\.\\") ? dd.DeviceName.Substring(4) : dd.DeviceName;
            m.Name = (string.IsNullOrEmpty(friendly) ? baseName : friendly) + " [" + num.Replace("DISPLAY", "Schermo ") + "]";
            if (!attached)
            {
                // detached output: list it only if a monitor is still connected or we saved its mode
                if (hasMon || LoadSaved(m.Device) != null) list.Add(m);
                continue;
            }
            var cur = NewMode();
            if (!Native.EnumDisplaySettings(m.Device, Native.ENUM_CURRENT_SETTINGS, ref cur)) continue;
            m.Current = cur.dmDisplayFrequency;
            var dm = NewMode();
            for (int k = 0; Native.EnumDisplaySettings(m.Device, k, ref dm); k++)
                if (dm.dmPelsWidth == cur.dmPelsWidth && dm.dmPelsHeight == cur.dmPelsHeight
                    && dm.dmBitsPerPel == cur.dmBitsPerPel && dm.dmDisplayFrequency > 1
                    && !m.Rates.Contains(dm.dmDisplayFrequency))
                    m.Rates.Add(dm.dmDisplayFrequency);
            m.Rates.Sort();
            list.Add(m);
        }
        return list;
    }

    const string SaveKey = @"Software\RefreshSwitch";

    static string KeyName(string device) { return device.Replace("\\", "_").Replace(".", ""); }

    static uint[] LoadSaved(string device)
    {
        using (var k = Registry.CurrentUser.OpenSubKey(SaveKey))
        {
            if (k == null) return null;
            var v = k.GetValue(KeyName(device)) as string;
            if (v == null) return null;
            var p = v.Split(',');
            if (p.Length != 6) return null;
            var r = new uint[6];
            for (int i = 0; i < 6; i++) r[i] = unchecked((uint)int.Parse(p[i]));
            return r;
        }
    }

    // detaches the monitor from the desktop (like unplugging it): windows move to the other screens
    public bool Disable()
    {
        var dm = NewMode();
        if (!Native.EnumDisplaySettings(Device, Native.ENUM_CURRENT_SETTINGS, ref dm)) return false;
        using (var k = Registry.CurrentUser.CreateSubKey(SaveKey))
            k.SetValue(KeyName(Device), string.Format("{0},{1},{2},{3},{4},{5}",
                dm.dmPositionX, dm.dmPositionY, dm.dmPelsWidth, dm.dmPelsHeight, dm.dmBitsPerPel, dm.dmDisplayFrequency));
        dm.dmFields = Native.DM_POSITION | Native.DM_PELSWIDTH | Native.DM_PELSHEIGHT;
        dm.dmPelsWidth = 0; dm.dmPelsHeight = 0;
        int r = Native.ChangeDisplaySettingsEx(Device, ref dm, IntPtr.Zero, Native.CDS_UPDATEREGISTRY | Native.CDS_NORESET, IntPtr.Zero);
        if (r != 0) return false;
        return Native.ChangeDisplaySettingsEx(null, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero) == 0;
    }

    public bool Enable()
    {
        var s = LoadSaved(Device);
        var dm = NewMode();
        if (s != null)
        {
            dm.dmPositionX = unchecked((int)s[0]); dm.dmPositionY = unchecked((int)s[1]);
            dm.dmPelsWidth = s[2]; dm.dmPelsHeight = s[3]; dm.dmBitsPerPel = s[4]; dm.dmDisplayFrequency = s[5];
        }
        else if (!Native.EnumDisplaySettings(Device, -2, ref dm)) return false; // ENUM_REGISTRY_SETTINGS
        dm.dmFields = Native.DM_POSITION | Native.DM_PELSWIDTH | Native.DM_PELSHEIGHT | Native.DM_BITSPERPEL | Native.DM_DISPLAYFREQUENCY;
        int r = Native.ChangeDisplaySettingsEx(Device, ref dm, IntPtr.Zero, Native.CDS_UPDATEREGISTRY | Native.CDS_NORESET, IntPtr.Zero);
        if (r != 0) return false;
        return Native.ChangeDisplaySettingsEx(null, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero) == 0;
    }

    // picks the requested rate, or the closest one this monitor supports
    public bool Apply(uint rate)
    {
        if (Rates.Count == 0) return false;
        uint best = Rates[0];
        foreach (var r in Rates) if (Math.Abs((int)r - (int)rate) < Math.Abs((int)best - (int)rate)) best = r;
        if (best == Current) return true;
        var dm = NewMode();
        if (!Native.EnumDisplaySettings(Device, Native.ENUM_CURRENT_SETTINGS, ref dm)) return false;
        dm.dmDisplayFrequency = best;
        dm.dmFields |= Native.DM_DISPLAYFREQUENCY;
        return Native.ChangeDisplaySettingsEx(Device, ref dm, IntPtr.Zero, Native.CDS_UPDATEREGISTRY, IntPtr.Zero) == 0;
    }
}

class App : ApplicationContext
{
    NotifyIcon tray = new NotifyIcon();
    Timer timer = new Timer();
    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public App()
    {
        tray.Visible = true;
        tray.ContextMenuStrip = new ContextMenuStrip();
        tray.ContextMenuStrip.Opening += (s, e) =>
        {
            try { BuildMenu(tray.ContextMenuStrip); }
            catch (Exception ex)
            {
                Log(ex);
                tray.ContextMenuStrip.Items.Clear();
                tray.ContextMenuStrip.Items.Add("Errore: " + ex.Message);
                tray.ContextMenuStrip.Items.Add("Esci", null, (s2, e2) => { tray.Visible = false; Application.Exit(); });
            }
        };
        tray.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) Cycle(); };
        timer.Interval = 4000;
        timer.Tick += (s, e) => Refresh();
        timer.Start();
        Refresh();
    }

    static List<uint> Union(List<Monitor> mons)
    {
        var u = new List<uint>();
        foreach (var m in mons) if (m.Active) foreach (var r in m.Rates) if (!u.Contains(r)) u.Add(r);
        u.Sort();
        return u;
    }

    void SetAll(uint rate)
    {
        bool ok = true;
        foreach (var m in Monitor.All()) if (m.Active) ok &= m.Apply(rate);
        if (!ok) tray.ShowBalloonTip(3000, "RefreshSwitch", "Alcuni monitor non hanno accettato la frequenza.", ToolTipIcon.Warning);
        Refresh();
    }

    void Cycle()
    {
        var mons = Monitor.All();
        var rates = Union(mons);
        if (rates.Count == 0) return;
        uint cur = 0;
        foreach (var m in mons) if (m.Active && m.Primary) cur = m.Current;
        uint next = rates[0];
        foreach (var r in rates) if (r > cur) { next = r; break; }
        SetAll(next);
    }

    void Refresh()
    {
        try
        {
            var mons = Monitor.All();
            uint shown = 0; string tip = "";
            foreach (var m in mons)
            {
                if (!m.Active) continue;
                if (m.Primary || shown == 0) shown = m.Current;
                tip += (tip.Length > 0 ? "\n" : "") + m.Name + ": " + m.Current + " Hz";
            }
            var old = tray.Icon;
            tray.Icon = MakeIcon(shown.ToString());
            if (old != null) old.Dispose();
            tray.Text = (tip.Length > 63 ? tip.Substring(0, 63) : tip);
        }
        catch { }
    }

    static Icon MakeIcon(string text)
    {
        using (var bmp = new Bitmap(32, 32))
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using (var b = new SolidBrush(Color.FromArgb(30, 100, 190))) g.FillEllipse(b, 1, 1, 30, 30);
            float size = text.Length >= 3 ? 14 : 18;
            using (var f = new Font("Segoe UI", size, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                var sz = g.MeasureString(text, f);
                g.DrawString(text, f, Brushes.White, (32 - sz.Width) / 2 + 0.5f, (32 - sz.Height) / 2 + 1);
            }
            return Icon.FromHandle(bmp.GetHicon());
        }
    }

    void BuildMenu(ContextMenuStrip m)
    {
        m.Items.Clear();
        var mons = Monitor.All();
        var all = Union(mons);
        m.Items.Add("Cambia frequenza (tutti)", null, (s, e) => Cycle());
        m.Items.Add(new ToolStripSeparator());
        var allMenu = new ToolStripMenuItem("Tutti i monitor");
        foreach (var r in all) { uint rr = r; allMenu.DropDownItems.Add(rr + " Hz", null, (s, e) => SetAll(rr)); }
        m.Items.Add(allMenu);
        foreach (var mon in mons)
        {
            var mm = mon;
            if (!mm.Active) continue;
            var sub = new ToolStripMenuItem(mm.Name + (mm.Primary ? " (principale)" : "") + " - " + mm.Current + " Hz");
            foreach (var r in mm.Rates)
            {
                uint rr = r;
                var it = new ToolStripMenuItem(rr + " Hz");
                it.Checked = rr == mm.Current;
                it.Click += (s, e) => { mm.Apply(rr); Refresh(); };
                sub.DropDownItems.Add(it);
            }
            m.Items.Add(sub);
        }
        m.Items.Add(new ToolStripSeparator());
        m.Items.Add("Spegni schermi (standby, il mouse li riaccende)", null, (s, e) => StandbyAll());
        int activeCount = mons.FindAll(x => x.Active).Count;
        foreach (var mon in mons)
        {
            var mm = mon;
            if (mm.Active)
            {
                var off = m.Items.Add("Disattiva " + mm.Name + " (come scollegarlo)", null, (s, e) => Toggle(mm, false));
                off.Enabled = activeCount > 1;
            }
            else m.Items.Add("Riattiva " + mm.Name, null, (s, e) => Toggle(mm, true));
        }
        m.Items.Add(new ToolStripSeparator());
        var auto = new ToolStripMenuItem("Avvia con Windows");
        auto.Checked = AutostartOn();
        auto.Click += (s, e) => SetAutostart(!AutostartOn());
        m.Items.Add(auto);
        m.Items.Add("Esci", null, (s, e) => { tray.Visible = false; Application.Exit(); });
    }

    public static void Log(Exception ex)
    {
        try
        {
            string dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RefreshSwitch");
            System.IO.Directory.CreateDirectory(dir);
            System.IO.File.AppendAllText(System.IO.Path.Combine(dir, "error.log"), DateTime.Now + "\r\n" + ex + "\r\n\r\n");
        }
        catch { }
    }

    void StandbyAll()
    {
        // short pause so the click itself does not wake the screens again
        var t = new Timer { Interval = 600 };
        t.Tick += (s, e) =>
        {
            t.Stop(); t.Dispose();
            Native.PostMessage((IntPtr)0xFFFF, 0x0112, (IntPtr)0xF170, (IntPtr)2);
        };
        t.Start();
    }

    void Toggle(Monitor m, bool enable)
    {
        bool ok = false;
        try { ok = enable ? m.Enable() : m.Disable(); }
        catch (Exception ex) { Log(ex); }
        if (!ok) tray.ShowBalloonTip(3000, "RefreshSwitch", "Operazione non riuscita su " + m.Name + ".", ToolTipIcon.Warning);
        Refresh();
    }

    static bool AutostartOn()
    {
        using (var k = Registry.CurrentUser.OpenSubKey(RunKey)) return k != null && k.GetValue("RefreshSwitch") != null;
    }
    static void SetAutostart(bool on)
    {
        using (var k = Registry.CurrentUser.OpenSubKey(RunKey, true))
        {
            if (on) k.SetValue("RefreshSwitch", "\"" + Application.ExecutablePath + "\"");
            else k.DeleteValue("RefreshSwitch", false);
        }
    }
}

static class Program
{
    [STAThread]
    static void Main()
    {
        bool created;
        using (var mtx = new System.Threading.Mutex(true, "RefreshSwitchSingleton", out created))
        {
            if (!created) return;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => App.Log(e.Exception);
            Application.EnableVisualStyles();
            Application.Run(new App());
        }
    }
}
