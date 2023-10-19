using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace Proiect_nou_cSharp
{
    internal class Update
    {
        private Form1 form;
        public Update(Form1 form)
        {
            this.form = form;
        }
        
        static string ReadLine(string path, int lineNumber)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                for (int i = 1; i < lineNumber; i++)
                {
                    if (reader.ReadLine() == null)
                    {
                        return null;
                    }
                }
                return reader.ReadLine();
            }
        }

        //Update Game
        public void UpdateGameLabels()
        {
            
            form.timeSpent = (int)(DateTime.Now - form.startTime).TotalSeconds;
            int days = (int)form.timeSpent / 86400;
            int hours = (int)form.timeSpent / 3600;
            int minutes = (int)form.timeSpent / 60;
            int secondss = (int)form.timeSpent % 60;
            string label;
            if (days == 0)
            {
                if (hours == 0)
                {
                    if (minutes == 0) label = "Timp jucat: " + secondss.ToString() + " secunde";
                    else label = "Timp jucat: " + minutes.ToString() + " minute si " + secondss.ToString() + " secunde";
                }
                else label = "Timp jucat: " + hours.ToString() + " ore, " + minutes.ToString() + " minute si " + secondss.ToString() + " secunde";
            }
            else label = "Timp jucat: " + days.ToString() + " zile, " + hours.ToString() + " ore, " + minutes.ToString() + " minute si " + secondss.ToString() + " secunde";
            if (days == 1) label = label.Replace("ys", "y");
            if (hours == 1) label = label.Replace("rs", "r");
            if (minutes == 1) label = label.Replace("es", "e");
            if (secondss == 1) label = label.Replace("ds", "d");
            form.timeSpentLabel.Text = label;

            form.timeLeft = form.seconds - form.timeSpent;
            days = (int)form.timeLeft / 86400;
            hours = (int)form.timeLeft / 3600;
            minutes = (int)form.timeLeft / 60;
            secondss = (int)form.timeLeft % 60;
            if (days == 0)
            {
                if (hours == 0)
                {
                    if (minutes == 0) label = "Timp ramas: " + secondss.ToString() + " secunde";
                    else label = "Timp ramas: " + minutes.ToString() + " minute si " + secondss.ToString() + " secunde";
                }
                else label = "Timp ramas: " + hours.ToString() + " ore, " + minutes.ToString() + " minute si " + secondss.ToString() + " secunde";
            }
            else label = "Timp ramas: " + days.ToString() + " zile, " + hours.ToString() + " ore, " + minutes.ToString() + " minute si " + secondss.ToString() + " secunde";
            if (days == 1) label = label.Replace("ys", "y");
            if (hours == 1) label = label.Replace("rs", "r");
            if (minutes == 1) label = label.Replace("es", "e");
            if (secondss == 1) label = label.Replace("ds", "d");
            form.timeLeftLabel.Text = label;

            form.scoreLabel.Text = "Scor: " + form.score.ToString();
            if (form.game_difficulty_setting == 1) form.difficultyLabel.Text = "Dificultate: Usor";
            if (form.game_difficulty_setting == 2) form.difficultyLabel.Text = "Dificultate: Mediu";
            if (form.game_difficulty_setting == 3) form.difficultyLabel.Text = "Dificultate: Greu";
            if (form.game_difficulty_setting == 4) form.difficultyLabel.Text = "Dificultate: Personalizat";

            form.movesLeft.Text = "Mutari ramase: " + form.moves_left.ToString() + "/" + form.maximum_number_of_steps_setting;

            form.label4.Text = "Statistici:\n"
                     + "Scor: " + form.score.ToString() + "\n"
                     + "Scor maximal: " + form.besttScore.ToString() + "\n"
                     + "Timp: " + form.timeSpent.ToString() + "\n"
                     + "Timp ramas: " + form.timeLeft.ToString() + "\n"
                     + "Dificultate: " + form.game_difficulty_setting.ToString() + "\n"
                     + "Dimensiune: " + form.game_dimension_setting.ToString() + "\n"
                     + "Mutari ramase:" + (form.moves_left-1).ToString() + "\n";

        }
        public void UpdateVariables()
        {
            if (form.timeLeft <= 0 || form.moves_left <= 0 || form.score < 0)
            {
                form.game_over = true;
                form.win = false;
            }
        }

        //Update Settings
        public void UpdateSettings()
        {

            form.game_difficulty_setting = 2;
            form.game_dimension_setting = 9;
            form.maximum_number_of_steps_setting = 50;
            form.seconds = 300;

            if (File.Exists(form.settingsPath))
            {

                string line;
                try
                {
                    line = ReadLine(form.settingsPath, 1);
                    form.game_difficulty_setting = Int32.Parse(line);

                    line = ReadLine(form.settingsPath, 2);
                    form.game_dimension_setting = Int32.Parse(line);

                    line = ReadLine(form.settingsPath, 3);
                    form.maximum_number_of_steps_setting = Int32.Parse(line);

                    line = ReadLine(form.settingsPath, 4);
                    form.seconds = Int32.Parse(line);

                }
                catch
                {

                }
                string settings = form.game_difficulty_setting.ToString() + "\n"
                               + form.game_dimension_setting.ToString() + "\n"
                               + form.maximum_number_of_steps_setting.ToString() + "\n"
                               + form.seconds.ToString() + "\n";
                File.WriteAllText(form.settingsPath, settings);
            }

            //Dezactivam toate butoanele si le activam in functie de setari:
            form.usorToolStripMenuItem.Checked = false; //usor
            form.mediuToolStripMenuItem.Checked = false; //mediu
            form.greuToolStripMenuItem.Checked = false; //greu
            form.personalizatToolStripMenuItem.Checked = false; //personalizat

            form.toolStripMenuItem2.Checked = false; // 7
            form.toolStripMenuItem3.Checked = false; // 9
            form.toolStripMenuItem4.Checked = false; // 11

            form.toolStripMenuItem5.Checked = false; // 100
            form.toolStripMenuItem6.Checked = false; // 50
            form.toolStripMenuItem7.Checked = false; // 25

            form.toolStripMenuItem8.Checked = false; // 10
            form.toolStripMenuItem9.Checked = false; // 5
            form.toolStripMenuItem10.Checked = false; // 3
            if (form.game_difficulty_setting == 1)
            {
                form.settingsLabel.Text = "Dimensiune: 7 \nDificultate: Easy \nNumar maxim de pasi: 100 \nTimp maxim: 10 minute";
                form.settingsLabel.BackColor = Color.LawnGreen;
                form.usorToolStripMenuItem.Checked = true;
            }
            else if (form.game_difficulty_setting == 2)
            {
                form.settingsLabel.Text = "Dimensiune: 9 \nDificultate: Medium \nNumar maxim de pasi: 50 \nTimp maxim: 5 minute";
                form.settingsLabel.BackColor = Color.Goldenrod;
                form.mediuToolStripMenuItem.Checked = true;
            }
            else if (form.game_difficulty_setting == 3)
            {
                form.settingsLabel.Text = "Dimensiune: 11 \nDificultate: Hard \nNumar maxim de pasi: 25 \nTimp maxim: 3 minute";
                form.settingsLabel.BackColor = Color.Tomato;
                form.greuToolStripMenuItem.Checked = true;
            }
            else
            {
                form.settingsLabel.Text = "Dimensiune: " + form.game_dimension_setting + "\nDificultate: Personalizat \nNumar maxim de pasi: " + form.maximum_number_of_steps_setting + " \nTimp maxim: " + form.seconds/60 + "minute";
                form.settingsLabel.BackColor = Color.DodgerBlue;
                form.personalizatToolStripMenuItem.Checked = true;
            }

            if (form.game_dimension_setting == 7) form.toolStripMenuItem2.Checked = true; // 7
            else if (form.game_dimension_setting == 9) form.toolStripMenuItem3.Checked = true; // 9
            else form.toolStripMenuItem4.Checked = true; // 11

            if (form.maximum_number_of_steps_setting == 100) form.toolStripMenuItem5.Checked = true; // 100
            else if (form.maximum_number_of_steps_setting == 50) form.toolStripMenuItem6.Checked = true; //50
            else form.toolStripMenuItem7.Checked = true; //25

            if (form.seconds == 600) form.toolStripMenuItem8.Checked = true; // 10
            else if (form.seconds == 300) form.toolStripMenuItem9.Checked = true; //5
            else form.toolStripMenuItem10.Checked = true; //3
        }
        //Update Leaderboard
        public void UpdateLeaderboard()
        {
            try
            {
                List<string> lines = File.ReadAllLines(form.leaderboardPath)
                            .Select(line => line.Split(';'))
                            .OrderByDescending(tokens => tokens[1])
                            .ThenBy(tokens => int.Parse(tokens[3]))
                            .ThenByDescending(tokens => int.Parse(tokens[2]))
                            .ThenBy(tokens => tokens[0])
                            .Select(tokens => string.Join(';'.ToString(), tokens))
                            .ToList();
                File.WriteAllLines(form.leaderboardPath, lines);
                int numLines = lines.Count;

                string dateString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                DateTime now = DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                form.leaderboardPlaces.Items.Clear();
                for (int i = 0; i <= 9; i++)
                {
                    if (i < numLines)
                    {
                        string[] separated_content = lines[i].Split(';');
                        DateTime data = DateTime.ParseExact(separated_content[4], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        TimeSpan diff = now - data;
                        if (diff.TotalSeconds < 60) separated_content[4] = ((int)diff.TotalSeconds).ToString() + " secunde in urma";
                        else if (diff.TotalMinutes < 60) separated_content[4] = ((int)diff.TotalMinutes).ToString() + " minute in urma ago";
                        else if (diff.TotalHours < 24) separated_content[4] = ((int)diff.TotalHours).ToString() + " ore in urma";
                        else if (diff.TotalDays < 7) separated_content[4] = ((int)diff.TotalDays).ToString() + " zile in urma";
                        else if (diff.TotalDays < 365.25 / 12) separated_content[4] = ((int)((int)diff.TotalDays / 7)).ToString() + " saptamani in urma";
                        else if (diff.TotalDays < 365.25) separated_content[4] = ((int)((int)diff.TotalDays / (365.25 / 12))).ToString() + " luni in urma";
                        else separated_content[4] = ((int)((int)diff.TotalDays / 365.25)).ToString() + " ani in urma";

                        string player_informations = i + 1 + ". " + separated_content[0] + " / " + separated_content[1] + " / " + separated_content[2] + " / " + separated_content[3] + " / " + separated_content[4];
                        form.leaderboardPlaces.Items.Add(player_informations);
                    }
                    else
                    {
                        string player_informations = i + 1 + ". NO_NAME / NO_SCORE / NO_DIMENSION / NO_TIME / NO_DATE";
                        form.leaderboardPlaces.Items.Add(player_informations);
                    }
                }
            }
            catch (FormatException)
            {
                using (var stream = new FileStream(form.leaderboardPath, FileMode.Truncate))
                {

                }
                for (int i = 0; i <= 9; i++)
                {
                    string player_informations = i + 1 + ". NO_NAME / NO_SCORE / NO_DIMENSION / NO_TIME / NO_DATE";
                    form.leaderboardPlaces.Items.Add(player_informations);
                }
            }
        }
    }
}
