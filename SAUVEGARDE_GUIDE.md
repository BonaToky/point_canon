# 📁 Guide de Sauvegarde du Jeu - JeuDePoints

## 🗄️ Configuration de PostgreSQL

### 1. Installation de PostgreSQL

#### Sur Windows:
1. Téléchargez PostgreSQL depuis https://www.postgresql.org/download/windows/
2. Installez PostgreSQL avec les paramètres par défaut
3. Notez votre mot de passe pour l'utilisateur `postgres`

#### Sur macOS:
```bash
brew install postgresql@15
brew services start postgresql@15
```

#### Sur Linux (Ubuntu/Debian):
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
```

### 2. Initialiser la base de données

#### Option A: Via psql (Invite de commande PostgreSQL)

1. Ouvrez `psql`:
   ```bash
   psql -U postgres
   ```

2. Exécutez le script SQL:
   ```sql
   \i 'C:/path/to/Scripts/init_postgres.sql'
   ```

#### Option B: Via pgAdmin (Interface graphique)

1. Ouvrez pgAdmin
2. Créez une nouvelle base de données nommée `jeu_de_points_db`
3. Ouvrez l'éditeur SQL et collez le contenu de `init_postgres.sql`
4. Exécutez le script

### 3. Configurer les paramètres de connexion

Modifiez `appsettings.json`:

```json
{
  "PostgreSQL": {
    "Host": "localhost",        // Adresse du serveur PostgreSQL
    "Port": 5432,               // Port PostgreSQL (5432 par défaut)
    "Database": "jeu_de_points_db",  // Nom de la base de données
    "Username": "postgres",     // Utilisateur PostgreSQL
    "Password": "votre_mot_de_passe",  // Votre mot de passe
    "ConnectionTimeout": 30
  }
}
```

## 📝 Utilisation dans le code

### Initialiser le service de sauvegarde

```csharp
using JeuDePoints.Services;
using Npgsql;

// Configuration de la chaîne de connexion
string connectionString = "Host=localhost;Port=5432;Database=jeu_de_points_db;Username=postgres;Password=votre_mot_de_passe;";

// Créer le service de sauvegarde
var sauvegardeService = new SauvegardeService(connectionString);

// Initialiser la base de données (crée les tables si nécessaire)
sauvegardeService.InitialiserBase();
```

### Sauvegarder une partie

```csharp
// Créer et jouer une partie
var jeu = new JeuService(10, 10, "Alice", "Bob");
// ... jouer le jeu ...

// Sauvegarder la partie
var sauvegarde = sauvegardeService.SauvegarderPartie(
    nom: "Partie Alice vs Bob",
    jeuService: jeu,
    partieTerminee: false
);

Console.WriteLine($"Partie sauvegardée avec ID: {sauvegarde.Id}");
```

### Sauvegarder une partie terminée

```csharp
var joueurGagnant = jeu.VerifierVictoire();

var sauvegarde = sauvegardeService.SauvegarderPartie(
    nom: "Partie Alice vs Bob (GAGNÉE)",
    jeuService: jeu,
    partieTerminee: true,
    joueurGagnant: joueurGagnant
);
```

### Charger une partie complète

```csharp
// Par ID
var sauvegarde = sauvegardeService.ChargerParId(1);

// Par nom
var sauvegarde = sauvegardeService.ChargerParNom("Partie Alice vs Bob");

// Afficher les informations
if (sauvegarde != null)
{
    Console.WriteLine($"Nom: {sauvegarde.Nom}");
    Console.WriteLine($"Sauvegardée le: {sauvegarde.DateSauvegarde}");
    Console.WriteLine($"Plateau: {sauvegarde.PlateauLongueur}x{sauvegarde.PlateauLargeur}");
    Console.WriteLine($"{sauvegarde.Joueur1Nom}: {sauvegarde.Joueur1Score}");
    Console.WriteLine($"{sauvegarde.Joueur2Nom}: {sauvegarde.Joueur2Score}");

    // État du jeu en JSON
    string etatJeu = sauvegarde.EtatJeu;
}
```

### Lister toutes les sauvegardes

```csharp
var sauvegardes = sauvegardeService.ListerToutesSauvegardes();

foreach (var sauvegarde in sauvegardes)
{
    Console.WriteLine($"[{sauvegarde.Id}] {sauvegarde.Nom} - {sauvegarde.DateSauvegarde:dd/MM/yyyy HH:mm}");
    Console.WriteLine($"  {sauvegarde.Joueur1Nom} ({sauvegarde.Joueur1Score}) vs {sauvegarde.Joueur2Nom} ({sauvegarde.Joueur2Score})");
}
```

### Lister les sauvegardes d'une date spécifique

```csharp
var sauvegardesAujourd = sauvegardeService.ListerSauvegardesParDate(DateTime.Today);

