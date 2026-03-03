using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using Monika.Shared;
using MonikaOnDesktop.Classes;
using Path = System.IO.Path;

namespace MonikaOnDesktop {
    public class ProcessReaction {
        public string Label { get; set; }
        public List<string> Apps { get; set; }
    }
    // Модель данных от расширения
    public class BrowserData {
        public string url { get; set; }
        public string google { get; set; }
        public string youtube { get; set; }
    }
    class MonikaSprite {
        public string head;
        public string ribbon;
        public string hair;
        public string body;
        public string leftArm;
        public string rightArm;
        public string c_body;
        public string c_leftArm;
        public string c_rightArm;
        public string eyes;
        public string eyebrows;
        public string mouth;
        public string blush;
        public string tears;
        public string sweat;
        public string nose;

        public List<string> acs = new List<string>();
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;

        MonikaSprite sprite = new MonikaSprite();

        static string currWear = "blackdress";
        static string currRibbon = "def";
        static string currHat = "null";
        public string hairType = "def";
        public string currAhoge = "null";

        static string mainPath = AppDomain.CurrentDomain.BaseDirectory;
        static string scriptsPath = System.IO.Path.Combine(mainPath, "scripts");

        static string monikaPath = System.IO.Path.Combine(mainPath, "monika");
        static string facePath = System.IO.Path.Combine(monikaPath, "f");
        static string bodyPath = System.IO.Path.Combine(monikaPath, "b");
        static string wearPath = System.IO.Path.Combine(monikaPath, "c", currWear);
        static string hairPath = System.IO.Path.Combine(monikaPath, "h");
        static string acsPath = System.IO.Path.Combine(monikaPath, "a");

        static string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "debug_log.txt");

        public string leaningWord = "leaning";
        public string nullPath = "null";

        string[] idle_poses = new string[] { "1", "2", "5", "6" };
        string[] idle_eyes = new string[] { "e", "l" };

        string idle_pose = "5ema";
        string main_pose = "";

        string face_happy = "h";
        string face_happy_2 = "d";


        Dictionary<int, string> NUM_ARMS = new Dictionary<int, string>(){
            { 0, "null" },
            { 1, "crossed" },
            { 2, "left-down" },
            { 3, "left-rest" },
            { 4, "right-down" },
            { 5, "right-point"},
            { 6, "right-restpoint"},
            { 7, "steepling"},
            { 8, "leaning-def-left-def"},
            { 9, "leaning-def-right-def"},
        };
        int[][] arms = new int[][]{
            new int[] {1, 7, 0},
            new int[] {2, 1, 0},
            new int[] {3, 3, 6},
            new int[] {4, 2, 5},
            new int[] {5, 8, 9 },
            new int[] {6, 2, 4 },
            new int[] {7, 2, 6 },
        };

        private bool _isTyping = false;
        private bool _cancelTyping = false;

        Point screenSize = new();

        private ProcessReaction _processManager = new();
        private ManagementEventWatcher _watcher;
        // Быстрый словарь: Имя процесса -> Метка в RPY
        private Dictionary<string, string> _appLookup = new();
        private Dictionary<string, string> _urlLookup = new();
        private Dictionary<string, string> _youtubeLookup = new();
        private Dictionary<string, string> _googleLookup = new();

        AppSettings settings = new();

        private LinearGradientBrush _dayNightGradient;
        double graientPosition = 0;

        private BotServerManager _server = new BotServerManager();
        JsonObject jsonObject;

        // В конструкторе или при инициализации:
        MultiplyEffect monikaShader = new();
        LanguageModel language;

        Random rnd = new();

        ScriptEngine _engine = new();
        string filePath = "";
        float dpiScale = 1f;

        int hm_guesses = 0;
        int hm_chances = 0;
        string hm_char = "m";
        string hm_word = "m";

