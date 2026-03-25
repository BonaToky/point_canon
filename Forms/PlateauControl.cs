using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using JeuDePoints.Models;
using JeuDePoints.Services;

namespace JeuDePoints.Forms
{
    public class PlateauControl : Control
    {
        private const int BoardMargin = 26;
        private JeuService? _jeu;
        private List<Position> _tempHighlights = new List<Position>();
        private System.Windows.Forms.Timer _highlightTimer;
        private System.Windows.Forms.Timer _shotTimer;
        private int _canonRowJ1;
        private int _canonRowJ2;
        private PointF? _projectilePosition;
        private PointF _projectileStart;
        private PointF _projectileEnd;
        private float _projectileProgress;
        private bool _projectileArc;
        private float _projectileArcHeight;
        private Color _projectileColor;
        private Action? _shotCompleted;

        public event Action<Position>? IntersectionClicked;

        public bool IsShotAnimating => _projectilePosition.HasValue;

        public PlateauControl()
        {
            DoubleBuffered = true;
            _highlightTimer = new System.Windows.Forms.Timer();
            _highlightTimer.Interval = 700;
            _highlightTimer.Tick += (s, e) => { _tempHighlights.Clear(); _highlightTimer.Stop(); Invalidate(); };

            _shotTimer = new System.Windows.Forms.Timer();
            _shotTimer.Interval = 1000 / 30;
            _shotTimer.Tick += ShotTimer_Tick;
            BackColor = Color.FromArgb(235, 247, 255);
        }

        public void SetJeu(JeuService jeu)
        {
            _jeu = jeu;
            int centre = _jeu.Plateau.Largeur / 2;
            _canonRowJ1 = centre;
            _canonRowJ2 = centre;
            Invalidate();
        }

        public void MoveActiveCannon(int delta)
        {
            if (_jeu == null)
            {
                return;
            }

            int maxRow = Math.Max(0, _jeu.Plateau.Largeur - 1);
            if (_jeu.JoueurActuel.Id == 1)
            {
                _canonRowJ1 = Math.Clamp(_canonRowJ1 + delta, 0, maxRow);
            }
            else
            {
                _canonRowJ2 = Math.Clamp(_canonRowJ2 + delta, 0, maxRow);
            }

            Invalidate();
        }

        public int GetActiveCannonRow()
        {
            if (_jeu == null)
            {
                return 0;
            }

            return _jeu.JoueurActuel.Id == 1 ? _canonRowJ1 : _canonRowJ2;
        }

        public void SetActiveCannonRow(int row)
        {
            if (_jeu == null)
            {
                return;
            }

            int maxRow = Math.Max(0, _jeu.Plateau.Largeur - 1);
            int rowValide = Math.Clamp(row, 0, maxRow);
            if (_jeu.JoueurActuel.Id == 1)
            {
                _canonRowJ1 = rowValide;
            }
            else
            {
                _canonRowJ2 = rowValide;
            }

            Invalidate();
        }

        public void PlayShotAnimation(int tireurId, Position cible, Action? onComplete = null, bool trajectoireMortier = false)
        {
            if (_jeu == null)
            {
                onComplete?.Invoke();
                return;
            }

            int cols = _jeu.Plateau.Longueur;
            int rows = _jeu.Plateau.Largeur;
            if (!TryGetBoardGeometry(cols, rows, out int startX, out int startY, out int cellSize, out int boardWidth, out _))
            {
                onComplete?.Invoke();
                return;
            }

            int canonWidth = Math.Max(46, cellSize + 10);
            int canonOffset = 12;
            int leftX = Math.Max(4, startX - canonWidth - canonOffset);
            int rightX = Math.Min(ClientSize.Width - canonWidth - 4, startX + boardWidth + canonOffset);

            float startYShot = startY + cible.Y * cellSize;
            float startXShot = tireurId == 1 ? leftX + canonWidth - 2 : rightX + 2;

            _projectileStart = new PointF(startXShot, startYShot);
            _projectileEnd = new PointF(startX + cible.X * cellSize, startYShot);
            _projectilePosition = _projectileStart;
            _projectileProgress = 0f;
            _projectileArc = trajectoireMortier;
            _projectileArcHeight = trajectoireMortier
                ? Math.Max(24f, Math.Abs(_projectileEnd.X - _projectileStart.X) * 0.22f)
                : 0f;
            _projectileColor = tireurId == 1 ? Color.Red : Color.Blue;
            _shotCompleted = onComplete;

            _shotTimer.Stop();
            _shotTimer.Start();
            Invalidate();
        }

