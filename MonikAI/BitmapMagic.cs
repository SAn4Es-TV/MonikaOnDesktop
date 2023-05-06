using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace MonikaOnDesktop
{
    internal static class BitmapMagic
    {
        public static BitmapImage BitmapToImageSource(Bitmap bm)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                if(bm != null) { 
                bm.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
                }
                else
                {
                    return null;
                }
            }
        }
        public static Bitmap ToColorTone(System.Drawing.Image image, float[] filter)
        {
            //creating a new bitmap image with selected color.
            //float scale = BrightnessSelection.Value / 128f;

            float r = filter[0];
            float g = filter[1];
            float b = filter[2];

            // Color Matrix
            ColorMatrix cm = new ColorMatrix(new float[][]
            {
                new float[] {r, 0, 0, 0, 0},
                new float[] {0, g, 0, 0, 0},
                new float[] {0, 0, b, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            });
            ImageAttributes ImAttribute = new ImageAttributes();
            ImAttribute.SetColorMatrix(cm);

            //Color Matrix on new bitmap image
            System.Drawing.Point[] points =
            {
                new System.Drawing.Point(0, 0),
                new System.Drawing.Point(image.Width - 1, 0),
                new System.Drawing.Point(0, image.Height - 1),
            };
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, image.Width, image.Height);

            Bitmap myBitmap = new Bitmap(image.Width, image.Height);
            using (Graphics graphics = Graphics.FromImage(myBitmap))
            {
                graphics.DrawImage(image, points, rect, GraphicsUnit.Pixel, ImAttribute);
            }
            return myBitmap;
        }
        public static Bitmap ToColorTone(Uri uri, float[] filter)
        {
            Bitmap bm;
            if (uri.AbsoluteUri.StartsWith("pack://"))
            {
                var sri = System.Windows.Application.GetResourceStream(uri);
                bm = new Bitmap(sri.Stream);
            }
            else
            {
                bm = new Bitmap(uri.LocalPath);
            }
            bm.MakeTransparent();
            System.Drawing.Image image = bm;
            //creating a new bitmap image with selected color.
            //float scale = BrightnessSelection.Value / 128f;

            float r = filter[0];
            float g = filter[1];
            float b = filter[2];

            // Color Matrix
            ColorMatrix cm = new ColorMatrix(new float[][]
            {
                new float[] {r, 0, 0, 0, 0},
                new float[] {0, g, 0, 0, 0},
                new float[] {0, 0, b, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            });
            ImageAttributes ImAttribute = new ImageAttributes();
            ImAttribute.SetColorMatrix(cm);

            //Color Matrix on new bitmap image
            System.Drawing.Point[] points =
            {
                new System.Drawing.Point(0, 0),
                new System.Drawing.Point(image.Width - 1, 0),
                new System.Drawing.Point(0, image.Height - 1),
            };
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, image.Width, image.Height);

            Bitmap myBitmap = new Bitmap(image.Width, image.Height);
            using (Graphics graphics = Graphics.FromImage(myBitmap))
            {
                graphics.DrawImage(image, points, rect, GraphicsUnit.Pixel, ImAttribute);
            }
            return myBitmap;
        }
    }
}
