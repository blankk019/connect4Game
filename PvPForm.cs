using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Connect4
{
    public partial class PvPForm : Form
    {
        private const int ROWS = 6;
        private const int COLS = 7;
        private const int CELL_SIZE = 100;
        private const int OFFSET_X = 60;
        private const int OFFSET_Y = 150;

        private bool isPlayerOneTurn = true;
        private bool gameOver = false;

        // Each player has 3 dictionaries:
        // RowMoves[row] = list of columns
        // ColMoves[col] = list of rows
        // Diag1Moves[col - row] = list of columns (main diagonal "\")
        // Diag2Moves[col + row] = list of columns (anti-diagonal "/")
        private Dictionary<int, List<int>> p1RowMoves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> p1ColMoves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> p1Diag1Moves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> p1Diag2Moves = new Dictionary<int, List<int>>();

        private Dictionary<int, List<int>> p2RowMoves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> p2ColMoves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> p2Diag1Moves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> p2Diag2Moves = new Dictionary<int, List<int>>();

        // To know which cells are filled in total
        private Dictionary<int, List<int>> allMoves = new Dictionary<int, List<int>>();

        private Button restartButton;
        private Label statusLabel;

        public PvPForm()
        {
            Text = "Connect 4 — Player vs Player";
            Size = new Size(900, 850);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;

            CreateUi();
            ResetGame();
        }

        private void CreateUi()
        {
            restartButton = new Button
            {
                Text = "Restart Game",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(150, 40),
                Location = new Point(OFFSET_X, 50)
            };
            restartButton.Click += (s, e) => ResetGame();
            Controls.Add(restartButton);

            statusLabel = new Label
            {
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(OFFSET_X + 200, 55)
            };
            Controls.Add(statusLabel);

            MouseClick += PvPForm_MouseClick;
        }

        private void ResetGame()
        {
            p1RowMoves.Clear(); p1ColMoves.Clear(); p1Diag1Moves.Clear(); p1Diag2Moves.Clear();
            p2RowMoves.Clear(); p2ColMoves.Clear(); p2Diag1Moves.Clear(); p2Diag2Moves.Clear();
            allMoves.Clear();
            isPlayerOneTurn = true;
            gameOver = false;
            UpdateStatus();
            Invalidate();
        }

        private void UpdateStatus()
        {
            statusLabel.Text = isPlayerOneTurn ? "Turn: Player 1 (Red)" : "Turn: Player 2 (Yellow)";
        }

        // ---------------------------------------------------
        // HANDLE PLAYER CLICKS
        // ---------------------------------------------------
        private void PvPForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (gameOver) return;

            // Check board limits
            if (e.Y < OFFSET_Y || e.Y > OFFSET_Y + ROWS * CELL_SIZE)
                return;

            int col = (e.X - OFFSET_X) / CELL_SIZE;
            if (col < 0 || col >= COLS)
                return;

            // Check how many discs already in column
            int filled = allMoves.ContainsKey(col) ? allMoves[col].Count : 0;
            if (filled >= ROWS)
                return;

            int newRow = ROWS - 1 - filled;

            // Record move
            if (!allMoves.ContainsKey(col))
                allMoves[col] = new List<int>();
            allMoves[col].Add(newRow);

            // Get current player's dictionaries
            Dictionary<int, List<int>> rowMoves = isPlayerOneTurn ? p1RowMoves : p2RowMoves;
            Dictionary<int, List<int>> colMoves = isPlayerOneTurn ? p1ColMoves : p2ColMoves;
            Dictionary<int, List<int>> diag1Moves = isPlayerOneTurn ? p1Diag1Moves : p2Diag1Moves;
            Dictionary<int, List<int>> diag2Moves = isPlayerOneTurn ? p1Diag2Moves : p2Diag2Moves;

            // Update all 3 structures
            AddMove(rowMoves, newRow, col);
            AddMove(colMoves, col, newRow);
            AddMove(diag1Moves, col - newRow, col);
            AddMove(diag2Moves, col + newRow, col);

            Invalidate(); // redraw

            // Check for win
            if (CheckWin(rowMoves) || CheckWin(colMoves) || CheckWin(diag1Moves) || CheckWin(diag2Moves))
            {
                gameOver = true;
                string winner = isPlayerOneTurn ? "Player 1 (Red)" : "Player 2 (Yellow)";
                MessageBox.Show(winner + " wins!", "Game Over");
                statusLabel.Text = winner + " wins!";
                return;
            }

            // Check for draw
            bool allFull = true;
            for (int i = 0; i < COLS; i++)
            {
                if (!allMoves.ContainsKey(i) || allMoves[i].Count < ROWS)
                {
                    allFull = false;
                    break;
                }
            }

            if (allFull)
            {
                gameOver = true;
                MessageBox.Show("It's a draw!", "Game Over");
                statusLabel.Text = "Draw!";
                return;
            }

            isPlayerOneTurn = !isPlayerOneTurn;
            UpdateStatus();
        }

        private void AddMove(Dictionary<int, List<int>> dict, int key, int value)
        {
            if (!dict.ContainsKey(key))
                dict[key] = new List<int>();

            dict[key].Add(value);
        }

        // ---------------------------------------------------
        // DRAWING THE BOARD
        // ---------------------------------------------------
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            g.FillRectangle(Brushes.Blue, OFFSET_X, OFFSET_Y, COLS * CELL_SIZE, ROWS * CELL_SIZE);

            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    int x = OFFSET_X + c * CELL_SIZE + 5;
                    int y = OFFSET_Y + r * CELL_SIZE + 5;
                    int size = CELL_SIZE - 10;

                    Brush brush = Brushes.White;

                    // Check which player has this cell
                    if (p1ColMoves.ContainsKey(c) && p1ColMoves[c].Contains(r))
                        brush = Brushes.Red;
                    else if (p2ColMoves.ContainsKey(c) && p2ColMoves[c].Contains(r))
                        brush = Brushes.Yellow;

                    g.FillEllipse(brush, x, y, size, size);
                    g.DrawEllipse(Pens.Black, x, y, size, size);
                }
            }
        }

        // ---------------------------------------------------
        // CHECK WIN (4 CONSECUTIVE NUMBERS)
        // ---------------------------------------------------
        private bool CheckWin(Dictionary<int, List<int>> dict)
        {
            foreach (var entry in dict)
            {
                List<int> list = entry.Value;
                if (list.Count < 4)
                    continue;

                list.Sort();

                int consecutive = 1;
                for (int i = 1; i < list.Count; i++)
                {
                    if (list[i] == list[i - 1] + 1)
                    {
                        consecutive++;
                        if (consecutive >= 4)
                            return true;
                    }
                    else
                    {
                        consecutive = 1;
                    }
                }
            }
            return false;
        }
    }
}
