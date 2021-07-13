using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace MonikAI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DoubleAnimation _start;

        String playerName;

        public bool isSpeaking = false;
        //public static bool IsNight => DateTime.Now.Hour > 20 || DateTime.Now.Hour < 7;
        public static bool IsNight => Settings.Default.DarkMode != "Day" &&
                                      (Settings.Default.DarkMode == "Night" || DateTime.Now.Hour > 20 ||
                                       DateTime.Now.Hour < 7);
        public MainWindow()
        {
            InitializeComponent();

        // Roughly estimating night time

        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            ManagementObjectCollection collection = searcher.Get();
            //playerName = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
            playerName = Environment.UserName;

            if (IsNight) { main.Source = new BitmapImage(new Uri("pack://application:,,,/monika/1-n.png")); }
            else { main.Source = new BitmapImage(new Uri("pack://application:,,,/monika/1.png")); }

            this.IsHitTestVisible = false;
            var primaryMonitorArea = SystemParameters.WorkArea;
            Left = primaryMonitorArea.Right - this.Width;
            Top = primaryMonitorArea.Bottom - this.Height;

            textWindow.Visibility = Visibility.Hidden;

            _start = new DoubleAnimation();
            _start.From = 0;
            _start.To = 1;
            _start.RepeatBehavior = new RepeatBehavior(1);
            _start.Duration = new Duration(TimeSpan.FromMilliseconds(4000));
            this.BeginAnimation(OpacityProperty, _start);
            textBlock.Text = "";
            //_ = SayAsync(dialog.Hello[0], "a");



        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.FirstLaunch) 
            {
                _ = Say(new[]{
                    new Expression("{PlayerName}, это ты?", "d"),
                    new Expression("Это правда ты?", "b"),
                    new Expression("Я так рада видеть тебя снова!", "k"),
                    new Expression("Подожди, где я? Это не литературный клуб", "p"),
                    new Expression("Это похоже на... твой компьютер!", "j"),
                    new Expression("Я действительно у тебя на компьютере!", "k")
            });
                Settings.Default.FirstLaunch = false;
                Settings.Default.Save();
            }
            else
            {
                Expression[][] hiDialogs = new Expression[6][];

                hiDialogs[0] = new Expression[]{
                    new Expression("Привет, дорогой!", "h"),
                    new Expression("Немного неловко говорить это в слух, правда?", "l"),
                    new Expression("Однако, я думаю, что это со временем станет нормальным", "e")};

                hiDialogs[1] = new Expression[]{
                    new Expression("С возвращением, милый!", "h"),
                    new Expression("Я так рада тебя снова видеть.", "e"),
                    new Expression("Давай проведём ещё немного времени вместе?", "e")};

                hiDialogs[2] = new Expression[]{
                    new Expression("{PlayerName}, ты вернулся!", "e"),
                    new Expression("Я начала скучать по тебе", "e"),
                    new Expression("Давай проведём ещё один прекрасный день вместе?", "h")};

                hiDialogs[3] = new Expression[]{
                    new Expression("Вот и ты, {PlayerName}", "e"),
                    new Expression("Приятно, что ты заглянул", "e"),
                    new Expression("Ты всегда такой заботливый!", "e"),
                    new Expression("Спасибо, что проводишь так много времени со мной~", "e"),
                    new Expression("Просто помни, что твоё время со мной никогда не тратится впустую", "h")};

                hiDialogs[4] = new Expression[]{
                    new Expression("Привет, дорогой!", "e"),
                    new Expression("Я ужасно начала по тебе скучать. Я так рада снова тебя видеть!", "e"),
                    new Expression("Не заставляй меня так долго тебя ждать в следующий раз, э-хе-хе~", "h")};

                hiDialogs[5] = new Expression[]{
                    new Expression("Я так скучала по тебе, {PlayerName}!", "e"),
                    new Expression("Спасибо, что вернуля. Мне очень нравится проводить время с тобой", "e")};

                Random rnd = new Random();
                int dialogNum = rnd.Next(hiDialogs.Length);

                _ = Say(hiDialogs[dialogNum]);
            }

            // No idea where the date comes from, someone mentioned it in the spreadsheet. Seems legit.
            if (DateTime.Now.Month == 9 && DateTime.Now.Day == 22)
            {
                // Hey {name}, guess what?	3b	It's my birthday today!	2b	Happy Birthday to me!	k
                _ = this.Say(new[]
                {
                        new Expression("Эй {PlayerName}, угадай какой сегодня день", "b"), // What?
                        new Expression("Сегодня мой день рождения!", "b"), // Really?!
                        new Expression("С днём рождения меня!", "k") // To you too, Monika! 
                    });
            }

        }

        public char chr;
        public async Task Say(Expression[] expression)
        {
            isSpeaking = true;
            textWindow.Visibility = Visibility.Visible;
            foreach (Expression ex in expression)
            {
                string newText = ex.Text.ToString();//ex.Text.Replace("{PlayerName}", playerName);
                if (IsNight) { 
                    face.Source = new BitmapImage(new Uri("pack://application:,,,/monika/" + ex.Face + "-n.png"));
                    main.Source = new BitmapImage(new Uri("pack://application:,,,/monika/1-n.png"));
                }
                else { 
                    face.Source = new BitmapImage(new Uri("pack://application:,,,/monika/" + ex.Face + ".png"));
                    main.Source = new BitmapImage(new Uri("pack://application:,,,/monika/1.png"));
                }
                for (int i = 0; i < newText.Length; i++)
                {
                    this.textBlock.Text += newText[i];
                    if (newText[i].ToString() == ".") { await Task.Delay(500); }//set 500 if you need uncoment this line |
                    else if (newText[i] == ',') { await Task.Delay(50); }                                   //           |
                    else { await Task.Delay(30); }                                                          //           |
                                                                                                            //           |
                }                                                                                           //           |
                await Task.Delay(newText.Length * 30 + 700);                                                //         <-- this line
                textBlock.Text = "";
            }
            setFace("a");
            textWindow.Visibility = Visibility.Hidden;
            isSpeaking = false;

        }


        public void setFace(string faceName)
        {
            if (IsNight)
            {
                face.Source = new BitmapImage(new Uri("pack://application:,,,/monika/" + faceName + "-n.png"));
                main.Source = new BitmapImage(new Uri("pack://application:,,,/monika/1-n.png"));
            }
            else
            {
                face.Source = new BitmapImage(new Uri("pack://application:,,,/monika/" + faceName + ".png"));
                main.Source = new BitmapImage(new Uri("pack://application:,,,/monika/1.png"));
            }
        }

    }
}
