using System;
using System.Management;
using System.IO;
using Microsoft.Win32;

namespace OZI_lab2_infocollector
{
    class Program
    {
        static void Main(string[] args)
        {
            string computerInfo = "";

            //інформація про комп'ютер
            SelectQuery SqComp = new SelectQuery("Win32_OperatingSystem");
            ManagementObjectSearcher compOSDetails = new ManagementObjectSearcher(SqComp);
            ManagementObjectCollection osDetailsCollectionComp = compOSDetails.Get();
            foreach (ManagementObject comp in osDetailsCollectionComp)
            {
                computerInfo += comp["SystemDirectory"].ToString();
                computerInfo += comp["WindowsDirectory"].ToString();
                Console.WriteLine("System directory: {0}", comp["SystemDirectory"].ToString());
                Console.WriteLine("Windows directory: {0}", comp["WindowsDirectory"].ToString());
            }
            computerInfo += Environment.MachineName;
            computerInfo += Environment.UserName;
            Console.WriteLine("Computer name: {0}", Environment.MachineName);
            Console.WriteLine("User name: {0}", Environment.UserName);

            //інформація про тип і підтип клавіатури
            ManagementObjectSearcher searchKeyboardType = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Keyboard");
            Console.WriteLine("Information about keyboard : ");
            foreach (ManagementObject queryObj in searchKeyboardType.Get())
            {
                Console.WriteLine("Name: {0}", queryObj["Name"]);
                Console.WriteLine("Description: {0}", queryObj["Description"]);
            }

            // інформація про висоту та ширину екрану
            ManagementObjectSearcher searchScreenInfo = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");

            System.Collections.Generic.List<Scr> scr = new System.Collections.Generic.List<Scr>();
            Console.WriteLine("Information about screen : ");
            foreach (ManagementObject queryObj in searchScreenInfo.Get())
            {
                if (queryObj["CurrentHorizontalResolution"] != null)
                {
                    Scr screen = new Scr();
                    screen.Width = queryObj["CurrentHorizontalResolution"].ToString();
                    screen.Height = queryObj["CurrentVerticalResolution"].ToString();
                    scr.Add(screen);
                }
            }
            foreach (var item in scr)
            {
                computerInfo += item.Width.ToString();
                Console.WriteLine($"Height = {item.Height}, Width = {item.Width} ");
            }

            // інформація про обсяг пам'яті
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Volume");

            System.Collections.Generic.List<SaveInfo> s = new System.Collections.Generic.List<SaveInfo>();
            Console.WriteLine("Information about disc : ");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                SaveInfo newS = new SaveInfo();
                if (queryObj["DriveLetter"] != null)
                {
                    newS.Letter = queryObj["DriveLetter"].ToString();
                    newS.Capacity = queryObj["Capacity"].ToString();
                    newS.FreeSpace = queryObj["FreeSpace"].ToString();
                    s.Add(newS);
                }
            }
            foreach (var item in s)
            {
                computerInfo += item.Capacity.ToString();
                Console.WriteLine($"letter = {item.Letter} ,capasity = {item.Capacity}");
            }

            //серійний номер диску, на якому встановлена програма
            ManagementObjectSearcher moSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject wmi_HD in moSearcher.Get())
            {
                computerInfo += wmi_HD["SerialNumber"].ToString();
                Console.WriteLine("Serial number:" + wmi_HD["SerialNumber"].ToString());
            }

            //файлова система диску, на якому встановлена програма
            string root = Path.GetPathRoot(System.Reflection.Assembly.GetEntryAssembly().Location);
            DriveInfo driveInfo = new DriveInfo(root);
            computerInfo += driveInfo.DriveFormat;
            Console.WriteLine(driveInfo.DriveFormat);


            Console.WriteLine();
            Console.WriteLine("All computer info : {0}", computerInfo);

            var key = "gheju392pkjd902bhfj334j22030893j";
            var encryptedComputerInfo = Encryption.Encrypt(key, computerInfo);
            Console.WriteLine("Encrypted info : {0}", encryptedComputerInfo);
            SaveEncryptedInfo(encryptedComputerInfo);
            ReadEncryptedInfo();

            Console.ReadKey();

        }

        public static void SaveEncryptedInfo(string encryptedInfo)
        {
            RegistryKey userKey = Registry.CurrentUser;
            RegistryKey personalKey = userKey.CreateSubKey("Khaniuchenko_");
            personalKey.SetValue("Khaniuchenko", encryptedInfo);
            personalKey.Close();
            Console.WriteLine();
            Console.WriteLine("Sign is added to registry");
            Console.WriteLine();


        }

        public static void ReadEncryptedInfo()
        {
            RegistryKey currentUserKey = Registry.CurrentUser;
            RegistryKey personalKey = currentUserKey.OpenSubKey("Khaniuchenko_", true);

            try
            {
                string signedData = personalKey.GetValue("KhaniuchenkoDD").ToString();
                Console.WriteLine(signedData);
            }
            catch
            {
                Console.WriteLine("Wrong key is provided. You can't install program");
                Console.ReadKey();
            }

            personalKey.Close();
        }

        public static void DeleteKey()
        {
            RegistryKey currentUserKey = Registry.CurrentUser;
            RegistryKey helloKey = currentUserKey.OpenSubKey("Khaniuchenko_", true);
            helloKey.DeleteValue("Khaniuchenko");
            currentUserKey.DeleteSubKey("Khaniuchenko_");
        }
    }


    class Scr
    {
        public string Height { get; set; }
        public string Width { get; set; }
    }
    class SaveInfo
    {
        public object Letter { get; set; }
        public object Capacity { get; set; }
        public object FreeSpace { get; set; }
    }
}
