using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        private bool isPlayerOneTurn = true;
        private bool gameOver = false;

        private Dictionary<int, List<int>> p1RowMoves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> p1ColMoves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> p1Diag1Moves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> p1Diag2Moves = new Dictionary<int, List<int>>();

        private Dictionary<int, List<int>> aiRowMoves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> aiColMoves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> aiDiag1Moves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> aiDiag2Moves = new Dictionary<int, List<int>>();

        private Dictionary<int, List<int>> allMoves = new Dictionary<int, List<int>>();

        private Button restartButton;
        private Label statusLabel;
        private Random rand = new Random();

        public PvEForm()
        {
            Text = "Connect 4 — Player vs AI";
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
            p1RowMoves.Clear(); p1ColMoves.Clear(); p1Diag1Moves.Clear(); p1Diag2Moves.Clear();
            aiRowMoves.Clear(); aiColMoves.Clear(); aiDiag1Moves.Clear(); aiDiag2Moves.Clear();
            allMoves.Clear();
            isPlayerOneTurn = true;
            gameOver = false;
            UpdateStatus();
            Invalidate();
        }

        private void UpdateStatus()
        {
            statusLabel.Text = isPlayerOneTurn ? "Your Turn (Red)" : "AI is thinking...";
        }

        private void PvEForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (gameOver || !isPlayerOneTurn)
                return;

            if (e.Y < OFFSET_Y || e.Y > OFFSET_Y + ROWS * CELL_SIZE)
                return;

            int col = (e.X - OFFSET_X) / CELL_SIZE;
            if (col < 0 || col >= COLS)
                return;

            if (!DropDisc(col, isPlayerOneTurn))
                return;

            if (CheckWinCondition())
                return;

            isPlayerOneTurn = false;
            UpdateStatus();

            // Let AI play after short delay
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 600 };
            timer.Tick += (s, ev) =>
            {
                timer.Stop();
                AIPlay();
            };
            timer.Start();
        }

        private void AIPlay()
        {
            if (gameOver)
                return;

            int col;
            List<int> availableCols = Enumerable.Range(0, COLS)
                .Where(c => !allMoves.ContainsKey(c) || allMoves[c].Count < ROWS)
                .ToList();

            if (availableCols.Count == 0) return;

            col = availableCols[rand.Next(availableCols.Count)];
            DropDisc(col, false);

            CheckWinCondition();

            isPlayerOneTurn = true;
            UpdateStatus();
        }

        private bool DropDisc(int col, bool isPlayer)
        {
            int filled = allMoves.ContainsKey(col) ? allMoves[col].Count : 0;
            if (filled >= ROWS)
                return false;

            int newRow = ROWS - 1 - filled;

            if (!allMoves.ContainsKey(col))
                allMoves[col] = new List<int>();
            allMoves[col].Add(newRow);

            var rowMoves = isPlayer ? p1RowMoves : aiRowMoves;
            var colMoves = isPlayer ? p1ColMoves : aiColMoves;
            var diag1Moves = isPlayer ? p1Diag1Moves : aiDiag1Moves;
            var diag2Moves = isPlayer ? p1Diag2Moves : aiDiag2Moves;

            AddMove(rowMoves, newRow, col);
            AddMove(colMoves, col, newRow);
            AddMove(diag1Moves, col - newRow, col);
            AddMove(diag2Moves, col + newRow, col);

            Invalidate();
            return true;
        }

        private void AddMove(Dictionary<int, List<int>> dict, int key, int value)
        {
            if (!dict.ContainsKey(key))
                dict[key] = new List<int>();
            dict[key].Add(value);
        }

        private bool CheckWinCondition()
        {
            if (CheckWin(p1RowMoves) || CheckWin(p1ColMoves) || CheckWin(p1Diag1Moves) || CheckWin(p1Diag2Moves))
            {
                gameOver = true;
                MessageBox.Show("You win!", "Game Over");
                statusLabel.Text = "You win!";
                return true;
            }

            if (CheckWin(aiRowMoves) || CheckWin(aiColMoves) || CheckWin(aiDiag1Moves) || CheckWin(aiDiag2Moves))
            {
                gameOver = true;
                MessageBox.Show("AI wins!", "Game Over");
                statusLabel.Text = "AI wins!";
                return true;
            }

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
                return true;
            }

            return false;
        }

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

                    if (p1ColMoves.ContainsKey(c) && p1ColMoves[c].Contains(r))
                        brush = Brushes.Red;
                    else if (aiColMoves.ContainsKey(c) && aiColMoves[c].Contains(r))
                        brush = Brushes.Yellow;

                    g.FillEllipse(brush, x, y, size, size);
                    g.DrawEllipse(Pens.Black, x, y, size, size);
                }
            }
        }
    }
}
