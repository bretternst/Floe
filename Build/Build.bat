set PATH=%PATH%;c:\windows\microsoft.net\framework\v3.5;C:\Program Files (x86)\Windows Installer XML v3\bin
msbuild ..\Floe.Net\Floe.Net.csproj /p:Configuration=Release
msbuild ..\Floe.UI\Floe.UI.csproj /P:Configuration=Release
copy ..\Floe.UI\bin\Release\*.exe .
copy ..\Floe.UI\bin\Release\*.dll .
candle floe.wxs
light floe.wixobj