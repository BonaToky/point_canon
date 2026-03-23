using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Npgsql;
using JeuDePoints.Models;

namespace JeuDePoints.Services
{
    public class SauvegardeService
    {
        private readonly string _connectionString;
        private const string TableName = "sauvegardes_jeu";

        public SauvegardeService(string connectionString)
        {
            _connectionString = connectionString;
            InitialiserBase();
        }

        /// <summary>
        /// Initialise la base de données et crée les tables si nécessaire
        /// </summary>
        public void InitialiserBase()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = $@"
                            CREATE TABLE IF NOT EXISTS {TableName} (
                                id SERIAL PRIMARY KEY,
                                nom VARCHAR(255) NOT NULL,
                                date_sauvegarde TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                etat_jeu TEXT NOT NULL,
                                tour_actuel INT NOT NULL,
                                joueur_actuel_id INT NOT NULL,
                                joueur1_nom VARCHAR(100) NOT NULL,
                                joueur2_nom VARCHAR(100) NOT NULL,
                                joueur1_score INT NOT NULL DEFAULT 0,
                                joueur2_score INT NOT NULL DEFAULT 0,
                                plateau_longueur INT NOT NULL,
                                plateau_largeur INT NOT NULL,
                                partie_terminee BOOLEAN NOT NULL DEFAULT FALSE,
                                joueur_gagnant_id INT,
                                created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
                            );

