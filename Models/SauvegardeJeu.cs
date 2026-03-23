using System;

namespace JeuDePoints.Models
{
    public class SauvegardeJeu
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public DateTime DateSauvegarde { get; set; }
        public string EtatJeu { get; set; } = string.Empty; // JSON sérialisé
        public int TourActuel { get; set; }
        public int JoueurActuelId { get; set; }
        public string Joueur1Nom { get; set; } = string.Empty;
        public string Joueur2Nom { get; set; } = string.Empty;
        public int Joueur1Score { get; set; }
        public int Joueur2Score { get; set; }
        public int PlateauLongueur { get; set; }
        public int PlateauLargeur { get; set; }
        public bool PartieTerminee { get; set; }
        public int? JoueurGagnantId { get; set; }

        public SauvegardeJeu() { }

        public SauvegardeJeu(
            string nom,
            string etatJeu,
            int tourActuel,
            int joueurActuelId,
            string joueur1Nom,
            string joueur2Nom,
            int joueur1Score,
            int joueur2Score,
            int plateauLongueur,
            int plateauLargeur,
            bool partieTerminee = false,
            int? joueurGagnantId = null)
        {
            Nom = nom;
            DateSauvegarde = DateTime.Now;
            EtatJeu = etatJeu;
            TourActuel = tourActuel;
            JoueurActuelId = joueurActuelId;
            Joueur1Nom = joueur1Nom;
            Joueur2Nom = joueur2Nom;
            Joueur1Score = joueur1Score;
            Joueur2Score = joueur2Score;
            PlateauLongueur = plateauLongueur;
            PlateauLargeur = plateauLargeur;
            PartieTerminee = partieTerminee;
            JoueurGagnantId = joueurGagnantId;
        }
    }
}
