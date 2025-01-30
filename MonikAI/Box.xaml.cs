using System;
using System.Collections.Generic;
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
        public string type { get; set; }
        public string aiText {
            get { return aiBox.Text; }
        }
        public Box() {
            InitializeComponent();
            switch (type) {
                case "ai":
                    this.Height = 170;
                    this.Width = 500;
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
    }
}
