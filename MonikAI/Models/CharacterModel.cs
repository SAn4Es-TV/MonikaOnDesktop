using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Shapes;

using static System.Net.Mime.MediaTypeNames;

namespace MonikaOnDesktop
{
    class CharacterModel
    {
        public string filePath { get; set; }
        public int affection { get; set; }
        public string pcName { get; set; }

        public CharacterModel(string path)
        {
            this.filePath = path;
        }

        public void saveData()
        {
            string text = affection.ToString() + ",";
            text += pcName + ",";
            using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.Default))
            {
                sw.WriteLine(text);
            }
        }
        public void loadData()
        {
            FileInfo fileInf = new FileInfo(filePath);
            if (fileInf.Exists)
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string[] data = sr.ReadToEnd().Split(",");
                    this.affection = int.Parse(data[0]);
                    this.pcName = data[1];
                }
            } else
            {
                using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.Default))
                {
                    string text = affection.ToString() + ",";
                    text += pcName + ",";
                    sw.WriteLine(text);
                }
            }
        }

    }
}
