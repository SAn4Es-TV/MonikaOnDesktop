using System;using System.Net.Http;using System.Net;using System.Text.RegularExpressions;using System.IO;using System.Threading.Tasks;using System.Diagnostics;


namespace SolicenTEAM
{
    public static class Updater
    {
        public static string gitUser;
        public static string gitRepo;
        public static string browserURL = "";
        public static string pathToArchive = "";
        public static bool readyToUpdate = false;
        public static string UpdateVersion = "";
        public static string CurrentVersion = "";
        public static string UpdateDescription = "";
        public static string responseString = "";
        public static string ExeFileName = "";
        public static string IgnoreFiles = "";

        public async static void DownloadUpdate(string gitUsername, string gitRepo)
        {
            Regex regexRegular = new Regex("\".*?.zip\"", RegexOptions.Multiline);
            var matches = regexRegular.Matches(responseString);
            var tempItem = "";

            if (browserURL == "")
            {
                Debug.WriteLine("browserURL - " + browserURL);
                foreach (var item in matches)
                {
                    if (item.ToString().StartsWith("\"browser"))
                    {
                        tempItem = item.ToString();
                        Debug.WriteLine("item - " + item);
                    }
                }

                regexRegular = new Regex("\".*?\"", RegexOptions.Multiline);
                matches = regexRegular.Matches(tempItem);

                foreach (var item in matches)
                {
                    if (item.ToString().StartsWith("\"https"))
                    {
                        //Console.WriteLine(item);
                        browserURL = item.ToString().Replace("\"", "");
                    }
                }
            }

            var pathArchive = Environment.CurrentDirectory + "\\" + gitRepo + ".zip";
            //Console.WriteLine(browserURL);
            Debug.WriteLine("browserURL - " + browserURL);
            WebClient webClient = new WebClient();
            if (File.Exists(pathArchive))
            File.Delete(pathArchive);

            // Продолжает попытки скачать файл если выбивает ошибку пути.
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (File.Exists(pathArchive)) { break; }
                    webClient.DownloadFile(browserURL, pathArchive);
                }
                catch
                {
                    Debug.WriteLine("Renty");
                    await Task.Delay(30000);
                    continue;
                }
            }






