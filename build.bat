@echo off
rem Builds the app with the C# compiler that ships with Windows. Just double-click this file.
cd /d "%~dp0"
set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
  echo The C# compiler was not found. It is part of .NET Framework 4, included in Windows 10 and 11.
  echo Compilatore C# non trovato. Fa parte di .NET Framework 4, incluso in Windows 10 e 11.
  pause
  exit /b 1
)
"%CSC%" -nologo -target:winexe -out:RefreshSwitch.exe -r:System.Windows.Forms.dll -r:System.Drawing.dll -r:System.Management.dll RefreshSwitch.cs Gothic.cs
if errorlevel 1 (
  echo.
  echo Build failed. If RefreshSwitch is running, close it from its menu ^(Exit^) and try again.
  echo Compilazione non riuscita. Se RefreshSwitch e' aperto, chiudilo dal suo menu ^(Esci^) e riprova.
  pause
  exit /b 1
)
echo.
echo Done: RefreshSwitch.exe is ready in this folder. Double-click it to start.
echo Fatto: RefreshSwitch.exe e' pronto in questa cartella. Fai doppio clic per avviarlo.
pause