        private string _pendingVarName;
        public MainWindow() {
            InitializeComponent();
            LoadSettings();
            UpdateVariables();
            GetDialogs();

            var wpfDpi = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformToDevice.M11;
            this.dpiScale = 1f / (float)wpfDpi.GetValueOrDefault(1);

            // 1. Создаем градиент (0 - полночь, 0.5 - полдень, 1.0 - снова полночь)
            _dayNightGradient = new LinearGradientBrush();
            _dayNightGradient.GradientStops.Add(new GradientStop(Color.FromRgb(175, 99, 71), 0)); // Утро
            _dayNightGradient.GradientStops.Add(new GradientStop(Colors.White, 0.5));                // День
            _dayNightGradient.GradientStops.Add(new GradientStop(Color.FromRgb(175, 99, 71), 1));  // Закат


            saybox.Visibility = Visibility.Hidden;

            screenSize.X = SystemParameters.WorkArea.Width;
            screenSize.Y = SystemParameters.WorkArea.Height;

            Left = screenSize.X - Width;
            Top = screenSize.Y - Height;

            Log("Application started.");

            // Настройка событий движка
            _engine.OnSpeech = async (text, pose) => { _isTyping = true; await TypeText(text, pose); };
            _engine.OnMenuRequired = (indent) => ShowMenu(indent);
            _engine.OnLog = (msg) => Log(msg);
            _engine.OnScriptFinished = () => {
                Dispatcher.Invoke(() => saybox.Visibility = Visibility.Hidden);
                _isTyping = false;
                setPose(idle_pose);
            };
            _engine.OnQuitCalled = () => {
                this.Dispatcher.Invoke(() => {
                    this.Close();
                });
            };
            _engine.OnInputRequired = (varName, text) => {
                Dispatcher.Invoke(() => {
                    _pendingVarName = varName;
                    InputContainer.Visibility = Visibility.Visible;
                    InputField.Text = "";
                    InputField.Focus(); // Сразу ставим курсор в поле
                    InputText.Text = text;
                });
            };
            _engine.RegisteredFunctions["hm_restart"] = () => {

                PipeManager.SendMessage("MonikaInteractionPipeR", $"HM_RESTART");
            };
            _engine.RegisteredFunctions["hm_close"] = () => {

                PipeManager.SendMessage("MonikaInteractionPipeR", $"HM_QUIT");
            };
            _engine.RegisteredFunctions["lang_ru"] = () => {

                ChangeLanguage("ru-RU");
            };
            _engine.RegisteredFunctions["lang_en"] = () => {

                ChangeLanguage("en-US");
            };
            CompositionTarget.Rendering += (s, e) => {
                MyOutline.UpdateScale(this.ActualWidth);
            };


            //setPose("2ekblsdra");
            //TypeText("Привет всем от всего выфвыф dsadsadsa i! ", "5ttbla");

            jsonObject = JsonNode.Parse(File.ReadAllText(System.IO.Path.Combine(monikaPath, "sprite_map.json"))).AsObject();

            MonikaContainer.Effect = monikaShader;
            if (settings.Username == "__fistrun__" && !File.Exists(Path.Combine(scriptsPath, "firstrun"))) {
                isMonikaHere = false;
            }

            setPose(idle_pose);
            var anim = new DoubleAnimation();
            anim.From = 0;
            anim.To = 1;
            anim.Duration = TimeSpan.FromSeconds(3);
            if (isMonikaHere)
                anim.Completed += Start_Completed;
            this.BeginAnimation(Window.OpacityProperty, anim);

            LoadConfig(Path.Combine(scriptsPath, GetDialogLang(), "processes.json"), _appLookup);
            LoadConfig(Path.Combine(scriptsPath, GetDialogLang(), "sites.json"), _urlLookup);
            LoadConfig(Path.Combine(scriptsPath, GetDialogLang(), "youtube.json"), _youtubeLookup);
            LoadConfig(Path.Combine(scriptsPath, GetDialogLang(), "google.json"), _googleLookup);

            createContextMenus();

            if (settings.Parsing["Processes"])
                StartMonitoring();

            if (settings.Parsing["Sites"] || settings.Parsing["Google"] || settings.Parsing["Youtube"])
                StartHttpServer();

            if (settings.AI)
                StartBunServer();

            if (isMonikaHere)
                // Запускаем сервер и говорим, что делать при получении сообщения
                PipeManager.StartServer("MonikaInteractionPipe", (msg) => {
                    Dispatcher.Invoke(() => {
                        if (!_isTyping) {
                            Log($"Получено событие: {msg}");
                            string key = "", arg = "";
                            string[] args = new string[] { };
                            if (msg.Contains(',')) {
                                key = msg.Split(',')[0];
                                arg = msg.Split(',')[1];
                                args = arg.Split(';');
                            } else {
                                key = msg;
                            }
                            switch (key) {
                                case "SINGLE_CLICK":
                                    if (rnd.Next(0, 2) == 0) {
                                        filePath = Path.Combine(scriptsPath, GetDialogLang(), "shimeji.rpy");
                                        RunLabel("click");
                                    }
                                    break;
                                case "ANNOYED":
                                    if (rnd.Next(0, 2) == 0) {
                                        filePath = Path.Combine(scriptsPath, GetDialogLang(), "shimeji.rpy");
                                        RunLabel("annoyed");
                                    }
                                    break;
                                case "HM_CLOSING":
                                    hm_chances = int.Parse(args[1]);
                                    hm_guesses = int.Parse(args[2]);
                                    hm_word = args[0];
                                    filePath = Path.Combine(scriptsPath, GetDialogLang(), "hangman.rpy");
                                    RunLabel("quit");
                                    break;
                                case "HM_WIN":
                                    filePath = Path.Combine(scriptsPath, GetDialogLang(), "hangman.rpy");
                                    RunLabel("win");
                                    break;
                                case "HM_LOSE":
                                    hm_word = args[0];
                                    filePath = Path.Combine(scriptsPath, GetDialogLang(), "hangman.rpy");
                                    RunLabel("lose");
                                    break;
                                case "HM_START":
                                    hm_char = args[0];
                                    filePath = Path.Combine(scriptsPath, GetDialogLang(), "hangman.rpy");
                                    RunLabel("start");
                                    break;
                                case "HM_TIP":
                                    hm_char = args[0];
                                    filePath = Path.Combine(scriptsPath, GetDialogLang(), "hangman.rpy");
                                    RunLabel("tip");
                                    break;
                            }
                        }
                    });
                });
            Loaded += (e, s) => {

                //Variable to hold the handle for the form
                var helper = new WindowInteropHelper(this).Handle;
                //Performing some magic to hide the form from Alt+Tab
                SetWindowLong(helper, GWL_EXSTYLE, (GetWindowLong(helper, GWL_EXSTYLE) | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
            };
            Closing += (e, s) => {
                _server.Stop();
            };
        }
        async void GetDialogs() {
            if (settings.DialogAutoUpdate && CheckForInternetConnection()) {
                Log("Получаем диалоги...");
                GitHubDownloader git = new GitHubDownloader();

                await git.DownloadDirectoryAsync("SAn4Es-TV", "MOD-Dialogs", "", Path.Combine(mainPath, "scripts"));
            }
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);

        private static double Lerp(double firstFloat, double secondFloat, double by) {
            return firstFloat * by + secondFloat * (1 - by);
        }
        bool isMonikaHere = true;
        private async void Start_Completed(object? sender, EventArgs e) {
            RunFilesWatcher(Path.Combine(mainPath, "characters"));
            Random random = new Random();
            _ = Task.Run(async () => {

                int counter = 0;
                if (settings.Username == "__fistrun__" && File.Exists(Path.Combine(scriptsPath, "firstrun"))) {
                    filePath = Path.Combine(scriptsPath, "start.rpy");
                    await RunLabel("start");
                    filePath = Path.Combine(scriptsPath, GetDialogLang(), "greetings.rpy");
                    await RunLabel("__firstrun__");
                    settings.Username = _engine.Variables["player1"].ToString();
                    File.Delete(Path.Combine(scriptsPath, "firstrun"));
                    SaveManager.Save(settings);
                    UpdateVariables();
                } else if (settings.PCname != System.Security.Principal.WindowsIdentity.GetCurrent().Name && settings.Username != "__fistrun__") {
                    filePath = Path.Combine(scriptsPath, GetDialogLang(), "greetings.rpy");
                    await RunLabel("__changepc__");
                    settings.PCname = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    SaveManager.Save(settings);
                    UpdateVariables();
                } else {
                    filePath = Path.Combine(scriptsPath, GetDialogLang(), "greetings.rpy");
                    await RunLabel("__random__");
                }

                int poseid = 2;
                int eyesid = 0;

                ImgNose.MouseLeftButtonDown += async (s, e) => {
                    if (!_isTyping) {
                        idle_pose = idle_poses[poseid];
                        idle_pose += face_happy;
                        idle_pose += "ua";
                        setPose(idle_pose);
                        await Task.Delay(2000);
                        idle_pose = idle_poses[poseid];
                        idle_pose += idle_eyes[eyesid];
                        idle_pose += "ua";
                        setPose(idle_pose);
                    }
                };
                ImgHairFront.MouseWheel += async (s, e) => {

                    if (!_isTyping) {
                        counter += 1;
                        if (counter > 20) {
                            idle_pose = idle_poses[poseid];
                            idle_pose += face_happy_2;
                            idle_pose += "ubfa";
                            setPose(idle_pose);
                            await Task.Delay(3000);
                            idle_pose = idle_poses[poseid];
                            idle_pose += idle_eyes[eyesid];
                            idle_pose += "ua";
                            setPose(idle_pose);
                            counter = 0;
                        }
                    }
                };
                var nextMove = DateTime.Now + TimeSpan.FromSeconds(random.Next(40, 180));
                var nextEye = DateTime.Now + TimeSpan.FromSeconds(random.Next(30, 60));
                var nextBlink = DateTime.Now + TimeSpan.FromSeconds(random.Next(1, 10));
                var nextDialog = DateTime.Now + TimeSpan.FromSeconds(random.Next(120, 300));
                while (true) {
                    // Idle animation
                    if (DateTime.Now >= nextMove && !_isTyping) {
                        // Build idle anim
                        poseid = random.Next(0, idle_poses.Length);
                        idle_pose = idle_poses[poseid];
                        idle_pose += idle_eyes[eyesid];
                        idle_pose += "ua";
                        setPose(idle_pose);
                        nextMove = DateTime.Now + TimeSpan.FromSeconds(random.Next(40, 180));
                    }
                    if (DateTime.Now >= nextEye && !_isTyping) {
                        // Build idle anim
                        eyesid = random.Next(0, idle_eyes.Length);
                        idle_pose = idle_poses[poseid];
                        idle_pose += idle_eyes[eyesid];
                        idle_pose += "ua";
                        setPose(idle_pose);
                        nextEye = DateTime.Now + TimeSpan.FromSeconds(random.Next(30, 60));
                    }
                    if (DateTime.Now >= nextBlink && !_isTyping) {
                        idle_pose = idle_poses[poseid];
                        idle_pose += "dua";
                        setPose(idle_pose);
                        nextBlink = DateTime.Now + TimeSpan.FromSeconds(random.Next(10, 25));
                        await Task.Delay(250);
                        idle_pose = idle_poses[poseid];
                        idle_pose += idle_eyes[eyesid];
                        idle_pose += "ua";
                        setPose(idle_pose);
                    }
                    if (DateTime.Now >= nextDialog && !_isTyping) {
                        filePath = Path.Combine(scriptsPath, GetDialogLang(), "idle.rpy");
                        RunLabel("__random__");
                        nextDialog = DateTime.Now + TimeSpan.FromSeconds(random.Next(120, 300));
                    }
                    await Task.Delay(50);
                }
            });

            Dispatcher.Invoke(() => {
                Task.Run(async () => {
                    var prev = new System.Drawing.Point();

                    var rectangle = new System.Drawing.Rectangle();
                    await this.Dispatcher.InvokeAsync(() => {
                        rectangle = new System.Drawing.Rectangle((int)(this.Left + this.Width / 2), (int)this.Top, (int)this.Width / 2,
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
                            if (settings.MouseHide) {
                                Dispatcher.Invoke(() => { mainApp.Opacity = opacity; });
                            } else {
                                Dispatcher.Invoke(() => { mainApp.Opacity = 1.0; });
                            }
                        }
                    }
                });
            });
        }
        private FileSystemWatcher watcher;
        public void RunFilesWatcher(string path) {
            watcher = new FileSystemWatcher(path);
            watcher.Filter = "*.*"; // Следим только за ZIP-архивами
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.FileName
                     | NotifyFilters.DirectoryName
                     | NotifyFilters.Attributes
                     | NotifyFilters.Size
                     | NotifyFilters.LastWrite;
            watcher.Created += async (s, e) => {
                if (e.FullPath.EndsWith(".gift")) {
                    ThreadPool.QueueUserWorkItem(async _ => {
                        // 1. Ждем, пока файл освободится (копирование завершится)
                        if (WaitForFile(e.FullPath)) {
                            try {
                                Log($"Подарен подарок: {e.Name}");
                                // Распаковка всего архива
                                ZipFile.ExtractToDirectory(e.FullPath, monikaPath, true);
                                filePath = Path.Combine(monikaPath, "gift.rpy");
                                string fileToDelete = filePath;
                                if (File.Exists(Path.Combine(monikaPath, $"gift_{settings.Lang}.rpy"))) {
                                    filePath = Path.Combine(monikaPath, $"gift_{settings.Lang}.rpy");
                                    fileToDelete = filePath;
                                }
                                if (File.Exists(fileToDelete)) {
                                    await RunLabelAsync(0);
                                    File.Delete(fileToDelete);
                                }
                                if (File.Exists(Path.Combine(monikaPath, "gift.rpy")))
                                    File.Delete(Path.Combine(monikaPath, "gift.rpy"));
                                File.Delete(e.FullPath);
                            } catch (Exception ex) {
                                Console.WriteLine($"Ошибка при распаковке: {ex.Message}");
                            }
                        }
                    });
                }
            };
            watcher.Changed += (s, e) => {
                Log($"... Изменен: {e.FullPath}");
            };
            watcher.Renamed += (s, e) => {
                Log($">>> Переименован: {e.FullPath}");
            };
            watcher.EnableRaisingEvents = true;
        }
        // Метод для проверки, не занят ли файл другим процессом
        private static bool WaitForFile(string path) {
            int attempts = 10;
            while (attempts > 0) {
                try {
                    using var fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    return true; // Файл свободен
                } catch (IOException) {
                    attempts--;
                    Thread.Sleep(500); // Ждем полсекунды перед следующей попыткой
                }
            }
            return false;
        }
        private void StartMonitoring() {
            _watcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));

            _watcher.EventArrived += (s, e) => {
                string newProcess = e.NewEvent.Properties["ProcessName"].Value.ToString();
                _appLookup.TryGetValue(newProcess, out string label);

                if (label != null) {
                    if (!_isTyping) {
                        // Прыгаем в UI поток для реакции
                        Dispatcher.Invoke(() => {
                            filePath = Path.Combine(scriptsPath, GetDialogLang(), "progs.rpy");
                            RunLabel(label);
                        });
                    }
                }
            };
            _watcher.Start();
        }
        public void StartBunServer() {
            _server.Start(Path.Combine(mainPath, "cai-bridge.exe"));
        }
        public void StartHttpServer() {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:7878/"); // Слушаем этот порт
            listener.Start();

            Task.Run(async () => {
                while (true) {
                    var context = await listener.GetContextAsync();
                    var request = context.Request;

                    if (request.HttpMethod == "POST") {
                        using (var reader = new System.IO.StreamReader(request.InputStream)) {
                            string data = reader.ReadToEnd(); // Получаем URL от расширения
                            if (!_isTyping)
                                Dispatcher.Invoke(() => ReactToBrowser(data));
                        }
                    }
                    context.Response.Close();
                }
            });
        }
        private void ReactToBrowser(string jsonData) {
            var data = JsonSerializer.Deserialize<BrowserData>(jsonData);

            // 1. Проверяем сначала поисковый запрос (если он есть)
            if (!string.IsNullOrEmpty(data.google) && settings.Parsing["Google"]) {
                string queryLower = data.google.ToLower();
                Log("Google query: " + queryLower);
                // Ищем совпадения в Keywords из твоего JSON
                _googleLookup.TryGetValue(queryLower, out string reaction);
                if (reaction != null) {
                    filePath = Path.Combine(scriptsPath, GetDialogLang(), "google.rpy");
                    RunLabel(reaction);
                    return;
                }
            }
            if (!string.IsNullOrEmpty(data.youtube) && settings.Parsing["Youtube"]) {
                string queryLower = data.youtube.ToLower();
                Log("Youtube query: " + queryLower);
                // Ищем совпадения в Keywords из твоего JSON
                _youtubeLookup.TryGetValue(queryLower, out string reaction);
                if (reaction != null) {
                    filePath = Path.Combine(scriptsPath, GetDialogLang(), "youtube.rpy");
                    RunLabel(reaction);
                    return;
                }
            }

            _urlLookup.TryGetValue(data.url.ToLower(), out string label);
            // 2. Если поискового запроса нет или ничего не нашли, проверяем просто URL
            if (label != null && settings.Parsing["Sites"]) {
                Log("Reacting to URL: " + data.url);
                filePath = Path.Combine(scriptsPath, GetDialogLang(), "sites.rpy");
                RunLabel(label);
            }
        }
        // Важно: Останавливаем слежку при закрытии окна
        protected override void OnClosed(EventArgs e) {
            _watcher?.Stop();
            _watcher?.Dispose();
            base.OnClosed(e);
        }

        void createContextMenus() {
            // 1. Create the ContextMenuStrip
            ContextMenu contextMenuStrip1 = new ContextMenu();

            contextMenuStrip1.Background = new SolidColorBrush(Color.FromRgb(235, 230, 244));
            contextMenuStrip1.Foreground = new SolidColorBrush(Color.FromRgb(187, 85, 153));
            contextMenuStrip1.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 189, 225));
            contextMenuStrip1.BorderThickness = new Thickness(4);
            contextMenuStrip1.FontFamily = new FontFamily("My Font");
            contextMenuStrip1.FontSize = 15;