        private void ShotTimer_Tick(object? sender, EventArgs e)
        {
            _projectileProgress += 0.11f;
            if (_projectileProgress >= 1f)
            {
                _shotTimer.Stop();
                _projectilePosition = null;
                var callback = _shotCompleted;
                _shotCompleted = null;
                Invalidate();
                callback?.Invoke();
                return;
            }

            float t = _projectileProgress;
            float eased = 1f - (1f - t) * (1f - t);
            float x = _projectileStart.X + (_projectileEnd.X - _projectileStart.X) * eased;
            float y;
            if (_projectileArc)
            {
                float baseY = _projectileStart.Y + (_projectileEnd.Y - _projectileStart.Y) * eased;
                float arcOffset = 4f * _projectileArcHeight * eased * (1f - eased);
                y = baseY - arcOffset;
            }
            else
            {
                y = _projectileStart.Y + (_projectileEnd.Y - _projectileStart.Y) * eased;
            }
            _projectilePosition = new PointF(x, y);
            Invalidate();
        }

        public void HighlightPositions(IEnumerable<Position> positions)
        {
            _tempHighlights = positions.ToList();
            _highlightTimer.Stop();
            _highlightTimer.Start();
            Invalidate();
        }

        private static Point ToPixel(Position pos, int startX, int startY, int cellSize)
        {
            return new Point(startX + pos.X * cellSize, startY + pos.Y * cellSize);
        }

        private static void DrawBeautifulTrace(Graphics g, Point a, Point b, Color couleurJoueur)
        {
            var glowColor = Color.FromArgb(70, couleurJoueur);
            var midColor = Color.FromArgb(170, ControlPaint.Light(couleurJoueur, 0.2f));
            var coreColor = ControlPaint.LightLight(couleurJoueur);

            using (var glow = new Pen(glowColor, 16f))
            {
                glow.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                glow.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                g.DrawLine(glow, a, b);
            }

            using (var mid = new Pen(midColor, 9f))
            {
                mid.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                mid.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                g.DrawLine(mid, a, b);
            }

            using (var core = new Pen(coreColor, 4.5f))
            {
                core.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                core.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                g.DrawLine(core, a, b);
            }
        }

