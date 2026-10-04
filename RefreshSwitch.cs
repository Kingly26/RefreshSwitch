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
    [DllImport("user32.dll")]
    public static extern int SetDisplayConfig(uint numPaths, IntPtr paths, uint numModes, IntPtr modes, uint flags);
    [DllImport("user32.dll")]
    public static extern bool DestroyIcon(IntPtr h);

    public const uint SDC_TOPOLOGY_EXTEND = 0x4, SDC_APPLY = 0x80;

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
            if ((dd.StateFlags & Native.ATTACHED) == 0) continue;
            var m = new Monitor { Device = dd.DeviceName, Primary = (dd.StateFlags & Native.PRIMARY) != 0 };
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
            m.Name = (string.IsNullOrEmpty(friendly) ? baseName : friendly) + " [" + num.Replace("DISPLAY", Lang.T("Schermo ", "Display ")) + "]";
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

    // detaches the monitor from the desktop (like unplugging it): windows move to the other screens
    public bool Disable()
    {
        var dm = NewMode();
        if (!Native.EnumDisplaySettings(Device, Native.ENUM_CURRENT_SETTINGS, ref dm)) return false;
        dm.dmFields = Native.DM_POSITION | Native.DM_PELSWIDTH | Native.DM_PELSHEIGHT;
        dm.dmPelsWidth = 0; dm.dmPelsHeight = 0;
        int r = Native.ChangeDisplaySettingsEx(Device, ref dm, IntPtr.Zero, Native.CDS_UPDATEREGISTRY | Native.CDS_NORESET, IntPtr.Zero);
        if (r != 0) return false;
        return Native.ChangeDisplaySettingsEx(null, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero) == 0;
    }

    // same as "Extend these displays" in Windows settings: brings back every connected monitor
    // with the layout Windows remembers for this set of screens
    public static bool ExtendAll()
    {
        return Native.SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, Native.SDC_TOPOLOGY_EXTEND | Native.SDC_APPLY) == 0;
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

static class Lang
{
    static readonly bool It = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "it";
    public static string T(string it, string en) { return It ? it : en; }
}

static class Settings
{
    const string Key = @"Software\RefreshSwitch";

    public static bool ClickCycle
    {
        get
        {
            try { using (var k = Registry.CurrentUser.OpenSubKey(Key)) return k != null && Convert.ToInt32(k.GetValue("ClickCycle", 0)) != 0; }
            catch { return false; }
        }
        set
        {
            try { using (var k = Registry.CurrentUser.CreateSubKey(Key)) k.SetValue("ClickCycle", value ? 1 : 0, RegistryValueKind.DWord); }
            catch { }
        }
    }

    public static int IconStyle
    {
        get
        {
            try
            {
                using (var k = Registry.CurrentUser.OpenSubKey(Key))
                {
                    int s = k == null ? 0 : Convert.ToInt32(k.GetValue("IconStyle", 0));
                    return s < 0 || s >= Icons.Count ? 0 : s;
                }
            }
            catch { return 0; }
        }
        set
        {
            try { using (var k = Registry.CurrentUser.CreateSubKey(Key)) k.SetValue("IconStyle", value, RegistryValueKind.DWord); }
            catch { }
        }
    }
}

static class Icons
{
    public const int Count = 10, FirstGothic = 7;
    static readonly Color Solid = Color.FromArgb(30, 100, 190), Bright = Color.FromArgb(86, 180, 255);

    public static string Name(int style)
    {
        switch (style)
        {
            case 0: return Lang.T("Cerchio pieno", "Filled circle");
            case 1: return Lang.T("Quadrato arrotondato", "Rounded square");
            case 2: return Lang.T("Riquadro (senza sfondo)", "Outline box (no background)");
            case 3: return Lang.T("Numero colorato (senza sfondo)", "Colored number (no background)");
            case 4: return Lang.T("Numero monocromatico (senza sfondo)", "Monochrome number (no background)");
            case 5: return Lang.T("Numero con Hz (senza sfondo)", "Number with Hz (no background)");
            case 6: return Lang.T("Colore in base agli Hz (senza sfondo)", "Color by refresh rate (no background)");
            case 7: return Lang.T("Gotico", "Gothic");
            case 8: return Lang.T("Gotico con spine", "Gothic with thorns");
            default: return Lang.T("Gotico rosso", "Gothic red");
        }
    }

