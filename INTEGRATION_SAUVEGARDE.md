# 🎮 Système de Sauvegarde - Résumé d'Intégration

## 📋 Fichiers créés

### Modèles
- **`Models/SauvegardeJeu.cs`** - Modèle pour stocker les informations de sauvegarde

### Services
- **`Services/SauvegardeService.cs`** - Service principal pour gérer les sauvegardes et chargements

### Configuration
- **`appsettings.json`** - Configuration PostgreSQL (à personnaliser)

### Scripts & Documentation
- **`Scripts/init_postgres.sql`** - Script d'initialisation PostgreSQL
- **`Examples/ExempleSauvegarde.cs`** - Exemple d'utilisation complet
- **`SAUVEGARDE_GUIDE.md`** - Guide détaillé

### Dépendances ajoutées
- **Npgsql 8.0.2** - Driver PostgreSQL pour C#
- **Newtonsoft.Json 13.0.3** - Sérialisation JSON

## 🚀 Étapes d'intégration

### 1️⃣ Installer PostgreSQL
```bash
# Windows: Télécharger depuis https://www.postgresql.org/download/windows/
# macOS: brew install postgresql@15
# Linux: sudo apt install postgresql postgresql-contrib
```

### 2️⃣ Configurer la base de données
```bash
# Ouvrir psql
psql -U postgres

# Exécuter le script SQL
\i 'C:/path/to/Scripts/init_postgres.sql'
```

### 3️⃣ Mettre à jour appsettings.json
```json
{
  "PostgreSQL": {
    "Host": "localhost",
    "Port": 5432,
    "Database": "jeu_de_points_db",
    "Username": "postgres",
    "Password": "VOTRE_MOT_DE_PASSE"
  }
}
```

### 4️⃣ Restaurer les dépendances NuGet
```bash
dotnet restore
```

### 5️⃣ Utiliser dans votre code

#### Dans MainForm.cs ou votre formulaire principal:
```csharp
private SauvegardeService _sauvegardeService;

public MainForm()
{
    InitializeComponent();

    // Initialiser le service de sauvegarde
    string connectionString = "Host=localhost;Port=5432;Database=jeu_de_points_db;Username=postgres;Password=..." ;
    _sauvegardeService = new SauvegardeService(connectionString);
    _sauvegardeService.InitialiserBase();
}

// Sauvegarder:
private void SauvegarderPartie(string nom)
{
    var sauvegarde = _sauvegardeService.SauvegarderPartie(nom, _jeuService);
    MessageBox.Show($"Partie sauvegardée (ID: {sauvegarde.Id})");
}

// Charger:
private void ChargerPartie(int id)
{
    var sauvegarde = _sauvegardeService.ChargerParId(id);
    if (sauvegarde != null)
    {
        // Recréer le jeu avec les paramètres sauvegardés
        _jeuService = new JeuService(
            sauvegarde.PlateauLongueur,
            sauvegarde.PlateauLargeur,
            sauvegarde.Joueur1Nom,
            sauvegarde.Joueur2Nom
        );
        MessageBox.Show("Partie chargée!");
    }
}

// Lister les sauvegardes:
private void AfficherSauvegardes()
{
    var sauvegardes = _sauvegardeService.ListerToutesSauvegardes();
    foreach (var s in sauvegardes)
    {
        Console.WriteLine($"[{s.Id}] {s.Nom} - {s.DateSauvegarde:dd/MM/yyyy HH:mm}");
    }
}
```

## 📊 Fonctionnalités principales

### Sauvegarde
- ✅ Sauvegarder l'état complet du jeu
- ✅ Stocker le plateau (10x10 par défaut)
- ✅ Conserver les scores des joueurs
- ✅ Mémoriser la date/heure de sauvegarde
- ✅ Supporter parties en cours et terminées

### Chargement
- ✅ Charger par ID
- ✅ Charger par nom
- ✅ Lister toutes les sauvegardes
- ✅ Filtrer par date
- ✅ Récupérer l'état complet du jeu

### Gestion
- ✅ Supprimer une sauvegarde
- ✅ Ajouter des index pour optimisation
- ✅ Vues SQL prédéfinies
- ✅ Fonctions SQL pour analyses

## 🗄️ Structure de données sauvegardée

### Table sauvegardes_jeu
| Champ | Type | Usage |
|-------|------|-------|
| id | SERIAL | Clé primaire |
| nom | VARCHAR(255) | Nom de la sauvegarde |
| date_sauvegarde | TIMESTAMP | Date/heure de création |
| etat_jeu | TEXT (JSON) | État complet en JSON |
| tour_actuel | INT | Numéro du tour |
| joueur_actuel_id | INT | Qui doit jouer |
| joueur1/2_nom | VARCHAR(100) | Noms des joueurs |
| joueur1/2_score | INT | Scores actuels |
| plateau_longueur/largeur | INT | Dimensions du plateau |
| partie_terminee | BOOLEAN | État de la partie |
| joueur_gagnant_id | INT | ID du gagnant |

## 🔄 Flux d'utilisation type

```
Créer un jeu → Jouer → Sauvegarder (--> Quitter)
                             ↓
                    Charger + Continuer jouer →...
```

## 📝 Exemple rapide

```csharp
// Configuration
var service = new SauvegardeService("Host=localhost;Database=jeu_de_points_db;...");

// Créer et jouer
var jeu = new JeuService(10, 10, "Alice", "Bob");
jeu.JouerCoup(new Position(0, 0));

// Sauvegarder
var save = service.SauvegarderPartie("Ma partie", jeu);

// Charger
var save2 = service.ChargerParId(save.Id);
Console.WriteLine($"Joueurs: {save2.Joueur1Nom} vs {save2.Joueur2Nom}");
Console.WriteLine($"Scores: {save2.Joueur1Score} - {save2.Joueur2Score}");
```

## ⚠️ Points importants

1. **Mot de passe PostgreSQL**: Changer dans `appsettings.json`
2. **Connection String**: S'assurer que les paramètres sont corrects
3. **Base de données**: Créer avec le script SQL fourni
4. **Dépendances**: Restaurer avec `dotnet restore`

## 🐛 Dépannage courant

| Erreur | Solution |
|--------|----------|
| "Connection refused" | Vérifier que PostgreSQL est actif |
| "password authentication failed" | Vérifier le mot de passe |
| "database does not exist" | Exécuter le script init_postgres.sql |
| "Timeout" | Augmenter ConnectionTimeout dans appsettings.json |

## 📚 Ressources

- PostgreSQL: https://www.postgresql.org/docs/
- Npgsql: https://www.npgsql.org/
- C# DateTime: https://docs.microsoft.com/en-us/dotnet/api/system.datetime

---

**Version**: 1.0
**Date**: 2024-03-23
**Auteur**: Claude Code Assistant