        private static void DrawCanon(Graphics g, Rectangle bounds, Color color, bool versDroite, bool actif)
        {
            Color cannonColor = actif ? ControlPaint.Light(color, 0.2f) : color;

            int tubeH = Math.Max(12, bounds.Height / 5);
            int tubeY = bounds.Y + (bounds.Height - tubeH) / 2;
            int tubeX = bounds.X + bounds.Width / 8;
            int tubeW = bounds.Width * 3 / 4;

            var tubeRect = new Rectangle(tubeX, tubeY, tubeW, tubeH);
            using (var tubeBrush = new LinearGradientBrush(tubeRect, ControlPaint.Light(cannonColor), ControlPaint.Dark(cannonColor), LinearGradientMode.Vertical))
            {
                g.FillRectangle(tubeBrush, tubeRect);
            }

            int boucheLargeur = Math.Max(8, bounds.Width / 8);
            var boucheRect = versDroite
                ? new Rectangle(tubeRect.Right - boucheLargeur / 2, tubeRect.Y - 1, boucheLargeur, tubeRect.Height + 2)
                : new Rectangle(tubeRect.X - boucheLargeur / 2, tubeRect.Y - 1, boucheLargeur, tubeRect.Height + 2);

            using (var bouche = new SolidBrush(ControlPaint.Dark(cannonColor)))
            {
                g.FillEllipse(bouche, boucheRect);
            }

            int arriereRayon = Math.Max(10, bounds.Height / 4);
            var arriereRect = versDroite
                ? new Rectangle(tubeRect.X - arriereRayon / 2, tubeRect.Y + tubeRect.Height / 2 - arriereRayon / 2, arriereRayon, arriereRayon)
                : new Rectangle(tubeRect.Right - arriereRayon / 2, tubeRect.Y + tubeRect.Height / 2 - arriereRayon / 2, arriereRayon, arriereRayon);

            using (var culasse = new SolidBrush(cannonColor))
            {
                g.FillEllipse(culasse, arriereRect);
            }

            if (actif)
            {
                using (var p = new Pen(Color.FromArgb(220, 255, 220, 130), 2f))
                {
                    p.DashStyle = DashStyle.Dash;
                    g.DrawRectangle(p, bounds);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_jeu == null)
            {
                return;
            }

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int cols = _jeu.Plateau.Longueur;
            int rows = _jeu.Plateau.Largeur;
            if (cols < 2 || rows < 2)
            {
                return;
            }

            if (!TryGetBoardGeometry(cols, rows, out int startX, out int startY, out int cellSize, out int boardWidth, out int boardHeight))
            {
                return;
            }

            using (var brush = new SolidBrush(Color.White))
            {
                g.FillRectangle(brush, startX, startY, boardWidth, boardHeight);
            }

            using (var pen = new Pen(Color.FromArgb(150, 205, 240), 1.6f))
            {
                for (int x = 0; x < cols; x++)
                {
                    int px = startX + x * cellSize;
                    g.DrawLine(pen, px, startY, px, startY + boardHeight);
                }

                for (int y = 0; y < rows; y++)
                {
                    int py = startY + y * cellSize;
                    g.DrawLine(pen, startX, py, startX + boardWidth, py);
                }
            }

            int canonHeight = Math.Max(60, boardHeight / 4);
            int canonWidth = Math.Max(46, cellSize + 10);
            int canonOffset = 12;
            bool j1Actif = _jeu.JoueurActuel.Id == 1;
            int centreYGauche = startY + _canonRowJ1 * cellSize;
            int centreYDroite = startY + _canonRowJ2 * cellSize;
            int canonYGauche = Math.Clamp(centreYGauche - canonHeight / 2, 4, Math.Max(4, ClientSize.Height - canonHeight - 4));
            int canonYDroite = Math.Clamp(centreYDroite - canonHeight / 2, 4, Math.Max(4, ClientSize.Height - canonHeight - 4));

            int leftX = Math.Max(4, startX - canonWidth - canonOffset);
            int rightX = Math.Min(ClientSize.Width - canonWidth - 4, startX + boardWidth + canonOffset);
            var canonGauche = new Rectangle(leftX, canonYGauche, canonWidth, canonHeight);
            var canonDroite = new Rectangle(rightX, canonYDroite, canonWidth, canonHeight);

            DrawCanon(g, canonGauche, Color.FromArgb(200, 60, 60), true, j1Actif);
            DrawCanon(g, canonDroite, Color.FromArgb(70, 120, 220), false, !j1Actif);

            // Tracé permanent des lignes gagnantes (effet visuel)
            foreach (var ligne in _jeu.GetLignesTracees())
            {
                if (ligne.Positions.Count < 2)
                {
                    continue;
                }

                var start = ToPixel(ligne.Positions.First(), startX, startY, cellSize);
                var end = ToPixel(ligne.Positions.Last(), startX, startY, cellSize);
                DrawBeautifulTrace(g, start, end, ligne.Joueur.Couleur);
            }

            int radius = Math.Max(6, cellSize / 5);
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    int cx = startX + x * cellSize;
                    int cy = startY + y * cellSize;
                    var cellule = _jeu.Plateau.GetCellule(new Position(x, y));

                    if (_tempHighlights.Any(p => p.X == x && p.Y == y))
                    {
                        using (var b = new SolidBrush(Color.FromArgb(180, 186, 225, 248)))
                        {
                            g.FillEllipse(b, cx - radius - 4, cy - radius - 4, (radius + 4) * 2, (radius + 4) * 2);
                        }
                    }

                    if (cellule != null && !cellule.EstVide)
                    {
                        var proprietaire = cellule.Proprietaire;
                        if (proprietaire != null)
                        {
                            using (var b = new SolidBrush(proprietaire.Couleur))
                            {
                                g.FillEllipse(b, cx - radius, cy - radius, radius * 2, radius * 2);
                            }
                        }
                    }
                    else
                    {
                        using (var b = new SolidBrush(Color.FromArgb(120, 150, 180)))
                        {
                            g.FillEllipse(b, cx - 2, cy - 2, 4, 4);
                        }
                    }

                    if (cellule != null && cellule.EstProtegee)
                    {
                        using (var p = new Pen(Color.DeepSkyBlue, 3))
                        {
                            g.DrawEllipse(p, cx - radius - 2, cy - radius - 2, (radius + 2) * 2, (radius + 2) * 2);
                        }
                    }
                }
            }

