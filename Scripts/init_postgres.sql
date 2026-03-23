-- Script de création de la base de données pour JeuDePoints
-- Exécutez ce script sur votre serveur PostgreSQL

-- Créer la base de données (si elle n'existe pas)
CREATE DATABASE jeu_de_points_db
    ENCODING 'UTF8'
    LOCALE 'en_US.UTF-8'
    TEMPLATE template0;

-- Se connecter à la base de données
\c jeu_de_points_db

-- Créer la table des sauvegardes
CREATE TABLE sauvegardes_jeu (
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

-- Créer les index pour optimiser les requêtes
CREATE INDEX idx_nom_sauvegarde ON sauvegardes_jeu(nom);
CREATE INDEX idx_date_sauvegarde ON sauvegardes_jeu(date_sauvegarde DESC);
CREATE INDEX idx_partie_terminee ON sauvegardes_jeu(partie_terminee);

-- Créer une vue pour les sauvegardes récentes
CREATE VIEW sauvegardes_recentes AS
SELECT * FROM sauvegardes_jeu
ORDER BY date_sauvegarde DESC
LIMIT 10;

-- Créer une vue pour les parties terminées
CREATE VIEW parties_terminees AS
SELECT
    id,
    nom,
    date_sauvegarde,
    joueur1_nom,
    joueur2_nom,
    joueur1_score,
    joueur2_score,
    joueur_gagnant_id,
    CASE
        WHEN joueur_gagnant_id = 1 THEN joueur1_nom
        WHEN joueur_gagnant_id = 2 THEN joueur2_nom
        ELSE 'Match nul'
    END AS gagnant
FROM sauvegardes_jeu
WHERE partie_terminee = TRUE
ORDER BY date_sauvegarde DESC;

-- Créer une fonction pour obtenir le résumé d'une sauvegarde
CREATE OR REPLACE FUNCTION get_sauvegarde_resume(p_id INT)
RETURNS TABLE (
    id INT,
    nom VARCHAR,
    date_sauvegarde TIMESTAMP,
    joueur1_nom VARCHAR,
    joueur2_nom VARCHAR,
    joueur1_score INT,
    joueur2_score INT,
    partie_terminee BOOLEAN,
    gagnant VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        sauvegardes_jeu.id,
        sauvegardes_jeu.nom,
        sauvegardes_jeu.date_sauvegarde,
        sauvegardes_jeu.joueur1_nom,
        sauvegardes_jeu.joueur2_nom,
        sauvegardes_jeu.joueur1_score,
        sauvegardes_jeu.joueur2_score,
        sauvegardes_jeu.partie_terminee,
        CASE
            WHEN sauvegardes_jeu.joueur_gagnant_id = 1 THEN sauvegardes_jeu.joueur1_nom
            WHEN sauvegardes_jeu.joueur_gagnant_id = 2 THEN sauvegardes_jeu.joueur2_nom
            ELSE 'En cours'
        END::VARCHAR
    FROM sauvegardes_jeu
    WHERE sauvegardes_jeu.id = p_id;
END;
$$ LANGUAGE plpgsql;

-- Créer une fonction pour supprimer les anciennes sauvegardes (plus de 30 jours)
CREATE OR REPLACE FUNCTION cleanup_anciennes_sauvegardes(p_jours INT DEFAULT 30)
RETURNS INT AS $$
DECLARE
    v_count INT;
BEGIN
    DELETE FROM sauvegardes_jeu
    WHERE date_sauvegarde < CURRENT_TIMESTAMP - INTERVAL '1 day' * p_jours
    AND partie_terminee = TRUE;

    GET DIAGNOSTICS v_count = ROW_COUNT;
    RETURN v_count;
END;
$$ LANGUAGE plpgsql;

-- Créer un trigger pour mettre à jour le timestamp
CREATE OR REPLACE FUNCTION update_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.created_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

GRANT ALL PRIVILEGES ON DATABASE jeu_de_points_db TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO postgres;

-- Afficher les informations de création
SELECT 'Base de données créée avec succès!' as status;
