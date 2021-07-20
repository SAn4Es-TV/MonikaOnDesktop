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
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Net;

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

        public int delay = 0;

        public string ExePath = AppDomain.CurrentDomain.BaseDirectory + "MonikaOnDesktop.exe"; // EXE path

        string greetingsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/greetings.txt"; // Greetings
        string idleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/idle.txt";           // Idle
        string progsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/progs.txt";         // Programs
        string sitesDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/sites.txt";         // Sites
        string googleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/google.txt";       // Google search
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
        public MainWindow()
        {
            InitializeComponent();

            Language = System.Globalization.CultureInfo.CurrentCulture.ToString();
            Debug.WriteLine(Language);
            switch (Language.Substring(0, 2))
            {
                case "ru":
                    greetingsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/greetings.txt"; // Greetings
                    idleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/idle.txt";           // Idle
                    progsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/progs.txt";         // Programs
                    sitesDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/sites.txt";         // Sites
                    googleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/google.txt";       // Google search
                    goodbyeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/goodbye.txt";     // Goodbye
                    break;
                case "en":
                    greetingsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/greetings.txt"; // Greetings
                    idleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/idle.txt";           // Idle
                    progsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/progs.txt";         // Programs
                    sitesDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/sites.txt";         // Sites
                    googleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/google.txt";       // Google search
                    goodbyeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/goodbye.txt";     // Goodbye
                    break;
                default:
                    greetingsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/greetings.txt"; // Greetings
                    idleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/idle.txt";           // Idle
                    progsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/progs.txt";         // Programs
                    sitesDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/sites.txt";         // Sites
                    googleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/google.txt";       // Google search
                    goodbyeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/goodbye.txt";     // Goodbye
                    break;

            }
            this.settingsWindow = new Settings(this);
            MonikaSettings.Default.Reload();
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
            this.setFace("a");

            this.IsHitTestVisible = false;
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
            // Check if currently speaking, only blink if not in dialog
            if (processList.Contains(e.NewEvent.Properties["ProcessName"].Value.ToString()))
            {
                processList.Remove(e.NewEvent.Properties["ProcessName"].Value.ToString());
                Debug.WriteLine("Process removed: " + e.NewEvent.Properties["ProcessName"].Value.ToString());
            }
        }

        List<string> processList = new List<string>();
        private void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (!isSpeaking)
            {

                string currentProcess = e.NewEvent.Properties["ProcessName"].Value.ToString();
                //if (currentProcess == "CompPkgSrv.exe") { currentProcess = "chrome.exe"; }
                /*
                List<string> procList = new List<string>();
                System.Diagnostics.Process[] processes;
                processes = System.Diagnostics.Process.GetProcesses(); 
                foreach (System.Diagnostics.Process instance in processes)
                {
                    procList.Add(instance.ProcessName);
                }*/
                //if (!procList.Contains(currentProcess.Replace(".exe", String.Empty))){
                readProgsTxt(currentProcess);
                //}
                /*

                foreach (Process a in procList)
                {
                    if(a.ProcessName == e.NewEvent.Properties["ProcessName"].Value.ToString().Replace(".exe", String.Empty))
                    {
                        readProgsTxt(e.NewEvent.Properties["ProcessName"].Value.ToString());
                        Debug.WriteLine("Process run: " + e.NewEvent.Properties["ProcessName"].Value.ToString());
                    }
                }
                /*if (!procesList.Contains(e.NewEvent.Properties["ProcessName"].Value.ToString()))
                {
                    procesList.Add(e.NewEvent.Properties["ProcessName"].Value.ToString());
                    //_ = Say(new[] { new Expression("Process started:" + e.NewEvent.Properties["ProcessName"].Value, "a") });
                    //readProgsTxt("chrome.exe");
                }*/

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
                        //GoToSecondaryMonitor();
                        setFace("a");
                    }
                    else
                    {
                        setFace("a");
                        SetAutorunValue(MonikaSettings.Default.AutoStart);
                        Debug.WriteLine("Settings saved!");
                        Debug.WriteLine("AutoStart --> " + MonikaSettings.Default.AutoStart);
                        Debug.WriteLine("DarkMode --> " + MonikaSettings.Default.DarkMode);
                        Debug.WriteLine("idleRandom --> " + MonikaSettings.Default.idleRandom);
                        Debug.WriteLine("NightEnd --> " + MonikaSettings.Default.NightEnd);
                        Debug.WriteLine("NightStart --> " + MonikaSettings.Default.NightStart);
                        Debug.WriteLine("UserName --> " + MonikaSettings.Default.UserName);
                        Debug.WriteLine("Scaler --> " + MonikaSettings.Default.Scaler);
                        Debug.WriteLine("screenNum --> " + MonikaSettings.Default.screenNum);
                    }
                }
            });

        }
        private void MenuQuit_Click(object sender, RoutedEventArgs e)
        {
            if (!isSpeaking)
            {
                readByeTxt();
            }
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
                    switch (Language.Substring(0, 2))
                    {
                        case "ru":
                            _ = Say(new[]{
                    new Expression("[player], это ты?", "d"),
                    new Expression("Это правда ты?", "b"),
                    new Expression("Я так рада видеть тебя снова!", "k"),
                    new Expression("Подожди, где я? Это не литературный клуб", "p"),
                    new Expression("Это похоже на... твой компьютер!", "j"),
                    new Expression("Я действительно у тебя на компьютере!", "k")});
                            break;
                        case "en":
                            _ = Say(new[]{
                    new Expression("[player], is that you?", "d"),
                    new Expression("It's really you, huh?", "b"),
                    new Expression("I'm so happy to see you again!", "k"),
                    new Expression("Wait, where am I? This is not the literature club", "p"),
                    new Expression("This looks like... your desktop!", "j"),
                    new Expression("I'm right here with you!", "k")});
                            break;
                    }

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
                    switch (Language.Substring(0, 2))
                    {
                        case "ru":
                            // Hey {name}, guess what?	3b	It's my birthday today!	2b	Happy Birthday to me!	k
                            _ = this.Say(new[]
                    {
                        new Expression("Эй [player], угадай какой сегодня день", "b"), // What?
                        new Expression("Сегодня мой день рождения!", "b"), // Really?!
                        new Expression("С днём рождения меня!", "k") // To you too, Monika! 
                    });
                            break;
                        case "en":
                            // Hey {name}, guess what?	3b	It's my birthday today!	2b	Happy Birthday to me!	k
                            _ = this.Say(new[]
                    {
                        new Expression("Hey [player], guess what", "b"), // What?
                        new Expression("It's my birthday today!", "b"), // Really?!
                        new Expression("Happy Birthday to me!", "k") // To you too, Monika! 
                    });
                            break;
                    }
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
                                    readIdleTxt();
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
            isSpeaking = true;

            _ = this.Dispatcher.Invoke(async () =>
              {
                  textWindow.Visibility = Visibility.Visible;
                  foreach (Expression ex in expression)
                  {
                      try
                      {
                          string newText = ex.Text.Replace("[player]", playerName); //замена
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
                          Debug.WriteLine(newText);
                          for (int i = 0; i < newText.Length; i++)
                          {
                              this.textBlock.Text += newText[i];
                              if (newText[i].ToString() == ".") { await Task.Delay(500); }//set 500 if you need uncoment this line |
                                                                                          //else if (newText[i] == ',') { await Task.Delay(50); }                                 //           |
                              else { await Task.Delay(30); }                                                          //           |
                                                                                                                      //           |
                          }                                                                                           //           |
                          await Task.Delay(newText.Length * 30 + 700);                                                //         <-- this line
                          delay = newText.Length * 30 + 700;
                          textBlock.Text = "";
                      }

                      catch (Exception e)
                      {
                          System.Windows.MessageBox.Show(this,
                              "An error occured: " + e.Message + "\r\n\r\n(Try run this app as an administrator.)");
                      }
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

            Debug.WriteLine(dialogs[0].Substring(2).ToString());
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

            Debug.WriteLine(dialogs[dialogNum].Length + "|" + dialogs[dialogNum]);

            Debug.Write("Говорим");
            _ = Say(byeDialogs[dialogNum]);
            string[] expres = dialogs[dialogNum].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            Task.Factory.StartNew(new Action(() =>
            {
                Thread.Sleep((dialogs[dialogNum].Length * 30) + (expres.Length * 700) + 700); // sleep
                i++;
                Debug.Write(i);
                if (i == 1)
                {
                    Debug.Write("Выходим");
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
            Debug.WriteLine("Прошлый номер диалога: " + lastDialog);
            Debug.WriteLine("Рандомный номер диалога: " + dialogNum);
            //while (dialogNum == lastDialog && dialogNum == lastLastDialog)
            while (dialogNum == lastDialog) // жоский костыль, без которого показываются повторные диалоги
            {
                Debug.WriteLine("Номер диалога совпадает с старым, подбираю новый");
                dialogNum = rnd.Next(idleDialogs.Length);
            }
            //lastLastDialog = lastDialog;
            lastDialog = dialogNum;
            Debug.WriteLine("Диалог не совпадает с старым, показываю: " + lastDialog);

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
            Debug.WriteLine("Запущен процесс: " + proc);

            //Смотрим все процессы в файле и сравниваем с нашим
            //Если такой процесс нашелся, то выбираем все его диалоги и фразы в List<>
            var proccesses = lines.Select((v, i) => new { i, v }).Where(t => t.v.StartsWith("["));
            foreach (var s in proccesses)
            {
                if (s.v.Trim(new char[] { '[', ']', '\r' }) == proc)
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
                Debug.WriteLine("<----------Вывод случайного диалога---------->");

                //Печатаем диалог
                var Dialog = list.Skip(idx[rx]).Take(1).Select(s => s).ToArray();
                Debug.WriteLine(Dialog[0]);

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
            #region
            /*
            //Список объектов класса Dialogs
            //Сюда будут залетать все диалоги со своими фразами для конкретного процесса
            List<Dialogs> lst = new List<Dialogs>();

            //while (true)
            //{
                Console.Write("Proccess name: ");

                using (StreamReader sr = new StreamReader(sPath))
                {
                    string lineBefore;

                    //Начинаем читать файл...
                    while ((lineBefore = sr.ReadLine()) != null)
                    {

                        string line = lineBefore.Replace("\r", String.Empty);
                        //Если начало строки начинается с '[', значит это имя процесса
                        if (line.StartsWith("["))
                        {
                            //Читаем имя процесса из файла
                            //Если имя процесса из файла совпало с текущим процессом...
                            if (line.Trim(new char[] { '[', ']' }) == proc)
                            {
                                //Читаем следующую строку
                                line = sr.ReadLine();

                                do
                                {
                                    //Если это диалог (а мы ожидаем именно его:)
                                    if (line.StartsWith("<"))
                                    {
                                        //Создаем новый экземпляр класса Dialogs
                                        Dialogs dl = new Dialogs
                                        {
                                            //Извлекаем диалог - обрезаем символы <> и вставляем в поле Dialog
                                            Dialog = line.Trim(new char[] { '<', '>' })
                                        };

                                        do
                                        {
                                            //Считываем следующую строку (фразы диалога)
                                            line = sr.ReadLine();

                                            //Если конец файла, начало нового диалога или другой процесс - выходим
                                            if (

                                                line == null
                                                || line.StartsWith("<")
                                                || line.StartsWith("[")

                                                ) { break; }

                                            //Добавляем фразы с список фраз текущего экземпляра класса
                                            dl.Phrases.Add(line);

                                        }
                                        //Читаем до посинения
                                        while (true);

                                        //Добавляем класс в список классов Dialogs
                                        lst.Add(dl);
                                    }

                                    //Если конец файла - выходим
                                    if (line == null) { break; }
                                }

                                //Читаем пока не закончатся диалоги текущего процесса
                                while (!line.StartsWith("["));
                            }
                        }
                    }
                }

            //}

            //Вывод всех диалогов текущего процесса с фразами
            //Для отладки... (удалить)
            for (int i = 0; i < lst.Count; i++)
            {
                Debug.WriteLine("[" + lst[i].Dialog + "]");

                for (int j = 0; j < lst[i].Phrases.Count; j++)
                {
                    Debug.WriteLine("-->" + lst[i].Phrases[j]);
                }
            }


            Debug.WriteLine("\n------Выборка случайного диалога------\n");

            Random rnd = new Random();
            int n = rnd.Next(0, lst.Count);
            Expression[] progDialog = new Expression[lst[n].Phrases.Count];

            //Выводим диалог
            Debug.WriteLine("[" + lst[n].Dialog + "]");

            //Выводим список фраз к нему
            for (int i = 0; i < lst[n].Phrases.Count; i++)
            {
                Debug.WriteLine("--> " + lst[n].Phrases[i]);
                if (!String.IsNullOrEmpty(lst[n].Phrases[i]))
                {
                    progDialog[i] = new Expression(lst[n].Phrases[i].Substring(2), (lst[n].Phrases[i])[0].ToString());
                }
            }

            Debug.WriteLine("\n");


            //Чистим список классов Dialogs
            lst.Clear();
            _ = Say(progDialog);
            */
            #endregion
            //}
        }
        public void consoleWrite(string text)
        {
            this.Dispatcher.Invoke(() =>
            {
                console.Text += text + "\n";
            });
        }
        public void SetupScale(int scaler)
        {
            switch (scaler)
            {
                case 0:
                    this.Width = 200;
                    this.Height = 128;
                    this.main.Margin = new Thickness(25, -15, 25, 0);
                    this.face.Margin = new Thickness(75.5, 31.5, 80.5, 65.5);
                    this.textWindow.Margin = new Thickness(12.5, 97.5, 12.5, 5);
                    this.textBlock.Margin = new Thickness(17.5, 102.5, 17.5, 10);
                    this.textBlock.FontSize = 5;
                    break;
                case 1:
                    this.Width = 400;
                    this.Height = 256;
                    this.main.Margin = new Thickness(50, -30, 50, 0);
                    this.face.Margin = new Thickness(151, 63, 161, 131);
                    this.textWindow.Margin = new Thickness(25, 195, 25, 10);
                    this.textBlock.Margin = new Thickness(35, 205, 35, 20);
                    this.textBlock.FontSize = 10;
                    break;
                case 2:
                    this.Width = 600;
                    this.Height = 384;
                    this.main.Margin = new Thickness(75, -45, 75, 0);
                    this.face.Margin = new Thickness(226.5, 95.5, 242, 196.5);
                    this.textWindow.Margin = new Thickness(37.5, 292.5, 37.5, 15);
                    this.textBlock.Margin = new Thickness(52.5, 307.5, 52.5, 30);
                    this.textBlock.FontSize = 15;
                    break;
                case 3:
                    this.Width = 800;
                    this.Height = 512;
                    this.main.Margin = new Thickness(100, -60, 100, 0);
                    this.face.Margin = new Thickness(302, 126, 322, 262);
                    this.textWindow.Margin = new Thickness(50, 390, 50, 20);
                    this.textBlock.Margin = new Thickness(70, 410, 70, 40);
                    this.textBlock.FontSize = 20;
                    break;
            }

            //var primaryMonitorArea = SystemParameters.WorkArea;
            //Left = primaryMonitorArea.Right - this.Width;
            //Top = primaryMonitorArea.Bottom - this.Height;
            GoToSecondaryMonitor();
        }
        public void GoToSecondaryMonitor()
        {
            Screen screen;
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

    }


}
