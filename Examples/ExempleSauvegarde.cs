using System;
using JeuDePoints.Models;
using JeuDePoints.Services;

namespace JeuDePoints.Examples
{
    /// <summary>
    /// Exemple d'utilisation du service de sauvegarde
    /// </summary>
    public class ExempleSauvegarde
    {
        public static void Executer()
        {
            // Configuration de la chaîne de connexion
            string connectionString = "Host=localhost;Port=5432;Database=jeu_de_points_db;Username=postgres;Password=votre_mot_de_passe;";

            try
            {
                // 1. Créer le service de sauvegarde
                var sauvegardeService = new SauvegardeService(connectionString);
                Console.WriteLine("✓ Service de sauvegarde initialisé");

                // 2. Initialiser la base de données (crée les tables si nécessaire)
                sauvegardeService.InitialiserBase();
                Console.WriteLine("✓ Base de données initialisée");

                // 3. Créer une nouvelle partie
                var jeu = new JeuService(10, 10, "Alice", "Bob");
                Console.WriteLine("✓ Nouvelle partie créée (Alice vs Bob)");

                // 4. Jouer quelques coups
                // Exemple: Alice placé un point en (0, 0)
                var resultat = jeu.JouerCoup(new Position(0, 0));
                Console.WriteLine($"  - Alice joue en (0, 0): {(resultat != null ? "Succès" : "Invalide")}");

                // Exemple: Bob placé un point en (1, 1)
                resultat = jeu.JouerCoup(new Position(1, 1));
                Console.WriteLine($"  - Bob joue en (1, 1): {(resultat != null ? "Succès" : "Invalide")}");

                // 5. Sauvegarder la partie en cours
                var sauvegarde = sauvegardeService.SauvegarderPartie(
                    nom: "Partie Alice vs Bob - Sauvegarde intermédiaire",
                    jeuService: jeu,
                    partieTerminee: false
                );
                Console.WriteLine($"✓ Partie sauvegardée (ID: {sauvegarde.Id})");

                // 6. Continuer à jouer
                resultat = jeu.JouerCoup(new Position(2, 2));
                resultat = jeu.JouerCoup(new Position(3, 3));

                // 7. Vérifier s'il y a un gagnant
                var gagnant = jeu.VerifierVictoire();

                if (gagnant != null)
                {
                    Console.WriteLine($"✓ Gagnant: {gagnant.Nom}!");
                    // Sauvegarder la partie terminée
                    sauvegarde = sauvegardeService.SauvegarderPartie(
                        nom: "Partie Alice vs Bob - TERMINÉE",
                        jeuService: jeu,
                        partieTerminee: true,
                        joueurGagnant: gagnant
                    );
                }
                else
                {
                    // Sauvegarder la partie finale
                    sauvegarde = sauvegardeService.SauvegarderPartie(
                        nom: "Partie Alice vs Bob - Finale",
                        jeuService: jeu,
                        partieTerminee: true
                    );
                }

                // 8. Lister toutes les sauvegardes
                Console.WriteLine("\n--- Sauvegardes disponibles ---");
                var allSauvegardes = sauvegardeService.ListerToutesSauvegardes();
                foreach (var s in allSauvegardes)
                {
                    Console.WriteLine($"[{s.Id}] {s.Nom}");
                    Console.WriteLine($"    Sauvegardée: {s.DateSauvegarde:dd/MM/yyyy HH:mm:ss}");
                    Console.WriteLine($"    {s.Joueur1Nom} ({s.Joueur1Score}) vs {s.Joueur2Nom} ({s.Joueur2Score})");
                    Console.WriteLine($"    Plateau: {s.PlateauLongueur}x{s.PlateauLargeur}");
                    if (s.PartieTerminee)
                    {
                        Console.WriteLine($"    Status: TERMINÉE");
                        if (s.JoueurGagnantId.HasValue)
                        {
                            string nomGagnant = s.JoueurGagnantId == 1 ? s.Joueur1Nom : s.Joueur2Nom;
                            Console.WriteLine($"    Gagnant: {nomGagnant}");
                        }
                    }
                    Console.WriteLine();
                }

                // 9. Charger une sauvegarde spécifique
                Console.WriteLine("\n--- Chargement d'une sauvegarde ---");
                var sauvegarde_chargee = sauvegardeService.ChargerParId(1);
                if (sauvegarde_chargee != null)
                {
                    Console.WriteLine($"Sauvegarde chargée: {sauvegarde_chargee.Nom}");
                    Console.WriteLine($"État du jeu (JSON):\n{sauvegarde_chargee.EtatJeu}");
                }

                // 10. Lister les sauvegardes d'une date spécifique
                Console.WriteLine("\n--- Sauvegardes d'aujourd'hui ---");
                var sauvegardesAujourd = sauvegardeService.ListerSauvegardesParDate(DateTime.Today);
                Console.WriteLine($"Nombre de sauvegardes: {sauvegardesAujourd.Count}");

                // 11. Supprimer une sauvegarde (optionnel)
                // Console.WriteLine("\n--- Suppression d'une sauvegarde ---");
                // bool supprimee = sauvegardeService.SupprimerSauvegarde(1);
                // Console.WriteLine($"Sauvegarde supprimée: {supprimee}");

                Console.WriteLine("\n✓ Exemple terminé avec succès!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Erreur: {ex.Message}");
                Console.WriteLine($"Détails: {ex.InnerException?.Message}");
            }
        }
    }
}
