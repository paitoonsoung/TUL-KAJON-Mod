$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$out = Join-Path $root 'TUL_ESP32_FLASH_TOOL_v2.1'
New-Item -ItemType Directory -Force -Path $out | Out-Null

$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path $csc)) {
    $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (!(Test-Path $csc)) {
    throw 'csc.exe not found. Install .NET Framework 4.x or Visual Studio Build Tools.'
}

$source = Join-Path $root 'TulESP32FlashTool.cs'
$outExe = Join-Path $out 'TulESP32FlashTool.exe'

& $csc /nologo /target:winexe /platform:anycpu /optimize+ "/out:$outExe" /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll "$source"

if ($LASTEXITCODE -ne 0) {
    throw 'C# compilation failed.'
}

$readme = Join-Path $root 'README.md'
if (Test-Path $readme) {
    Copy-Item $readme (Join-Path $out 'README.md') -Force
}

Write-Host ""
Write-Host "BUILD COMPLETE"
Write-Host "EXE: $outExe"
