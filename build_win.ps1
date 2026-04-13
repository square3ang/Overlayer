[CmdletBinding()]
param(
    [string] $Configuration = 'Release',
    [switch] $Force
)

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $scriptRoot

$buildDir = Join-Path $scriptRoot 'build'
$libDir = Join-Path $buildDir 'lib'
$langDir = Join-Path $buildDir 'lang'

if(Test-Path $buildDir) {
    if($Force) {
        Remove-Item $buildDir -Recurse -Force -ErrorAction SilentlyContinue
    } else {
        Write-Host "Build folder already exists. Rerun with -Force to recreate." -ForegroundColor Yellow
        return
    }
}

New-Item -ItemType Directory -Path $libDir -Force | Out-Null
New-Item -ItemType Directory -Path $langDir -Force | Out-Null

# Prompt whether to auto-overwrite the built files into a game folder
$autoChoice = Read-Host "Auto-overwrite to game folder? (y/N)"
$AutoOverwrite = $false
if($autoChoice -match '^[yY]') { $AutoOverwrite = $true }

# If user requested auto-overwrite, ask for destination; empty means try to infer from project PostBuildEvent
$DestPathInput = $null
if($AutoOverwrite) {
    $DestPathInput = Read-Host "Enter game folder path (leave empty to infer from project's post-build event)"
}

function Find-And-Copy($namePatterns, $destFolder) {
    foreach($pat in $namePatterns) {
        $found = Get-ChildItem -Path $scriptRoot -Recurse -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -like $pat } | Select-Object -First 1
        if($found) {
            Copy-Item -Path $found.FullName -Destination $destFolder -Force
            Write-Host "Copied $($found.Name) -> $destFolder"
            return $true
        }
    }
    Write-Host "Warning: Could not find $($namePatterns -join ', ')" -ForegroundColor Yellow
    return $false
}

# Copy main DLLs into build root
$mainDlls = @(
    @{name='Overlayer.dll'; patterns=@('Overlayer.dll')},
    @{name='Overlayer.Bootstrapper.dll'; patterns=@('Overlayer.Bootstrapper.dll')}
)

foreach($entry in $mainDlls) {
    $name = $entry.name
    $found = Get-ChildItem -Path $scriptRoot -Recurse -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -ieq $name } | Select-Object -First 1
    if($found) {
        Copy-Item -Path $found.FullName -Destination $buildDir -Force
        Write-Host "Copied $name -> $buildDir"
    } else {
        Write-Host "Warning: $name not found." -ForegroundColor Yellow
    }
}

# Copy ov3_logo.png and info.json
$filesToRoot = @('ov3_logo.png', 'info.json')
foreach($f in $filesToRoot) {
    $found = Get-ChildItem -Path $scriptRoot -Recurse -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -ieq $f } | Select-Object -First 1
    if($found) {
        Copy-Item -Path $found.FullName -Destination $buildDir -Force
        Write-Host "Copied $($found.Name) -> $buildDir"
    } else {
        Write-Host "Warning: $f not found." -ForegroundColor Yellow
    }
}

# Copy lang folder (prefer Overlayer\MiscFiles\lang)
$possibleLangSources = @(Join-Path $scriptRoot 'Overlayer\MiscFiles\lang', Join-Path $scriptRoot 'MiscFiles\lang', Join-Path $scriptRoot 'Overlayer\MiscFiles')
$copiedLang = $false
foreach($src in $possibleLangSources) {
    if(Test-Path $src) {
        # if src is directory and contains 'lang' subfolder
        if((Get-Item $src).PSIsContainer -and (Test-Path (Join-Path $src 'lang'))) {
            Copy-Item -Path (Join-Path $src 'lang') -Destination $langDir -Recurse -Force
            Write-Host "Copied lang -> $langDir"
            $copiedLang = $true
            break
        }
        # if src itself is the lang folder
        if((Get-Item $src).PSIsContainer -and (Get-ChildItem $src -ErrorAction SilentlyContinue)) {
            Copy-Item -Path $src -Destination $langDir -Recurse -Force
            Write-Host "Copied $src -> $langDir"
            $copiedLang = $true
            break
        }
    }
}
if(-not $copiedLang) {
    # fallback: try to find a folder named 'lang'
    $foundLang = Get-ChildItem -Path $scriptRoot -Recurse -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -ieq 'lang' } | Select-Object -First 1
    if($foundLang) {
        Copy-Item -Path $foundLang.FullName -Destination $langDir -Recurse -Force
        Write-Host "Copied lang -> $langDir"
    } else {
        Write-Host "Warning: lang folder not found (expected under Overlayer\\MiscFiles\\lang)." -ForegroundColor Yellow
    }
}

# Libraries to copy into lib
$libs = @(
    'Acornima',
    'Jint',
    'LibreHardwareMonitorLib',
    'NCalc',
    'System.Memory',
    'System.Numerics.Vectors',
    'System.Runtime.CompilerServices.Unsafe',
    'Vostok.Sys.Metrics.PerfCounters'
)

