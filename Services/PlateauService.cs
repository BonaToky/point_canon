using System;
using JeuDePoints.Models;

namespace JeuDePoints.Services
{
    public class PlateauService
    {
        private Cellule[,] _grille;
        public int Longueur { get; private set; }
        public int Largeur { get; private set; }

        public PlateauService(int longueur, int largeur)
        {
            if (longueur < 5 || largeur < 5)
                throw new ArgumentException("Le plateau doit faire au moins 5x5 pour pouvoir aligner 5 points");

            Longueur = longueur;
            Largeur = largeur;
            _grille = new Cellule[longueur, largeur];
            
            for (int i = 0; i < longueur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    _grille[i, j] = new Cellule();
                }
            }
        }

        public bool PlacerPoint(Position pos, Joueur joueur)
        {
            if (!EstDansPlateau(pos))
                return false;

            if (!_grille[pos.X, pos.Y].EstVide)
                return false;

            if (_grille[pos.X, pos.Y].EstProtegee)
                return false;

            _grille[pos.X, pos.Y].PlacerPoint(joueur);
            return true;
        }

        public bool TirerSurPointAdverse(Position pos, Joueur tireur)
        {
            if (!EstDansPlateau(pos))
                return false;

            var cellule = _grille[pos.X, pos.Y];
            if (cellule.EstVide)
                return false;

            if (cellule.Proprietaire == tireur)
                return false;

            cellule.Proprietaire = null;
            cellule.EstProtegee = false;
            return true;
        }

        public bool EstDansPlateau(Position pos)
        {
            return pos.X >= 0 && pos.X < Longueur && pos.Y >= 0 && pos.Y < Largeur;
        }

        public Joueur GetProprietaire(Position pos)
        {
            if (!EstDansPlateau(pos))
                return null;
            return _grille[pos.X, pos.Y].Proprietaire;
        }

        public bool EstVide(Position pos)
        {
            if (!EstDansPlateau(pos))
                return false;
            return _grille[pos.X, pos.Y].EstVide;
        }

        public void ProtegerPositions(System.Collections.Generic.List<Position> positions)
        {
            foreach (var pos in positions)
            {
                if (EstDansPlateau(pos))
                {
                    _grille[pos.X, pos.Y].EstProtegee = true;
                }
            }
        }

        public void EffacerProtections()
        {
            for (int i = 0; i < Longueur; i++)
            {
                for (int j = 0; j < Largeur; j++)
                {
                    _grille[i, j].EstProtegee = false;
                }
            }
        }

        public Cellule GetCellule(Position pos)
        {
            if (!EstDansPlateau(pos))
                return null;
            return _grille[pos.X, pos.Y];
        }

        public void Reinitialiser()
        {
            for (int i = 0; i < Longueur; i++)
            {
                for (int j = 0; j < Largeur; j++)
                {
                    _grille[i, j] = new Cellule();
                }
            }
        }
    }
}