    public static bool LightTaskbar()
    {
        try
        {
            using (var k = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                return k != null && Convert.ToInt32(k.GetValue("SystemUsesLightTheme", 0)) != 0;
        }
        catch { return false; }
    }

    // green from 120 Hz, yellow from 75 Hz, orange below
    static Color RateColor(uint hz, bool light)
    {
        if (hz >= 120) return light ? Color.FromArgb(30, 140, 70) : Color.FromArgb(80, 220, 130);
        if (hz >= 75) return light ? Color.FromArgb(160, 120, 0) : Color.FromArgb(250, 210, 70);
        return light ? Color.FromArgb(200, 90, 20) : Color.FromArgb(255, 150, 70);
    }

    // draws s centered in the box, with the largest font (up to maxPx) that fits its width
    static void Text(Graphics g, string s, RectangleF box, float maxPx, Color c)
    {
        using (var sf = new StringFormat(StringFormat.GenericTypographic))
        {
            sf.Alignment = StringAlignment.Center; sf.LineAlignment = StringAlignment.Center;
            float px = maxPx;
            using (var probe = new Font("Segoe UI", 100, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                float w = g.MeasureString(s, probe, 1000, sf).Width;
                if (w > 0) px = Math.Min(maxPx, box.Width * 100f / w);
            }
            using (var f = new Font("Segoe UI", px, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var b = new SolidBrush(c))
                g.DrawString(s, f, b, box, sf);
        }
    }

    static GraphicsPath RoundRect(float x, float y, float w, float h, float r)
    {
        var p = new GraphicsPath();
        p.AddArc(x, y, r, r, 180, 90); p.AddArc(x + w - r, y, r, r, 270, 90);
        p.AddArc(x + w - r, y + h - r, r, r, 0, 90); p.AddArc(x, y + h - r, r, r, 90, 90);
        p.CloseFigure();
        return p;
    }

    public static Bitmap Render(int style, uint hz, bool light)
    {
        var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            string s = hz.ToString();
            Color accent = light ? Solid : Bright, mono = light ? Color.FromArgb(30, 30, 30) : Color.White;
            var full = new RectangleF(0, 0, 32, 32);
            switch (style)
            {
                case 0:
                    using (var b = new SolidBrush(Solid)) g.FillEllipse(b, 0, 0, 32, 32);
                    Text(g, s, new RectangleF(4, 0, 24, 32), 18, Color.White);
                    break;
                case 1:
                    using (var b = new SolidBrush(Solid)) using (var p = RoundRect(0, 3, 32, 26, 10)) g.FillPath(b, p);
                    Text(g, s, new RectangleF(2, 0, 28, 32), 20, Color.White);
                    break;
                case 2:
                    using (var pen = new Pen(accent, 2f)) using (var p = RoundRect(1, 4, 30, 24, 9)) g.DrawPath(pen, p);
                    Text(g, s, new RectangleF(4, 0, 24, 32), 18, accent);
                    break;
                case 3: Text(g, s, full, 28, accent); break;
                case 4: Text(g, s, full, 28, mono); break;
                case 5:
                    Text(g, s, new RectangleF(0, -1, 32, 22), 22, mono);
                    Text(g, "Hz", new RectangleF(0, 18, 32, 14), 13, accent);
                    break;
                case 6: Text(g, s, full, 28, RateColor(hz, light)); break;
                case 7: Gothic.Draw(g, s, full, mono, false); break;
                case 8: Gothic.Draw(g, s, full, mono, true); break;
                default: Gothic.Draw(g, s, full, light ? Color.FromArgb(190, 25, 35) : Color.FromArgb(225, 40, 45), true); break;
            }
        }
        return bmp;
    }

    // contact sheet of every style at a few refresh rates, on a dark and a light taskbar
    public static void SavePreview(string path)
    {
        const int cell = 56, labelW = 310, pad = 12;
        uint[] rates = { 60, 75, 144, 240 };
        int w = labelW + cell * 8 + pad * 3, h = pad * 2 + 28 + cell * Count;
        using (var bmp = new Bitmap(w, h))
        using (var g = Graphics.FromImage(bmp))
        using (var f = new Font("Segoe UI", 15, FontStyle.Regular, GraphicsUnit.Pixel))
        {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.Clear(Color.FromArgb(250, 250, 250));
            int xDark = labelW + pad, xLight = xDark + cell * 4 + pad, y0 = pad + 28;
            using (var d = new SolidBrush(Color.FromArgb(32, 32, 32))) g.FillRectangle(d, xDark, y0, cell * 4, cell * Count);
            using (var l = new SolidBrush(Color.FromArgb(238, 238, 238))) g.FillRectangle(l, xLight, y0, cell * 4, cell * Count);
            for (int c = 0; c < 4; c++)
            {
                g.DrawString(rates[c] + " Hz", f, Brushes.Black, xDark + c * cell + 2, pad);
                g.DrawString(rates[c] + " Hz", f, Brushes.Black, xLight + c * cell + 2, pad);
            }
            for (int s = 0; s < Count; s++)
            {
                int y = y0 + s * cell;
                g.DrawString((s + 1) + ". " + Name(s), f, Brushes.Black, pad, y + 18);
                for (int c = 0; c < 4; c++)
                {
                    using (var a = Render(s, rates[c], false)) g.DrawImageUnscaled(a, xDark + c * cell + 12, y + 12);
                    using (var b = Render(s, rates[c], true)) g.DrawImageUnscaled(b, xLight + c * cell + 12, y + 12);
                }
            }
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}

class App : ApplicationContext
{
    NotifyIcon tray = new NotifyIcon();
    Timer timer = new Timer();
    IntPtr iconHandle = IntPtr.Zero;
    string iconKey = "";
    uint shownHz = 0;
    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public App()
    {
        tray.Visible = true;
        tray.ContextMenuStrip = new ContextMenuStrip();
        tray.ContextMenuStrip.Opening += (s, e) =>
        {
            // an empty menu starts with Cancel = true, which would swallow the first click
            e.Cancel = false;
            try { BuildMenu(tray.ContextMenuStrip); }
            catch (Exception ex)
            {
                Log(ex);
                tray.ContextMenuStrip.Items.Clear();
                tray.ContextMenuStrip.Items.Add(Lang.T("Errore: ", "Error: ") + ex.Message);
                tray.ContextMenuStrip.Items.Add(Lang.T("Esci", "Exit"), null, (s2, e2) => { tray.Visible = false; Application.Exit(); });
            }
        };
        tray.MouseClick += (s, e) =>
        {
            if (e.Button != MouseButtons.Left) return;
            if (Settings.ClickCycle) Cycle();
            else ShowMenu();
        };
        timer.Interval = 4000;
        timer.Tick += (s, e) => Refresh();
        timer.Start();
        Refresh();
    }

    // opens the tray menu the same way a right-click does
    void ShowMenu()
    {
        var mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (mi != null) mi.Invoke(tray, null);
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
        if (!ok) tray.ShowBalloonTip(3000, "RefreshSwitch", Lang.T("Alcuni monitor non hanno accettato la frequenza.", "Some monitors did not accept the refresh rate."), ToolTipIcon.Warning);
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
            shownHz = shown;
            UpdateIcon();
            tray.Text = (tip.Length > 63 ? tip.Substring(0, 63) : tip);
        }
        catch { }
    }

    void UpdateIcon()
    {
        int style = Settings.IconStyle;
        bool light = Icons.LightTaskbar();
        string key = style + "/" + shownHz + "/" + light;
        if (key == iconKey) return;
        IntPtr h;
        using (var bmp = Icons.Render(style, shownHz, light)) h = bmp.GetHicon();
        tray.Icon = Icon.FromHandle(h);
        if (iconHandle != IntPtr.Zero) Native.DestroyIcon(iconHandle);
        iconHandle = h;
        iconKey = key;
    }

    void BuildMenu(ContextMenuStrip m)
    {
        m.Items.Clear();
        var mons = Monitor.All();
        var all = Union(mons);
        m.Items.Add(Lang.T("Cambia frequenza (tutti)", "Change refresh rate (all)"), null, (s, e) => Cycle());
        m.Items.Add(new ToolStripSeparator());
        var allMenu = new ToolStripMenuItem(Lang.T("Tutti i monitor", "All monitors"));
        foreach (var r in all) { uint rr = r; allMenu.DropDownItems.Add(rr + " Hz", null, (s, e) => SetAll(rr)); }
        m.Items.Add(allMenu);
        foreach (var mon in mons)
        {
            var mm = mon;
            if (!mm.Active) continue;
            var sub = new ToolStripMenuItem(mm.Name + (mm.Primary ? Lang.T(" (principale)", " (primary)") : "") + " - " + mm.Current + " Hz");
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
        m.Items.Add(Lang.T("Spegni schermi (standby, il mouse li riaccende)", "Turn screens off (standby, mouse wakes them)"), null, (s, e) => StandbyAll());
        int activeCount = mons.FindAll(x => x.Active).Count;
        foreach (var mon in mons)
        {
            var mm = mon;
            var off = m.Items.Add(Lang.T("Disattiva ", "Disable ") + mm.Name + Lang.T(" (come scollegarlo)", " (like unplugging it)"), null, (s, e) => Disable(mm));
            off.Enabled = activeCount > 1;
        }
        m.Items.Add(Lang.T("Riattiva tutti i monitor (Estendi)", "Re-enable all monitors (Extend)"), null, (s, e) => EnableAll());
        m.Items.Add(new ToolStripSeparator());
        var click = new ToolStripMenuItem(Lang.T("Cambia frequenza con un clic sull'icona", "Change refresh rate by clicking the icon"));
        click.Checked = Settings.ClickCycle;
        click.Click += (s, e) => { Settings.ClickCycle = !Settings.ClickCycle; };
        m.Items.Add(click);
        var styles = new ToolStripMenuItem(Lang.T("Stile icona", "Icon style"));
        int current = Settings.IconStyle;
        for (int i = 0; i < Icons.Count; i++)
        {
            int style = i;
            if (style == Icons.FirstGothic) styles.DropDownItems.Add(new ToolStripSeparator());
            var it = new ToolStripMenuItem(Icons.Name(style), Icons.Render(style, shownHz, true));
            it.Checked = style == current;
            it.Click += (s, e) => { Settings.IconStyle = style; UpdateIcon(); };
            styles.DropDownItems.Add(it);
        }
        m.Items.Add(styles);
        var auto = new ToolStripMenuItem(Lang.T("Avvia con Windows", "Start with Windows"));
        auto.Checked = AutostartOn();
        auto.Click += (s, e) => SetAutostart(!AutostartOn());
        m.Items.Add(auto);
        m.Items.Add(Lang.T("Esci", "Exit"), null, (s, e) => { tray.Visible = false; Application.Exit(); });
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

    void Disable(Monitor m)
    {
        bool ok = false;
        try { ok = m.Disable(); }
        catch (Exception ex) { Log(ex); }
        if (!ok) tray.ShowBalloonTip(3000, "RefreshSwitch", Lang.T("Operazione non riuscita su ", "Operation failed on ") + m.Name + ".", ToolTipIcon.Warning);
        Refresh();
    }

    void EnableAll()
    {
        bool ok = false;
        try { ok = Monitor.ExtendAll(); }
        catch (Exception ex) { Log(ex); }
        if (!ok) tray.ShowBalloonTip(3000, "RefreshSwitch", Lang.T("Non sono riuscito a riattivare i monitor.", "Could not re-enable the monitors."), ToolTipIcon.Warning);
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
    static void Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--preview") { Icons.SavePreview(args[1]); return; }
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
