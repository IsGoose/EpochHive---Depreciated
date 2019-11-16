using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using RGiesecke.DllExport;

namespace EpochHive
{
    public class Entry
    {
        public static Config Config { get; set; }
        public static Logger Logger { get; set; }

        [DllExport("_RVExtension@12", CallingConvention = System.Runtime.InteropServices.CallingConvention.Winapi)]
        public static void RVExtension(StringBuilder output, int outputSize, [MarshalAs(UnmanagedType.LPStr)] string function)
        {
            try { 
            outputSize--;
            string[] sqfParams = function.Split(new char[] { ':' },StringSplitOptions.RemoveEmptyEntries);
            var result = RunHive(sqfParams);
            output.Append(result.Result);
            }catch(Exception e)
            {
                //File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "errorLog.txt", e.Message);
            }
        }
        public static HiveResult RunHive(string[] p)
        {
            Logger = new Logger();
            Config = Config.Load();
            var hiveResult = new HiveResult() { Result = "[\"FAIL\"]" };
            if (Config != null)
            {
                var result = Database.Connect(Config);
                if (!result.Success)
                {
                    Logger.Severity = "CRITICAL";
                    Logger.LogText = "HIVE Could not connect to MySQL Server";
                    Logger.ErrorMessage = result.Exception;
                    Logger.Params.AddRange(new string[] { "HOST: " + Config.Host, "PASSWORD: " + Config.Password, "USERNAME: " + Config.User, "SCHEMA: " + Config.Schema });
                    Logger.Log();

                    return result;
                }

                if (p.Length < 2)
                {
                    Logger.Severity = "CRITICAL";
                    Logger.LogText = "Not enough Parameters supplied to call";
                    Logger.ErrorMessage = null;
                    Logger.Params.AddRange(p);
                    Logger.Log();
                    hiveResult.Success = false;
                    hiveResult.Result = "[\"FAIL\"]";
                    return hiveResult;
                }

                string child = p[1];
                HiveResult res = Utility.DelegateMethod(child, p.GetRangeAfter(0));
                Database.Disconnect();
                if (res.Success)
                {
                    if (res.Result == null)
                        res.Result = "[\"PASS\"]";
                    else
                        res.Result = "[\"PASS\"," + res.Result + "]";
                    //TODO return result
                    Console.WriteLine(res.Result);
                    if (Config.LogLevel.ToLower() == "debug")
                    {
                        Logger.Severity = "DEBUG";
                        Logger.LogText = $"CHILD {child} execution SUCCESS";
                        Logger.ErrorMessage = res.Result;
                        Logger.Params.AddRange(p.GetRangeAfter(0));
                        Logger.Log();
                    }
                    return res;
                }
                else
                {
                    res.Result = "[\"FAIL\"]";
                    Logger.LogText = $"CHILD {child} execution FAILED";
                    Logger.Severity = "CRITICAL";
                    Logger.ErrorMessage = res.Exception == null ? "" : res.Exception;
                    Logger.Params.AddRange(p.GetRangeAfter(0));
                    Logger.Log();
                }
            } else
            {
                Logger.Severity = "CRITICAL";
                Logger.LogText = "Unable to load Hive Config File (hivecfg.json)";
                Logger.Params.AddRange(p.GetRangeAfter(0));
                Logger.Log();
                hiveResult.Result = "[\"FAIL\"]";
                hiveResult.Success = false;
            }
            return hiveResult;
        }
    }
}
