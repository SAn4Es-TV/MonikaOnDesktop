using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Microsoft.Win32;

using VGPrompter;

namespace MonikaOnDesktop {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        #region Всякое

        public DoubleAnimation _start;  // Анимация запуска
        public DoubleAnimation _quit;   // Анимация выхода

        String playerName;              // Имя игрока

        public int delay1 = 0;          // Задержка
        #endregion
        #region Пути
        public string ExePath = AppDomain.CurrentDomain.BaseDirectory + "MonikaOnDesktop.exe"; // Путь к ЕХЕ

        string greetingsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/greetings.txt"; // Приветствия
        string idleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/idle.txt";           // Рандомные диалоги
        string progsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/progs.txt";         // Реакции на программы
        string sitesDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/sites.txt";         // Реакции на сайты
        string googleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/google.txt";       // Реакции на запросы Гугуля
        string youtubeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/youtube.txt";     // Реакции на запросы Утуба
        string goodbyeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/goodbye.txt";     // Прощания
        string giftsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/gifts/gifts.txt";// Подарки
        string updateDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/upd.txt";          // Подарки

        static string subFolderPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/Sub/";

        DirectoryInfo subDialogDirectory = new DirectoryInfo(subFolderPath);
        DirectoryInfo greetingsDialogDirectory = new DirectoryInfo(subFolderPath + "Greetings/");
        DirectoryInfo idleDialogDirectory = new DirectoryInfo(subFolderPath + "Idle/");
        DirectoryInfo progsDialogDirectory = new DirectoryInfo(subFolderPath + "Progs/");
        DirectoryInfo sitesDialogDirectory = new DirectoryInfo(subFolderPath + "Sites/");
        DirectoryInfo googleDialogDirectory = new DirectoryInfo(subFolderPath + "Google/");
        DirectoryInfo youtubeDialogDirectory = new DirectoryInfo(subFolderPath + "YouTube/");
        DirectoryInfo goodbyeDialogDirectory = new DirectoryInfo(subFolderPath + "Goodbye/");
        List<DirectoryInfo> dirs = new List<DirectoryInfo>();
        #endregion
        #region Переменные
        public static bool IsNight => MonikaSettings.Default.DarkMode != "Day" &&
                                      (MonikaSettings.Default.DarkMode == "Night" || DateTime.Now.Hour > (MonikaSettings.Default.NightStart - 1) ||
                                       DateTime.Now.Hour < (MonikaSettings.Default.NightEnd + 1));               // Проверка День/Ночь
        public bool oldIsNight;
        public static bool IsBDay => DateTime.Now.Month == 9 && DateTime.Now.Day == 22;                          // Проверка Дня Рождения
        private bool applicationRunning = true;     // Запущено ли приложение (серьёзно, так нужно =ъ)
        public bool isSpeaking = true;              // Идёт ли разговорчик
        bool firsLaunch;                            // Первый ли запуск

        public int lastDialog;                      // Номер последнего диалога
        public int lastLastDialog;                  // Номер последнего последнего диалога (Лол, что???)

        private float dpiScale = 1.0f;

        string Language;                            // Текущий язык приложения

        private const string GOOGLE_REGEX = ".*\\.?google\\..{2,3}.*q\\=(.*?)($|&)";        // Шаблон запроса гугл (google.com/search?q=hi)
        private const string YOUTUBE_REGEX = ".*\\.?youtube\\..{2,3}.*y\\=(.*?)($|&)";      // Шаблон запроса ютуб (youtube.com/results?search_query=hi)

        public string lastProcess;                  // Имя прошлого процесса
        public string normalPose = "1esc";
        const string name = "MonikaStartUp";
        public string lastQuery;

        CharacterModel Monika = new CharacterModel(AppDomain.CurrentDomain.BaseDirectory + "/characters/monika.chr", AppDomain.CurrentDomain.BaseDirectory + "/characters/"); // Персонаж Моники
        private Settings settingsWindow;            // Окно настроек

        private NotifyIcon NI = new NotifyIcon();

        public float[] nightFilter = { 0.6861919617166911f, 0.387275212f, 0.27662517f };
        public float[] dayFilter = { 1f, 1f, 1f };
        public float[] mainFilter = { 1f, 1f, 1f };
        #endregion
        public MainWindow()     // Код главного окна
        {
            #region Этот код нам нафиг не нужен (зачем брать настройки из реестра, если они теперь хранятся в файле Моники)
            /*
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
            }*/
            #endregion

            InitializeComponent();                      // Инициализация ЮИ (Юзер Интерфейс)(Вроде для этого)
            oldIsNight = IsNight;
            mainFilter = nightFilter;
            AllowsTransparency = true;
            DirectoryInfo dirInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "/characters");
            if (!dirInfo.Exists) {
                dirInfo.Create();
            }
            Monika.loadData();
            firsLaunch = !Monika.fileExist();           // Если файла нету, то это первый запуск

            SolicenTEAM.Updater.ExeFileName = "MonikaOnDesktop"; // Имя ЕХЕ для перезапука
            SolicenTEAM.Updater.IgnoreFiles = "characters/monika.chr";     //  игнорируемый файл

            this.settingsWindow = new Settings(this);   // Объявляем окно настроек (так нужно)
            MonikaSettings.Default.Reload();            // Читаем настройки

            if (String.IsNullOrEmpty(MonikaSettings.Default.UserName) || MonikaSettings.Default.UserName == "{PlayerName}") {
                playerName = Environment.UserName;
            } else {
                playerName = Monika.playerName;
            }

            m_Languages.Clear();                        // Чистим список языков
            m_Languages.Add(new CultureInfo("en-US"));  // Нейтральная культура для этого проекта
            m_Languages.Add(new CultureInfo("ru-RU"));  // Стороняя культура
            LanguageChanged += App_LanguageChanged;     // Присваиваем функцию смены языка к ивенту смены языка
            Lang = new CultureInfo(Monika.lang);     // Ставим язык из настроек
            Language = Lang.Parent.ToString();          // Ставим имя языка
            //Debug.WriteLine(Language);                  // Дебуг язика
            setLanguage(Language);                      // Устанавливаем язык

            //playerName = "Denis Solicen";             // Режим Солицена
            this.setFace(normalPose);                       // Ставим спокойный вид

            setupFolders();

            LangBox.Visibility = Visibility.Hidden;
            NameBox.Visibility = Visibility.Hidden;
            textWindow.Visibility = Visibility.Hidden;  // Прячем розовую коробку текста
            textBlock.Text = "";                        // Убираем весь текст

            SetAutorunValue(Monika.autoStart);  // Ставим параметр автозапуска

            if (IsBDay)
                Debug.WriteLine("Сегодня день рождения?: Да");
            else
                Debug.WriteLine("Сегодня день рождения?: Нет");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");  // Я хз что это
            ManagementObjectCollection collection = searcher.Get();  // Также

            try {
                ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
                startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
                startWatch.Start();

                ManagementEventWatcher stopWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
                stopWatch.EventArrived += new EventArrivedEventHandler(stopWatch_EventArrived);
                stopWatch.Start();
            } catch (Exception ex) {
                System.Windows.MessageBox.Show(this,
                   "An error occured: " + ex.Message + "\r\n\r\n(Try run this app as an administrator.)");
            }
            SystemEvents.PowerModeChanged += OnPowerChange;

            FileSystemWatcher giftWatcher = new FileSystemWatcher();
            giftWatcher.Path = Monika.giftsPath;
            giftWatcher.NotifyFilter = NotifyFilters.FileName;
            giftWatcher.Created += GiftWatcher_Created;
            giftWatcher.EnableRaisingEvents = true;
            loadGifts();
            
