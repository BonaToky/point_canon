using System;
using System.Collections.Generic;
using System.Linq;
using JeuDePoints.Models;

namespace JeuDePoints.Services
{
    public class JeuService
    {
        public class TirCanonResultat
        {
            public int TireurId { get; }
            public Position Cible { get; }

            public TirCanonResultat(int tireurId, Position cible)
            {
                TireurId = tireurId;
                Cible = cible;
            }
        }

        private PlateauService _plateau;
        private DetectionService _detection;
        private List<Joueur> _joueurs;
        private int _tourActuel;
        private List<LigneTracee> _lignesTracees;

        public JeuService(int longueur, int largeur, string nomJoueur1, string nomJoueur2)
        {
            _plateau = new PlateauService(longueur, largeur);
            _detection = new DetectionService(_plateau);
            _lignesTracees = new List<LigneTracee>();
            
            _joueurs = new List<Joueur>
            {
                new Joueur(1, nomJoueur1, 'R', System.Drawing.Color.Red),
                new Joueur(2, nomJoueur2, 'B', System.Drawing.Color.Blue)
            };
            
            _tourActuel = 0;
        }

        public PlateauService Plateau => _plateau;

        public List<Joueur> GetJoueurs()
        {
            return _joueurs;
        }

        public Joueur JoueurActuel => _joueurs[_tourActuel];

        // Retourne null si coup invalide, une liste vide si coup valide sans nouvelles lignes,
        // ou la liste des nouvelles lignes tracées.
        public List<LigneTracee>? JouerCoup(Position position)
        {
            // Vérifier si la case est disponible
            if (!_plateau.EstVide(position))
                return null;

            // Placer le point
            if (!_plateau.PlacerPoint(position, JoueurActuel))
                return null;

            // Vérifier les lignes créées
            var nouvellesLignes = _detection.VerifierNouvellesLignes(position, JoueurActuel);
            nouvellesLignes = nouvellesLignes
                .Where(ligne => !ToucheOuCroiseLigneAdverse(ligne))
                .ToList();

            if (nouvellesLignes.Any())
            {
                // Ajouter les lignes tracées et protéger les cases
                foreach (var ligne in nouvellesLignes)
                {
                    _lignesTracees.Add(ligne);
                    _plateau.ProtegerPositions(ligne.Positions);
                    JoueurActuel.Score++;
                }
                ChangerTour();
                return nouvellesLignes;
            }

            ChangerTour();
            return new List<LigneTracee>(); // Coup valide sans nouvelles lignes
        }

