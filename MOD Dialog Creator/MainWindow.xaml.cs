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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MOD_Dialog_Creator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public float[] nightFilter = { 0.6861919617166911f, 0.387275212f, 0.27662517f };
        public float[] dayFilter = { 1f, 1f, 1f };
        public float[] mainFilter = { 1f, 1f, 1f };

        public bool IsNight;

        CharacterModel Monika = new CharacterModel(AppDomain.CurrentDomain.BaseDirectory + "/characters/monika.chr", AppDomain.CurrentDomain.BaseDirectory + "/characters/"); // Персонаж Моники
        public string normalPose = "1esc";
        public MainWindow()
        {
            InitializeComponent();
            setFace(normalPose);
        }

        public async void setFace(string faceName)
        {
            if (IsNight)
                mainFilter = nightFilter;
            else
                mainFilter = dayFilter;

            int body = int.Parse(faceName[0].ToString());
            string eye = faceName[1].ToString();
            string eyebrow = faceName[2].ToString();
            string mouth = faceName[3].ToString();

            RedrawCostume(body, Monika.costumeName);
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    switch (body)
                    {
                        case 1:
                            //var bitmap = new Bitmap("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png");
                            //this.Body.Source = BitmapMagic.BitmapToImageSource(bitmap);
                            this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png"), mainFilter));
                            this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[12] + ".png"), mainFilter));
                            this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[13] + ".png"), mainFilter));
                            this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[10] + ".png"), mainFilter));
                            this.Hand1.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            this.Hand2.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            break;
                        case 2:
                            this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png"), mainFilter));
                            this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[12] + ".png"), mainFilter));
                            this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[13] + ".png"), mainFilter));
                            this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[0] + ".png"), mainFilter));
                            this.Hand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[1] + ".png"), mainFilter));
                            this.Hand2.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            break;
                        case 3:
                            this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png"), mainFilter));
                            this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[12] + ".png"), mainFilter));
                            this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[13] + ".png"), mainFilter));
                            this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[8] + ".png"), mainFilter));
                            this.Hand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[6] + ".png"), mainFilter));
                            this.Hand2.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            break;
                        case 4:
                            this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png"), mainFilter));
                            this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[12] + ".png"), mainFilter));
                            this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[13] + ".png"), mainFilter));
                            this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[8] + ".png"), mainFilter));
                            this.Hand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[5] + ".png"), mainFilter));
                            this.Hand2.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            break;
                        case 5:
                            this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[14] + ".png"), mainFilter));
                            this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[15] + ".png"), mainFilter));
                            this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[16] + ".png"), mainFilter));
                            this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[2] + ".png"), mainFilter));
                            this.Hand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[3] + ".png"), mainFilter));
                            this.Hand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[4] + ".png"), mainFilter));
                            break;
                        case 6:
                            this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png"), mainFilter));
                            this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[12] + ".png"), mainFilter));
                            this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[13] + ".png"), mainFilter));
                            this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[7] + ".png"), mainFilter));
                            this.Hand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[5] + ".png"), mainFilter));
                            this.Hand2.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            break;
                        case 7:
                            this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png"), mainFilter));
                            this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[12] + ".png"), mainFilter));
                            this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[13] + ".png"), mainFilter));
                            this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[10] + ".png"), mainFilter));
                            this.Hand1.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            this.Hand2.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            break;
                        default:
                            this.Body.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[11] + ".png"), mainFilter));
                            this.Body1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[12] + ".png"), mainFilter));
                            this.Head.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[13] + ".png"), mainFilter));
                            this.Hand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[5] + ".png"), mainFilter));
                            this.Hand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/b/" + Monika.body[8] + ".png"), mainFilter));
                            this.Hand2.Source = null;//BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            break;
                    }
                    if (body == 5) { Monika.leaningWord = "leaning-def-"; } else { Monika.leaningWord = ""; }
                    switch (eye)
                    {
                        case "e":
                            this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[4] + ".png"), mainFilter));
                            break;
                        case "w":
                            this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[11] + ".png"), mainFilter));
                            break;
                        case "s":
                            this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[10] + ".png"), mainFilter));
                            break;
                        case "t":
                            this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[6] + ".png"), mainFilter));
                            break;
                        case "c":
                            this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[2] + ".png"), mainFilter));
                            break;
                        case "r":
                            this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[5] + ".png"), mainFilter));
                            break;
                        case "l":
                            this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[3] + ".png"), mainFilter));
                            break;
                        case "h":
                            this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[0] + ".png"), mainFilter));
                            break;
                        case "d":
                            this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[1] + ".png"), mainFilter));
                            break;
                        case "k":
                            this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[12] + ".png"), mainFilter));
                            break;
                        case "n":
                            this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[13] + ".png"), mainFilter));
                            break;
                        case "f":
                            this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[9] + ".png"), mainFilter));
                            break;
                        case "m":
                            this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[7] + ".png"), mainFilter));
                            break;
                        case "g":
                            this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[8] + ".png"), mainFilter));
                            break;
                        default:
                            this.Eyes.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fe/face-" + Monika.leaningWord + Monika.eyes[4] + ".png"), mainFilter));
                            break;
                    }
                    switch (eyebrow)
                    {
                        case "u":
                            this.EyeBrow.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fb/face-" + Monika.leaningWord + Monika.eyesBrow[4] + ".png"), mainFilter));
                            break;
                        case "k":
                            this.EyeBrow.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fb/face-" + Monika.leaningWord + Monika.eyesBrow[1] + ".png"), mainFilter));
                            break;
                        case "s":
                            this.EyeBrow.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fb/face-" + Monika.leaningWord + Monika.eyesBrow[2] + ".png"), mainFilter));
                            break;
                        case "t":
                            this.EyeBrow.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fb/face-" + Monika.leaningWord + Monika.eyesBrow[3] + ".png"), mainFilter));
                            break;
                        case "f":
                            this.EyeBrow.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fb/face-" + Monika.leaningWord + Monika.eyesBrow[0] + ".png"), mainFilter));
                            break;
                        default:
                            this.EyeBrow.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fb/face-" + Monika.leaningWord + Monika.eyesBrow[4] + ".png"), mainFilter));
                            break;
                    }
                    switch (mouth)
                    {
                        case "a":
                            this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[4] + ".png"), mainFilter));
                            break;
                        case "b":
                            this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[0] + ".png"), mainFilter));
                            break;
                        case "c":
                            this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[5] + ".png"), mainFilter));
                            break;
                        case "d":
                            this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[3] + ".png"), mainFilter));
                            break;
                        case "o":
                            this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[1] + ".png"), mainFilter));
                            break;
                        case "u":
                            this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[6] + ".png"), mainFilter));
                            break;
                        case "w":
                            this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[8] + ".png"), mainFilter));
                            break;
                        case "p":
                            this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[2] + ".png"), mainFilter));
                            break;
                        case "t":
                            this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[7] + ".png"), mainFilter));
                            break;
                        default:
                            this.Mouth.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/fm/face-" + Monika.leaningWord + Monika.mouth[4] + ".png"), mainFilter));
                            break;
                    }
                    if (body == 5) { Monika.leaningWord = "-leaning"; } else { Monika.leaningWord = ""; }
                    string hairPath = "hair" + Monika.leaningWord + "-" + Monika.hairType;
                    string nosePath = "face" + Monika.leaningWord + "-nose-def.png";
                    this.Face.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/face/" + nosePath), mainFilter));
                    this.Hair.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/h/" + hairPath + "-front.png"), mainFilter));
                    this.HairBack.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/h/" + hairPath + "-back.png"), mainFilter));

                    this.table1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/t/chair-def.png"), mainFilter));
                    this.table2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/t/table-def.png"), mainFilter));
                    this.table3.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/t/table-def-s.png"), mainFilter));
                });
            }
            catch
            {
            }
        }
        public void RedrawCostume(int body, string costume)
        {
            string pathCost = AppDomain.CurrentDomain.BaseDirectory + "/costumes/";

            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    switch (body)
                    {
                        case 1:
                            this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[11] + ".png"), mainFilter));
                            this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[12] + ".png"), mainFilter));
                            this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[10] + ".png"), mainFilter));
                            this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            break;
                        case 2:
                            this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[11] + ".png"), mainFilter));
                            this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[12] + ".png"), mainFilter));
                            this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[0] + ".png"), mainFilter));
                            this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[1] + ".png"), mainFilter));
                            this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            break;
                        case 3:
                            this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[11] + ".png"), mainFilter));
                            this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[12] + ".png"), mainFilter));
                            this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[8] + ".png"), mainFilter));
                            this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[6] + ".png"), mainFilter));
                            this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            break;
                        case 4:
                            this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[11] + ".png"), mainFilter));
                            this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[12] + ".png"), mainFilter));
                            this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[8] + ".png"), mainFilter));
                            this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[5] + ".png"), mainFilter));
                            this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            break;
                        case 5:
                            this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[14] + ".png"), mainFilter));
                            this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[15] + ".png"), mainFilter));
                            this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[2] + ".png"), mainFilter));
                            this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[3] + ".png"), mainFilter));
                            this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[4] + ".png"), mainFilter));
                            break;
                        case 6:
                            this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[11] + ".png"), mainFilter));
                            this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[12] + ".png"), mainFilter));
                            this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[7] + ".png"), mainFilter));
                            this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[5] + ".png"), mainFilter));
                            this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            break;
                        case 7:
                            this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[11] + ".png"), mainFilter));
                            this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[12] + ".png"), mainFilter));
                            this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[10] + ".png"), mainFilter));
                            this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            break;
                        default:
                            this.UniformBody.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[11] + ".png"), mainFilter));
                            this.UniformBody1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[12] + ".png"), mainFilter));
                            this.UniformHand.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[5] + ".png"), mainFilter));
                            this.UniformHand1.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri(pathCost + costume + "/" + Monika.body[8] + ".png"), mainFilter));
                            this.UniformHand2.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/" + Monika.nullPath + ".png"), mainFilter));
                            break;
                    }
                    if (body == 5) { Monika.leaningWord = "5"; } else { Monika.leaningWord = "0"; }
                    string ribbonPath = "acs-ribbon_def-" + Monika.leaningWord + ".png";
                    this.Ribbon_back.Source = BitmapMagic.BitmapToImageSource(BitmapMagic.ToColorTone(new Uri("pack://application:,,,/assets/monika/ribbon/" + ribbonPath), mainFilter));
                });
            }
            catch
            {

            };
        }
    }
}