            if (_projectilePosition.HasValue)
            {
                var pos = _projectilePosition.Value;
                using (var ombre = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                {
                    g.FillEllipse(ombre, pos.X - 7, pos.Y - 4, 14, 12);
                }

                using (var balle = new SolidBrush(_projectileColor))
                {
                    g.FillEllipse(balle, pos.X - 6, pos.Y - 6, 12, 12);
                }

                using (var reflet = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
                {
                    g.FillEllipse(reflet, pos.X - 3, pos.Y - 4, 4, 4);
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_jeu == null)
            {
                return;
            }

            int cols = _jeu.Plateau.Longueur;
            int rows = _jeu.Plateau.Largeur;
            if (cols < 2 || rows < 2)
            {
                return;
            }

            if (!TryGetBoardGeometry(cols, rows, out int startX, out int startY, out int cellSize, out _, out _))
            {
                return;
            }

            double bestDist = double.MaxValue;
            Position? best = null;

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    int cx = startX + x * cellSize;
                    int cy = startY + y * cellSize;
                    double dx = e.X - cx;
                    double dy = e.Y - cy;
                    double dist = Math.Sqrt(dx * dx + dy * dy);

                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = new Position(x, y);
                    }
                }
            }

            if (best != null && bestDist <= Math.Max(10, cellSize / 3))
            {
                IntersectionClicked?.Invoke(best);
            }
        }

        private bool TryGetBoardGeometry(int cols, int rows, out int startX, out int startY, out int cellSize, out int boardWidth, out int boardHeight)
        {
            int availableWidth = Math.Max(0, ClientSize.Width - 2 * BoardMargin);
            int availableHeight = Math.Max(0, ClientSize.Height - 2 * BoardMargin);

            int cellByWidth = availableWidth / (cols - 1);
            int cellByHeight = availableHeight / (rows - 1);
            cellSize = Math.Min(cellByWidth, cellByHeight);

            if (cellSize <= 0)
            {
                startX = 0;
                startY = 0;
                boardWidth = 0;
                boardHeight = 0;
                return false;
            }

            boardWidth = cellSize * (cols - 1);
            boardHeight = cellSize * (rows - 1);
            startX = (ClientSize.Width - boardWidth) / 2;
            startY = (ClientSize.Height - boardHeight) / 2;
            return true;
        }
    }
}