                            CREATE INDEX IF NOT EXISTS idx_nom_sauvegarde ON {TableName}(nom);
                            CREATE INDEX IF NOT EXISTS idx_date_sauvegarde ON {TableName}(date_sauvegarde DESC);
                        ";

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erreur lors de l'initialisation de la base de données", ex);
            }
        }

        /// <summary>
        /// Sauvegarde l'état actuel du jeu
        /// </summary>
        public SauvegardeJeu SauvegarderPartie(
            string nom,
            JeuService jeuService,
            bool partieTerminee = false,
            Joueur? joueurGagnant = null)
        {
            try
            {
                // Sérialiser l'état du jeu
                var etatJeu = SerialiserEtatJeu(jeuService);

                // Créer l'objet de sauvegarde
                var sauvegarde = new SauvegardeJeu(
                    nom: nom,
                    etatJeu: etatJeu,
                    tourActuel: jeuService.GetJoueurs().IndexOf(jeuService.JoueurActuel),
                    joueurActuelId: jeuService.JoueurActuel.Id,
                    joueur1Nom: jeuService.GetJoueurs()[0].Nom,
                    joueur2Nom: jeuService.GetJoueurs()[1].Nom,
                    joueur1Score: jeuService.GetJoueurs()[0].Score,
                    joueur2Score: jeuService.GetJoueurs()[1].Score,
                    plateauLongueur: jeuService.Plateau.Longueur,
                    plateauLargeur: jeuService.Plateau.Largeur,
                    partieTerminee: partieTerminee,
                    joueurGagnantId: joueurGagnant?.Id
                );

                // Insérer en base de données
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = $@"
                            INSERT INTO {TableName} (
                                nom, date_sauvegarde, etat_jeu, tour_actuel, joueur_actuel_id,
                                joueur1_nom, joueur2_nom, joueur1_score, joueur2_score,
                                plateau_longueur, plateau_largeur, partie_terminee, joueur_gagnant_id
                            ) VALUES (
                                @nom, @dateSauvegarde, @etatJeu, @tourActuel, @joueurActuelId,
                                @joueur1Nom, @joueur2Nom, @joueur1Score, @joueur2Score,
                                @plateauLongueur, @plateauLargeur, @partieTerminee, @joueurGagnantId
                            ) RETURNING id;
                        ";

                        command.Parameters.AddWithValue("@nom", sauvegarde.Nom);
                        command.Parameters.AddWithValue("@dateSauvegarde", sauvegarde.DateSauvegarde);
                        command.Parameters.AddWithValue("@etatJeu", sauvegarde.EtatJeu);
                        command.Parameters.AddWithValue("@tourActuel", sauvegarde.TourActuel);
                        command.Parameters.AddWithValue("@joueurActuelId", sauvegarde.JoueurActuelId);
                        command.Parameters.AddWithValue("@joueur1Nom", sauvegarde.Joueur1Nom);
                        command.Parameters.AddWithValue("@joueur2Nom", sauvegarde.Joueur2Nom);
                        command.Parameters.AddWithValue("@joueur1Score", sauvegarde.Joueur1Score);
                        command.Parameters.AddWithValue("@joueur2Score", sauvegarde.Joueur2Score);
                        command.Parameters.AddWithValue("@plateauLongueur", sauvegarde.PlateauLongueur);
                        command.Parameters.AddWithValue("@plateauLargeur", sauvegarde.PlateauLargeur);
                        command.Parameters.AddWithValue("@partieTerminee", sauvegarde.PartieTerminee);
                        command.Parameters.AddWithValue("@joueurGagnantId", sauvegarde.JoueurGagnantId ?? (object)DBNull.Value);

                        var result = command.ExecuteScalar();
                        sauvegarde.Id = (int)result!;
                    }
                }

                return sauvegarde;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erreur lors de la sauvegarde de la partie", ex);
            }
        }

        /// <summary>
        /// Récupère une sauvegarde par son ID
        /// </summary>
        public SauvegardeJeu? ChargerParId(int id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = $"SELECT * FROM {TableName} WHERE id = @id;";
                        command.Parameters.AddWithValue("@id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return CreerSauvegardeDepuisLecteur(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erreur lors du chargement de la sauvegarde", ex);
            }

            return null;
        }

        /// <summary>
        /// Récupère une sauvegarde par son nom
        /// </summary>
        public SauvegardeJeu? ChargerParNom(string nom)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = $@"
                            SELECT * FROM {TableName}
                            WHERE nom = @nom
                            ORDER BY date_sauvegarde DESC
                            LIMIT 1;
                        ";
                        command.Parameters.AddWithValue("@nom", nom);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return CreerSauvegardeDepuisLecteur(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erreur lors du chargement de la sauvegarde par nom", ex);
            }

            return null;
        }

        /// <summary>
        /// Récupère toutes les sauvegardes
        /// </summary>
        public List<SauvegardeJeu> ListerToutesSauvegardes()
        {
            var sauvegardes = new List<SauvegardeJeu>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = $@"
                            SELECT * FROM {TableName}
                            ORDER BY date_sauvegarde DESC;
                        ";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sauvegardes.Add(CreerSauvegardeDepuisLecteur(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erreur lors de la récupération des sauvegardes", ex);
            }

            return sauvegardes;
        }

        /// <summary>
        /// Récupère les sauvegardes pour une certaine date
        /// </summary>
        public List<SauvegardeJeu> ListerSauvegardesParDate(DateTime date)
        {
            var sauvegardes = new List<SauvegardeJeu>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = $@"
                            SELECT * FROM {TableName}
                            WHERE DATE(date_sauvegarde) = @date
                            ORDER BY date_sauvegarde DESC;
                        ";
                        command.Parameters.AddWithValue("@date", date.Date);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sauvegardes.Add(CreerSauvegardeDepuisLecteur(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erreur lors de la récupération des sauvegardes par date", ex);
            }

            return sauvegardes;
        }

        /// <summary>
        /// Supprime une sauvegarde par son ID
        /// </summary>
        public bool SupprimerSauvegarde(int id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = $"DELETE FROM {TableName} WHERE id = @id;";
                        command.Parameters.AddWithValue("@id", id);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erreur lors de la suppression de la sauvegarde", ex);
            }
        }

        /// <summary>
        /// Récupère l'état du jeu depuis la sauvegarde (JSON)
        /// </summary>
        public string ObtenirEtatJeu(int id)
        {
            var sauvegarde = ChargerParId(id);
            return sauvegarde?.EtatJeu ?? string.Empty;
        }

        /// <summary>
        /// Crée un objet SauvegardeJeu à partir d'un lecteur SQL
        /// </summary>
        private SauvegardeJeu CreerSauvegardeDepuisLecteur(NpgsqlDataReader reader)
        {
            return new SauvegardeJeu(
                nom: reader.GetString(reader.GetOrdinal("nom")),
                etatJeu: reader.GetString(reader.GetOrdinal("etat_jeu")),
                tourActuel: reader.GetInt32(reader.GetOrdinal("tour_actuel")),
                joueurActuelId: reader.GetInt32(reader.GetOrdinal("joueur_actuel_id")),
                joueur1Nom: reader.GetString(reader.GetOrdinal("joueur1_nom")),
                joueur2Nom: reader.GetString(reader.GetOrdinal("joueur2_nom")),
                joueur1Score: reader.GetInt32(reader.GetOrdinal("joueur1_score")),
                joueur2Score: reader.GetInt32(reader.GetOrdinal("joueur2_score")),
                plateauLongueur: reader.GetInt32(reader.GetOrdinal("plateau_longueur")),
                plateauLargeur: reader.GetInt32(reader.GetOrdinal("plateau_largeur")),
                partieTerminee: reader.GetBoolean(reader.GetOrdinal("partie_terminee")),
                joueurGagnantId: reader.IsDBNull(reader.GetOrdinal("joueur_gagnant_id"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("joueur_gagnant_id"))
            )
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                DateSauvegarde = reader.GetDateTime(reader.GetOrdinal("date_sauvegarde"))
            };
        }

        /// <summary>
        /// Sérialise l'état complet du jeu en JSON
        /// </summary>
        private string SerialiserEtatJeu(JeuService jeuService)
        {
            var plateau = jeuService.Plateau;
            var lignesTracees = jeuService.GetLignesTracees();

            var etat = new
            {
                plateau = new
                {
                    longueur = plateau.Longueur,
                    largeur = plateau.Largeur,
                    cellules = SérialiserPlateau(plateau)
                },
                lignesTracees = lignesTracees.Select(l => new
                {
                    joueurId = l.Joueur.Id,
                    joueurNom = l.Joueur.Nom,
                    direction = l.Direction,
                    positions = l.Positions.Select(p => new { x = p.X, y = p.Y }).ToList()
                }).ToList()
            };

            return JsonSerializer.Serialize(etat, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Sérialise l'état du plateau
        /// </summary>
        private List<List<object>> SérialiserPlateau(PlateauService plateau)
        {
            var cellules = new List<List<object>>();

            for (int i = 0; i < plateau.Longueur; i++)
            {
                var ligne = new List<object>();
                for (int j = 0; j < plateau.Largeur; j++)
                {
                    var cellule = plateau.GetCellule(new Position(i, j));
                    ligne.Add(new
                    {
                        estVide = cellule?.EstVide ?? true,
                        proprietaireId = cellule?.Proprietaire?.Id,
                        proprietaireNom = cellule?.Proprietaire?.Nom,
                        estProtegee = cellule?.EstProtegee ?? false
                    });
                }
                cellules.Add(ligne);
            }

            return cellules;
        }
    }
}
