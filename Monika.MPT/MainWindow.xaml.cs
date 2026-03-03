using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using static System.Net.Mime.MediaTypeNames;



namespace Monika.MPT
{
    class MonikaSprite
    {
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
    public partial class MainWindow : Window
    {
        MonikaSprite sprite = new MonikaSprite();

        static string currWear = "blackdress";
        public string hairType = "def";
        public string currAhoge = "null";

        static string mainPath = AppDomain.CurrentDomain.BaseDirectory;

        static string monikaPath = System.IO.Path.Combine(mainPath, "monika");
        static string facePath = System.IO.Path.Combine(monikaPath, "f");
        static string bodyPath = System.IO.Path.Combine(monikaPath, "b");
        static string wearPath = System.IO.Path.Combine(monikaPath, "c", currWear);
        static string hairPath = System.IO.Path.Combine(monikaPath, "h");

        public string leaningWord = "leaning";
        public string nullPath = "null";


        string idle_pose = "5eua";
        int pos = 5;
        int eye = 0;
        int brow = 1;
        int mon = 0;
        int tear = 0;
        int blus = 0;
        string main_pose = "";


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
        string[] eyebrowKeys = { "f", "u", "k", "s", "t" };
        string[] eyeKeys = { "e", "w", "s", "t", "c", "r", "l", "h", "d", "k", "n", "f", "m", "g" };
        string[] mouthKeys = { "a", "b", "c", "d", "o", "u", "w", "x", "p", "t" };
        string[] armKeys = { "1", "2", "3", "4", "5", "6", "7" };

        string[] blushKeys = { "", "bl", "bs", "bf" };
        string[] tearKeys = { "", "ts", "td", "tp", "tu" };
        string[] sweatKeys = { "", "sdl", "sdr" };

        JsonObject jsonObject;

