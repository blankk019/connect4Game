using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Connect4
{
    public partial class PvPForm : Form
    {
        private const int ROWS = 6;
        private const int COLS = 7;

        private TableLayoutPanel gridPanel;
        private Button[] dropButtons;
        private Button[,] cellButtons;
        private Label statusLabel;
        private Button restartButton;

        // Your dictionary-based storage
        private Dictionary<int, List<int>> player1Moves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> player2Moves = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> allMoves = new Dictionary<int, List<int>>(); // tracks every piece per column

        private bool isPlayerOneTurn = true;

        public PvPForm()
        {
            Text = "Connect4 — PvP";
            Size = new Size(900, 800); // Set initial size
            MinimumSize = new Size(900, 800); // Set minimum size
            StartPosition = FormStartPosition.CenterScreen; // Center on screen
            BuildUi();
            ResetGame();
        }

        private void BuildUi()
        {
            // Parent panel to hold topPanel and gridPanel
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(0),
                BackColor = Color.Transparent
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 90)); // Top panel height
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Grid panel fills remaining space

            // Top status + restart
            var topPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, // Fill the row
                Height = 80,           // Match mainPanel row height
                Padding = new Padding(8),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            statusLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Text = "Turn: Player 1 (Red)",
                TextAlign = ContentAlignment.MiddleLeft
            };

            restartButton = new Button
            {
                Text = "Restart",
                Size = new Size(120, 48), // Fixed size for better visibility
                Margin = new Padding(8, 8, 8, 8),
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            restartButton.Click += (s, e) => ResetGame();

            topPanel.Controls.Add(statusLabel);
            topPanel.Controls.Add(new Label { Width = 20 }); // spacer
            topPanel.Controls.Add(restartButton);

            // Grid (header row for drop buttons + 6 rows for cells)
            gridPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = COLS,
                RowCount = ROWS + 1, // +1 for drop header
                Padding = new Padding(8),
                BackColor = Color.Transparent
            };

            // Column style
            for (int c = 0; c < COLS; c++)
            {
                gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / COLS));
            }

            // Row styles (header fixed, rest equal)
            gridPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // header row height increased
            for (int r = 0; r < ROWS; r++)
                gridPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / ROWS));

            // create drop buttons
            dropButtons = new Button[COLS];
            for (int c = 0; c < COLS; c++)
            {
                var btn = new Button
                {
                    Dock = DockStyle.Fill,
                    Text = "PLAY HERE",
                    Font = new Font("Segoe UI", 16, FontStyle.Bold), // reduced font size
                    Tag = c,
                    Margin = new Padding(4),
                    Padding = new Padding(6) // reduced padding
                };
                btn.Click += DropButton_Click;
                dropButtons[c] = btn;
                gridPanel.Controls.Add(btn, c, 0);
            }

            // create cell buttons (row 0 top, row ROWS-1 bottom)
            cellButtons = new Button[ROWS, COLS];
            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    var cell = new Button
                    {
                        Dock = DockStyle.Fill,
                        Enabled = false, // user clicks header only
                        BackColor = Color.LightGray,
                        Margin = new Padding(6),
                        FlatStyle = FlatStyle.Flat
                    };
                    cell.Tag = new Point(c, r);
                    cellButtons[r, c] = cell;
                    gridPanel.Controls.Add(cell, c, r + 1);
                }
            }

            // Add panels to mainPanel
            mainPanel.Controls.Add(topPanel, 0, 0);
            mainPanel.Controls.Add(gridPanel, 0, 1);

            // Add mainPanel to form
            Controls.Add(mainPanel);
        }

        private void DropButton_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            int col = (int)btn.Tag;

            int filled = allMoves.ContainsKey(col) ? allMoves[col].Count : 0;
            if (filled >= ROWS)
            {
                // already full — disable the button
                btn.Enabled = false;
                return;
            }

            int newRow = ROWS - 1 - filled; // bottom-most available row

            // Update global column record
            if (!allMoves.ContainsKey(col)) allMoves[col] = new List<int>();
            allMoves[col].Add(newRow);

            // Update player record
            var playerMoves = isPlayerOneTurn ? player1Moves : player2Moves;
            if (!playerMoves.ContainsKey(col)) playerMoves[col] = new List<int>();
            playerMoves[col].Add(newRow);

            // Update UI
            var cell = cellButtons[newRow, col];
            cell.BackColor = isPlayerOneTurn ? Color.Red : Color.Gold;
            // optional: disable cell so it doesn't get focus
            cell.Enabled = false;

            // If column is full now, disable header
            if (allMoves[col].Count >= ROWS)
                dropButtons[col].Enabled = false;

            // Check win for current player
            var winningCells = CheckWin(playerMoves, col, newRow);
            if (winningCells != null)
            {
                HighlightWinningCells(winningCells);
                MessageBox.Show($"Player {(isPlayerOneTurn ? 1 : 2)} wins!", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DisableAllColumns();
                statusLabel.Text = $"Player {(isPlayerOneTurn ? 1 : 2)} wins!";
                return;
            }

            // Check draw (no enabled drop buttons)
            if (IsBoardFull())
            {
                MessageBox.Show("Draw — board is full.", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
                statusLabel.Text = "Draw";
                return;
            }

            // Switch turn
            isPlayerOneTurn = !isPlayerOneTurn;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            statusLabel.Text = $"Turn: Player {(isPlayerOneTurn ? 1 : 2)} {(isPlayerOneTurn ? "(Red)" : "(Yellow)")}";
        }

        private bool IsBoardFull()
        {
            for (int c = 0; c < COLS; c++)
            {
                if (dropButtons[c].Enabled) return false;
            }
            return true;
        }

        private void DisableAllColumns()
        {
            for (int c = 0; c < COLS; c++) dropButtons[c].Enabled = false;
        }

        private void HighlightWinningCells(List<Point> winCells)
        {
            foreach (var p in winCells)
            {
                // p.X = col, p.Y = row
                var b = cellButtons[p.Y, p.X];
                b.BackColor = Color.LimeGreen; // highlight
                // optional: thick border (FlatAppearance only available on FlatStyle)
                b.FlatStyle = FlatStyle.Flat;
            }
        }

        private void ResetGame()
        {
            // clear data
            player1Moves.Clear();
            player2Moves.Clear();
            allMoves.Clear();
            isPlayerOneTurn = true;
            // reset UI
            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    cellButtons[r, c].BackColor = Color.LightGray;
                    cellButtons[r, c].Enabled = false;
                    cellButtons[r, c].FlatStyle = FlatStyle.Standard;
                }
            }
            for (int c = 0; c < COLS; c++)
            {
                dropButtons[c].Enabled = true;
            }
            UpdateStatus();
        }

        // returns list of points that form the winning 4+ line OR null if no win
        private List<Point> CheckWin(Dictionary<int, List<int>> moves, int col, int row)
        {
            // check vertical (dCol=0,dRow=1)
            var win = CollectLine(moves, col, row, 0, 1)
                   ?? CollectLine(moves, col, row, 1, 0)    // horizontal
                   ?? CollectLine(moves, col, row, 1, 1)    // diagonal down-right
                   ?? CollectLine(moves, col, row, 1, -1);  // diagonal up-right
            return win;
        }

        // Collects all connected cells in both directions for given direction (dCol,dRow).
        // returns list if count >= 4, otherwise null.
        private List<Point> CollectLine(Dictionary<int, List<int>> moves, int startCol, int startRow, int dCol, int dRow)
        {
            var cells = new List<Point> { new Point(startCol, startRow) };

            // forward direction
            int c = startCol + dCol, r = startRow + dRow;
            while (c >= 0 && c < COLS && r >= 0 && r < ROWS)
            {
                if (moves.ContainsKey(c) && moves[c].Contains(r))
                    cells.Add(new Point(c, r));
                else
                    break;
                c += dCol; r += dRow;
            }

            // backward direction
            c = startCol - dCol; r = startRow - dRow;
            while (c >= 0 && c < COLS && r >= 0 && r < ROWS)
            {
                if (moves.ContainsKey(c) && moves[c].Contains(r))
                    cells.Add(new Point(c, r));
                else
                    break;
                c -= dCol; r -= dRow;
            }

            return cells.Count >= 4 ? cells : null;
        }
    }
}