            pathToArchive = pathArchive; 
            readyToUpdate = true;
        }

        public async static void ExtractArchive()
        {
            while (!readyToUpdate)
            {
                Debug.WriteLine("Update not ready");
                await Task.Delay(10);
            }
            Debug.WriteLine("Create config");
            CreateConfig();
            Debug.WriteLine("Create version.ini");
            var path = Environment.CurrentDirectory + "\\";
            File.WriteAllText(path + "version.ini", UpdateVersion);
            await Task.Delay(100);
            /*
            string processName = "Updater";
            var arrayProcesses = Process.GetProcessesByName(processName);
            while (arrayProcesses == null || arrayProcesses.Length < 1)
            {
                Debug.WriteLine("Starting Updater");
                Process.Start("Updater.exe");
                Debug.WriteLine("Updater count: " + arrayProcesses.Length);
                await Task.Delay(2000);
                arrayProcesses = Process.GetProcessesByName(processName);
            }*/
            Debug.WriteLine("Starting Updater");
            Process.Start("Updater.exe");
            Debug.WriteLine("Exiting");
            Environment.Exit(0);

        }

        public async static Task GetUpdateVersion()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) { return; }
            if (responseString == "") { ResponseStringAsync(); }
            await Task.Delay(1000);

            Regex regexRegular = new Regex("\".*?\"", RegexOptions.Multiline);
            var matches = regexRegular.Matches(responseString);
            var tempItem = "";

            foreach (var item in matches)
            {
                if (item.ToString().StartsWith("\"v"))
                {
                    tempItem = item.ToString().Replace("v", "");
                    //Console.WriteLine(item);
                }
            }

            //Console.WriteLine(tempItem.ToString().Replace("\"", ""));
            await Task.Delay(100);
            GetCurrentVersion(tempItem.ToString().Replace("\"", ""));
            await Task.Delay(100);
            if (UpdateDescription == "") { GetUpdateDescription(); }
        }

        static HttpClient client = new HttpClient(); //
        static async void ResponseStringAsync()
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10; Win64; x64; rv:60.0) Gecko/20100101 Firefox/60.0");

            int x = 1;
            for (int i = 0; i < x; i++)
            {
                if (x == 200) { break; }
                try
                {
                    if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) { await Task.Delay(100); continue; }
                    await Task.Delay(1);
                    if (responseString != "") { break; }
                    var resultURL = "https://api.github.com/repos/" + gitUser + "/" + gitRepo + "/releases/latest";
                    responseString = await client.GetStringAsync(resultURL);
                    //Console.WriteLine(responseString);
                    if (responseString != "") { GetUpdateVersion(); }
                    await Task.Delay(30000); // Было 30000
                }
                catch (Exception e)
                {
                    x++;
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Количество попыток:  " + x);
                    continue;
                }
            }


        }

        public async static void GetUpdateDescription()
        {



            Regex regexRegular = new Regex("\".*?\"", RegexOptions.Multiline);
            var matches = regexRegular.Matches(responseString);
            var tempItem = "";
            int x = 1;

            if (responseString == "") { return; }
            if (UpdateVersion == "")
            {
                for (int i = 0; i < x; i++)
                {
                    await Task.Delay(600);
                    if (UpdateVersion != "") { break; }
                    x++;
                }
            }

            foreach (string str2 in responseString.Split(','))
            {
                if (str2 == responseString.Split(',')[responseString.Split(',').Length - 1])
                {
                    string pattern = @"\r\n";
                    //Console.WriteLine(str2.ToString());
                    tempItem = str2.ToString().Remove(str2.ToString().Length - 2);
                    tempItem = tempItem.Replace(pattern, "");
                    pattern = @"\";
                    tempItem = tempItem.Replace(pattern, "");
                    tempItem = tempItem.Replace("\"", "");
                    tempItem = tempItem.Substring(3);
                    string[] description = tempItem.Split('*');

                    foreach (string str in description)
                    {
                        var temp = str;
                        
                        //if (temp == description[0]) { continue; }
                        temp = str.Replace("*", "");
                        if (temp.StartsWith("body:")) { temp = temp.Substring(6); }
                        //UpdateDescription += " — ";
                        //UpdateDescription += temp + "\n\n";
                        UpdateDescription += temp + "\n";
                    }


                    //Console.WriteLine(tempItem);
                    break;
                }
            }
        }

        static async void GetCurrentVersion(string updateVersion)
        {
            var path = Environment.CurrentDirectory + "\\";
            if (File.Exists(path + "version.ini"))
            {
                string version = File.ReadAllText(path + "version.ini");
                if (updateVersion != version)
                {
                    await Task.Delay(100);
                    //DownloadUpdate(gitUser, gitRepo);
                    UpdateVersion = updateVersion;
                }
                else if (updateVersion == version)
                {
                    UpdateVersion = updateVersion;
                }
            }
            else
            {
                File.WriteAllText(path + "version.ini", "1.0.0");
                //DownloadUpdate(gitUser, gitRepo);

            }
        }

        public static void CreateConfig()
        {
            if (ExeFileName != "")
            {
                var allInfo = ExeFileName + "\n" + IgnoreFiles;
                //var allInfo = ExeFileName;
                File.WriteAllText(Environment.CurrentDirectory + "\\" + "UpdateConfig.ini", allInfo);
            }
        }

        public static async Task CheckUpdate(string GitUsername, string GitRepo)
        {
            if (File.Exists(Environment.CurrentDirectory + "\\" + "version.ini"))
                CurrentVersion = File.ReadAllText(Environment.CurrentDirectory + "\\" + "version.ini");

            await Task.Delay(100);
            gitUser = GitUsername;
            gitRepo = GitRepo;

            await GetUpdateVersion();
            await Task.Delay(10000);
            CurrentVersion = File.ReadAllText
                (Environment.CurrentDirectory + "\\" + "version.ini");

            Console.WriteLine("Текущая версия: " + CurrentVersion);
            Console.WriteLine("Версия обновления: " + SolicenTEAM.Updater.UpdateVersion);
            if (SolicenTEAM.Updater.UpdateVersion != CurrentVersion && SolicenTEAM.Updater.UpdateVersion != "")
            {

            }
            

        }
    }
}
