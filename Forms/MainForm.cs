using System;
using System.Drawing;
using System.Windows.Forms;
using JeuDePoints.Models;
using JeuDePoints.Services;
using System.Collections.Generic;
using System.Linq;

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
        private Label _labelScoreJ1 = null!;
        private Label _labelScoreJ2 = null!;
        private Label _labelModeAction = null!;
        private Button _btnNouvellePartie = null!;
        private Button _btnModePlacer = null!;
        private Button _btnModeTirer = null!;
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
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;

            // Panel d'information
            _infoPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = Color.FromArgb(50, 50, 50)
            };

            // Label du tour
            _labelTour = new Label
            {
                Location = new Point(16, 20),
                Size = new Size(198, 90),
                Font = new Font("Arial", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Text = "Tour du Joueur Rouge (R)"
            };

            // Labels des scores
            _labelScoreJ1 = new Label
            {
                Location = new Point(16, 140),
                Size = new Size(198, 30),
                Font = new Font("Arial", 12),
                ForeColor = Color.White,
                Text = "Rouge: 0"
            };

            _labelScoreJ2 = new Label
            {
                Location = new Point(16, 174),
                Size = new Size(198, 30),
                Font = new Font("Arial", 12),
                ForeColor = Color.White,
                Text = "Bleu: 0"
            };

            _labelModeAction = new Label
            {
                Location = new Point(16, 214),
                Size = new Size(198, 44),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Text = "Action: Placer"
            };

            _btnModePlacer = new Button
            {
                Location = new Point(16, 266),
                Size = new Size(198, 34),
                Text = "Placer un point",
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(95, 95, 95),
                ForeColor = Color.White,
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
                BackColor = Color.FromArgb(95, 95, 95),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnModeTirer.Click += (s, e) =>
            {
                _modeAction = ModeAction.TirerCanon;
                MettreAJourBoutonsAction();
            };

            // Bouton nouvelle partie
            _btnNouvellePartie = new Button
            {
                Location = new Point(16, 354),
                Size = new Size(198, 40),
                Text = "Nouvelle Partie",
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnNouvellePartie.Click += BtnNouvellePartie_Click;

            _infoPanel.Controls.Add(_labelTour);
            _infoPanel.Controls.Add(_labelScoreJ1);
            _infoPanel.Controls.Add(_labelScoreJ2);
            _infoPanel.Controls.Add(_labelModeAction);
            _infoPanel.Controls.Add(_btnModePlacer);
            _infoPanel.Controls.Add(_btnModeTirer);
            _infoPanel.Controls.Add(_btnNouvellePartie);

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
                BackColor = Color.FromArgb(238, 232, 220)
            };

            var card = new Panel
            {
                Size = new Size(430, 220),
                BackColor = Color.FromArgb(255, 248, 236),
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
                ForeColor = Color.FromArgb(70, 50, 30)
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
                BackColor = Color.FromArgb(80, 120, 75),
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
            CreerPlateauGraphique();
            MettreAJourAffichage();
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
                var resultatTir = _jeu.TirerCanonAvecResultat(position);
                if (resultatTir == null)
                {
                    _jeu.PasserTour();
                    _modeAction = ModeAction.PlacerPoint;
                    MettreAJourAffichage();
                    MessageBox.Show("Tir manqué. Le tour passe au joueur suivant.", "Tir", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                });
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

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_jeu == null || !_plateauControl.Visible || _tirEnCours)
            {
                return;
            }

            if (e.KeyCode == Keys.W)
            {
                _modeAction = ModeAction.TirerCanon;
                _plateauControl.MoveActiveCannon(-1);
                MettreAJourBoutonsAction();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.S)
            {
                _modeAction = ModeAction.TirerCanon;
                _plateauControl.MoveActiveCannon(1);
                MettreAJourBoutonsAction();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Space)
            {
                _modeAction = ModeAction.TirerCanon;
                int ligne = _plateauControl.GetActiveCannonRow();
                var resultatTir = _jeu.TirerCanonSurLigneAvecResultat(ligne);
                if (resultatTir == null)
                {
                    _jeu.PasserTour();
                    _modeAction = ModeAction.PlacerPoint;
                    MettreAJourAffichage();
                    MessageBox.Show("Tir manqué. Le tour passe au joueur suivant.", "Tir", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
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
                });
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
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
    }
}