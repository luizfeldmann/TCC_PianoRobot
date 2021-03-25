using System;
using System.Configuration;
using System.Diagnostics;
using System.Collections.Specialized;
using Manager = System.Configuration.ConfigurationManager;

namespace pianocam
{
    public static class Configs
    {
        
        private static Configuration ConfigFile;
        private static KeyValueConfigurationCollection AppSettings;

        private static void ReOpen()
        {
            if (ConfigFile != null && AppSettings != null)
                return;
            
            ConfigFile = Manager.OpenExeConfiguration(ConfigurationUserLevel.None);
            AppSettings = ConfigFile.AppSettings.Settings;
            Debug.WriteLine("CONFIG FILE OPEN");
        }

        public static string Read(string key)
        {
            ReOpen();
            if (AppSettings[key] == null)
                return "";
            
            return AppSettings[key].Value;
        }

        public static int ReadInt(string key)
        {
            ReOpen();
            if (AppSettings[key] == null)
                return 0;

            int i = Convert.ToInt32(AppSettings[key].Value);
                
            Debug.WriteLine("INT {0} = {1}", key, i);

            return i;
        }

        public static float ReadFloat(string key)
        {
            ReOpen();
            if (AppSettings[key] == null)
                return 0;


            float f = float.Parse(AppSettings[key].Value, System.Globalization.CultureInfo.InvariantCulture);

            Debug.WriteLine("FLOAT {0} = {1}", key, f);

            return f;
        }

        public static void OpenEditor()
        {
            ReOpen();
            Process.Start("open", string.Format("-t '{0}'", ConfigFile.FilePath));
        }
    }
}