            /*
            FileSystemWatcher acsWatcher = new FileSystemWatcher();
            acsWatcher.Path = Monika.giftsPath;
            acsWatcher.NotifyFilter = NotifyFilters.FileName;
            acsWatcher.Deleted += AcsWatcher_Deleted;
            acsWatcher.Created += AcsWatcher_Created;
            acsWatcher.EnableRaisingEvents = true;*/
        }
        public void Window_Loaded(object sender, RoutedEventArgs e)     // Когда программа проснётся
        {
            if (IsNight)
                mainFilter = nightFilter;
            else
                mainFilter = dayFilter;
            /*
            new ToastContentBuilder()
       .AddArgument("action", "viewConversation")
       .AddArgument("conversationId", 9813)
       .AddText("Смотри что я умею!")
       .AddText("Я научилась отправлять уведомления =)")
       .Show();*/
            var wpfDpi = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformToDevice.M11;
            this.dpiScale = 1f / (float)wpfDpi.GetValueOrDefault(1);

            Monika.loadData(); // Грузим данные 
            SetupScale(Monika.Scaler);  // Ставим размер окна
            Lang = MonikaSettings.Default.Language;
            _start = new DoubleAnimation();
            _start.From = 0;
            _start.To = 1;
            _start.RepeatBehavior = new RepeatBehavior(1);
            _start.Duration = new Duration(TimeSpan.FromMilliseconds(4000));
            _start.Completed += async (sender, args) => {
                //RegistryKey reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
                RegistryKey WN = Registry.CurrentUser.OpenSubKey("MonikaOnDesktop", true);

                if (firsLaunch && Monika.pcName != Environment.MachineName)     // Если Это первый запуск
                {
                    _ = FirstLaunch();
                } else if (!firsLaunch && Monika.pcName != Environment.MachineName) // Если сменили ПК
                  {
                    Monika.pcName = Environment.MachineName;
                    isSpeaking = true;
                    switch (Language.Substring(0, 2)) {
                        case "ru":
                        _ = this.Say(true, new[]
                {
                        new Expression("[player]...", "1rsu"), // What?
                        new Expression("Я чуствую себя как-то по другому..", "1rsu"), // Really?!
                        new Expression("Ты.. Сменил компьютер?", "1esu"), // Really?!
                        new Expression("Или.. Переустановил систему?", "1esc"), // Really?!
                        new Expression("В любом случае, ты спасибо тебе, что сохранил мой файл", "5esc")
                    });
                        break;
                        case "en":
                        _ = this.Say(true, new[]
                {
                        new Expression("[player]...", "1rsu"),
                        new Expression("I feel somehow different..", "1rsu"),
                        new Expression("You.. Changed computer?", "1esu"),
                        new Expression("Or.. Reinstalled the system?", "1esc"),
                        new Expression("Anyway, thank you for saving my file", "5esc")
                    });
                        break;
                        default:
                        _ = this.Say(true, new[]
                {
                        new Expression("[player]...", "1rsu"),
                        new Expression("I feel somehow different..", "1rsu"),
                        new Expression("You.. Changed computer?", "1esu"),
                        new Expression("Or.. Reinstalled the system?", "1esc"),
                        new Expression("Anyway, thank you for saving my file", "5esc")
                    });
                        break;
                    }
                    isSpeaking = false;
                }
                  // No idea where the date comes from, someone mentioned it in the spreadsheet. Seems legit.
                  else if (!firsLaunch && Monika.pcName == Environment.MachineName && IsBDay) // День рождения
                  {
                    isSpeaking = true;
                    switch (Language.Substring(0, 2)) {
                        case "ru":
                        // Hey {name}, guess what?	3b	It's my birthday today!	2b	Happy Birthday to me!	k
                        _ = this.Say(true, new[]
                {
                        new Expression("Эй [player], угадай какой сегодня день", "1euс"), // What?
                        new Expression("Сегодня мой день рождения!", "1suo"), // Really?!
                        new Expression("С днём рождения меня!", "2huo") // To you too, Monika! 
                    });
                        break;
                        case "en":
                        // Hey {name}, guess what?	3b	It's my birthday today!	2b	Happy Birthday to me!	k
                        _ = this.Say(true, new[]
                {
                        new Expression("Hey [player], guess what", "1euс"), // What?
                        new Expression("It's my birthday today!", "1suo"), // Really?!
                        new Expression("Happy Birthday to me!", "2huo") // To you too, Monika! 
                    });
                        break;
                        default:
                        // Hey {name}, guess what?	3b	It's my birthday today!	2b	Happy Birthday to me!	k
                        _ = this.Say(true, new[]
                {
                        new Expression("Hey [player], guess what", "1euс"), // What?
                        new Expression("It's my birthday today!", "1suo"), // Really?!
                        new Expression("Happy Birthday to me!", "2huo") // To you too, Monika! 
                    });
                        break;
                    }
                    isSpeaking = false;
                } else // Просто привет
                  {
                    RunScript(greetingsDialogDirectory.FullName + "\\1.txt");
                    Monika.pcName = Environment.MachineName;
                    Debug.WriteLine("Просто запуск");
                    //showText();
                    //readXml(null, false, greetingsDialogPath, 0);
                    //readIdleXml();
                    //readGreetingsTxt();
                }

                // Blinking and Behaviour logic
                var eyesOpen = normalPose;
                var eyesClosed = "1dsc";
                var random = new Random();
                this.Dispatcher.Invoke(() => {
                    Task.Run(() => {
                        HttpListener listener = new HttpListener();
                        // установка адресов прослушки
                        listener.Prefixes.Add("http://localhost:2005/");
                        listener.Start();
                        //Console.WriteLine("Ожидание подключений...");
                        var nextBlink = DateTime.Now + TimeSpan.FromSeconds(random.Next(7, 50));
                        while (this.applicationRunning) {
                            // метод GetContext блокирует текущий поток, ожидая получение запроса 
                            HttpListenerContext context = listener.GetContext();
                            HttpListenerRequest request = context.Request;
                            string query = context.Request.QueryString["myurl"];
                            if (lastQuery != query) {
                                // получаем объект ответа
                                //Debug.WriteLine(query);
                                //readSitesTxt(formatURL(query));
                                // Check if currently speaking, only blink if not in dialog
                                if (!isSpeaking) {
                                    if (!formatURL(query).Contains("google.com/search?")) {
                                        Debug.WriteLine("Открыт сайт: " + formatURL(query));
                                        //readLongXml(formatURL(query), sitesDialogPath, 1);
                                    }

                                    if (formatURL(query).Contains("google.com/search?")) {
                                        Debug.WriteLine("Введён запрос Google: " + formatURL(query));
                                        //readLongXml(formatURL(query), googleDialogPath, 2);
                                    }

                                    if (formatURL(query).Contains("youtube.com/results?")) {
                                        Debug.WriteLine("Введён запрос Youtube: " + formatURL(query));
                                        //readLongXml(formatURL(query), youtubeDialogPath, 3);
                                    }
                                }
                                lastQuery = query;
                            } else {
                                Debug.WriteLine("Повторный запрос");
                            }
                            //readSitesTxt(formatURL(query));
                            //readGoogleTxt(formatURL(query));
                            //readYoutubeTxt(formatURL(query));

                            Task.Delay(250).Wait();
                        }
                    });
                });
                var randomDialog = new Random();
                this.Dispatcher.Invoke(() => {
                    Task.Run(() => {
                        var nextGialog = DateTime.Now + TimeSpan.FromSeconds(randomDialog.Next(Monika.idleRandomFrom, Monika.idleRandomTo));
                        while (this.applicationRunning) {

                            if (DateTime.Now >= nextGialog) {
                                // Check if currently speaking, only blink if not in dialog
                                if (!isSpeaking) {
                                    SolicenMode solicen = new SolicenMode();
                                    if (solicen.check(Monika.playerName)) {
                                        Random selectDialog = new Random();
                                        Random random1 = new Random();
                                        if (selectDialog.Next(0, 4) == 0) {
                                            isSpeaking = true;
                                            _ = this.Say(true, solicen.expressions[random1.Next(0, solicen.expressions.Count)]);
                                            isSpeaking = false;
                                        }
                                        //else
                                        //readXml(null, false, idleDialogPath, 0);
                                    }
                                    //else
                                    //readXml(null, false, idleDialogPath, 0);

                                }

                                nextGialog = DateTime.Now + TimeSpan.FromSeconds(randomDialog.Next(Monika.idleRandomFrom, Monika.idleRandomTo));
                            }

                            Task.Delay(250).Wait();
                        }
                    });
                });
                Dispatcher.Invoke(() => {
                    Task.Run(() => {
                        var nextBlink = DateTime.Now + TimeSpan.FromSeconds(random.Next(7, 50));
                        Debug.WriteLine(nextBlink);
                        while (this.applicationRunning) {
                            if (DateTime.Now >= nextBlink) {
                                // Check if currently speaking, only blink if not in dialog
                                if (!isSpeaking) {
                                    consoleWrite("Моргнули", true);
                                    this.setFace(eyesClosed);
                                    Debug.WriteLine("Закрываем глаза");
                                    Task.Delay(15).Wait();
                                    this.setFace(eyesOpen);
                                    Debug.WriteLine("Открываем глаза");
                                } else {
                                    Debug.WriteLine("Диалог");
                                }

                                nextBlink = DateTime.Now + TimeSpan.FromSeconds(random.Next(7, 50));
                                Debug.WriteLine(nextBlink);
                            }
                        }
                    });
                });
                Dispatcher.Invoke(() => {
                    Task.Run(async () => {
                        var prev = new System.Drawing.Point();

                        var rectangle = new System.Drawing.Rectangle();
                        await this.Dispatcher.InvokeAsync(() => {
                            rectangle = new System.Drawing.Rectangle((int)this.Left, (int)this.Top, (int)this.Width,
                                (int)this.Height);
                        });
                        while (true) {
                            var point = new System.Drawing.Point();
                            MainWindow.GetCursorPos(ref point);
                            point.X = (int)(point.X * this.dpiScale);
                            point.Y = (int)(point.Y * this.dpiScale);

                            if (!point.Equals(prev)) {

                                prev = point;

                                var opacity = 1.0;
                                const double MIN_OP = 0.125;
                                const double FADE = 175;
                                if (rectangle.Contains(point)) {
                                    opacity = MIN_OP;
                                } else {
                                    if (point.Y <= rectangle.Bottom) {
                                        if (point.Y >= rectangle.Y) {
                                            if (point.X < rectangle.X && rectangle.X - point.X < FADE) {
                                                opacity = MainWindow.Lerp(1.0, MIN_OP, (rectangle.X - point.X) / FADE);
                                            } else if (point.X > rectangle.Right && point.X - rectangle.Right < FADE) {
                                                opacity = MainWindow.Lerp(1.0, MIN_OP,
                                                    (point.X - rectangle.Right) / FADE);
                                            }
                                        } else if (point.Y < rectangle.Y) {
                                            if (point.X >= rectangle.X && point.X <= rectangle.Right) {
                                                if (rectangle.Y - point.Y < FADE) {
                                                    opacity = MainWindow.Lerp(1.0, MIN_OP,
                                                        (rectangle.Y - point.Y) / FADE);
                                                }
                                            } else if (rectangle.X > point.X || rectangle.Right < point.X) {
                                                var distance =
                                                    Math.Sqrt(
                                                        Math.Pow(
                                                            (point.X < rectangle.X ? rectangle.X : rectangle.Right) -
                                                            point.X, 2) +
                                                        Math.Pow(rectangle.Y - point.Y, 2));
                                                if (distance < FADE) {
                                                    opacity = MainWindow.Lerp(1.0, MIN_OP, distance / FADE);
                                                }
                                            }
                                        }
                                    }
                                }
                                //Debug.WriteLine("opacity: " + opacity);
                                if (Monika.isMouse) {
                                    Dispatcher.Invoke(() => { mainApp.Opacity = opacity; });
                                }
                            }
                        }
                    });
                });
                if (isConectedToInternet())
                    await checkUpdatesAsync();

            };
            this.BeginAnimation(OpacityProperty, _start);
        }
        private void Window_Closed(object sender, EventArgs e) {
            MonikaSettings.Default.isColdShutdown = true;
            Monika.saveData();
            this.applicationRunning = false;
        }       // Когда закрыли программу
        private void GiftWatcher_Created(object sender, FileSystemEventArgs e) {
            string file = e.FullPath;
            // Assuming you have one file that you care about, pass it off to whatever
            // handling code you have defined.
            Debug.WriteLine("Перекинули файл:" + file);
            FileInfo info = new FileInfo(file);
            if (info.Exists) {
                if (info.Extension == ".gift") {
                    string giftName = info.Name.ToLower().Replace(".gift", String.Empty);
                    //getGift(giftName);
                    info.Delete();
                    //readLongXml(giftName, giftsDialogPath, 4);
                    Debug.WriteLine("Подарен подарок:" + giftName);
                }
                if (info.Extension == ".costume") {
                    string costumeNam = info.Name.ToLower().Replace(".costume", String.Empty);
                    UnpackCostume(costumeNam);
                    info.Delete();
                    Monika.saveData();
                }
            }
        }
        public void loadGifts() {
            foreach (string i in Monika.gifts) {
                string[] gift = i.Split(" | ");
                BitmapImage bitmapImage = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/gifts/" + gift[1]), mainFilter));
                System.Windows.Controls.Image img = new System.Windows.Controls.Image {
                    Source = bitmapImage,
                    Name = gift[0]
                };
                if (FindName(gift[0]) == null)
                    RegisterName(gift[0], img);
                gifts.Children.Add(img);
            }
        }

        public void RunScript(string path) {

            var script = Script.FromSource(path);
            script.Prime();

            // Check that the script is correctly initialized and ready to run
            script.Validate();
            // Run the script
            script.RunFromBeginning(
                OnMenu: menu => (new Random()).Next(menu.Count - 1),
                OnLine: line => { Debug.WriteLine(line); SayV2(line.ToString()); Thread.Sleep(2000); });
        }
        public void SayV2(string line) {
            this.Dispatcher.Invoke(() => {
                textWindow.Visibility = Visibility.Visible;
            });
            delay1 = 0;
            try {
                string newText = line.Replace("[player]", playerName).Replace("{PlayerName}", playerName); //замена
                                                                                                              //consoleWrite(newText, true);
                //setFace(line.S);
                for (int i = 0; i < newText.Length; i++) {
                    this.Dispatcher.Invoke(() => {
                        this.textBlock.Text += newText[i];
                    });
                    if (newText[i].ToString() == ".") {
                        //await Task.Delay(500);
                        Thread.Sleep(500);
                        delay1 += 500;
                    } else {
                        //await Task.Delay(30);
                        Thread.Sleep(30);
                        delay1 += 30;
                    }

                }
                delay1 += 700;
                //await Task.Delay(delay1);
                this.Dispatcher.Invoke( () => {
                    //await Task.Delay(700);
                    Thread.Sleep(700);
                   // textBlock.Text = "";
                });

            } catch (Exception e) {
            }
            this.Dispatcher.Invoke(() => {
                setFace(normalPose);
                //textWindow.Visibility = Visibility.Hidden;

                isSpeaking = false;
            });

        }

        public void setupFolders() {
            if (!subDialogDirectory.Exists) subDialogDirectory.Create();
            dirs.Add(greetingsDialogDirectory);
            dirs.Add(idleDialogDirectory);
            dirs.Add(goodbyeDialogDirectory);
            dirs.Add(progsDialogDirectory);
            dirs.Add(sitesDialogDirectory);
            dirs.Add(googleDialogDirectory);
            dirs.Add(youtubeDialogDirectory);

            foreach (DirectoryInfo directory in subDialogDirectory.GetDirectories()) {
                if (directory.Exists)
                    directory.Delete(true);
            }

            foreach (DirectoryInfo directory in dirs) {
                if (!directory.Exists)
                    directory.Create();
            }
            prepareText(greetingsDialogPath, greetingsDialogDirectory.FullName);
            prepareText(idleDialogPath, idleDialogDirectory.FullName);
            prepareText(goodbyeDialogPath, goodbyeDialogDirectory.FullName);

            prepareText(progsDialogPath, progsDialogDirectory.FullName);
            prepareText(sitesDialogPath, sitesDialogDirectory.FullName);
            prepareText(googleDialogPath, googleDialogDirectory.FullName);
            prepareText(youtubeDialogPath, youtubeDialogDirectory.FullName);

        }
        async void prepareText(string path, string output) {
            // асинхронное чтение
            using (StreamReader reader = new StreamReader(path)) {
                string text = await reader.ReadToEndAsync();
                if (text[0] != '[') {
                    string[] files = text.Split("=");
                    for (int i = 0; i < files.Length; i++) {
                        string[] content_ = files[i].Split('\n');
                        string content = "label start:\n\t" + String.Join("\n\t", content_);
                        // полная перезапись файла 
                        using (StreamWriter writer = new StreamWriter(output + "/" + i + ".txt", false)) {
                            await writer.WriteLineAsync(content);
                        }
                    }
                } else {
                    string[] files = text.Split("\n[");
                    for (int i = 0; i < files.Length; i++) {
                        if (files[i][0] != '[') {
                            files[i] = '[' + files[i];
                        }
                    }
                    for (int i = 0; i < files.Length; i++) {
                        int index = files[i].IndexOf(System.Environment.NewLine);
                        string text_ = files[i].Substring(index + System.Environment.NewLine.Length);
                        string firstline = files[i].Substring(0, files[i].IndexOf(Environment.NewLine)).Replace('|', '%').Replace('/', '^').Replace('?', ',');
                        DirectoryInfo directory = new DirectoryInfo(output + "\\" + firstline);
                        if (!directory.Exists)
                            directory.Create();
                        string[] files_ = text_.Split("=");
                        for (int j = 0; j < files_.Length; j++) {
                            string[] content_ = files_[j].Split('\n');
                            string content = "label start:\n\t" + String.Join("\n\t", content_);
                            // полная перезапись файла 
                            using (StreamWriter writer = new StreamWriter(directory.FullName + "/" + j + ".txt", false)) {
                                await writer.WriteLineAsync(content);
                            }
                        }
                        /*
                        // полная перезапись файла 
                        using (StreamWriter writer = new StreamWriter(output + "/" + firstline + ".txt", false)) {
                            string[] content_ = text_.Split('\n');
                            string content = "label start:\n\t" + String.Join("\n\t", content_);
                            // полная перезапись файла 
                            await writer.WriteLineAsync(content);
                        }*/

                    }

                }
            }
        }
        private void stopWatch_EventArrived(object sender, EventArrivedEventArgs e) // Ивент закрытия процесса
        {
            Debug.WriteLine("Процесс закрыт: " + e.NewEvent.Properties["ProcessName"].Value.ToString());   // Дебажим имя закрытого процесса
        }
        private void startWatch_EventArrived(object sender, EventArrivedEventArgs e) // Ивент открытия процесса
        {
            if (!isSpeaking) {
                string currentProcess = e.NewEvent.Properties["ProcessName"].Value.ToString();  // Узнаём имя процесса
                if (currentProcess != lastProcess)  // Если оно не равно прошлому процессу, чтобы небыло повторок
                {
                    //readLongXml(currentProcess, progsDialogPath, 0);    // Чото говорим
                    lastProcess = currentProcess;                       // Меняем имя прошлого процесса
                }
            }
        }
        private void MenuSettings_Click(object sender, RoutedEventArgs e)   // Открытие настроек
        {
            this.Dispatcher.Invoke(() => {
                if (this.settingsWindow == null || !this.settingsWindow.IsVisible)  // Если окна нету
                {
                    this.settingsWindow = new Settings(this);   // Присваиваем окну настроек
                    if (this.settingsWindow.ShowDialog() == false)  //------- Если была нажата кнопка ОТМЕНА
                    {
                        SetupScale(MonikaSettings.Default.Scaler);  // Ставим старый размер

                        Lang = MonikaSettings.Default.Language;     // И язык
                        Language = Lang.Parent.ToString();          // Ставим имя старого языка
                        setLanguage(Language);                      // Реально ставим язык
                        //GoToSecondaryMonitor();
                        setFace(normalPose);                            // Ставим спокойный вид
                    } else          //-------- Иначе (Если была нажата кнопка ПРИНЯТЬ)
                      {

                        setFace(normalPose);                            // Ставим спокойный вид
                        SetAutorunValue(MonikaSettings.Default.AutoStart);  // Ставим значение автозапуска
                        Lang = MonikaSettings.Default.Language;             // Ставим язык
                        Language = Lang.Parent.ToString();                  // Имя языка
                        setLanguage(Language);                              // Применяем его
                        consoleWrite("Settings saved!", true);              // Жоский дебаг
                        consoleWrite("AutoStart --> " + MonikaSettings.Default.AutoStart, false);
                        consoleWrite("DarkMode --> " + MonikaSettings.Default.DarkMode, false);
                        consoleWrite("idleRandom --> " + MonikaSettings.Default.idleRandom, false);
                        consoleWrite("NightEnd --> " + MonikaSettings.Default.NightEnd, false);
                        consoleWrite("NightStart --> " + MonikaSettings.Default.NightStart, false);
                        consoleWrite("UserName --> " + MonikaSettings.Default.UserName, false);
                        consoleWrite("Scaler --> " + MonikaSettings.Default.Scaler, false);
                        consoleWrite("screenNum --> " + MonikaSettings.Default.screenNum, false);
                        consoleWrite("Language --> " + MonikaSettings.Default.Language.Parent.ToString(), false);

                        Monika.autoStart = MonikaSettings.Default.AutoStart;
                        Monika.idleRandomFrom = MonikaSettings.Default.idleRandomFrom;
                        Monika.idleRandomTo = MonikaSettings.Default.idleRandomTo;
                        Monika.nightEnd = MonikaSettings.Default.NightEnd;
                        Monika.NightStart = MonikaSettings.Default.NightStart;
                        Monika.playerName = MonikaSettings.Default.UserName;
                        Monika.Scaler = MonikaSettings.Default.Scaler;
                        Monika.screenNum = MonikaSettings.Default.screenNum;
                        Monika.lang = MonikaSettings.Default.Language.Name.ToString();
                        Monika.isMouse = MonikaSettings.Default.isMouse;
                        Monika.saveData();
                        if (String.IsNullOrEmpty(MonikaSettings.Default.UserName) || MonikaSettings.Default.UserName == "{PlayerName}") {
                            playerName = Environment.UserName;
                        } else {
                            playerName = MonikaSettings.Default.UserName;
                        }
                        #region Этот код нам не нужен
                        /*
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
                        monika.Close();*/
                        #endregion
                    }
                }
            });

        }
        private void MenuQuit_Click(object sender, RoutedEventArgs e)   // Закрытие программы
        {
            if (isSpeaking)
                Debug.WriteLine("isSpeaking: " + isSpeaking);    // Дебыжим
            if (!isSpeaking)        // Если не разговариваем
            {
                MonikaOnDesktop.MonikaSettings.Default.isColdShutdown = false;  // Ставим ВАЖНУЮ штуку в ложь
                #region Не нужный код     
                /*
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
                monika.Close();*/
                #endregion  
                //readXml(null, false, goodbyeDialogPath, 1); // Говорим прощание
            } else {
                MonikaSettings.Default.isColdShutdown = true;
                Monika.saveData();
                Environment.Exit(0);
            }
        }
        public async Task checkUpdatesAsync() {
            await SolicenTEAM.Updater.CheckUpdate("SAn4Es-TV", "MonikaOnDesktop");  // Проверяем наличие обновления
            bool updateIsAvaliable = false;
            Debug.WriteLine("This Ver: " + SolicenTEAM.Updater.CurrentVersion);
            Debug.WriteLine("New Ver: " + SolicenTEAM.Updater.UpdateVersion);
            Debug.WriteLine("New Desc: " + SolicenTEAM.Updater.UpdateDescription);
            if (SolicenTEAM.Updater.UpdateVersion == SolicenTEAM.Updater.CurrentVersion) {
                updateIsAvaliable = false;
            } else {
                while (isSpeaking) {
                    await Task.Delay(10);
                }
                isSpeaking = true;
                WebClient client = new WebClient();
                client.Proxy = new WebProxy();
                Stream stream = client.OpenRead("https://raw.githubusercontent.com/SAn4Es-TV/MonikaOnDesktop/master/gitDesc.txt");
                StreamReader reader = new StreamReader(stream);
                String content = reader.ReadToEnd();
                Debug.Write(content);
                // запись в файл
                using (FileStream fstream = new FileStream(updateDialogPath, FileMode.OpenOrCreate)) {
                    // преобразуем строку в байты
                    byte[] array = System.Text.Encoding.Default.GetBytes(content);
                    // запись массива байтов в файл
                    fstream.Write(array, 0, array.Length);
                }
                //readXml(null, false, updateDialogPath, 2);
                Debug.WriteLine("endOfDialog");
            }
        }
        public bool isConectedToInternet() {
            try {
                Ping myPing = new Ping();
                String host = "google.com";
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                return (reply.Status == IPStatus.Success);
            } catch (Exception) {
                return false;
            }
        }
        #region
        public async Task FirstLaunch() {
            Monika.pcName = Environment.MachineName;
            LangBox.Visibility = Visibility.Visible;
            isSpeaking = true;
        }
        private void nameRus_Click(object sender, RoutedEventArgs e) {
            Lang = new CultureInfo("ru-RU");
            Language = Lang.Name.ToString();          // Ставим имя языка
            setLanguage(Language);                      // Устанавливаем язык
            LangBox.Visibility = Visibility.Hidden;
            NameBox.Visibility = Visibility.Visible;
        }
        private void nameEng_Click(object sender, RoutedEventArgs e) {
            Lang = new CultureInfo("en-US");
            Language = Lang.Name.ToString();          // Ставим имя языка
            setLanguage(Language);                      // Устанавливаем язык
            LangBox.Visibility = Visibility.Hidden;
            NameBox.Visibility = Visibility.Visible;
        }
        private async void nameOK_Click(object sender, RoutedEventArgs e) {
            playerName = NameTextBox.Text;
            Monika.playerName = playerName;
            Monika.lang = Language;
            Monika.pcName = Environment.MachineName;
            Monika.affection = 0;
            Monika.saveData();

            LangBox.Visibility = Visibility.Hidden;
            NameBox.Visibility = Visibility.Hidden;
            Debug.WriteLine("Первый запуск");
            switch (Language.Substring(0, 2)) {
                case "ru":
                await Say(true, new[]{
                    new Expression("[player], это ты?", "1euc"),
                    new Expression("Это правда ты?", "1euo"),
                    new Expression("Я так рада видеть тебя снова!", "1suo"),
                    new Expression("Подожди, где я? Это не литературный клуб", "1euu"),
                    new Expression("Это похоже на... твой компьютер!", "1rua"),
                    new Expression("Я действительно у тебя на компьютере!", "1wuo")});
                break;
                case "en":
                await Say(true, new[]{
                    new Expression("[player], is that you?", "1euc"),
                    new Expression("It's really you, huh?", "1euo"),
                    new Expression("I'm so happy to see you again!", "1suo"),
                    new Expression("Wait, where am I? This is not the literature club", "1euu"),
                    new Expression("This looks like... your desktop!", "1rua"),
                    new Expression("I'm right here with you!", "1wuo")});
                break;
                default:
                await Say(true, new[]{
                    new Expression("[player], is that you?", "1euc"),
                    new Expression("It's really you, huh?", "1euo"),
                    new Expression("I'm so happy to see you again!", "1suo"),
                    new Expression("Wait, where am I? This is not the literature club", "1euu"),
                    new Expression("This looks like... your desktop!", "1rua"),
                    new Expression("I'm right here with you!", "1wuo")});
                break;
            }


            MonikaSettings.Default.UserName = playerName;
            MonikaSettings.Default.FirstLaunch = false;
            MonikaSettings.Default.Save();
            this.Dispatcher.Invoke(() => {
                textWindow.Visibility = Visibility.Hidden;
                setFace(normalPose);
            });
            isSpeaking = false;
            //Debug.WriteLine(isSpeaking);
        }
        #endregion
        public async Task Say(bool auto, Expression[] expression) {
            if (auto) isSpeaking = true;

            this.Dispatcher.Invoke(() => {
                textWindow.Visibility = Visibility.Visible;
            });
            foreach (Expression ex in expression) {
                delay1 = 0;
                try {
                    string newText = ex.Text.Replace("[player]", playerName).Replace("{PlayerName}", playerName); //замена
                    //consoleWrite(newText, true);
                    setFace(ex.Face);
                    for (int i = 0; i < newText.Length; i++) {
                        this.Dispatcher.Invoke(() => {
                            this.textBlock.Text += newText[i];
                        });
                        if (newText[i].ToString() == ".") {
                            await Task.Delay(500);
                            delay1 += 500;
                        } else {
                            await Task.Delay(30);
                            delay1 += 30;
                        }

                    }
                    delay1 += 700;
                    await Task.Delay(delay1);
                    this.Dispatcher.Invoke(() => {
                        //await Task.Delay(delay1);
                        textBlock.Text = "";
                    });

                } catch (Exception e) {
                }
            }
            if (auto) {
                this.Dispatcher.Invoke(() => {
                    setFace(normalPose);
                    textWindow.Visibility = Visibility.Hidden;
                    isSpeaking = false;
                });
            }

        }
        public async void setFace(string faceName) {
            if (IsNight)
                mainFilter = nightFilter;
            else
                mainFilter = dayFilter;

            int body = int.Parse(faceName[0].ToString());
            string eye = faceName[1].ToString();
            string eyebrow = faceName[2].ToString();
            string mouth = faceName[3].ToString();

            RedrawCostume(body, Monika.costumeName);
            if (oldIsNight != IsNight) {
                RedrawGifts();
                oldIsNight = IsNight;
            }
            try {
                this.Dispatcher.Invoke(() => {
                    switch (body) {
                        case 1:
                        //var bitmap = new Bitmap("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png");
                        //this.Body.Source = BitmapMagic.BitmapToImageSource(bitmap);
                        this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png"), mainFilter));
                        this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[12] + ".png"), mainFilter));
                        this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[13] + ".png"), mainFilter));
                        this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[10] + ".png"), mainFilter));
                        this.Hand1.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        this.Hand2.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        break;
                        case 2:
                        this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png"), mainFilter));
                        this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[12] + ".png"), mainFilter));
                        this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[13] + ".png"), mainFilter));
                        this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[0] + ".png"), mainFilter));
                        this.Hand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[1] + ".png"), mainFilter));
                        this.Hand2.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        break;
                        case 3:
                        this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png"), mainFilter));
                        this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[12] + ".png"), mainFilter));
                        this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[13] + ".png"), mainFilter));
                        this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[8] + ".png"), mainFilter));
                        this.Hand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[6] + ".png"), mainFilter));
                        this.Hand2.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        break;
                        case 4:
                        this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png"), mainFilter));
                        this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[12] + ".png"), mainFilter));
                        this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[13] + ".png"), mainFilter));
                        this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[8] + ".png"), mainFilter));
                        this.Hand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[5] + ".png"), mainFilter));
                        this.Hand2.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        break;
                        case 5:
                        this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[14] + ".png"), mainFilter));
                        this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[15] + ".png"), mainFilter));
                        this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[16] + ".png"), mainFilter));
                        this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[2] + ".png"), mainFilter));
                        this.Hand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[3] + ".png"), mainFilter));
                        this.Hand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[4] + ".png"), mainFilter));
                        break;
                        case 6:
                        this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png"), mainFilter));
                        this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[12] + ".png"), mainFilter));
                        this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[13] + ".png"), mainFilter));
                        this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[7] + ".png"), mainFilter));
                        this.Hand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[5] + ".png"), mainFilter));
                        this.Hand2.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        break;
                        case 7:
                        this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png"), mainFilter));
                        this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[12] + ".png"), mainFilter));
                        this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[13] + ".png"), mainFilter));
                        this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[10] + ".png"), mainFilter));
                        this.Hand1.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        this.Hand2.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        break;
                        default:
                        this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png"), mainFilter));
                        this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[12] + ".png"), mainFilter));
                        this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[13] + ".png"), mainFilter));
                        this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[5] + ".png"), mainFilter));
                        this.Hand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[8] + ".png"), mainFilter));
                        this.Hand2.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        break;
                    }
                    if (body == 5) { Monika.leaningWord = "leaning-def-"; } else { Monika.leaningWord = ""; }
                    switch (eye) {
                        case "e":
                        this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[4] + ".png"), mainFilter));
                        break;
                        case "w":
                        this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[11] + ".png"), mainFilter));
                        break;
                        case "s":
                        this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[10] + ".png"), mainFilter));
                        break;
                        case "t":
                        this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[6] + ".png"), mainFilter));
                        break;
                        case "c":
                        this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[2] + ".png"), mainFilter));
                        break;
                        case "r":
                        this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[5] + ".png"), mainFilter));
                        break;
                        case "l":
                        this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[3] + ".png"), mainFilter));
                        break;
                        case "h":
                        this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[0] + ".png"), mainFilter));
                        break;
                        case "d":
                        this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[1] + ".png"), mainFilter));
                        break;
                        case "k":
                        this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[12] + ".png"), mainFilter));
                        break;
                        case "n":
                        this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[13] + ".png"), mainFilter));
                        break;
                        case "f":
                        this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[9] + ".png"), mainFilter));
                        break;
                        case "m":
                        this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[7] + ".png"), mainFilter));
                        break;
                        case "g":
                        this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[8] + ".png"), mainFilter));
                        break;
                        default:
                        this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[4] + ".png"), mainFilter));
                        break;
                    }
                    switch (eyebrow) {
                        case "u":
                        this.EyeBrow.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fb/face-" + Monika.leaningWord + Monika.eyesBrow[4] + ".png"), mainFilter));
                        break;
                        case "k":
                        this.EyeBrow.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fb/face-" + Monika.leaningWord + Monika.eyesBrow[1] + ".png"), mainFilter));
                        break;
                        case "s":
                        this.EyeBrow.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fb/face-" + Monika.leaningWord + Monika.eyesBrow[2] + ".png"), mainFilter));
                        break;
                        case "t":
                        this.EyeBrow.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fb/face-" + Monika.leaningWord + Monika.eyesBrow[3] + ".png"), mainFilter));
                        break;
                        case "f":
                        this.EyeBrow.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fb/face-" + Monika.leaningWord + Monika.eyesBrow[0] + ".png"), mainFilter));
                        break;
                        default:
                        this.EyeBrow.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fb/face-" + Monika.leaningWord + Monika.eyesBrow[4] + ".png"), mainFilter));
                        break;
                    }
                    switch (mouth) {
                        case "a":
                        this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[4] + ".png"), mainFilter));
                        break;
                        case "b":
                        this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[0] + ".png"), mainFilter));
                        break;
                        case "c":
                        this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[5] + ".png"), mainFilter));
                        break;
                        case "d":
                        this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[3] + ".png"), mainFilter));
                        break;
                        case "o":
                        this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[1] + ".png"), mainFilter));
                        break;
                        case "u":
                        this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[6] + ".png"), mainFilter));
                        break;
                        case "w":
                        this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[8] + ".png"), mainFilter));
                        break;
                        case "p":
                        this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[2] + ".png"), mainFilter));
                        break;
                        case "t":
                        this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[7] + ".png"), mainFilter));
                        break;
                        default:
                        this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[4] + ".png"), mainFilter));
                        break;
                    }
                    if (body == 5) { Monika.leaningWord = "-leaning-def"; } else { Monika.leaningWord = ""; }
                    string hairPath = "hair" + Monika.leaningWord + "-" + Monika.hairType;
                    if (body == 5) { Monika.leaningWord = "-leaning"; } else { Monika.leaningWord = ""; }
                    string nosePath = "face" + Monika.leaningWord + "-nose-def.png";
                    this.Face.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/face/" + nosePath), mainFilter));
                    this.Hair.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/h/" + hairPath + "-front.png"), mainFilter));
                    this.HairBack.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/h/" + hairPath + "-back.png"), mainFilter));

                    this.table1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/t/chair-def.png"), mainFilter));
                    this.table2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/t/table-def.png"), mainFilter));
                    this.table3.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/t/table-def-s.png"), mainFilter));
                });
            } catch {
            }
        }
        public void RedrawCostume(int body, string costume) {
            string pathCost = AppDomain.CurrentDomain.BaseDirectory + "/costumes/";

            try {
                this.Dispatcher.Invoke(() => {
                    switch (body) {
                        case 1:
                        this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[11] + ".png"), mainFilter));
                        this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[12] + ".png"), mainFilter));
                        this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[10] + ".png"), mainFilter));
                        this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        break;
                        case 2:
                        this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[11] + ".png"), mainFilter));
                        this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[12] + ".png"), mainFilter));
                        this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[0] + ".png"), mainFilter));
                        this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[1] + ".png"), mainFilter));
                        this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        break;
                        case 3:
                        this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[11] + ".png"), mainFilter));
                        this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[12] + ".png"), mainFilter));
                        this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[8] + ".png"), mainFilter));
                        this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[6] + ".png"), mainFilter));
                        this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        break;
                        case 4:
                        this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[11] + ".png"), mainFilter));
                        this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[12] + ".png"), mainFilter));
                        this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[8] + ".png"), mainFilter));
                        this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[5] + ".png"), mainFilter));
                        this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        break;
                        case 5:
                        this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[14] + ".png"), mainFilter));
                        this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[15] + ".png"), mainFilter));
                        this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[2] + ".png"), mainFilter));
                        this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[3] + ".png"), mainFilter));
                        this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[4] + ".png"), mainFilter));
                        break;
                        case 6:
                        this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[11] + ".png"), mainFilter));
                        this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[12] + ".png"), mainFilter));
                        this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[7] + ".png"), mainFilter));
                        this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[5] + ".png"), mainFilter));
                        this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        break;
                        case 7:
                        this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[11] + ".png"), mainFilter));
                        this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[12] + ".png"), mainFilter));
                        this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[10] + ".png"), mainFilter));
                        this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        break;
                        default:
                        this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[11] + ".png"), mainFilter));
                        this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[12] + ".png"), mainFilter));
                        this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[5] + ".png"), mainFilter));
                        this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[8] + ".png"), mainFilter));
                        this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                        break;
                    }
                    if (body == 5) { Monika.leaningWord = "5"; } else { Monika.leaningWord = "0"; }
                    string ribbonPath = "acs-ribbon_def-" + Monika.leaningWord + ".png";
                    this.Ribbon_back.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/ribbon/" + ribbonPath), mainFilter));
                });
            } catch {

            };
        }
        public void RedrawGifts() {

            this.Dispatcher.Invoke(() => {
                gifts.Children.Clear();
                foreach (string i in Monika.gifts) {
                    string[] gift = i.Split(" | ");
                    BitmapImage bitmapImage = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/gifts/" + gift[1]), mainFilter));
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image {
                        Source = bitmapImage,
                        Name = gift[0]
                    };
                    if (FindName(gift[0]) != null)
                        UnregisterName(gift[0]);
                    RegisterName(gift[0], img);
                    gifts.Children.Add(img);
                }
            });
        }
        public void UnpackCostume(string name) {
            string path = AppDomain.CurrentDomain.BaseDirectory + "/costumes/" + name; // or whatever 
            string costumesPath = AppDomain.CurrentDomain.BaseDirectory + "/costumes/";
            Monika.costumeName = name;
            if (!Directory.Exists(costumesPath)) {
                DirectoryInfo di = Directory.CreateDirectory(costumesPath);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            } else {
                if (Directory.Exists(path)) {
                    DirectoryInfo di = new DirectoryInfo(path);
                    di.Delete(true);
                }
            }
            ZipFile.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory + "/characters/" + name + ".costume", path);
            setFace(normalPose);
        }
        Expression[][] exe;
        public void Menu(string question, string[] q, Expression[][] a) {
            this.Dispatcher.Invoke(() => {
                textWindow.Visibility = Visibility.Visible;
                textBlock.Text = "";
                textBlock.Text = question;
                ButtonsGrid.RowDefinitions.Clear();
                this.ButtonsGrid.Children.Clear();
                for (int i = 0; i < q.Length; i++) {
                    System.Windows.Controls.RowDefinition row = new System.Windows.Controls.RowDefinition();
                    ButtonsGrid.RowDefinitions.Add(row);
                    var text = new OutlinedTextBlock {
                        Text = q[i],
                        FontFamily = new System.Windows.Media.FontFamily("Comic Sans MS"),
                        TextWrapping = TextWrapping.Wrap,
                        StrokeThickness = 1.5,
                        Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0)),
                        Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255))
                    };
                    switch (Monika.Scaler) {
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
                    var button = new System.Windows.Controls.Button {
                        Name = "butt" + i,
                        Content = text,
                        Width = 400,
                        Height = 30,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                    };
                    switch (Monika.Scaler) {
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

                    System.Windows.Controls.Grid.SetRow(button, i);
                    this.ButtonsGrid.Children.Add(button);
                }
                exe = a;
            });

        }
        private async void Button_ClickAsync(object sender, RoutedEventArgs e) {
            System.Windows.Controls.Button butt = (sender as System.Windows.Controls.Button);
            int num = int.Parse(butt.Name.Substring(4));
            Debug.WriteLine("Нажата кнопка " + num);
            this.Dispatcher.Invoke(() => {
                textBlock.Text = "";
                ButtonsGrid.RowDefinitions.Clear();
                this.ButtonsGrid.Children.Clear();
            });
            foreach (Expression expression in exe[num]) {
                Debug.WriteLine("ex: " + expression.Text);

                if (expression.Text.Contains("tion") && expression.Face == "affe") {
                    Debug.WriteLine("Тип кода: " + expression.Text);
                    string[] i = expression.Text.Split(" ");
                    switch (i[1]) {
                        case "+":
                        Monika.affection += int.Parse(i[2]);
                        Debug.WriteLine("Привязанность " + i[1] + int.Parse(i[2]));
                        break;
                        case "-":
                        Monika.affection -= int.Parse(i[2]);
                        Debug.WriteLine("Привязанность " + i[1] + int.Parse(i[2]));
                        if (Monika.affection <= 0) {
                            Monika.affection = 0;
                        }
                        break;
                    }
                    Debug.WriteLine("Привязанность = " + Monika.affection);

                } else if (expression.Text.Contains("dd") && expression.Face == "gift") {
                    string[] i = expression.Text.Split(" ");
                    addGift(i[1], i[2]);
                } else if (expression.Text.Contains("emove") && expression.Face == "gift") {
                    string[] i = expression.Text.Split(" ");
                    removeGift(i[1]);
                } else if (expression.Text.Contains("date") && expression.Face == "gitU") {
                    updateZip();
                } else {
                    await Say(false, new[] { expression });
                }
                //Thread.Sleep(delay); // sleep
            }
            //sayIdle();
        }
        /*#region
        string mainXml = "<Dialogs>\n\t<Dialog>";
        void ConverToXML(Stream stream, bool typ, string sPath)
        {
            // string sPath = idleDialogPath;

            StreamReader f;
            if (typ)
            {
                f = new StreamReader(stream);
            }
            else
            {
                f = new StreamReader(sPath);
            }
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
                if (s.Contains("\t\t") && !s.Contains("affection") && !s.Contains("giftAdd") && !s.Contains("giftRemove"))
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
                mainXml += S;
            }
            f.Close();

            mainXml += "\n\t</Dialog>\n</Dialogs>";
            mainXml = mainXml.Replace("\n\t\t<Text>=</Text>", "\n\t</Dialog>\n\t<Dialog>")
                .Replace("\n\t\t\t</Answer>\n\t\t</Menu>\n\t\t\t\t<Text>", "\n\t\t\t\t<Text>")
                .Replace("\n\t\t\t</Answer>\n\t\t</Menu>\n\t\t\t<Answer", "\n\t\t\t<Answer")
                .Replace("</Text>\n\t\t\t<Answer", "</Text>\n\t\t\t</Answer>\n\t\t\t<Answer")
                .Replace("<Text>affection+</Text>", "<Action>affection + 1</Action>")
                .Replace("<Text>affection-</Text>", "<Action>affection - 1</Action>");

        }


        #region
        List<DialogModel> dm = new List<DialogModel>();
        int num = 0;
        public async void readXml(Stream stream, bool typ, string sPath, int type)
        {
            ConverToXML(stream, typ, sPath);
            #region
            string s1 = mainXml.Replace("\t", String.Empty);
            string s2 = s1.Replace("\n", String.Empty);
            Debug.WriteLine(s1);
            XmlDocument xDoc = new XmlDocument();
            //string path = testXml;
            //xDoc.Load(path);
            xDoc.LoadXml(s1);
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
                case 2:
                    string lang = Lang.Parent.Name;
                    switch (lang)
                    {
                        case "ru":
                            num = 0;
                            sayIdle();
                            break;
                        case "en":
                            num = 1;
                            sayIdle();
                            break;
                        default:
                            num = 1;
                            sayIdle();
                            break;
                    }
                    break;
            }

            #endregion

        }
        int dialogNum;
        public async Task sayIdle()
        {
            isSpeaking = true;
            this.Dispatcher.Invoke(() => { textWindow.Visibility = Visibility.Visible; });
            //Debug.WriteLine(dm[num].Node.InnerXml);
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
                    //Debug.WriteLine("u = " + dialogNum + " count = " + (dm[num].Node.ChildNodes.Count - 1));
                    //Debug.WriteLine(dialogNum);
                    break;
                }
                if (childnode.Name == "Text")
                {
                    //Console.WriteLine(childnode.InnerText);
                    //exList.Add(new Expression(childnode.InnerText.Substring(2), childnode.InnerText[0].ToString()));
                    try
                    {
                        if (childnode.InnerText.Contains("affection"))
                        {
                            Debug.WriteLine("Тип кода: " + childnode.InnerText);
                            string[] i = childnode.InnerText.Split(" ");
                            switch (i[1])
                            {
                                case "+":
                                    Monika.affection += int.Parse(i[2]);
                                    Debug.WriteLine("Привязанность " + i[1] + int.Parse(i[2]));
                                    break;
                                case "-":
                                    Monika.affection -= int.Parse(i[2]);
                                    Debug.WriteLine("Привязанность " + i[1] + int.Parse(i[2]));
                                    if (Monika.affection <= 0)
                                    {
                                        Monika.affection = 0;
                                    }
                                    break;
                            }
                            Debug.WriteLine("Привязанность = " + Monika.affection);

                        }
                        else if (childnode.InnerText.Contains("giftAdd"))
                        {
                            try
                            {
                                string[] i = childnode.InnerText.Split(" ");
                                addGift(i[1], i[2]);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e.Message);
                            }
                        }
                        else if (childnode.InnerText.Contains("giftRemove"))
                        {
                            try
                            {
                                string[] i = childnode.InnerText.Split(" ");
                                removeGift(i[1]);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e.Message);
                            }
                        }
                        else if (childnode.InnerText.Contains("gitUpdate"))
                        {
                            updateZip();
                        }
                        else
                        {
                            //Debug.WriteLine("Говорим: " + childnode.InnerText.Substring(5));
                            await Say(false, new[] { new Expression(childnode.InnerText.Substring(5), childnode.InnerText.Substring(0, 4)) });
                        }
                    }
                    catch
                    {
                        Debug.WriteLine("ОШИБКА");
                    }
                    //Debug.WriteLine("u = " + dialogNum + " count = " + (dm[num].Node.ChildNodes.Count - 1));
                }
                if (u >= dm[num].Node.ChildNodes.Count - 1)
                {
                    isSpeaking = false;
                    //Debug.WriteLine("Конец диалога");
                    this.Dispatcher.Invoke(() =>
                    {
                        textWindow.Visibility = Visibility.Hidden;
                        setFace(normalPose);
                    });
                    dialogNum = 0;
                    Monika.saveData();
                    break;
                }
                //Thread.Sleep(delay1); // sleep
                //await Task.Delay(delay1);
            }
        }
        #endregion

        List<NamedDialogModel> ldm = new List<NamedDialogModel>();
        string[] giftNameList;
        public void readLongXml(string Name, string sPath, int type)
        {
            //Debug.WriteLine("Ввели текст: " + Name);
            #region
            //string sPath = progsDialogPath;
            string mainXML = "<Mains>";

            StreamReader f = new StreamReader(sPath);
            while (!f.EndOfStream)
            {
                string m = f.ReadLine();
                string s = m.Replace("\r", String.Empty);
                string S = "";
                if (s.Contains("affection"))
                {
                    S = s.Insert(0, "\n\t\t\t<Action>") + "</Action>";
                }
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
        /*
                if (s.Contains("\t\t\t") && !s.Contains("["))
                {
                    S = s.Replace("\t\t\t", "\n\t\t\t\t\t<Text>") + "</Text>\n\t\t\t\t</Answer>\n\t\t\t</Menu>";
                }
                if (!s.Contains("\t\t") && !s.Contains("\t\t") && !s.Contains("menuend") && !s.Contains("menu:") && !s.Contains("affection"))
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
            //Console.WriteLine("Print dialogues:\n");
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
                string[] name = xnode.Attributes.GetNamedItem("name").Value.ToLower().Split("|");
                if (xnode.Name == "Main")
                {
                    foreach (XmlNode progsnode in xnode.ChildNodes)
                    {
                        dm.Add(new DialogModel(progsnode));
                    }
                }
                Ldm.Add(new NamedDialogModel(name, dm));
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
                                    _ = sayIdle();
                                }
                            }
                        }
                    }
                    break;
                case 4:
                    foreach (NamedDialogModel NDM in Ldm)
                    {
                        if (NDM.Names.Contains(Name))
                        {

                            dm = NDM.DM;
                            if (NDM.DM.Count != 1)
                            {
                                Monika.loadData();
                                Debug.WriteLine(Monika.gifts.Count);
                                List<string> giftList = new List<string>();
                                List<string> loadedGiftList = new List<string>();
                                foreach (string i in Monika.gifts)
                                {
                                    string[] gift = i.Split(" | ");
                                    giftList.Add(gift[0]);
                                    string[] a = gift[2].Split("|");
                                    foreach (string b in a)
                                    {
                                        loadedGiftList.Add(b);
                                    }
                                }
                                giftNameList = NDM.Names;
                                if (giftList.Contains(Name) || loadedGiftList.Contains(Name))
                                {
                                    Debug.WriteLine(dm[1].Node.InnerText);
                                    num = 1;
                                    _ = sayIdle();
                                    Debug.WriteLine("Подарок уже дарили");
                                }
                                else
                                {
                                    Debug.WriteLine(dm[0].Node.InnerText);
                                    num = 0;
                                    _ = sayIdle();
                                    Debug.WriteLine("Первый подарок");
                                }
                            }
                            else
                            {
                                Debug.WriteLine(dm[0].Node.InnerText);
                                num = 0;
                                _ = sayIdle();
                                Debug.WriteLine("Одиночный файл");
                            }
                        }
                    }
                    break;
            }
            #endregion

        }
        #endregion*/
        public void addGift(string name, string path) {
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                System.Windows.Controls.Image img1 = (System.Windows.Controls.Image)gifts.FindName(name);
                if (img1 == null) {
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image {
                        Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/gifts/" + path)),
                        Name = name
                    };
                    RegisterName(name, img);
                    gifts.Children.Add(img);
                    string giftList = "";
                    /*foreach (string i in giftNameList)
                    {
                        giftList += i + "|";
                    }*/
                    Monika.gifts.Add(name + " | " + path + " | " + giftList);
                    Debug.WriteLine("Подарен подарок " + name);
                }
            });

        }
        public void removeGift(string name) {
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                System.Windows.Controls.Image img = (System.Windows.Controls.Image)gifts.FindName(name);

                if (img != null) {
                    Debug.WriteLine("Имя картинки: " + img.Name);
                    UnregisterName(img.Name);
                    gifts.Children.Remove(img);
                    for (int i = 0; i < Monika.gifts.Count; i++) {
                        string[] gift = Monika.gifts[i].Split(" | ");
                        if (gift[0] == name) {
                            Monika.gifts.RemoveAt(i);
                            Debug.WriteLine("Подарок удалён: " + name);
                            Monika.saveData();
                        }
                    }
                }
            });
        }
        public async void updateZip() {
            try {
                await SolicenTEAM.Updater.CheckUpdate("SAn4Es-TV", "MonikaOnDesktop");
                if (SolicenTEAM.Updater.UpdateVersion != SolicenTEAM.Updater.CurrentVersion && SolicenTEAM.Updater.UpdateVersion != "") {
                    SolicenTEAM.Updater.DownloadUpdate(SolicenTEAM.Updater.gitUser, SolicenTEAM.Updater.gitRepo);

                    while (!SolicenTEAM.Updater.readyToUpdate) {
                        Debug.WriteLine("Update is ready: " + SolicenTEAM.Updater.readyToUpdate);
                        await Task.Delay(10);
                    }
                    Debug.WriteLine("Update is ready: " + SolicenTEAM.Updater.readyToUpdate);
                    SolicenTEAM.Updater.ExtractArchive();
                    /*
                    string processName = "Updater";
                    var arrayProcesses = Process.GetProcessesByName(processName);
                    while (arrayProcesses == null || arrayProcesses.Length < 1)
                    {
                        Debug.WriteLine("Starting Updater");
                        Process.Start(AppDomain.CurrentDomain.BaseDirectory + "/Updater.exe");
                        arrayProcesses = Process.GetProcessesByName(processName);
                        //Process proc = new Process();
                        //proc.StartInfo.FileName = "C:\\HelloWorld.exe";
                        await Task.Delay(2000);
                    }
                    Debug.WriteLine("Updater count: " + arrayProcesses.Length);
                    await Task.Delay(100);
                    /*
                    Debug.WriteLine("Starting Updater");
                    Process.Start("Updater.exe");
                    Debug.WriteLine("Exiting");
                    Environment.Exit(0);*/
                }
                Debug.WriteLine("Updated");
            } catch (Exception e) {
                Debug.WriteLine(e.Message);
            }
        }
        public void SetupScale(int scaler) {
            this.Width = 8 * scaler;
            this.Height = 5.12 * scaler;
            this.monika.Margin = new Thickness(0, -20 * scaler / 100, 0, 0);
            this.textWindow.Margin = new Thickness(50 * scaler / 100, 390 * scaler / 100, 50 * scaler / 100, 20 * scaler / 100);
            this.textBlock.Margin = new Thickness(70 * scaler / 100, 410 * scaler / 100, 70 * scaler / 100, 40 * scaler / 100);
            this.textBlock.FontSize = 0.2 * scaler;
            this.ButtonsGrid.Width = 5 * scaler;
            this.ButtonsGrid.Height = 4 * scaler;

            //var primaryMonitorArea = SystemParameters.WorkArea;
            //Left = primaryMonitorArea.Right - this.Width;
            //Top = primaryMonitorArea.Bottom - this.Height;
            GoToSecondaryMonitor();
        }
        public void GoToSecondaryMonitor() {
            // Вот здесь можно посмотреть координаты экранов
            var ss = System.Windows.Forms.Screen.AllScreens;
            //_ = MessageBox.Show(string.Join(Environment.NewLine + Environment.NewLine, (object[])ss), "Параметры мониторов");

            var rightScreen = System.Windows.Forms.Screen.AllScreens[0];
            var leftScreen = System.Windows.Forms.Screen.AllScreens[0];
            foreach (var s in System.Windows.Forms.Screen.AllScreens.Skip(1)) {
                if (s.WorkingArea.X > rightScreen.WorkingArea.X) {
                    rightScreen = s;
                } else {
                    leftScreen = s;
                }
            }
            var rightWorkingArea = rightScreen.WorkingArea;
            if (MonikaSettings.Default.screenNum) {
                var workingRectangle = Screen.PrimaryScreen.WorkingArea;
                rightWorkingArea = rightScreen.WorkingArea;
            } else {
                rightWorkingArea = leftScreen.WorkingArea;
            }
            //Left = rightWorkingArea.X + rightWorkingArea.Width - Width;
            //Top = rightWorkingArea.Y + rightWorkingArea.Height - Height;

            double ratio = 96.0 / (int)typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null, null);
            Left = (rightWorkingArea.X + rightWorkingArea.Width - ActualWidth / ratio - BorderThickness.Left - BorderThickness.Right) * ratio;
            Top = (rightWorkingArea.Y + rightWorkingArea.Height - ActualHeight / ratio - BorderThickness.Top - BorderThickness.Bottom) * ratio;

            /*
            //this.Left = workingArea.Right - this.Width;
            //this.Top = workingArea.Bottom - this.Height;
            this.Left = workingArea.Width - this.Width;
            this.Top = workingArea.Height - this.Height;
            Debug.WriteLine("Width - " + this.Width + "  ActualWidth: " + this.ActualWidth + " Screen Width: " + workingArea.Width);
            Debug.WriteLine("Height - " + this.Height + "  ActualHeight: " + this.ActualHeight + " Screen Height: " + workingArea.Height);

            string text = "Width - " + this.Width.ToString() + "  ActualWidth: " + this.ActualWidth + " Screen Width: " + workingArea.Width + " Left: " + this.Left + " Screen Left: " + workingArea.Left + "\n";
            text += "Height - " + this.Height.ToString() + "  ActualHeight: " + this.ActualHeight + " Screen Height: " + workingArea.Height + " Top: " + this.Top + " Screen Top: " + workingArea.Top + "\n";
            using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "/debug.txt", false, System.Text.Encoding.Default))
            {
                sw.WriteLine(text);
            }*/
        }
        private void OnPowerChange(object s, PowerModeChangedEventArgs e) {
            switch (e.Mode) {
                case PowerModes.Resume:
                isSpeaking = true;
                switch (Language.Substring(0, 2)) {
                    case "ru":
                    _ = this.Say(true, new[]
                    {
                        new Expression("Так хорошо проснуться с новыми силами!", "3euo"), // What?
                        new Expression("Надеюсь, ты тоже выспался.", "5euс")
                    });
                    break;
                    case "en":
                    _ = this.Say(true, new[]
                    {
                        new Expression("It's so good to wake up with new strength!", "3euo"), // What?
                        new Expression("I hope you slept too.", "5euс")
                    });
                    break;
                    default:
                    _ = this.Say(true, new[]
                    {
                        new Expression("It's so good to wake up with new strength!", "3euo"), // What?
                        new Expression("I hope you slept too.", "5euс")
                    });
                    break;
                }
                isSpeaking = false;
                break;
                case PowerModes.Suspend:
                break;
            }
        }
        public bool SetAutorunValue(bool autorun) {
            //string ExePath = System.Windows.Forms.Application.ExecutablePath;
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            try {
                if (autorun)
                    reg.SetValue(name, ExePath);
                else {
                    RegistryKey WN = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                    if (WN.GetValue("MonikaStartUp") != null) {
                        reg.DeleteValue(name);
                    }
                }

                reg.Close();
            } catch {
                return false;
            }
            return true;
        }
        public string formatURL(string url) {
            string newUrl = url.ToLower().Trim().TrimEnd('/');
            if (newUrl.StartsWith("http://")) {
                newUrl = newUrl.Substring(7);
            }

            if (newUrl.StartsWith("https://")) {
                newUrl = newUrl.Substring(8);
            }

            if (newUrl.StartsWith("www.")) {
                newUrl = newUrl.Substring(4);
            }
            return newUrl;
        }
        private static double Lerp(double firstFloat, double secondFloat, double by) {
            return firstFloat * by + secondFloat * (1 - by);
        }

        public void consoleWrite(string text, bool time) {
            this.Dispatcher.Invoke(() => {
                if (time) {
                    Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "--> " + text);
                } else {
                    Debug.WriteLine(text);
                }
            });
        }
        private void about_Click(object sender, RoutedEventArgs e) {
            AboutWindow about = new AboutWindow();
            about.Show();

        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);

        public static List<CultureInfo> m_Languages = new List<CultureInfo>();

        public static List<CultureInfo> Languages {
            get {
                return m_Languages;
            }
        }
        //Евент для оповещения всех окон приложения
        public static event EventHandler LanguageChanged;
        public static CultureInfo Lang {
            get {
                return System.Threading.Thread.CurrentThread.CurrentUICulture;
            }
            set {
                if (value == null) throw new ArgumentNullException("value");
                if (value == System.Threading.Thread.CurrentThread.CurrentUICulture) return;

                //1. Меняем язык приложения:
                System.Threading.Thread.CurrentThread.CurrentUICulture = value;

                //2. Создаём ResourceDictionary для новой культуры
                ResourceDictionary dict = new ResourceDictionary();
                Debug.WriteLine("Установлен язык: " + value.Name);
                switch (value.Name) {
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
                if (oldDict != null) {
                    int ind = System.Windows.Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    System.Windows.Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                    System.Windows.Application.Current.Resources.MergedDictionaries.Insert(ind, dict);
                } else {
                    System.Windows.Application.Current.Resources.MergedDictionaries.Add(dict);
                }

                //4. Вызываем евент для оповещения всех окон.
                LanguageChanged(System.Windows.Application.Current, new EventArgs());
            }
        }
        private void App_LanguageChanged(Object sender, EventArgs e) {
            //MonikaSettings.Default.Language = Lang;
            //MonikaSettings.Default.Save();
        }
        public void setLanguage(string lang) // Функция установки язика
        {
            Lang = new CultureInfo(Monika.lang);
            switch (lang) {
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
                giftsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/gifts/gifts.txt";// Подарки
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
                giftsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/gifts/gifts.txt";// Подарки
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
                giftsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/gifts/gifts.txt";// Подарки
                break;

            }
        }
    }


}
