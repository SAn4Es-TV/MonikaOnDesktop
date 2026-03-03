using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MonikaOnDesktop {
    public class AlphaTapImage : Image {
        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters) {
            var source = Source as BitmapSource;
            if (source == null) return null;

            // Координаты клика
            Point pt = hitTestParameters.HitPoint;

            // Приведение координат к размеру исходного файла
            int x = (int)(pt.X * source.PixelWidth / ActualWidth);
            int y = (int)(pt.Y * source.PixelHeight / ActualHeight);

            if (x < 0 || x >= source.PixelWidth || y < 0 || y >= source.PixelHeight)
                return null;

            byte[] pixel = new byte[4];
            source.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);

            // Если прозрачность меньше порога (например, 10 из 255), игнорируем клик
            if (pixel[3] < 10)
                return null;

            return new PointHitTestResult(this, pt);
        }
    }
}
