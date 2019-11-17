using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;

namespace EpochHive
{
    public class Utility
    {
        
        public static HiveResult DelegateMethod(string child, string[] args)
        {
            HiveResult result = new HiveResult() { Success = false,Exception = "No Child Method Found" };
            switch (child)
            {
                //Object Calls
                case "300": result = Database.DeleteObjectFile(args[1]); break; // Object Stream
                case "302": result = Database.ObjectStream(args[1]); break; // Object Stream
                case "303": result = Database.UpdateObjectInventory(args[1], args[2], false);  break; //updateObjectInventory
                case "304": result = Database.DeleteObject(args[1], false); break; //deleteObject - server_deleteObj.sqf/server_deleteObjDirect.sqf/server_publishVehicle3.sqf
                case "305": result = Database.UpdateVehiclePosition(args[1], args[2], args[3]);break;//updateVehicleMovement
                case "306": result = Database.UpdateVehicleStatus(args[1], args[2], args[3], String2Bool(args[4])); break; //updateVehicleStatus 
                case "307": result = Database.ReturnDateTime(); break; //getDateTime (HiveExtApp.cpp) - server_monitor.sqf
                case "308": result = Database.PublishObject(args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]); break; //createObject -server_publishObject.sqf/server_publishVehicle.sqf/server_publishVehicle2.sqf/server_publishVehicle3.sqf/server_swapObject.sqf
                case "309": result = Database.UpdateObjectInventory(args[1], args[2], true); break; //updateObjectInventory
                case "310": result = Database.DeleteObject(args[1], true); break; //deleteObject - server_deleteObj.sqf/server_deleteObjDirect.sqf/server_publishVehicle3.sqf
                case "388": result = Database.FetchObjectID(args[1]); break; //fetchObjectId server_publishVehicle.sqf/server_publishVehicle2.sqf/server_publishVehicle3.sqf
                case "396": result = Database.UpdateObjectDatestamp(args[1], false); break; //updateDatestampObject - server_maintainArea.sqf
                case "397": result = Database.UpdateObjectDatestamp(args[1], true); break; //updateDatestampObject - server_maintainArea.sqf
                case "398": result = Database.UpdateVehicleStatus(args[1], args[2], args[3], true); break; //updateDatestampObject - server_maintainArea.sqf

                //Character Data Calls
                case "100": result = Database.FetchPlayerCharacters(args[1]); break; //TODO fetchCharacterInitial - server_playerLogin.sqf
                case "101": result = args.Length >= 5 ? Database.FetchCharacterInitial(args[1], args[2], args[3],args[4]) : Database.FetchCharacterInitial(args[1], args[2],args[3]); break; //TODO fetchCharacterInitial - server_playerLogin.sqf
                case "102": result = Database.FetchCharacterDetails(args[1]); break; //fetchCharacterDetails - server_playerSetup.sqf
                case "103": result = Database.RecordLogin(args[1], args[2], args[3]); break; //recordLogin - server_functions.sqf(dayz_recordLogin)
                case "201": result = Database.UpdateCharacter(args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14], args[15]); break; //playerUpdate(HiveExtApp.cpp) followed by updateCharacter (SqlCharDataSource) - server_playerSync.sqf
                case "202": result = Database.KillCharacter(args[1], args[2], args[3]); break; //killCharacter - server_playerDied.sqf
                case "203": result = Database.InitCharacter(args[1], args[2], args[3]); break; //initCharacter - server_playerLogin.sqf
                case "205": result = Database.UpdateGlobalCoins(args[1], args[2],args[3],args[4]); break; //Unknown Code - server_playerSync.sqf "Updates Global Coins"

                //Virtual Garage Calls
                case "800": result = Database.GetPlayerVehicles(args[1],args[2]); break;
                case "801": result = Database.GetVehicleForSpawn(args[1], args[2], args[3]); break;
                case "802": result = Database.StoreVehicle(args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14]); break;
                case "803": result = Database.MaintainVehicles(args[1]); break;
            }
            return result;
        }

        public static bool String2Bool(string val)
        {
            if (val.ToUpper() == "TRUE")
                return true;
            return false;
        }

    }
    public static class Extensions
    {
        public static string[] GetRangeAfter(this string[] array,int index)
        {
            var s = new List<string>();
            for(int i = index + 1;i<array.Length;i++)
                s.Add(array[i]);
            return s.ToArray();

        }
        public static string GetCommandLine(this Process process)
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            using (ManagementObjectCollection objects = searcher.Get())
            {
                return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
            }

        }
    }
}
