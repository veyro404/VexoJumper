[CmdletBinding()]
param(
    [ValidateSet('win-x64', 'win-arm64')]
    [string]$Runtime = 'win-x64',
    [string]$RequiredSdk = '10.0.404',
    [switch]$SkipRun
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$Solution = Join-Path $ProjectRoot 'VexoJumper.sln'
$Project = Join-Path $ProjectRoot 'src\VexoJumper\VexoJumper.csproj'
$Output = Join-Path $ProjectRoot "artifacts\$Runtime"
$LogDir = Join-Path $ProjectRoot 'artifacts\logs'
New-Item -ItemType Directory -Force -Path $LogDir | Out-Null
$Log = Join-Path $LogDir ("build-{0}.log" -f (Get-Date -Format 'yyyyMMdd-HHmmss'))

function Invoke-DotNet {
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Step
    )

    Write-Host "`n== $Step ==" -ForegroundColor Cyan
    & dotnet @Arguments 2>&1 | Tee-Object -FilePath $Log -Append
    $code = $LASTEXITCODE
    if ($code -ne 0) {
        throw "Step '$Step' failed with exit code $code. See log: $Log"
    }
}

try {
    if ($env:OS -ne 'Windows_NT') {
        throw 'This is a WPF application. Run build and publish on Windows.'
    }
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw '.NET CLI (dotnet) was not found. Install the required .NET SDK.'
    }
    if (-not (Test-Path -LiteralPath $Solution)) {
        throw "Solution file not found: $Solution"
    }
    if (-not (Test-Path -LiteralPath $Project)) {
        throw "Project file not found: $Project"
    }

    "Vexo Jumper build started: $(Get-Date -Format o)" | Set-Content -LiteralPath $Log -Encoding UTF8

    $sdkList = @(& dotnet --list-sdks 2>&1)
    $sdkExitCode = $LASTEXITCODE
    $sdkList | Tee-Object -FilePath $Log -Append | Out-Host
    if ($sdkExitCode -ne 0) {
        throw "Could not query installed .NET SDKs. Exit code: $sdkExitCode"
    }

    $sdkPattern = '^' + [regex]::Escape($RequiredSdk) + '\s+\['
    $sdkFound = $false
    foreach ($line in $sdkList) {
        if ([string]$line -match $sdkPattern) {
            $sdkFound = $true
            break
        }
    }
    if (-not $sdkFound) {
        throw ".NET SDK $RequiredSdk is not installed. Installed SDKs:`n$($sdkList -join "`n")"
    }

    # Fetch the official Vazirmatn font files and embed them as WPF resources.
    # If the network is unavailable, continue with the Windows fallback font.
    $fontDir = Join-Path $ProjectRoot 'src\VexoJumper\Assets\Fonts'
    New-Item -ItemType Directory -Force -Path $fontDir | Out-Null
    $fontFiles = @('Vazirmatn-Regular.ttf', 'Vazirmatn-Medium.ttf', 'Vazirmatn-SemiBold.ttf', 'Vazirmatn-Bold.ttf')
    foreach ($fontFile in $fontFiles) {
        $fontPath = Join-Path $fontDir $fontFile
        if (-not (Test-Path -LiteralPath $fontPath)) {
            $fontUrls = @(
                "https://cdn.jsdelivr.net/npm/vazirmatn@33.003/fonts/ttf/$fontFile",
                "https://cdn.jsdelivr.net/gh/rastikerdar/vazirmatn@master/fonts/ttf/$fontFile"
            )
            $fontDownloaded = $false
            foreach ($fontUrl in $fontUrls) {
                try {
                    Invoke-WebRequest -Uri $fontUrl -OutFile $fontPath -UseBasicParsing -TimeoutSec 25
                    if ((Get-Item -LiteralPath $fontPath).Length -lt 10000) { Remove-Item -LiteralPath $fontPath -Force; throw 'Downloaded font file is unexpectedly small.' }
                    Write-Host "Font added: $fontFile" -ForegroundColor Green
                    $fontDownloaded = $true
                    break
                }
                catch {
                    if (Test-Path -LiteralPath $fontPath) { Remove-Item -LiteralPath $fontPath -Force }
                }
            }
            if (-not $fontDownloaded) {
                Write-Warning "Could not download $fontFile. The app will use Segoe UI fallback. Check internet access or place the official TTF in $fontDir."
            }
        }
    }

    # Remove stale WPF generated resources from previous source versions.
    $binDir = Join-Path $ProjectRoot 'src\VexoJumper\bin'
    $objDir = Join-Path $ProjectRoot 'src\VexoJumper\obj'
    if (Test-Path -LiteralPath $binDir) { Remove-Item -LiteralPath $binDir -Recurse -Force }
    if (Test-Path -LiteralPath $objDir) { Remove-Item -LiteralPath $objDir -Recurse -Force }

    Invoke-DotNet -Arguments @('--info') -Step 'Check .NET environment'
    Invoke-DotNet -Arguments @('restore', $Solution) -Step 'Restore dependencies'
    Invoke-DotNet -Arguments @('build', $Solution, '-c', 'Release', '--no-restore', '-warnaserror') -Step 'Build Release (warnings as errors)'
    Invoke-DotNet -Arguments @('publish', $Project, '-c', 'Release', '-r', $Runtime, '--self-contained', 'true', '-p:PublishSingleFile=true', '-p:IncludeNativeLibrariesForSelfExtract=true', '-p:EnableWindowsTargeting=true', '-o', $Output) -Step 'Publish self-contained application'

    $exe = Join-Path $Output 'VexoJumper.exe'
    if (-not (Test-Path -LiteralPath $exe)) {
        throw "Published executable was not found: $exe"
    }

    $hash = Get-FileHash -LiteralPath $exe -Algorithm SHA256
    $hashText = "SHA256  $($hash.Hash)  VexoJumper.exe"
    $hashText | Set-Content -LiteralPath (Join-Path $Output 'SHA256.txt') -Encoding ASCII

    Write-Host "`nBuild and publish completed successfully." -ForegroundColor Green
    Write-Host "Output: $exe"
    Write-Host "Log: $Log"
    Write-Host $hashText

    if (-not $SkipRun) {
        Start-Process -FilePath $exe
    }
}
catch {
    $message = $_.Exception.Message
    Write-Host "BUILD FAILED: $message" -ForegroundColor Red
    if (Test-Path -LiteralPath $Log) {
        Write-Host "Log file: $Log" -ForegroundColor Yellow
    }
    exit 1
}
