﻿using System;
using System.Collections;
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
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

using CharacterAi.Client;
using CharacterAi.Client.Models;
using CharacterAi.Client.Models.Common;

using Microsoft.Win32;

using SolicenTEAM;

using VGPrompter;

using static System.Net.Mime.MediaTypeNames;

namespace MonikaOnDesktop {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        #region Обновлятор Солицена
        static SolicenTEAM.Updater.UpdateConfig uConfig
            = new SolicenTEAM.Updater.UpdateConfig {
                gitUser = "SAn4Es-TV",
                gitRepo = "MonikaOnDesktop",
                IgnoreFiles = "characters/monika.chr",
                ExeFileName = "MonikaOnDesktop"
            };
        SolicenTEAM.Updater Updater = new SolicenTEAM.Updater(uConfig);
        #endregion
        #region Сбор мусора
        [DllImport("kernel32.dll")]
        static extern bool SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(object hObject);
        #endregion
        #region Всякое

        public DoubleAnimation _start;  // Анимация запуска
        public DoubleAnimation _quit;   // Анимация выхода

        String playerName;              // Имя игрока

        public int delay1 = 0;          // Задержка
        #endregion
        #region Пути
        protected readonly static string baseDir = AppDomain.CurrentDomain.BaseDirectory;   // Папка запуска
        protected readonly static string baseGiftsPath = $"{baseDir}\\gifts\\";             // Путь к подаркам
        protected readonly static string subFolderPath = $"{baseDir}\\Dialogs\\Sub\\";      // Путь к диалогам

        protected readonly string assetsPath = "pack://application:,,,/assets";             // Папка ассетов
        protected readonly string ExePath = baseDir + "MonikaOnDesktop.exe";                // Путь к ЕХЕ
        private string greetingsDialogPath = baseDir + "/Dialogs/greetings.txt";            // Приветствия
        private string idleDialogPath = baseDir + "/Dialogs/idle.txt";                      // Случайные диалоги
        private string progsDialogPath = baseDir + "/Dialogs/progs.txt";                    // Реакции на программы
        private string sitesDialogPath = baseDir + "/Dialogs/sites.txt";                    // Реакции на сайты
        private string googleDialogPath = baseDir + "/Dialogs/google.txt";                  // Реакции на запросы в Гугуля
        private string youtubeDialogPath = baseDir + "/Dialogs/youtube.txt";                // Реакции на запросы в Ютабе
        private string goodbyeDialogPath = baseDir + "/Dialogs/goodbye.txt";                // Прощания
        private string giftsDialogPath = baseDir + "/Dialogs/ru/gifts/gifts.txt";           // Подарки
        private string updateDialogPath = baseDir + "/Dialogs/upd.txt";                     // Обновления


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
        //public bool isSpeaking = false;              // Идёт ли разговорчик
        public bool _speak = false;
        public bool isSpeaking {
            get {
                return _speak;
            }
            set {
                _speak = value;
            }
        }
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

        CharacterModel Monika = new CharacterModel($"{baseDir}\\characters\\monika.chr", $"{baseDir}characters\\"); // Персонаж Моники
        private Settings settingsWindow;            // Окно настроек

        private NotifyIcon NI = new NotifyIcon();

        public float[] nightFilter = { 0.6861919617166911f, 0.387275212f, 0.27662517f };
        public float[] dayFilter = { 1f, 1f, 1f };
        public float[] mainFilter = { 1f, 1f, 1f };

        public bool mouse = true;
        #endregion
        #region CharacterAI
        string AIpath = @"script.txt";
        string characterId = "aywKj4vjL0-X2QeZj2VFcCqPlZ4HmzH0FNlebJKcjTk";
        CharacterAi.Client.CharacterAiClient client = new CharacterAiClient();
        CaiCharacter character;
        string chatId;
        string AUTH_TOKEN;
        string USERNAME;
        string USER_ID;
        #endregion

        /// <summary>
        /// Метод начальной инициализации текстового поля.
        /// </summary>
        /// Не идеален, просто вынесен в отдельный метод.
        private void InitializeTextBox() {
            textWindow.Visibility = Visibility.Hidden;  // Прячем розовую коробку текста
            textBlock.Text = "";                        // Убираем весь текст
        }

        private DirectoryInfo GetCharacterDirectory() {
            DirectoryInfo dirInfo = new DirectoryInfo(baseDir + "/characters");
            if (!dirInfo.Exists) {
                dirInfo.Create();
            }
            return dirInfo;
        }
        ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
        private void InitializeLanguage() {
            m_Languages.Clear();                        // Чистим список языков
            m_Languages.Add(new CultureInfo("en-US"));  // Нейтральная культура для этого проекта
            m_Languages.Add(new CultureInfo("ru-RU"));  // Сторонняя культура
            LanguageChanged += App_LanguageChanged;     // Присваиваем функцию смены языка к ивенту смены языка
            Lang = new CultureInfo(Monika.lang);        // Ставим язык из настроек
            Language = Lang.Parent.ToString();          // Ставим имя языка
            //Debug.WriteLine(Language);                // Дебаг языка
            setLanguage(Language);                      // Устанавливаем язык
        }

