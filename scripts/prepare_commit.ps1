<#
Prepare and create a single commit with formatting and build check.

Usage (PowerShell):
.\scripts\prepare_commit.ps1

What it does:

Note: this script will run `git` commands only after interactive confirmation.
#>

Write-Host "Preparing repository: formatting, building, and optional single commit" -ForegroundColor Cyan

function Run-Command($cmd, $failMessage) {
    Write-Host "-> $cmd" -ForegroundColor DarkCyan
    $res = Invoke-Expression $cmd 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host $failMessage -ForegroundColor Red
        return $false
    }
    return $true
}

# Try dotnet format
Write-Host "Running 'dotnet format' (if installed)..." -ForegroundColor Yellow
$formatOk = $true
try {
    & dotnet format --version > $null 2>&1
    $formatOk = $LASTEXITCODE -eq 0 -or $LASTEXITCODE -eq $null
}
catch { $formatOk = $false }

if ($formatOk) {
    if (-not (Run-Command "dotnet format" "dotnet format failed")) {
        Write-Host "You can install dotnet-format: `dotnet tool install -g dotnet-format`" -ForegroundColor Yellow
    }
} else {
    Write-Host "dotnet-format not found. To auto-format, install it globally:" -ForegroundColor Yellow
    Write-Host "  dotnet tool install -g dotnet-format" -ForegroundColor Yellow
}

# Build
Write-Host "Building project..." -ForegroundColor Yellow
if (-not (Run-Command "dotnet build \"windows-cleaner.csproj\" -v minimal" "dotnet build failed")) {
    Write-Host "Build failed — aborting prepare commit." -ForegroundColor Red
    exit 1
}

# Confirm git actions
$proceed = Read-Host "Stage all changes and create a single commit now? (y/N)"
if ($proceed -ne 'y' -and $proceed -ne 'Y') {
    Write-Host "Done. No git operations performed. Review changes, then commit as you prefer." -ForegroundColor Green
    exit 0
}

Write-Host "Staging all changes..." -ForegroundColor Yellow
if (-not (Run-Command "git add -A" "git add failed")) { exit 1 }

$msg = Read-Host "Commit message (default: 'chore: format and group changes before commit')"
if ([string]::IsNullOrWhiteSpace($msg)) { $msg = "chore: format and group changes before commit" }

if (-not (Run-Command "git commit -m \"$msg\"" "git commit failed")) { exit 1 }

Write-Host "Commit created successfully." -ForegroundColor Green
Write-Host "Tip: if you need to squash multiple existing commits into one, use interactive rebase: 'git rebase -i HEAD~N'" -ForegroundColor Cyan
