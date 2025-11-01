using System;
using System.Drawing;
using System.Windows.Forms;
namespace sudoku_final
{
    public enum Kesulitan
    {
        Easy,
        Medium,
        Hard,
        Expert
    }

    public partial class Form1 : Form
    {
        private bool validasiAktif = false;
        private bool gameDimulai = false;
        private TextBox[,] cells = new TextBox[9, 9];
        private Timer gameTimer;
        private int elapsedSeconds = 0;
        private int errorCount = 0;
        private Label lblTimer, lblScore;
        private Random rand = new Random();
        private Button btnGenerate, btnSolve, btnReset, btnRestore, btnHint;
        private ComboBox cmbLevel;
        private Button btnExit;
        private int[,] puzzleAwal;

        public Form1()
        {
            try
            {
                InitializeComponent();
                membuatGrid();
                membuatButtons();
                CreateLabels();
                CreateTimer();
                Startgame();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saat inisialisasi Form: " + ex.Message);
            }
            this.BackColor = Color.Black;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MessageBox.Show("Sudoku started");
        }
        private void membuatGrid()
        {
            int size = 50;
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    var tb = new TextBox
                    {
                        Width = size,
                        Height = size,
                        Location = new Point(col * size, row * size),
                        TextAlign = HorizontalAlignment.Center,
                        Font = new Font("Segoe UI", 14),
                        MaxLength = 1,
                        Tag = (row, col),
                        ForeColor = Color.DeepPink,
                        BackColor = Color.Black
                    };
                    tb.KeyPress += Cell_KeyPress;
                    tb.KeyDown += Cell_KeyDown;
                    tb.TextChanged += Cell_TextChanged;
                    cells[row, col] = tb;
                    this.Controls.Add(tb);
                }
            }
        }

        private void membuatButtons()
        {
            btnGenerate = new Button { Text = "Generate", Location = new Point(500, 50), BackColor = Color.White, ForeColor = Color.Black };
            btnGenerate.Click += BtnGuardedClick((s, e) =>
            {
                Kesulitan level = (Kesulitan)cmbLevel.SelectedIndex;
                LoadPuzzle(GeneratePuzzle(level));
            });
            btnGenerate.Enabled = false;
            this.Controls.Add(btnGenerate);

            btnSolve = new Button { Text = "Solving", Location = new Point(500, 90), BackColor = Color.White, ForeColor = Color.Black };
            btnSolve.Click += BtnGuardedClick((s, e) => SolvePuzzle());
            btnSolve.Enabled = false;
            this.Controls.Add(btnSolve);

            btnReset = new Button {Text = "Reset", Location = new Point(500, 130), BackColor = Color.White, ForeColor = Color.Black };
            btnReset.Click += BtnGuardedClick((s, e) => ClearGrid());
            btnReset.Enabled = false;
            this.Controls.Add(btnReset);

            btnHint = new Button
            {
                Text = "Hint",
                Location = new Point(500, 170),
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            btnHint.Click += BtnGuardedClick((s, e) => beriBantuan());
            btnHint.Enabled = false;
            this.Controls.Add(btnHint);

            cmbLevel = new ComboBox
            {
                Location = new Point(500, 10),
                Width = 75,
                BackColor = Color.White,
                ForeColor = Color.Black,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbLevel.Items.AddRange(new string[] { "Easy", "Medium", "Hard", "Expert" });
            cmbLevel.SelectedIndex = 0;
            this.Controls.Add(cmbLevel);

            btnExit = new Button
            {
                Text = "Keluar",
                Location = new Point(500, 250),
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            btnExit.Click += (s, e) =>
            {
                var result = MessageBox.Show("leave?", "Confirmation", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    this.Close();
                }
            };
            this.Controls.Add(btnExit);

            btnRestore = new Button
            {
                Text = "Restore",
                Location = new Point(500, 210),
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            btnRestore.Click += BtnGuardedClick((s, e) => RestorePuzzle());
            btnRestore.Enabled = false;
            this.Controls.Add(btnRestore);
        }

        private EventHandler BtnGuardedClick(EventHandler actualHandler)
        {
            return (sender, e) =>
            {
                if (!gameDimulai)
                {
                    MessageBox.Show("Press Start button first!");
                }
                actualHandler(sender, e);
            };
        }

        private void Startgame()
        {
            if (btnGenerate == null) MessageBox.Show("btnGenerate belum dibuat");
            if (btnSolve == null) MessageBox.Show("btnSolve belum dibuat");
            if (btnReset == null) MessageBox.Show("btnReset belum dibuat");
            ClearGrid();
            gameDimulai = true;

            btnGenerate.Enabled = true;
            btnSolve.Enabled = true;
            btnReset.Enabled = true;
            btnHint.Enabled = true;
            btnRestore.Enabled = true;

            if (gameDimulai)
            {
                return;
            }
        }

        private void RestorePuzzle()
        {
            if (puzzleAwal == null) return;

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (puzzleAwal[r, c] != 0)
                    {
                        cells[r, c].Text = puzzleAwal[r, c].ToString();
                        cells[r, c].BackColor = Color.LightGray;
                        cells[r, c].ReadOnly = true;
                    }
                    else
                    {
                        cells[r, c].Text = "";
                        cells[r, c].BackColor = Color.White;
                        cells[r, c].ReadOnly = false;
                    }
                }
            }
        }

        private void CreateLabels()
        {
            lblTimer = new Label
            {
                Text = "Time: 0s",
                Location = new Point(500, 290),
                Width = 100,
                BackColor = Color.Black,
                ForeColor = Color.DeepPink
            };
            lblScore = new Label
            {
                Text = "Score: 0",
                Location = new Point(500, 330),
                Width = 100,
                BackColor = Color.Black,
                ForeColor = Color.DeepPink
            };
            this.Controls.Add(lblTimer);
            this.Controls.Add(lblScore);
        }

        private void CreateTimer()
        {
            gameTimer = new Timer { Interval = 1000 };
            gameTimer.Tick += (s, e) =>
            {
                elapsedSeconds++;
                lblTimer.Text = $"Time: {elapsedSeconds}";
                UpdateScore();
            };
        }

        private void Cell_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && (e.KeyChar < '1' || e.KeyChar > '9'))
                e.Handled = true;
        }

        private void Cell_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && sender is TextBox tb)
            {
                var (row, col) = ((int, int))tb.Tag;
                if (col < 8) cells[row, col + 1].Focus();
                else if (row < 8) cells[row + 1, 0].Focus();
            }
        }

        private void Cell_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == null) return;

            if (int.TryParse(tb.Text, out int val))
            {
                var (row, col) = ((int, int))tb.Tag;
                int[,] board = GetBoard();

                if (validasiAktif && val >= 1 && val <= 9 && IsValid(board, row, col, val))
                {
                    tb.BackColor = Color.LightGreen;
                    tb.ForeColor = Color.DeepPink;
                }
                else if (validasiAktif)
                {
                    tb.BackColor = Color.LightCoral;
                    tb.ForeColor = Color.DeepPink;
                    errorCount++;
                }
                else
                {
                    tb.BackColor = Color.White;
                    tb.ForeColor = Color.DeepPink;
                }
            }
            else
            {
                tb.BackColor = Color.White;
            }
            if (validasiAktif)
            {
                CheckCompletion();
                UpdateScore();
            }
        }

        private void CheckCompletion()
        {
            int[,] board = GetBoard();

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (board[r, c] == 0 || !IsValid(board, r, c, board[r, c]))
                    {
                        return;

                    }
                }
            }
            gameTimer.Stop();
            MessageBox.Show("Sudoku completed!");
        }

        private void beriBantuan()
        {
            int[,] board = GetBoard();
            int[,] solution = new int[9, 9];
            Array.Copy(board, solution, board.Length);

            if (!Solve(solution))
            {
                MessageBox.Show("Sudoku tidak bisa diselesaikan.");
                return;
            }

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (board[r, c] == 0)
                    {
                        cells[r, c].Text = solution[r, c].ToString();
                        cells[r, c].BackColor = Color.Khaki;
                        cells[r, c].ReadOnly = true;
                        return;
                    }
                }
            }
            MessageBox.Show("Tidak ada sel kosong untuk diberi Bantuan.");
        }

        private void UpdateScore()
        {
            int score = Math.Max(1000 - elapsedSeconds - errorCount * 50, 0);
            lblScore.Text = $"Score: {score}";
        }

        private void ClearGrid(bool resetTimer = true)
        {
            foreach (var tb in cells)
            {
                tb.Text = "";
                tb.BackColor = Color.White;
                tb.ReadOnly = false;
            }

            if (resetTimer)
            {
                elapsedSeconds = 0;
                errorCount = 0;
                lblTimer.Text = "Time: 0s";
                lblScore.Text = "Score: 0";
                gameTimer.Stop();
            }
        }

        private void LoadPuzzle(int[,] puzzle)
        {
            ClearGrid();
            puzzleAwal = (int[,])puzzle.Clone();
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    if (puzzle[r, c] != 0)
                    {
                        cells[r, c].Text = puzzle[r, c].ToString();
                        cells[r, c].BackColor = Color.LightGray;
                        cells[r, c].ReadOnly = true;
                    }
            elapsedSeconds = 0;
            errorCount = 0;
            lblTimer.Text = "Time: 0s";
            lblScore.Text = "Score: 0";
            gameTimer.Start();
            validasiAktif = true; // Aktifkan validasi setelah puzzle dimuat
        }

        private void SolvePuzzle()
        {
            int[,] board = GetBoard();
            if (Solve(board))
            {
                for (int r = 0; r < 9; r++)
                    for (int c = 0; c < 9; c++)
                        cells[r, c].Text = board[r, c].ToString();
                gameTimer.Stop();
            }
            else
            {
                MessageBox.Show("Puzzle tidak bisa diselesaikan.");
            }
        }

        private int[,] GetBoard()
        {
            int[,] board = new int[9, 9];
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    board[r, c] = int.TryParse(cells[r, c].Text, out int val) ? val : 0;
            return board;
        }

        private bool IsValid(int[,] board, int row, int col, int num)
        {
            for (int i = 0; i < 9; i++)
                if ((board[row, i] == num && i != col) || (board[i, col] == num && i != row))
                    return false;

            int startRow = row / 3 * 3, startCol = col / 3 * 3;
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                {
                    int rr = startRow + r, cc = startCol + c;
                    if ((rr != row || cc != col) && board[rr, cc] == num)
                        return false;
                }

            return true;
        }

        private bool Solve(int[,] board)
        {
            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 9; col++)
                    if (board[row, col] == 0)
                    {
                        for (int num = 1; num <= 9; num++)
                        {
                            if (IsValid(board, row, col, num))
                            {
                                board[row, col] = num;
                                if (Solve(board)) return true;
                                board[row, col] = 0;
                            }
                        }
                        return false;
                    }
            return true;
        }

        private int[,] GeneratePuzzle(Kesulitan level)
        {
            int[,] fullBoard = GenerateFullBoard();
            int clues; 
            switch (level)
            {
                case Kesulitan.Easy: clues = 40; break;
                case Kesulitan.Medium: clues = 34; break;
                case Kesulitan.Hard: clues = 30; break;
                case Kesulitan.Expert: clues = 24; break;
                default: clues = 36; break;
            };
            return RemoveCells(fullBoard, clues);
        }

        private int[,] RemoveCells(int[,] board, int clues)
        {
            int[,] puzzle = (int[,])board.Clone();
            int cellsToRemove = 81 - clues;
            Random rand = new Random();

            while (cellsToRemove > 0)
            {
                int row = rand.Next(0, 9);
                int col = rand.Next(0, 9);

                if (puzzle[row, col] != 0)
                {
                    puzzle[row, col] = 0;
                    cellsToRemove--;
                }
            }
            return puzzle;
        }
        private void FillDiagonalBlocks(int[,] board)
        {
            for (int i = 0; i < 9; i += 3)
                FillBlock(board, i, i);
        }

        private void FillBlock(int[,] board, int row, int col)
        {
            int[] nums = Shuffle();
            int idx = 0;
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    board[row + r, col + c] = nums[idx++];
        }

        private int[] Shuffle()
        {
            int[] nums = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            for (int i = nums.Length - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (nums[i], nums[j]) = (nums[j], nums[i]);
            }
            return nums;
        }

        private int[,] GenerateFullBoard()
        {
            int[,] board = new int[9, 9];
            Solve(board);
            return board;
        }
    }
}
