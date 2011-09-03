msbuild ..\Floe.Net\Floe.Net.csproj /p:Configuration=Release
msbuild ..\Floe.Configuration\Floe.Configuration.csproj /P:Configuration=Release
msbuild ..\Floe.UI\Floe.UI.csproj /P:Configuration=Release
copy ..\Floe.UI\bin\Release\*.exe .
copy ..\Floe.UI\bin\Release\*.dll .
candle setup.wxs -ext WiXNetFxExtension
light setup.wixobj -ext WiXNetFxExtension -cultures:en-US -ext WixUIExtension.dll -dWixUILicenseRtf=License.rtf

