using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;

namespace MonikaOnDesktop {
    /// <summary>
    /// Логика взаимодействия для WearSelectWindow.xaml
    /// </summary>
    public partial class WearSelectWindow : Window {
        public string CurrentHair { get; set; }
        public string CurrentWear { get; set; }
        public string CurrentRibbon { get; set; }
        public string CurrentHat { get; set; }
        public string CurrentAhoge { get; set; }
        public List<string> CurrAcs = new List<string>();


        static string mainPath = AppDomain.CurrentDomain.BaseDirectory;

        static string monikaPath = System.IO.Path.Combine(mainPath, "monika");
        static string wearPath = System.IO.Path.Combine(monikaPath, "c");
        static string bodyPath = System.IO.Path.Combine(monikaPath, "b");
        static string hairPath = System.IO.Path.Combine(monikaPath, "h");
        static string acsPath = System.IO.Path.Combine(monikaPath, "a");


        public WearSelectWindow(string currentHair, string currentWear, string currentRibbon, string currentHat, string currentAhoge, List<string> currAcs) {
            InitializeComponent();
            /*Task.Run(async () =>
            {
                await LoadWearOptionsAsync();
            });*/
            CurrentHair = currentHair;
            CurrentWear = currentWear;
            CurrentRibbon = currentRibbon;
            CurrentHat = currentHat;
            CurrentAhoge = currentAhoge;
            CurrAcs = new List<string>(currAcs);

            loadListsAsync(CurrentHair, "h");
        }
        // Делаем метод асинхронным
        async Task loadListsAsync(string type, string path, string acsType = "") {
            string folder = System.IO.Path.Combine(monikaPath, path);
            DirectoryInfo directory = new DirectoryInfo(folder);
            if (path == "a" && acsType != "o") {
                RadioButton option = new RadioButton {
                    Width = 260,
                    Height = 190,
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    Margin = new Thickness(5),
                    Name = "null",
                    GroupName = "hair"
                };
                option.Style = (Style)FindResource("UpperMenuModeButton");

                option.Checked += (s, e) => {
                    type = "null";
                    switch (path + acsType) {
                        case "ar":
                        CurrentRibbon = "null";
                        break;
                        case "ah":
                        CurrentHat = "null";
                        break;
                        case "aa":
                        CurrentAhoge = "null";
                        break;
                    }
                };
                if (type == "null") {
                    option.IsChecked = true;
                }

                Image img = new Image { Source = GetImage(Path.Combine(acsPath, "blank.png"), false), Width = 260, Height = 190, HorizontalAlignment = HorizontalAlignment.Center };
                option.Content = img;

                libHair.Children.Add(option);
            }


            foreach (var file in directory.GetDirectories()) {
                string currentPath = System.IO.Path.Combine(folder, file.Name);

                if (!string.IsNullOrEmpty(acsType)) {
                    string jsonPath = Path.Combine(currentPath, "data.json");
                    if (File.Exists(jsonPath)) {
                        try {
                            string jsonString = await File.ReadAllTextAsync(jsonPath);
                            using (JsonDocument doc = JsonDocument.Parse(jsonString)) {
                                if (doc.RootElement.TryGetProperty("acs_type", out JsonElement layerElement)) {
                                    switch (acsType) {
                                        case "h":
                                        if (layerElement.GetString() != "hat") {
                                            continue;
                                        }
                                        break;
                                        case "r":
                                        if (layerElement.GetString() != "ribbon") {
                                            continue;
                                        }
                                        break;
                                        case "a":
                                        if (layerElement.GetString() != "ahoge") {
                                            continue;
                                        }
                                        break;
                                        case "o":
                                        if (layerElement.GetString() != "other") {
                                            continue;
                                        }
                                        break;
                                    }
                                }
                            }
                        } catch (Exception ex) { }
                    }
                }
                //ImageSource preview = await Task.Run(() => CreatePreviewWear(file.Name, path));
                //SaveDrawingToFile(preview, Path.Combine(currentPath, "preview.png"));
                ImageSource preview;
                if (File.Exists($"{currentPath}/preview.png"))
                    preview = GetImage($"{currentPath}/preview.png", true);
                else {
                    preview = await Task.Run(() => CreatePreviewWear(file.Name, path));
                    SaveDrawingToFile(preview, Path.Combine(currentPath, "preview.png"));
                }
                await Dispatcher.InvokeAsync(async () => {
                    if (acsType == "o") {
                        CheckBox option = new CheckBox {
                            Width = 260,
                            Height = 190,
                            Background = Brushes.Transparent,
                            BorderBrush = Brushes.Transparent,
                            Margin = new Thickness(5),
                            Name = file.Name
                        };
                        option.Style = (Style)FindResource("CheckBoxStyle");

                        option.Checked += (s, e) => {
                            CurrAcs.Add(file.Name);

                        };
                        option.Unchecked += (s, e) => {
                            CurrAcs.RemoveAll(x => x == file.Name);

                        };
                        if (CurrAcs.Contains(file.Name)) {
                            option.IsChecked = true;
                        }

                        Image img = new Image { Stretch = Stretch.Uniform, Source = preview, Width = 260, Height = 190, HorizontalAlignment = HorizontalAlignment.Center };
                        option.Content = img;

                        libHair.Children.Add(option);
                    } else {
                        RadioButton option = new RadioButton {
                            Width = 260,
                            Height = 190,
                            Background = Brushes.Transparent,
                            BorderBrush = Brushes.Transparent,
                            Margin = new Thickness(5),
                            Name = file.Name,
                            GroupName = "hair"
                        };
                        option.Style = (Style)FindResource("UpperMenuModeButton");

                        option.Checked += (s, e) => {
                            type = file.Name;
                            switch (path + acsType) {
                                case "h":
                                CurrentHair = file.Name;
                                break;
                                case "c":
                                CurrentWear = file.Name;
                                break;
                                case "ar":
                                CurrentRibbon = file.Name;
                                break;
                                case "ah":
                                CurrentHat = file.Name;
                                break;
                                case "aa":
                                CurrentAhoge = file.Name;
                                break;
                            }
                        };
                        if (file.Name == type) {
                            option.IsChecked = true;
                        }

                        Image img = new Image { Stretch = Stretch.Uniform, Source = preview, Width = 260, Height = 190, HorizontalAlignment = HorizontalAlignment.Center };
                        option.Content = img;

                        libHair.Children.Add(option);
                    }
                });
            }
        }

