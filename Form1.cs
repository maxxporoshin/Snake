using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snake
{
    struct Pos
    {
        public int i;
        public int j;
        public Pos(int i, int j) { this.i = i; this.j = j; }
        public static bool operator ==(Pos p1, Pos p2)
        {
            if ((p1.i == p2.i) && (p1.j == p2.j))
                return true;
            else
                return false;
        }
        public static bool operator !=(Pos p1, Pos p2)
        {
            return !(p1 == p2);
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    enum DirEnum { Up, Down, Right, Left }

    public partial class Form1 : Form
    {

        private int cellSize = 16; //size of the one cell
        private Size gridSize = new Size(15, 15); //size of the main grid matrix
        private int interval = 100; //main timer interval
        private int sqz = 2; //snake's and food's rects reducing
        private bool isDrawingGrid = false;

        private Rectangle[,] grid;
        private List<Pos> snake = new List<Pos>();
        private List<Pos> food = new List<Pos>();
        private Point dragFrom;
        private Timer timer;
        private DirEnum curDir;
        private DirEnum newDir;
        private Pos nextPos;
        private bool isWaitingForUser = true;
        private Random random = new Random();
        private Pos[] freeCells;
     
        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            this.ClientSize = new Size(cellSize * gridSize.Width, cellSize * gridSize.Height);
            timer = new Timer();
            timer.Tick += new System.EventHandler(this.OnTimerTick);
            timer.Interval = interval;
            InitializeGrid();
            MessageBox.Show("Move snake: Arrows" + Environment.NewLine 
                + "Pause: Space" + Environment.NewLine + "Quit: Q", "Welcome");
            newGame();
        }

        private void InitializeGrid()
        {
            int n = this.ClientSize.Width / cellSize;
            int m = this.ClientSize.Height / cellSize;
            grid = new Rectangle[n, m];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    grid[i, j] = new Rectangle(cellSize * i, cellSize * j, cellSize, cellSize);
                }
        }

        private void InitializeSnake()
        {
            snake.Clear();
            int i0 = gridSize.Width / 2, j0 = gridSize.Height / 2;
            for (int i = 0; i < 5; i++)
            {
                snake.Add(new Pos(i0, j0 + i));
            }
        }

        private void DrawGrid(PaintEventArgs e)
        {
            Pen pen = new Pen(Color.Black);
            for (int i = 0; i < grid.GetLength(0); i++)
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    e.Graphics.DrawRectangle(pen, grid[i, j]);
                }
            pen.Dispose();
        }

        private void DrawSnake(PaintEventArgs e)
        {
            SolidBrush brush = new SolidBrush(Color.Black);
            for (int i = 0; i < snake.Count; i++)
            {
                e.Graphics.FillRectangle(brush, squeezeRect(grid[snake[i].i, snake[i].j])); 
            }
            brush.Dispose();
        }

        private void DrawFood(PaintEventArgs e)
        {
            SolidBrush brush = new SolidBrush(Color.Crimson);
            foreach (Pos p in food)
            {
                e.Graphics.FillRectangle(brush, squeezeRect(grid[p.i, p.j]));
            }
            brush.Dispose();
        }

        private void moveSnake()
        {
            nextPos = snake[0];
            switch (newDir)
            {
                case DirEnum.Up:
                    nextPos.j--;
                    break;
                case DirEnum.Down:
                    nextPos.j++;
                    break;
                case DirEnum.Left:
                    nextPos.i--;
                    break;
                case DirEnum.Right:
                    nextPos.i++;
                    break;
            }
            bool isFoodEaten;
            if (!checkCollision(out isFoodEaten))
            {
                if (isFoodEaten)
                {
                    snake.Insert(0, nextPos);
                    food.Remove(nextPos);
                    spawnFood();
                }
                else
                {
                    for (int k = snake.Count - 1; k > 0; k--)
                    {
                        snake[k] = snake[k - 1];
                    }
                    snake[0] = nextPos;
                }
            }
            else
                lose();
        }

        private void spawnFood()
        {
            freeCells = new Pos[gridSize.Width * gridSize.Height];
            for (int i = 0; i < gridSize.Width; i++)
                for (int j = 0; j < gridSize.Height; j++)
                {
                    bool isFound = false;
                    for (int k = 0; k < snake.Count; k++)
                        if (snake[k] == new Pos(i, j)) isFound = true;
                    if (!isFound) freeCells[i + j * gridSize.Width] = new Pos(i, j);
                }
            Pos p = freeCells[random.Next(0, freeCells.Length - 1)];
            food.Add(p);
        }

        private void newGame()
        {
            if (isWaitingForUser)
            {
                InitializeSnake();
                food.Clear();
                newDir = DirEnum.Up;
            }
            else
            {
                timer.Start();
                spawnFood();
            }
        }

        private bool checkCollision(out bool isFoodEaten)
        {
            if (food.Contains(nextPos))
                isFoodEaten = true;
            else
                isFoodEaten = false;
            if ((nextPos.i < 0 || nextPos.j < 0) ||
                    (nextPos.i >= gridSize.Width || nextPos.j >= gridSize.Height))
                return true;
            foreach (Pos p in snake)
            {
                if ((nextPos.i == p.i) && (nextPos.j == p.j))
                    return true;
            }
            return false;
        }

        private void lose()
        {
            timer.Stop();
            String str = "Your Score: " + snake.Count.ToString();
            MessageBox.Show(str, "You Lose");
            isWaitingForUser = true;
            newGame();
        }

        private Rectangle squeezeRect(Rectangle rect)
        {
            return new Rectangle(rect.X + sqz, rect.Y + sqz, rect.Width - 2 * sqz, rect.Height - 2 * sqz);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            dragFrom = e.Location;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - dragFrom.X;
                this.Top += e.Y - dragFrom.Y; 
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            moveSnake();
            curDir = newDir;
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                //arrow keys - snake movement
                case Keys.Left:
                    if (curDir != DirEnum.Right)
                        newDir = DirEnum.Left;
                        break;
                case Keys.Right:
                    if (curDir != DirEnum.Left)
                        newDir = DirEnum.Right;
                        break;
                case Keys.Up:
                    if (curDir != DirEnum.Down)
                        newDir = DirEnum.Up;
                        break;
                case Keys.Down:
                    if (curDir != DirEnum.Up)
                        newDir = DirEnum.Down;
                        break;
                //utility
                case Keys.Space: //pause
                    timer.Enabled = timer.Enabled ? false : true;
                    break;
                case Keys.Q: //exit
                    Application.Exit();
                    break;
           }
                if (isWaitingForUser)
                {
                    isWaitingForUser = false;
                    newGame();
                }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (isDrawingGrid) DrawGrid(e);
            DrawSnake(e);
            DrawFood(e);
        }
    }
}
