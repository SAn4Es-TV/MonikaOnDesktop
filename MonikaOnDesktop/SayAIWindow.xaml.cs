using System.Windows;
using System.Windows.Input;

namespace MonikaOnDesktop {
    /// <summary>
    /// Логика взаимодействия для SayAIWindow.xaml
    /// </summary>
    public partial class SayAIWindow : Window {
        public string Text { get; set; }
        public SayAIWindow() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Text = InputTextBox.Text;
            DialogResult = true;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            this.Close();


        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Text = InputTextBox.Text;
                DialogResult = true;
                this.Close();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {

            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
