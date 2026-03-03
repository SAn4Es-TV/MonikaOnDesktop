using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MonikaOnDesktop.Classes;
using Path = System.IO.Path;

namespace MonikaOnDesktop {
    /// <summary>
    /// Логика взаимодействия для MainSettingsView.xaml
    /// </summary>
    public partial class MainSettingsView : UserControl {// Внутри вашего CustomControl.xaml.cs
        public static readonly RoutedEvent ScaleChangedEvent = EventManager.RegisterRoutedEvent(
            "ScaleChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(MainSettingsView));

        public event RoutedPropertyChangedEventHandler<double> ScaleChanged {
            add { AddHandler(ScaleChangedEvent, value); }
            remove { RemoveHandler(ScaleChangedEvent, value); }
        }
        public static readonly RoutedEvent LangChangedEvent = EventManager.RegisterRoutedEvent(
            "LangChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(MainSettingsView));

        public event RoutedPropertyChangedEventHandler<double> LangChanged {
            add { AddHandler(LangChangedEvent, value); }
            remove { RemoveHandler(LangChangedEvent, value); }
        }
        public MainSettingsView() {
            InitializeComponent();
            this.Loaded += (s, e) => {
                if (this.DataContext is SettingsViewModel settingsVM) {
                    string currentLang = settingsVM.Settings.Lang;
                    foreach (LanguageModel model in GetAvailableLanguages()) {
                        RadioButton radioButton = new RadioButton();
                        radioButton.Tag = model;
                        radioButton.Content = model.Name;
                        Debug.WriteLine((DataContext as AppSettings));
                        if (model.CultureCode == currentLang)
                            radioButton.IsChecked = true;
                        radioButton.Checked += (s, e) => {

                            var selectedModel = (LanguageModel)((RadioButton)s).Tag;
                            settingsVM.Settings.Lang = selectedModel.CultureCode;
                            settingsVM.Save();
                            RaiseEvent(new RoutedPropertyChangedEventArgs<double>(1, 2, LangChangedEvent));

                        };
                        langCategory.Children.Add(radioButton);
                    }
                }
            };

        }

        private void Thumb_DragDelta() {

        }

        private void ScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            RaiseEvent(new RoutedPropertyChangedEventArgs<double>(e.OldValue, e.NewValue, ScaleChangedEvent));
        }
        // В окне настроек или ViewModel:
        public List<LanguageModel> GetAvailableLanguages() {
            var languages = new List<LanguageModel>();
            string langPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "langs");

            if (!Directory.Exists(langPath)) return languages;

            foreach (var file in Directory.GetFiles(langPath, "*.xaml")) {
                try {
                    var dict = new ResourceDictionary { Source = new Uri(file, UriKind.Absolute) };
                    if (dict.Contains("LanguageName")) {
                        languages.Add(new LanguageModel {
                            Name = dict["LanguageName"].ToString(),
                            Uri = dict.Source,
                            CultureCode = Path.GetFileNameWithoutExtension(file)
                        });
                    }
                } catch { /* Ошибка в XAML файле пользователя — пропускаем */ }
            }
            return languages;
        }
    }
}
