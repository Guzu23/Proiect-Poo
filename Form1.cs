using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Data.Common;
using static Proiect_nou_cSharp.Form1;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Reflection;
using System.Threading;

namespace Proiect_nou_cSharp
{
    public partial class Form1 : Form
    {
        public DateTime startTime = DateTime.Now;
        Random random = new Random();
        System.Drawing.Image img;
        Graphics g;
        Joc2D joc;
        Bitmap bitmap = new Bitmap(739, 680);

        private Update updater;
        private System.Windows.Forms.Timer updateTimer;

        string portalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Resources", "portal.png");
        string treePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Resources", "tree.png");
        string treasurePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Resources", "treasure.png");
        string surprisePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Resources", "surprise.png");
        public string leaderboardPath = Path.Combine(Directory.GetCurrentDirectory(), "leaderboard.txt");
        public string settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.txt");

        public int game_difficulty_setting { get; set; }
        public int game_dimension_setting { get; set; }
        public int maximum_number_of_steps_setting { get; set; }
        public int minutes { get; set; }
        public int seconds { get; set; }
        public int timeLeft { get; set; }
        public int timeSpent = 0;
        public int score = 0;
        public int moves_left;

        public bool apasat;
        public bool ingame = false;
        public bool game_over = false;
        public bool win = false;
        public struct portals
        {
            public int portal1_x { get; set; }
            public int portal1_y { get; set; }
            public int portal2_x { get; set; }
            public int portal2_y { get; set; }
        }
        public portals ports;

        public Tile[,] tiles = new Tile[12, 12];

        public List<(int, int)> besttPath;
        public int besttScore;
        public int[,] besttMemo;

        public int xPion = 50;
        public int yPion = 271;
        public int iPion = 0;
        public int jPion = 0;
        public Form1()
        {
            InitializeComponent();

            game_difficulty_setting = 2;
            game_dimension_setting = 9;
            maximum_number_of_steps_setting = 50;
            seconds = 300;

            updater = new Update(this);
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 16; // 60fps
            updateTimer.Tick += UpdateTimer_Tick;
            updater.UpdateLeaderboard();
            updater.UpdateSettings();
            updater.UpdateVariables();
            iPion = (int)(game_dimension_setting / 2);

            pictureBox1.MouseDown += pictureBox1_MouseDown;
            pictureBox1.MouseMove += pictureBox1_MouseMove;
            pictureBox1.MouseUp += pictureBox1_MouseUp;

            pictureBox2.Paint += pictureBox2_Paint;

            moves_left = maximum_number_of_steps_setting;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            updateTimer.Start();
            img = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(img);
            joc = new Joc2D(img.Width, img.Height, g, this);
        }
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (File.Exists("salvare.txt"))
            {
                continuaJocToolStripMenuItem.Enabled = true;
            }
            else
            {
                continuaJocToolStripMenuItem.Enabled = false;
            }

            updater.UpdateSettings();

            if (game_over == false && ingame == true)
            {
                pictureBox1.Enabled = true;
                updater.UpdateGameLabels();
                updater.UpdateVariables();
            }
            else if (game_over == true && win == true)
            {
                pictureBox1.Enabled = false;
                panel1.Enabled = true;
                panel1.Visible = true;
                (besttPath, besttScore, besttMemo) = FindHighestScorePath
                    (tiles,

                     ((int)game_dimension_setting / 2,
                     0),

                     ((int)game_dimension_setting / 2,
                     game_dimension_setting - 1),

                     game_dimension_setting,

                     ports,

                     maximum_number_of_steps_setting);

                foreach (var node in besttPath)
                {
                    tiles[node.Item1, node.Item2].col = Color.AntiqueWhite;
                    if (tiles[node.Item1, node.Item2].Portal == true)
                    {
                        tiles[ports.portal1_y, ports.portal1_x].col = Color.AliceBlue;
                        tiles[ports.portal2_y, ports.portal2_x].col = Color.AliceBlue;
                    }
                    tiles[node.Item1, node.Item2].text = besttPath.IndexOf(node).ToString();
                }
                //tiles[(int)game_dimension_setting / 2, game_dimension_setting - 1].text = besttScore.ToString();
                joc.deseneaza_joc();
            }
            else if (game_over == true && win == false && ingame == true)
            {
                youLost.Visible = true;
                youLost.Enabled = true;
                pictureBox1.Enabled = false;
                ingame = false;
            }
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            menuPanel.Visible = true;
            gamePanel.Visible = false;
            settingsPanel.Visible = false;
            leaderboardPanel.Visible = false;
            aboutPanel.Visible = false;
            ingame = false;
        }
        private void newGame_Click(object sender, EventArgs e)
        {
            youLost.Visible = false;
            youLost.Enabled = false;
            menuPanel.Visible = false;
            gamePanel.Visible = true;
        }
        private void settings_Click(object sender, EventArgs e)
        {
            menuPanel.Visible = false;
            settingsPanel.Visible = true;
            updater.UpdateSettings();
        }
        private void leaderboard_Click(object sender, EventArgs e)
        {
            menuPanel.Visible = false;
            leaderboardPanel.Visible = true;
            updater.UpdateLeaderboard();
        }
        private void about_Click(object sender, EventArgs e)
        {
            menuPanel.Visible = false;
            aboutPanel.Visible = true;
        }


