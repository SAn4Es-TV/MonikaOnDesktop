using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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

            MainWindow.LanguageChanged += LanguageChanged;
            CultureInfo currLang = MonikaSettings.Default.Language;

            //Заполняем меню смены языка:
            comboBox.Items.Clear();
            DataContext = this;

            //cbItems = new ObservableCollection<ComboBoxItem>();
            Debug.WriteLine("Текущий язык: " + currLang);
            foreach (var lang in MainWindow.m_Languages)
            {
                ComboBoxItem menuLang = new ComboBoxItem();
                menuLang.Content = lang.DisplayName;
                menuLang.Tag = lang;
                menuLang.Selected += ChangeLanguageClick;
                //Debug.WriteLine("Languages: " + lang);
                //comboBox.Items.Add(menuLang);
                comboBox.Items.Add(menuLang);
                if (lang.DisplayName == currLang.DisplayName)
                {
                    Debug.WriteLine(lang + "=" + currLang);
                    //comboBox.Text = lang.DisplayName;
                    comboBox.SelectedItem = menuLang;
                }
                else
                {
                    Debug.WriteLine(lang + "!=" + currLang);
                }

                Debug.WriteLine(menuLang.IsSelected.ToString());

            }
            /*
            for (int item = 0; item < comboBox.Items.Count; item++)
            {
                if(((ComboBoxItem)comboBox.Items[item]).Content.ToString() == currLang.DisplayName)
                {
                    Debug.WriteLine(((ComboBoxItem)comboBox.Items[0]).Content);
                    comboBox.SelectedItem = ((ComboBoxItem)comboBox.Items[0]);
                    comboBox.Text = ((ComboBoxItem)comboBox.Items[0]).Content.ToString();
                }
            }
            string tagToSelect = MonikaSettings.Default.Language.DisplayName; 
            var selectedItem = comboBox.Items
            .Cast<ComboBoxItem>()
            .Where(e => e.Content == tagToSelect)
            .FirstOrDefault();
            comboBox.SelectedItem = selectedItem;
*/
            t = label6;

            if (MonikaSettings.Default.UserName == "{PlayerName}") { userName.Text = ""; } else { userName.Text = MonikaSettings.Default.UserName; }
            monikaSize.Value = MonikaSettings.Default.Scaler;
            randomIdle.Value = MonikaSettings.Default.idleRandom;
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
            MonikaSettings.Default.Language = MainWindow.Lang;
            MonikaSettings.Default.UserName = userName.Text;
            MonikaSettings.Default.Scaler = (int)monikaSize.Value;
            MonikaSettings.Default.NightEnd = int.Parse(nightEndText.Text);
            MonikaSettings.Default.NightStart = int.Parse(nightStartText.Text);
            MonikaSettings.Default.idleRandom = (int)randomIdle.Value;
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
            RegistryKey currentUserKey = Registry.CurrentUser;
            RegistryKey monikaKey = currentUserKey.CreateSubKey("MonikaOnDesktop");
            monikaKey.SetValue("FirstLaunch", MonikaSettings.Default.FirstLaunch);
            monikaKey.SetValue("Language", MonikaSettings.Default.Language);
            monikaKey.SetValue("UserName", MonikaSettings.Default.UserName);
            monikaKey.SetValue("Scaler", MonikaSettings.Default.Scaler);
            monikaKey.SetValue("NightEnd", MonikaSettings.Default.NightEnd);
            monikaKey.SetValue("NightStart", MonikaSettings.Default.NightStart);
            monikaKey.SetValue("idleRandom", MonikaSettings.Default.idleRandom);
            monikaKey.SetValue("screenNum", MonikaSettings.Default.screenNum);
            monikaKey.SetValue("AutoStart", MonikaSettings.Default.AutoStart);
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
                        t.Text = (string)this.TryFindResource("s_labelDP1"); //"Очень редко(4-10 мин)";
                        break;
                    case 1:
                        t.Text = (string)this.TryFindResource("s_labelDP2"); //"Редко(3-8 мин)";
                        break;
                    case 2:
                        t.Text = (string)this.TryFindResource("s_labelDP3"); //"Нормально(2-5 мин)";
                        break;
                    case 3:
                        t.Text = (string)this.TryFindResource("s_labelDP4"); //"Часто(1-3 мин)";
                        break;
                    case 4:
                        t.Text = (string)this.TryFindResource("s_labelDP5"); //"Очень часто(30 сек - 2 мин)";
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
        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void LanguageChanged(Object sender, EventArgs e)
        {
            CultureInfo currLang = MainWindow.Lang;

            //Отмечаем нужный пункт смены языка как выбранный язык
            foreach (ComboBoxItem i in comboBox.Items)
            {
                CultureInfo ci = i.Tag as CultureInfo;
                //i.IsSelected = ci != null && ci.Equals(currLang);
                if (ci != null && ci.Equals(currLang))
                {
                    comboBox.SelectedItem = i;
                }
            }
        }

        private void ChangeLanguageClick(Object sender, EventArgs e)
        {
            ComboBoxItem mi = sender as ComboBoxItem;
            if (mi != null)
            {
                CultureInfo lang = mi.Tag as CultureInfo;
                if (lang != null)
                {
                    MainWindow.Lang = lang;
                }
            }

        }
    }
}
