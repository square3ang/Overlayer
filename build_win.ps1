[CmdletBinding()]
param(
    [ValidateSet("Release", "Debug")]
    [string] $Configuration = 'Release'
)

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $scriptRoot

# -----------------------------
# Logger
# -----------------------------
function Log($type, $msg) {
    switch($type) {
        "INFO" { Write-Host "[IN] $msg" -ForegroundColor DarkCyan }
        "COPY" { Write-Host "[->] $msg" -ForegroundColor Blue }
        "PLAN" { Write-Host "[>>] $msg" -ForegroundColor Cyan }
        "SKIP" { Write-Host "[..] $msg" -ForegroundColor DarkGray }
        "HINT" { Write-Host "[TI] $msg" -ForegroundColor Magenta }
        "WARN" { Write-Host "[.!] $msg" -ForegroundColor Yellow }
        "OK"   { Write-Host "[OK] $msg" -ForegroundColor Green }
        "ERR"  { Write-Host "[!!] $msg" -ForegroundColor Red }
        default { Write-Host "[--] $msg" }
    }
}

function Ask($msg) {
    Write-Host "[??] $msg"
    Write-Host "<< " -NoNewline
    return Read-Host
}

# First log
Log INFO "Initializing..."
Log INFO "Configuration: $Configuration"

# -----------------------------
# Paths
# -----------------------------
$buildRoot = Join-Path $scriptRoot 'build'

$resultRoot = Join-Path $buildRoot 'result'
$buildDir   = Join-Path $resultRoot 'win64'
$libDir     = Join-Path $buildDir 'lib'
$langDir    = Join-Path $buildDir 'lang'

$zipRoot = Join-Path $buildRoot 'zip'
$zipDir  = Join-Path $zipRoot 'win64'
$zipFile = Join-Path $zipDir 'Overlayer.zip'

$settingsFile = Join-Path $scriptRoot 'build_settings.json'

# -----------------------------
# Load / Init Settings
# -----------------------------
$AutoOverwrite = $null
$DestPathInput = $null
$DoZip = $null

# -----------------------------
# Utils
# -----------------------------
function Find-And-Copy($patterns, $dest) {
    foreach($pat in $patterns) {
        $found = Get-ChildItem $scriptRoot -Recurse -File -ErrorAction SilentlyContinue |
                 Where-Object { $_.Name -like $pat } | Select-Object -First 1
        if($found) {
            Copy-Item $found.FullName $dest -Force
            Log COPY "$($found.FullName) -> $dest"
            return
        }
    }
    Log WARN "Lib not found: $patterns"
}

function Invoke-Step([string] $name, [scriptblock] $action) {
    try {
        & $action
        Log OK $name
    } catch {
        Log ERR "$name -> $($_.Exception.Message)"
        exit 1
    }
}

if(Test-Path $settingsFile) {
    try {
        $saved = Get-Content $settingsFile | ConvertFrom-Json

        if($saved.PSObject.Properties.Name -contains 'AutoOverwrite') {
            $AutoOverwrite = $saved.AutoOverwrite
        }

        if($saved.PSObject.Properties.Name -contains 'Destination') {
            $DestPathInput = $saved.Destination
        }

        if($saved.PSObject.Properties.Name -contains 'DoZip') {
            $DoZip = $saved.DoZip
        }

        Log OK "Loaded settings"
    } catch {
        Log WARN "Failed to load settings"
    }
}

# -----------------------------
# Ask missing settings
# -----------------------------
if($null -eq $AutoOverwrite) {
    $AutoOverwrite = (Ask "Overwrite the built files to the Mods/Overlayer folder in ADOFAI? (y/N)") -match '^[yY]'
}

if($AutoOverwrite -and [string]::IsNullOrWhiteSpace($DestPathInput)) {
    $defaultPath = "C:\Program Files (x86)\Steam\steamapps\common\A Dance of Fire and Ice\Mods\Overlayer"

    Log HINT "Enter the ADOFAI game folder (where the .exe is located)"
    Log HINT "Example: C:\Program Files (x86)\Steam\steamapps\common\A Dance of Fire and Ice"
    Log HINT "Mods\Overlayer will be appended automatically"

    $_input = Ask "Enter game folder path (empty = default)"

    if([string]::IsNullOrWhiteSpace($_input)) {
        $DestPathInput = $defaultPath
        Log HINT "Using default Mods/Overlayer path"
    } else {
        $DestPathInput = Join-Path $_input "Mods\Overlayer"
    }

    Log PLAN "Destination -> $DestPathInput"

    if(Test-Path $DestPathInput) {
        Log OK "Path exists"
    } else {
        Log WARN "Path does not exist"
    }
}

if($null -eq $DoZip) {
    $DoZip = -not ((Ask "Create zip? (Y/n)") -match '^[nN]')
}

# -----------------------------
# Clean
# -----------------------------
Log PLAN "Cleaning build directory..."

