using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EpochHive
{
    public class Logger
    {
        public string Severity;
        public string LogText;
        public string ErrorMessage;
        public List<string> Params = new List<string>();

        public void Log()
        { 
            var log = new List<string>();
            log.Add(DateTime.Now.ToShortDateString() + ":" +  DateTime.Now.ToShortTimeString() + " " + Severity);
            log.Add(LogText);
            if (ErrorMessage != null)
                log.Add(ErrorMessage);
            log.Add("GIVEN PARAMETERS:");
            foreach (string s in Params)
                log.Add("\t " + s);
            log.Add("");

            File.AppendAllLines(AppDomain.CurrentDomain.BaseDirectory + "EpochHiveLOG.txt", log);

        }
    }
}