            // 2. Create and add menu items

            MenuItem AiItem = new MenuItem() { Header = Application.Current.TryFindResource("loc_Context_Say") };

            MenuItem wearItem = new MenuItem() { Header = Application.Current.TryFindResource("loc_Context_Appearance") };
            MenuItem setItem = new MenuItem() { Header = Application.Current.TryFindResource("loc_Context_Settings") };
            MenuItem quitItem = new MenuItem() { Header = Application.Current.TryFindResource("loc_Context_Exit") };


            if (isMonikaHere) {
                contextMenuStrip1.Items.Add(wearItem);
                if (settings.AI)
                    contextMenuStrip1.Items.Add(AiItem);
                contextMenuStrip1.Items.Add(setItem);
            }
            contextMenuStrip1.Items.Add(quitItem);

            AiItem.Click += (s, e) => {
                _isTyping = true;
                SayAIWindow aIWindow = new SayAIWindow();
                if (aIWindow.ShowDialog() == true) {
                    SayToAIAsync(aIWindow.Text);
                }
            };
            wearItem.Click += (s, e) => {
                List<string> acsList = new List<string>();
                if (sprite.acs.Count > 3)
                    acsList = new List<string>(sprite.acs.Take(Math.Max(0, sprite.acs.Count - 3)));
                WearSelectWindow wearSelectWindow = new WearSelectWindow(hairType, currWear, currRibbon, currHat, currAhoge, acsList);
                wearSelectWindow.Owner = this;
                wearSelectWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (wearSelectWindow.ShowDialog() == true) {
                    hairType = wearSelectWindow.CurrentHair;
                    currWear = wearSelectWindow.CurrentWear;
                    currHat = wearSelectWindow.CurrentHat;
                    currRibbon = wearSelectWindow.CurrentRibbon;
                    currAhoge = wearSelectWindow.CurrentAhoge;

                    _engine.Variables["player"] = settings.Username;

                    wearPath = System.IO.Path.Combine(monikaPath, "c", currWear);
                    CleanupCache();
                    sprite.acs.Clear();
                    sprite.acs.AddRange(wearSelectWindow.CurrAcs);
                    sprite.acs.Add(currAhoge);
                    sprite.acs.Add(currHat);
                    sprite.acs.Add(currRibbon);
                    setPose(idle_pose);
                }
            };
            setItem.Click += (s, e) => {
                SettingsWindow settingsWindow = new SettingsWindow(settings);
                settingsWindow.Owner = this;
                settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                settingsWindow.RequestMainWindowResize += (newSize) => {
                    ApplySmoothScale(newSize);
                };
                settingsWindow.RequestMainWindowLang += (newSize) => {

                    ChangeLanguage();
                    AiItem.Header = Application.Current.TryFindResource("loc_Context_Say");

                    wearItem.Header = Application.Current.TryFindResource("loc_Context_Appearance");
                    setItem.Header = Application.Current.TryFindResource("loc_Context_Settings");
                    quitItem.Header = Application.Current.TryFindResource("loc_Context_Exit");
                };
                if (settingsWindow.ShowDialog() == true) {
                    setPose(idle_pose);
                    SaveSettings();
                }
            };
            quitItem.Click += (s, e) => {
                SaveSettings();

                if (!_isTyping) {
                    Random random = new Random();
                    filePath = Path.Combine(scriptsPath, GetDialogLang(), "goodbye.rpy");
                    RunLabel("__random__");
                }
            };

