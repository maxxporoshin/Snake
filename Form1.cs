using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Snake
{

    enum DirEnum { Up, Down, Right, Left }

    public partial class Form1 : Form
    {
        //Size of the one cell.
        private int cellSize = 16;
        //Size of the main grid matrix.
        private Size gridSize = new Size(15, 15);
        //Main timer interval.
        private int interval = 100;
        //Snake's and food's cells reducing.
        private int sqz = 2;
        //Draw grid by cell's cells(todo: draw by lines).
        private bool isDrawingGrid = false;

        //Main grid which is initialized in form constructor with set in gridSize size.
        private Rectangle[,] grid;
		//Positions of the snake body's cells on the grid.
        private List<Point> snake = new List<Point>();
		//Positions of the food cells on the grid.
        private List<Point> food = new List<Point>();
		//Location where mouse was clicked on form, used to drag the form.
        private Point dragFrom;
		//Main game timer.
        private Timer timer;
		//Current direction of the snake.
        private DirEnum curDir;
		//New directions of the snake that set by player with arrows and WASD.
		private LinkedList<DirEnum> newDirs = new LinkedList<DirEnum>();
		//Position where snake will move on the next timer's tick.
		private Point nextPos;
		//Random unit, used in food spawning.
        private Random random = new Random();
		//Storing all snake free cells of the grid, used in food spawning 
        private Point[] freeCells;
     
		//Initialize component, grid, timer, greet message; start new game. 
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
                snake.Add(new Point(i0, j0 + i));
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
                e.Graphics.FillRectangle(brush, squeezeRect(grid[snake[i].X, snake[i].Y])); 
            }
            brush.Dispose();
        }

        private void DrawFood(PaintEventArgs e)
        {
            SolidBrush brush = new SolidBrush(Color.Crimson);
            foreach (Point p in food)
            {
                e.Graphics.FillRectangle(brush, squeezeRect(grid[p.X, p.Y]));
            }
            brush.Dispose();
        }

        private void moveSnake()
        {
            nextPos = snake[0];
			if (newDirs.Count == 0)
				newDirs.AddLast(curDir);
			DirEnum dir = newDirs.First.Value;
            switch (dir)
            {
                case DirEnum.Up:
                    nextPos.Y--;
                    break;
                case DirEnum.Down:
                    nextPos.Y++;
                    break;
                case DirEnum.Left:
                    nextPos.X--;
                    break;
                case DirEnum.Right:
                    nextPos.X++;
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
			//Fill the freeCells array with cells in which there is no snake
			//and then choose random index from this array.
            freeCells = new Point[gridSize.Width * gridSize.Height];
            for (int i = 0; i < gridSize.Width; i++)
                for (int j = 0; j < gridSize.Height; j++)
                {
                    bool isFound = false;
                    for (int k = 0; k < snake.Count; k++)
                        if (snake[k] == new Point(i, j)) isFound = true;
                    if (!isFound) freeCells[i + j * gridSize.Width] = new Point(i, j);
                }
            Point p = freeCells[random.Next(0, freeCells.Length - 1)];
            food.Add(p);
        }

        private void newGame()
        {
			InitializeSnake();
			food.Clear();
			newDirs.Clear();
			newDirs.AddLast(DirEnum.Up);
            spawnFood();
        }

        private bool checkCollision(out bool isFoodEaten)
        {
            if (food.Contains(nextPos))
                isFoodEaten = true;
            else
                isFoodEaten = false;
            if ((nextPos.X < 0 || nextPos.Y < 0) ||
                    (nextPos.X >= gridSize.Width || nextPos.Y >= gridSize.Height))
                return true;
            foreach (Point p in snake)
            {
                if ((nextPos.X == p.X) && (nextPos.Y == p.Y))
                    return true;
            }
            return false;
        }

        private void lose()
        {
            timer.Enabled = false;
            String str = "Your Score: " + snake.Count.ToString();
            MessageBox.Show(str, "You Lose");
            newGame();
        }

		//Make the rect smaller, retain the same center point.
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
            curDir = newDirs.First.Value;
			newDirs.RemoveFirst();
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
			DirEnum dir;
			if (newDirs.Count > 0)
				dir = newDirs.Last.Value;
			else
				dir = curDir;
            switch (e.KeyCode)
            {
				//Snake movement - Arrows and WASD.
				case Keys.Left:
				case Keys.A:
                    if ((dir != DirEnum.Right) && (dir != DirEnum.Left))
                        newDirs.AddLast(DirEnum.Left);
					timer.Enabled = true;
                        break;
                case Keys.Right:
				case Keys.D:
                    if ((dir != DirEnum.Left)  && (dir != DirEnum.Right))
                        newDirs.AddLast(DirEnum.Right);
					timer.Enabled = true;
					break;
				case Keys.Up:
				case Keys.W:
                    if ((dir != DirEnum.Down)  && (dir != DirEnum.Up))
                        newDirs.AddLast(DirEnum.Up);
					timer.Enabled = true;
					break;
				case Keys.Down:
				case Keys.S:
                    if ((dir != DirEnum.Up)  && (dir != DirEnum.Down))
                        newDirs.AddLast(DirEnum.Down);
					timer.Enabled = true;
					break;
                //Pause.
                case Keys.Space:
                    timer.Enabled = timer.Enabled ? false : true;
                    break;
				//Quit.
                case Keys.Q:
                    Application.Exit();
                    break;
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
