using System.Collections.Generic;

namespace JeuDePoints.Models
{
    public class LigneTracee
    {
        public Joueur Joueur { get; set; }
        public List<Position> Positions { get; set; }
        public string Direction { get; set; } // "Horizontal", "Vertical", "Diagonal"

        public LigneTracee(Joueur joueur, List<Position> positions, string direction)
        {
            Joueur = joueur;
            Positions = positions;
            Direction = direction;
        }
    }
}