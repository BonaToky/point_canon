using JeuDePoints.Models;
using JeuDePoints.Services;

namespace JeuDePoints.Interfaces
{
    public interface IAffichage
    {
        void AfficherPlateau(PlateauService plateau, List<LigneTracee> lignesTracees);
        void AfficherMessage(string message);
        void AfficherErreur(string erreur);
        void AfficherVictoire(Joueur joueur);
        Position DemanderPosition(int longueur, int largeur);
        void AfficherScores(List<Joueur> joueurs);
        void AfficherRegles();
    }
}