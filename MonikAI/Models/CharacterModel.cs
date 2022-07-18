using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Linq;

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
        public int Scaler = 100;
        public bool screenNum = false;
        public bool isMouse = false;
        public string costumeName = "def";
        public int secondTogether = 0;

        public List<string> gifts = new List<string>();


        public string leaningWord = "leaning";
        public string nullPath = "null";
        public string[] body = new string[] { "arms-crossed-5",
            "arms-crossed-10",
            "arms-leaning-def-left-def-10",
            "arms-leaning-def-right-def-5",
            "arms-leaning-def-right-def-10",
            "arms-left-down-0",
            "arms-left-rest-10",
            "arms-right-down-0",
            "arms-right-point-0",
            "arms-right-restpoint-10",
            "arms-steepling-10",
            "body-def-0",
            "body-def-1",
            "body-def-head",
            "body-leaning-def-0",
            "body-leaning-def-1",
            "body-leaning-def-head"};
        public string[] eyes = new string[] { "eyes-closedhappy",
            "eyes-closedsad",
            "eyes-crazy",
            "eyes-left",
            "eyes-normal",
            "eyes-right",
            "eyes-smug",
            "eyes-smugleft",
            "eyes-smugright",
            "eyes-soft",
            "eyes-sparkle",
            "eyes-wide",
            "eyes-winkleft",
            "eyes-winkright",};
        public string[] eyesBrow = new string[] { "eyebrows-furrowed",
            "eyebrows-knit",
            "eyebrows-mid",
            "eyebrows-think",
            "eyebrows-up"};
        public string[] mouth = new string[] { "mouth-angry",
            "mouth-big",
            "mouth-gasp",
            "mouth-pout",
            "mouth-small",
            "mouth-smile",
            "mouth-smirk",
            "mouth-smug",
            "mouth-triangle",
            "mouth-wide",};

        public string hairType = "def";
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
            
            XDocument xDoc = new XDocument();
            XElement xml = new XElement("xml");
            XElement mainSettings = new XElement("settings");
            XElement affection_ = new XElement("affection"); affection_.Value = affection.ToString();
            XElement pcName_ = new XElement("pcName"); pcName_.Value = pcName;
            XElement player_ = new XElement("player"); player_.Value = playerName;
            XElement lang_ = new XElement("lang"); lang_.Value = lang;
            XElement autoStart_ = new XElement("autoStart"); autoStart_.Value = autoStart.ToString();
            XElement idleRandomFrom_ = new XElement("idleRandomFrom"); idleRandomFrom_.Value = idleRandomFrom.ToString();
            XElement idleRandomTo_ = new XElement("idleRandomTo"); idleRandomTo_.Value = idleRandomTo.ToString();
            XElement NightStart_ = new XElement("NightStart"); NightStart_.Value = NightStart.ToString();
            XElement nightEnd_ = new XElement("nightEnd"); nightEnd_.Value = nightEnd.ToString();
            XElement Scaler_ = new XElement("Scaler"); Scaler_.Value = Scaler.ToString();
            XElement screenNum_ = new XElement("screenNum"); screenNum_.Value = screenNum.ToString();
            XElement isMouse_ = new XElement("isMouse"); isMouse_.Value = isMouse.ToString();
            XElement secondTogether_ = new XElement("secondTogether"); secondTogether_.Value = secondTogether.ToString();
            XElement costume_ = new XElement("costume"); costume_.Value = costumeName.ToString();

            mainSettings.Add(affection_);
            mainSettings.Add(pcName_);
            mainSettings.Add(player_);
            mainSettings.Add(lang_);
            mainSettings.Add(autoStart_);
            mainSettings.Add(idleRandomFrom_);
            mainSettings.Add(idleRandomTo_);
            mainSettings.Add(NightStart_);
            mainSettings.Add(nightEnd_);
            mainSettings.Add(Scaler_);
            mainSettings.Add(screenNum_);
            mainSettings.Add(isMouse_);
            mainSettings.Add(secondTogether_);
            mainSettings.Add(costume_);

            XElement gifts_ = new XElement("gifts");
            if (gifts.Count != 0)
            {
                foreach (string gift in gifts)
                {
                    if (!String.IsNullOrEmpty(gift) || !String.IsNullOrWhiteSpace(gift) || gift==" ")
                    {
                        XElement gift_ = new XElement("gift"); gift_.Value = gift;
                        gifts_.Add(gift_);
                        Debug.WriteLine("saved: " + gift);
                    }
                }
            }
            xml.Add(mainSettings);
            xml.Add(gifts_);
            xDoc.Add(xml);

            DirectoryInfo dirInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "/characters");
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            xDoc.Save(filePath);
        }
        public void loadData()
        {
            if (File.Exists(filePath))
            {
                XDocument xDoc = XDocument.Load(filePath);
                XElement xml = xDoc.Element("xml");
                if (xml != null)
                {
                    XElement settings = xml.Element("settings");
                    if (settings != null)
                    {

                        // проходим по всем элементам person
                        foreach (XElement option in settings.Elements())
                        {
                            switch (option.Name.LocalName)
                            {
                                case "affection":
                                    affection = int.Parse(option.Value);
                                    break;
                                case "pcName":
                                    pcName = option.Value;
                                    break;
                                case "player":
                                    playerName = option.Value;
                                    break;
                                case "lang":
                                    lang = option.Value;
                                    break;
                                case "autoStart":
                                    autoStart = bool.Parse(option.Value);
                                    break;
                                case "idleRandomFrom":
                                    idleRandomFrom = int.Parse(option.Value);
                                    break;
                                case "idleRandomTo":
                                    idleRandomTo = int.Parse(option.Value);
                                    break;
                                case "NightStart":
                                    NightStart = int.Parse(option.Value);
                                    break;
                                case "nightEnd":
                                    nightEnd = int.Parse(option.Value);
                                    break;
                                case "Scaler":
                                    Scaler = int.Parse(option.Value);
                                    break;
                                case "screenNum":
                                    screenNum = bool.Parse(option.Value);
                                    break;
                                case "isMouse":
                                    isMouse = bool.Parse(option.Value);
                                    break;
                                case "secondTogether":
                                    secondTogether = int.Parse(option.Value);
                                    break;
                                case "costume":
                                    costumeName = option.Value;
                                    break;
                            }
                        }
                    }
                    XElement gifts_ = xml.Element("gifts");
                    if (gifts_ != null)
                    {
                        List<string> newGiftsList = new List<string>();
                        // проходим по всем элементам person
                        foreach (XElement gift in gifts_.Elements("gift"))
                        {
                            newGiftsList.Add(gift.Value);
                            Debug.WriteLine("Loaded: " + gift.Value);
                        }
                        gifts = newGiftsList;
                    }
                }
            }
        }

        public void saveData_()
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
            text += secondTogether + ",";
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
        public void loadData_()
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

                    if (!String.IsNullOrEmpty(data[10]))
                        this.screenNum = Boolean.Parse(data[10]);
                    else
                        this.screenNum = false;

                    if (!String.IsNullOrEmpty(data[11]))
                        this.isMouse = Boolean.Parse(data[11]);
                    else
                        this.isMouse = false;

                    if (!String.IsNullOrEmpty(data[12]))
                        this.secondTogether = int.Parse(data[12]);
                    else
                        this.secondTogether = 0;

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