        private bool ToucheOuCroiseLigneAdverse(LigneTracee candidate)
        {
            if (candidate.Positions.Count < 2)
            {
                return false;
            }

            var a1 = candidate.Positions.First();
            var a2 = candidate.Positions.Last();

            foreach (var existante in _lignesTracees)
            {
                if (existante.Joueur.Id == candidate.Joueur.Id)
                {
                    continue;
                }

                if (existante.Positions.Count < 2)
                {
                    continue;
                }

                var b1 = existante.Positions.First();
                var b2 = existante.Positions.Last();
                if (SegmentsSeTouchentOuSeCroisent(a1, a2, b1, b2))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SegmentsSeTouchentOuSeCroisent(Position p1, Position p2, Position q1, Position q2)
        {
            int o1 = Orientation(p1, p2, q1);
            int o2 = Orientation(p1, p2, q2);
            int o3 = Orientation(q1, q2, p1);
            int o4 = Orientation(q1, q2, p2);

            if (o1 != o2 && o3 != o4)
            {
                return true;
            }

            if (o1 == 0 && SurSegment(p1, q1, p2)) return true;
            if (o2 == 0 && SurSegment(p1, q2, p2)) return true;
            if (o3 == 0 && SurSegment(q1, p1, q2)) return true;
            if (o4 == 0 && SurSegment(q1, p2, q2)) return true;

            return false;
        }

        private static int Orientation(Position a, Position b, Position c)
        {
            long valeur = (long)(b.Y - a.Y) * (c.X - b.X) - (long)(b.X - a.X) * (c.Y - b.Y);
            if (valeur == 0) return 0;
            return valeur > 0 ? 1 : 2;
        }

        private static bool SurSegment(Position a, Position b, Position c)
        {
            return b.X <= Math.Max(a.X, c.X) && b.X >= Math.Min(a.X, c.X)
                && b.Y <= Math.Max(a.Y, c.Y) && b.Y >= Math.Min(a.Y, c.Y);
        }

        public bool TirerCanon(Position position)
        {
            var tir = TirerCanonAvecResultat(position);
            if (tir == null)
                return false;

            return FinaliserTirCanon(tir);
        }

        public TirCanonResultat? TirerCanonAvecResultat(Position position)
        {
            int tireurId = JoueurActuel.Id;
            if (!_plateau.EstDansPlateau(position))
                return null;

            var cellule = _plateau.GetCellule(position);
            if (cellule == null || cellule.EstVide)
                return null;

            if (cellule.Proprietaire == JoueurActuel)
                return null;

            return new TirCanonResultat(tireurId, position);
        }

        public bool FinaliserTirCanon(TirCanonResultat tir)
        {
            if (tir.TireurId != JoueurActuel.Id)
                return false;

            if (!_plateau.TirerSurPointAdverse(tir.Cible, JoueurActuel))
                return false;

            var lignesSupprimees = _lignesTracees
                .Where(l => l.Positions.Any(p => p.X == tir.Cible.X && p.Y == tir.Cible.Y))
                .ToList();

            foreach (var ligne in lignesSupprimees)
            {
                ligne.Joueur.Score = Math.Max(0, ligne.Joueur.Score - 1);
                _lignesTracees.Remove(ligne);
            }

            _plateau.EffacerProtections();
            foreach (var ligne in _lignesTracees)
            {
                _plateau.ProtegerPositions(ligne.Positions);
            }

            ChangerTour();
            return true;
        }

        public bool TirerCanonSurLigne(int ligne)
        {
            var tir = TirerCanonSurLigneAvecResultat(ligne);
            if (tir == null)
                return false;

            return FinaliserTirCanon(tir);
        }

        public TirCanonResultat? TirerCanonSurLigneAvecResultat(int ligne)
        {
            if (ligne < 0 || ligne >= _plateau.Largeur)
                return null;

            int tireurId = JoueurActuel.Id;

            int startX = tireurId == 1 ? 0 : _plateau.Longueur - 1;
            int endX = tireurId == 1 ? _plateau.Longueur : -1;
            int step = tireurId == 1 ? 1 : -1;

            for (int x = startX; x != endX; x += step)
            {
                var pos = new Position(x, ligne);
                var cellule = _plateau.GetCellule(pos);
                if (cellule == null || cellule.EstVide)
                    continue;

                if (cellule.Proprietaire == JoueurActuel)
                    continue;

                return new TirCanonResultat(tireurId, pos);
            }

            return null;
        }

        private void ChangerTour()
        {
            _tourActuel = (_tourActuel + 1) % _joueurs.Count;
        }

        public void PasserTour()
        {
            ChangerTour();
        }

        public Joueur? VerifierVictoire()
        {
            // Si un joueur a au moins une ligne tracée, il a gagné
            if (_lignesTracees.Any())
            {
                return _lignesTracees.Last().Joueur;
            }
            return null;
        }

        public bool EstMatchNul()
        {
            for (int i = 0; i < _plateau.Longueur; i++)
            {
                for (int j = 0; j < _plateau.Largeur; j++)
                {
                    if (_plateau.EstVide(new Position(i, j)))
                        return false;
                }
            }
            return true;
        }

        public PlateauService GetPlateau()
        {
            return _plateau;
        }

        public List<LigneTracee> GetLignesTracees()
        {
            return _lignesTracees;
        }

        public void Reinitialiser()
        {
            _plateau.Reinitialiser();
            _lignesTracees.Clear();
            _tourActuel = 0;
        }
    }
}