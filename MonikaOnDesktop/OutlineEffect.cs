using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MonikaOnDesktop {
    public class OutlineEffect : ShaderEffect {
        private static readonly PixelShader _pixelShader = new PixelShader() {
            UriSource = new Uri("pack://application:,,,/MonikaOnDesktop;component/outlineeffect.ps") // Проверь путь к файлу!
        }; public OutlineEffect() {
            this.PixelShader = _pixelShader;
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(OutlineColorProperty);
            UpdateShaderValue(ThicknessProperty);
            UpdateShaderValue(SoftnessProperty);
        }

        public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty("Input", typeof(OutlineEffect), 0);

        public static readonly DependencyProperty OutlineColorProperty =
            DependencyProperty.Register("OutlineColor", typeof(Color), typeof(OutlineEffect),
                new UIPropertyMetadata(Colors.Black, PixelShaderConstantCallback(0)));

        public static readonly DependencyProperty ThicknessProperty =
        DependencyProperty.Register("Thickness", typeof(double), typeof(OutlineEffect),
            new PropertyMetadata(2.0, OnThicknessChanged));

        public static readonly DependencyProperty SoftnessProperty =
            DependencyProperty.Register("Softness", typeof(double), typeof(OutlineEffect),
                new UIPropertyMetadata(0.5, PixelShaderConstantCallback(2)));

        // Реальная толщина, которая уходит в шейдер (регистр c1)
        private static readonly DependencyProperty ActualThicknessProperty =
            DependencyProperty.Register("ActualThickness", typeof(double), typeof(OutlineEffect),
                new UIPropertyMetadata(2.0, PixelShaderConstantCallback(1)));

        public Color OutlineColor { get => (Color)GetValue(OutlineColorProperty); set => SetValue(OutlineColorProperty, value); }
        public double Thickness { get => (double)GetValue(ThicknessProperty); set => SetValue(ThicknessProperty, value); }
        public double Softness { get => (double)GetValue(SoftnessProperty); set => SetValue(SoftnessProperty, value); }

        // Метод для пересчета толщины при изменении размера окна
        public void UpdateScale(double currentWindowWidth, double designWidth = 1280.0) {
            double scale = currentWindowWidth / designWidth;
            // Умножаем базовую толщину на текущий масштаб окна
            SetValue(ActualThicknessProperty, Thickness * scale);
        }

        private static void OnThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((OutlineEffect)d).UpdateScale(Application.Current.MainWindow.ActualWidth);
        }
    }
}