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
        public string filePath;
        public int affection = 0;
        public string pcName;
        public string playerName;
        public string lang = "en-US";
        public bool autoStart;
        public int idleRandomFrom = 120;
        public int idleRandomTo = 300;
        public int NightStart = 21;
        public int nightEnd = 6;
        public int Scaler = 3;
        public bool screenNum = false;

        public CharacterModel(string path)
        {
            this.filePath = path;
        }
        public bool fileExist()
        {
            FileInfo fileInf = new FileInfo(filePath);
            return fileInf.Exists;
        }

        public void saveData()
        {
            MonikaSettings.Default.Language = new System.Globalization.CultureInfo(lang);
            MonikaSettings.Default.UserName = playerName;
            MonikaSettings.Default.Scaler = Scaler;
            MonikaSettings.Default.NightEnd = nightEnd;
            MonikaSettings.Default.NightStart = NightStart;
            MonikaSettings.Default.idleRandomFrom = idleRandomFrom;
            MonikaSettings.Default.idleRandomTo = idleRandomTo;
            MonikaSettings.Default.screenNum = screenNum;
            MonikaSettings.Default.AutoStart = autoStart;

            string text = affection.ToString() + ",";
            text += pcName + ",";
            text += playerName + ",";
            text += lang + ",";
            text += autoStart + ",";
            text += idleRandomFrom + ",";
            text += idleRandomTo + ",";
            text += NightStart + ",";
            text += nightEnd + ",";
            text += Scaler + ",";
            text += screenNum + ",";
            DirectoryInfo dirInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "/characters");
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
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
                    this.playerName = data[2];
                    this.lang = data[3];
                    this.autoStart = Boolean.Parse(data[4]);
                    this.idleRandomFrom = int.Parse(data[5]);
                    this.idleRandomTo = int.Parse(data[6]);
                    this.NightStart = int.Parse(data[7]);
                    this.nightEnd = int.Parse(data[8]);
                    this.Scaler = int.Parse(data[9]);
                    this.screenNum = Boolean.Parse(data[10]);
                }
            }
        }
        
    }
}
