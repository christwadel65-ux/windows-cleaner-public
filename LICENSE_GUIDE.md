# Système de Licence Windows Cleaner - Guide Utilisateur

## 🎯 Résumé

Windows Cleaner dispose maintenant d'un système de licence complet avec:
- ✅ Essai gratuit **7 jours** automatique
- ✅ Verrouillage par **Hardware ID** (unique par ordinateur)
- ✅ Activation de licence permanente
- ✅ Interfaces conviviales pour la gestion de licence

## 🚀 Première Utilisation

### Pour l'Utilisateur Final

1. **Lancer l'application**
   - Windows Cleaner démarre automatiquement avec une licence d'essai de 7 jours
   - Un fichier de licence est créé dans: `%AppData%\WindowsCleaner\license.dat`

2. **Voir le statut de la licence**
   - Aller dans: **Menu → Licence** (à ajouter dans MainForm)
   - Voir les jours restants avant expiration
   - Copier votre Hardware ID si nécessaire

3. **Avant l'expiration (jours 1-7)**
   - Fonctionnalité complète sans restrictions
   - Message d'avertissement à partir du jour 5

4. **Après l'expiration**
   - L'application refuse de démarrer
   - Message: "Veuillez activer une licence"

## 🔐 Activation d'une Licence

### Pour les Administrateurs

#### Étape 1: Récupérer le Hardware ID

Demander à l'utilisateur de:
1. Ouvrir Windows Cleaner
2. Aller à **Menu → Licence**
3. Copier la valeur "Hardware ID"

Exemple:
```
Hardware ID: ABC123DEF456GHI789
```

#### Étape 2: Générer une Clé de Licence

Utiliser le script PowerShell fourni:

```powershell
# Ouvrir PowerShell en tant qu'administrateur
cd scripts

# Générer une nouvelle clé
.\manage-licenses.ps1 -Action Generate

# Le script demande:
# "Entrez le Hardware ID: ABC123DEF456GHI789"
# 
# Résultat: "abcd1234efgh5678ijkl90mn"
```

**Alternative: Validation rapide**
```powershell
.\manage-licenses.ps1 -Action Validate

# Le script demande les deux valeurs et confirme si elles correspondent
```

#### Étape 3: Fournir la Clé à l'Utilisateur

Transmettre la clé de manière sécurisée (email, chat sécurisé, etc.)

### Pour l'Utilisateur - Activation

1. **Ouvrir Windows Cleaner**

2. **Accéder au Menu Licence**
   - Cliquer sur **"Licence"** dans le menu
   - Voir le statut actuel (jours restants)

3. **Cliquer sur "Activer Licence"**

4. **Entrer la Clé**
   - Coller la clé reçue de l'administrateur
   - Cliquer sur **"Activer"**

5. **Confirmation**
   - Message: "Licence activée avec succès!"
   - L'application est maintenant autorisée indéfiniment

## 📋 Fichiers Créés

### Pour les Développeurs

```
src/WindowsCleaner/
├── Core/
│   ├── HardwareIdProvider.cs      # Génération de l'ID matériel
│   └── LicenseManager.cs           # Gestion de la licence
└── UI/
    ├── LicenseForm.cs              # Affichage du statut
    └── ActivationForm.cs           # Formulaire d'activation

scripts/
└── manage-licenses.ps1             # Script d'administration

docs/
└── LICENSE_SYSTEM.md               # Documentation technique détaillée
```

### Pour l'Utilisateur

```
%AppData%/WindowsCleaner/
└── license.dat                     # Fichier de licence (JSON)
```

## 🔧 Configuration

### Durée d'Essai

Modifier dans [HardwareIdProvider.cs](../src/WindowsCleaner/Core/LicenseManager.cs#L25):

```csharp
private const int TRIAL_DAYS = 7;  // ← Changer cette valeur
```

### Clé Secrète HMAC

**⚠️ IMPORTANT POUR LA PRODUCTION:**

Avant de déployer, changer la clé secrète dans:
- `LicenseManager.cs` (C#)
- `manage-licenses.ps1` (PowerShell)

Doivent utiliser la **même clé**!

```csharp
// Dans LicenseManager.cs
var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("WindowsCleaner-2024"));
                                                  ↑
                                         Changer cette valeur
```

```powershell
# Dans manage-licenses.ps1
$SECRET_KEY = "WindowsCleaner-2024"
            ↑
  Même valeur qu'en C#
```

## 🛡️ Sécurité

### Fonctionnement

1. **Hardware ID Unique**
   - Combinaison de: CPU ID + Numéro Série Disque + Adresse MAC
   - Hash en SHA256 → 16 caractères
   - Impossible de contrefaire sans changer le matériel

2. **Clé de Licence**
   - Générée avec HMAC-SHA256
   - Liée au Hardware ID spécifique
   - Valide uniquement pour cet ordinateur

3. **Stockage Sécurisé**
   - Fichier JSON en %AppData% (accès utilisateur normal)
   - Pas de chiffrement fort (l'ID matériel est la clé)

### Limitation Connue

- Si l'utilisateur change drastiquement son matériel (changement disque dur principal, remplacement CPU, etc.), la licence devient invalide
- Solution: Générer une nouvelle clé après le changement

## 📊 Comportement par Scénario

| Scénario | Comportement |
|----------|-------------|
| **Première utilisation** | Crée license.dat, essai 7 jours, lancement OK |
| **Jour 1-4 de l'essai** | Normal, pas de message |
| **Jour 5-7 de l'essai** | Avertissement: "4 jours restants" |
| **Jour 8+** | Refuse de démarrer, propose activation |
| **Licence activée** | Lancement illimité |
| **Hardware changé** | Erreur "Hardware ID mismatch" |
| **Fichier license.dat supprimé** | Remet à zéro: nouvel essai de 7 jours |

## 🐛 Dépannage

### "Hardware ID mismatch"
**Cause**: Changement du matériel
**Solution**: Générer une nouvelle clé de licence

### "Clé de licence invalide"
**Cause**: 
- Hardware ID incorrect
- Clé générée avec une clé secrète différente

**Solution**: Vérifier que les deux clés secrètes correspondent

### "Fichier license.dat introuvable"
**Cause**: Données corrompues ou supprimées
**Solution**: Supprimer le fichier, redémarrer (nouvel essai)

## 📞 Support

### Pour les Administrateurs

- Vérifier les logs dans `%AppData%\WindowsCleaner\`
- Consulter `docs/LICENSE_SYSTEM.md` pour les détails techniques

### Pour les Utilisateurs

- Contacter l'administrateur système
- Fournir le Hardware ID pour obtenir une clé

## ✅ Checklist d'Implémentation

- [x] Classe HardwareIdProvider créée
- [x] Classe LicenseManager créée
- [x] Formulaire LicenseForm créé
- [x] Formulaire ActivationForm créé
- [x] Vérification au démarrage dans Program.cs
- [x] Script PowerShell manage-licenses.ps1
- [x] Documentation complète
- [ ] Intégration du menu "Licence" dans MainForm (À FAIRE)
- [ ] Tests utilisateur
- [ ] Génération de clés secrètes de production

## 🔄 Intégration dans MainForm

Pour ajouter le menu "Licence" dans la fenêtre principale:

```csharp
// Dans MainForm.cs
menuStrip1.Items.Add("Licence").Click += (s, e) => 
{
    LicenseManager.ShowLicenseForm(this);
};
```

---

**Version**: 1.0  
**Créé**: Janvier 2025  
**Basé sur**: Windows Cleaner v2.0+
