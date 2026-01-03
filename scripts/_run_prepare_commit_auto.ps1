try {
    dotnet format "windows-cleaner.csproj" --verbosity minimal
} catch {
    Write-Host 'dotnet format not available or failed'
}

# Build
dotnet build "windows-cleaner.csproj" -v minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host 'Build failed — aborting.'
    exit 1
}

# Check for git changes
$st = git status --porcelain
if ($st -and $st.Length -gt 0) {
    Write-Host 'Changes detected — staging and committing.'
    git add -A
    git commit -m 'chore: format and group changes before commit'
    if ($LASTEXITCODE -eq 0) {
        Write-Host 'Commit created successfully.'
    } else {
        Write-Host 'git commit failed or no changes to commit.'
    }
} else {
    Write-Host 'No changes to commit.'
}