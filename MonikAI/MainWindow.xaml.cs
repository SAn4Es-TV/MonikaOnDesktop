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
using System.Reflection;

namespace MonikaOnDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DoubleAnimation _start;

        String playerName;

        public int delay = 0;

        string greetingsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/greetings.txt"; // Greetings
        string idleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/idle.txt";           // Idle
        string progsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/progs.txt";         // Programs
        string sitesDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/sites.txt";         // Sites
        string googleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/google.txt";       // Google search
        string goodbyeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/goodbye.txt";     // Goodbye

        public static bool IsNight => MonikaSettings.Default.DarkMode != "Day" &&
                                      (MonikaSettings.Default.DarkMode == "Night" || DateTime.Now.Hour > 20 ||
                                       DateTime.Now.Hour < 7);
        private bool applicationRunning = true;
        public bool isSpeaking;
        public MainWindow()
        {
            InitializeComponent();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            ManagementObjectCollection collection = searcher.Get();
            //playerName = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
            playerName = Environment.UserName;

            this.setFace("a");

            this.IsHitTestVisible = false;
            var primaryMonitorArea = SystemParameters.WorkArea;
            Left = primaryMonitorArea.Right - this.Width;
            Top = primaryMonitorArea.Bottom - this.Height;

            textWindow.Visibility = Visibility.Hidden;

            textBlock.Text = "";

        }
        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {

        }
        private void MenuQuit_Click(object sender, RoutedEventArgs e)
        {/*
            readByeTxt();
            Task.Delay(delay).Wait();
            isSpeaking = false;
            if (!isSpeaking)
            {*/
            MonikaOnDesktop.MonikaSettings.Default.isColdShutdown = false;
            System.Windows.Application.Current.Shutdown();
            //}
        }
        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _start = new DoubleAnimation();
            _start.From = 0;
            _start.To = 1;
            _start.RepeatBehavior = new RepeatBehavior(1);
            _start.Duration = new Duration(TimeSpan.FromMilliseconds(4000));
            _start.Completed += (sender, args) =>
            {
                if (MonikaSettings.Default.FirstLaunch)
                {
                    _ = Say(new[]{
                    new Expression("[player], это ты?", "d"),
                    new Expression("Это правда ты?", "b"),
                    new Expression("Я так рада видеть тебя снова!", "k"),
                    new Expression("Подожди, где я? Это не литературный клуб", "p"),
                    new Expression("Это похоже на... твой компьютер!", "j"),
                    new Expression("Я действительно у тебя на компьютере!", "k")
            });
                    MonikaSettings.Default.FirstLaunch = false;
                    MonikaSettings.Default.Save();
                }
                else
                {
                    readGreetingsTxt();
                }

                // No idea where the date comes from, someone mentioned it in the spreadsheet. Seems legit.
                if (DateTime.Now.Month == 9 && DateTime.Now.Day == 22)
                {
                    // Hey {name}, guess what?	3b	It's my birthday today!	2b	Happy Birthday to me!	k
                    _ = this.Say(new[]
                    {
                        new Expression("Эй [player], угадай какой сегодня день", "b"), // What?
                        new Expression("Сегодня мой день рождения!", "b"), // Really?!
                        new Expression("С днём рождения меня!", "k") // To you too, Monika! 
                    });
                }

                // Blinking and Behaviour logic
                var eyesOpen = "a";
                var eyesClosed = "j";
                var random = new Random();
                this.Dispatcher.Invoke(() =>
                {
                    Task.Run(() =>
                    {
                        var nextBlink = DateTime.Now + TimeSpan.FromSeconds(random.Next(7, 50));
                        while (this.applicationRunning)
                        {

                            if (DateTime.Now >= nextBlink)
                            {
                                // Check if currently speaking, only blink if not in dialog
                                if (!isSpeaking)
                                {
                                    this.setFace(eyesClosed);
                                    Debug.WriteLine("eyesClosed");
                                    Task.Delay(200).Wait();
                                    this.setFace(eyesOpen);
                                    Debug.WriteLine("eyesOpen");
                                }

                                nextBlink = DateTime.Now + TimeSpan.FromSeconds(random.Next(7, 50));
                            }

                            Task.Delay(250).Wait();
                        }
                    });
                });

                try
                {
                    var randomDialog = new Random();
                    this.Dispatcher.Invoke(() =>
                    {
                        Task.Run(() =>
                        {
                            var nextGialog = DateTime.Now + TimeSpan.FromSeconds(randomDialog.Next(120, 300));
                            while (this.applicationRunning)
                            {

                                if (DateTime.Now >= nextGialog)
                                {
                                // Check if currently speaking, only blink if not in dialog
                                if (!isSpeaking)
                                    {
                                        readIdleTxt();
                                    }

                                    nextGialog = DateTime.Now + TimeSpan.FromSeconds(randomDialog.Next(120, 300));
                                }

                                Task.Delay(250).Wait();
                            }
                        });
                    });
                }

                catch (InvalidOperationException exc)
                {
                    MessageBox.Show(exc.ToString());
                }

                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            };
            this.BeginAnimation(OpacityProperty, _start);
            /*
            Thread mainThread = new Thread(mainLoop);
            mainThread.Start();*/
        }
        public async Task Say(Expression[] expression)
        {
            isSpeaking = true;

            _ = this.Dispatcher.Invoke(async () =>
              {
                  textWindow.Visibility = Visibility.Visible;
                  foreach (Expression ex in expression)
                  {
                      string newText = ex.Text.Replace("[player]", playerName);
                      if (IsNight)
                      {
                          face.Source = new BitmapImage(new Uri("pack://application:,,,/monika/" + ex.Face + "-n.png"));
                          main.Source = new BitmapImage(new Uri("pack://application:,,,/monika/1-n.png"));
                      }
                      else
                      {
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
                    delay = newText.Length * 30 + 700;
                      textBlock.Text = "";
                  }
                  setFace("a");
                  textWindow.Visibility = Visibility.Hidden;
                  isSpeaking = false;
              });

        }
        public void setFace(string faceName)
        {
            if (IsNight)
            {
                this.Dispatcher.Invoke(() =>
                {
                    face.Source = new BitmapImage(new Uri("pack://application:,,,/monika/" + faceName + "-n.png"));
                    main.Source = new BitmapImage(new Uri("pack://application:,,,/monika/1-n.png"));
                });
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    face.Source = new BitmapImage(new Uri("pack://application:,,,/monika/" + faceName + ".png"));
                    main.Source = new BitmapImage(new Uri("pack://application:,,,/monika/1.png"));
                });
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            this.applicationRunning = false;
        }
        public void readGreetingsTxt()
        {
            string mainFile = File.ReadAllText(greetingsDialogPath);
            string[] dialogs = mainFile.Split(new string[] { "\r\n=\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Expression[][] hiDialogs = new Expression[dialogs.Length][];

            Debug.WriteLine(dialogs[0].Substring(2).ToString());
            for (int a = 0; a < dialogs.Length; a++)
            {
                string[] express = dialogs[a].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                Expression[] hiDialog = new Expression[express.Length];
                for (int b = 0; b < express.Length; b++)
                {
                    hiDialog[b] = new Expression(express[b].Substring(2), (express[b])[0].ToString());
                }
                hiDialogs[a] = hiDialog;
            }

            Random rnd = new Random();
            int dialogNum = rnd.Next(hiDialogs.Length);

            _ = Say(hiDialogs[dialogNum]);
        }
        public void readByeTxt()
        {
            string mainFile = File.ReadAllText(goodbyeDialogPath);
            string[] dialogs = mainFile.Split(new string[] { "\r\n=\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Expression[][] byeDialogs = new Expression[dialogs.Length][];

            Debug.WriteLine(dialogs[0].Substring(2).ToString());
            for (int a = 0; a < dialogs.Length; a++)
            {
                string[] express = dialogs[a].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                Expression[] byeDialog = new Expression[express.Length];
                for (int b = 0; b < express.Length; b++)
                {
                    byeDialog[b] = new Expression(express[b].Substring(2), (express[b])[0].ToString());
                }
                byeDialogs[a] = byeDialog;
            }

            Random rnd = new Random();
            int dialogNum = rnd.Next(byeDialogs.Length);

            _ = Say(byeDialogs[dialogNum]);
        }
        public void readIdleTxt()
        {
            string mainFile = File.ReadAllText(idleDialogPath);
            string[] dialogs = mainFile.Split(new string[] { "\r\n=\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Expression[][] idleDialogs = new Expression[dialogs.Length][];

            for (int a = 0; a < dialogs.Length; a++)
            {
                string[] express = dialogs[a].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                Expression[] idleDialog = new Expression[express.Length];
                for (int b = 0; b < express.Length; b++)
                {
                    idleDialog[b] = new Expression(express[b].Substring(2), (express[b])[0].ToString());
                }
                idleDialogs[a] = idleDialog;
            }

            Random rnd = new Random();
            int dialogNum = rnd.Next(idleDialogs.Length);

            _ = Say(idleDialogs[dialogNum]);
            Debug.WriteLine(dialogs[dialogNum].Substring(2).ToString());
        }

    }
}
