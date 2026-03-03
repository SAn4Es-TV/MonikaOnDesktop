using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace MonikaOnDesktop {// Модель для конкретной опции (чекбокс или слайдер)
    public class SettingsViewModel : INotifyPropertyChanged {
        private object _currentView;
        public AppSettings Settings { get; set; }

        public object CurrentView {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public SettingsViewModel() {
            // Загружаем настройки через ваш SaveManager
            Settings = SaveManager.Load();
            // По умолчанию открываем первую вкладку
            CurrentView = Settings;
        }

        public void Save() => SaveManager.Save(Settings);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window {


        public SettingsViewModel ViewModel { get; set; }
        public SettingsWindow(AppSettings appSettings) {
            InitializeComponent();
            ViewModel = new SettingsViewModel();
            ViewModel.Settings = appSettings;
            DataContext = ViewModel;

            this.AddHandler(CheckBox.ClickEvent, new RoutedEventHandler(Save_Click));
            // Устанавливаем начальную страницу
            UpdateView("Main");
        }
        private void Tab_Checked(object sender, RoutedEventArgs e) {
            if (sender is RadioButton rb) {
                UpdateView(rb.Tag.ToString());
            }
        }

        private void UpdateView(string tag) {
            if (SettingsContent == null) return;

            switch (tag) {
                case "Main":
                MainSettingsView mainSettingsView = new MainSettingsView { DataContext = ViewModel };
                mainSettingsView.ScaleChanged += CustomControl_SizeChanged;
                mainSettingsView.LangChanged += MainSettingsView_LangChanged;

                SettingsContent.Content = mainSettingsView;
                break;
                case "AI":
                SettingsContent.Content = new AISettingsView { DataContext = ViewModel };
                break;
            }
        }

        public event Action<double> RequestMainWindowLang;
        private void MainSettingsView_LangChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Debug.WriteLine($"Language changed to: {ViewModel.Settings.Lang}");
            RequestMainWindowLang?.Invoke(e.NewValue);
        }

        public event Action<double> RequestMainWindowResize;
        private void CustomControl_SizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            // Ловим всплывшее событие от CustomControl и кидаем его главному окну
            RequestMainWindowResize?.Invoke(e.NewValue);
        }


        private void Save_Click(object sender, RoutedEventArgs e) {
            ViewModel.Save();
        }

        private void Exit_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Close();
        }
    }
}