        public MainWindow()
        {
            InitializeComponent();
            InitDrawing();

            jsonObject = JsonNode.Parse(File.ReadAllText(System.IO.Path.Combine(monikaPath, "sprite_map.json"))).AsObject();
            updateTexts();
            textBox.Text = idle_pose;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Храним слои в памяти как DrawingGroup
        private DrawingGroup _monikaLayers = new DrawingGroup();
        private DrawingImage _finalImageSource;

        // Инициализация при старте
        private void InitDrawing()
        {
            _finalImageSource = new DrawingImage(_monikaLayers);
            monika.Source = _finalImageSource; // Привязываем один раз!
        }
        void setPose(string pose)
        {
            wearPath = System.IO.Path.Combine(monikaPath, "c", currWear);

            try
            {
                decode(pose);

                var imagesToCache = new List<string>()
            {
                $"{monikaPath}/t/chair-def.png",
                $"{sprite.hair}-back.png",
                $"{sprite.body}-0.png",
                $"{sprite.c_body}-0.png",
                $"{sprite.leftArm}-0.png",
                $"{sprite.rightArm}-0.png",
                $"{sprite.head}-head.png",
                $"{monikaPath}/t/table-def.png",
                $"{monikaPath}/t/table-def-s.png",
                $"{sprite.leftArm}-5.png",
                $"{sprite.rightArm}-5.png",
                $"{sprite.body}-1.png",
                $"{sprite.hair}-front.png",
                $"{sprite.ribbon}.png",

                $"{sprite.leftArm}-10.png",
                $"{sprite.rightArm}-10.png",
            };
                // Предзагружаем их в кэш (в фоне)
                //foreach (var path in imagesToCache) GetImage(path, true);

                // 2. Возвращаемся в UI поток только для финальной сборки
                this.Dispatcher.Invoke(() =>
                {
                    var visual = new DrawingVisual();
                    using (var context = _monikaLayers.Open())
                    {
                        var rect = new Rect(0, 0, 1280, 850);

                        context.DrawImage(GetImage($"{monikaPath}/t/chair-def.png"), rect);
                        context.DrawImage(GetImage($"{sprite.hair}-back.png"), rect);

                        context.DrawImage(GetImage($"{sprite.body}-0.png"), rect);
                        context.DrawImage(GetImage($"{sprite.c_body}-0.png"), rect);

                        context.DrawImage(GetImage($"{sprite.leftArm}-0.png"), rect);
                        context.DrawImage(GetImage($"{sprite.rightArm}-0.png"), rect);
                        context.DrawImage(GetImage($"{sprite.c_leftArm}-0.png"), rect);
                        context.DrawImage(GetImage($"{sprite.c_rightArm}-0.png"), rect);

                        context.DrawImage(GetImage($"{sprite.head}-head.png"), rect);
                        context.DrawImage(GetImage($"{monikaPath}/t/table-def.png"), rect);
                        context.DrawImage(GetImage($"{monikaPath}/t/table-def-s.png"), rect);

                        context.DrawImage(GetImage($"{sprite.leftArm}-5.png"), rect);
                        context.DrawImage(GetImage($"{sprite.rightArm}-5.png"), rect);
                        context.DrawImage(GetImage($"{sprite.c_leftArm}-5.png"), rect);
                        context.DrawImage(GetImage($"{sprite.c_rightArm}-5.png"), rect);

                        context.DrawImage(GetImage($"{sprite.body}-1.png"), rect);
                        context.DrawImage(GetImage($"{sprite.c_body}-1.png"), rect);

                        context.DrawImage(GetImage($"{sprite.hair}-front.png"), rect);

                        context.DrawImage(GetImage($"{sprite.eyes}.png"), rect);
                        context.DrawImage(GetImage($"{sprite.nose}.png"), rect);
                        context.DrawImage(GetImage($"{sprite.eyebrows}.png"), rect);
                        context.DrawImage(GetImage($"{sprite.mouth}.png"), rect);
                        context.DrawImage(GetImage($"{sprite.tears}.png"), rect);
                        context.DrawImage(GetImage($"{sprite.blush}.png"), rect);


                        context.DrawImage(GetImage($"{sprite.leftArm}-10.png"), rect);
                        context.DrawImage(GetImage($"{sprite.rightArm}-10.png"), rect);
                        context.DrawImage(GetImage($"{sprite.c_leftArm}-10.png"), rect);
                        context.DrawImage(GetImage($"{sprite.c_rightArm}-10.png"), rect);

                    }

                    //var result = new RenderTargetBitmap(1280, 850, 96, 96, PixelFormats.Pbgra32);
                    //result.Render(visual);
                    //result.Freeze(); // Чтобы Image быстрее его проглотил


                    //bodyImg.Source = result;
                });
            }
            catch (Exception e)
            {

            }
        }
        void decode(string pose)
        {
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
            if (pose.Length > 4)
            {
                string extras = pose.Substring(3, pose.Length - 4);

                int index = 0;
                while (index < extras.Length)
                {
                    char prefix = extras[index];
                    index++;

                    switch (prefix)
                    {
                        case 'b':
                            if (index < extras.Length)
                            {
                                char type = extras[index];
                                blush = "b" + type.ToString();
                                index++;
                            }
                            break;

                        case 't':
                            if (index < extras.Length)
                            {
                                char type = extras[index];
                                tears = "t" + type.ToString();
                                index++;
                            }
                            break;

                        case 's':
                            if (index < extras.Length)
                            {
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
        private Dictionary<string, BitmapSource> _cache = new Dictionary<string, BitmapSource>();

        public BitmapSource GetImage(string fileName, bool cache = false)
        {
            // 1. Сначала ищем в словаре
            if (_cache.TryGetValue(fileName, out var existing)) return existing;

            // 2. Если нет, проверяем файл ОДИН раз
            if (!File.Exists(fileName)) fileName = Path.Combine(monikaPath, "null.png");

            // 3. Грузим и замораживаем (Freeze), чтобы не было проблем с потоками
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(fileName);
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            bi.Freeze();

            if (cache) _cache[fileName] = bi;
            return bi;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(textBox.Text.Length >= 4)
            {
                setPose(textBox.Text);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Name.Contains("pose"))
                pos -= 1;
            if (button.Name.Contains("brows"))
                brow -= 1;
            if (button.Name.Contains("eyes"))
                eye -= 1;
            if (button.Name.Contains("month"))
                mon -= 1;
            if (button.Name.Contains("tears"))
                tear -= 1;
            if (button.Name.Contains("blush"))
                blus -= 1;


            if (pos < 1) pos = 1;
            if (brow < 0) brow = 0;
            if (eye < 0) eye = 0;
            if (mon < 0) mon = 0;
            if (tear < 0) tear = 0;
            if (blus < 0) blus = 0;

            updateTexts();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Name.Contains("pose"))
                pos += 1;
            if (button.Name.Contains("brows"))
                brow += 1;
            if (button.Name.Contains("eyes"))
                eye += 1;
            if (button.Name.Contains("month"))
                mon += 1;
            if (button.Name.Contains("tears"))
                tear += 1;
            if (button.Name.Contains("blush"))
                blus += 1;


            if (pos > 7) pos = 7;
            if (brow > eyebrowKeys.Length - 1) brow = eyebrowKeys.Length-1;
            if (eye > eyeKeys.Length - 1) eye = eyeKeys.Length - 1;
            if (mon > mouthKeys.Length - 1) mon = mouthKeys.Length - 1;
            if (tear > tearKeys.Length - 1) tear = tearKeys.Length - 1;
            if (blus > blushKeys.Length - 1) blus = blushKeys.Length - 1;

            updateTexts();
        }
        void updateTexts()
        {
            browsText.Text = jsonObject["eyebrows"][eyebrowKeys[brow]].ToString().FirstLetterToUpper();
            eyesText.Text = jsonObject["eyes"][eyeKeys[eye]].ToString().FirstLetterToUpper(); 
            mouthText.Text = jsonObject["mouth"][mouthKeys[mon]].ToString().FirstLetterToUpper();
            blushText.Text = (jsonObject["blush"]?[blushKeys[blus]] ?? "null").ToString().FirstLetterToUpper();
            tearsText.Text = (jsonObject["tears"]?[tearKeys[tear]] ?? "null").ToString().FirstLetterToUpper();

            string code = pos.ToString() + eyeKeys[eye] + eyebrowKeys[brow] + tearKeys[tear] + blushKeys[blus] + mouthKeys[mon];
            setPose(code);
            Debug.WriteLine(code);
            msg.Text = "Code: " + code;
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(pos.ToString() + eyeKeys[eye] + eyebrowKeys[brow] + tearKeys[tear] + blushKeys[blus] + mouthKeys[mon]);

        }
    }public static class StringExtensions
{
    public static string FirstLetterToUpper(this string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return source;
        }

        // Optional: Force the rest of the string to be lowercase
        // If you want to keep the original casing of the rest of the string, remove .ToLower()
        return char.ToUpper(source[0]) + source.Substring(1).ToLower();
    }
}
}