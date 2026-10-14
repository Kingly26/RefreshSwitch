# Accessibility

RefreshSwitch is a small tray app: an icon next to the clock and a menu. This page says what it does to be usable by everyone, and where it falls short.

It has not been tested with a screen reader or other assistive technology yet. The points below describe how the app is built and what Windows is expected to do with it. If something does not work for you, please [open an issue](../../issues): accessibility problems are treated as bugs.

## What should work

- **Keyboard.** Press `Win` + `B` to move focus to the tray, use the arrow keys to reach the RefreshSwitch icon, then press the Menu key or `Shift` + `F10` to open the menu. The menu is a standard Windows menu: arrow keys to move, `Enter` to choose, `Esc` to close.
- **Screen readers.** The icon has a text label that lists each monitor with its refresh rate, for example "DELL U2723QE [Display 1]: 60 Hz". Menu entries are standard menu items with text labels, and the current rate of each monitor is exposed as checked.
- **Not colour alone.** The refresh rate is always shown as a number. In the **Color by refresh rate** style the colour is only an extra cue, so the icon stays readable with colour blindness.
- **Light and dark taskbars.** The monochrome styles switch between white and black to follow the Windows theme.
- **Language.** The interface follows the Windows language, in English or Italian.
- **No time pressure.** Nothing in the app needs a quick reaction, and it never plays sounds or animations of its own.

## Choosing a readable icon

The icon is only 16 to 32 pixels and often has to fit three digits, so the style matters. From the **Icon style** menu:

- the easiest to read are **Monochrome number** and **Colored number**, where the digits are largest;
- the **Gothic** styles are decorative and harder to read at this size, especially with three digits.

## Things to be careful with

- **The screen goes black for a moment** when the refresh rate changes or a monitor is disabled or re-enabled. That is Windows applying the new mode. If sudden flashes are a problem for you, leave **Change refresh rate by clicking the icon** switched off, so a rate only changes when you pick it from the menu.
- **Disabling a monitor moves its windows** to the screens that remain. If you rely on a magnifier or a fixed window layout, expect to rearrange it afterwards.
- **Turn screens off** puts every monitor in standby. Moving the mouse or pressing any key wakes them.

## Known limits

- The icon cannot be made larger than Windows draws it. Raising the display scale in Windows settings enlarges the whole tray, icon included.
- The icon's text label is cut at 63 characters by Windows, so with several monitors the last ones may be missing from it. The menu always lists them all.
- There is no global keyboard shortcut; you have to go through the tray.
- Pressing `Enter` on the icon does nothing: use the Menu key or `Shift` + `F10`.
- Errors are shown as Windows notifications, which disappear after a few seconds. They are also saved in `%LOCALAPPDATA%\RefreshSwitch\error.log`.
