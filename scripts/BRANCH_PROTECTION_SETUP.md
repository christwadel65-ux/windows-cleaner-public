# 🔐 Guide : Configurer la Protection de Branche Automatiquement

## Option 1️⃣ : Script PowerShell (RECOMMANDÉ)

C'est **le plus simple** et ça fait tout automatiquement !

### Étapes :

#### 1. Créer un GitHub Personal Access Token (PAT)

1. Va sur : https://github.com/settings/tokens
2. Clique **"Generate new token"** → **"Generate new token (classic)"**
3. **Token name** : `branch-protection-config`
4. **Expiration** : `90 days` (ou personnalisé)
5. **Scopes** : Coche `repo` (full control)
6. **Generate token** et **copie-le** (tu ne pourras le voir qu'une fois !)

```
⚠️ Garde ce token secret ! Ne le commit JAMAIS !
```

#### 2. Exécuter le Script

Ouvre **PowerShell** et exécute :

```powershell
# Option A: Si tu es dans le repo
cd "D:\GitHub\Windows Cleaner"
.\scripts\configure-branch-protection.ps1 `
  -GitHubToken "ghp_xxxxxxxxxxxxxxxxxxxxx" `
  -Owner "christwadel65-ux" `
  -Repo "Windows-Cleaner" `
  -Branch "master"

# Option B: Depuis n'importe où
D:\GitHub\Windows Cleaner\scripts\configure-branch-protection.ps1 `
  -GitHubToken "ghp_xxxxxxxxxxxxxxxxxxxxx" `
  -Owner "christwadel65-ux" `
  -Repo "Windows-Cleaner" `
  -Branch "master"
```

#### 3. Vérifier

Le script te montrera :
```
🔐 Configuring branch protection for: christwadel65-ux/Windows-Cleaner (master)

📋 Creating branch protection rule...
✅ Branch protection rule created successfully!

📊 Current branch protection settings:

  Pull Request Reviews:
    - Required approvals: 1
    - Dismiss stale reviews: False
    - Require code owner review: False

  Status Checks:
    - Strict mode: True
    - Contexts: build, test, security

  Other Rules:
    - Enforce for administrators: True

✅ Configuration completed successfully!
```

---

## Option 2️⃣ : Configuration Manuelle (JSON)

Si tu préfères faire manuellement, voici ce que configure le script :

**Fichier de référence** : `scripts/branch-protection-config.json`

```json
{
  "pullRequestReviews": {
    "required": true,
    "requiredApprovingReviewCount": 1,
    "dismissStaleReviews": false
  },
  "statusChecks": {
    "required": true,
    "strict": true,
    "contexts": ["build", "test", "security"]
  },
  "enforceAdmins": true
}
```

---

## Option 3️⃣ : Via CLI GitHub (Alternative)

Si tu as **GitHub CLI** installé :

```bash
# Installation (si pas installé)
choco install gh  # Windows

# Configurez l'authentification
gh auth login

# Créer la règle de protection
gh api /repos/christwadel65-ux/Windows-Cleaner/branches/master/protection \
  -X PUT \
  -f required_pull_request_reviews[dismiss_stale_reviews]=false \
  -f required_pull_request_reviews[require_code_owner_reviews]=false \
  -f required_pull_request_reviews[required_approving_review_count]=1 \
  -f enforce_admins=true \
  -f required_status_checks[strict]=true \
  -f required_status_checks[contexts][]=build
```

---

## 🚨 Dépannage

### ❌ Erreur : "Requires push access to the repository"
```
Solution: Ton token n'a pas les bons droits
- Va sur https://github.com/settings/tokens
- Edit le token et coche "repo" (complet)
```

### ❌ Erreur : "Status checks not found"
```
Solution: Les workflows ne sont pas lancés encore
- Va sur "Actions" et vérifie que security.yml a tourné
- Réessaie le script après
```

### ❌ Erreur : "Branch not found"
```
Solution: Le nom de branche est mal tapé
- Utilise : master (pas main)
- Ou change dans le script
```

---

## 📝 Résumé

| Méthode | Simplicité | Automatisé | Requis |
|---------|-----------|-----------|--------|
| PowerShell | ⭐⭐⭐ | ✅ | PAT |
| JSON Manual | ⭐⭐ | ❌ | UI GitHub |
| GitHub CLI | ⭐⭐ | ✅ | gh CLI |

**Recommandation** : **Utilise le Script PowerShell** (Option 1) 🚀

---

## 🔄 Automatiser avec Actions

Tu peux même l'exécuter automatiquement en créant un workflow :

```yaml
# .github/workflows/setup-protection.yml
name: Setup Branch Protection
on: workflow_dispatch  # Manuel seulement

jobs:
  protect:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Configure branch protection
        run: |
          curl -X PUT \
            -H "Authorization: token ${{ secrets.GITHUB_TOKEN }}" \
            -d '{"required_pull_request_reviews": {...}}' \
            https://api.github.com/repos/${{ github.repository }}/branches/master/protection
```

Besoin d'aide pour exécuter ? 👀
