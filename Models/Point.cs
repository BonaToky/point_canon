namespace JeuDePoints.Models
{
    public class Cellule
    {
        public Joueur? Proprietaire { get; set; }
        public bool EstVide => Proprietaire == null;
        public bool EstProtegee { get; set; } // Pour les lignes déjà tracées

        public Cellule()
        {
            EstProtegee = false;
        }

        public void PlacerPoint(Joueur joueur)
        {
            if (EstVide && !EstProtegee)
            {
                Proprietaire = joueur;
            }
        }
    }
}