Invoke-Step "Remove previous build" {
    Remove-Item $buildRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Invoke-Step "Recreate directories" {
    New-Item -ItemType Directory -Path $libDir -Force | Out-Null
    New-Item -ItemType Directory -Path $langDir -Force | Out-Null
    New-Item -ItemType Directory -Path $zipDir -Force | Out-Null
}

Log OK "Build directory ready"

# -----------------------------
# Build
# -----------------------------
Log PLAN "Building project..."

Invoke-Step "Build" {
    $projects = Get-ChildItem $scriptRoot -Recurse -Filter *.csproj |
                Where-Object {
                    $_.Name -like 'Overlayer*.csproj' -or
                    $_.Name -like '*Bootstrapper*.csproj'
                }

    foreach($p in $projects) {
        Log PLAN "Building $($p.Name)..."
        dotnet build $p.FullName -c $Configuration -v minimal

        if($LASTEXITCODE -ne 0) {
            throw "Build failed: $($p.Name)"
        }
    }
}

# -----------------------------
# Copy Overlayer DLLs
# -----------------------------
Log PLAN "Copying Overlayer DLLs..."

Invoke-Step "Copy Overlayer DLLs" {
    foreach($name in @('Overlayer.dll','Overlayer.Bootstrapper.dll')) {
        $found = Get-ChildItem $scriptRoot -Recurse -File |
                 Where-Object {
                     $_.Name -ieq $name -and $_.FullName -like "*\bin\$Configuration\*"
                 } |
                 Select-Object -First 1

        if($found) {
            Copy-Item $found.FullName $buildDir -Force
            Log COPY "$($found.FullName) -> $buildDir"
        }
        else {
            Log WARN "Missing DLL: $name"
        }
    }
}

# -----------------------------
# Root files
# -----------------------------
Log PLAN "Copying root files..."

Invoke-Step "Copy root files" {
    foreach($f in @('info.json','ov3_logo.png','update.txt')) {
        $found = Get-ChildItem $scriptRoot -Recurse -File |
                 Where-Object { $_.Name -ieq $f } |
                 Select-Object -First 1

        if($found) {
            Copy-Item $found.FullName $buildDir -Force
            Log COPY "$($found.FullName) -> $buildDir"
        }
        else {
            Log WARN "$f not found"
        }
    }
}

# -----------------------------
# Lang
# -----------------------------
Log PLAN "Copying language files..."

Invoke-Step "Copy language files" {
    $copied = $false

    foreach($src in @(
        (Join-Path $scriptRoot 'Overlayer\MiscFiles\lang'),
        (Join-Path $scriptRoot 'MiscFiles\lang')
    )) {
        if(Test-Path $src) {
            Copy-Item "$src\*" $langDir -Recurse -Force
            Log COPY "$src -> $langDir"
            $copied = $true
            break
        }
    }

    if(-not $copied) {
        Log WARN "No lang directory copied"
    }
}

# -----------------------------
# Libs
# -----------------------------
Log PLAN "Copying libraries..."

$libs = @(
    'Acornima','Jint','LibreHardwareMonitorLib','NCalc',
    'System.Memory','System.Numerics.Vectors',
    'System.Runtime.CompilerServices.Unsafe','Vostok.Sys.Metrics.PerfCounters'
)

Invoke-Step "Copy libraries" {
    foreach($lib in $libs) {

        $found = $null

        foreach($pat in @("$lib.dll","*$lib*.dll")) {
            $found = Get-ChildItem -Path $scriptRoot -Recurse -File -ErrorAction SilentlyContinue |
                     Where-Object { $_.Name -like $pat } |
                     Select-Object -First 1

            if($found) { break }
        }

        if($null -ne $found) {
            Copy-Item $found.FullName $libDir -Force
            Log COPY "$($found.Name) -> $libDir"
        } else {
            Log WARN "Missing Lib: $lib"
        }
    }
}

# -----------------------------
# Auto overwrite
# -----------------------------
if($AutoOverwrite -and $DestPathInput) {
    Log PLAN "Applying auto overwrite..."

    Invoke-Step "Auto overwrite" {
        Copy-Item (Join-Path $buildDir '*') $DestPathInput -Recurse -Force
        Log COPY "Build -> $DestPathInput"
    }
}
else {
    Log SKIP "Auto-overwrite disabled"
}

# -----------------------------
# Zip
# -----------------------------
Log PLAN "Creating zip archive..."

if($DoZip) {
    Invoke-Step "Zip archive" {
        $zipSource = Join-Path $buildDir '*'
        Compress-Archive -Path $zipSource -DestinationPath $zipFile -Force -ErrorAction Stop

        # -----------------------------
        # Size info
        # -----------------------------
        $sourceSize = (Get-ChildItem $buildDir -Recurse -File |
                      Measure-Object -Property Length -Sum).Sum

        $zipSize = (Get-Item $zipFile).Length

        $saved = $sourceSize - $zipSize
        $ratio = if($sourceSize -ne 0) { [math]::Round(($zipSize / $sourceSize) * 100, 2) } else { 0 }

        Log INFO ("{0:N2} MB -> {1:N2} MB ({2}% compressed, saved {3:N2} MB)" -f `
            ($sourceSize / 1MB), ($zipSize / 1MB), $ratio, ($saved / 1MB))
    }
} else {
    Log SKIP "Zip disabled"
}

# -----------------------------
# Save settings
# -----------------------------
[ordered]@{
    AutoOverwrite = $AutoOverwrite
    Destination = $DestPathInput
    DoZip = $DoZip
    Timestamp = (Get-Date).ToString('o')
} | ConvertTo-Json -Depth 3 | Out-File $settingsFile -Encoding utf8

Log OK "Done!"
