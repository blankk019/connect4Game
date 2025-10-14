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

        private bool isPlayerTurn = true;
        private bool gameOver = false;

        private Dictionary<int, List<int>> playerRowMoves = new();
        private Dictionary<int, List<int>> playerColMoves = new();
        private Dictionary<int, List<int>> playerDiag1Moves = new();
        private Dictionary<int, List<int>> playerDiag2Moves = new();

        private Dictionary<int, List<int>> aiRowMoves = new();
        private Dictionary<int, List<int>> aiColMoves = new();
        private Dictionary<int, List<int>> aiDiag1Moves = new();
        private Dictionary<int, List<int>> aiDiag2Moves = new();

        private Dictionary<int, List<int>> allMoves = new();

        private Button restartButton;
        private Label statusLabel;
        private Random random = new Random();

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
            playerRowMoves.Clear(); playerColMoves.Clear(); playerDiag1Moves.Clear(); playerDiag2Moves.Clear();
            aiRowMoves.Clear(); aiColMoves.Clear(); aiDiag1Moves.Clear(); aiDiag2Moves.Clear();
            allMoves.Clear();
            isPlayerTurn = true;
            gameOver = false;
            UpdateStatus();
            Invalidate();
        }

        private void UpdateStatus()
        {
            statusLabel.Text = isPlayerTurn ? "Your Turn (Red)" : "AI Thinking...";
        }

        // -------------------------------------------
        // HANDLE CLICKS (PLAYER MOVE)
        // -------------------------------------------
        private void PvEForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (gameOver || !isPlayerTurn) return;

            if (e.Y < OFFSET_Y || e.Y > OFFSET_Y + ROWS * CELL_SIZE)
                return;

            int col = (e.X - OFFSET_X) / CELL_SIZE;
            if (col < 0 || col >= COLS)
                return;

            int filled = allMoves.ContainsKey(col) ? allMoves[col].Count : 0;
            if (filled >= ROWS)
                return;

            int newRow = ROWS - 1 - filled;

            // record player move
            if (!allMoves.ContainsKey(col))
                allMoves[col] = new List<int>();
            allMoves[col].Add(newRow);

            AddMove(playerRowMoves, newRow, col);
            AddMove(playerColMoves, col, newRow);
            AddMove(playerDiag1Moves, col - newRow, col);
            AddMove(playerDiag2Moves, col + newRow, col);

            Invalidate();

            if (CheckWin(playerRowMoves) || CheckWin(playerColMoves) ||
                CheckWin(playerDiag1Moves) || CheckWin(playerDiag2Moves))
            {
                gameOver = true;
                statusLabel.Text = "You Win!";
                MessageBox.Show("You Win!", "Game Over");
                return;
            }

            if (IsDraw())
            {
                gameOver = true;
                statusLabel.Text = "Draw!";
                MessageBox.Show("It's a draw!", "Game Over");
                return;
            }

            isPlayerTurn = false;
            UpdateStatus();

            // Let AI move after short delay
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 600 };
            timer.Tick += (s, ev) =>
            {
                timer.Stop();
                AIMove();
            };
            timer.Start();
        }

        // -------------------------------------------
        // AI MOVE LOGIC
        // -------------------------------------------
        private void AIMove()
        {
            if (gameOver) return;

            List<int> validCols = new();
            for (int c = 0; c < COLS; c++)
            {
                if (!allMoves.ContainsKey(c) || allMoves[c].Count < ROWS)
                    validCols.Add(c);
            }

            if (validCols.Count == 0) return;

            int col = validCols[random.Next(validCols.Count)];
            int newRow = ROWS - 1 - (allMoves.ContainsKey(col) ? allMoves[col].Count : 0);

            if (!allMoves.ContainsKey(col))
                allMoves[col] = new List<int>();
            allMoves[col].Add(newRow);

            AddMove(aiRowMoves, newRow, col);
            AddMove(aiColMoves, col, newRow);
            AddMove(aiDiag1Moves, col - newRow, col);
            AddMove(aiDiag2Moves, col + newRow, col);

            Invalidate();

            if (CheckWin(aiRowMoves) || CheckWin(aiColMoves) ||
                CheckWin(aiDiag1Moves) || CheckWin(aiDiag2Moves))
            {
                gameOver = true;
                statusLabel.Text = "AI Wins!";
                MessageBox.Show("AI Wins!", "Game Over");
                return;
            }

            if (IsDraw())
            {
                gameOver = true;
                statusLabel.Text = "Draw!";
                MessageBox.Show("It's a draw!", "Game Over");
                return;
            }

            isPlayerTurn = true;
            UpdateStatus();
        }

        private void AddMove(Dictionary<int, List<int>> dict, int key, int value)
        {
            if (!dict.ContainsKey(key))
                dict[key] = new List<int>();
            dict[key].Add(value);
        }

        // -------------------------------------------
        // DRAW THE BOARD
        // -------------------------------------------
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

                    if (playerColMoves.ContainsKey(c) && playerColMoves[c].Contains(r))
                        brush = Brushes.Red;
                    else if (aiColMoves.ContainsKey(c) && aiColMoves[c].Contains(r))
                        brush = Brushes.Yellow;

                    g.FillEllipse(brush, x, y, size, size);
                    g.DrawEllipse(Pens.Black, x, y, size, size);
                }
            }
        }

        // -------------------------------------------
        // WIN & DRAW CHECK
        // -------------------------------------------
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

        private bool IsDraw()
        {
            for (int i = 0; i < COLS; i++)
            {
                if (!allMoves.ContainsKey(i) || allMoves[i].Count < ROWS)
                    return false;
            }
            return true;
        }
    }
}
