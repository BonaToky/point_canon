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
        private Dictionary<int, HashSet<Position>> _casesPerduesParJoueur;

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

            _casesPerduesParJoueur = new Dictionary<int, HashSet<Position>>
            {
                { _joueurs[0].Id, new HashSet<Position>() },
                { _joueurs[1].Id, new HashSet<Position>() }
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
                .Where(ligne => !_lignesTracees.Any(existante => LignesIdentiques(existante, ligne)))
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

        private static bool LignesIdentiques(LigneTracee a, LigneTracee b)
        {
            if (a.Joueur.Id != b.Joueur.Id || a.Positions.Count != b.Positions.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Positions.Count; i++)
            {
                if (a.Positions[i].X != b.Positions[i].X || a.Positions[i].Y != b.Positions[i].Y)
                {
                    return false;
                }
            }

            return true;
        }

        private double CalculerPorteeCanonReelle(int puissance)
        {
            int puissanceNormalisee = Math.Clamp(puissance, 1, 9);
            int distanceMax = Math.Max(0, _plateau.Longueur - 1);
            return ((double)(puissanceNormalisee - 1) / 8.0) * distanceMax;
        }

        private int CalculerPorteeCanon(int puissance)
        {
            return (int)Math.Floor(CalculerPorteeCanonReelle(puissance));
        }

        public Position CalculerDestinationCanonVers(Position positionVisee, int puissance = 9)
        {
            int tireurId = JoueurActuel.Id;
            int ligne = Math.Clamp(positionVisee.Y, 0, _plateau.Largeur - 1);
            int portee = CalculerPorteeCanon(puissance);

            int startX = tireurId == 1 ? 0 : _plateau.Longueur - 1;
            int limiteIncluse = tireurId == 1
                ? Math.Min(_plateau.Longueur - 1, startX + portee)
                : Math.Max(0, startX - portee);

            int xDestination = tireurId == 1
                ? Math.Clamp(positionVisee.X, startX, limiteIncluse)
                : Math.Clamp(positionVisee.X, limiteIncluse, startX);

            return new Position(xDestination, ligne);
        }

        public Position CalculerDestinationCanonSurLigne(int ligne, int puissance = 9)
        {
            int tireurId = JoueurActuel.Id;
            int ligneValide = Math.Clamp(ligne, 0, _plateau.Largeur - 1);
            int portee = CalculerPorteeCanon(puissance);

            int startX = tireurId == 1 ? 0 : _plateau.Longueur - 1;
            int limiteIncluse = tireurId == 1
                ? Math.Min(_plateau.Longueur - 1, startX + portee)
                : Math.Max(0, startX - portee);

            return new Position(limiteIncluse, ligneValide);
        }

        public bool TirerCanon(Position position, int puissance = 9)
        {
            var tir = TirerCanonAvecResultat(position, puissance);
            if (tir == null)
                return false;

            return FinaliserTirCanon(tir);
        }

        public TirCanonResultat? TirerCanonAvecResultat(Position position, int puissance = 9)
        {
            int tireurId = JoueurActuel.Id;
            if (!_plateau.EstDansPlateau(position))
                return null;

            double porteeReelle = CalculerPorteeCanonReelle(puissance);
            int startX = tireurId == 1 ? 0 : _plateau.Longueur - 1;
            int distance = Math.Abs(position.X - startX);
            if (distance > porteeReelle)
                return null;

            var cellule = _plateau.GetCellule(position);
            if (cellule == null)
                return null;

            // Les points appartenant a une ligne alignee/protegee sont intouchables.
            if (cellule.EstProtegee)
                return null;

            if (cellule.Proprietaire == JoueurActuel)
                return null;

            bool caseRecuperable = EstCaseRecuperablePourJoueur(JoueurActuel.Id, position);
            if (cellule.EstVide && !caseRecuperable)
                return null;

            return new TirCanonResultat(tireurId, position);
        }

        public bool FinaliserTirCanon(TirCanonResultat tir)
        {
            if (tir.TireurId != JoueurActuel.Id)
                return false;

            var celluleCible = _plateau.GetCellule(tir.Cible);
            if (celluleCible == null)
                return false;

            if (celluleCible.EstProtegee)
                return false;

            if (celluleCible.Proprietaire == JoueurActuel)
                return false;

            bool caseRecuperable = EstCaseRecuperablePourJoueur(JoueurActuel.Id, tir.Cible);
            var proprietaireAvant = celluleCible.Proprietaire;

            if (proprietaireAvant == null)
            {
                if (!caseRecuperable)
                    return false;

                celluleCible.Proprietaire = JoueurActuel;
            }
            else
            {
                if (caseRecuperable)
                {
                    // Reprise de case: un point adverse sur la case perdue devient le point du tireur.
                    celluleCible.Proprietaire = JoueurActuel;
                }
                else
                {
                    // Tir standard: suppression du point adverse et mémorisation de la case perdue.
                    celluleCible.Proprietaire = null;
                    MemoriserCasePerdue(proprietaireAvant.Id, tir.Cible);
                }
            }

            celluleCible.EstProtegee = false;
            if (caseRecuperable)
            {
                _casesPerduesParJoueur[JoueurActuel.Id].Remove(tir.Cible);
            }

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

        public bool TirerCanonSurLigne(int ligne, int puissance = 9)
        {
            var tir = TirerCanonSurLigneAvecResultat(ligne, puissance);
            if (tir == null)
                return false;

            return FinaliserTirCanon(tir);
        }

        public TirCanonResultat? TirerCanonSurLigneAvecResultat(int ligne, int puissance = 9)
        {
            if (ligne < 0 || ligne >= _plateau.Largeur)
                return null;

            int tireurId = JoueurActuel.Id;
            int portee = CalculerPorteeCanon(puissance);

            int startX = tireurId == 1 ? 0 : _plateau.Longueur - 1;
            int limiteIncluse = tireurId == 1
                ? Math.Min(_plateau.Longueur - 1, startX + portee)
                : Math.Max(0, startX - portee);

            // Logique mortier: impact uniquement sur la case de portée, sans collision intermédiaire.
            var impact = new Position(limiteIncluse, ligne);
            var cellule = _plateau.GetCellule(impact);
            if (cellule == null)
                return null;

            if (cellule.EstProtegee)
                return null;

            if (cellule.Proprietaire == JoueurActuel)
                return null;

            bool caseRecuperable = EstCaseRecuperablePourJoueur(JoueurActuel.Id, impact);
            if (cellule.EstVide && !caseRecuperable)
                return null;

            return new TirCanonResultat(tireurId, impact);
        }

        private bool EstCaseRecuperablePourJoueur(int joueurId, Position position)
        {
            if (!_casesPerduesParJoueur.TryGetValue(joueurId, out var casesPerdues))
            {
                return false;
            }

            return casesPerdues.Contains(position);
        }

        private void MemoriserCasePerdue(int joueurId, Position position)
        {
            if (!_casesPerduesParJoueur.TryGetValue(joueurId, out var casesPerdues))
            {
                casesPerdues = new HashSet<Position>();
                _casesPerduesParJoueur[joueurId] = casesPerdues;
            }

            casesPerdues.Add(new Position(position.X, position.Y));
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

        public Dictionary<int, List<Position>> GetCasesPerduesParJoueur()
        {
            return _casesPerduesParJoueur.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(p => new Position(p.X, p.Y)).ToList());
        }

        public void DefinirCasesPerduesParJoueur(Dictionary<int, List<Position>> donnees)
        {
            _casesPerduesParJoueur.Clear();

            foreach (var joueur in _joueurs)
            {
                _casesPerduesParJoueur[joueur.Id] = new HashSet<Position>();
            }

            foreach (var entree in donnees)
            {
                if (!_casesPerduesParJoueur.ContainsKey(entree.Key))
                {
                    continue;
                }

                foreach (var position in entree.Value)
                {
                    _casesPerduesParJoueur[entree.Key].Add(new Position(position.X, position.Y));
                }
            }
        }

        public void Reinitialiser()
        {
            _plateau.Reinitialiser();
            _lignesTracees.Clear();
            foreach (var casesPerdues in _casesPerduesParJoueur.Values)
            {
                casesPerdues.Clear();
            }
            _tourActuel = 0;
        }
    }
}