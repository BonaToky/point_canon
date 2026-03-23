using System;
using System.Collections.Generic;
using JeuDePoints.Interfaces;
using JeuDePoints.Models;
using JeuDePoints.Services;

namespace JeuDePoints.Affichages
{
    public class AffichageConsole : IAffichage
    {
        public void AfficherPlateau(PlateauService plateau, List<LigneTracee> lignesTracees)
        {
            for (int y = 0; y < plateau.Largeur; y++)
            {
                for (int x = 0; x < plateau.Longueur; x++)
                {
                    var cell = plateau.GetCellule(new Position(x, y));
                    if (cell == null || cell.Proprietaire == null)
                        Console.Write(".");
                    else
                        Console.Write(cell.Proprietaire.Symbole);
                }
                Console.WriteLine();
            }
        }

        public void AfficherMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void AfficherErreur(string erreur)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(erreur);
            Console.ForegroundColor = prev;
        }

        public void AfficherVictoire(Joueur joueur)
        {
            Console.WriteLine($"Victoire de {joueur.Nom} !");
        }

        public Position DemanderPosition(int longueur, int largeur)
        {
            int x, y;
            do
            {
                Console.Write($"Entrez X (0-{longueur - 1}) : ");
            } while (!int.TryParse(Console.ReadLine(), out x) || x < 0 || x >= longueur);

            do
            {
                Console.Write($"Entrez Y (0-{largeur - 1}) : ");
            } while (!int.TryParse(Console.ReadLine(), out y) || y < 0 || y >= largeur);

            return new Position(x, y);
        }

        public void AfficherScores(List<Joueur> joueurs)
        {
            foreach (var j in joueurs)
            {
                Console.WriteLine($"{j.Nom} ({j.Symbole}) : {j.Score}");
            }
        }

        public void AfficherRegles()
        {
            Console.WriteLine("But du jeu : Aligner 5 points pour tracer une ligne.");
        }
    }
}
