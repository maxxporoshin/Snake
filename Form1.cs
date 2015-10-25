using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Snake
{

    enum Direction { Up, Down, Right, Left }

	enum State { Game, Greeting, GameOver, Pause }

    public partial class Form1 : Form
    {
        //Size of the one cell.
        private int cellSize = 32;
        //Size of the main grid matrix.
        private Size gridSize = new Size(15, 15);
        //Main timer interval.
        private int interval = 100;
        //Snake's and food's cells reducing.
        private int sqz = 2;

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
        private Direction curDir;
		//New directions of the snake that set by player with arrows and WASD.
		private LinkedList<Direction> newDirs = new LinkedList<Direction>();
		//Position where snake will move on the next timer's tick.
		private Point nextPos;
		//Random unit, used in food spawning.
        private Random random = new Random();
		//Storing all snake free cells of the grid, used in food spawning.
        private Point[] freeCells;
		State gameState;
     
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
			setState(State.Greeting);
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

		private void setState(State state)
		{
			switch (state)
			{
				case State.Greeting:
					InitializeSnake();
					food.Clear();
					newDirs.Clear();
					newDirs.AddLast(Direction.Up);
					spawnFood();
					break;
				case State.Game:
					timer.Start();
					break;
				case State.Pause:
					timer.Stop();
					break;
				case State.GameOver:
					timer.Stop();
					break;
			}
			gameState = state;
			Invalidate();
		}

		private void moveSnake()
        {
            nextPos = snake[0];
			if (newDirs.Count == 0)
				newDirs.AddLast(curDir);
			Direction dir = newDirs.First.Value;
            switch (dir)
            {
                case Direction.Up:
                    nextPos.Y--;
                    break;
                case Direction.Down:
                    nextPos.Y++;
                    break;
                case Direction.Left:
                    nextPos.X--;
                    break;
                case Direction.Right:
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
				setState(State.GameOver);
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
			Direction dir;
			if (newDirs.Count > 0)
				dir = newDirs.Last.Value;
			else
				dir = curDir;
			bool isControlKeyPressed = false;
            switch (e.KeyCode)
            {
				//Snake movement - Arrows and WASD.
				case Keys.Left:
				case Keys.A:
                    if ((dir != Direction.Right) && (dir != Direction.Left))
                        newDirs.AddLast(Direction.Left);
					isControlKeyPressed = true;
					break;
                case Keys.Right:
				case Keys.D:
                    if ((dir != Direction.Left)  && (dir != Direction.Right))
                        newDirs.AddLast(Direction.Right);
					isControlKeyPressed = true;
					break;
				case Keys.Up:
				case Keys.W:
                    if ((dir != Direction.Down)  && (dir != Direction.Up))
                        newDirs.AddLast(Direction.Up);
					isControlKeyPressed = true;
					break;
				case Keys.Down:
				case Keys.S:
                    if ((dir != Direction.Up)  && (dir != Direction.Down))
                        newDirs.AddLast(Direction.Down);
					isControlKeyPressed = true;
					break;
                //Pause.
                case Keys.Space:
					if (gameState == State.Game)
					{
						setState(State.Pause);
					}
					else
					{
						isControlKeyPressed = true;
					}
					break;
				case Keys.Enter:
					isControlKeyPressed = true;
					break;
				//Quit.
                case Keys.Q:
				case Keys.Escape:
                    Application.Exit();
                    break;
			}
			if (isControlKeyPressed)
			{
				if (gameState != State.GameOver)
				{
					setState(State.Game);
				}
				else
				{
					setState(State.Greeting);
				}
			}			
        }

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			SolidBrush brush = new SolidBrush(Color.Black);
			byte alpha = 255;
			StringFormat sf = new StringFormat();
			sf.Alignment = StringAlignment.Center;
			sf.LineAlignment = StringAlignment.Center;
			Font font = new Font("Arial", 20);
			String text;
			switch (gameState) {
				case State.Game:
					break;
				case State.Greeting:
					text = "New Game\n\nMove Snake: Arrows or WASD\n\nPause: Space\n\nQuit: Q or ESC";
					e.Graphics.DrawString(text, font, brush, new Rectangle(new Point(0, 0), ClientSize), sf);
					alpha = 50;
					break;
				case State.Pause:
					text = "Pause";
					e.Graphics.DrawString(text, font, brush, new Rectangle(new Point(0, 0), ClientSize), sf);
					alpha = 150;
					break;
				case State.GameOver:
					text = "Game Over\n\n\nYour score: " + snake.Count.ToString();
					e.Graphics.DrawString(text, font, brush, new Rectangle(new Point(0, 0), ClientSize), sf);
					alpha = 50;
					break;
			}
			//Draw snake.
			brush.Color = Color.FromArgb(alpha, Color.BlueViolet);
			e.Graphics.FillRectangle(brush, squeezeRect(grid[snake[0].X, snake[0].Y]));
			brush.Color = Color.FromArgb(alpha, Color.ForestGreen);
			for (int i = 1; i < snake.Count; i++)
			{
				e.Graphics.FillRectangle(brush, squeezeRect(grid[snake[i].X, snake[i].Y]));
			}
			//Draw food.
			brush.Color = Color.FromArgb(alpha, Color.Black);
			foreach (Point p in food)
			{
				e.Graphics.FillRectangle(brush, squeezeRect(grid[p.X, p.Y]));
			}
			brush.Dispose();
			Pen pen = new Pen(Color.BlueViolet, 5);
			Point[] points = { new Point(0, 0), new Point(ClientSize.Width - 1, 0),
				new Point(ClientSize.Width - 1, ClientSize.Height - 1), new Point(0, ClientSize.Height - 1), new Point(0, 0) };
			e.Graphics.DrawLines(pen, points);
		}
	}
}
