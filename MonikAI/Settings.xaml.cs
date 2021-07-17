using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MonikaOnDesktop
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        TextBlock t;
        private readonly MainWindow mainWindow;
        public Settings(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            t = label6;/*
            switch (randomIdle.Value)
            {
                case 0:
                    label6.Text = "Очень редко(4-10 мин)";
                    break;
                case 1:
                    label6.Text = "Редко(3-8 мин)";
                    break;
                case 2:
                    label6.Text = "Нормально(2-5 мин)";
                    break;
                case 3:
                    label6.Text = "Часто(1-3 мин)";
                    break;
                case 4:
                    label6.Text = "Очень часто(30 сек - 2 мин)";
                    break;
            }*/

            if (MonikaSettings.Default.UserName == "{PlayerName}") { userName.Text = ""; } else { userName.Text = MonikaSettings.Default.UserName; }
             monikaSize.Value = MonikaSettings.Default.Scaler;
            randomIdle.Value = MonikaSettings.Default.idleRandomFrom;
            nightEndText.Text = MonikaSettings.Default.NightEnd.ToString();
            nightStartText.Text = MonikaSettings.Default.NightStart.ToString();
            checkBox1.IsChecked = MonikaSettings.Default.AutoStart;
            checkBox.IsChecked = MonikaSettings.Default.screenNum;
            if (System.Windows.Forms.Screen.AllScreens.Length == 1)
            {
                checkBox.Visibility = Visibility.Hidden;
                label7.Visibility = Visibility.Hidden;
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            MonikaSettings.Default.UserName = userName.Text;
            MonikaSettings.Default.Scaler = (int)monikaSize.Value;
            MonikaSettings.Default.NightEnd = int.Parse(nightEndText.Text);
            MonikaSettings.Default.NightStart = int.Parse(nightStartText.Text);
            MonikaSettings.Default.idleRandomFrom = (int)randomIdle.Value;
            MonikaSettings.Default.screenNum = (bool)checkBox.IsChecked;
            MonikaSettings.Default.AutoStart = (bool)checkBox1.IsChecked;
            switch (randomIdle.Value)
            {
                case 0:
                    MonikaSettings.Default.idleRandomFrom = 240;
                    MonikaSettings.Default.idleRandomTo = 600;
                    break;
                case 1:
                    MonikaSettings.Default.idleRandomFrom = 180;
                    MonikaSettings.Default.idleRandomTo = 480;
                    break;
                case 2:
                    MonikaSettings.Default.idleRandomFrom = 120;
                    MonikaSettings.Default.idleRandomTo = 300;
                    break;
                case 3:
                    MonikaSettings.Default.idleRandomFrom = 60;
                    MonikaSettings.Default.idleRandomTo = 180;
                    break;
                case 4:
                    MonikaSettings.Default.idleRandomFrom = 30;
                    MonikaSettings.Default.idleRandomTo = 120;
                    break;
            }
            MonikaSettings.Default.Save();
            this.DialogResult = true;
            this.Close();
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {            
            this.Close();
        }

        private void randomIdle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (label6 != null)
            {
                switch (randomIdle.Value)
                {
                    case 0:
                        t.Text = "Очень редко(4-10 мин)";
                        break;
                    case 1:
                        t.Text = "Редко(3-8 мин)";
                        break;
                    case 2:
                        t.Text = "Нормально(2-5 мин)";
                        break;
                    case 3:
                        t.Text = "Часто(1-3 мин)";
                        break;
                    case 4:
                        t.Text = "Очень часто(30 сек - 2 мин)";
                        break;
                }
            }
        }

        private void monikaSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.mainWindow.SetupScale((int)monikaSize.Value);
            //this.mainWindow.GoToSecondaryMonitor();
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            MonikaSettings.Default.screenNum = true;
            //MonikaSettings.Default.screenNum = (bool)checkBox.IsChecked;
            MonikaSettings.Default.Save();
            this.mainWindow.SetupScale((int)monikaSize.Value);
            //this.mainWindow.GoToSecondaryMonitor();
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            MonikaSettings.Default.screenNum = false;
            //MonikaSettings.Default.screenNum = (bool)checkBox.IsChecked;
            MonikaSettings.Default.Save();
            this.mainWindow.SetupScale((int)monikaSize.Value);
            //this.mainWindow.GoToSecondaryMonitor();
        }
    }
}
