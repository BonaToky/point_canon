using System;
using System.Collections.Generic;
using JeuDePoints.Models;

namespace JeuDePoints.Services
{
    public class DetectionService
    {
        private PlateauService _plateau;

        public DetectionService(PlateauService plateau)
        {
            _plateau = plateau;
        }

        // Vérifie si un point placé crée une ligne de 5 points consécutifs
        public List<LigneTracee> VerifierNouvellesLignes(Position dernierCoup, Joueur joueur)
        {
            var lignesTrouvees = new List<LigneTracee>();

            // Vérifier les 4 directions
            lignesTrouvees.AddRange(VerifierDirection(dernierCoup, joueur, 1, 0, "Horizontal"));    // Horizontal
            lignesTrouvees.AddRange(VerifierDirection(dernierCoup, joueur, 0, 1, "Vertical"));      // Vertical
            lignesTrouvees.AddRange(VerifierDirection(dernierCoup, joueur, 1, 1, "Diagonal \\"));   // Diagonal descendante
            lignesTrouvees.AddRange(VerifierDirection(dernierCoup, joueur, 1, -1, "Diagonal /"));   // Diagonal montante

            // Eviter les doublons de lignes (mêmes 5 positions) détectées via plusieurs chemins.
            var uniques = new List<LigneTracee>();
            foreach (var ligne in lignesTrouvees)
            {
                bool existe = uniques.Any(u => u.Positions.Count == ligne.Positions.Count
                    && u.Positions.Zip(ligne.Positions, (a, b) => a.X == b.X && a.Y == b.Y).All(eq => eq));

                if (!existe)
                {
                    uniques.Add(ligne);
                }
            }

            return uniques;
        }

        private List<LigneTracee> VerifierDirection(Position start, Joueur joueur, int dx, int dy, string nomDirection)
        {
            var lignes = new List<LigneTracee>();
            var pointsAlignes = new List<Position> { start };

            // Chercher dans la direction positive
            int i = 1;
            while (true)
            {
                Position pos = new Position(start.X + (dx * i), start.Y + (dy * i));
                if (!_plateau.EstDansPlateau(pos))
                    break;
                    
                var proprio = _plateau.GetProprietaire(pos);
                if (proprio == joueur)
                {
                    pointsAlignes.Add(pos);
                }
                else
                {
                    break; // Coupure par un point adverse ou case vide
                }

                i++;
            }

            // Chercher dans la direction négative
            i = 1;
            while (true)
            {
                Position pos = new Position(start.X - (dx * i), start.Y - (dy * i));
                if (!_plateau.EstDansPlateau(pos))
                    break;
                    
                var proprio = _plateau.GetProprietaire(pos);
                if (proprio == joueur)
                {
                    pointsAlignes.Insert(0, pos);
                }
                else
                {
                    break; // Coupure par un point adverse ou case vide
                }

                i++;
            }

            // Si on a au moins 5 points consécutifs, on peut avoir plusieurs lignes
            if (pointsAlignes.Count >= 5)
            {
                // Extraire toutes les lignes de 5 points consécutifs
                for (int index = 0; index <= pointsAlignes.Count - 5; index++)
                {
                    var ligne = pointsAlignes.GetRange(index, 5);
                    lignes.Add(new LigneTracee(joueur, ligne, nomDirection));
                }
            }

            return lignes;
        }

        // Vérifie si une ligne est coupée par des points adverses
        public bool EstLigneValide(List<Position> positions, Joueur joueur)
        {
            foreach (var pos in positions)
            {
                var proprio = _plateau.GetProprietaire(pos);
                if (proprio != joueur)
                    return false;
            }
            return true;
        }
    }
}