        public MainWindow()     // Код главного окна
        {
            CollectAllGarbage();                        // Принудительно собираем мусор перед запуском
            InitializeComponent();                      // Инициализация ЮИ (Юзер Интерфейс)(Вроде для этого)
            SetProcessWorkingSetSize
                (Process.GetCurrentProcess()            // Ограничиваем приложению доступ к оперативной памяти (by Solicen)
                .Handle, -1, -1);


            oldIsNight = IsNight;
            mainFilter = nightFilter;
            AllowsTransparency = true;

            var dirInfo = GetCharacterDirectory();      // Получение папки персонажей


            Monika.loadData();
            firsLaunch = !Monika.fileExist();           // Если файла нету, то это первый запуск
            this.settingsWindow = new Settings(this);   // Объявляем окно настроек (так нужно)
            MonikaSettings.Default.Reload();            // Читаем настройки

            if (String.IsNullOrEmpty(MonikaSettings.Default.UserName) || MonikaSettings.Default.UserName == "{PlayerName}") {
                playerName = Environment.UserName;
            }
            else {
                playerName = Monika.playerName;
            }

            InitializeLanguage();                        // Инициализация текущего языка         
            playerName =
                (Environment.GetCommandLineArgs()        // Режим Солицена (by Solicen)
                .Any(arg => arg == "--solicen"))         // Переписанный код, для активации режима по аргументу вместо закодированного метода
                ? "Denis Solicen" : playerName;

            this.setFace(normalPose);                    // Ставим спокойный вид
            InitializeTextBox();                         // Инициализация текстового поля (вынесено by Solicen)
            SetAutorunValue(Monika.autoStart);           // Ставим параметр автозапуска

            if (IsBDay) Debug.WriteLine("Сегодня день рождения?: Да");
            else Debug.WriteLine("Сегодня день рождения?: Нет");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");  // Я хз что это
            ManagementObjectCollection collection = searcher.Get();  // Также

            try {

                startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
                startWatch.Start();

                //ManagementEventWatcher stopWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
                //stopWatch.EventArrived += new EventArrivedEventHandler(stopWatch_EventArrived);
                //stopWatch.Start();
            }
            catch (Exception ex) {
                /*System.Windows.MessageBox.Show(this,
                   "An error occured: " + ex.Message + "\r\n\r\n(Try run this app as an administrator.)");*/
            }
            SystemEvents.PowerModeChanged += OnPowerChange;

            giftWatcher.Path = Monika.giftsPath;
            giftWatcher.NotifyFilter = NotifyFilters.FileName;
            giftWatcher.IncludeSubdirectories = true;
            giftWatcher.Created += GiftWatcher_Created;
            giftWatcher.EnableRaisingEvents = true;
            loadGifts();
            mouse = Monika.isMouse;

            /*
            FileSystemWatcher acsWatcher = new FileSystemWatcher();
            acsWatcher.Path = Monika.giftsPath;
            acsWatcher.NotifyFilter = NotifyFilters.FileName;
            acsWatcher.Deleted += AcsWatcher_Deleted;
            acsWatcher.Created += AcsWatcher_Created;
            acsWatcher.EnableRaisingEvents = true;*/

            CollectAllGarbage();
        }

        FileSystemWatcher giftWatcher = new FileSystemWatcher();
        // TODO: Расширить этот метод для сбора большего количества мусора
        private void CollectAllGarbage() {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.KeepAlive(giftWatcher);
            GC.KeepAlive(startWatch);
        }

        string GetCharacterAiHistory(string setHistory = null) {
            var tokenfile = $"{baseDir}\\token.txt";
            if (File.Exists(tokenfile)) {
                var line = File.ReadAllLines(tokenfile)[0];
                if (setHistory == null) {
                    try {
                        return line.Split('|')[3];
                    }
                    catch {
                        return string.Empty;
                    }
                }
                else {
                    line += $"|{setHistory}";
                }

            }

            return string.Empty;
        }
        async Task<bool> InitializationSetupCharacterAI() {
            var tokenfile = $"{baseDir}\\token.txt";
            var emailFile = $"{baseDir}\\email.txt";

            if (File.Exists(tokenfile)) {
                var line = File.ReadAllLines(tokenfile)[0];
                var token = line.Split('|')[0];
                var userId = line.Split('|')[1];
                var userName = line.Split('|')[2];

                AUTH_TOKEN = token;
                USERNAME = userName;
                USER_ID = userId;

                return true;

            }
            else if (File.Exists(emailFile)) {
                var key = File.ReadAllLines(emailFile)[0];
                if (key.Contains("@")) {
                    await client.SendLoginEmailAsync(key);
                    Environment.Exit(0);
                }
                else {
                    var user = await client.LoginByLinkAsync(key);
                    File.WriteAllText(tokenfile, $"{user.Token}|{user.UserId}|{user.Username}");
                    File.Delete(emailFile);
                    Environment.Exit(0);
                }
            }

            return false;
        }

