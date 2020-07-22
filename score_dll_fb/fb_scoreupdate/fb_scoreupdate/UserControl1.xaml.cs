using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;
using System.Windows.Markup;
using System.Xml;
using System.Threading;
using System.ComponentModel;

namespace fb_scoreupdate
{
    public partial class UserControl1 : UserControl
    {
        private bool posession = false;
        private bool announcementOn = false;
        private string team1Score = "0";
        private string team2Score = "0";
        private string timer = "";
        private string quarter = "";
        private string announcementType = "";
        private string team1Col = "";
        private string team2Col = "";
        private string team1IconPath = "";
        private string team2IconPath = "";
        private string team1Name = "";
        private string team2Name = "";
        private string down = "";
        private string downandnum = "";
        private string team1Record;
        private string team2Record;
        private string team1Timeouts;
        private string team2Timeouts;

        private DispatcherTimer upd;
        private static DateTime lastUpdate;
        private string tempScoreDataPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/scoreman/fb_score.txt";

        public UserControl1()
        {
            InitializeComponent();
            MonitorFile(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/scoreman/");
            ReadScoreData(tempScoreDataPath);
            UpdateScoreData();

            upd = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 250) };
            upd.Tick += Update;
            upd.Start();

            AnnouncementBox.Visibility = Visibility.Hidden;
        }
        void Update(object sender, EventArgs e) { UpdateScoreData(); }
        private void MonitorFile(string path)
        {
            FileSystemWatcher watcher = new FileSystemWatcher { Path = path };
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            FileInfo f = new FileInfo(tempScoreDataPath);
            if (f.Exists)
            {
                if (f.LastWriteTimeUtc > lastUpdate)
                {
                    lastUpdate = f.LastWriteTimeUtc;
                    ReadScoreData(tempScoreDataPath);
                }
            }
        }
        public bool IsFileUsedByOtherProcess(string path)
        {
            bool Locked = false;
            try
            {
                FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                fs.Close();
            }
            catch
            {
                Locked = true;
            }
            return Locked;
        }
        void ReadScoreData (string path)
        {
            if (IsFileUsedByOtherProcess(tempScoreDataPath))
                return;
            else
            {
                //read the txt
                using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    string[] lines = System.IO.File.ReadAllLines(path);
                    team1Score = lines[0];
                    team2Score = lines[1];
                    timer = lines[2];
                    Boolean.TryParse(lines[3], out posession);
                    quarter = lines[4];
                    Boolean.TryParse(lines[5], out announcementOn);
                    announcementType = lines[6];
                    team1Col = lines[7];
                    team2Col = lines[8];
                    team1IconPath = lines[9];
                    team2IconPath = lines[10];
                    team1Name = lines[11];
                    team2Name = lines[12];
                    down = lines[13];
                    downandnum = lines[14];
                    team1Timeouts = lines[15];
                    team2Timeouts = lines[16];
                    team1Record = lines[17] + " - " + lines[18];
                    team2Record = lines[19] + " - " + lines[20];
                }
            }
        }
        void UpdateScoreData()
        {
            if (IsFileUsedByOtherProcess(tempScoreDataPath))
                return;
            else
            {
                int[] t1Timeouts = { Int32.Parse(team1Timeouts.Remove(1, 2)), Int32.Parse(team1Timeouts.Remove(0, 1).Remove(1, 1)), Int32.Parse(team1Timeouts.Remove(0, 2)) };
                int[] t2Timeouts = { Int32.Parse(team2Timeouts.Remove(1, 2)), Int32.Parse(team2Timeouts.Remove(0, 1).Remove(1, 1)), Int32.Parse(team2Timeouts.Remove(0, 2)) };
                SolidColorBrush t1col = new SolidColorBrush((Color)ColorConverter.ConvertFromString(team1Col));//convert from #00000 to an actual color
                SolidColorBrush t2col = new SolidColorBrush((Color)ColorConverter.ConvertFromString(team2Col));//convert from #00000 to an actual color
                Rectangle[] team1Rects = { Team1Timeout1, Team1Timeout2, Team1Timeout3 };
                Rectangle[] team2Rects = { Team2Timeout1, Team2Timeout2, Team2Timeout3 };

                Team1_Rect.Fill = t1col;
                Team1_Rect_TitleBar.Fill = t1col;
                Team2_Rect.Fill = t2col;
                Team2_Rect_TitleBar.Fill = t2col;

                Team1_Score.Content = team1Score;
                Team2_Score.Content = team2Score;
                Team1_Name.Content = team1Name.ToUpper();
                Team2_Name.Content = team2Name.ToUpper();
                Team1_Record.Content = team1Record;
                Team2_Record.Content = team2Record;
                Timer.Content = timer;

                AnnouncementBox.Content = announcementType.Remove(0, "System.Windows.Controls.ListBoxItem: ".Length).ToUpper();
                Quarter.Content = quarter.Remove(0, "System.Windows.Controls.ListBoxItem: ".Length);
                Stats.Content = down + " & " + downandnum;

                for (int i = 0; i < t1Timeouts.Length; i++)
                {
                    team1Rects[i].Visibility = t1Timeouts[i] == 1 ? Visibility.Visible : Visibility.Hidden;
                }
                for (int i = 0; i < t2Timeouts.Length; i++)
                {
                    team2Rects[i].Visibility = t2Timeouts[i] == 1 ? Visibility.Visible : Visibility.Hidden;
                }

                Team1_Pos.Visibility = posession ? Visibility.Hidden : Visibility.Visible;
                Team2_Pos.Visibility = posession ? Visibility.Visible : Visibility.Hidden;
                AnnouncementBox.Visibility = announcementOn ? Visibility.Visible : Visibility.Hidden;

                if (File.Exists(team1IconPath))
                {
                    Team1_Icon.Source = new BitmapImage(new Uri(team1IconPath, UriKind.RelativeOrAbsolute));
                }
                if (File.Exists(team2IconPath))
                {
                    Team2_Icon.Source = new BitmapImage(new Uri(team2IconPath, UriKind.RelativeOrAbsolute));
                }
            }
        }
    }
}