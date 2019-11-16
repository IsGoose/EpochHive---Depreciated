﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace EpochHive
{
    public class Database
    {
        public static string ConnectionString { get; set; }
        public static MySqlConnection Connection { get; set; }


        /// <summary>
        /// Carries out simple SQL Read to determine if a player is new to Database or not
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static bool IsNewPlayer(string uid)
        {
            MySqlDataReader reader = ExecuteBasicRead($"select PlayerUID from player_data where PlayerUID = \"{uid}\";");
            if (reader.Read())
            {
                reader.Close();
                return false;
            }
            reader.Close();
            return true;
        }


        /// <summary>
        /// Fetch Character Data for server_playerSetup.sqf
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        public static HiveResult FetchCharacterDetails(string cid)
        {
            var result = new HiveResult();
            List<string> data = new List<string>();
            MySqlDataReader reader = ExecuteBasicRead("SELECT `Worldspace`, `Medical`, `KillsZ`, `HeadshotsZ`, `KillsH`, `KillsB`, `CurrentState`, `Humanity`, `InstanceID` " +
            $"FROM `Character_DATA` WHERE `CharacterID`=\"{cid}\";");
            if (reader.Read())
            {
                string ws = reader["Worldspace"].ToString();
                string medical = reader["Medical"].ToString();
                string stats = $"[{reader["KillsZ"].ToString()},{reader["HeadshotsZ"].ToString()},{reader["KillsH"].ToString()},{reader["KillsB"].ToString()}]";
                string currentState = reader["CurrentState"].ToString();
                string humanity = reader["Humanity"].ToString();
                string instance = reader["InstanceID"].ToString();
                data.Add(medical);
                data.Add(stats);
                data.Add(currentState);
                data.Add(ws);
                data.Add(humanity);
                data.Add(instance);
                reader.Close();
                result.Success = true;
                result.Result = string.Join(",", data);
                return result;
            }
            reader.Close();
            result.Success = false;
            return result;
        }

        public static HiveResult FetchPlayerCharacters(string uid)
        {
            HiveResult res = new HiveResult();
            var chars = new List<string>();
            var reader = ExecuteBasicRead($"SELECT `CharacterID`, `Slot`, `Worldspace`, `Alive`, `Generation`, `Humanity`, `KillsZ`, `HeadshotsZ`, `KillsH`, `KillsB`, `DistanceFoot`, `Model`, `Infected`,TIMESTAMPDIFF(MINUTE,`LastLogin`,NOW()) as `LastLoginDiff`,TIMESTAMPDIFF(MINUTE,`Datestamp`,`LastLogin`) as `SurvivalTime` FROM `Character_DATA` `cd1`INNER JOIN (SELECT MAX(`CharacterID`) `MaxCharacterID` FROM `Character_DATA` WHERE `PlayerUID` = \"{uid}\" GROUP BY `Slot`) `cd2` ON `cd1`.`CharacterID` = `cd2`.`MaxCharacterID` ORDER BY `Slot`");
            if(!reader.HasRows)
            {
                res.Success = false;
                res.Exception = "No Characters Found!";
                return res;
            }
            while (reader.Read())
            {
                List<string> chardata = new List<string>();
                string charId = reader["CharacterID"].ToString();
                string slot = "\"" + reader["Slot"].ToString() + "\"";
                string ws = reader["Worldspace"].ToString() != "" ? reader["Worldspace"].ToString() : "[]";
                string alive = reader["Alive"].ToString();
                string generation = reader["Generation"].ToString();
                string humanity = reader["Humanity"].ToString();
                string killsZ = reader["KillsZ"].ToString();
                string headZ = reader["HeadshotsZ"].ToString();
                string killsH = reader["KillsH"].ToString();
                string killsB = reader["KillsB"].ToString();
                string distanceFoot = reader["DistanceFoot"].ToString();
                string model = "\"" +  reader["Model"].ToString() + "\"";
                string infected = reader["Infected"].ToString();
                string lastLog = reader["LastLoginDiff"].ToString();
                string survTime = reader["SurvivalTime"].ToString();
                chardata.Add(charId);
                chardata.Add(slot);
                chardata.Add(ws);
                chardata.Add(alive);
                chardata.Add(generation);
                chardata.Add(humanity);
                chardata.Add($"[{killsZ},{headZ},{killsH},{killsB}]");
                chardata.Add(distanceFoot);
                chardata.Add(model);
                chardata.Add(infected);
                chardata.Add(lastLog);
                chardata.Add(survTime);
                chars.Add(string.Join(",",chardata));
            }
            res.Result = string.Join(",", chars);
            res.Success = true;
            return res;
        }

        /// <summary>
        /// Overload of FetchCharacterInitial - Support for Multiple Characters
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="dayzInstance"></param>
        /// <param name="playername"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public static HiveResult FetchCharacterInitial(string uid, string dayzInstance, string playername,string slot)
        {
            var result = new HiveResult();
            List<string> data = new List<string>();
            bool isNew = IsNewPlayer(uid);
            string playerGroup = "[]";
            string playerCoins = "0";
            string bankCoins = "0";
            if (!isNew)
            {
                MySqlDataReader firstreader = ExecuteBasicRead($"select PlayerName,PlayerSex,playerGroup,PlayerCoins,BankCoins from Player_DATA where PlayerUID = \"{uid}\";");
                if (firstreader.Read())
                {
                    playerGroup = firstreader[2].ToString();
                    playerCoins = firstreader[3].ToString();
                    bankCoins = firstreader[4].ToString();
                    if (firstreader[0].ToString() != playername)
                    {
                        firstreader.Close();
                        ExecuteNoReturn($"update Player_DATA set PlayerName = \"{playername}\" where PlayerUID = \"{uid}\";");
                    }
                    else
                    {
                        firstreader.Close();
                    }
                }
                if (!firstreader.IsClosed)
                    firstreader.Close();

            }
            if (isNew)
            {
                ExecuteNoReturn($"insert into Player_DATA (PlayerUID,PlayerName,playerGroup) values (\"{uid}\",\"{playername}\",\"{playerGroup}\");");
            }
            MySqlDataReader reader = ExecuteBasicRead($"SELECT `CharacterID`, `Worldspace`, `Inventory`, `Backpack`, TIMESTAMPDIFF(MINUTE,`Datestamp`,`LastLogin`) as `SurvivalTime`,TIMESTAMPDIFF(MINUTE,`LastAte`,NOW()) as `MinsLastAte`, " +
             $"TIMESTAMPDIFF(MINUTE,`LastDrank`,NOW()) as `MinsLastDrank`, `Model`,`duration`,`Coins` FROM `Character_DATA` WHERE `PlayerUID` = \"{uid}\" AND `Slot` = {slot} AND `Alive` = 1 ORDER BY `CharacterID` DESC LIMIT 1");
            string infected = "0";
            string charid = "-1";
            string worldspace = "[]";
            string inventory = "[]";
            string backpack = "[]";
            string[] survival = { "0", "0", "0", "0" };
            string characterCoins = "0";
            string model = "";
            bool isNewChar = true;
            if (reader.HasRows && reader.Read())
            {
                isNewChar = false;
                charid = reader[0].ToString();
                worldspace = reader[1].ToString();
                inventory = reader[2].ToString();
                backpack = reader[3].ToString();
                survival[0] = reader[4].ToString();
                survival[1] = reader[5].ToString();
                survival[2] = reader[6].ToString();
                survival[3] = reader[8].ToString();
                model = reader[7].ToString();
                characterCoins = reader[9].ToString();
                reader.Close();
                ExecuteNoReturn($"update Character_DATA set LastLogin = CURRENT_TIMESTAMP where CharacterID = \"{charid}\";");
            }
            else
            {
                int gen = -1;
                string humanity = "2500";
                reader.Close();
                reader = ExecuteBasicRead($"SELECT `Generation`, `Humanity`, `Model`, `Infected` FROM `Character_DATA` WHERE PlayerUID = \"{uid}\" AND `Alive` = 0 ORDER BY `CharacterID` DESC LIMIT 1");
                if (reader.HasRows && reader.Read())
                {
                    gen = int.Parse(reader[0].ToString());
                    gen++;

                    humanity = reader[1].ToString();
                    model = reader[2].ToString();
                    infected = reader[3].ToString();
                    reader.Close();
                }
                if (!reader.IsClosed)
                    reader.Close();
                string medical = "[]";
                ExecuteNoReturn("INSERT INTO `Character_DATA` (`PlayerUID`, `InstanceID`, `Worldspace`, `Inventory`, `Backpack`, `Medical`, `Generation`, `Datestamp`, `LastLogin`, `LastAte`, `LastDrank`, `Humanity`) values " +
                $"('{uid}','11', '{worldspace}', '{inventory}', '{backpack}', '{medical}', '{gen}', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, '{humanity}');");
                reader = ExecuteBasicRead($"select CharacterID from Character_DATA where PlayerUID = \"{uid}\" and Alive = '1';");
                if (reader.HasRows && reader.Read())
                {
                    charid = reader[0].ToString();
                }
                else
                {
                    result.Success = false;
                    result.Exception = "Character does not exist in the Database";
                    return result;
                }
            }
            data.Add(isNew.ToString());
            data.Add("\"" + charid + "\"");
            if (!isNewChar)
            {
                data.Add(worldspace);
                data.Add(inventory);
                data.Add(backpack);
                data.Add($"[{survival[0]},{survival[1]},{survival[2]},{survival[3]}]");
                data.Add(characterCoins);
            }
            else
            {
                data.Add(infected);
            }
            if (model == "")
                model = "\"\"";
            data.Add(model);
            data.Add(playerGroup);
            data.Add(playerCoins);
            data.Add(bankCoins);
            data.Add("\"SW-HIVE V.0\"");
            result.Success = true;
            result.Result = string.Join(",", data);
            return result;
        }

        /// <summary>
        /// Update Player Name, Update new character information and return new information for server_playerLogin.sqf
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="playername"></param>
        /// <returns></returns>
        public static HiveResult FetchCharacterInitial(string uid,string dayzInstance, string playername)
        {
            var result = new HiveResult();
            List<string> data = new List<string>();
            bool isNew = IsNewPlayer(uid);
            string playerGroup = "[]";
            string playerCoins = "0";
            string bankCoins = "0";
            if (!isNew)
            {
                MySqlDataReader firstreader = ExecuteBasicRead($"select PlayerName,PlayerSex,playerGroup,PlayerCoins,BankCoins from Player_DATA where PlayerUID = \"{uid}\";");
                if (firstreader.Read())
                {
                    playerGroup = firstreader[2].ToString();
                    playerCoins = firstreader[3].ToString();
                    bankCoins = firstreader[4].ToString();
                    if (firstreader[0].ToString() != playername)
                    {
                        firstreader.Close();
                        ExecuteNoReturn($"update Player_DATA set PlayerName = \"{playername}\" where PlayerUID = \"{uid}\";");
                    }
                    else
                    {
                        firstreader.Close();
                    }
                }
                if (!firstreader.IsClosed)
                    firstreader.Close();

            }
            if (isNew)
            {
                ExecuteNoReturn($"insert into Player_DATA (PlayerUID,PlayerName,playerGroup) values (\"{uid}\",\"{playername}\",\"{playerGroup}\");");
            }
            MySqlDataReader reader = ExecuteBasicRead($"SELECT `CharacterID`, `Worldspace`, `Inventory`, `Backpack`, TIMESTAMPDIFF(MINUTE,`Datestamp`,`LastLogin`) as `SurvivalTime`,TIMESTAMPDIFF(MINUTE,`LastAte`,NOW()) as `MinsLastAte`, " +
             $"TIMESTAMPDIFF(MINUTE,`LastDrank`,NOW()) as `MinsLastDrank`, `Model`,`duration`,`Coins` FROM `Character_DATA` WHERE `PlayerUID` = \"{uid}\" AND `Alive` = 1 ORDER BY `CharacterID` DESC LIMIT 1");
            string infected = "0";
            string charid = "-1";
            string worldspace = "[]";
            string inventory = "[]";
            string backpack = "[]";
            string[] survival = { "0", "0", "0", "0" };
            string characterCoins = "0";
            string model = "";
            bool isNewChar = true;
            if (reader.HasRows && reader.Read())
            {
                isNewChar = false;
                charid = reader[0].ToString();
                worldspace = reader[1].ToString();
                inventory = reader[2].ToString();
                backpack = reader[3].ToString();
                survival[0] = reader[4].ToString();
                survival[1] = reader[5].ToString();
                survival[2] = reader[6].ToString();
                survival[3] = reader[8].ToString();
                model = reader[7].ToString();
                characterCoins = reader[9].ToString();
                reader.Close();
                ExecuteNoReturn($"update Character_DATA set LastLogin = CURRENT_TIMESTAMP where CharacterID = \"{charid}\";");
            }
            else
            {
                int gen = -1;
                string humanity = "2500";
                reader.Close();
                reader = ExecuteBasicRead($"SELECT `Generation`, `Humanity`, `Model`, `Infected` FROM `Character_DATA` WHERE PlayerUID = \"{uid}\" AND `Alive` = 0 ORDER BY `CharacterID` DESC LIMIT 1");
                if (reader.HasRows && reader.Read())
                {
                    gen = int.Parse(reader[0].ToString());
                    gen++;

                    humanity = reader[1].ToString();
                    model = reader[2].ToString();
                    infected = reader[3].ToString();
                    reader.Close();
                }
                if (!reader.IsClosed)
                    reader.Close();
                string medical = "[]";
                ExecuteNoReturn("INSERT INTO `Character_DATA` (`PlayerUID`, `InstanceID`, `Worldspace`, `Inventory`, `Backpack`, `Medical`, `Generation`, `Datestamp`, `LastLogin`, `LastAte`, `LastDrank`, `Humanity`) values " +
                $"('{uid}','11', '{worldspace}', '{inventory}', '{backpack}', '{medical}', '{gen}', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, '{humanity}');");
                reader = ExecuteBasicRead($"select CharacterID from Character_DATA where PlayerUID = \"{uid}\" and Alive = '1';");
                if (reader.HasRows && reader.Read())
                {
                    charid = reader[0].ToString();
                }
                else
                {
                    result.Success = false;
                    result.Exception = "Character does not exist in the Database";
                    return result;
                }
            }
            data.Add(isNew.ToString());
            data.Add("\"" + charid + "\"");
            if (!isNewChar)
            {
                data.Add(worldspace);
                data.Add(inventory);
                data.Add(backpack);
                data.Add($"[{survival[0]},{survival[1]},{survival[2]},{survival[3]}]");
                data.Add(characterCoins);
            }
            else
            {
                data.Add(infected);
            }
            if (model == "")
                model = "\"\"";
            data.Add(model);
            data.Add(playerGroup);
            data.Add(playerCoins);
            data.Add(bankCoins);
            data.Add("1");
            result.Success = true;
            result.Result = string.Join(",", data);
            return result;
        }

        /// <summary>
        /// Returns the ObjectID from the Database for the object of the given ObjectUID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static HiveResult FetchObjectID(string uid)
        {
            HiveResult result = new HiveResult();
            try
            {
                MySqlDataReader reader = ExecuteBasicRead($"select ObjectID from object_data where ObjectUID = \"{uid}\";");
                if (reader.Read())
                {
                    result.Success = true;
                    result.Result = reader["ObjectID"].ToString();
                    return result;
                }
                result.Exception = "Unable to get ObjectID from ObjectUID";
                result.Success = false;
                return result;
            } catch(Exception e)
            {
                result.Success = false;
                result.Exception = e.Message;
                return result;
            }

        }
        /// <summary>
        /// Updates Global Coins
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="globalcoins"></param>
        public static HiveResult UpdateGlobalCoins(string uid, string globalcoins)
        {
            return ExecuteNoReturn($"update Player_DATA set BankCoins = '{globalcoins}' where PlayerUID = '{uid}';");
        }
        /// <summary>
        /// Initalise Character
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="inventory"></param>
        /// <param name="backpack"></param>
        public static HiveResult InitCharacter(string cid, string inventory, string backpack)
        {
            return ExecuteNoReturn($"UPDATE `Character_DATA` SET `Inventory` = '{inventory}' , `Backpack` = '{backpack}' WHERE `CharacterID` = '{cid}';");
        }
        /// <summary>
        /// "Kill" Character in database
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="duration"></param>
        /// <param name="infected"></param>
        public static HiveResult KillCharacter(string cid, string duration, string infected = "0")
        {
            return ExecuteNoReturn($"UPDATE `Character_DATA` SET `Alive` = 0, `Infected` = '{infected}', `LastLogin` = DATE_SUB(CURRENT_TIMESTAMP, INTERVAL {duration} MINUTE) WHERE `CharacterID` = '{cid}' AND `Alive` = 1;");
        }
        /// <summary>
        /// Update Character Attributes
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="ws"></param>
        /// <param name="gear"></param>
        /// <param name="backpack"></param>
        /// <param name="medical"></param>
        /// <param name="kills"></param>
        /// <param name="hs"></param>
        /// <param name="distancefoot"></param>
        /// <param name="timesince"></param>
        /// <param name="currentstate"></param>
        /// <param name="killsH"></param>
        /// <param name="killsB"></param>
        /// <param name="model"></param>
        /// <param name="humanity"></param>
        /// <param name="coins"></param>
        /// <returns></returns>
        public static HiveResult UpdateCharacter(string cid,
           string ws, string gear, string backpack,
           string medical,
           string kills, string hs, string distancefoot,
           string timesince, string currentstate,
           string killsH, string killsB, string model,
           string humanity, string coins = "0"
           )
        {

            return ExecuteNoReturn("update Character_DATA set " +
                $"Worldspace = '{ws}',Inventory = '{gear}',Backpack = '{backpack}',Medical = '{medical}',KillsZ = '{kills}'," +
                $"HeadshotsZ = '{hs}',DistanceFoot = '{distancefoot}',Duration = '{timesince}',CurrentState = '{currentstate}'," +
                $"KillsH = '{killsH}',KillsB = '{killsB}',Model = '{model}',Humanity = '{humanity}',Coins = '{coins}' " +
                $"where CharacterID = '{cid}';"
                );
        }
        /// <summary>
        /// Record the Login of player and action
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="cid"></param>
        /// <param name="action"></param>
        public static HiveResult RecordLogin(string uid, string cid, string action)
        {
            return ExecuteNoReturn($"insert into Player_LOGIN (PlayerUID,CharacterID,Datestamp,Action) values ('{uid}','{cid}',CURRENT_TIMESTAMP,'{action}');");
        }
        /// <summary>
        /// Updates object Damage and Datestamp (used for maintaining objects)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="byUid"></param>
        public static HiveResult UpdateObjectDatestamp(string id, bool byUid)
        {
            return ExecuteNoReturn("update object_data set Datestamp = CURRENT_TIMESTAMP, Damage = \"0\" where `" + ((byUid == true) ? "ObjectUID" : "ObjectID") + "` = \"" + id + "\";");
        }
        /// <summary>
        /// Publishes an Object to the Database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="classname"></param>
        /// <param name="damage"></param>
        /// <param name="charid"></param>
        /// <param name="worldspace"></param>
        /// <param name="inventory"></param>
        /// <param name="hitpoints"></param>
        /// <param name="fuel"></param>
        public static HiveResult PublishObject(string id, string classname, string damage, string charid, string worldspace, string inventory, string hitpoints, string fuel)
        {
            return ExecuteNoReturn("insert into object_data (`ObjectUID`, `Instance`, `Classname`, `Damage`, `CharacterID`, `Worldspace`, `Inventory`, `Hitpoints`, `Fuel`, `Datestamp`) VALUES" +
                $"(\"{id}\",11,\"{classname}\",{damage},'{charid}','{worldspace}','{inventory}','{hitpoints}\',{fuel},CURRENT_TIMESTAMP);"
                );

        }
        /// <summary>
        /// Updates a Vehicls Hitpoints and Damage
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hitpoints"></param>
        /// <param name="damage"></param>
        public static HiveResult UpdateVehicleStatus(string id, string hitpoints, string damage, bool byUid)
        {
            return ExecuteNoReturn($"update object_data set Hitpoints = \"{hitpoints}\",Damage = \"{damage}\" where `" + ((byUid == true) ? "ObjectUID" : "ObjectID") + $"` = \"{id}\";");
        }
        /// <summary>
        /// Updates a Vehicles Position and Fuel
        /// </summary>
        /// <param name="id"></param>
        /// <param name="worldspace"></param>
        /// <param name="fuel"></param>
        public static HiveResult UpdateVehiclePosition(string id, string worldspace, string fuel)
        {
            return ExecuteNoReturn($"update object_data set Worldspace = \"{worldspace}\",Fuel = \"{fuel}\" where ObjectID = \"{id}\";");

        }
        /// <summary>
        /// Updates an Objects Inventory
        /// </summary>
        /// <param name="id"></param>
        /// <param name="inventory"></param>
        /// <param name="byUid"></param>
        public static HiveResult UpdateObjectInventory(string id, string inventory, bool byUid)
        {
            return ExecuteNoReturn("update object_data set Inventory = \"" + inventory + "\" where `" + ((byUid == true) ? "ObjectUID" : "ObjectID") + "` = \"" + id + "\";");

        }
        /// <summary>
        /// Deletes an Object from the Database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="byUid"></param>
        public static HiveResult DeleteObject(string id, bool byUid)
        {
            return ExecuteNoReturn("delete from object_data where `" + ((byUid == true) ? "ObjectUID" : "ObjectID") + "` = \"" + id + "\";");
        }

        /// <summary>
        /// Returns System DateTime
        /// </summary>
        /// <returns></returns>
        public static HiveResult ReturnDateTime()
        {
            HiveResult result = new HiveResult();
            result.Success = true;
            
            DateTime Now = DateTime.Now;
            string Date = "[";
            Date += Now.Year.ToString() + ",";
            Date += Now.Month.ToString() + ",";
            Date += Now.Day.ToString() + ",";
            Date += Entry.Config.Time.ToLower() == "static" ? Entry.Config.Hour.ToString() :  Now.Hour.ToString() + ",";
            Date += Now.Minute.ToString() + "]";
            result.Result = Date;
            return result;
        }


        /// <summary>
        /// Basic Read Method
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static MySqlDataReader ExecuteBasicRead(string cmd)
        {
            MySqlCommand command = new MySqlCommand(cmd, Connection);
            return command.ExecuteReader();
        }
        /// <summary>
        /// Execute Given SQL Statement without awaiting return. Returns false if SQL Statement fails, else returns true
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static HiveResult ExecuteNoReturn(string cmd)
        {
            var result = new HiveResult();
            try
            {
                MySqlCommand command = new MySqlCommand(cmd, Connection);
                command.ExecuteNonQuery();
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Success = false;
                result.Exception = e.Message;
            }
            return result;
        }
        public static HiveResult Connect(Config cfg)
        {
            var result = new HiveResult();
            try
            {
                string conn = $"Server={cfg.Host};Database={cfg.Schema};Uid={cfg.User};Pwd={cfg.Password};";
                Connection = new MySqlConnection(conn);
                Connection.Open();
                if (Connection.Ping())
                {
                    ConnectionString = conn;
                    result.Success = true;
                    return result;
                }
                result.Success = false;
                result.Exception = "Could not Connect to Database";
                return result;

            }
            catch (Exception e)
            {
                result.Success = false;
                result.Exception = e.Message;
                return result;
            }
        }
        public static void Disconnect()
        {
            Connection.Close();
        }
    }
}
