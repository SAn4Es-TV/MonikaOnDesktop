using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Monika.Shared;

namespace Monika.Hangman {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        List<KeyValuePair<string, int>> dictionary = new List<KeyValuePair<string, int>>();
        string Word = "";
        string character = "m";
        string FormatedWord = "";
        List<string> Missed = new List<string>();
        List<string> Correct = new List<string>();
        Random random;

        int popitka = 6;
        int correct = 0;

        bool losed = false;
        bool win = false;

        bool want_close = true;

        string isSayori = "s";
        void loadWords() {

            /*if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ru.txt"))) {
                dictionary = File.ReadLines(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "words.txt"))
       .Select(line => line.Split(','))
       .Where(parts => parts.Length == 2 && int.TryParse(parts[1], out _))
       .Select(parts => new KeyValuePair<string, int>(parts[0].Trim(), int.Parse(parts[1])))
       .ToList();
            } else {
                dictionary = File.ReadLines(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "words_ru.txt"))
       .Select(line => line.Split(','))
       .Where(parts => parts.Length == 2 && int.TryParse(parts[1], out _))
       .Select(parts => new KeyValuePair<string, int>(parts[0].Trim(), int.Parse(parts[1])))
       .ToList();
            }*/
            string[] resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (var res in resources) Debug.WriteLine(res);

            string resName;
            string all = "";
            // Формат: "ИмяПроекта.Папка.Файл.расширение"
            //string resourceName = "Monika.Hangman.Resources.words.txt";

            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ru.txt")))
                resName = "Monika.Hangman.words.txt";
            else
                resName = "Monika.Hangman.words_ru.txt";

            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resName))
            using (StreamReader reader = new StreamReader(stream)) {
                all = reader.ReadToEnd();
            }
            /*
            var file = GetResourceStream(resName);

            using (var reader = new StreamReader(file)) {
                all = reader.ReadToEnd();
            }*/

            dictionary = all.Split('\n')
       .Select(line => line.Split(','))
       .Where(parts => parts.Length == 2 && int.TryParse(parts[1], out _))
       .Select(parts => new KeyValuePair<string, int>(parts[0].Trim(), int.Parse(parts[1])))
       .ToList();
        }
        static UnmanagedMemoryStream GetResourceStream(string resName) {
            var assembly = Assembly.GetExecutingAssembly();
            var strResources = assembly.GetName().Name + ".g.resources";
            var rStream = assembly.GetManifestResourceStream(strResources);
            var resourceReader = new System.Resources.ResourceReader(rStream);
            var items = resourceReader.OfType<System.Collections.DictionaryEntry>();
            var stream = items.First(x => (x.Key as string) == resName.ToLower()).Value;
            return (UnmanagedMemoryStream)stream;
        }
        public MainWindow() {
            InitializeComponent();

            loadWords();
            Random random = new Random();
            if (random.Next(1, 10) == 1) {
                isSayori = "_s";

            } else {
                isSayori = "";
            }

            hangman.Source = new BitmapImage(new Uri($"pack://application:,,,/hangman/hm{isSayori}_6.png"));

            random = new Random();
            GetWord();
            Debug.WriteLine(Word);
            foreach (char letter in Word) {
                FormatedWord += "_ ";
            }
            word.Text = FormatedWord;
            PipeManager.SendMessage("MonikaInteractionPipe", $"HM_START,{character}");

            // Запускаем сервер и говорим, что делать при получении сообщения
            PipeManager.StartServer("MonikaInteractionPipeR", (msg) => {
                Dispatcher.Invoke(() => {
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
                        case "HM_RESTART":
                            popitka = 6;
                            correct = 0;

                            hangman.Source = new BitmapImage(new Uri($"pack://application:,,,/hangman/hm{isSayori}_{popitka}.png"));

                            win = false;
                            losed = false;
                            FormatedWord = "";
                            random = new Random();
                            GetWord();
                            Debug.WriteLine(Word);
                            foreach (char letter in Word) {
                                FormatedWord += "_ ";
                            }
                            word.Text = FormatedWord;
                            missed.Text = "Missed: ";
                            Missed = new List<string>();
                            PipeManager.SendMessage("MonikaInteractionPipe", $"HM_TIP,{character}");

                            break;
                        case "HM_QUIT":
                            want_close = false;
                            this.Close();
                            break;
                    }
                });
            });

            this.KeyDown += (s, e) => {
                if (e.Key == Key.Enter && !losed && !win && textBox.Text != "?") {
                    bool Answered = false;
                    word.Text = "";
                    missed.Text = "Missed: ";
                    char[] chars = FormatedWord.ToCharArray();
                    for (int i = 0; i < Word.Length; i++) {
                        if (Word[i].ToString().ToLower() == textBox.Text.ToLower()) {
                            chars[i * 2] = textBox.Text.ToCharArray()[0];
                            Answered = true;
                        }
                    }
                    if (!Answered && !Missed.Contains(textBox.Text)) {
                        if (isSayori == "_s") {
                            Title = "Ĥ̵̴̸̵̶̷̡̛̯̗͔̠̳͇̫̣͙̹̗̪̺̜͓̮̺̽̈̐̎̎͗̊̆̏̽̔͛͑̏͊͒͌̎̔̐̚̚͘̕͝͠͝͝ạ̴̶̴̷̸̢̛̘̬̖̠͚͈̳̘̩͕̳̳͎̦̺͓̥̯̝͋̃̈́̆̾͌̈́̂̐͛͊͂͘̕ñ̴̴̵̶̶̨̢̧̛͉̰̲̫̱̼̻̫̙̳͇̰̝̩͇͛̐̆̔̄̉͆̆̊̿̄̓̐̽̿̕̚͝ͅg̷̶̵̵̴̡̛̪̠͚̪̖̳̤̱̖͍̱̪̟͔̠̫̪̩̮̈́͆͛̇̇̈́̈́͐̊̐͌̽̿́̕͜͝͠m̷̶̸̶̵̨̡̛̟̜̠͎͇̝̣͔̰͔̳͍̜̞͎̞̀̈́̿̾̒̐̋̓͑̍͋̌̇́͗̌͝͝ặ̴̶̷̴̶̢̢̧̨̨̛̗̹͚͍̤̙͖̠̥͇̗̱̥͍͙̠͎̯̟̖̟̳̄͆̿̌̉̋͆͆͐̿̃̃͆̋͂̈̔͜ň̸̴̴̴̶̨̢̧̨̟̥͚͔̟͕̗͇̩̭̰̜̯̺̱̱͓̈́́͂̇͌̆̈͌̀̒͊̑̃̑̔̒̇̿̆̿̚͜͝͝";
                        }

                        Missed.Add(textBox.Text);
                        popitka -= 1;
                        hangman.Source = new BitmapImage(new Uri($"pack://application:,,,/hangman/hm{isSayori}_{popitka}.png"));

                        if (popitka == 0) {
                            losed = true;
                            Debug.WriteLine("Ты проиграл!");
                            PipeManager.SendMessage("MonikaInteractionPipe", $"HM_LOSE,{Word}");
                        }

                    }
                    if (Answered) {
                        correct += 1;
                    }
                    foreach (string letter in Missed) {
                        missed.Text += letter + " ";
                    }
                    FormatedWord = new string(chars);
                    if (!FormatedWord.Contains("_")) {
                        win = true;
                        Debug.WriteLine("Ты выиграл!");
                        PipeManager.SendMessage("MonikaInteractionPipe", $"HM_WIN,{Word}");
                    }
                    word.Text = FormatedWord;
                    textBox.Text = "";
                } else if (e.Key == Key.Enter && textBox.Text == "?") {
                    PipeManager.SendMessage("MonikaInteractionPipe", $"HM_TIP,{character}");
                    textBox.Text = "";
                }
                this.Closing += (sender, e) => {
                    if (!losed && !win && want_close) {
                        PipeManager.SendMessage("MonikaInteractionPipe", $"HM_CLOSING,{Word};{popitka};{correct}");
                    }
                };
            };
            void GetWord() {
                Random rng = new Random();

                if (dictionary.Count > 0) {
                    int index = rng.Next(dictionary.Count);
                    var randomEntry = dictionary[index];

                    Word = randomEntry.Key;
                    string[] names = new string[] { "s", "n", "y", "m" };
                    character = names[randomEntry.Value - 1];
                    Console.WriteLine($"Выпало: {randomEntry.Key} (число: {randomEntry.Value})");
                }

            }
        }
    }
}