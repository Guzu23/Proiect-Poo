using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Proiect_nou_cSharp.Form1;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace Proiect_nou_cSharp
{
    internal class Joc2D
    {
        public Form1 form;
        int w, h;
        Graphics g;
        Piesa piesa;
        Piesa piesaSelectata;
        int dx, dy;
        int cStart = 300;
        int lStart = 50;
        int l = (int)(15 * Math.Sqrt(3));
        Pen creion = new Pen(Color.Black, 1);
        Brush c1 = Brushes.Blue;
        Brush c2 = Brushes.Red;
        System.Drawing.Font f = new Font("Times New Roman", 18, FontStyle.Bold | FontStyle.Italic);
        StringFormat sf;
        public Joc2D(int w, int h, Graphics g, Form1 form)
        {
            this.form = form;
            this.w = w;
            this.h = h;
            this.g = g;
            l = (int)(30 * 0.89);
            sf = new StringFormat();
            piesaSelectata = null;
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
        }
        public void JocNou(int piesaXstart, int piesaYstart, int iPion, int jPion)
        {
            piesaSelectata = null;
            adauga_piesa(piesaXstart, piesaYstart, iPion, jPion);
            deseneaza();
            form.pictureBox1.Refresh();
        }
        public void adauga_piesa(int piesaXstart, int piesaYstart, int iPion, int jPion)
        {
            piesa = new Piesa(10, 271, 20, 30, form.tiles, g, (int)(form.game_dimension_setting / 2), 0);
            aseaza_piesa(piesa, piesaXstart, piesaYstart, iPion, jPion);
        }
        public void aseaza_piesa(Piesa piesa, int x, int y, int ii, int jj)
        {
            piesa.X = x - 20 / 2;
            piesa.Y = y - 30 / 2;
            piesa.i = ii;
            piesa.j = jj;
            form.iPion = ii;
            form.jPion = jj;
        }
        //===============================
        public void selecteaza_piesa(int x, int y)
        {
            if (piesa is Piesa)
            {
                if (piesa.este_deasupra(x, y))
                {
                    piesaSelectata = piesa;
                    dx = piesa.X - x;
                    dy = piesa.Y - y;
                    cStart = piesa.X;
                    lStart = piesa.Y;
                    return;
                }
            }
            deseneaza();
        }
        public void muta_piesa(int x, int y)
        {
            if (piesaSelectata != null)
            {
                piesaSelectata.X = x + dx;
                piesaSelectata.Y = y + dy;
                deseneaza();
            }
        }
        public void elibereaza_piesa(int x, int y)
        {
            if (piesaSelectata is Piesa && piesaSelectata != null)
            {
                int pLin = piesaSelectata.i;
                int pCol = piesaSelectata.j;
                int[] dxx = new int[] { 0, 0, -1, 1, -1, 1 };
                int[] dyy = new int[] { -1, 1, 0, 0, -1, 1 };
                if (pLin < (int)form.game_dimension_setting/2)
                {
                    dxx = new int[] { 0, 0, -1, 1, -1, 1 };
                    dyy = new int[] { -1, 1, 0, 0, -1, 1 };
                }
                else if (pLin > (int)form.game_dimension_setting / 2)
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
                    int ii = pLin + dxx[i];
                    int jj = pCol + dyy[i];
                    if (ii >= 0 && ii <= 10 && jj >= 0 && jj <= 10)
                    {
                        if (form.tiles[ii, jj] != null)
                        {
                            if (IsInTile(x, y, ii, jj) && mutare_valida(ii, jj))
                            {
                                if (form.tiles[ii, jj].text == "Start") continue;
                                int CenterX = form.tiles[ii, jj].CenterX;
                                int CenterY = form.tiles[ii, jj].CenterY;
                                aseaza_piesa(piesaSelectata, CenterX, CenterY, ii, jj);
                                update_tile(ii, jj);
                                piesaSelectata = null;
                                cStart = CenterX;
                                lStart = CenterY;
                                form.xPion = CenterX;
                                form.yPion = CenterY;
                                return;
                            }
                        }
                    }
                }
                aseaza_piesa(piesaSelectata, cStart + 10, lStart + 15, piesa.i, piesa.j);
                piesaSelectata = null;
            }
            deseneaza();
        }
        public bool IsInTile(int pawnX, int pawnY, int tileRow, int tileColumn)
        {
            int tileCenterX = form.tiles[tileRow, tileColumn].CenterX;
            int tileCenterY = form.tiles[tileRow, tileColumn].CenterY;

            int deltaX = Math.Abs(pawnX - tileCenterX);
            int deltaY = Math.Abs(pawnY - tileCenterY);

            int maxDistance = (int)l;

            if ((deltaX <= 25 && deltaY <= 12) || deltaX * deltaX + deltaY * deltaY <= maxDistance * maxDistance)
                return true;
            return false;
        }
        bool mutare_valida(int i, int j)
        {
            if (form.tiles[i, j].Visited || form.tiles[i, j].Obstacle) return false;
            return true;
        }
        bool path_left(int i, int j)
        {
            int[] dxx = new int[] { 0, 0, -1, 1, -1, 1 };
            int[] dyy = new int[] { -1, 1, 0, 0, -1, 1 };
            if (i < (int)form.game_dimension_setting / 2)
            {
                dxx = new int[] { 0, 0, -1, 1, -1, 1 };
                dyy = new int[] { -1, 1, 0, 0, -1, 1 };
            }
            else if (i > (int)form.game_dimension_setting / 2)
            {
                dxx = new int[] { 0, 0, -1, 1, -1, 1 };
                dyy = new int[] { -1, 1, 0, 0, 1, -1 };
            }
            else
            {
                dxx = new int[] { 0, 0, -1, 1, -1, 1 };
                dyy = new int[] { -1, 1, 0, 0, -1, -1 };
            }
            for (int k = 0; k < 6; k++)
            {
                int newRow = i + dxx[k];
                int newCol = j + dyy[k];
                if (newRow >= 0 && newRow < form.game_dimension_setting && newCol >= 0 && newCol < form.game_dimension_setting)
                {
                    if (form.tiles[newRow, newCol] != null)
                    {
                        if (form.tiles[newRow, newCol].Obstacle == false && form.tiles[newRow, newCol].Visited == false && (newRow, newCol) != ((int)form.game_dimension_setting/2, 0))
                        {
                            return true;
                        }
                    }

                }
            }
            return false;
            
        }
        public void update_tile(int i, int j)
        {
            form.tiles[i, j].Visited = true;
            form.tiles[i, j].col = Color.Yellow;
            form.score += form.tiles[i, j].Points;
            form.moves_left--;
            if (i == (int)form.game_dimension_setting/2 && j == form.game_dimension_setting - 1) 
            {
                form.win = true;
                form.game_over = true;
                form.ingame = false;
                return;
            }
            else if (form.tiles[i,j].Portal == true)
            {
                if(i==form.ports.portal1_y && j == form.ports.portal1_x)
                {
                    int CenterX = form.tiles[form.ports.portal2_y, form.ports.portal2_x].CenterX;
                    int CenterY = form.tiles[form.ports.portal2_y, form.ports.portal2_x].CenterY;
                    
                    form.tiles[form.ports.portal2_y, form.ports.portal2_x].Visited = true;
                    form.tiles[form.ports.portal2_y, form.ports.portal2_x].col = Color.Yellow;
                    
                    cStart = CenterX;
                    lStart = CenterY;
                    i = form.ports.portal2_y;
                    j = form.ports.portal2_x;

                    form.iPion = i;
                    form.jPion = j;
                    form.xPion = CenterX;
                    form.yPion = CenterY;
                    aseaza_piesa(piesaSelectata, CenterX, CenterY, form.ports.portal2_y, form.ports.portal2_x);
                    piesaSelectata = null;
                }
                else
                {
                    int CenterX = form.tiles[form.ports.portal1_y, form.ports.portal1_x].CenterX;
                    int CenterY = form.tiles[form.ports.portal1_y, form.ports.portal1_x].CenterY;
                    
                    form.tiles[form.ports.portal1_y, form.ports.portal1_x].Visited = true;
                    form.tiles[form.ports.portal1_y, form.ports.portal1_x].col = Color.Yellow;
                    
                    cStart = CenterX;
                    lStart = CenterY;
                    i = form.ports.portal1_y;
                    j = form.ports.portal1_x;

                    form.iPion = i;
                    form.jPion = j;
                    form.xPion = CenterX;
                    form.yPion = CenterY;
                    aseaza_piesa(piesaSelectata, CenterX, CenterY, form.ports.portal1_y, form.ports.portal1_x);
                    piesaSelectata = null;
                }
                
            }    

            //Verificam daca mai sunt hexagoane libere in jurul nostru pentru a continua jocul
            if (path_left(i, j)) return;
            form.game_over = true;
            form.win = false;
        }
        //===============================
        public void deseneaza()
        {
            g.Clear(Color.White);
            deseneaza_joc();
            form.pictureBox1.Refresh();
        }
        public void deseneaza_joc()
        {
            Bitmap bmp = new Bitmap(form.pictureBox1.Width, form.pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                foreach (var tile in form.tiles)
                {
                    if (tile != null)
                    {
                        tile.drawHexagon(g);
                    }
                }
                piesa.deseneaza(g);
            }
            form.pictureBox1.Image = bmp;
            form.pictureBox1.Refresh();
        }
    }
}
