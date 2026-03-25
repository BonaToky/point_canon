using System;
using System.Drawing;
using System.Windows.Forms;
using JeuDePoints.Models;
using JeuDePoints.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace JeuDePoints.Forms
{
    public partial class MainForm : Form
    {
        private enum ModeAction
        {
            PlacerPoint,
            TirerCanon
        }

        private JeuService? _jeu;
        private PlateauControl _plateauControl = null!;
        private Label _labelTour = null!;
        private Label _labelMatricule = null!;
        private Label _labelScoreJ1 = null!;
        private Label _labelScoreJ2 = null!;
        private Label _labelModeAction = null!;
        private Button _btnNouvellePartie = null!;
        private Button _btnSauvegarder = null!;
        private Button _btnCharger = null!;
        private Button _btnModePlacer = null!;
        private Button _btnModeTirer = null!;
        private Label _labelPuissance = null!;
        private Label _labelPorteeChoisie = null!;
        private NumericUpDown _numPuissanceCanon = null!;
        private Label _labelTaillePlateau = null!;
        private NumericUpDown _numLongueurJeu = null!;
        private NumericUpDown _numLargeurJeu = null!;
        private Button _btnAppliquerTaille = null!;
        private SauvegardeService? _sauvegardeService;
        private Panel _infoPanel = null!;
        private Panel _setupPanel = null!;
        private NumericUpDown _numLongueur = null!;
        private NumericUpDown _numLargeur = null!;
        private ModeAction _modeAction = ModeAction.PlacerPoint;
        private bool _tirEnCours;

        public MainForm()
        {
            SetupUI();
            AfficherConfiguration();
        }

        private void SetupUI()
        {
            this.Text = "Jeu de Points - Alignez 5 !";
            this.Size = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;

            // Panel d'information
            _infoPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = Color.FromArgb(201, 236, 255)
            };

            // Label du tour
            _labelTour = new Label
            {
                Location = new Point(16, 20),
                Size = new Size(198, 54),
                Font = new Font("Arial", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 73, 120),
                Text = "Tour du Joueur Rouge (R)"
            };

            _labelMatricule = new Label
            {
                Location = new Point(16, 76),
                Size = new Size(198, 38),
                Font = new Font("Arial", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 120, 190),
                Text = "ETU3321"
            };

            // Labels des scores
            _labelScoreJ1 = new Label
            {
                Location = new Point(16, 140),
                Size = new Size(198, 30),
                Font = new Font("Arial", 12),
                ForeColor = Color.FromArgb(18, 73, 120),
                Text = "Rouge: 0"
            };

            _labelScoreJ2 = new Label
            {
                Location = new Point(16, 174),
                Size = new Size(198, 30),
                Font = new Font("Arial", 12),
                ForeColor = Color.FromArgb(18, 73, 120),
                Text = "Bleu: 0"
            };

            _labelModeAction = new Label
            {
                Location = new Point(16, 214),
                Size = new Size(198, 44),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 73, 120),
                Text = "Action: Placer"
            };

            _btnModePlacer = new Button
            {
                Location = new Point(16, 266),
                Size = new Size(198, 34),
                Text = "Placer un point",
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(232, 247, 255),
                ForeColor = Color.FromArgb(18, 73, 120),
                FlatStyle = FlatStyle.Flat
            };
            _btnModePlacer.Click += (s, e) =>
            {
                _modeAction = ModeAction.PlacerPoint;
                MettreAJourBoutonsAction();
            };

            _btnModeTirer = new Button
            {
                Location = new Point(16, 306),
                Size = new Size(198, 34),
                Text = "Tirer au canon",
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(232, 247, 255),
                ForeColor = Color.FromArgb(18, 73, 120),
                FlatStyle = FlatStyle.Flat
            };
            _btnModeTirer.Click += (s, e) =>
            {
                _modeAction = ModeAction.TirerCanon;
                MettreAJourBoutonsAction();
            };

            _labelPuissance = new Label
            {
                Location = new Point(16, 354),
                Size = new Size(198, 22),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 73, 120),
                Text = "Portée canon (Ctrl+1..9):"
            };

            _numPuissanceCanon = new NumericUpDown
            {
                Location = new Point(16, 378),
                Size = new Size(198, 28),
                Minimum = 1,
                Maximum = 9,
                Value = 9,
                Font = new Font("Arial", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(230, 230, 230),
                ForeColor = Color.Black
            };
            _numPuissanceCanon.Visible = false;
            _numPuissanceCanon.TabStop = false;
            _numPuissanceCanon.ValueChanged += (s, e) => MettreAJourLabelPorteeChoisie();

            _labelPorteeChoisie = new Label
            {
                Location = new Point(16, 380),
                Size = new Size(198, 22),
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 120, 190),
                Text = "Portée choisie: 9"
            };

            // Bouton nouvelle partie
            _btnNouvellePartie = new Button
            {
                Location = new Point(16, 412),
                Size = new Size(198, 40),
                Text = "Nouvelle Partie",
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(222, 243, 255),
                ForeColor = Color.FromArgb(18, 73, 120),
                FlatStyle = FlatStyle.Flat
            };
            _btnNouvellePartie.Click += BtnNouvellePartie_Click;

            _btnSauvegarder = new Button
            {
                Location = new Point(16, 460),
                Size = new Size(198, 36),
                Text = "Sauvegarder partie",
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(209, 237, 255),
                ForeColor = Color.FromArgb(18, 73, 120),
                FlatStyle = FlatStyle.Flat
            };
            _btnSauvegarder.Click += BtnSauvegarder_Click;

            _btnCharger = new Button
            {
                Location = new Point(16, 502),
                Size = new Size(198, 36),
                Text = "Charger partie",
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(198, 232, 252),
                ForeColor = Color.FromArgb(18, 73, 120),
                FlatStyle = FlatStyle.Flat
            };
            _btnCharger.Click += BtnCharger_Click;

            _labelTaillePlateau = new Label
            {
                Location = new Point(16, 546),
                Size = new Size(198, 20),
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 73, 120),
                Text = "Taille plateau (en jeu):"
            };

            _numLongueurJeu = new NumericUpDown
            {
                Location = new Point(16, 570),
                Size = new Size(92, 26),
                Minimum = 5,
                Maximum = 30,
                Value = 10,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            _numLargeurJeu = new NumericUpDown
            {
                Location = new Point(122, 570),
                Size = new Size(92, 26),
                Minimum = 5,
                Maximum = 30,
                Value = 10,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            _btnAppliquerTaille = new Button
            {
                Location = new Point(16, 602),
                Size = new Size(198, 32),
                Text = "Appliquer taille",
                Font = new Font("Arial", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(186, 225, 248),
                ForeColor = Color.FromArgb(18, 73, 120),
                FlatStyle = FlatStyle.Flat
            };
            _btnAppliquerTaille.Click += BtnAppliquerTaille_Click;

            _infoPanel.Controls.Add(_labelTour);
            _infoPanel.Controls.Add(_labelMatricule);
            _infoPanel.Controls.Add(_labelScoreJ1);
            _infoPanel.Controls.Add(_labelScoreJ2);
            _infoPanel.Controls.Add(_labelModeAction);
            _infoPanel.Controls.Add(_btnModePlacer);
            _infoPanel.Controls.Add(_btnModeTirer);
            _infoPanel.Controls.Add(_labelPuissance);
            _infoPanel.Controls.Add(_labelPorteeChoisie);
            _infoPanel.Controls.Add(_btnNouvellePartie);
            _infoPanel.Controls.Add(_btnSauvegarder);
            _infoPanel.Controls.Add(_btnCharger);
            _infoPanel.Controls.Add(_labelTaillePlateau);
            _infoPanel.Controls.Add(_numLongueurJeu);
            _infoPanel.Controls.Add(_numLargeurJeu);
            _infoPanel.Controls.Add(_btnAppliquerTaille);

            MettreAJourLabelPorteeChoisie();

            // Panel du plateau (custom control with intersections)
            _plateauControl = new PlateauControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(20)
            };
            _plateauControl.IntersectionClicked += OnIntersectionClicked;

            this.Controls.Add(_plateauControl);
            this.Controls.Add(_infoPanel);

            // Panel de configuration intégré (même fenêtre)
            _setupPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(239, 249, 255)
            };

            var card = new Panel
            {
                Size = new Size(430, 220),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            card.Location = new Point(
                (this.ClientSize.Width - card.Width) / 2,
                (this.ClientSize.Height - card.Height) / 2);

            card.Anchor = AnchorStyles.None;

            var titre = new Label
            {
                Text = "Configuration du plateau",
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 18),
                ForeColor = Color.FromArgb(18, 73, 120)
            };

            var lblLongueur = new Label
            {
                Text = "Longueur :",
                Location = new Point(24, 76),
                Size = new Size(120, 28),
                Font = new Font("Arial", 11, FontStyle.Regular)
            };

            _numLongueur = new NumericUpDown
            {
                Location = new Point(160, 76),
                Size = new Size(120, 28),
                Minimum = 5,
                Maximum = 30,
                Value = 10,
                Font = new Font("Arial", 11, FontStyle.Bold)
            };

            var lblLargeur = new Label
            {
                Text = "Largeur :",
                Location = new Point(24, 116),
                Size = new Size(120, 28),
                Font = new Font("Arial", 11, FontStyle.Regular)
            };

            _numLargeur = new NumericUpDown
            {
                Location = new Point(160, 116),
                Size = new Size(120, 28),
                Minimum = 5,
                Maximum = 30,
                Value = 10,
                Font = new Font("Arial", 11, FontStyle.Bold)
            };

            var btnValider = new Button
            {
                Text = "Valider et lancer",
                Location = new Point(24, 162),
                Size = new Size(256, 36),
                BackColor = Color.FromArgb(186, 225, 248),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnValider.Click += BtnValiderConfiguration_Click;

            card.Controls.Add(titre);
            card.Controls.Add(lblLongueur);
            card.Controls.Add(_numLongueur);
            card.Controls.Add(lblLargeur);
            card.Controls.Add(_numLargeur);
            card.Controls.Add(btnValider);

            _setupPanel.Controls.Add(card);
            this.Controls.Add(_setupPanel);

            this.Resize += (s, e) =>
            {
                card.Location = new Point(
                    (this.ClientSize.Width - card.Width) / 2,
                    (this.ClientSize.Height - card.Height) / 2);
            };
        }

        private void AfficherConfiguration()
        {
            _infoPanel.Visible = false;
            _plateauControl.Visible = false;
            _setupPanel.Visible = true;
        }

        private void InitialiserJeu(int longueur, int largeur)
        {
            if (longueur < 5 || largeur < 5)
            {
                MessageBox.Show("La longueur et la largeur doivent être au moins 5.", "Configuration invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string nomJ1 = "Joueur Rouge";
            string nomJ2 = "Joueur Bleu";

            _jeu = new JeuService(longueur, largeur, nomJ1, nomJ2);
            _setupPanel.Visible = false;
            _infoPanel.Visible = true;
            _plateauControl.Visible = true;
            _numLongueurJeu.Value = longueur;
            _numLargeurJeu.Value = largeur;
            CreerPlateauGraphique();
            MettreAJourAffichage();
        }

        private void BtnAppliquerTaille_Click(object? sender, EventArgs e)
        {
            int longueur = (int)_numLongueurJeu.Value;
            int largeur = (int)_numLargeurJeu.Value;

            if (MessageBox.Show(
                $"Appliquer un plateau {longueur}x{largeur} et recommencer la partie ?",
                "Changer la taille",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _modeAction = ModeAction.PlacerPoint;
            _tirEnCours = false;
            InitialiserJeu(longueur, largeur);
        }

        private void BtnValiderConfiguration_Click(object? sender, EventArgs e)
        {
            InitialiserJeu((int)_numLongueur.Value, (int)_numLargeur.Value);
        }

        private void CreerPlateauGraphique()
        {
            if (_jeu == null)
            {
                return;
            }

            _plateauControl.SetJeu(_jeu);
            _plateauControl.Invalidate();
        }

        private void OnIntersectionClicked(Position position)
        {
            if (_jeu == null || _tirEnCours) return;

            if (_modeAction == ModeAction.TirerCanon)
            {
                int puissance = ObtenirPuissanceCanon();
                _plateauControl.SetActiveCannonRow(position.Y);
                int tireurId = _jeu.JoueurActuel.Id;
                var resultatTir = _jeu.TirerCanonAvecResultat(position, puissance);
                if (resultatTir == null)
                {
                    Position destinationRatee = _jeu.CalculerDestinationCanonVers(position, puissance);
                    _tirEnCours = true;
                    _plateauControl.PlayShotAnimation(tireurId, destinationRatee, () =>
                    {
                        if (_jeu != null)
                        {
                            _jeu.PasserTour();
                        }

                        _modeAction = ModeAction.PlacerPoint;
                        _tirEnCours = false;
                        MettreAJourAffichage();
                        MessageBox.Show("Tir manqué. Le tour passe au joueur suivant.", "Tir", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }, trajectoireMortier: true);
                    return;
                }

                _tirEnCours = true;
                _plateauControl.PlayShotAnimation(resultatTir.TireurId, resultatTir.Cible, () =>
                {
                    if (_jeu != null)
                    {
                        _jeu.FinaliserTirCanon(resultatTir);
                    }
                    _modeAction = ModeAction.PlacerPoint;
                    _tirEnCours = false;
                    MettreAJourAffichage();
                }, trajectoireMortier: true);
                return;
            }

            var nouvelles = _jeu.JouerCoup(position);
            if (nouvelles == null)
            {
                MessageBox.Show("Intersection invalide !\nDéjà occupée, protégée, ou hors plateau.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _modeAction = ModeAction.PlacerPoint;

            // Toujours mettre à jour l'affichage
            MettreAJourAffichage();

            // Si des lignes ont été tracées, on les met en évidence visuellement mais la partie continue
            if (nouvelles.Count > 0)
            {
                // demander au contrôle de surligner temporairement
                var allPositions = nouvelles.SelectMany(l => l.Positions).ToList();
                _plateauControl.HighlightPositions(allPositions);
            }

            // On vérifie uniquement le match nul pour terminer la partie
            if (_jeu.EstMatchNul())
            {
                MessageBox.Show("Match nul ! Le plateau est plein.", "Fin de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (MessageBox.Show("Voulez-vous rejouer ?", "Nouvelle partie", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    NouvellePartie();
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (TryHandleCanonKeyboardShortcut(keyData))
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (TryHandleCanonKeyboardShortcut(e.KeyData))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
        }

        private bool TryHandleCanonKeyboardShortcut(Keys keyData)
        {
            if (_jeu == null || !_plateauControl.Visible || _tirEnCours)
            {
                return false;
            }

            if ((keyData & Keys.Control) == Keys.Control
                && TryGetPuissanceFromKey(keyData & Keys.KeyCode, out int puissanceSelectionnee))
            {
                DefinirPuissanceCanon(puissanceSelectionnee);
                _modeAction = ModeAction.TirerCanon;
                MettreAJourBoutonsAction();

                int ligne = _plateauControl.GetActiveCannonRow();
                int tireurId = _jeu.JoueurActuel.Id;
                var resultatTir = _jeu.TirerCanonSurLigneAvecResultat(ligne, puissanceSelectionnee);
                if (resultatTir == null)
                {
                    Position destinationRatee = _jeu.CalculerDestinationCanonSurLigne(ligne, puissanceSelectionnee);
                    _tirEnCours = true;
                    _plateauControl.PlayShotAnimation(tireurId, destinationRatee, () =>
                    {
                        if (_jeu != null)
                        {
                            _jeu.PasserTour();
                        }

                        _modeAction = ModeAction.PlacerPoint;
                        _tirEnCours = false;
                        MettreAJourAffichage();
                        MessageBox.Show("Tir manqué. Le tour passe au joueur suivant.", "Tir", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }, trajectoireMortier: true);
                }
                else
                {
                    _tirEnCours = true;
                    _plateauControl.PlayShotAnimation(resultatTir.TireurId, resultatTir.Cible, () =>
                    {
                        if (_jeu != null)
                        {
                            _jeu.FinaliserTirCanon(resultatTir);
                        }
                        _modeAction = ModeAction.PlacerPoint;
                        _tirEnCours = false;
                        MettreAJourAffichage();
                    }, trajectoireMortier: true);
                }

                return true;
            }

            Keys key = keyData & Keys.KeyCode;
            if (key == Keys.Up)
            {
                _plateauControl.MoveActiveCannon(-1);
                return true;
            }

            if (key == Keys.Down)
            {
                _plateauControl.MoveActiveCannon(1);
                return true;
            }

            return false;
        }

        private void MettreAJourAffichage()
        {
            if (_jeu == null)
            {
                return;
            }

            // Forcer le rendu immédiat évite le retard visuel du tracé jusqu'au coup suivant.
            _plateauControl.Refresh();

            // Mettre à jour les labels
            var joueurs = _jeu.GetJoueurs();
            _labelScoreJ1.Text = $"{joueurs[0].Nom}: {joueurs[0].Score}";
            _labelScoreJ2.Text = $"{joueurs[1].Nom}: {joueurs[1].Score}";

            var joueurActuel = _jeu.JoueurActuel;
            _labelTour.Text = $"Tour de {joueurActuel.Nom} ({joueurActuel.Symbole})";
            _labelTour.ForeColor = joueurActuel.Couleur;

            MettreAJourBoutonsAction();
        }

        private void MettreAJourBoutonsAction()
        {
            bool placerActif = _modeAction == ModeAction.PlacerPoint;
            _btnModePlacer.BackColor = placerActif ? Color.FromArgb(70, 130, 80) : Color.FromArgb(95, 95, 95);
            _btnModeTirer.BackColor = placerActif ? Color.FromArgb(95, 95, 95) : Color.FromArgb(150, 90, 65);
            _labelModeAction.Text = placerActif
                ? "Action: Placer un point"
                : "Action: Tirer au canon";
        }

        private int ObtenirPuissanceCanon()
        {
            return (int)_numPuissanceCanon.Value;
        }

        private void DefinirPuissanceCanon(int puissance)
        {
            if (puissance < _numPuissanceCanon.Minimum || puissance > _numPuissanceCanon.Maximum)
            {
                return;
            }

            _numPuissanceCanon.Value = puissance;
            MettreAJourLabelPorteeChoisie();
        }

        private void MettreAJourLabelPorteeChoisie()
        {
            if (_labelPorteeChoisie == null)
            {
                return;
            }

            _labelPorteeChoisie.Text = $"Portée choisie: {ObtenirPuissanceCanon()}";
        }

        private static bool TryGetPuissanceFromKey(Keys key, out int puissance)
        {
            puissance = 0;

            if (key >= Keys.D1 && key <= Keys.D9)
            {
                puissance = key - Keys.D0;
                return true;
            }

            if (key >= Keys.NumPad1 && key <= Keys.NumPad9)
            {
                puissance = key - Keys.NumPad0;
                return true;
            }

            return false;
        }

        private void BtnNouvellePartie_Click(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Voulez-vous vraiment recommencer une nouvelle partie ?", 
                "Nouvelle partie", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                NouvellePartie();
            }
        }

        private void NouvellePartie()
        {
            if (_jeu == null)
            {
                return;
            }

            int longueur = _jeu.Plateau.Longueur;
            int largeur = _jeu.Plateau.Largeur;
            
            string nomJ1 = _jeu.GetJoueurs()[0].Nom;
            string nomJ2 = _jeu.GetJoueurs()[1].Nom;
            
            _jeu = new JeuService(longueur, largeur, nomJ1, nomJ2);
            _modeAction = ModeAction.PlacerPoint;
            _tirEnCours = false;
            CreerPlateauGraphique();
            MettreAJourAffichage();
        }

        private void BtnSauvegarder_Click(object? sender, EventArgs e)
        {
            if (_jeu == null)
            {
                MessageBox.Show("Aucune partie en cours à sauvegarder.", "Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!InitialiserSauvegardeService())
            {
                return;
            }

            try
            {
                var gagnant = _jeu.VerifierVictoire();
                bool partieTerminee = gagnant != null || _jeu.EstMatchNul();
                string nom = $"Partie {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                var sauvegarde = _sauvegardeService!.SauvegarderPartie(
                    nom: nom,
                    jeuService: _jeu,
                    partieTerminee: partieTerminee,
                    joueurGagnant: gagnant
                );

                MessageBox.Show($"Partie sauvegardée (ID: {sauvegarde.Id}).", "Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde: {ex.Message}", "Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCharger_Click(object? sender, EventArgs e)
        {
            if (!InitialiserSauvegardeService())
            {
                return;
            }

            try
            {
                var sauvegardes = _sauvegardeService!.ListerToutesSauvegardes();
                if (sauvegardes.Count == 0)
                {
                    MessageBox.Show("Aucune sauvegarde disponible.", "Chargement", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var selection = AfficherDialogueSelectionSauvegarde(sauvegardes);
                if (selection == null)
                {
                    return;
                }

                ChargerSauvegarde(selection);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement: {ex.Message}", "Chargement", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool InitialiserSauvegardeService()
        {
            if (_sauvegardeService != null)
            {
                return true;
            }

            try
            {
                string appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (!File.Exists(appSettingsPath))
                {
                    appSettingsPath = Path.Combine(Application.StartupPath, "appsettings.json");
                }

                if (!File.Exists(appSettingsPath))
                {
                    MessageBox.Show("Fichier appsettings.json introuvable. Impossible d'initialiser la sauvegarde.", "Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                string json = File.ReadAllText(appSettingsPath);
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("PostgreSQL", out var pg))
                {
                    MessageBox.Show("Section PostgreSQL absente dans appsettings.json.", "Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                string host = pg.GetProperty("Host").GetString() ?? "localhost";
                int port = pg.GetProperty("Port").GetInt32();
                string database = pg.GetProperty("Database").GetString() ?? "jeu_de_points_db";
                string username = pg.GetProperty("Username").GetString() ?? "postgres";
                string password = pg.GetProperty("Password").GetString() ?? string.Empty;

                string connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};";
                _sauvegardeService = new SauvegardeService(connectionString);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossible d'initialiser la sauvegarde: {ex.Message}", "Configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private SauvegardeJeu? AfficherDialogueSelectionSauvegarde(List<SauvegardeJeu> sauvegardes)
        {
            using var dialog = new Form
            {
                Text = "Choisir une sauvegarde",
                Size = new Size(620, 380),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var list = new ListBox
            {
                Dock = DockStyle.Top,
                Height = 280,
                Font = new Font("Consolas", 10)
            };

            foreach (var s in sauvegardes)
            {
                list.Items.Add($"[{s.Id}] {s.DateSauvegarde:dd/MM/yyyy HH:mm} - {s.Nom}");
            }

            if (list.Items.Count > 0)
            {
                list.SelectedIndex = 0;
            }

            var btnOk = new Button
            {
                Text = "Charger",
                DialogResult = DialogResult.OK,
                Width = 120,
                Height = 32,
                Left = 360,
                Top = 295
            };

            var btnAnnuler = new Button
            {
                Text = "Annuler",
                DialogResult = DialogResult.Cancel,
                Width = 120,
                Height = 32,
                Left = 490,
                Top = 295
            };

            dialog.Controls.Add(list);
            dialog.Controls.Add(btnOk);
            dialog.Controls.Add(btnAnnuler);
            dialog.AcceptButton = btnOk;
            dialog.CancelButton = btnAnnuler;

            if (dialog.ShowDialog(this) != DialogResult.OK || list.SelectedIndex < 0)
            {
                return null;
            }

            return sauvegardes[list.SelectedIndex];
        }

        private void ChargerSauvegarde(SauvegardeJeu sauvegarde)
        {
            _jeu = new JeuService(
                sauvegarde.PlateauLongueur,
                sauvegarde.PlateauLargeur,
                sauvegarde.Joueur1Nom,
                sauvegarde.Joueur2Nom);

            var joueurs = _jeu.GetJoueurs();
            var joueur1 = joueurs[0];
            var joueur2 = joueurs[1];

            using var doc = JsonDocument.Parse(sauvegarde.EtatJeu);
            var root = doc.RootElement;

            if (root.TryGetProperty("plateau", out var plateauJson)
                && plateauJson.TryGetProperty("cellules", out var cellules))
            {
                for (int x = 0; x < cellules.GetArrayLength(); x++)
                {
                    var row = cellules[x];
                    for (int y = 0; y < row.GetArrayLength(); y++)
                    {
                        var cellData = row[y];
                        var cellule = _jeu.Plateau.GetCellule(new Position(x, y));
                        if (cellule == null)
                        {
                            continue;
                        }

                        bool estVide = cellData.GetProperty("estVide").GetBoolean();
                        cellule.Proprietaire = estVide
                            ? null
                            : (cellData.GetProperty("proprietaireId").GetInt32() == joueur1.Id ? joueur1 : joueur2);
                        cellule.EstProtegee = cellData.GetProperty("estProtegee").GetBoolean();
                    }
                }
            }

            _jeu.GetLignesTracees().Clear();
            if (root.TryGetProperty("lignesTracees", out var lignesJson))
            {
                foreach (var ligneJson in lignesJson.EnumerateArray())
                {
                    int joueurId = ligneJson.GetProperty("joueurId").GetInt32();
                    string direction = ligneJson.GetProperty("direction").GetString() ?? "Inconnue";
                    var positions = new List<Position>();

                    foreach (var posJson in ligneJson.GetProperty("positions").EnumerateArray())
                    {
                        positions.Add(new Position(
                            posJson.GetProperty("x").GetInt32(),
                            posJson.GetProperty("y").GetInt32()));
                    }

                    var joueur = joueurId == joueur1.Id ? joueur1 : joueur2;
                    _jeu.GetLignesTracees().Add(new LigneTracee(joueur, positions, direction));
                }
            }

            joueur1.Score = sauvegarde.Joueur1Score;
            joueur2.Score = sauvegarde.Joueur2Score;

            if (_jeu.JoueurActuel.Id != sauvegarde.JoueurActuelId)
            {
                _jeu.PasserTour();
            }

            _modeAction = ModeAction.PlacerPoint;
            _tirEnCours = false;
            _setupPanel.Visible = false;
            _infoPanel.Visible = true;
            _plateauControl.Visible = true;
            CreerPlateauGraphique();
            MettreAJourAffichage();

            MessageBox.Show($"Sauvegarde chargée: {sauvegarde.Nom}", "Chargement", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}