            this.ContextMenu = contextMenuStrip1;
        }
        private async Task TypeText(string text, string pose, bool isAi = false) {
            UpdateVariables();
            foreach (var v in _engine.Variables) {
                text = text.Replace("[" + v.Key + "]", v.Value?.ToString());
            }

            var tcs = new TaskCompletionSource<bool>();
            await this.Dispatcher.Invoke(async () => {
                saybox.Visibility = Visibility.Visible;
                setPose(pose);

                StringBuilder sb = new StringBuilder();

                foreach (char c in text) {
                    if (_cancelTyping) {
                        sayboxText.Text = text;
                        break;
                    }
                    sb.Append(c);
                    sayboxText.Text = sb.ToString();

                    if (".!?~".Contains(c))
                        await Task.Delay(1000);
                    else
                        await Task.Delay(30);
                }
                if (isAi) _isTyping = false;
                _cancelTyping = false;
            });
        }


        // Основной метод запуска по имени
        private async Task RunLabel(string labelName) {
            try {

                var tcs = new TaskCompletionSource<bool>();
                Action onFinished = null;
                onFinished = () => {
                    _engine.OnScriptFinished -= onFinished;
                    tcs.SetResult(true);
                };
                _engine.OnScriptFinished += onFinished;

                if (filePath != null) {
                    string content = File.ReadAllText(filePath);
                    _engine.LoadContent(content);
                }

                if (labelName == "__random__") {
                    labelName = _engine.GetRandomLabel();
                    if (labelName == "firstrun" && labelName == "__changepc__") {
                        labelName = _engine.GetRandomLabel();
                    }
                }

                if (labelName != null && _engine.Labels.ContainsKey(labelName)) {
                    Log($"[Engine] Запуск метки: {labelName}");
                    _isTyping = true;
                    _engine.CurrentIndex = _engine.Labels[labelName];

                    _cancelTyping = false;
                    await _engine.ExecuteNext();
                    await tcs.Task;
                } else {
                    Log($"[Error] Метка {labelName} не найдена!");
                }
            } catch (Exception ex) { Log("Error running label: " + ex.Message); }
        }

        // Дополнительный метод для запуска по номеру (индексу)
        private async Task RunLabelAsync(int labelIndex) {
            if (filePath != null) {
                string content = File.ReadAllText(filePath);
                _engine.LoadContent(content);
            }

            string labelName = _engine.GetLabelByIndex(labelIndex);
            if (labelName != null) {
                await RunLabel(labelName);
            } else {
                Log($"[Error] Метка под номером {labelIndex} не существует в текущем скрипте.");
            }
        }
        private void ShowMenu(int menuIndent) {
            Dispatcher.Invoke(() => {
                MenuStackPanel.Children.Clear();
                MenuStackPanel.Visibility = Visibility.Visible;

                int i = _engine.CurrentIndex + 1;
                while (i < _engine.Commands.Count && _engine.Commands[i].IndentLevel > menuIndent) {
                    var cmd = _engine.Commands[i];
                    // Ищем только заголовки кнопок (отступ menu + 4)
                    if (cmd.Type == CommandType.Speech && cmd.IndentLevel == menuIndent + 4) {
                        Button b = new Button {
                            Content = cmd.Text.TrimEnd(':'),
                            Margin = new Thickness(5),
                            Padding = new Thickness(20, 5, 20, 5)
                        };

                        int branchIdx = i + 1;

                        b.Click += async (s, e) => {
                            MenuStackPanel.Visibility = Visibility.Collapsed;
                            _engine.CurrentIndex = branchIdx;

                            await _engine.ExecuteNext();
                            _engine.SkipBlock(menuIndent);
                            await _engine.ExecuteNext();
                        };
                        MenuStackPanel.Children.Add(b);
                    }
                    i++;
                }
            });
        }
        private async void InputField_KeyDown(object sender, KeyEventArgs e) {
            Debug.WriteLine("key: " + e.Key);
            if ((e.Key == Key.Enter || e.Key == Key.Return) && !string.IsNullOrWhiteSpace(InputField.Text)) {
                string result = InputField.Text;
                InputContainer.Visibility = Visibility.Collapsed;

                // Сохраняем введенный текст в переменные движка
                _engine.Variables[_pendingVarName] = result;

                // Продолжаем выполнение скрипта
                await Task.Run(async () => await _engine.ExecuteNext());
            }
        }
        async Task SayToAIAsync(string text) {
            using var client = new HttpClient();
            //var payload = new { message = text };
            //var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.GetStringAsync($"http://localhost:{settings.port}/chat?text={Uri.EscapeDataString(text)}");
            //var responseString = await response.Content.ReadAsStringAsync();
            //response = response.Replace(". ", "\n").Replace("! ", "\n").Replace("? ", "\n");
            response = "label start:\n" + response + "\nreturn";

            _engine.LoadContent(response);

            Log($"[Engine] Запуск метки: start");
            _isTyping = true;
            _engine.CurrentIndex = _engine.Labels["start"];

            _cancelTyping = false;
            Task.Run(async () => await _engine.ExecuteNext());
            Log($"Получен ответ: {response}");
        }

        void setPose(string pose) {
            try {
                decode(pose); // Заполняем объект sprite путями

                this.Dispatcher.Invoke(() => {
                    // Статика и база
                    SetLayer(ImgChair, $"{monikaPath}/t/chair-def.png");
                    if (isMonikaHere) {
                        SetLayer(ImgHairBack, $"{sprite.hair}-back.png");

                        SetLayer(ImgBody, $"{sprite.body}-0.png");
                        SetLayer(ImgC_Body, $"{sprite.c_body}-0.png");

                        // Руки (0 слой)
                        SetLayer(ImgArmL, $"{sprite.leftArm}-0.png");
                        SetLayer(ImgArmR, $"{sprite.rightArm}-0.png");
                        SetLayer(ImgC_ArmL, $"{sprite.c_leftArm}-0.png");
                        SetLayer(ImgC_ArmR, $"{sprite.c_rightArm}-0.png");

                        SetLayer(ImgHead, $"{sprite.head}-head.png");
                    }
                    SetLayer(ImgTable, $"{monikaPath}/t/table-def.png");
                    if (isMonikaHere) {
                        SetLayer(ImgTableS, $"{monikaPath}/t/table-def-s.png");

                        // Руки (5 слой - над столом)
                        SetLayer(ImgArmL_Top, $"{sprite.leftArm}-5.png");
                        SetLayer(ImgArmR_Top, $"{sprite.rightArm}-5.png");
                        SetLayer(ImgC_ArmL_Top, $"{sprite.c_leftArm}-5.png");
                        SetLayer(ImgC_ArmR_Top, $"{sprite.c_rightArm}-5.png");

                        SetLayer(ImgBody1, $"{sprite.body}-1.png");
                        SetLayer(ImgC_Body1, $"{sprite.c_body}-1.png");

                        // Волосы и лицо
                        SetLayer(ImgHairFront, $"{sprite.hair}-front.png");
                        SetLayer(ImgEyes, $"{sprite.eyes}.png");
                        SetLayer(ImgNose, $"{sprite.nose}.png");
                        SetLayer(ImgEyebrows, $"{sprite.eyebrows}.png");
                        SetLayer(ImgMouth, $"{sprite.mouth}.png");
                        SetLayer(ImgTears, $"{sprite.tears}.png");
                        SetLayer(ImgBlush, $"{sprite.blush}.png");

                        SetLayer(ImgArmL_Top2, $"{sprite.leftArm}-10.png");
                        SetLayer(ImgArmR_Top2, $"{sprite.rightArm}-10.png");
                        SetLayer(ImgC_ArmL_Top2, $"{sprite.c_leftArm}-10.png");
                        SetLayer(ImgC_ArmR_Top2, $"{sprite.c_rightArm}-10.png");

                        UpdateAccessories();
                    }

                    if (settings.Filter) {
                        Color color = GetColorAtPoint(_dayNightGradient, new Point(graientPosition, 0));
                        monikaShader.FilterColor = color;
                        MonikaContainer.Effect = monikaShader;
                    } else {
                        MonikaContainer.Effect = null;
                    }
                });
                CleanupCache();
            } catch (Exception e) {
                Log("Ошибка в setPose: " + e.Message);
            }
        }
        private void AddAcsToLayer(ItemsControl layerContainer, string path) {
            if (string.IsNullOrEmpty(path) || path.Contains("null")) return;

            AlphaTapImage img = new AlphaTapImage {
                Source = GetImage(path, true),
                Stretch = Stretch.Uniform
            };
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.Fant);

            layerContainer.Items.Add(img);
        }
        void UpdateAccessories() {
            ItemsPreAcs.Items.Clear();
            ItemsBseAcs.Items.Clear();
            ItemsBbhAcs.Items.Clear();
            ItemsAfhAcs.Items.Clear();
            ItemsAseAcs.Items.Clear();
            ItemsPstAcs.Items.Clear();

            foreach (var acsName in sprite.acs) {
                if (string.IsNullOrEmpty(acsName) || acsName == "null") continue;

                string jsonPath = Path.Combine(acsPath, acsName, "data.json");
                if (!File.Exists(jsonPath)) continue;

                try {
                    string jsonString = File.ReadAllText(jsonPath);
                    using (JsonDocument doc = JsonDocument.Parse(jsonString)) {
                        string layerName = doc.RootElement.GetProperty("layer").ToString();
                        string poseKey = main_pose[0].ToString();

                        if (doc.RootElement.TryGetProperty("posemap", out JsonElement posemap) &&
                            posemap.TryGetProperty(poseKey, out JsonElement imgName)) {
                            string fullPath = Path.Combine(acsPath, acsName, imgName.ToString() + ".png");

                            switch (layerName) {
                                case "PRE_ACS": AddAcsToLayer(ItemsPreAcs, fullPath); break;
                                case "BSE_ACS": AddAcsToLayer(ItemsBseAcs, fullPath); break;
                                case "BBH_ACS": AddAcsToLayer(ItemsBbhAcs, fullPath); break;
                                case "AFH_ACS": AddAcsToLayer(ItemsAfhAcs, fullPath); break;
                                case "ASE_ACS": AddAcsToLayer(ItemsAseAcs, fullPath); break;
                                case "PST_ACS": AddAcsToLayer(ItemsPstAcs, fullPath); break;
                            }
                        }
                    }
                } catch (Exception ex) { Log($"Ошибка загрузки аксессуара {acsName}: {ex.Message}"); }
            }
        }
        private void SetLayer(Image layer, string path) {
            if (string.IsNullOrEmpty(path) || path.EndsWith("null.png") || path.Contains("null")) {
                layer.Source = null;
            } else {
                layer.Source = GetImage(path, true);
            }
        }
        void UpdateShaderColor() {
            graientPosition = ConvertRange(0, 24, 0, 2, DateTime.Now.Hour);

            Color color = GetColorAtPoint(_dayNightGradient, new Point(graientPosition, 0));

            monikaShader.FilterColor = settings.Filter
                ? color
                : Color.FromRgb(255, 255, 255);
        }
        void decode(string pose) {
            if (pose.Contains("[")) { pose = pose.Replace("[", "").Replace("]", ""); }
            if (pose.Contains(",")) { pose = pose.Replace(",", ""); }
            if (pose == "m") return;
            if (pose == "null") return;
            if (pose.Contains(' ')) { pose = pose.Split(' ')[1]; }
            main_pose = pose;
            string body = pose[0].ToString();
            string eyes = pose[1].ToString();
            string brows = pose[2].ToString();
            string blush = "";
            string tears = "";
            string sweat = "";
            string mouth = pose[pose.Length - 1].ToString();
            if (pose.Length > 4) {
                string extras = pose.Substring(3, pose.Length - 4);

                int index = 0;
                while (index < extras.Length) {
                    char prefix = extras[index];
                    index++;

                    switch (prefix) {
                        case 'b':
                            if (index < extras.Length) {
                                char type = extras[index];
                                blush = "b" + type.ToString();
                                index++;
                            }
                            break;

                        case 't':
                            if (index < extras.Length) {
                                char type = extras[index];
                                tears = "t" + type.ToString();
                                index++;
                            }
                            break;

                        case 's':
                            if (index < extras.Length) {
                                string type = extras[index].ToString() + extras[index + 1].ToString();

                                sweat = "s" + type.ToString();
                                index++;
                            }
                            break;

                    }
                }
            }


            string left = NUM_ARMS[int.Parse(arms[int.Parse(body) - 1][1].ToString())];
            string right = NUM_ARMS[int.Parse(arms[int.Parse(body) - 1][2].ToString())];
            string b = (body == "5") ? "leaning-" : "";
            string lword = (body == "5") ? "leaning-def-" : "";
            string ribbonlword = (body == "5") ? "5" : "0";

            string browKey = jsonObject["eyebrows"]?[brows] != null ? brows : "u";
            string eyeKey = jsonObject["eyes"]?[eyes] != null ? eyes : "e";
            string mouthKey = jsonObject["mouth"]?[mouth] != null ? mouth : "a";

            sprite.hair = Path.Combine(hairPath, hairType,
                $"hair-{lword}{hairType}");

            sprite.head = Path.Combine(bodyPath,
                $"body-{b}def");
            sprite.body = Path.Combine(bodyPath,
                $"body-{b}def");
            sprite.leftArm = Path.Combine(bodyPath,
                $"arms-{left}");
            sprite.rightArm = Path.Combine(bodyPath,
                $"arms-{right}");

            sprite.c_body = Path.Combine(wearPath,
                $"body-{b}def");
            sprite.c_leftArm = Path.Combine(wearPath,
                $"arms-{left}");
            sprite.c_rightArm = Path.Combine(wearPath,
                $"arms-{right}");

            sprite.eyes = Path.Combine(facePath, $"face-{lword}eyes-{jsonObject["eyes"][eyeKey]}");
            sprite.eyebrows = Path.Combine(facePath, $"face-{lword}eyebrows-{jsonObject["eyebrows"][browKey]}");
            sprite.mouth = Path.Combine(facePath, $"face-{lword}mouth-{jsonObject["mouth"][mouthKey]}");

            sprite.blush = Path.Combine(facePath,
                $"face-{lword}blush-{(jsonObject["blush"]?[blush] ?? "null")}");
            sprite.tears = Path.Combine(facePath,
                $"face-{lword}tears-{(jsonObject["tears"]?[tears] ?? "null")}");
            sprite.sweat = Path.Combine(facePath,
                $"face-{lword}sweatdrop-{(jsonObject["sweat"]?[sweat] ?? "null")}");
            sprite.nose = Path.Combine(facePath,
                $"face-{lword}nose-def");

        }

        void LoadAcs(DrawingContext context, string layer) {
            foreach (var path in sprite.acs) {
                if (!String.IsNullOrEmpty(path) && path != "null")
                    context.DrawImage(GetImage(GetAcs(path, layer), true), new Rect(0, 0, 1280, 850));
            }
        }
        private Dictionary<string, JsonDocument> _acsJsonCache = new();

        JsonDocument GetAcsJson(string path) {
            if (_acsJsonCache.TryGetValue(path, out var doc))
                return doc;

            if (!File.Exists(path))
                return null;

            var newDoc = JsonDocument.Parse(File.ReadAllText(path));
            _acsJsonCache[path] = newDoc;
            return newDoc;
        }

        string GetAcs(string name, string layer, int arm = -1) {
            string result = result = Path.Combine(monikaPath, "null.png");

            string jsonPath = Path.Combine(acsPath, name, "data.json"); // Или другое имя
            if (File.Exists(jsonPath)) {
                JsonDocument doc = GetAcsJson(jsonPath);
                if (doc.RootElement.TryGetProperty("layer", out JsonElement layerElement)) {
                    if (layerElement.ToString() == layer)
                        if (doc.RootElement.TryGetProperty("posemap", out JsonElement posemapElement))
                            if (posemapElement.TryGetProperty(main_pose[0].ToString(), out JsonElement poseElement))
                                result = Path.Combine(acsPath, name, poseElement.ToString() + ".png");
                            else
                                result = Path.Combine(monikaPath, "null.png");
                }
                //}
            }

            return result;
        }

        private Dictionary<string, (BitmapSource Image, DateTime LastUsed)> _smartCache
            = new Dictionary<string, (BitmapSource, DateTime)>();
        public BitmapSource GetImage(string fileName, bool cache = false) {
            fileName = Path.GetFullPath(fileName).ToLower();

            if (_smartCache.TryGetValue(fileName, out var cachedItem)) {
                _smartCache[fileName] = (cachedItem.Image, DateTime.Now);
                return cachedItem.Image;
            }

            if (!File.Exists(fileName)) fileName = Path.Combine(monikaPath, "null.png");

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(fileName);
            //bi.DecodePixelWidth = 1000;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            bi.Freeze();

            if (cache) {
                _smartCache[fileName] = (bi, DateTime.Now);
            }
            return bi;
        }
        private void CleanupCache(int minutesUnused = 5) {
            var now = DateTime.Now;
            var keysToRemove = _smartCache
                .Where(kvp => (now - kvp.Value.LastUsed).TotalMinutes > minutesUnused)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove) {
                _smartCache.Remove(key);
            }

            if (keysToRemove.Count > 0) {
                Log($"[Cache] Очищено объектов: {keysToRemove.Count}. Текущий размер кэша: {_smartCache.Count}");
                GC.Collect(1, GCCollectionMode.Optimized);
            }
        }
        public static Color GetColorAtPoint(LinearGradientBrush brush, Point point) {
            // 1. Calculate the normalized distance (t) along the gradient vector
            Vector startToEnd = brush.EndPoint - brush.StartPoint;
            Vector startToPoint = point - brush.StartPoint;

            double lengthSquared = startToEnd.LengthSquared;
            if (lengthSquared == 0) return Colors.Transparent; // Avoid division by zero

            // t is the projection of the point onto the gradient vector, normalized by the vector's length squared
            double t = Vector.CrossProduct(startToPoint, startToEnd) / lengthSquared;

            // Apply clamping based on the SpreadMethod (default is Pad)
            if (brush.SpreadMethod == GradientSpreadMethod.Pad) {
                t = Math.Max(0.0, Math.Min(1.0, t));
            }
            // Additional logic needed for Reflect or Repeat SpreadMethods

            // 2. Find the relevant gradient stops
            var stops = brush.GradientStops.OrderBy(s => s.Offset).ToList();
            if (stops.Count == 0) return Colors.Transparent;
            if (t <= 0.0) return stops.First().Color;
            if (t >= 1.0) return stops.Last().Color;

            GradientStop stopBefore = stops.Last(s => s.Offset <= t);
            GradientStop stopAfter = stops.First(s => s.Offset >= t);

            // 3. Interpolate between the two bounding stops
            double segmentLength = stopAfter.Offset - stopBefore.Offset;
            double segmentPos = t - stopBefore.Offset;
            double percentage = segmentPos / segmentLength;

            return InterpolateColors(stopBefore.Color, stopAfter.Color, percentage);
        }

        private static Color InterpolateColors(Color color1, Color color2, double percentage) {
            int a = (int)((color1.A * (1 - percentage)) + (color2.A * percentage));
            int r = (int)((color1.R * (1 - percentage)) + (color2.R * percentage));
            int g = (int)((color1.G * (1 - percentage)) + (color2.G * percentage));
            int b = (int)((color1.B * (1 - percentage)) + (color2.B * percentage));

            return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
        }

        private static readonly object _lock = new object();
        public void Log(string message) {
            Task.Run(() => {
                lock (_lock) {
                    try {
                        string logLine = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
                        Debug.Write(logLine);
                        File.AppendAllText(logPath, logLine);
                    } catch { /* Ошибки лога не должны крашить игру */ }
                }
            });
        }
        public static double ConvertRange(
    int originalStart, int originalEnd, // original range
    int newStart, int newEnd, // desired range
    int value) // value to convert
        {
            double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
            return (double)(newStart + ((value - originalStart) * scale));
        }

        public void LoadConfig(string path, Dictionary<string, string> lookup) {
            try {
                string jsonString = File.ReadAllText(path);
                var reactions = JsonSerializer.Deserialize<List<ProcessReaction>>(jsonString);

                lookup.Clear();
                foreach (var reaction in reactions) {
                    foreach (var app in reaction.Apps) {
                        lookup[app.ToLower()] = reaction.Label;
                    }
                }
            } catch (Exception ex) {
                System.Windows.MessageBox.Show($"Ошибка загрузки конфига: {ex.Message}");
            }
        }

        public void SaveSettings() {
            settings.PCname = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            settings.CurrentHair = hairType;
            settings.CurrentWear = currWear;
            settings.CurrentHat = currHat;
            settings.CurrentRibbon = currRibbon;
            settings.CurrentAhoge = currAhoge;
            if (sprite.acs.Count > 3)
                settings.Acs = new List<string>(sprite.acs.Take(Math.Max(0, sprite.acs.Count - 3)));
            if (settings.AutoStart) {
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                reg.SetValue("MOD_AutoStart", exePath);
            } else {
                RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                reg.DeleteValue("MOD_AutoStart", false);
            }


            SaveManager.Save(settings);
        }
        public void LoadSettings() {
            settings = SaveManager.Load();
            hairType = settings.CurrentHair;
            currWear = settings.CurrentWear;
            wearPath = System.IO.Path.Combine(monikaPath, "c", currWear);
            currHat = settings.CurrentHat;
            currRibbon = settings.CurrentRibbon;
            currAhoge = settings.CurrentAhoge;
            sprite.acs.Clear();
            sprite.acs.AddRange(settings.Acs);
            sprite.acs.Add(currAhoge);
            sprite.acs.Add(currHat);
            sprite.acs.Add(currRibbon);

            ApplySmoothScale(settings.Scale);


            ChangeLanguage();
        }
        void ChangeLanguage(string lang) {
            settings.Lang = lang;
            ChangeLanguage();
        }
        void ChangeLanguage() {

            var languages = new List<LanguageModel>();

            string langPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "langs", $"{settings.Lang}.xaml");
            /*
            try
            {
                var dict = new ResourceDictionary { Source = new Uri(langPath, UriKind.Absolute) };
                if (dict.Contains("LanguageName"))
                {
                    language = new LanguageModel
                    {
                        Name = dict["LanguageName"].ToString(),
                        Uri = dict.Source,
                        CultureCode = Path.GetFileNameWithoutExtension(langPath)
                    };
                }
            }
            catch { /* Ошибка в XAML файле пользователя — пропускаем  }
            */

            string fullPath = Path.Combine(langPath);

            if (File.Exists(fullPath)) {
                using (FileStream fs = new FileStream(fullPath, FileMode.Open)) {
                    ResourceDictionary dict = (ResourceDictionary)System.Windows.Markup.XamlReader.Load(fs);
                    dict.Source = new Uri(fullPath, UriKind.Absolute);

                    var appResources = Application.Current.Resources.MergedDictionaries;

                    var oldDict = appResources.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.ToLower().Contains("langs"));
                    if (oldDict != null) appResources.Remove(oldDict);

                    appResources.Add(dict);
                }
            } else {
                return;
            }
            createContextMenus();
        }
        void UpdateVariables() {
            _engine.Variables["player"] = settings.Username;
            _engine.Variables["minute"] = DateTime.Now.Minute;
            _engine.Variables["hour"] = DateTime.Now.Hour;
            _engine.Variables["day"] = DateTime.Now.Day;
            _engine.Variables["month"] = DateTime.Now.Month;
            _engine.Variables["pcname"] = Environment.MachineName;
            _engine.Variables["username"] = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            _engine.Variables["chances"] = hm_chances;
            _engine.Variables["word"] = hm_word;
            _engine.Variables["guesses"] = hm_guesses;
            _engine.Variables["hm_hint"] = hm_char;


        }
        private void ApplySmoothScale(double targetScale) {
            // Анимация масштаба
            DoubleAnimation anim = new DoubleAnimation(targetScale, TimeSpan.FromSeconds(0.2));
            anim.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };

            // Подписываемся на обновление каждого кадра анимации, чтобы окно двигалось вслед за размером
            //anim.CurrentTimeInvalidated += (s, e) => UpdatePosition();
            //anim.Completed += (s, e) => UpdatePosition_();
            MainScale.ScaleX = targetScale;
            MainScale.ScaleY = targetScale;
            //MainScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            //MainScale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);



        }

        private void UpdatePosition() {
            // Получаем рабочую область экрана (без учета панели задач)
            var desktopWorkingArea = SystemParameters.WorkArea;

            // Устанавливаем положение окна так, чтобы его правый нижний угол совпадал с углом экрана
            this.Left = desktopWorkingArea.Right - this.ActualWidth;
            this.Top = desktopWorkingArea.Bottom - this.ActualHeight;
        }


        private void UpdatePosition_() {
            // Получаем рабочую область экрана (без учета панели задач)
            var desktopWorkingArea = SystemParameters.WorkArea;

            // Устанавливаем положение окна так, чтобы его правый нижний угол совпадал с углом экрана
            var Left = desktopWorkingArea.Right - this.ActualWidth;
            var Top = desktopWorkingArea.Bottom - this.ActualHeight;

            DoubleAnimation animL = new DoubleAnimation(Left, TimeSpan.FromSeconds(0.1));
            DoubleAnimation animT = new DoubleAnimation(Top, TimeSpan.FromSeconds(0.1));
            animL.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            animT.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };

            this.BeginAnimation(LeftProperty, animL);
            this.BeginAnimation(TopProperty, animT);
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e) {
            /*
            // Меняем масштаб: вверх — больше, вниз — меньше
            if (e.Delta > 0) _currentScale += 0.1;
            else _currentScale -= 0.1;

            // Ограничители, чтобы Моника не исчезла и не заняла весь экран
            if (_currentScale < 0.5) _currentScale = 0.5;
            if (_currentScale > 3.0) _currentScale = 3.0;

            ApplySmoothScale(_currentScale);*/
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            // Получаем рабочую область экрана (без учета панели задач)
            var desktopWorkingArea = SystemParameters.WorkArea;

            // Устанавливаем положение окна так, чтобы его правый нижний угол совпадал с углом экрана
            this.Left = desktopWorkingArea.Right - this.ActualWidth;
            this.Top = desktopWorkingArea.Bottom - this.ActualHeight;
            //MyOutline.UpdateScale(e.NewSize.Width);
        }
        public string GetDialogLang() {
            // Пытаемся найти ключ в ресурсах приложения
            if (Application.Current.TryFindResource("DialogPath") is string path) {
                if (Directory.Exists(Path.Combine(scriptsPath, path)) && Directory.GetFiles(Path.Combine(scriptsPath, path)).Length >= 7)
                    return path; // Вернет "dialogs/ru/" или "dialogs/en/"
                else
                    return "ru"; // Если папки нет, возвращаем значение по умолчанию
            }
            return "ru"; // Значение по умолчанию
        }
        public static bool CheckForInternetConnection() {
            try {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com")) {
                    return true;
                }
            } catch {
                return false;
            }
        }

        private void sayboxText_SizeChanged(object sender, SizeChangedEventArgs e) {
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020, WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        // Вспомогательный метод для переключения режима
        private void SetClickThrough(bool isClickThrough) {
            var hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

            if (isClickThrough) {
                // Добавляем флаг "прозрачности" для кликов
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            } else {
                // Убираем флаг (битовая маска NOT)
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle & ~WS_EX_TRANSPARENT);
            }
        }

    }
}