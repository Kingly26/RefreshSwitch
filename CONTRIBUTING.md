# Contributing

Thanks for wanting to help. Bug reports, ideas and pull requests are all welcome, in English or Italian.

## Reporting a bug or asking for a feature

Open an issue and pick the matching template. For a bug, the most useful details are:

- your Windows version (10 or 11) and the app language;
- what you did, what you expected and what happened instead;
- a screenshot of the tray icon or menu, if it helps.

## Changing the code

The app is plain C# for .NET Framework 4, compiled with the compiler that ships with Windows. There is nothing to install and no project file.

1. Fork the repository and create a branch for your change.
2. Edit the `.cs` files with any text editor.
3. Double-click `build.bat` to compile. Close the app first, or the build cannot replace the `.exe`.
4. Run the app and check that your change works.
5. Open a pull request and describe what you changed and how you tested it.

A few things to keep in mind:

- The compiler only supports C# 5, so newer syntax (string interpolation with `$"..."`, `?.`, expression-bodied members) does not compile.
- Every text the user sees must be given in both languages with `Lang.T("italiano", "English")`.
- `Gothic.cs` is shared with the sister app, so keep the two copies identical.
- If you add or change an icon style, regenerate `icon-styles.png` by running the app with `--preview icon-styles.png`.
- Keep changes small and focused: one pull request per fix or feature.

By contributing you agree that your work is released under the project's [MIT license](LICENSE).
