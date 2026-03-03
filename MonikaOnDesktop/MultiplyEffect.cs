using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MonikaOnDesktop {
    public class MultiplyEffect : ShaderEffect {
        public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty("Input", typeof(MultiplyEffect), 0);

        public static readonly DependencyProperty FilterColorProperty =
            DependencyProperty.Register("FilterColor", typeof(Color), typeof(MultiplyEffect),
            new UIPropertyMetadata(Colors.White, PixelShaderConstantCallback(0)));

        public MultiplyEffect() {
            // Укажи путь к файлу внутри твоего проекта
            PixelShader = new PixelShader() {
                UriSource = new Uri("pack://application:,,,/MultiplyEffect.ps")
            };

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(FilterColorProperty);
        }

        public Brush Input {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        public Color FilterColor {
            get => (Color)GetValue(FilterColorProperty);
            set => SetValue(FilterColorProperty, value);
        }
    }
}
