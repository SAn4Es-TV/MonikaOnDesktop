using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MonikaOnDesktop {
    /// <summary>
    /// Логика взаимодействия для Box.xaml
    /// </summary>
    public partial class Box : Window {
        public string aiText {
            get { return aiBox.Text; }
        }
        public string name {
            get { return nameBox.Text; }
        }
        public string lang {
            get { return lang_; }
        }
        private string lang_ = "";
        public Box(string type) {
            InitializeComponent();
            Debug.WriteLine(type);
            switch (type) {
                case "ai":
                    this.Height = 170;
                    this.Width = 500;
                    AiForm.Visibility = Visibility.Visible;
                    StartForm.Visibility = Visibility.Hidden;
                    break;
                case "start":
                    this.Height = 250;
                    this.Width = 500;
                    AiForm.Visibility = Visibility.Hidden;
                    StartForm.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {

            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void OK_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Close();
        }

        private void Ru_Click(object sender, RoutedEventArgs e) {
            lang_ = "ru";
            OutlinedTextBlock tr = Ru.Content as OutlinedTextBlock;
            OutlinedTextBlock te = En.Content as OutlinedTextBlock;
            tr.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 100, 184));
            tr.Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            te.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            te.Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 100, 184));
        }

        private void En_Click(object sender, RoutedEventArgs e) {
            lang_ = "en";
            OutlinedTextBlock tr = Ru.Content as OutlinedTextBlock;
            OutlinedTextBlock te = En.Content as OutlinedTextBlock;
            tr.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            tr.Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 100, 184));

            te.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 100, 184));
            te.Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

        }
    }
}