var sauvegardesHier = sauvegardeService.ListerSauvegardesParDate(DateTime.Today.AddDays(-1));
```

### Supprimer une sauvegarde

```csharp
// Par ID
bool supprimee = sauvegardeService.SupprimerSauvegarde(1);

if (supprimee)
{
    Console.WriteLine("Sauvegarde supprimée");
}
```

### Récupérer uniquement l'état du jeu (JSON)

```csharp
string etatJson = sauvegardeService.ObtenirEtatJeu(1);
Console.WriteLine(etatJson);
```

## 🗂️ Structure de la table PostgreSQL

| Colonne | Type | Description |
|---------|------|------------|
| `id` | SERIAL PRIMARY KEY | Identifiant unique |
| `nom` | VARCHAR(255) | Nom de la sauvegarde |
| `date_sauvegarde` | TIMESTAMP | Date et heure de la sauvegarde |
| `etat_jeu` | TEXT | État complet du jeu en JSON |
| `tour_actuel` | INT | Numéro du tour |
| `joueur_actuel_id` | INT | ID du joueur actuel |
| `joueur1_nom` | VARCHAR(100) | Nom du premier joueur |
| `joueur2_nom` | VARCHAR(100) | Nom du deuxième joueur |
| `joueur1_score` | INT | Score du joueur 1 |
| `joueur2_score` | INT | Score du joueur 2 |
| `plateau_longueur` | INT | Longueur du plateau |
| `plateau_largeur` | INT | Largeur du plateau |
| `partie_terminee` | BOOLEAN | Partie terminée ou non |
| `joueur_gagnant_id` | INT (nullable) | ID du gagnant (si partie terminée) |
| `created_at` | TIMESTAMP | Timestamp de création |

## 🔍 Requêtes SQL utiles

### Voir les 10 dernières sauvegardes
```sql
SELECT * FROM sauvegardes_recentes;
```

### Voir toutes les parties terminées
```sql
SELECT * FROM parties_terminees;
```

### Compter les sauvegardes d'un jour
```sql
SELECT COUNT(*) FROM sauvegardes_jeu
WHERE DATE(date_sauvegarde) = '2024-03-23';
```

### Supprimer les sauvegardes de plus de 30 jours
```sql
SELECT cleanup_anciennes_sauvegardes(30);
```

### Obtenir les résumé d'une sauvegarde spécifique
```sql
SELECT * FROM get_sauvegarde_resume(1);
```

## 🛠️ Dépannage

### Erreur: "Connection refused"
- Vérifiez que PostgreSQL est en cours d'exécution
- Vérifiez l'adresse Host et le Port dans `appsettings.json`
- Assurez-vous que le port 5432 n'est pas bloqué par un firewall

### Erreur: "password authentication failed"
- Vérifiez votre mot de passe dans `appsettings.json`
- Réinitialisez le mot de passe PostgreSQL si oublié

### Erreur: "database does not exist"
- Assurez-vous que le script SQL a été exécuté correctement
- Vérifiez le nom de la base de données: `jeu_de_points_db`
- Exécutez `sauvegardeService.InitialiserBase()` dans votre code

## 📊 Statistiques

Pour obtenir des statistiques sur les sauvegardes:

```sql
-- Nombre total de sauvegardes
SELECT COUNT(*) as total_sauvegardes FROM sauvegardes_jeu;

-- Nombre de parties terminées
SELECT COUNT(*) as parties_terminees FROM sauvegardes_jeu WHERE partie_terminee = TRUE;

-- Parties par joueur
SELECT joueur1_nom, COUNT(*) as parties FROM sauvegardes_jeu GROUP BY joueur1_nom;

-- Score moyen par joueur
SELECT
    joueur1_nom,
    AVG(joueur1_score) as score_moyen,
    MAX(joueur1_score) as meilleur_score
FROM sauvegardes_jeu
GROUP BY joueur1_nom;
```

## 💾 Sauvegarde de la base PostgreSQL

### Backup complet
```bash
pg_dump -U postgres -d jeu_de_points_db -f backup.sql
```

### Restore depuis un backup
```bash
psql -U postgres -d jeu_de_points_db -f backup.sql
```

---

**Besoin d'aide ?** Consultez la documentation PostgreSQL: https://www.postgresql.org/docs/
