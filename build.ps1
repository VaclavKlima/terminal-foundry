param(
    [string]$PhpExePath,
    [string]$OutDir = "dist"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-CscPath {
    $csc = Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\csc.exe"
    if (Test-Path $csc) { return $csc }
    $csc = Join-Path $env:WINDIR "Microsoft.NET\Framework\v3.5\csc.exe"
    if (Test-Path $csc) { return $csc }
    $csc = Join-Path $env:WINDIR "Microsoft.NET\Framework\v2.0.50727\csc.exe"
    if (Test-Path $csc) { return $csc }
    throw "Could not find csc.exe. Install .NET Framework or provide a compiler."
}

function Resolve-PhpExe {
    param([string]$PathHint)

    function Resolve-PhpFromBat {
        param([string]$BatPath)

        if (-not (Test-Path $BatPath)) { return $null }
        $content = Get-Content -Path $BatPath -ErrorAction SilentlyContinue
        if (-not $content) { return $null }
        $match = [regex]::Match($content -join "`n", '"([^"]+\\php\.exe)"', "IgnoreCase")
        if ($match.Success -and (Test-Path $match.Groups[1].Value)) {
            return (Resolve-Path $match.Groups[1].Value).Path
        }
        return $null
    }

    if ($PathHint) {
        if (-not (Test-Path $PathHint)) { throw "PhpExePath not found: $PathHint" }
        if ($PathHint -like "*.bat") {
            $resolved = Resolve-PhpFromBat -BatPath $PathHint
            if ($resolved) { return $resolved }
            throw "PhpExePath points to a .bat but php.exe could not be resolved: $PathHint"
        }
        return (Resolve-Path $PathHint).Path
    }

    $cmd = Get-Command php -ErrorAction SilentlyContinue
    if ($cmd) {
        if ($cmd.Source -like "*.exe") { return $cmd.Source }
        if ($cmd.Source -like "*.bat") {
            $resolved = Resolve-PhpFromBat -BatPath $cmd.Source
            if ($resolved) { return $resolved }
        }
    }

    throw "php.exe not found. Pass -PhpExePath with the full path to php.exe."
}

$cscPath = Get-CscPath
$phpExe = Resolve-PhpExe -PathHint $PhpExePath
$phpDir = Split-Path $phpExe -Parent

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

& $cscPath /nologo /target:winexe /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll /out:"$OutDir\PhpCompiler.exe" "Framework\Cli\Launcher.cs" "Framework\Cli\Launcher\*.cs"

Copy-Item -Force "index.php" $OutDir
if (Test-Path "vendor") { Copy-Item -Recurse -Force "vendor" $OutDir }
if (Test-Path "App") { Copy-Item -Recurse -Force "App" $OutDir }
if (Test-Path "Framework") { Copy-Item -Recurse -Force "Framework" $OutDir }
if (Test-Path "composer.json") { Copy-Item -Force "composer.json" $OutDir }
if (Test-Path "composer.lock") { Copy-Item -Force "composer.lock" $OutDir }

$phpWinExe = Join-Path $phpDir "php-win.exe"
if (Test-Path $phpWinExe) {
    Copy-Item -Force $phpWinExe $OutDir
}
Copy-Item -Force $phpExe $OutDir
Get-ChildItem -Path $phpDir -Filter "*.dll" -File -ErrorAction SilentlyContinue | Copy-Item -Force -Destination $OutDir
if (Test-Path (Join-Path $phpDir "ext")) { Copy-Item -Recurse -Force (Join-Path $phpDir "ext") $OutDir }
if (Test-Path (Join-Path $phpDir "php.ini")) { Copy-Item -Force (Join-Path $phpDir "php.ini") $OutDir }

Write-Host "Built $OutDir\PhpCompiler.exe"
