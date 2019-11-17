using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Management;
using System.Linq;

namespace EpochHive
{
    public class Config
    {
        public string Host { get; set; }
        public string Password { get; set; }
        public string Schema { get; set; }
        public string User { get; set; }
        public string LogLevel { get; set; }
        public string Time { get; set; }
        public int Hour { get; set; }
        public int Instance { get; set; }
        public Config()
        {

        }
        public static Config Load()
        {

            Config cfgx = new Config();
            try
            {
                var serverProcess = Process.GetProcessesByName("arma2oaserver");
                string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                foreach (var s in serverProcess)
                {
                    string processDir = s.MainModule.FileName.Replace("arma2oaserver.exe", "");
                    if (processDir == currentDir) {
                        string cmd = s.GetCommandLine();
                        int ind = cmd.IndexOf("-config=") + 8;
                        cmd = cmd.Remove(0, ind);
                        string cfgPath = cmd.Substring(0, cmd.IndexOf("\""));
                        cfgPath = cfgPath.Remove(cfgPath.LastIndexOf("\\") + 1);
                        Config cfg = JsonConvert.DeserializeObject<Config>(File.ReadAllText(cfgPath + "hiveCfg.json"));
                        return cfg;
                    }
                }


                    
            }
            catch (Exception e)
            {
                //File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "errorLog.txt", e.Message);
            }
            return null;
        }
    }
}
