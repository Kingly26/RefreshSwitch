# RefreshSwitch

English | [Italiano](README.it.md)

A Windows tray app to change the refresh rate (Hz) of all your monitors, and to turn monitors off without unplugging them.

- **Change refresh rate by clicking the icon** (off by default): when enabled in the menu, a left-click switches to the next refresh rate on all monitors; the menu stays available with a right-click
- **Click** the tray icon (left or right) to open the menu: pick a rate for all monitors or for a single monitor, turn screens off, disable/re-enable a monitor, Start with Windows, Exit
- The icon shows the Hz of the primary monitor; the tooltip lists all monitors
- Monitors are shown with their real model name, plus the display number
- No administrator rights required
- The UI language follows Windows (Italian or English)

If a monitor does not support the requested rate, the closest one available at its current resolution is used.

## Who it is for

I recommend RefreshSwitch to anyone who needs to disconnect a monitor as if the cable were physically unplugged, without touching it.

One example is watching Netflix in 4K from Microsoft Edge (a browser extension is required): a second screen attached to the PC can get in the way, and the usual fix is to unplug it. With **Disable [monitor]** the screen leaves the desktop the same way, and **Re-enable all monitors** brings it back when you are done.

It is also handy if you often change refresh rate, for example a high rate for games and a lower one to save power or to match a video.

## Turning monitors off

- **Turn screens off**: puts all monitors in standby; moving the mouse or pressing a key wakes them.
- **Disable [monitor]**: detaches the monitor from the desktop, like unplugging the cable (windows move to the other screens). The last active monitor cannot be disabled.
- **Re-enable all monitors (Extend)**: does the same as "Extend these displays" in Windows settings, so every connected monitor comes back with the layout Windows remembers.

## Icon styles

Ten styles, selectable from the menu, shown here on a dark and a light taskbar. The monochrome ones follow the Windows theme. The gothic numbers are drawn by the app itself (a calligraphy nib swept along each digit).

![Icon styles](icon-styles.png)

Errors are logged to `%LOCALAPPDATA%\RefreshSwitch\error.log`.

## How to get the app (no programming needed)

You do not need to install anything: the app is built with a tool that is already part of Windows 10 and 11.

1. At the top of this page click the green **Code** button, then **Download ZIP**.
2. Open your Downloads folder, right-click the ZIP file and choose **Extract All...**, then **Extract**.
3. Open the extracted folder and double-click **`build.bat`** (it may show simply as `build`).
   - If a blue "Windows protected your PC" window appears, click **More info**, then **Run anyway**. It appears because the file was downloaded from the internet.
4. A black window opens and after a moment says **Done**. Press any key to close it.
5. In the same folder there is now **`RefreshSwitch.exe`**: double-click it to start the app. No administrator permission is needed.
6. The icon appears in the tray, next to the clock. If you do not see it, click the small **^** arrow; you can drag the icon onto the taskbar to keep it always visible.

To start it automatically, click the icon and tick **Start with Windows**. You can move the folder wherever you like first: if you move it afterwards, untick and tick the option again.

To update, download the ZIP again and repeat the steps. Close the app first (click the icon, then **Exit**), or the build cannot replace the file.

### From the command line

If you prefer, this is the command that `build.bat` runs:

```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe -nologo -target:winexe -out:RefreshSwitch.exe -r:System.Windows.Forms.dll -r:System.Drawing.dll -r:System.Management.dll RefreshSwitch.cs Gothic.cs
```
