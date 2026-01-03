$env:Path = "C:\Program Files\Git\cmd;$env:Path"

# Vérifier git
Write-Host "Git version:" -ForegroundColor Cyan
git --version

# Configurer git
Write-Host "Configuring git..." -ForegroundColor Yellow
git config --global user.email "dev@example.com"
git config --global user.name "Developer"

# Vérifier les changements
Write-Host "Checking git status..." -ForegroundColor Cyan
$st = git status --porcelain
if ($st) {
    Write-Host "Changes found:" -ForegroundColor Green
    Write-Host $st
    Write-Host "Staging and committing..." -ForegroundColor Yellow
    git add -A
    git commit -m "chore: format code and group changes - advanced cleanup UI with themed DataGridView, persistent settings, context menu"
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Commit created successfully!" -ForegroundColor Green
        git log --oneline -1
    } else {
        Write-Host "Commit failed." -ForegroundColor Red
    }
} else {
    Write-Host "No changes to commit." -ForegroundColor Yellow
}
