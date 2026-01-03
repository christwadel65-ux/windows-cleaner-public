# GitHub Branch Protection Automation Script
# Configure branch protection rules via GitHub API

# ⚠️ REQUIREMENTS:
# 1. GitHub Personal Access Token (PAT) with 'admin:org_hook' scope
# 2. PowerShell 5.0+

param(
    [Parameter(Mandatory=$true)]
    [string]$GitHubToken,
    
    [Parameter(Mandatory=$true)]
    [string]$Owner = "christwadel65-ux",
    
    [Parameter(Mandatory=$true)]
    [string]$Repo = "Windows-Cleaner",
    
    [string]$Branch = "master"
)

# GitHub API base URL
$BaseUrl = "https://api.github.com/repos/$Owner/$Repo"
$Headers = @{
    "Authorization" = "token $GitHubToken"
    "Accept" = "application/vnd.github.v3+json"
    "Content-Type" = "application/json"
}

Write-Host "🔐 Configuring branch protection for: $Owner/$Repo ($Branch)" -ForegroundColor Cyan
Write-Host ""

# 1. Create branch protection rule
Write-Host "📋 Creating branch protection rule..." -ForegroundColor Yellow

$ProtectionConfig = @{
    required_pull_request_reviews = @{
        dismiss_stale_reviews = $false
        require_code_owner_reviews = $false
        required_approving_review_count = 1
    }
    enforce_admins = $true
    required_status_checks = @{
        strict = $true
        contexts = @(
            "build",
            "test"
        )
    }
    restrictions = $null
}

try {
    $Response = Invoke-RestMethod `
        -Uri "$BaseUrl/branches/$Branch/protection" `
        -Method PUT `
        -Headers $Headers `
        -Body (ConvertTo-Json $ProtectionConfig -Depth 10) `
        -ErrorAction Stop
    
    Write-Host "✅ Branch protection rule created successfully!" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host "❌ Error creating branch protection rule:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# 2. Get current settings
Write-Host "📊 Current branch protection settings:" -ForegroundColor Yellow

try {
    $CurrentSettings = Invoke-RestMethod `
        -Uri "$BaseUrl/branches/$Branch/protection" `
        -Method GET `
        -Headers $Headers `
        -ErrorAction Stop
    
    Write-Host ""
    Write-Host "  Pull Request Reviews:" -ForegroundColor Cyan
    Write-Host "    - Required approvals: $($CurrentSettings.required_pull_request_reviews.required_approving_review_count)"
    Write-Host "    - Dismiss stale reviews: $($CurrentSettings.required_pull_request_reviews.dismiss_stale_reviews)"
    Write-Host "    - Require code owner review: $($CurrentSettings.required_pull_request_reviews.require_code_owner_reviews)"
    
    Write-Host ""
    Write-Host "  Status Checks:" -ForegroundColor Cyan
    Write-Host "    - Strict mode: $($CurrentSettings.required_status_checks.strict)"
    Write-Host "    - Contexts: $($CurrentSettings.required_status_checks.contexts -join ', ')"
    
    Write-Host ""
    Write-Host "  Other Rules:" -ForegroundColor Cyan
    Write-Host "    - Enforce for administrators: $($CurrentSettings.enforce_admins)"
    
    Write-Host ""
    Write-Host "✅ Configuration completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "❌ Error fetching settings:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host ""
Write-Host "📖 Next steps:" -ForegroundColor Cyan
Write-Host "  1. Go to: https://github.com/$Owner/$Repo/settings/branches"
Write-Host "  2. Verify the '$Branch' branch is protected"
Write-Host "  3. Adjust settings if needed via GitHub UI"
Write-Host ""