        private void updateSettingsFile()
        {
            string settings = game_difficulty_setting.ToString() + "\n"
                               + game_dimension_setting.ToString() + "\n"
                               + maximum_number_of_steps_setting.ToString() + "\n"
                               + seconds.ToString() + "\n";
            File.WriteAllText(settingsPath, settings);
        }
        private void difficultyEasy_Click(object sender, EventArgs e)
        {
            game_difficulty_setting = 1;
            game_dimension_setting = 7;
            maximum_number_of_steps_setting = 100;
            seconds = 600;
            updateSettingsFile();
        }
        private void difficultyMedium_Click(object sender, EventArgs e)
        {
            game_difficulty_setting = 2;
            game_dimension_setting = 9;
            maximum_number_of_steps_setting = 50;
            seconds = 300;
            updateSettingsFile();
        }
        private void difficultyHard_Click(object sender, EventArgs e)
        {
            game_difficulty_setting = 3;
            game_dimension_setting = 11;
            maximum_number_of_steps_setting = 25;
            seconds = 180;
            updateSettingsFile();
        }
        private void usorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            game_difficulty_setting = 1;
            game_dimension_setting = 7;
            maximum_number_of_steps_setting = 100;
            seconds = 600;
            updateSettingsFile();
        }
        private void mediuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            game_difficulty_setting = 2;
            game_dimension_setting = 9;
            maximum_number_of_steps_setting = 50;
            seconds = 300;
            updateSettingsFile();
        }
        private void greuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            game_difficulty_setting = 3;
            game_dimension_setting = 11;
            maximum_number_of_steps_setting = 25;
            seconds = 180;
            updateSettingsFile();
        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            game_difficulty_setting = 4;
            game_dimension_setting = 7;
            updateSettingsFile();
        }
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            game_difficulty_setting = 4;
            game_dimension_setting = 9;
            updateSettingsFile();
        }
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            game_difficulty_setting = 4;
            game_dimension_setting = 11;
            updateSettingsFile();
        }
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            game_difficulty_setting = 4;
            maximum_number_of_steps_setting = 100;
            updateSettingsFile();
        }
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            game_difficulty_setting = 4;
            maximum_number_of_steps_setting = 50;
            updateSettingsFile();
        }
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            game_difficulty_setting = 4;
            maximum_number_of_steps_setting = 25;
            updateSettingsFile();
        }
        private void timpMaximToolStripMenuItem_Click(object sender, EventArgs e)
        {
            game_difficulty_setting = 4;
            seconds = 600;
            updateSettingsFile();
        }
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            game_difficulty_setting = 4;
            seconds = 300;
            updateSettingsFile();
        }
        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            game_difficulty_setting = 4;
            seconds = 180;
            updateSettingsFile();
        }
        private void exportFoaieDeJocToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png";
            saveFileDialog.Title = "Save Image";
            saveFileDialog.FileName = "harta.png";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                string directory = Path.GetDirectoryName(filePath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath);

                string newFilePath = filePath;
                int counter = 1;

                while (File.Exists(newFilePath))
                {
                    string numberedFileName = $"{fileNameWithoutExtension} ({counter}){extension}";
                    newFilePath = Path.Combine(directory, numberedFileName);
                    counter++;
                }

                pictureBox1.Image.Save(newFilePath, ImageFormat.Png);
                MessageBox.Show("Imagine salvata cu succes!");
            }

        }
        private void imprimareFoaieDeJocToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = new PrintDocument();

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                PrintDocument printDocument = printDialog.Document;
                printDocument.PrintPage += PrintDocument_PrintPage;
                printDocument.Print();
            }
        }
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            RectangleF bounds = e.PageBounds;

            float aspectRatio = (float)pictureBox1.Image.Width / (float)pictureBox1.Image.Height;

            float newWidth = bounds.Width;
            float newHeight = newWidth / aspectRatio;

            if (newHeight > bounds.Height)
            {
                newHeight = bounds.Height;
                newWidth = newHeight * aspectRatio;
            }

            float x = (bounds.Width - newWidth) / 2;
            float y = (bounds.Height - newHeight) / 2;

            g.DrawImage(pictureBox1.Image, x, y, newWidth, newHeight);
        }
        private void closeGameButton_Click(object sender, EventArgs e)
        {
            confirmCloseGamePanel.Visible = true;
        }
        private void yesButton_Click(object sender, EventArgs e)
        {
            game_over = true;
            win = false;
            ingame = false;

            confirmCloseGamePanel.Visible = false;
            ingamePanel.Visible = false;
            exportFoaieDeJocToolStripMenuItem.Enabled = false;
            imprimareFoaieDeJocToolStripMenuItem.Enabled = false;
            configurareJocToolStripMenuItem.Enabled = true;
            gameStartIntructions.Visible = true;
            gameStartIntructions.Enabled = true;
            panel1.Visible = false;
            panel1.Enabled = false;
            youLost.Visible = false;
            youLost.Enabled = false;
            continuaJocToolStripMenuItem.Enabled = false;

            button5.Enabled = true;
            button5.Visible = true;
        }
        private void noButton_Click(object sender, EventArgs e)
        {
            confirmCloseGamePanel.Visible = false;
        }
        private void clearLeaderboard_Click(object sender, EventArgs e)
        {
            confirmClearLeaderboardPanel.Visible = true;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            confirmClearLeaderboardPanel.Visible = false;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            leaderboardPlaces.Items.Clear();
            confirmClearLeaderboardPanel.Visible = false;
            using (var stream = new FileStream(leaderboardPath, FileMode.Truncate)) { }
            updater.UpdateLeaderboard();
        }
        private void exit_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //Introdu un nou jucator/scor in clasament
            List<string> lines = File.ReadAllLines(leaderboardPath)
                        .Select(line => line.Split(';'))
                        .OrderByDescending(tokens => tokens[1])
                        .ThenBy(tokens => int.Parse(tokens[3]))
                        .ThenByDescending(tokens => int.Parse(tokens[2]))
                        .ThenBy(tokens => tokens[0])
                        .Select(tokens => string.Join(";", tokens))
                        .ToList();
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string newLine = textBox1.Text + ";" + score + ";" + game_dimension_setting + ";" + timeSpent + ";" + currentTime;
            lines.Add(newLine);
            File.WriteAllLines(leaderboardPath, lines);

            ingame = false;
            panel1.Visible = false;
            ingamePanel.Visible = false;
            panel1.Enabled = false;
            win = false;
            gameStartIntructions.Visible = true;
            gameStartIntructions.Enabled = true;
            continuaJocToolStripMenuItem.Enabled = false;
            button5.Enabled = true;
            button5.Visible = true;
        }

        private void jocNouToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new_game();
        }

        public class Tile
        {
            public Form1 form;
            public bool Start { get; set; } = false;
            public bool Finish { get; set; } = false;
            public int CenterX { get; set; } = 0;
            public int CenterY { get; set; } = 0;
            public bool Visited { get; set; } = false;
            public bool Surprise { get; set; } = false;
            public bool Obstacle { get; set; } = false;
            public bool Treasure { get; set; } = false;
            public bool Portal { get; set; } = false;
            public bool Treasure2 { get; set; } = false;
            public bool Normal { get; set; } = false;
            public int Points { get; set; } = 0;
            public Color col { get; set; } = Color.Gray;
            public System.Drawing.Image img { get; set; }
            public string text { get; set; }
            public int i { get; set; } = -1;
            public int j { get; set; } = -1;

            public Tile(int x, int y, int points)
            {
                CenterX = x;
                CenterY = y;
                Points = points;
            }

            public Tile(Tile t)
            {
                this.Visited = t.Visited;
                this.Obstacle = t.Obstacle;
                this.Points = t.Points;
                this.Portal = t.Portal;
            }

            public void drawHexagon(Graphics g)
            {
                Point[] vertices = new Point[6];
                for (int j = 0; j < 6; j++)
                {
                    double angle = 2 * Math.PI / 6 * j + Math.PI / 6;
                    int x = (int)(CenterX + 30 * Math.Cos(angle));
                    int y = (int)(CenterY + 30 * Math.Sin(angle));
                    vertices[j] = new Point(x, y);
                }

                using (Brush brush = new SolidBrush(col))
                {
                    g.FillPolygon(brush, vertices);
                }
                using (Pen pen = new Pen(Color.Black))
                {
                    g.DrawPolygon(pen, vertices);
                }

                if (img != null)
                {
                    int imageX = CenterX - 40 / 2 + 3;
                    int imageY = CenterY - 40 / 2 + 4;
                    g.DrawImage(img, imageX, imageY);
                }

                if (text != null)
                {
                    using (Font font = new Font("Arial", 12))
                    using (Brush brush = new SolidBrush(Color.Black))
                    {
                        SizeF textSize = g.MeasureString(text, font);

                        float textX = CenterX - textSize.Width / 2;

                        float textY = CenterY - textSize.Height / 2;

                        PointF position = new PointF(textX, textY);
                        //text = CenterX.ToString() + " " + CenterY.ToString();
                        g.DrawString(text, font, brush, position);
                    }
                }
            }
        }
        public class Piesa
        {
            protected int x, y, w, h;
            public int i, j;
            Tile[,] tiles;
            System.Drawing.Image imgP;
            Graphics g;
            public Piesa(int x, int y, int w, int h, Tile[,] tiles, Graphics g, int i, int j)
            {
                this.i = i;
                this.j = j;
                this.g = g;
                this.tiles = tiles;
                this.x = x;
                this.y = y;
                this.w = w;
                this.h = h;
                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Resources", "pion.gif");
                imgP = System.Drawing.Image.FromFile(imagePath);
            }
            public bool este_deasupra(int x, int y)
            {
                if (x < this.x) return false;
                if (y < this.y) return false;
                if (x > this.x + this.w) return false;
                if (y > this.y + this.h) return false;
                return true;
            }

            public int X
            {
                get { return x; }
                set { x = value; }
            }
            public int Y
            {
                get { return y; }
                set { y = value; }

            }

            public void deseneaza(Graphics g)
            {
                g.DrawImage(imgP, X, Y, w, h);
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            apasat = true;
            joc.selecteaza_piesa(e.X, e.Y);
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (apasat)
            {
                joc.muta_piesa(e.X, e.Y);
                pictureBox1.Refresh();
            }
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            apasat = false;
            joc.elibereaza_piesa(e.X, e.Y);
            pictureBox1.Refresh();
        }
        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            Pen pen = new Pen(Color.Red, 2);
            Brush brush = new SolidBrush(Color.Blue);

            Tile start = new Tile(50, 30, 0);
            start.col = Color.Blue;
            start.text = "Start";
            start.drawHexagon(g);

            Tile finish = new Tile(50, 100, 0);
            finish.col = Color.Red;
            finish.text = "Finish";
            finish.drawHexagon(g);

            Tile tree = new Tile(50, 170, 0);
            tree.col = Color.FromArgb(131, 179, 90);
            tree.img = System.Drawing.Image.FromFile(treePath);
            tree.drawHexagon(g);

            Tile normal = new Tile(50, 240, 0);
            normal.col = Color.Gray;
            normal.text = "-13";
            normal.drawHexagon(g);

            Tile surprise = new Tile(50, 310, 0);
            surprise.col = Color.FromArgb(255, 174, 201);
            surprise.img = System.Drawing.Image.FromFile(surprisePath);
            surprise.drawHexagon(g);

            Tile treasure = new Tile(50, 380, 0);
            treasure.col = Color.Gold;
            treasure.img = System.Drawing.Image.FromFile(treasurePath);
            treasure.drawHexagon(g);

            Tile portal = new Tile(50, 450, 0);
            portal.col = ColorTranslator.FromHtml("#A349A4");
            portal.img = System.Drawing.Image.FromFile(portalPath);
            portal.drawHexagon(g);
        }

        public void reset_variables()
        {
            joc.JocNou(50, 271, iPion, jPion);
            game_over = false;
            win = false;
            ingame = true;

            startTime = DateTime.Now;
            timeSpent = 0;
            score = 0;
            game_difficulty_setting = 2;
            game_dimension_setting = 9;
            maximum_number_of_steps_setting = 50;
            seconds = 300;

            updater.UpdateLeaderboard();
            updater.UpdateSettings();

            moves_left = maximum_number_of_steps_setting;

            ingamePanel.Visible = true;
            exportFoaieDeJocToolStripMenuItem.Enabled = true;
            imprimareFoaieDeJocToolStripMenuItem.Enabled = true;
            button5.Visible = false;
            configurareJocToolStripMenuItem.Enabled = false;
            gameStartIntructions.Visible = false;
            gameStartIntructions.Enabled = false;
            youLost.Visible = false;
            youLost.Enabled = false;
        }
        public void generate_portals()
        {
            while (true)
            {
                ports.portal1_x = random.Next(0, (int)(game_dimension_setting / 2) + 1);
                ports.portal1_y = random.Next(0, game_dimension_setting);
                ports.portal2_x = random.Next(0, (int)(game_dimension_setting / 2) + 1);
                ports.portal2_y = random.Next(0, game_dimension_setting);

                if (((ports.portal1_x != ports.portal2_x) || (ports.portal1_y != ports.portal2_y))
                    //!=Start
                    && ((ports.portal1_x != 0) && (ports.portal1_y != (int)(game_dimension_setting / 2) + 1))
                    && ((ports.portal2_x != 0) && (ports.portal2_y != (int)(game_dimension_setting / 2) + 1))

                    //!=Finish
                    && ((ports.portal1_x != game_dimension_setting - 1) && (ports.portal1_y != (int)(game_dimension_setting / 2) + 1))
                    && ((ports.portal2_x != game_dimension_setting - 1) && (ports.portal2_y != (int)(game_dimension_setting / 2) + 1))
                    ) break;
            }
        }
        public Tile setTile(int i, int j, portals ports, int x, int y)
        {
            Tile tile = new Tile(x, y, 0);
            tile.CenterX = x; tile.CenterY = y;
            int randomNumber = random.Next(1, 101);

            if ((i == (int)(game_dimension_setting / 2)) && (j == 0))
            {
                tile.Start = true;
                tile.col = Color.Blue;
                tile.text = "Start";
            }
            else if ((i == (int)(game_dimension_setting / 2)) && (j == game_dimension_setting - 1))
            {
                tile.Finish = true;
                tile.col = Color.Red;
                tile.text = "Finish";
            }
            else if ((ports.portal1_x == j) && ports.portal1_y == i)
            {
                tile.Portal = true;
                tile.col = ColorTranslator.FromHtml("#A349A4");
                tile.img = System.Drawing.Image.FromFile(portalPath); ;
            }
            else if ((ports.portal2_x == j) && ports.portal2_y == i)
            {
                tile.Portal = true;
                tile.col = ColorTranslator.FromHtml("#A349A4");
                tile.img = System.Drawing.Image.FromFile(portalPath); ;
            }
            else if (randomNumber < 25)
            {
                tile.Obstacle = true;
                tile.col = Color.FromArgb(131, 179, 90);
                tile.img = System.Drawing.Image.FromFile(treePath);
            }
            else if (randomNumber < 35)
            {
                tile.Treasure2 = true;
                tile.col = Color.FromArgb(131, 179, 90);
                tile.img = System.Drawing.Image.FromFile(treePath);
                tile.Points = 200;
            }
            else if (randomNumber < 40)
            {
                tile.col = Color.Gold;
                tile.img = System.Drawing.Image.FromFile(treasurePath);
                tile.Treasure = true;
                tile.Points = 100;
            }
            else if (randomNumber < 55)
            {
                tile.col = Color.FromArgb(255, 174, 201);
                tile.Points = random.Next(-100, 100);
                tile.Surprise = true;
                tile.img = System.Drawing.Image.FromFile(surprisePath);
                //tile.text = tile.Points.ToString();
            }
            else
            {
                tile.Normal = true;
                tile.col = Color.Gray;
                tile.Points = random.Next(-20, 21);
                if (tile.Points < 0)
                {
                    tile.text = tile.Points.ToString();
                }
                else
                {
                    tile.text = "+" + tile.Points.ToString();
                }
            }
            return tile;
        }
        public void generate_tiles()
        {
            tiles = new Tile[12, 12];
            int c = 0;
            int count = (int)(game_dimension_setting / 2);
            int first_x = 50;
            int first_y = 300;
            int size = 30;
            for (int i = 0; i < game_dimension_setting; i++)
            {
                if (i < (int)(game_dimension_setting / 2) + 1)
                {
                    int x = first_x + (int)(size * 0.86) * count;
                    int y = first_y - (int)(size * 0.86) * count * 2 - 6 * i;
                    for (int j = 0; j < (int)(game_dimension_setting / 2) + i + 1; j++)
                    {
                        int xx = x + size * j * (int)(Math.Sqrt(3)) + 20 * j;
                        int yy = y;
                        tiles[i, j] = setTile(i, j, ports, xx, yy); ;
                    }
                    count--;
                }
                else
                {
                    int x = first_x + (int)(size * 0.86) * count + 50;
                    int y = first_y + (int)(size * 0.86) * count * 2 - 6 * i + 100;
                    for (int j = 0; j < game_dimension_setting - c - 1; j++)
                    {
                        int xx = x + size * j * (int)(Math.Sqrt(3)) + 20 * j;
                        int yy = y;
                        tiles[i, j] = setTile(i, j, ports, xx, yy);
                    }
                    c++;
                    count++;
                }
            }
        }
        private void new_game()
        {
            iPion = (int)game_dimension_setting / 2;
            jPion = 0;
            reset_variables();
            generate_portals();
            generate_tiles();
            while (true)
            {
                (besttPath, besttScore, besttMemo) = FindHighestScorePath
                    (tiles,

                     ((int)game_dimension_setting / 2,
                     0),


                     ((int)game_dimension_setting / 2,
                     game_dimension_setting - 1),

                     game_dimension_setting,

                     ports,

                     maximum_number_of_steps_setting);

                if (besttPath != null)
                {
                    if (besttPath.Count > 0) break;
                }
                generate_tiles();
            }
            joc.deseneaza_joc();
        }

        public static (List<(int, int)> path, int score, int[,] memo) FindHighestScorePath(Tile[,] tiles, (int, int) start, (int, int) target, int game_dimension_setting, portals ports, int maximum_number_of_steps_setting)
        {
            int rows = tiles.GetLength(0);
            int cols = tiles.GetLength(1);
            Tile[,] matrix = new Tile[rows, cols];
            int[,] memo = new int[rows, cols];
            for (int i = 0; i < game_dimension_setting; i++)
            {
                for (int j = 0; j < game_dimension_setting; j++)
                {
                    if (tiles[i, j] != null)
                    {
                        matrix[i, j] = tiles[i, j];
                        memo[i, j] = -9999999;
                    }
                }
            }
            bool[,] visited = new bool[rows, cols];

            List<(int, int)> currentPath = new List<(int, int)>();
            List<(int, int)> bestPath = new List<(int, int)>();
            int highestScore = 0;

            DFS(matrix, start, target, visited, memo, currentPath, ref bestPath, ref highestScore, game_dimension_setting, ports, maximum_number_of_steps_setting);

            return (bestPath, highestScore, memo);
        }
        private static void DFS(Tile[,] matrix, (int, int) current, (int, int) target, bool[,] visited, int[,] memo, List<(int, int)> currentPath, ref List<(int, int)> bestPath, ref int highestScore, int game_dimension_setting, portals ports, int maximum_number_of_steps_setting)
        {
            int row = current.Item1;
            int col = current.Item2;

            //Verificam daca pozitia exista sau daca e valida
            if (row < 0 || row >= matrix.GetLength(0) || col < 0 || col >= matrix.GetLength(1)) return;
            if (matrix[row, col] == null) return;
            if (matrix[row, col].Obstacle == true) return;
            if (visited[row, col]) return;
            int oldRow = row;
            int oldCol = col;

            if (matrix[oldRow, oldCol].Portal == true)
            {
                visited[oldRow, oldCol] = true;
                if (row == ports.portal1_y && col == ports.portal1_x)
                {
                    row = ports.portal2_y;
                    col = ports.portal2_x;
                }
                else
                {
                    row = ports.portal1_y;
                    col = ports.portal1_x;
                }
            }


            //Actualizam scorul
            int currentScore = CalculatePathScore(matrix, currentPath);
            if (currentScore < 0) return;

            if (true)
            {
                //Verificam daca pozitia e deja memoizata
                if (memo[row, col] > -999998)
                {
                    if (currentScore + matrix[row, col].Points < memo[row, col]) return;
                }
                memo[row, col] = currentScore + matrix[row, col].Points;
            }

            visited[row, col] = true;
            currentPath.Add(current);

            //Verificam daca a ajuns la final si daca avem un scor mai mare
            if (current == target && currentScore > highestScore)
            {
                highestScore = currentScore;
                bestPath = new List<(int, int)>(currentPath);
                //Console.Write("Solutie!\n");
                //foreach (var var in bestPath)
                //{
                //    Console.Write(var.Item1 + " " + var.Item2 + "\n");
                //}
                //Console.Write("Score for this path: " + memo[row, col] + " \n");
            }

            //Verificam vecinii
            int[] dxx = new int[] { 0, 0, -1, 1, -1, 1 };
            int[] dyy = new int[] { -1, 1, 0, 0, -1, 1 };
            if (row < (int)game_dimension_setting / 2)
            {
                dxx = new int[] { 0, 0, -1, 1, -1, 1 };
                dyy = new int[] { -1, 1, 0, 0, -1, 1 };
            }
            else if (row > (int)game_dimension_setting / 2)
            {
                dxx = new int[] { 0, 0, -1, 1, -1, 1 };
                dyy = new int[] { -1, 1, 0, 0, 1, -1 };
            }
            else
            {
                dxx = new int[] { 0, 0, -1, 1, -1, 1 };
                dyy = new int[] { -1, 1, 0, 0, -1, -1 };
            }

            for (int i = 0; i < 6; i++)
            {
                //Console.Write(i + " ");
                int newRow = row + dxx[i];
                int newCol = col + dyy[i];
                if (currentPath.Count <= maximum_number_of_steps_setting)
                    DFS(matrix, (newRow, newCol), target, visited, memo, currentPath, ref bestPath, ref highestScore, game_dimension_setting, ports, maximum_number_of_steps_setting);
            }
            //Console.Write("\n");

            currentPath.RemoveAt(currentPath.Count - 1);
            visited[row, col] = false;

            if (matrix[row, col].Portal == true)
            {
                visited[oldRow, oldCol] = false;
            }
        }
        private static int CalculatePathScore(Tile[,] matrix, List<(int, int)> path)
        {
            int score = 0;

            foreach (var cell in path)
            {
                score += matrix[cell.Item1, cell.Item2].Points;
            }

            return score;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (game_over) return;
            //Scrierea pionului, a scorului+mutarilor ramase, a portalelor si a matricei în fișier
            using (StreamWriter writer = new StreamWriter("salvare.txt"))
            {
                writer.WriteLine($"{xPion};{yPion};{iPion};{jPion}");
                writer.WriteLine($"{score};{moves_left}");
                writer.WriteLine($"{ports.portal1_x};{ports.portal1_y};{ports.portal2_x};{ports.portal2_y}");
                for (int i = 0; i < game_dimension_setting; i++)
                {
                    for (int j = 0; j < game_dimension_setting; j++)
                    {
                        if (tiles[i, j] != null)
                        {
                            Tile tile = tiles[i, j];
                            writer.WriteLine($"{i};{j};{tile.CenterX};{tile.CenterY};{tile.Points};{tile.Start};{tile.Finish};{tile.Visited};{tile.Obstacle};{tile.Surprise};{tile.Portal};{tile.Treasure};{tile.Treasure2};{tile.Normal}");
                        }
                    }
                }
            }
        }

        public void continueGame()
        {
            reset_variables();
            importaJoc();
            joc.deseneaza_joc();
        }
        public void importaJoc()
        {
            game_over = false;
            ingame = true;

            startTime = DateTime.Now;

            updater.UpdateLeaderboard();
            updater.UpdateSettings();

            tiles = new Tile[12, 12];

            using (StreamReader reader = new StreamReader("salvare.txt"))
            {

                string line = reader.ReadLine();
                string[] values = line.Split(';');
                xPion = int.Parse(values[0]);
                yPion = int.Parse(values[1]);
                iPion = int.Parse(values[2]);
                jPion = int.Parse(values[3]);
                joc.JocNou(xPion, yPion, iPion, jPion);

                line = reader.ReadLine();
                values = line.Split(';');
                score = int.Parse(values[0]);
                moves_left = int.Parse(values[1]);

                line = reader.ReadLine();
                values = line.Split(';');
                ports.portal1_x = int.Parse(values[0]);
                ports.portal1_y = int.Parse(values[1]);
                ports.portal2_x = int.Parse(values[2]);
                ports.portal2_y = int.Parse(values[3]);

                while ((line = reader.ReadLine()) != null)
                {
                    values = line.Split(';');
                    int i = int.Parse(values[0]);
                    int j = int.Parse(values[1]);
                    tiles[i, j] = new Tile(0, 0, 0);

                    tiles[i, j].CenterX = int.Parse(values[2]);
                    tiles[i, j].CenterY = int.Parse(values[3]);
                    tiles[i, j].Points = int.Parse(values[4]);
                    tiles[i, j].Start = bool.Parse(values[5]);
                    tiles[i, j].Finish = bool.Parse(values[6]);
                    tiles[i, j].Visited = bool.Parse(values[7]);
                    tiles[i, j].Obstacle = bool.Parse(values[8]);
                    tiles[i, j].Surprise = bool.Parse(values[9]);
                    tiles[i, j].Portal = bool.Parse(values[10]);
                    tiles[i, j].Treasure = bool.Parse(values[11]);
                    tiles[i, j].Treasure2 = bool.Parse(values[12]);
                    tiles[i, j].Normal = bool.Parse(values[13]);

                    Color col = Color.Yellow;
                    if (tiles[i, j].Start == true)
                    {
                        tiles[i, j].text = "Start";
                        col = Color.Blue;
                    }
                    else if (tiles[i, j].Finish == true)
                    {
                        tiles[i, j].text = "Finish";
                        col = Color.Red;
                    }
                    else if (tiles[i, j].Surprise == true)
                    {
                        tiles[i, j].img = System.Drawing.Image.FromFile(surprisePath);
                        col = Color.FromArgb(255, 174, 201);
                    }
                    else if (tiles[i, j].Obstacle == true)
                    {
                        tiles[i, j].img = System.Drawing.Image.FromFile(treePath);
                        col = Color.FromArgb(131, 179, 90);
                    }
                    else if (tiles[i, j].Portal == true)
                    {
                        tiles[i, j].img = System.Drawing.Image.FromFile(portalPath);
                        col = ColorTranslator.FromHtml("#A349A4");
                    }
                    else if (tiles[i, j].Treasure == true)
                    {
                        tiles[i, j].img = System.Drawing.Image.FromFile(treasurePath);
                        col = Color.Gold;
                    }
                    else if (tiles[i, j].Treasure2 == true)
                    {
                        tiles[i, j].img = System.Drawing.Image.FromFile(treePath);
                        col = Color.FromArgb(131, 179, 90);
                    }
                    else if (tiles[i, j].Normal == true)
                    {
                        col = Color.Gray;
                        if (tiles[i, j].Points < 0) tiles[i, j].text = tiles[i, j].Points.ToString();
                        else tiles[i, j].text = "+" + tiles[i, j].Points.ToString();
                    }

                    if (tiles[i, j].Visited == true) col = Color.Yellow;
                    tiles[i, j].col = col;
                }
            }
            File.Delete("salvare.txt");
        }
        private void continuaJocToolStripMenuItem_Click(object sender, EventArgs e)
        {
            continueGame();
        }

    }
}
