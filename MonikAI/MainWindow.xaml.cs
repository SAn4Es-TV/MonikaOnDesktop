﻿using System;
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
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Net;
using System.Windows.Automation;
using Lyre;
using System.Globalization;
using System.Web;
using System.Xml;
using MonikaOnDesktop.Models;

namespace MonikaOnDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public DoubleAnimation _start;
        public DoubleAnimation _quit;

        String playerName;

        public int delay1 = 0;

        public string ExePath = AppDomain.CurrentDomain.BaseDirectory + "MonikaOnDesktop.exe"; // EXE path

        string greetingsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/greetings.txt"; // Greetings
        string idleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/idle.txt";           // Idle
        string progsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/progs.txt";         // Programs
        string sitesDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/sites.txt";         // Sites
        string googleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/google.txt";       // Google search
        string youtubeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/youtube.txt";     // Youtube search
        string goodbyeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/goodbye.txt";     // Goodbye

        public static bool IsNight => MonikaSettings.Default.DarkMode != "Day" &&
                                      (MonikaSettings.Default.DarkMode == "Night" || DateTime.Now.Hour > (MonikaSettings.Default.NightStart - 1) ||
                                       DateTime.Now.Hour < (MonikaSettings.Default.NightEnd + 1));
        private bool applicationRunning = true;
        public bool isSpeaking = true;


        private Settings settingsWindow;

        //private static HttpListener _listener;
        public int lastDialog;
        public int lastLastDialog;

        string Language;

        //google.com/search?q=hi
        //youtube.com/results?search_query=hi
        private const string GOOGLE_REGEX = ".*\\.?google\\..{2,3}.*q\\=(.*?)($|&)";
        private const string YOUTUBE_REGEX = ".*\\.?youtube\\..{2,3}.*y\\=(.*?)($|&)";

        private EventWaitHandle wh = new AutoResetEvent(false);
        public MainWindow()
        {
            RegistryKey monikaKey = Registry.CurrentUser.OpenSubKey("MonikaOnDesktop");
            if (monikaKey != null)
            {
                CultureInfo lang = CultureInfo.GetCultureInfo(monikaKey.GetValue("Language").ToString());
                MonikaSettings.Default.isColdShutdown = bool.Parse(monikaKey.GetValue("isColdShutdown").ToString());
                MonikaSettings.Default.FirstLaunch = bool.Parse(monikaKey.GetValue("FirstLaunch").ToString());
                MonikaSettings.Default.Language = lang;
                MonikaSettings.Default.UserName = monikaKey.GetValue("UserName").ToString();
                MonikaSettings.Default.Scaler = (int)monikaKey.GetValue("Scaler");
                MonikaSettings.Default.NightEnd = (int)monikaKey.GetValue("NightEnd");
                MonikaSettings.Default.NightStart = (int)monikaKey.GetValue("NightStart");
                MonikaSettings.Default.idleRandom = (int)monikaKey.GetValue("idleRandom");
                MonikaSettings.Default.screenNum = bool.Parse(monikaKey.GetValue("screenNum").ToString());
                MonikaSettings.Default.AutoStart = bool.Parse(monikaKey.GetValue("AutoStart").ToString());
                monikaKey.Close();
            }
            else
            {
                RegistryKey currentUserKey = Registry.CurrentUser;
                RegistryKey monika = currentUserKey.CreateSubKey("MonikaOnDesktop");
                monika.SetValue("isColdShutdown", MonikaSettings.Default.isColdShutdown);
                monika.SetValue("Language", MonikaSettings.Default.Language);
                monika.SetValue("FirstLaunch", MonikaSettings.Default.FirstLaunch);
                monika.SetValue("UserName", MonikaSettings.Default.UserName);
                monika.SetValue("Scaler", MonikaSettings.Default.Scaler);
                monika.SetValue("NightEnd", MonikaSettings.Default.NightEnd);
                monika.SetValue("NightStart", MonikaSettings.Default.NightStart);
                monika.SetValue("idleRandom", MonikaSettings.Default.idleRandom);
                monika.SetValue("screenNum", MonikaSettings.Default.screenNum);
                monika.SetValue("AutoStart", MonikaSettings.Default.AutoStart);
                monika.Close();
            }

            InitializeComponent();


            m_Languages.Clear();
            m_Languages.Add(new CultureInfo("en-US")); //Нейтральная культура для этого проекта
            m_Languages.Add(new CultureInfo("ru-RU"));

            this.settingsWindow = new Settings(this);
            MonikaSettings.Default.Reload();

            LanguageChanged += App_LanguageChanged;
            Lang = MonikaSettings.Default.Language;

            Language = MonikaSettings.Default.Language.Parent.ToString();

            if (String.IsNullOrEmpty(MonikaSettings.Default.UserName) || MonikaSettings.Default.UserName == "{PlayerName}")
            {
                playerName = Environment.UserName;
            }
            else
            {
                playerName = MonikaSettings.Default.UserName;
            }

            //App.Language = MonikaSettings.Default.Language;
            Debug.WriteLine(Language);
            setLanguage(Language);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            ManagementObjectCollection collection = searcher.Get();
            //playerName = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];

            if (String.IsNullOrEmpty(MonikaSettings.Default.UserName))
            {
                playerName = Environment.UserName;
            }
            else
            {
                playerName = MonikaSettings.Default.UserName;
            }
            //playerName = "Denis Solicen";
            this.setFace("1esa");

            //this.IsHitTestVisible = false;
            //var primaryMonitorArea = Screen.PrimaryScreen.Bounds;
            //Left = primaryMonitorArea.Right - this.Width;
            //Top = primaryMonitorArea.Bottom - this.Height;

            textWindow.Visibility = Visibility.Hidden;

            textBlock.Text = "";
            SetupScale(MonikaSettings.Default.Scaler);
            SetAutorunValue(MonikaSettings.Default.AutoStart);

            try
            {
                ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
                startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
                startWatch.Start();

                ManagementEventWatcher stopWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
                stopWatch.EventArrived += new EventArrivedEventHandler(stopWatch_EventArrived);
                stopWatch.Start();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(this,
                    "An error occured: " + ex.Message + "\r\n\r\n(Try run this app as an administrator.)");
            }

        }

        private void stopWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            Debug.WriteLine("Process removed: " + e.NewEvent.Properties["ProcessName"].Value.ToString());
        }
        public string lastProcess;
        private void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (!isSpeaking)
            {
                //consoleWrite("Запущен процесс: " + e.NewEvent.Properties["ProcessName"].Value.ToString(), true);
                string currentProcess = e.NewEvent.Properties["ProcessName"].Value.ToString();
                if (currentProcess != lastProcess)
                {
                    //readProgsTxt(currentProcess);
                    readLongXml(currentProcess, progsDialogPath, 0);
                    lastProcess = currentProcess;
                }
            }
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (this.settingsWindow == null || !this.settingsWindow.IsVisible)
                {
                    this.settingsWindow = new Settings(this);
                    if (this.settingsWindow.ShowDialog() == false)
                    {
                        SetupScale(MonikaSettings.Default.Scaler);
                        Language = MonikaSettings.Default.Language.Parent.ToString();
                        Lang = MonikaSettings.Default.Language;
                        setLanguage(Language);
                        //GoToSecondaryMonitor();
                        setFace("1esa");
                    }
                    else
                    {

                        setFace("1esa");
                        SetAutorunValue(MonikaSettings.Default.AutoStart);
                        Language = MonikaSettings.Default.Language.Parent.ToString();
                        Lang = MonikaSettings.Default.Language;
                        setLanguage(Language);
                        consoleWrite("Settings saved!", true);
                        consoleWrite("AutoStart --> " + MonikaSettings.Default.AutoStart, false);
                        consoleWrite("DarkMode --> " + MonikaSettings.Default.DarkMode, false);
                        consoleWrite("idleRandom --> " + MonikaSettings.Default.idleRandom, false);
                        consoleWrite("NightEnd --> " + MonikaSettings.Default.NightEnd, false);
                        consoleWrite("NightStart --> " + MonikaSettings.Default.NightStart, false);
                        consoleWrite("UserName --> " + MonikaSettings.Default.UserName, false);
                        consoleWrite("Scaler --> " + MonikaSettings.Default.Scaler, false);
                        consoleWrite("screenNum --> " + MonikaSettings.Default.screenNum, false);
                        consoleWrite("Language --> " + MonikaSettings.Default.Language.Parent.ToString(), false);
                        if (String.IsNullOrEmpty(MonikaSettings.Default.UserName) || MonikaSettings.Default.UserName == "{PlayerName}")
                        {
                            playerName = Environment.UserName;
                        }
                        else
                        {
                            playerName = MonikaSettings.Default.UserName;
                        }
                        RegistryKey currentUserKey = Registry.CurrentUser;
                        RegistryKey monika = currentUserKey.CreateSubKey("MonikaOnDesktop");
                        monika.SetValue("isColdShutdown", MonikaSettings.Default.isColdShutdown);
                        monika.SetValue("Language", MonikaSettings.Default.Language);
                        monika.SetValue("FirstLaunch", MonikaSettings.Default.FirstLaunch);
                        monika.SetValue("UserName", MonikaSettings.Default.UserName);
                        monika.SetValue("Scaler", MonikaSettings.Default.Scaler);
                        monika.SetValue("NightEnd", MonikaSettings.Default.NightEnd);
                        monika.SetValue("NightStart", MonikaSettings.Default.NightStart);
                        monika.SetValue("idleRandom", MonikaSettings.Default.idleRandom);
                        monika.SetValue("screenNum", MonikaSettings.Default.screenNum);
                        monika.SetValue("AutoStart", MonikaSettings.Default.AutoStart);
                        monika.Close();
                    }
                }
            });

        }
        public void setLanguage(string lang)
        {
            Lang = MonikaSettings.Default.Language;
            switch (lang)
            {
                case "ru":
                    quitMenu.Header = "Выход";
                    settingsMenu.Header = "Настройки";

                    greetingsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/greetings.txt"; // Greetings
                    idleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/idle.txt";           // Idle
                    progsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/progs.txt";         // Programs
                    sitesDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/sites.txt";         // Sites
                    googleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/google.txt";       // Google search
                    youtubeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/youtube.txt";     // Youtube search
                    goodbyeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/goodbye.txt";     // Goodbye
                    break;
                case "en":
                    quitMenu.Header = "Quit";
                    settingsMenu.Header = "Settings";

                    greetingsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/greetings.txt"; // Greetings
                    idleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/idle.txt";           // Idle
                    progsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/progs.txt";         // Programs
                    sitesDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/sites.txt";         // Sites
                    googleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/google.txt";       // Google search
                    youtubeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/youtube.txt";     // Youtube search
                    goodbyeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/goodbye.txt";     // Goodbye
                    break;
                default:
                    quitMenu.Header = "Quit";
                    settingsMenu.Header = "Settings";

                    greetingsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/greetings.txt"; // Greetings
                    idleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/idle.txt";           // Idle
                    progsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/progs.txt";         // Programs
                    sitesDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/sites.txt";         // Sites
                    googleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/google.txt";       // Google search
                    youtubeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/youtube.txt";     // Youtube search
                    goodbyeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/goodbye.txt";     // Goodbye
                    break;

            }
        }
        private void MenuQuit_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(isSpeaking);
            if (!isSpeaking)
            {
                MonikaOnDesktop.MonikaSettings.Default.isColdShutdown = false;
                RegistryKey currentUserKey = Registry.CurrentUser;
                RegistryKey monika = currentUserKey.CreateSubKey("MonikaOnDesktop");
                monika.SetValue("isColdShutdown", MonikaSettings.Default.isColdShutdown);
                monika.SetValue("Language", MonikaSettings.Default.Language);
                monika.SetValue("FirstLaunch", MonikaSettings.Default.FirstLaunch);
                monika.SetValue("UserName", MonikaSettings.Default.UserName);
                monika.SetValue("Scaler", MonikaSettings.Default.Scaler);
                monika.SetValue("NightEnd", MonikaSettings.Default.NightEnd);
                monika.SetValue("NightStart", MonikaSettings.Default.NightStart);
                monika.SetValue("idleRandom", MonikaSettings.Default.idleRandom);
                monika.SetValue("screenNum", MonikaSettings.Default.screenNum);
                monika.SetValue("AutoStart", MonikaSettings.Default.AutoStart);
                monika.Close();
                //readByeTxt();
                readXml(goodbyeDialogPath, 1);
            }
        }
        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Lang = MonikaSettings.Default.Language;
            _start = new DoubleAnimation();
            _start.From = 0;
            _start.To = 1;
            _start.RepeatBehavior = new RepeatBehavior(1);
            _start.Duration = new Duration(TimeSpan.FromMilliseconds(4000));
            _start.Completed += async (sender, args) =>
            {
                //RegistryKey reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
                RegistryKey WN = Registry.CurrentUser.OpenSubKey("MonikaOnDesktop", true);

                if (MonikaSettings.Default.FirstLaunch && WN == null)
                {
                    isSpeaking = true;
                    Debug.WriteLine("First Launch");
                    switch (Language.Substring(0, 2))
                    {
                        case "ru":
                            await Say(new[]{
                    new Expression("[player], это ты?", "1eua"),
                    new Expression("Это правда ты?", "1eub"),
                    new Expression("Я так рада видеть тебя снова!", "1sub"),
                    new Expression("Подожди, где я? Это не литературный клуб", "1euc"),
                    new Expression("Это похоже на... твой компьютер!", "1rud"),
                    new Expression("Я действительно у тебя на компьютере!", "1wub")});
                            break;
                        case "en":
                            await Say(new[]{
                    new Expression("[player], is that you?", "1eua"),
                    new Expression("It's really you, huh?", "1eub"),
                    new Expression("I'm so happy to see you again!", "1sub"),
                    new Expression("Wait, where am I? This is not the literature club", "1euc"),
                    new Expression("This looks like... your desktop!", "1rud"),
                    new Expression("I'm right here with you!", "1wub")});
                            break;
                        default:
                            await Say(new[]{
                    new Expression("[player], is that you?", "1eua"),
                    new Expression("It's really you, huh?", "1eub"),
                    new Expression("I'm so happy to see you again!", "1sub"),
                    new Expression("Wait, where am I? This is not the literature club", "1euc"),
                    new Expression("This looks like... your desktop!", "1rud"),
                    new Expression("I'm right here with you!", "1wub")});
                            break;
                    }

                    MonikaSettings.Default.FirstLaunch = false;
                    MonikaSettings.Default.Save();
                    this.Dispatcher.Invoke(() =>
                    {
                        textWindow.Visibility = Visibility.Hidden;
                        setFace("1esa");
                    });
                    isSpeaking = false;
                }
                else if (MonikaSettings.Default.FirstLaunch && WN != null)
                {
                    isSpeaking = true;
                    Debug.WriteLine("Update");
                    switch (Language.Substring(0, 2))
                    {
                        case "ru":
                            await Say(new[]{
                    new Expression("[player], привет!", "1eub"),
                    new Expression("Я заметила изменения в коде", "1ruc"),
                    new Expression("...", "1ruc"),
                    new Expression("Ты установил обновление?", "1sub"),
                    new Expression("Давай посмотрим, что здесь нового", "2wub"),
                    new Expression("Тут написано, что мы можем общаться при помощи кнопок", "3wub"),
                    new Expression("Но я ещё не совсем разобралась с ними", "2dud"),
                    new Expression("Давай подождём немного, может я что-то найду", "2hub"),});
                            break;
                        case "en":
                            await Say(new[]{
                    new Expression("[player], hello!", "1eub"),
                    new Expression("I noticed changes in the code", "1ruc"),
                    new Expression("...", "1ruc"),
                    new Expression("Have you installed the update?", "1sub"),
                    new Expression("Let's see what's new then", "2wub"),
                    new Expression("It says here that we can communicate using buttons", "3wub"),
                    new Expression("But I haven't quite figured it out yet", "2dud"),
                    new Expression("Let's wait a bit, maybe I'll find something", "2hub")});
                            break;
                        default:
                            await Say(new[]{
                    new Expression("[player], hello!", "1eub"),
                    new Expression("I noticed changes in the code", "1ruc"),
                    new Expression("...", "1ruc"),
                    new Expression("Have you installed the update?", "1sub"),
                    new Expression("Let's see what's new then", "2wub"),
                    new Expression("It says here that we can communicate using buttons", "3wub"),
                    new Expression("But I haven't quite figured it out yet", "2dud"),
                    new Expression("Let's wait a bit, maybe I'll find something", "2hub")});
                            break;
                    }

                    this.Dispatcher.Invoke(() =>
                    {
                        textWindow.Visibility = Visibility.Hidden;
                        setFace("1esa");
                    });
                    isSpeaking = false;
                }
                else
                {
                    Debug.WriteLine("just launch");
                    //showText();
                    readXml(greetingsDialogPath, 0);
                    //readIdleXml();
                    //readGreetingsTxt();
                }
                // No idea where the date comes from, someone mentioned it in the spreadsheet. Seems legit.
                if (DateTime.Now.Month == 9 && DateTime.Now.Day == 22)
                {
                    isSpeaking = true;
                    switch (Language.Substring(0, 2))
                    {
                        case "ru":
                            // Hey {name}, guess what?	3b	It's my birthday today!	2b	Happy Birthday to me!	k
                            _ = this.Say(new[]
                    {
                        new Expression("Эй [player], угадай какой сегодня день", "1eub"), // What?
                        new Expression("Сегодня мой день рождения!", "1sub"), // Really?!
                        new Expression("С днём рождения меня!", "2hub") // To you too, Monika! 
                    });
                            break;
                        case "en":
                            // Hey {name}, guess what?	3b	It's my birthday today!	2b	Happy Birthday to me!	k
                            _ = this.Say(new[]
                    {
                        new Expression("Hey [player], guess what", "1eub"), // What?
                        new Expression("It's my birthday today!", "1sub"), // Really?!
                        new Expression("Happy Birthday to me!", "2hub") // To you too, Monika! 
                    });
                            break;
                        default:
                            // Hey {name}, guess what?	3b	It's my birthday today!	2b	Happy Birthday to me!	k
                            _ = this.Say(new[]
                    {
                        new Expression("Hey [player], guess what", "1eub"), // What?
                        new Expression("It's my birthday today!", "1sub"), // Really?!
                        new Expression("Happy Birthday to me!", "2hub") // To you too, Monika! 
                    });
                            break;
                    }
                    isSpeaking = false;
                }


                // Blinking and Behaviour logic
                var eyesOpen = "1esa";
                var eyesClosed = "1dsa";
                var random = new Random();
                this.Dispatcher.Invoke(() =>
                {
                    Task.Run(async () =>
                    {
                        HttpListener listener = new HttpListener();
                        // установка адресов прослушки
                        listener.Prefixes.Add("http://localhost:2005/");
                        listener.Start();
                        //Console.WriteLine("Ожидание подключений...");
                        var nextBlink = DateTime.Now + TimeSpan.FromSeconds(random.Next(7, 50));
                        while (this.applicationRunning)
                        {
                            // метод GetContext блокирует текущий поток, ожидая получение запроса 
                            HttpListenerContext context = listener.GetContext();
                            HttpListenerRequest request = context.Request;
                            string query = context.Request.QueryString["myurl"];
                            // получаем объект ответа
                            //Debug.WriteLine(query);
                            //readSitesTxt(formatURL(query));
                            Debug.WriteLine(formatURL(query));
                            readLongXml(formatURL(query), sitesDialogPath, 1);
                            readLongXml(formatURL(query), googleDialogPath, 2);
                            readLongXml(formatURL(query), youtubeDialogPath, 3);
                            //readSitesTxt(formatURL(query));
                            //readGoogleTxt(formatURL(query));
                            //readYoutubeTxt(formatURL(query));
                            if (DateTime.Now >= nextBlink)
                            {
                                // Check if currently speaking, only blink if not in dialog
                                if (!isSpeaking)
                                {
                                    consoleWrite("Моргнули", true);
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

                var randomDialog = new Random();
                this.Dispatcher.Invoke(() =>
                {
                    Task.Run(() =>
                    {
                        var nextGialog = DateTime.Now + TimeSpan.FromSeconds(randomDialog.Next(MonikaSettings.Default.idleRandomFrom, MonikaSettings.Default.idleRandomTo));
                        while (this.applicationRunning)
                        {

                            if (DateTime.Now >= nextGialog)
                            {
                                // Check if currently speaking, only blink if not in dialog
                                if (!isSpeaking)
                                {
                                    readXml(idleDialogPath, 0);
                                    //readIdleTxt();
                                }

                                nextGialog = DateTime.Now + TimeSpan.FromSeconds(randomDialog.Next(MonikaSettings.Default.idleRandomFrom, MonikaSettings.Default.idleRandomTo));
                            }

                            Task.Delay(250).Wait();
                        }
                    });
                });
            };
            this.BeginAnimation(OpacityProperty, _start);
        }
        public async Task Say(Expression[] expression)
        {
            //isSpeaking = true;
            /*

            if (String.IsNullOrEmpty(MonikaSettings.Default.UserName) || MonikaSettings.Default.UserName == "{PlayerName}")
            {
                playerName = Environment.UserName;
            }
            else
            {
                playerName = MonikaSettings.Default.UserName;
            }*/
            this.Dispatcher.Invoke(() =>
            {
                textWindow.Visibility = Visibility.Visible;
            });
            foreach (Expression ex in expression)
            {
                delay1 = 0;
                try
                {
                    string newText = ex.Text.Replace("[player]", playerName).Replace("{PlayerName}", playerName); //замена
                    consoleWrite(newText, true);
                    setFace(ex.Face);
                    for (int i = 0; i < newText.Length; i++)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            this.textBlock.Text += newText[i];
                        });
                        if (newText[i].ToString() == ".")
                        {
                            await Task.Delay(500);
                            delay1 += 500;
                        }

                        else
                        {
                            await Task.Delay(30);
                            delay1 += 30;
                        }

                    }
                    delay1 += 700;
                    await Task.Delay(delay1);
                    this.Dispatcher.Invoke(() =>
                    {
                        //await Task.Delay(delay1);
                        textBlock.Text = "";
                    });

                }

                catch (Exception e)
                {
                }
            }
            this.Dispatcher.Invoke(() =>
            {
                //textWindow.Visibility = Visibility.Hidden;
            });

        }
        Expression[][] exe;
        public void Menu(string question, string[] q, Expression[][] a)
        {
            this.Dispatcher.Invoke(() =>
            {
                textWindow.Visibility = Visibility.Visible;
                textBlock.Text = "";
                textBlock.Text = question;
                ButtonsGrid.RowDefinitions.Clear();
                this.ButtonsGrid.Children.Clear();
                for (int i = 0; i < q.Length; i++)
                {
                    RowDefinition row = new RowDefinition();
                    ButtonsGrid.RowDefinitions.Add(row);
                    var text = new OutlinedTextBlock
                    {
                        Text = q[i],
                        FontFamily = new FontFamily("Comic Sans MS"),
                        TextWrapping = TextWrapping.Wrap,
                        StrokeThickness = 1.5,
                        Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                        Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                    };
                    switch (MonikaSettings.Default.Scaler)
                    {
                        case 0:
                            text.FontSize = 5;
                            break;
                        case 1:
                            text.FontSize = 10;
                            break;
                        case 2:
                            text.FontSize = 15;
                            break;
                        case 3:
                            text.FontSize = 20;
                            break;
                    }
                    var button = new Button
                    {
                        Name = "butt" + i,
                        Content = text,
                        Width = 400,
                        Height = 30,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    switch (MonikaSettings.Default.Scaler)
                    {
                        case 0:
                            button.Width = 100;
                            button.Height = 10;
                            break;
                        case 1:
                            button.Width = 200;
                            button.Height = 20;
                            break;
                        case 2:
                            button.Width = 300;
                            button.Height = 30;
                            break;
                        case 3:
                            button.Width = 400;
                            button.Height = 40;
                            break;
                    }
                    button.Click += Button_ClickAsync;

                    Grid.SetRow(button, i);
                    this.ButtonsGrid.Children.Add(button);
                }
                exe = a;
            });

        }
        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            Button butt = (sender as Button);
            int num = int.Parse(butt.Name.Substring(4));
            Debug.WriteLine("Clicked " + num);
            this.Dispatcher.Invoke(() => {
                textBlock.Text = "";
                ButtonsGrid.RowDefinitions.Clear();
                this.ButtonsGrid.Children.Clear();
            });
            foreach (Expression expression in exe[num])
            {
                await Say(new[] { expression });
                //Thread.Sleep(delay); // sleep
            }
            sayIdle();
        }
        public void setFace(string faceName)
        {
            int body = int.Parse(faceName[0].ToString());
            string eye = faceName[1].ToString();
            string eyebrow = faceName[2].ToString();
            string mouth = faceName[3].ToString();
            //Debug.Write("Body: " + body + ", Eyes: " + eye + ", Eyesbrow: " + eyebrow + ", Mouth: " + mouth + "\n");
            if (IsNight)
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (body == 5)
                    {
                        this.main.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/body/body-" + body + "-n.png"));
                        this.hairBack.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/hair/hair-leaning-def-def-back-n.png"));
                        this.hairFront.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/hair/hair-leaning-def-def-front-n.png"));
                        this.ribbon.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/ribbon/acs-ribbon_def-5-n.png"));
                        this.arms.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/body/" + body + "-n.png"));
                        this.eyes.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/eyes/5" + eye + "-n.png"));
                        this.brow.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/brows/5" + eyebrow + "-n.png"));
                        this.mouth.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/mouth/5" + mouth + "-n.png"));
                        this.nose.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/face-leaning-def-nose-def-n.png"));
                    }
                    else if (body == 1)
                    {
                        this.main.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/body/body-" + body + "-n.png"));
                        this.hairBack.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/hair/hair-def-back-n.png"));
                        this.hairFront.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/hair/hair-def-front-n.png"));
                        this.ribbon.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/ribbon/acs-ribbon_def-0-n.png"));
                        this.arms.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/body/" + body + "-n.png"));
                        this.eyes.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/eyes/" + eye + "-n.png"));
                        this.brow.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/brows/" + eyebrow + "-n.png"));
                        this.mouth.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/mouth/" + mouth + "-n.png"));
                        this.nose.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/face-nose-def-n.png"));
                    }
                    else
                    {
                        this.main.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/body/body-1-n.png"));
                        this.hairBack.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/hair/hair-def-back-n.png"));
                        this.hairFront.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/hair/hair-def-front-n.png"));
                        this.ribbon.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/ribbon/acs-ribbon_def-0-n.png"));
                        this.arms.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/body/" + body + "-n.png"));
                        this.eyes.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/eyes/" + eye + "-n.png"));
                        this.brow.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/brows/" + eyebrow + "-n.png"));
                        this.mouth.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/mouth/" + mouth + "-n.png"));
                        this.nose.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/face-nose-def-n.png"));
                    }
                });
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (body == 5)
                    {
                        this.main.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/body/body-" + body + ".png"));
                        this.hairBack.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/hair/hair-leaning-def-def-back.png"));
                        this.hairFront.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/hair/hair-leaning-def-def-front.png"));
                        this.ribbon.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/ribbon/acs-ribbon_def-5.png"));
                        this.arms.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/body/" + body + ".png"));
                        this.eyes.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/eyes/5" + eye + ".png"));
                        this.brow.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/brows/5" + eyebrow + ".png"));
                        this.mouth.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/mouth/5" + mouth + ".png"));
                        this.nose.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/face-leaning-def-nose-def.png"));

                    }
                    else if (body == 1)
                    {
                        this.main.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/body/body-" + body + ".png"));
                        this.hairBack.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/hair/hair-def-back.png"));
                        this.hairFront.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/hair/hair-def-front.png"));
                        this.ribbon.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/ribbon/acs-ribbon_def-0.png"));
                        this.arms.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/body/" + body + ".png"));
                        this.eyes.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/eyes/" + eye + ".png"));
                        this.brow.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/brows/" + eyebrow + ".png"));
                        this.mouth.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/mouth/" + mouth + ".png"));
                        this.nose.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/face-nose-def.png"));
                    }
                    else
                    {
                        this.main.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/body/body-1.png"));
                        this.hairBack.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/hair/hair-def-back.png"));
                        this.hairFront.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/hair/hair-def-front.png"));
                        this.ribbon.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/ribbon/acs-ribbon_def-0.png"));
                        this.arms.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/body/" + body + ".png"));
                        this.eyes.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/eyes/" + eye + ".png"));
                        this.brow.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/brows/" + eyebrow + ".png"));
                        this.mouth.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/mouth/" + mouth + ".png"));
                        this.nose.Source = new BitmapImage(new Uri("pack://application:,,,/assets/monika/face/face-nose-def.png"));
                    }

                });
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            MonikaSettings.Default.isColdShutdown = true;
            RegistryKey currentUserKey = Registry.CurrentUser;
            RegistryKey monika = currentUserKey.CreateSubKey("MonikaOnDesktop");
            monika.SetValue("isColdShutdown", MonikaSettings.Default.isColdShutdown);
            monika.SetValue("Language", MonikaSettings.Default.Language);
            monika.SetValue("FirstLaunch", MonikaSettings.Default.FirstLaunch);
            monika.SetValue("UserName", MonikaSettings.Default.UserName);
            monika.SetValue("Scaler", MonikaSettings.Default.Scaler);
            monika.SetValue("NightEnd", MonikaSettings.Default.NightEnd);
            monika.SetValue("NightStart", MonikaSettings.Default.NightStart);
            monika.SetValue("idleRandom", MonikaSettings.Default.idleRandom);
            monika.SetValue("screenNum", MonikaSettings.Default.screenNum);
            monika.SetValue("AutoStart", MonikaSettings.Default.AutoStart);
            monika.Close();
            this.applicationRunning = false;
        }
        #region
        /*
        public void readGreetingsTxt()
        {
            string mf = File.ReadAllText(greetingsDialogPath);
            string mainFile = mf.Replace("\r", String.Empty);
            string[] dialogs = mainFile.Split(new string[] { "\n=\n" }, StringSplitOptions.RemoveEmptyEntries);
            Expression[][] hiDialogs = new Expression[dialogs.Length][];

            for (int a = 0; a < dialogs.Length; a++)
            {
                string[] express = dialogs[a].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
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
            string mf = File.ReadAllText(goodbyeDialogPath);
            string mainFile = mf.Replace("\r", String.Empty);
            string[] dialogs = mainFile.Split(new string[] { "\n=\n" }, StringSplitOptions.RemoveEmptyEntries);
            Expression[][] byeDialogs = new Expression[dialogs.Length][];

            for (int a = 0; a < dialogs.Length; a++)
            {
                string[] express = dialogs[a].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                Expression[] byeDialog = new Expression[express.Length];
                for (int b = 0; b < express.Length; b++)
                {
                    byeDialog[b] = new Expression(express[b].Substring(2), (express[b])[0].ToString());
                }
                byeDialogs[a] = byeDialog;
            }

            Random rnd = new Random();
            int dialogNum = rnd.Next(byeDialogs.Length);


            string[] expres = dialogs[dialogNum].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            Task.Factory.StartNew(new Action(() =>
            {
                consoleWrite("Говорим", true);
            _ = Say(byeDialogs[dialogNum]);
                int delay = 0;
                foreach(char s in dialogs[dialogNum])
                {
                    switch (s)
                    {
                        case '.':
                            delay += 600;
                            break;
                        default:
                            delay += 50;
                            break;
                    }
                }
                delay = delay + (expres.Length * 800) + 1000;
                Debug.WriteLine(dialogs[dialogNum][0] + "|" + expres.Length + "|" + delay);
                Thread.Sleep(delay); // sleep
                i++;
                if (i == 1)
                {
                    consoleWrite("Выходим", true);
                    MonikaOnDesktop.MonikaSettings.Default.isColdShutdown = false;
                    Environment.Exit(0);
                }
            }));
        }
        public void readIdleTxt()
        {
            string mf = File.ReadAllText(idleDialogPath);
            string mainFile = mf.Replace("\r", String.Empty);
            string[] dialogs = mainFile.Split(new string[] { "\n=\n" }, StringSplitOptions.RemoveEmptyEntries);
            Expression[][] idleDialogs;
            if (playerName == "Denis Solicen")
            {
                idleDialogs = new Expression[dialogs.Length + 2][];
            }
            else
            {
                idleDialogs = new Expression[dialogs.Length][];
            }

            for (int a = 0; a < dialogs.Length; a++)
            {
                string ex = dialogs[a].Replace("\r", String.Empty);
                string[] express = dialogs[a].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                Expression[] idleDialog = new Expression[express.Length];
                for (int b = 0; b < express.Length; b++)
                {
                    idleDialog[b] = new Expression(express[b].Substring(2), (express[b])[0].ToString());
                }
                idleDialogs[a] = idleDialog;
            }
            if (playerName == "Denis Solicen")
            {
                idleDialogs[dialogs.Length] = new[]
                    {
                        new Expression("Эй [player], чем ты там занимаешся?", "b"),
                        new Expression("Не забывай, что я живу у тебя на компьютере", "b"),
                        new Expression("Поэтому от меня не скрытся", "k"),
                        new Expression("Я же вижу, что ты занимаешся не тем, чем нужно", "k"),
                        new Expression("Люди ждут твоего перевода!", "k"),
                        new Expression("Лучше переведи Monika After Story Mod", "k")
                    };
                idleDialogs[dialogs.Length + 1] = new[]
                    {
                        new Expression("Я надеюсь, ты занимаешся переводом?", "b"),
                        new Expression("Знаешь, не очень хорошо говорить на английком и русском сразу", "b"),
                        new Expression("К тому же, люди могут и не знать английского", "b"),
                        new Expression("И это плохо, когда ты говоришь с человеком, который тебя не понимает", "b"),
                        new Expression("Я буду очень рада, если ты будешь переводить мод на русский язик", "b")
                    };
            }

            Random rnd = new Random();
            int dialogNum = rnd.Next(idleDialogs.Length);
            //Debug.WriteLine("Позапрошлый номер диалога: " + lastLastDialog);
            consoleWrite("Прошлый номер диалога: " + lastDialog, true);
            consoleWrite("Рандомный номер диалога: " + dialogNum, true);
            //while (dialogNum == lastDialog && dialogNum == lastLastDialog)
            while (dialogNum == lastDialog) // жоский костыль, без которого показываются повторные диалоги
            {
                consoleWrite("Номер диалога совпадает с старым, подбираю новый", true);
                dialogNum = rnd.Next(idleDialogs.Length);
            }
            //lastLastDialog = lastDialog;
            lastDialog = dialogNum;
            consoleWrite("Диалог не совпадает с старым, показываю: " + lastDialog, true);

            _ = Say(idleDialogs[dialogNum]);

        }
        public void readProgsTxt(string proc)
        {
            string sPath = progsDialogPath;
            string[] lines;
            List<string> list = new List<string>();
            Random rnd = new Random();

            using (StreamReader sr = new StreamReader(sPath))
            {
                lines = sr.ReadToEnd().Split('\n');
            }

            //Вечный цикл...
            //while (true)
            //{
            consoleWrite("Запущен процесс: " + proc, true);

            //Смотрим все процессы в файле и сравниваем с нашим
            //Если такой процесс нашелся, то выбираем все его диалоги и фразы в List<>
            var proccesses = lines.Select((v, i) => new { i, v }).Where(t => t.v.StartsWith("["));
            foreach (var s in proccesses)
            {
                var a = s.v.Trim(new char[] { '[', ']', '\r' });
                string[] b = a.Split("|");
                foreach (var c in b)
                {
                    if (c == proc)
                    {
                        var dialogs = lines.Skip(s.i + 1).TakeWhile(x => !x.StartsWith("["));

                        foreach (var v in dialogs)
                        {
                            if (v == "\r") continue; //Здесь избавляемся от той проблемы с пустой строкой, в посте выше.
                            list.Add(v);
                        }
                        break;
                    }
                }
            }


            //Ищем индексы вхождений строк диалогов (начинаются с "<")
            var dialIdx = list.Select((v, i) => new { i, v }).Where(t => t.v.StartsWith("<"));
            List<int> idx = new List<int>();

            //Добавляем эти индексы в список, из которого будем брать случайный элемент (диалог)
            foreach (var s in dialIdx)
                idx.Add(s.i);

            //ГПСЧ
            int rx = rnd.Next(0, idx.Count);

            //Если список пуст -> уходим на второй круг... (т.е. такого процесса нет)
            //if (list.Count == 0) continue;

            if (list.Count != 0)
            {
                consoleWrite("<----------Вывод случайного диалога---------->", false);

                //Печатаем диалог
                var Dialog = list.Skip(idx[rx]).Take(1).Select(s => s).ToArray();
                consoleWrite(Dialog[0], true);

                //Печатаем фразы к нему
                List<string> Phrases = list.Skip(idx[rx] + 1).TakeWhile(x => !x.StartsWith("<")).ToList();
                Expression[] progDialog = new Expression[Phrases.Count];
                for (int i = 0; i < Phrases.Count; i++)
                {
                    Debug.WriteLine(Phrases[i]);
                    progDialog[i] = new Expression(Phrases[i].Substring(2), (Phrases[i])[0].ToString());
                }
                _ = Say(progDialog);
            }
            //Моем полы
            list.Clear();
        }
        public void readSitesTxt(string site)
        {
            string sPath = sitesDialogPath;
            string[] lines;
            List<string> list = new List<string>();
            Random rnd = new Random();

            using (StreamReader sr = new StreamReader(sPath))
            {
                lines = sr.ReadToEnd().Split('\n');
            }

            //Вечный цикл...
            //while (true)
            //{
            consoleWrite("Открыт сайт: " + site, true);

            //Смотрим все процессы в файле и сравниваем с нашим
            //Если такой процесс нашелся, то выбираем все его диалоги и фразы в List<>
            var proccesses = lines.Select((v, i) => new { i, v }).Where(t => t.v.StartsWith("["));
            foreach (var s in proccesses)
            {
                var a = s.v.Trim(new char[] { '[', ']', '\r' });
                string[] b = a.Split("|");
                foreach (var c in b)
                {
                    string d = c.ToLower().Trim().TrimEnd('/');
                    
                    //Обновлено определение сайтов на более новое через Regex.Matches - обновление подготовил Денис Солицен
                    MatchCollection allIp = Regex.Matches(d, site);
                    
                    if (d.StartsWith("http://"))
                    {
                        d = d.Substring(7);
                    }

                    if (d.StartsWith("https://"))
                    {
                        d = d.Substring(8);
                    }

                    if (d.StartsWith("www."))
                    {
                        d = d.Substring(4);
                    }
                    
                    //Собственно обновленный метод определения
                    foreach (Match ip in allIp)
                    {
                        var dialogs = lines.Skip(s.i + 1).TakeWhile(x => !x.StartsWith("["));
                        foreach (var v in dialogs)
                        {
                            if (v == "\r") continue; //Здесь избавляемся от той проблемы с пустой строкой, в посте выше.
                            list.Add(v);
                        }
                        break;
                    }
                    
    
                }
            }


            //Ищем индексы вхождений строк диалогов (начинаются с "<")
            var dialIdx = list.Select((v, i) => new { i, v }).Where(t => t.v.StartsWith("<"));
            List<int> idx = new List<int>();

            //Добавляем эти индексы в список, из которого будем брать случайный элемент (диалог)
            foreach (var s in dialIdx)
                idx.Add(s.i);

            //ГПСЧ
            int rx = rnd.Next(0, idx.Count);

            //Если список пуст -> уходим на второй круг... (т.е. такого процесса нет)
            //if (list.Count == 0) continue;

            if (list.Count != 0)
            {
                consoleWrite("<----------Вывод случайного диалога---------->", false);

                //Печатаем диалог
                var Dialog = list.Skip(idx[rx]).Take(1).Select(s => s).ToArray();
                consoleWrite(Dialog[0], true);

                //Печатаем фразы к нему
                List<string> Phrases = list.Skip(idx[rx] + 1).TakeWhile(x => !x.StartsWith("<")).ToList();
                Expression[] siteDialog = new Expression[Phrases.Count];
                for (int i = 0; i < Phrases.Count; i++)
                {
                    Debug.WriteLine(Phrases[i]);
                    siteDialog[i] = new Expression(Phrases[i].Substring(2), (Phrases[i])[0].ToString());
                }
                _ = Say(siteDialog);
            }
            //Моем полы
            list.Clear();
        }
        public void readGoogleTxt(string site)
        {
            string sPath = googleDialogPath;
            string[] lines;
            List<string> list = new List<string>();
            Random rnd = new Random();

            using (StreamReader sr = new StreamReader(sPath))
            {
                lines = sr.ReadToEnd().Split('\n');
            }

            //Вечный цикл...
            //while (true)
            //{
            consoleWrite("Открыт сайт: " + site, true);
            var googleMatchDeb = Regex.Match(site, GOOGLE_REGEX, RegexOptions.Compiled);
            var searchDeb = HttpUtility.UrlDecode(googleMatchDeb.Groups[1].ToString()).Trim();
            consoleWrite("Извлекаю запрос: " + searchDeb.ToLower().Trim(), true);

            //Смотрим все процессы в файле и сравниваем с нашим
            //Если такой процесс нашелся, то выбираем все его диалоги и фразы в List<>
            var proccesses = lines.Select((v, i) => new { i, v }).Where(t => t.v.StartsWith("["));
            foreach (var s in proccesses)
            {
                var a = s.v.Trim(new char[] { '[', ']', '\r' });
                string[] b = a.Split("|");
                foreach (var c in b)
                {
                    string d = c.ToLower().Trim().TrimEnd('/');
                    if (d.StartsWith("http://"))
                    {
                        d = d.Substring(7);
                    }

                    if (d.StartsWith("https://"))
                    {
                        d = d.Substring(8);
                    }

                    if (d.StartsWith("www."))
                    {
                        d = d.Substring(4);
                    }

                    var googleMatch = Regex.Match(site, GOOGLE_REGEX, RegexOptions.Compiled);
                    if (googleMatch.Success)
                    {
                        var search = HttpUtility.UrlDecode(googleMatch.Groups[1].ToString()).Trim();
                        if (d == search.ToLower().Trim())
                        {
                            var dialogs = lines.Skip(s.i + 1).TakeWhile(x => !x.StartsWith("["));

                            foreach (var v in dialogs)
                            {
                                if (v == "\r") continue; //Здесь избавляемся от той проблемы с пустой строкой, в посте выше.
                                list.Add(v);
                            }
                            break;
                        }
                    }
                }
            }


            //Ищем индексы вхождений строк диалогов (начинаются с "<")
            var dialIdx = list.Select((v, i) => new { i, v }).Where(t => t.v.StartsWith("<"));
            List<int> idx = new List<int>();

            //Добавляем эти индексы в список, из которого будем брать случайный элемент (диалог)
            foreach (var s in dialIdx)
                idx.Add(s.i);

            //ГПСЧ
            int rx = rnd.Next(0, idx.Count);

            //Если список пуст -> уходим на второй круг... (т.е. такого процесса нет)
            //if (list.Count == 0) continue;

            if (list.Count != 0)
            {
                consoleWrite("<----------Вывод случайного диалога---------->", false);

                //Печатаем диалог
                var Dialog = list.Skip(idx[rx]).Take(1).Select(s => s).ToArray();
                consoleWrite(Dialog[0], true);

                //Печатаем фразы к нему
                List<string> Phrases = list.Skip(idx[rx] + 1).TakeWhile(x => !x.StartsWith("<")).ToList();
                Expression[] siteDialog = new Expression[Phrases.Count];
                for (int i = 0; i < Phrases.Count; i++)
                {
                    //consoleWrite(Phrases[i], false);
                    siteDialog[i] = new Expression(Phrases[i].Substring(2), (Phrases[i])[0].ToString());
                }
                _ = Say(siteDialog);
            }
            //Моем полы
            list.Clear();
        }
        public void readYoutubeTxt(string site)
        {
            string sPath = youtubeDialogPath;
            string[] lines;
            List<string> list = new List<string>();
            Random rnd = new Random();

            using (StreamReader sr = new StreamReader(sPath))
            {
                lines = sr.ReadToEnd().Split('\n');
            }

            //Вечный цикл...
            //while (true)
            //{
            consoleWrite("Открыт сайт: " + site, true);
            var googleMatchDeb = Regex.Match(site, YOUTUBE_REGEX, RegexOptions.Compiled);
            var searchDeb = HttpUtility.UrlDecode(googleMatchDeb.Groups[1].ToString()).Trim();
            consoleWrite("Извлекаю запрос: " + searchDeb.ToLower().Trim(), true);

            //Смотрим все процессы в файле и сравниваем с нашим
            //Если такой процесс нашелся, то выбираем все его диалоги и фразы в List<>
            var proccesses = lines.Select((v, i) => new { i, v }).Where(t => t.v.StartsWith("["));
            foreach (var s in proccesses)
            {
                var a = s.v.Trim(new char[] { '[', ']', '\r' });
                string[] b = a.Split("|");
                foreach (var c in b)
                {
                    string d = c.ToLower().Trim().TrimEnd('/');
                    if (d.StartsWith("http://"))
                    {
                        d = d.Substring(7);
                    }

                    if (d.StartsWith("https://"))
                    {
                        d = d.Substring(8);
                    }

                    if (d.StartsWith("www."))
                    {
                        d = d.Substring(4);
                    }

                    var ytMatch = Regex.Match(site, YOUTUBE_REGEX, RegexOptions.Compiled);
                    if (ytMatch.Success)
                    {
                        var search = HttpUtility.UrlDecode(ytMatch.Groups[1].ToString()).Trim();
                        if (d == search.ToLower().Trim())
                        {
                            var dialogs = lines.Skip(s.i + 1).TakeWhile(x => !x.StartsWith("["));

                            foreach (var v in dialogs)
                            {
                                if (v == "\r") continue; //Здесь избавляемся от той проблемы с пустой строкой, в посте выше.
                                list.Add(v);
                            }
                            break;
                        }
                    }
                }
            }


            //Ищем индексы вхождений строк диалогов (начинаются с "<")
            var dialIdx = list.Select((v, i) => new { i, v }).Where(t => t.v.StartsWith("<"));
            List<int> idx = new List<int>();

            //Добавляем эти индексы в список, из которого будем брать случайный элемент (диалог)
            foreach (var s in dialIdx)
                idx.Add(s.i);

            //ГПСЧ
            int rx = rnd.Next(0, idx.Count);

            //Если список пуст -> уходим на второй круг... (т.е. такого процесса нет)
            //if (list.Count == 0) continue;

            if (list.Count != 0)
            {
                consoleWrite("<----------Вывод случайного диалога---------->", false);
                //Debug.WriteLine("<----------Вывод случайного диалога---------->");

                //Печатаем диалог
                var Dialog = list.Skip(idx[rx]).Take(1).Select(s => s).ToArray();
                consoleWrite(Dialog[0], true);

                //Печатаем фразы к нему
                List<string> Phrases = list.Skip(idx[rx] + 1).TakeWhile(x => !x.StartsWith("<")).ToList();
                Expression[] siteDialog = new Expression[Phrases.Count];
                for (int i = 0; i < Phrases.Count; i++)
                {
                    //Debug.WriteLine(Phrases[i]);
                    siteDialog[i] = new Expression(Phrases[i].Substring(2), (Phrases[i])[0].ToString());
                }
                _ = Say(siteDialog);
            }
            //Моем полы
            list.Clear();
        }
        */

        #endregion

        List<DialogModel> dm = new List<DialogModel>();
        int num = 0;
        #region
        public async void readXml(string sPath, int type)
        {
            // string sPath = idleDialogPath;
            string mainXML = "<Dialogs>\n\t<Dialog>";

            StreamReader f = new StreamReader(sPath);
            while (!f.EndOfStream)
            {
                string m = f.ReadLine();
                string s = m.Replace("\r", String.Empty).Replace("    ", "\t");
                string S = "";
                if (s.Contains("menu:"))
                {
                    S = s.Replace("menu:", "\n\t\t<Menu>");
                }
                if (s.Contains("menuend"))
                {
                    S = s.Replace("menuend", "\n\t\t\t</Answer>\n\t\t</Menu>");
                }
                if (s.Contains("\t\t"))
                {
                    //S = s.Insert(0, "\n\t\t\t<answer text = \"") + "\">";
                    S = s.Replace("\t\t", "\n\t\t\t<Answer text=\"") + "\">";
                }
                if (s.Contains("ansend"))
                {
                    S = s.Replace("ansend", "\n\t\t\t</Answer>");
                }
                if (s.Contains("\t\t\t"))
                {
                    S = s.Replace("\t\t\t", "\n\t\t\t\t<Text>") + "</Text>\n\t\t\t</Answer>\n\t\t</Menu>";
                }
                if (!s.Contains("\t\t") && !s.Contains("\t\t") && !s.Contains("menuend") && !s.Contains("menu:"))
                {
                    S = s.Insert(0, "\n\t\t<Text>") + "</Text>";
                }
                mainXML += S;
            }
            f.Close();

            mainXML += "\n\t</Dialog>\n</Dialogs>";
            string mainxml = mainXML.Replace("\n\t\t<Text>=</Text>", "\n\t</Dialog>\n\t<Dialog>");
            string mainXml = mainxml.Replace("\n\t\t\t</Answer>\n\t\t</Menu>\n\t\t\t\t<Text>", "\n\t\t\t\t<Text>").Replace("\n\t\t\t</Answer>\n\t\t</Menu>\n\t\t\t<Answer", "\n\t\t\t<Answer").Replace("</Text>\n\t\t\t<Answer", "</Text>\n\t\t\t</Answer>\n\t\t\t<Answer");

            #region
            string s1 = mainXml.Replace("\t", String.Empty);
            string s2 = s1.Replace("\n", String.Empty);
            //Debug.WriteLine(mainXml);
            XmlDocument xDoc = new XmlDocument();
            //string path = testXml;
            //xDoc.Load(path);
            xDoc.LoadXml(s2);
            // получим корневой элемент
            XmlElement xRoot = xDoc.DocumentElement;
            // обход всех узлов в корневом элементе
            List<DialogModel> idm = new List<DialogModel>();
            foreach (XmlNode xnode in xRoot)
            {
                if (xnode.Name == "Dialog")
                {
                    //Console.WriteLine("Dialog:");
                    idm.Add(new DialogModel(xnode));
                }
            }
            dm = idm;
            Random rnd = new Random();
            switch (type)
            {
                case 0:
                    num = rnd.Next(idm.Count);
                    Debug.WriteLine("Прошлый номер диалога: " + lastDialog);
                    Debug.WriteLine("Рандомный номер диалога: " + num);
                    //while (dialogNum == lastDialog && dialogNum == lastLastDialog)
                    while (num == lastDialog) // жоский костыль, без которого показываются повторные диалоги
                    {
                        Debug.WriteLine("Номер диалога совпадает с старым, подбираю новый");
                        num = rnd.Next(idm.Count);
                    }
                    //lastLastDialog = lastDialog;
                    lastDialog = num;
                    Debug.WriteLine("Диалог не совпадает с старым, показываю: " + lastDialog);
                    sayIdle();
                    break;
                case 1:
                    num = rnd.Next(idm.Count);
                    await sayIdle();
                    Environment.Exit(0);
                    break;
            }

            #endregion

        }
        int dialogNum;
        public async Task sayIdle()
        {
            isSpeaking = true;
            this.Dispatcher.Invoke(() => { textWindow.Visibility = Visibility.Visible; });
            Debug.WriteLine(dm[num].Node.InnerXml);
            for (int u = dialogNum; u < dm[num].Node.ChildNodes.Count; u++)
            {
                int delay = 0;
                XmlNode childnode = dm[num].Node.ChildNodes[u];
                // если узел age
                if (childnode.Name == "Menu")
                {
                    string[] q = new string[childnode.ChildNodes.Count];
                    Expression[][] ex = new Expression[childnode.ChildNodes.Count][];
                    for (int i = 0; i < childnode.ChildNodes.Count; i++)
                    {
                        XmlNode attr = childnode.ChildNodes[i].Attributes.GetNamedItem("text");
                        q[i] = attr.Value;
                        Expression[] ex1 = new Expression[childnode.ChildNodes[i].ChildNodes.Count];
                        for (int a = 0; a < childnode.ChildNodes[i].ChildNodes.Count; a++)
                        {
                            ex1[a] = new Expression(childnode.ChildNodes[i].ChildNodes[a].InnerText.Substring(5), childnode.ChildNodes[i].ChildNodes[a].InnerText.Substring(0, 4).ToString());
                        }
                        ex[i] = ex1;
                    }
                    Menu(dm[num].Node.ChildNodes[u - 1].InnerText.Substring(5), q, ex);
                    dialogNum = u + 1;
                    Debug.WriteLine(dialogNum);
                    break;
                }
                if (childnode.Name == "Text")
                {
                    //Console.WriteLine(childnode.InnerText);
                    //exList.Add(new Expression(childnode.InnerText.Substring(2), childnode.InnerText[0].ToString()));
                    try
                    {
                        await Say(new[] { new Expression(childnode.InnerText.Substring(5), childnode.InnerText.Substring(0, 4)) });
                    }
                    catch
                    {
                        Debug.WriteLine("ERR");
                    }
                }
                if (u >= dm[num].Node.ChildNodes.Count - 1)
                {
                    isSpeaking = false;
                    Debug.WriteLine("End");
                    this.Dispatcher.Invoke(() => {
                        textWindow.Visibility = Visibility.Hidden;
                        setFace("1esa");
                    });
                    dialogNum = 0;
                    break;
                }
                //Thread.Sleep(delay1); // sleep
                //await Task.Delay(delay1);
            }
        }
        #endregion

        List<NamedDialogModel> ldm = new List<NamedDialogModel>();
        public void readLongXml(string Name, string sPath, int type)
        {
            #region
            //string sPath = progsDialogPath;
            string mainXML = "<Mains>";

            StreamReader f = new StreamReader(sPath);
            while (!f.EndOfStream)
            {
                string m = f.ReadLine();
                string s = m.Replace("\r", String.Empty);
                string S = "";
                if (s.Contains("menu:"))
                {
                    S = s.Replace("menu:", "\n\t\t\t<Menu>");
                }
                if (s.Contains("menuend"))
                {
                    S = s.Replace("menuend", "\n\t\t\t</Menu>");
                }
                if (s.Contains("\t\t"))
                {
                    //S = s.Insert(0, "\n\t\t\t<answer text = \"") + "\">";
                    S = s.Replace("\t\t", "\n\t\t\t\t<Answer text=\"") + "\">";
                }
                if (s.Contains("ansend"))
                {
                    S = s.Replace("ansend", "\n\t\t\t\t</Answer>");
                }/*
                if (s.Contains("\t\t<Text>["))
                {
                    S = s.Replace("\t\t<Text>[", "\n\t<Process name=\"");
                    S = s.Replace("]</Text>", "\">");
                }*/
                if (s.Contains("\t\t\t") && !s.Contains("["))
                {
                    S = s.Replace("\t\t\t", "\n\t\t\t\t\t<Text>") + "</Text>\n\t\t\t\t</Answer>\n\t\t\t</Menu>";
                }
                if (!s.Contains("\t\t") && !s.Contains("\t\t") && !s.Contains("menuend") && !s.Contains("menu:"))
                {
                    S = s.Insert(0, "\n\t\t\t<Text>") + "</Text>";
                }
                mainXML += S;
            }
            f.Close();

            mainXML += "\n\t\t</Dialog>\n\t</Main>\n</Mains>";
            string mainxml = mainXML.Replace("\t\t\t<Text>=</Text>", "\t\t</Dialog>\n\t\t<Dialog>").Replace("\t\t\t<Text>[", "\t\t</Dialog>\n\t</Main>\n\t<Main name=\"").Replace("]</Text>", "\">\n\t\t<Dialog>");
            string mainXml = mainxml.Replace("<Mains>\n\t\t</Dialog>\n\t</Main>", "<Mains>").Replace("\n\t\t\t\t</Answer>\n\t\t\t</Menu>\n\t\t\t\t\t<Text>", "\n\t\t\t\t\t<Text>").Replace("\n\t\t\t\t</Answer>\n\t\t\t</Menu>\n\t\t\t\t<Answer", "\n\t\t\t\t<Answer").Replace("</Text>\n\t\t\t\t<Answer", "</Text>\n\t\t\t\t</Answer>\n\t\t\t\t<Answer");

            //Console.Write("XML Example:\n" + mainXml);
            Console.WriteLine("Print dialogues:\n");
            #endregion
            #region
            string s1 = mainXml.Replace("\t", String.Empty);
            string s2 = s1.Replace("\n", String.Empty);
            XmlDocument xDoc = new XmlDocument();
            //string path = testXml;
            //xDoc.Load(path);
            xDoc.LoadXml(s2);
            // получим корневой элемент
            XmlElement xRoot = xDoc.DocumentElement;
            string[][] names = new string[xRoot.ChildNodes.Count][];
            List<NamedDialogModel> Ldm = new List<NamedDialogModel>();
            // обход всех узлов в корневом элементе
            foreach (XmlNode xnode in xRoot)
            {
                List<DialogModel> dm = new List<DialogModel>();
                string[] name = xnode.Attributes.GetNamedItem("name").Value.Split("|");
                if (xnode.Name == "Main")
                {
                    foreach (XmlNode progsnode in xnode.ChildNodes)
                    {
                        dm.Add(new DialogModel(progsnode));
                    }
                }
                Ldm.Add(new NamedDialogModel(name, dm));
                //Console.WriteLine(ndm[0].Names[0] + "|" + ndm[0].DM[0].Node.InnerText);
            }
            dialogNum = 0;
            switch (type)
            {
                case 0:
                    foreach (NamedDialogModel NDM in Ldm)
                    {
                        if (NDM.Names.Contains(Name))
                        {
                            dm = NDM.DM;

                            Random rnd = new Random();
                            num = rnd.Next(dm.Count);
                            sayIdle();
                        }
                    }
                    break;
                case 1:
                    foreach (NamedDialogModel NDM in Ldm)
                    {
                        foreach (string c in NDM.Names)
                        {
                            string d = c.ToLower().Trim().TrimEnd('/');

                            //Обновлено определение сайтов на более новое через Regex.Matches - обновление подготовил Денис Солицен

                            if (d.StartsWith("http://"))
                            {
                                d = d.Substring(7);
                            }

                            if (d.StartsWith("https://"))
                            {
                                d = d.Substring(8);
                            }

                            if (d.StartsWith("www."))
                            {
                                d = d.Substring(4);
                            }
                            if (Name.Contains(d))
                            {
                                dm = NDM.DM;

                                Random rnd = new Random();
                                num = rnd.Next(dm.Count);
                                sayIdle();
                            }
                        }
                    }
                    break;
                case 2:
                    foreach (NamedDialogModel NDM in Ldm)
                    {
                        foreach (string c in NDM.Names)
                        {
                            string d = c.ToLower().Trim().TrimEnd('/');

                            //Обновлено определение сайтов на более новое через Regex.Matches - обновление подготовил Денис Солицен

                            if (d.StartsWith("http://"))
                            {
                                d = d.Substring(7);
                            }

                            if (d.StartsWith("https://"))
                            {
                                d = d.Substring(8);
                            }

                            if (d.StartsWith("www."))
                            {
                                d = d.Substring(4);
                            }
                            var googleMatch = Regex.Match(Name, GOOGLE_REGEX, RegexOptions.Compiled);
                            if (googleMatch.Success)
                            {
                                var search = HttpUtility.UrlDecode(googleMatch.Groups[1].ToString()).Trim();
                                if (search.ToLower().Trim().Contains(d))
                                {
                                    dm = NDM.DM;

                                    Random rnd = new Random();
                                    num = rnd.Next(dm.Count);
                                    sayIdle();
                                }
                            }
                        }
                    }
                    break;
                case 3:
                    foreach (NamedDialogModel NDM in Ldm)
                    {
                        foreach (string c in NDM.Names)
                        {
                            string d = c.ToLower().Trim().TrimEnd('/');

                            //Обновлено определение сайтов на более новое через Regex.Matches - обновление подготовил Денис Солицен

                            if (d.StartsWith("http://"))
                            {
                                d = d.Substring(7);
                            }

                            if (d.StartsWith("https://"))
                            {
                                d = d.Substring(8);
                            }

                            if (d.StartsWith("www."))
                            {
                                d = d.Substring(4);
                            }
                            var youtubeMatch = Regex.Match(Name, YOUTUBE_REGEX, RegexOptions.Compiled);
                            if (youtubeMatch.Success)
                            {
                                var search = HttpUtility.UrlDecode(youtubeMatch.Groups[1].ToString()).Trim();
                                if (search.ToLower().Trim().Contains(d))
                                {
                                    dm = NDM.DM;

                                    Random rnd = new Random();
                                    num = rnd.Next(dm.Count);
                                    sayIdle();
                                }
                            }
                        }
                    }
                    break;

            }
            #endregion

        }
        public void consoleWrite(string text, bool time)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (time)
                {
                    Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "--> " + text);
                }
                else
                {
                    Debug.WriteLine(text);
                }
            });
        }
        public void SetupScale(int scaler)
        {
            switch (scaler)
            {
                case 0:
                    this.Width = 200;
                    this.Height = 128;
                    this.main.Margin = new Thickness(0, -15, 0, 0);
                    this.hairBack.Margin = new Thickness(0, -15, 0, 0);
                    this.hairFront.Margin = new Thickness(0, -15, 0, 0);
                    this.arms.Margin = new Thickness(0, -15, 0, 0);
                    this.ribbon.Margin = new Thickness(0, -15, 0, 0);
                    this.textWindow.Margin = new Thickness(12.5, 97.5, 12.5, 5);
                    this.textBlock.Margin = new Thickness(17.5, 102.5, 17.5, 10);
                    this.textBlock.FontSize = 5;
                    this.ButtonsGrid.Width = 125;
                    this.ButtonsGrid.Height = 100;
                    break;
                case 1:
                    this.Width = 400;
                    this.Height = 256;
                    this.main.Margin = new Thickness(0, -15, 0, 0);
                    this.hairBack.Margin = new Thickness(0, -15, 0, 0);
                    this.hairFront.Margin = new Thickness(0, -15, 0, 0);
                    this.arms.Margin = new Thickness(0, -15, 0, 0);
                    this.ribbon.Margin = new Thickness(0, -15, 0, 0);
                    this.textWindow.Margin = new Thickness(25, 195, 25, 10);
                    this.textBlock.Margin = new Thickness(35, 205, 35, 20);
                    this.textBlock.FontSize = 10;
                    this.ButtonsGrid.Width = 250;
                    this.ButtonsGrid.Height = 200;
                    break;
                case 2:
                    this.Width = 600;
                    this.Height = 384;
                    this.main.Margin = new Thickness(0, -15, 0, 0);
                    this.hairBack.Margin = new Thickness(0, -15, 0, 0);
                    this.hairFront.Margin = new Thickness(0, -15, 0, 0);
                    this.arms.Margin = new Thickness(0, -15, 0, 0);
                    this.ribbon.Margin = new Thickness(0, -15, 0, 0);
                    this.textWindow.Margin = new Thickness(37.5, 292.5, 37.5, 15);
                    this.textBlock.Margin = new Thickness(52.5, 307.5, 52.5, 30);
                    this.textBlock.FontSize = 15;
                    this.ButtonsGrid.Width = 375;
                    this.ButtonsGrid.Height = 300;
                    break;
                case 3:
                    this.Width = 800;
                    this.Height = 512;
                    this.main.Margin = new Thickness(0, -20, 0, 0);
                    this.hairBack.Margin = new Thickness(0, -20, 0, 0);
                    this.hairFront.Margin = new Thickness(0, -20, 0, 0);
                    this.arms.Margin = new Thickness(0, -20, 0, 0);
                    this.ribbon.Margin = new Thickness(0, -20, 0, 0);
                    this.textWindow.Margin = new Thickness(50, 390, 50, 20);
                    this.textBlock.Margin = new Thickness(70, 410, 70, 40);
                    this.textBlock.FontSize = 20;
                    this.ButtonsGrid.Width = 500;
                    this.ButtonsGrid.Height = 400;
                    break;
                default:
                    this.Width = 600;
                    this.Height = 384;
                    this.main.Margin = new Thickness(0, -15, 0, 0);
                    this.hairBack.Margin = new Thickness(0, -15, 0, 0);
                    this.hairFront.Margin = new Thickness(0, -15, 0, 0);
                    this.arms.Margin = new Thickness(0, -15, 0, 0);
                    this.ribbon.Margin = new Thickness(0, -15, 0, 0);
                    this.textWindow.Margin = new Thickness(37.5, 292.5, 37.5, 15);
                    this.textBlock.Margin = new Thickness(52.5, 307.5, 52.5, 30);
                    this.textBlock.FontSize = 15;
                    this.ButtonsGrid.Width = 375;
                    this.ButtonsGrid.Height = 300;
                    break;
            }

            //var primaryMonitorArea = SystemParameters.WorkArea;
            //Left = primaryMonitorArea.Right - this.Width;
            //Top = primaryMonitorArea.Bottom - this.Height;
            GoToSecondaryMonitor();
        }
        public void GoToSecondaryMonitor()
        {
            System.Windows.Forms.Screen screen;
            if (System.Windows.Forms.Screen.AllScreens.Length != 1)
            {
                if (MonikaSettings.Default.screenNum)
                {
                    screen = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(s => !s.Primary);
                }
                else
                {
                    screen = System.Windows.Forms.Screen.AllScreens[0];
                }
            }
            else
            {
                screen = System.Windows.Forms.Screen.AllScreens[0];
            }
            if (screen == null)
            {
                return;
            }

            var workingArea = screen.WorkingArea;
            this.Left = workingArea.Right - this.Width;
            this.Top = workingArea.Bottom - this.Height;
        }

        const string name = "MonikaStartUp";
        public bool SetAutorunValue(bool autorun)
        {
            //string ExePath = System.Windows.Forms.Application.ExecutablePath;
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            try
            {
                if (autorun)
                    reg.SetValue(name, ExePath);
                else
                {
                    RegistryKey WN = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                    if (WN.GetValue("MonikaStartUp") != null)
                    {
                        reg.DeleteValue(name);
                    }
                }

                reg.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }
        public string formatURL(string url)
        {
            string newUrl = url.ToLower().Trim().TrimEnd('/');
            if (newUrl.StartsWith("http://"))
            {
                newUrl = newUrl.Substring(7);
            }

            if (newUrl.StartsWith("https://"))
            {
                newUrl = newUrl.Substring(8);
            }

            if (newUrl.StartsWith("www."))
            {
                newUrl = newUrl.Substring(4);
            }
            return newUrl;
        }

        public static List<CultureInfo> m_Languages = new List<CultureInfo>();
        public static List<CultureInfo> Languages
        {
            get
            {
                return m_Languages;
            }
        }
        //Евент для оповещения всех окон приложения
        public static event EventHandler LanguageChanged;
        public static CultureInfo Lang
        {
            get
            {
                return System.Threading.Thread.CurrentThread.CurrentUICulture;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (value == System.Threading.Thread.CurrentThread.CurrentUICulture) return;

                //1. Меняем язык приложения:
                System.Threading.Thread.CurrentThread.CurrentUICulture = value;

                //2. Создаём ResourceDictionary для новой культуры
                ResourceDictionary dict = new ResourceDictionary();
                Debug.WriteLine("Установлен язык: " + value.Name);
                switch (value.Name)
                {
                    case "ru-RU":
                        dict.Source = new Uri(String.Format("/Resources/lang.{0}.xaml", value.Name), UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("/Resources/lang.xaml", UriKind.Relative);
                        break;
                }

                //3. Находим старую ResourceDictionary и удаляем его и добавляем новую ResourceDictionary
                ResourceDictionary oldDict = (from d in System.Windows.Application.Current.Resources.MergedDictionaries
                                              where d.Source != null && d.Source.OriginalString.StartsWith("/Resources/lang.")
                                              select d).FirstOrDefault();
                if (oldDict != null)
                {
                    int ind = System.Windows.Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    System.Windows.Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                    System.Windows.Application.Current.Resources.MergedDictionaries.Insert(ind, dict);
                }
                else
                {
                    System.Windows.Application.Current.Resources.MergedDictionaries.Add(dict);
                }

                //4. Вызываем евент для оповещения всех окон.
                LanguageChanged(System.Windows.Application.Current, new EventArgs());
            }
        }
        private void App_LanguageChanged(Object sender, EventArgs e)
        {
            //MonikaSettings.Default.Language = Lang;
            //MonikaSettings.Default.Save();
        }
    }


}