        /// <summary>
        /// Инициализация и настройка CharacterAI модуля для Моники.
        /// </summary>
        /// <returns></returns>
        public async Task SetupCharacterAIModule() {
            if (await InitializationSetupCharacterAI() == false) return;
            if (Monika.AI && isConectedToInternet()) {
                this.MouseDoubleClick += window_MouseDoubleClick;

                CaiCharacter character = await client.GetCharacterInfoAsync(characterId, AUTH_TOKEN);
                List<CaiChat> result = await client.GetChatsAsync(characterId, AUTH_TOKEN);

                chatId = GetCharacterAiHistory();
                if (chatId == string.Empty) {
                    chatId = client.CreateNewChat(characterId, USER_ID, AUTH_TOKEN);
                    GetCharacterAiHistory(chatId);
                }

                if (chatId is null) {
                    return;
                }

            }
        }


        public async void Window_Loaded(object sender, RoutedEventArgs e)     // Когда программа проснётся
        {

            await SetupCharacterAIModule();
            if (IsNight) mainFilter = nightFilter;
            else mainFilter = dayFilter;

            /*
            new ToastContentBuilder()
       .AddArgument("action", "viewConversation")
       .AddArgument("conversationId", 9813)
       .AddText("Смотри что я умею!")
       .AddText("Я научилась отправлять уведомления =)")
       .Show();*/

            var wpfDpi = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformToDevice.M11;
            this.dpiScale = 1f / (float)wpfDpi.GetValueOrDefault(1);

            Monika.loadData();                        // Грузим данные 
            SetupScale(Monika.Scaler);                // Ставим размер окна
            Lang = MonikaSettings.Default.Language;

            //UnpackCostume(Monika.costumeName);

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
                }
                else if (!firsLaunch && Monika.pcName != Environment.MachineName) // Если сменили ПК
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
                }
                else // Просто привет
                {

                    Monika.pcName = Environment.MachineName;
                    Debug.WriteLine("Просто запуск");
                    runRpy(greetingsDialogPath, "random_");
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
                                // Check if currently speaking, only blink if not in dialog
                                if (!isSpeaking) {
                                    if (!formatURL(query).Contains("google.com/search?")) {
                                        Debug.WriteLine("Открыт сайт: " + formatURL(query));
                                        DirectoryInfo info = new DirectoryInfo(sitesDialogDirectory.FullName + "\\[" + formatURL(query) + "]");
                                        //RunScript(info.FullName + "\\" + new Random().Next(info.GetFiles().Length) + ".txt");

                                        runRpy(sitesDialogPath, formatURL(query), true);
                                        //runRpy(idleDialogPath, "random_");
                                        //readLongXml(formatURL(query), sitesDialogPath, 1);
                                    }

                                    if (formatURL(query).Contains("google.com/search?")) {
                                        Debug.WriteLine("Введён запрос Google: " + formatURL(query));
                                        DirectoryInfo info = new DirectoryInfo(googleDialogDirectory.FullName + "\\[" + formatURL(query) + "]");
                                        //RunScript(info.FullName + "\\" + new Random().Next(info.GetFiles().Length) + ".txt");
                                        runRpy(googleDialogPath, formatURL(query), true);
                                        //readLongXml(formatURL(query), googleDialogPath, 2);
                                    }

                                    if (formatURL(query).Contains("youtube.com/results?")) {
                                        Debug.WriteLine("Введён запрос Youtube: " + formatURL(query));
                                        DirectoryInfo info = new DirectoryInfo(youtubeDialogDirectory.FullName + "\\[" + formatURL(query) + "]");
                                        //RunScript(info.FullName + "\\" + new Random().Next(info.GetFiles().Length) + ".txt");
                                        runRpy(youtubeDialogPath, formatURL(query), true);
                                        //readLongXml(formatURL(query), youtubeDialogPath, 3);
                                    }
                                }
                                lastQuery = query;
                            }
                            else {
                                Debug.WriteLine("Повторный запрос");
                            }

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
                                Debug.WriteLine("DialoG check: " + isSpeaking);
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
                                        else {

                                            Debug.WriteLine("DIALOG");
                                            //RunScript(idleDialogDirectory.FullName + "\\" + new Random().Next(idleDialogDirectory.GetFiles().Length) + ".txt");
                                            runRpy(idleDialogPath, "random_");
                                        }
                                    }
                                    else {

                                        Debug.WriteLine("DIALOG");
                                        //RunScript(idleDialogDirectory.FullName + "\\" + new Random().Next(idleDialogDirectory.GetFiles().Length) + ".txt");
                                        runRpy(idleDialogPath, "random_");
                                    }

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
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);

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
                                }

                                nextBlink = DateTime.Now + TimeSpan.FromSeconds(random.Next(7, 50));
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
                                }
                                else {
                                    if (point.Y <= rectangle.Bottom) {
                                        if (point.Y >= rectangle.Y) {
                                            if (point.X < rectangle.X && rectangle.X - point.X < FADE) {
                                                opacity = MainWindow.Lerp(1.0, MIN_OP, (rectangle.X - point.X) / FADE);
                                            }
                                            else if (point.X > rectangle.Right && point.X - rectangle.Right < FADE) {
                                                opacity = MainWindow.Lerp(1.0, MIN_OP,
                                                    (point.X - rectangle.Right) / FADE);
                                            }
                                        }
                                        else if (point.Y < rectangle.Y) {
                                            if (point.X >= rectangle.X && point.X <= rectangle.Right) {
                                                if (rectangle.Y - point.Y < FADE) {
                                                    opacity = MainWindow.Lerp(1.0, MIN_OP,
                                                        (rectangle.Y - point.Y) / FADE);
                                                }
                                            }
                                            else if (rectangle.X > point.X || rectangle.Right < point.X) {
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
                                if (mouse) {
                                    Dispatcher.Invoke(() => { mainApp.Opacity = opacity; });
                                }
                                else {
                                    Dispatcher.Invoke(() => { mainApp.Opacity = 1.0; });
                                }
                            }
                        }
                    });
                });
                /*if (isConectedToInternet())
                    await checkUpdatesAsync();*/

            };
            this.BeginAnimation(OpacityProperty, _start);
        }
        private void Window_Closed(object sender, EventArgs e) {
            MonikaSettings.Default.isColdShutdown = true;
            Monika.saveData();
            this.applicationRunning = false;
        }       // Когда закрыли программу
        bool isTyping = false;

        /// <summary>
        /// Метод который активируется при перекидывании подарка.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GiftWatcher_Created(object sender, FileSystemEventArgs e) {
            string file = e.FullPath;
            // Assuming you have one file that you care about, pass it off to whatever
            // handling code you have defined.
            Debug.WriteLine("Перекинули файл:" + file);
            FileInfo info = new FileInfo(file);
            if (info.Exists) {
                if (info.Extension == ".gift") {
                    string giftName = info.Name.ToLower().Replace(".gift", String.Empty);

                    string path = $"{baseGiftsPath}{giftName}\\";
                    if (!Directory.Exists(baseGiftsPath)) {
                        DirectoryInfo di = Directory.CreateDirectory(baseGiftsPath);
                        di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                    }
                    File.Copy(AppDomain.CurrentDomain.BaseDirectory + "/characters/" + giftName + ".gift", baseGiftsPath + giftName + ".gift", true);
                    ZipFile.ExtractToDirectory(baseGiftsPath + giftName + ".gift", path);
                    RunScript(path + "ru.txt");
                    addGift(giftName, path + giftName + ".png");
                    //getGift(giftName);
                    info.Delete();
                    //readLongXml(giftName, giftsDialogPath, 4);
                    Debug.WriteLine("Подарен подарок:" + giftName);
                    RedrawGifts();
                }
                if (info.Extension == ".costume") {
                    string costumeNam = info.Name.ToLower().Replace(".costume", String.Empty);
                    UnpackCostume(costumeNam);
                    info.Delete();
                    Monika.saveData();
                }
            }
        }
        /// <summary>
        /// Метод который загружает подарки для Моники.
        /// </summary>
        public void loadGifts() {
            var spanGifts = CollectionsMarshal.AsSpan(Monika.gifts);
            foreach (string i in spanGifts) {
                string[] gift = i.Split(" | ");
                BitmapImage bitmapImage = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(gift[1]), mainFilter));
                System.Windows.Controls.Image img = new System.Windows.Controls.Image {
                    Source = bitmapImage,
                    Name = gift[0]
                };
                if (FindName(gift[0]) == null)
                    RegisterName(gift[0], img);
                gifts.Children.Add(img);
                DeleteObject(bitmapImage);
            }
        }
        bool inChoise = false;
        string oldLine = "";
        public async void RunScript(string path) {
            isSpeaking = true;
            var script = Script.FromSource(path);
            script.Prime();

            // Check that the script is correctly initialized and ready to run
            script.Validate();
            // Run the script

            try {
                foreach (var x in script) {
                    Debug.WriteLine(x.ToString());
                    if (x is Script.Menu) {

                        var menu = x as Script.Menu;

                        Menu(oldLine, menu.Choices.Count, menu.Choices);
                        script.CurrentChoiceIndex = (uint?)ch;

                    }
                    else if (x is Script.DialogueLine) {
                        while (inChoise) {
                            await Task.Delay(100);
                        }
                        var line = x as Script.DialogueLine;

                        oldLine = line.ToString().Substring(6).Replace("'player'", playerName);
                        await SayV2(line.ToString() + Environment.NewLine);

                    }
                    else if (x is Script.Reference) {

                        var reference = x as Script.Reference;
                        switch (reference.Tag) {
                            case "DoNothing":
                                Console.WriteLine("Why bother?");
                                break;
                            default:
                                reference.Action();
                                break;
                        }

                    }
                }
            }
            catch {

            }
            /*script.RunFromBeginning(
                OnMenu: menu => (new Random()).Next(menu.Count - 1),
                OnLine: line => { SayV2(line.ToString()); });*/
            this.Dispatcher.Invoke(() => {
                setFace(normalPose);
                textWindow.Visibility = Visibility.Hidden;
                isSpeaking = false;
            });
            isSpeaking = false;
            Debug.WriteLine("speak: " + isSpeaking);

            CollectAllGarbage();
            script = null;

        }
        async Task SayV2(string line, int delay = 20) {
            /*while (isSpeaking) {
                await Task.Delay(100);
            }
            isSpeaking = true;*/

            this.Dispatcher.Invoke(() => {
                textWindow.Visibility = Visibility.Visible;
            });
            string newText = "";
            if (Char.IsDigit(line[0])) {
                newText = line.Substring(6).Replace("'player'", playerName).Replace("{PlayerName}", playerName); //замена
                setFace(line.Substring(0, 4));
                CollectAllGarbage();
            }
            else {
                newText = line.Replace("'player'", playerName).Replace("{PlayerName}", playerName).Replace("<Anonymous>: ", "");
                newText = char.ToUpper(newText[0]) + newText.Substring(1);
                setFace(normalPose);
                CollectAllGarbage();
            }

            for (int i = 0; i < newText.Length; i++) {
                this.Dispatcher.Invoke(() => {
                    this.textBlock.Text += newText[i];
                });
                if (newText[i].ToString() == ".") {
                    await Task.Delay(500);
                }
                else {
                    await Task.Delay(delay);
                }

            }
            await this.Dispatcher.Invoke(async () => {
                await Task.Delay(700);
                textBlock.Text = "";
            });

            CollectAllGarbage(); // А тут уже пытаемся собирать излишний мусор
        }
        int ch = 0;
        void Menu(string s, int num, List<Script.Choice> choices) {
            inChoise = true;
            mouse = false;
            textWindow.Visibility = Visibility.Visible;
            ButtonsGrid.RowDefinitions.Clear();
            textBlock.Text = "";
            textBlock.Text = s;
            this.ButtonsGrid.Children.Clear();
            for (int i = 0; i < num; i++) {
                System.Windows.Controls.RowDefinition row = new System.Windows.Controls.RowDefinition();
                ButtonsGrid.RowDefinitions.Add(row);
                var text = new OutlinedTextBlock {
                    Text = choices[i].Text,
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
                button.Click += Button_Click;

                System.Windows.Controls.Grid.SetRow(button, i);
                this.ButtonsGrid.Children.Add(button);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e) {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
            string s = button.Name.Replace("butt", "");
            ch = int.Parse(s);
            inChoise = false;
            this.Dispatcher.Invoke(() => {
                textBlock.Text = "";
                ButtonsGrid.RowDefinitions.Clear();
                this.ButtonsGrid.Children.Clear();
                mouse = Monika.isMouse;
            });
        }

        bool random = false;

        public async Task runRpy(string file, string label, bool n = false) {

            if (n) {
                var lines = File.ReadAllLines(file.Replace("rpy", "txt")).ToList();
                int index = lines.FindIndex(a => a.Contains(label));
                if (index != -1)
                    label = lines[index + 1];
                else
                    return;
                //Debug.WriteLine(lines[index+1]);

            }


            isSpeaking = true;
            var script = Script.FromSource(file);

            script.Actions = new Dictionary<string, Action>() {
                {"norandom", () => {random = false; } },
                {"random", () => {random = true; } }
            };

            script.Prime();

            // Check that the script is correctly initialized and ready to run
            script.Validate();
            // Run the script
            if (label == "random_")
                script.StartingLabel = script.Lines.ToList()[new Random().Next(0, script.Lines.Count)].Key;
            else
                script.StartingLabel = label;

            try {
                foreach (var x in script) {
                    Debug.WriteLine(x.ToString());
                    if (x is Script.Menu) {

                        var menu = x as Script.Menu;
                        if (random) {
                            script.CurrentChoiceIndex = (uint?)new Random().Next(0, menu.Choices.Count - 1);
                        }
                        else {
                            Menu(oldLine, menu.Choices.Count, menu.Choices);
                            script.CurrentChoiceIndex = (uint?)ch;
                        }

                    }
                    else if (x is Script.DialogueLine) {
                        while (inChoise) {
                            await Task.Delay(100);
                        }
                        var line = x as Script.DialogueLine;

                        oldLine = line.ToString().Substring(6).Replace("'player'", playerName);
                        await SayV2(line.ToString() + Environment.NewLine);

                    }
                    else if (x is Script.Reference) {

                        var reference = x as Script.Reference;
                        switch (reference.Tag) {
                            case "DoNothing":
                                Console.WriteLine("Why bother?");
                                break;
                            default:
                                reference.Action();
                                break;
                        }

                    }
                }
            }
            catch {

            }
            /*script.RunFromBeginning(
                OnMenu: menu => (new Random()).Next(menu.Count - 1),
                OnLine: line => { SayV2(line.ToString()); });*/
            this.Dispatcher.Invoke(() => {
                setFace(normalPose);
                textWindow.Visibility = Visibility.Hidden;
                isSpeaking = false;
            });
            isSpeaking = false;
            Debug.WriteLine("speak: " + isSpeaking);

            CollectAllGarbage();
            script = null;
        }
/*
        private void stopWatch_EventArrived(object sender, EventArrivedEventArgs e) // Ивент закрытия процесса
        {
            Debug.WriteLine("Процесс закрыт: " + e.NewEvent.Properties["ProcessName"].Value.ToString());   // Дебажим имя закрытого процесса
        }*/
        private void startWatch_EventArrived(object sender, EventArrivedEventArgs e) // Ивент открытия процесса
        {
            if (!isSpeaking) {
                string currentProcess = e.NewEvent.Properties["ProcessName"].Value.ToString();  // Узнаём имя процесса
                if (currentProcess != lastProcess)  // Если оно не равно прошлому процессу, чтобы небыло повторок
                {
                    //Debug.WriteLine("Процесс открыт: " + e.NewEvent.Properties["ProcessName"].Value.ToString());   // Дебажим имя закрытого процесса

                    DirectoryInfo info = new DirectoryInfo(progsDialogDirectory.FullName + "\\[" + formatURL(currentProcess) + "]");
                    try {
                        //RunScript(info.FullName + "\\" + new Random().Next(info.GetFiles().Length) + ".txt");
                        runRpy(progsDialogPath, formatURL(currentProcess), true);
                    }
                    catch {

                    }
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
                    }
                    else          //-------- Иначе (Если была нажата кнопка ПРИНЯТЬ)
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
                        Monika.AI = MonikaSettings.Default.ai;
                        Monika.aiToken = MonikaSettings.Default.aitoken;
                        Monika.saveData();
                        if (String.IsNullOrEmpty(MonikaSettings.Default.UserName) || MonikaSettings.Default.UserName == "{PlayerName}") {
                            playerName = Environment.UserName;
                        }
                        else {
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
        private async void MenuQuit_Click(object sender, RoutedEventArgs e)   // Закрытие программы
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

                runRpy(goodbyeDialogPath, "random_");
                //RunScript(goodbyeDialogDirectory.FullName + "\\" + new Random().Next(goodbyeDialogDirectory.GetFiles().Length) + ".txt");
                //readXml(null, false, goodbyeDialogPath, 1); // Говорим прощание

                while (isSpeaking) {
                    await Task.Delay(100);
                }
                MonikaSettings.Default.isColdShutdown = true;
                Monika.saveData();
                Environment.Exit(0);
            }
            else {
            }
        }

        public async Task checkUpdatesAsync() {
            await Updater.CheckUpdate("SAn4Es-TV", "MonikaOnDesktop");  // Проверяем наличие обновления
            //Debug.WriteLine("This Ver: " + SolicenTEAM.Updater.CurrentVersion);
            //Debug.WriteLine("New Ver: " + SolicenTEAM.Updater.UpdateVersion);
            //Debug.WriteLine("New Desc: " + SolicenTEAM.Updater.UpdateDescription);
            if (Updater.UpdateIsAvailable()) { }
            else {
                while (isSpeaking) {
                    await Task.Delay(10);
                }
                //isSpeaking = true;
                WebClient client = new WebClient();
                client.Proxy = new WebProxy();
                Stream stream = client.OpenRead("https://raw.githubusercontent.com/SAn4Es-TV/MonikaOnDesktop/master/gitDesc.txt");
                StreamReader reader = new StreamReader(stream);
                String content = reader.ReadToEnd();
                //Debug.Write(content);
                // запись в файл
                using (FileStream fstream = new FileStream(updateDialogPath, FileMode.OpenOrCreate)) {
                    // преобразуем строку в байты
                    byte[] array = System.Text.Encoding.Default.GetBytes(content);
                    // запись массива байтов в файл
                    fstream.Write(array, 0, array.Length);
                }
                //readXml(null, false, updateDialogPath, 2);
                //Debug.WriteLine("endOfDialog");
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
            }
            catch (Exception) {
                return false;
            }
        }
        #region
        public async Task FirstLaunch() {
            Monika.pcName = Environment.MachineName;
            Box box = new Box("start");
            if (box.ShowDialog() == true) {
                switch (box.lang) {
                    case "ru":
                        Lang = new CultureInfo("ru-RU");
                        Language = Lang.Name.ToString();          // Ставим имя языка
                        setLanguage(Language);                      // Устанавливаем язык
                        break;
                    case "en":
                        Lang = new CultureInfo("en-US");
                        Language = Lang.Name.ToString();          // Ставим имя языка
                        setLanguage(Language);                      // Устанавливаем язык
                        break;
                }
                //LangBox.Visibility = Visibility.Visible;
                isSpeaking = true;
                playerName = box.name;
                Monika.playerName = playerName;
                Monika.lang = Language;
                Monika.pcName = Environment.MachineName;
                Monika.affection = 0;
                Monika.saveData();

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
            }
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
                        }
                        else {
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

                }
                catch (Exception e) {
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
                    DeleteObject(Eyes);
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
                    DeleteObject(eyebrow);
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
                    DeleteObject(mouth);
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

                    DeleteObject(Face);
                    DeleteObject(Hair);
                    DeleteObject(HairBack);

                    this.Face.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/face/" + nosePath), mainFilter));
                    this.Hair.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/h/" + hairPath + "-front.png"), mainFilter));
                    this.HairBack.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/h/" + hairPath + "-back.png"), mainFilter));

                    this.table1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/t/chair-def.png"), mainFilter));
                    this.table2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/t/table-def.png"), mainFilter));
                    this.table3.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/t/table-def-s.png"), mainFilter));
                });
            }
            catch {
            }
        }
        public void RedrawCostume(int body, string costume) {
            CollectAllGarbage();
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
            }
            catch {

            };
        }
        public void RedrawGifts() {

            this.Dispatcher.Invoke(() => {
                gifts.Children.Clear();
                foreach (string i in Monika.gifts) {
                    string[] gift = i.Split(" | ");
                    BitmapImage bitmapImage = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(gift[1]), mainFilter));
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
            File.Copy(AppDomain.CurrentDomain.BaseDirectory + "/characters/" + name + ".costume", costumesPath + name + ".costume", true);
            Monika.costumeName = name;
            if (!Directory.Exists(costumesPath)) {
                DirectoryInfo di = Directory.CreateDirectory(costumesPath);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            else {
                if (Directory.Exists(path)) {
                    DirectoryInfo di = new DirectoryInfo(path);
                    di.Delete(true);
                    Directory.CreateDirectory(path);
                }
            }
            ZipFile.ExtractToDirectory(AppDomain.CurrentDomain.BaseDirectory + "/costumes/" + name + ".costume", AppDomain.CurrentDomain.BaseDirectory + "/costumes/" + name);
            setFace(normalPose);
        }
        public void addGift(string name, string path) {
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                System.Windows.Controls.Image img1 = (System.Windows.Controls.Image)gifts.FindName(name);
                if (img1 == null) {
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image {
                        //Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/gifts/" + name + path)),
                        Source = new BitmapImage(new Uri(path)),
                        Name = name
                    };
                    RegisterName(name, img);
                    gifts.Children.Add(img);
                    string giftList = "";
                    /*foreach (string i in giftNameList)
                    {
                        giftList += i + "|";
                    }*/
                    Monika.gifts.Add(name + " | " + path + " | ");
                    Monika.saveData();
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
                await Updater.CheckUpdate("SAn4Es-TV", "MonikaOnDesktop");
                if (Updater.UpdateVersion != Updater.CurrentVersion && Updater.UpdateVersion != "") {
                    await Updater.DownloadUpdate();

                    while (!Updater.readyToUpdate) {
                        Debug.WriteLine("Update is ready: " + Updater.readyToUpdate);
                        await Task.Delay(10);
                    }
                    Debug.WriteLine("Update is ready: " + Updater.readyToUpdate);
                    Updater.ExtractArchive();
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
            }
            catch (Exception e) {
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
                }
                else {
                    leftScreen = s;
                }
            }
            var rightWorkingArea = rightScreen.WorkingArea;
            if (MonikaSettings.Default.screenNum) {
                var workingRectangle = Screen.PrimaryScreen.WorkingArea;
                rightWorkingArea = rightScreen.WorkingArea;
            }
            else {
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
            }
            catch {
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
                }
                else {
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
                }
                else {
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

                    greetingsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/greetings.rpy"; // Greetings
                    idleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/idle.rpy";           // Idle
                    progsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "Dialogs\\ru\\progs.rpy";         // Programs
                    sitesDialogPath = AppDomain.CurrentDomain.BaseDirectory + "Dialogs\\ru\\sites.rpy";         // Sites
                    googleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "Dialogs\\ru\\google.rpy";       // Google search
                    youtubeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "Dialogs\\ru\\youtube.rpy";     // Youtube search
                    goodbyeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs\\ru\\goodbye.rpy";     // Goodbye
                    giftsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/ru/gifts/gifts.txt";// Подарки
                    break;
                case "en":
                    quitMenu.Header = "Quit";
                    settingsMenu.Header = "Settings";

                    greetingsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/greetings.rpy"; // Greetings
                    idleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/idle.rpy";           // Idle
                    progsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "Dialogs\\en\\progs.rpy";         // Programs
                    sitesDialogPath = AppDomain.CurrentDomain.BaseDirectory + "Dialogs\\en\\sites.rpy";         // Sites
                    googleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "Dialogs\\en\\google.rpy";       // Google search
                    youtubeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "Dialogs\\en\\youtube.rpy";     // Youtube search
                    goodbyeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/goodbye.rpy";     // Goodbye
                    giftsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/gifts/gifts.txt";// Подарки
                    break;
                default:
                    quitMenu.Header = "Quit";
                    settingsMenu.Header = "Settings";

                    greetingsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/greetings.rpy"; // Greetings
                    idleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/idle.rpy";           // Idle
                    progsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "Dialogs\\en\\progs.rpy";         // Programs
                    sitesDialogPath = AppDomain.CurrentDomain.BaseDirectory + "Dialogs\\en\\sites.rpy";         // Sites
                    googleDialogPath = AppDomain.CurrentDomain.BaseDirectory + "Dialogs\\en\\google.rpy";       // Google search
                    youtubeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "Dialogs\\en\\youtube.rpy";     // Youtube search
                    goodbyeDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/goodbye.rpy";     // Goodbye
                    giftsDialogPath = AppDomain.CurrentDomain.BaseDirectory + "/Dialogs/en/gifts/gifts.txt";// Подарки
                    break;

            }
        }

        private async void window_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            Box box = new Box("ai");
            box.Left = this.Left;
            box.Top = this.Top;
            if (box.ShowDialog() == true) {
                string foo = box.aiText;
                if(!String.IsNullOrEmpty(foo)) {
                    if (await InitializationSetupCharacterAI() != false) {
                        isTyping = true;
                        string _message = foo;
                        Debug.WriteLine(_message);

                        CaiSendMessageInputData data = new CaiSendMessageInputData {
                            CharacterId = characterId,
                            ChatId = chatId,
                            Message = _message,
                            UserId = USER_ID,
                            Username = USERNAME,
                            UserAuthToken = AUTH_TOKEN
                        };

                        var response = client.SendMessageToChat(data);
                        string message = response; // => "Hey!"
                        string text = message.Replace("\n", ".")
                            .Replace("\"", "\'")
                            .Replace("...", "...\n")
                            .Replace("!", "!\n")
                            .Replace("?", "?\n")
                            .Replace(".", ".\n")
                            .Replace("\" ", "\"").Replace("\\", "");
                        string[] _text = text.Split("\n");
                        List<string> strings = new List<string>();
                        foreach (string s in _text) {
                            Debug.Write("[AI]: " + s + " (" + String.IsNullOrEmpty(s) + ") : ");
                            if (s != "\t" && !(String.IsNullOrEmpty(s)) && s != "") {
                                Debug.WriteLine("PASS");
                                string l = "";
                                if (!s.StartsWith("\"")) l += "\t\"";
                                l += s;
                                if (!s.EndsWith("\"")) l += "\"";
                                if (l != "\t\"\"" && l != "\t\".\"")
                                    strings.Add(l);
                            }
                        }
                        string final = "label start: \n";
                        foreach (string s in strings) {
                            final += s + "\n";
                        }
                        //.Replace("\"\n\t\"", "");
                        string fileText = "";
                        string dbgtxt = text.Replace("\n", "").Replace("\t", "");
                        /*if (text.Replace("\n", "").Replace("\t", "").EndsWith("\"\"")) {
                            int lastSpaceIndex = text.LastIndexOf('\"');
                            fileText = text.Substring(0, lastSpaceIndex);
                            fileText = "label start: \n\t\"" + fileText;
                            //fileText = fileText.Replace("\"");
                        }

                        else
                            fileText = "label start: \n\t\"" + text + "\"";*/
                        /*
                        // Swipe
                        var newCharacterResponse = await client.CallCharacterAsync(
                            characterId: character.Id,
                            characterTgt: character.Tgt,
                            historyId: historyId,
                            parentMsgUuid: userMessageUuid
                        );*/
                        // полная перезапись файла 
                        using (StreamWriter writer = new StreamWriter(AIpath, false)) {
                            //await writer.WriteLineAsync(fileText.Replace("\"\n\t\"\n", "\""));
                            await writer.WriteLineAsync(final.Replace("\"\"", "\""));
                        }
                        RunScript(AIpath);

                        isTyping = false;
                    }
                }
            }
        }
    }


}