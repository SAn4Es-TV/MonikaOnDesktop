using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MonikaOnDesktop {
    public class OutlinedText : Shape {
        // Добавь это в свой класс OutlinedTextBlock
        public static readonly DependencyProperty VisibleCharactersCountProperty = DependencyProperty.Register(
            "VisibleCharactersCount", typeof(int), typeof(OutlinedTextBlock),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(OutlinedText), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public int VisibleCharactersCount {
            get => (int)GetValue(VisibleCharactersCountProperty);
            set => SetValue(VisibleCharactersCountProperty, value);
        }
        // Добавьте здесь свойства FontSize, FontFamily, FontWeight по аналогии, 
        // если планируете менять их динамически.

        protected override Geometry DefiningGeometry {
            get {
                var formattedText = new FormattedText(
                    Text,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"), // Можно вынести в DependencyProperty
                    32,                      // Можно вынести в DependencyProperty
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                // Превращаем текст в векторные контуры
                return formattedText.BuildGeometry(new Point(0, 0));
            }
        }
    }
}