        public void SaveDrawingToFile(ImageSource drawing, string fileName) {
            var drawingImage = new Image { Source = drawing };
            var width = 260;
            var height = 190;
            drawingImage.Arrange(new Rect(0, 0, width, height));

            var bitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingImage);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = new FileStream(fileName, FileMode.Create)) {
                encoder.Save(stream);
            }
        }
        private ImageSource CreatePreviewWear(string path, string type) {
            // Важно: RenderTargetBitmap требует STA поток, поэтому 
            // иногда проще подготовить DrawingGroup и заморозить её.

            var drawingGroup = new DrawingGroup();
            using (var context = drawingGroup.Open()) {
                switch (type) {
                    case "c":
                    context.DrawImage(GetImage($"{monikaPath}/t/chair-def.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{hairPath}/orcaramelo_ponytailbraid/hair-orcaramelo_ponytailbraid-back.png", true), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage($"{bodyPath}/body-def-0.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{wearPath}/{path}/body-def-0.png", false), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage($"{bodyPath}/body-def-head.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{monikaPath}/t/table-def.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{monikaPath}/t/table-def-s.png", true), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage($"{bodyPath}/body-def-1.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{wearPath}/{path}/body-def-1.png", false), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage($"{hairPath}/orcaramelo_ponytailbraid/hair-orcaramelo_ponytailbraid-front.png", true), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage($"{bodyPath}/arms-steepling-10.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{wearPath}/{path}/arms-steepling-10.png", false), new Rect(0, 0, 260, 180));
                    break;
                    case "h":
                    context.DrawImage(GetImage($"{monikaPath}/t/chair-def.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{hairPath}/{path}/hair-{path}-back.png", false), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage($"{bodyPath}/body-def-0.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{wearPath}/def/body-def-0.png", true), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage($"{bodyPath}/body-def-head.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{monikaPath}/t/table-def.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{monikaPath}/t/table-def-s.png", true), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage($"{bodyPath}/body-def-1.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{wearPath}/def/body-def-1.png", true), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage($"{hairPath}/{path}/hair-{path}-front.png", false), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage($"{bodyPath}/arms-steepling-10.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{wearPath}/def/arms-steepling-10.png", true), new Rect(0, 0, 260, 180));
                    break;
                    case "a":
                    context.DrawImage(GetImage(GetAcs(path, "PRE_ACS"), true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{hairPath}/orcaramelo_ponytailbraid/hair-orcaramelo_ponytailbraid-back.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage(GetAcs(path, "BBH_ACS"), true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{monikaPath}/t/chair-def.png", true), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage($"{bodyPath}/body-def-0.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{wearPath}/def/body-def-0.png", true), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage(GetAcs(path, "BBA_ACS"), true), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage($"{bodyPath}/body-def-head.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{monikaPath}/t/table-def.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{monikaPath}/t/table-def-s.png", true), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage($"{bodyPath}/body-def-1.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{wearPath}/def/body-def-1.png", true), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage($"{hairPath}/orcaramelo_ponytailbraid/hair-orcaramelo_ponytailbraid-front.png", false), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage(GetAcs(path, "AFH_ACS"), true), new Rect(0, 0, 260, 180));

                    context.DrawImage(GetImage($"{bodyPath}/arms-steepling-10.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage($"{wearPath}/def/arms-steepling-10.png", true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage(GetAcs(path, "ASE_ACS"), true), new Rect(0, 0, 260, 180));
                    context.DrawImage(GetImage(GetAcs(path, "PST_ACS"), true), new Rect(0, 0, 260, 180));
                    break;
                }
            }

            drawingGroup.Freeze(); // Это позволяет передавать объект между потоками

            // Возвращаем как DrawingImage (легче для памяти, чем RenderTargetBitmap)
            var imageSource = new DrawingImage(drawingGroup);
            imageSource.Freeze();


            return imageSource;
        }
        string GetAcs(string name, string layer, int arm = -1) {
            string result = "";

            string jsonPath = Path.Combine(acsPath, name, "data.json"); // Или другое имя

            string jsonString = File.ReadAllText(jsonPath);
            using (JsonDocument doc = JsonDocument.Parse(jsonString)) {
                // Допустим, в JSON есть поле "draw_layer"
                if (doc.RootElement.TryGetProperty("layer", out JsonElement layerElement)) {
                    if (layerElement.ToString() == layer) {
                        // Допустим, в JSON есть поле "draw_layer"
                        if (doc.RootElement.TryGetProperty("posemap", out JsonElement posemapElement)) {
                            // Допустим, в JSON есть поле "draw_layer"
                            if (posemapElement.TryGetProperty("2", out JsonElement poseElement)) {
                                result = Path.Combine(acsPath, name, poseElement.ToString() + ".png");
                            }
                        }
                    } else {
                        result = Path.Combine(monikaPath, "null.png");
                    }
                }
            }

            return result;
        }

        private Dictionary<string, BitmapSource> _cache = new Dictionary<string, BitmapSource>();
        private readonly object _cacheLock = new object();
        public BitmapSource GetImage(string fileName, bool useCache) {
            if (!File.Exists(fileName)) fileName = Path.Combine(monikaPath, "null.png");

            if (useCache) {
                lock (_cacheLock) {
                    if (_cache.TryGetValue(fileName, out var cached)) return cached;
                }
            }

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(fileName); // Убедитесь в правильности пути
            if (fileName.Contains("null.png")) {
                bi.DecodePixelWidth = 1;
                bi.DecodePixelHeight = 1;
            } else {
                bi.DecodePixelWidth = 260;
            }
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            bi.Freeze();

            if (useCache) {
                lock (_cacheLock) { _cache[fileName] = bi; }
            }
            return bi;
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e) {
            Debug.WriteLine($"{CurrentHair}, {CurrentHat}, {CurrentRibbon}, {CurrentWear}");
            this.DialogResult = true;

        }
        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Tab_Click(object sender, RoutedEventArgs e) {
            Button button = (Button)sender;
            libHair.Children.Clear();
            switch (button.Name) {
                case "Hair":
                loadListsAsync(CurrentHair, "h");
                break;
                case "Wear":
                loadListsAsync(CurrentWear, "c");
                break;
                case "Hat":
                loadListsAsync(CurrentHat, "a", "h");
                break;
                case "Ribbon":
                loadListsAsync(CurrentRibbon, "a", "r");
                break;
                case "Ahoge":
                loadListsAsync(CurrentAhoge, "a", "a");
                break;
                case "Other":
                loadListsAsync(CurrentRibbon, "a", "o");
                break;
            }

        }
    }
}