foreach($lib in $libs) {
    $patterns = @("$lib.dll", "$lib.*.dll", "*$lib*.dll")
    Find-And-Copy $patterns $libDir | Out-Null
}

Write-Host "Build tree created at: $buildDir" -ForegroundColor Green
Write-Host "Contents:"
Get-ChildItem -Path $buildDir -Recurse | ForEach-Object { Write-Host $_.FullName }

# If requested, attempt to copy build contents into the game folder (inferred or explicit)
$buildSettings = [ordered]@{
    AutoOverwrite = $AutoOverwrite
    Destination = $null
    InferredFrom = $null
    FilesCopied = @()
    Timestamp = (Get-Date).ToString('o')
}

if($AutoOverwrite) {
    $dest = $null
    if([string]::IsNullOrWhiteSpace($DestPathInput)) {
        # Try to infer from a .csproj PostBuildEvent or OutputPath
        $proj = Get-ChildItem -Path $scriptRoot -Recurse -Filter *.csproj -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -like 'Overlayer*.csproj' } | Select-Object -First 1
        if(-not $proj) {
            $proj = Get-ChildItem -Path $scriptRoot -Recurse -Filter *.csproj -ErrorAction SilentlyContinue | Select-Object -First 1
        }

        if($proj) {
            try {
                $xml = [xml](Get-Content $proj.FullName -ErrorAction Stop)
                # Try to collect PostBuildEvent nodes
                $postNodes = $xml.Project.PropertyGroup | ForEach-Object { $_.PostBuildEvent } | Where-Object { $_ -ne $null }
                $postText = $postNodes -join "`n"
                $buildSettings.InferredFrom = $proj.FullName

                if(-not [string]::IsNullOrWhiteSpace($postText)) {
                    $matches = [regex]::Matches($postText, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
                    if($matches.Count -gt 0) {
                        # Choose last quoted value as likely destination and try to expand common MSBuild macros
                        $candidate = $matches[-1]
                        $projDir = Split-Path -Parent $proj.FullName
                        $outPathNode = $xml.Project.PropertyGroup | Where-Object { $_.OutputPath } | Select-Object -First 1
                        $outPath = $null
                        if($outPathNode) { $outPath = $outPathNode.OutputPath }
                        if(-not $outPath) { $outPath = "bin\\$Configuration\\" }

                        $candidate = $candidate -replace '\$\((ProjectDir|MSBuildProjectDirectory)\)', [regex]::Escape($projDir)
                        $candidate = $candidate -replace '\$\((SolutionDir)\)', [regex]::Escape($scriptRoot)
                        $candidate = $candidate -replace '\$\((Configuration)\)', $Configuration
                        $candidate = $candidate -replace '\$\((TargetDir|OutDir)\)', (Join-Path $projDir $outPath)

                        # If relative, make absolute relative to project dir
                        if(-not [System.IO.Path]::IsPathRooted($candidate)) {
                            $candidate = Join-Path $projDir $candidate
                        }
                        $dest = $candidate
                    }
                }

                if(-not $dest) {
                    # Fallback to project's output path
                    $projDir = Split-Path -Parent $proj.FullName
                    $outPathNode = $xml.Project.PropertyGroup | Where-Object { $_.OutputPath } | Select-Object -First 1
                    $outPath = $outPathNode.OutputPath -replace '\$\((Configuration)\)', $Configuration
                    if(-not [System.IO.Path]::IsPathRooted($outPath)) {
                        $dest = Join-Path $projDir $outPath
                    } else {
                        $dest = $outPath
                    }
                }
            } catch {
                Write-Host "Warning: Failed to parse project file to infer destination: $($_.Exception.Message)" -ForegroundColor Yellow
            }
        }
    } else {
        $dest = $DestPathInput
    }

    if($dest) {
        # Ensure destination exists
        try {
            New-Item -ItemType Directory -Path $dest -Force | Out-Null
            # Copy build contents to destination
            Copy-Item -Path (Join-Path $buildDir '*') -Destination $dest -Recurse -Force -ErrorAction Stop
            $files = Get-ChildItem -Path $buildDir -Recurse | ForEach-Object { $_.FullName }
            $buildSettings.Destination = $dest
            $buildSettings.FilesCopied = $files
            Write-Host "Copied build contents -> $dest" -ForegroundColor Green
        } catch {
            Write-Host "Warning: Failed to copy build contents to $dest: $($_.Exception.Message)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "No destination could be inferred and none supplied; skipping auto-overwrite." -ForegroundColor Yellow
    }
}

# Write build settings artifact
$settingsFile = Join-Path $buildDir 'build_settings.json'
try {
    $buildSettings | ConvertTo-Json -Depth 5 | Out-File -FilePath $settingsFile -Encoding utf8
    Write-Host "Wrote build settings -> $settingsFile" -ForegroundColor Green
} catch {
    Write-Host "Warning: Failed to write build settings: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "Done." -ForegroundColor Green
