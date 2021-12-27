using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Shapes;

using static System.Net.Mime.MediaTypeNames;

namespace MonikaOnDesktop
{
    class CharacterModel
    {
        public string filePath;
        public string giftsPath;
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
        public bool isMouse = false;

        public List<string> gifts = new List<string>(); 

        public CharacterModel(string path, string giftsPath)
        {
            this.filePath = path;
            this.giftsPath = giftsPath;
            foreach (string gift in gifts)
            {
                Debug.WriteLine("start: " + gift);
            }
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
            MonikaSettings.Default.isMouse = isMouse;

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
            text += isMouse + ",";
            text += "\n==\r\n";
            if (gifts.Count != 0)
            {
                foreach (string gift in gifts)
                {
                    if (!String.IsNullOrEmpty(gift) || !String.IsNullOrWhiteSpace(gift) || gift==" ")
                    {
                        text += gift + "\n";
                        Debug.WriteLine("saved: " + gift);
                    }
                }
            }
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
                    string file = sr.ReadToEnd();
                    string[] lines = new string[1];
                    if (file.Contains("\n==\r\n"))
                    {
                        lines = file.Split("\n==\r\n");
                    }
                    if (file.Contains("\n==\n"))
                    {
                        lines = file.Split("\n==\n");
                    }
                    string[] data = lines[0].Split(",");
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
                    this.isMouse = Boolean.Parse(data[11]);

                    string line = lines[1].Replace("\n\n", String.Empty).Replace("\r", String.Empty);
                    string[] giftsLoaded = line.Split("\n");
                    //Debug.WriteLine(line);
                    List<string> newGiftsList = new List<string>();
                    foreach(string gift in giftsLoaded)
                    {
                        if (!String.IsNullOrEmpty(gift) || !String.IsNullOrWhiteSpace(gift) || gift == " ")
                        {
                            newGiftsList.Add(gift);
                            Debug.WriteLine("Loaded: " + gift);
                        }
                    }
                    gifts = newGiftsList;
                }
            }
        }
        
    }
}
