namespace JeuDePoints.Models
{
    using System.Drawing;

    public class Joueur
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public char Symbole { get; set; }
        public Color Couleur { get; set; }
        public int Score { get; set; }

        public Joueur(int id, string nom, char symbole, Color couleur)
        {
            Id = id;
            Nom = nom;
            Symbole = symbole;
            Couleur = couleur;
            Score = 0;
        }
    }
}