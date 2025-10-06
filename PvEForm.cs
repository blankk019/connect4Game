using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Connect4
{
    public partial class PvEForm : Form
    {
        private const int ROWS = 6;
        private const int COLS = 7;
        private const int CELL_SIZE = 100;
        private const int OFFSET_X = 60;
        private const int OFFSET_Y = 150;

        // Game data
        private Dictionary<int, List<int>> playerMoves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> aiMoves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> allMoves = new Dictionary<int, List<int>>();

        private bool isPlayerTurn = true;
        private bool gameOver = false;

        private Button restartButton;
        private Label statusLabel;
        private Random random = new Random();

        public PvEForm()
        {
            Text = "Connect 4 — Player vs Computer";
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

            MouseClick += PvEForm_MouseClick;
        }

        private void ResetGame()
        {
            playerMoves.Clear();
            aiMoves.Clear();
            allMoves.Clear();
            isPlayerTurn = true;
            gameOver = false;
            statusLabel.Text = "Your Turn (Red)";
            Invalidate();
        }

        // Handle mouse click
        private void PvEForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (gameOver || !isPlayerTurn)
                return;

            if (e.Y < OFFSET_Y || e.Y > OFFSET_Y + ROWS * CELL_SIZE)
                return;

            int col = (e.X - OFFSET_X) / CELL_SIZE;
            if (col < 0 || col >= COLS)
                return;

            MakeMove(col, true);
        }

        // Handles both player and AI moves
        private void MakeMove(int col, bool isPlayer)
        {
            int filled = allMoves.ContainsKey(col) ? allMoves[col].Count : 0;
            if (filled >= ROWS)
                return; // Column is full

            int newRow = ROWS - 1 - filled;

            // Add to allMoves
            if (!allMoves.ContainsKey(col))
                allMoves[col] = new List<int>();
            allMoves[col].Add(newRow);

            // Add to player or AI dictionary
            Dictionary<int, List<int>> moves = isPlayer ? playerMoves : aiMoves;
            if (!moves.ContainsKey(col))
                moves[col] = new List<int>();
            moves[col].Add(newRow);

            Invalidate(); // Redraw

            // Check for win
            List<Point> winCells = CheckWin(moves, col, newRow);
            if (winCells != null)
            {
                gameOver = true;
                string winner = isPlayer ? "You (Red)" : "Computer (Yellow)";
                MessageBox.Show(winner + " win!", "Game Over");
                statusLabel.Text = winner + " win!";
                return;
            }

            // Check for draw
            bool full = true;
            for (int i = 0; i < COLS; i++)
            {
                if (!allMoves.ContainsKey(i) || allMoves[i].Count < ROWS)
                {
                    full = false;
                    break;
                }
            }

            if (full)
            {
                gameOver = true;
                MessageBox.Show("It's a draw!", "Game Over");
                statusLabel.Text = "Draw!";
                return;
            }

            // Switch turns
            isPlayerTurn = !isPlayerTurn;
            statusLabel.Text = isPlayerTurn ? "Your Turn (Red)" : "Computer's Turn (Yellow)";

            // AI plays immediately
            if (!isPlayerTurn && !gameOver)
                AiMove();
        }

        // Simple random AI
        private void AiMove()
        {
            int col;

            // Find a random valid column
            do
            {
                col = random.Next(COLS);
            } while (allMoves.ContainsKey(col) && allMoves[col].Count >= ROWS);

            MakeMove(col, false);
        }

        // Draw board
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

                    if (playerMoves.ContainsKey(c) && playerMoves[c].Contains(r))
                        brush = Brushes.Red;
                    else if (aiMoves.ContainsKey(c) && aiMoves[c].Contains(r))
                        brush = Brushes.Yellow;

                    g.FillEllipse(brush, x, y, size, size);
                    g.DrawEllipse(Pens.Black, x, y, size, size);
                }
            }
        }

        // Check win conditions
        private List<Point> CheckWin(Dictionary<int, List<int>> moves, int col, int row)
        {
            List<Point> result;

            result = CollectLine(moves, col, row, 0, 1);  // Vertical
            if (result != null) return result;

            result = CollectLine(moves, col, row, 1, 0);  // Horizontal
            if (result != null) return result;

            result = CollectLine(moves, col, row, 1, 1);  // Diagonal \
            if (result != null) return result;

            result = CollectLine(moves, col, row, 1, -1); // Diagonal /
            return result;
        }

        private List<Point> CollectLine(Dictionary<int, List<int>> moves, int startCol, int startRow, int dCol, int dRow)
        {
            List<Point> connected = new List<Point>();
            connected.Add(new Point(startCol, startRow));

            // Forward
            int c = startCol + dCol;
            int r = startRow + dRow;
            while (c >= 0 && c < COLS && r >= 0 && r < ROWS)
            {
                if (moves.ContainsKey(c) && moves[c].Contains(r))
                    connected.Add(new Point(c, r));
                else
                    break;

                c += dCol;
                r += dRow;
            }

            // Backward
            c = startCol - dCol;
            r = startRow - dRow;
            while (c >= 0 && c < COLS && r >= 0 && r < ROWS)
            {
                if (moves.ContainsKey(c) && moves[c].Contains(r))
                    connected.Add(new Point(c, r));
                else
                    break;

                c -= dCol;
                r -= dRow;
            }

            if (connected.Count >= 4)
                return connected;

            return null;
        }
    }
}
