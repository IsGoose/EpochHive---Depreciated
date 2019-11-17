# EpochHive
Epoch Hive for Epoch 1.0.7

*Supporting by Default:*
- [Epoch](https://epochmod.com/a2dayzepoch.php)
- [Multicharacter](https://epochmod.com/forum/topic/4944-release-multiple-character-support-now-compatible-with-epoch-1051/)
- [VirtualGarage](https://github.com/oiad/virtualGarage)

Epoch Hive was built upon the .NET Framework (ver. 4.7.2)

## Build Instructions
- Open the downloaded Solution File (EpochHive.sln)
- Navigate to  "Project" >> "Manage NuGet Packages"  and click "Restore"
- Restart Visual Studio and reopen the EpochHive.sln
- Make Sure the Build Platform Target is set to "x86"
- Build (CTRL + B or "Build" >> "Build EpochHive) 

## Usages
*Epoch Hive Should only be used with DayZ Epoch*  
After building EpochHive (see above), you will need to navigate to "...\bin\Debug\" and copy all DLL files to your root Server Install Directory.  
Next, create a new file in your Server Config Directory called "hiveCfg.json"  
Add the following JSON text to the file:
```json
{
	"Host": "localhost",
	"User": "root",
	"Password": "",
	"Schema": "dayz_epoch",
	"Instance": 11,
	"Time": "local",
	"Hour": 11,
	"LogLevel": "debug"
}
```  

This is EpochHive's Config File - the file that EpochHive will read from when it is called  
- Host: Host Address of the Database you are connecting to (localhost for local machine)
- User: Database User for EpochHive to connect with
- Password: Password of Database User
- Schema: Database Schema to read/write data (dayz_epoch is the default schema for epoch)
- Instance: InstanceID for you Server (11 is for Chernarus)
- Time: Time Setting. "local" for Local Server System Time, "static" for Static Time as defined below
- Hour: Hour to return if Time is set to "static" - not used if Time is set to "local"
- LogLevel: If "debug", then EpochHive will log every action. if left emtpy ("")  EpochHive will only log Critical Errors

EpochHive will log to a file called "EpochHiveLOG.txt" located in your root Server Install Directory   
  
### Dependencies
- [.NET Framework version 4.7.2](https://dotnet.microsoft.com/download/thank-you/net472)

### NuGet Packages
- [Newtonsoft.Json - James Newton-King](https://www.newtonsoft.com/json)
- [UnmanagedExports - Robert Giesecke](https://sites.google.com/site/robertgiesecke/Home/uploads/unmanagedexports)
- [MySql.Data - Oracle](https://dev.mysql.com/downloads/)
- [Google.Protobuf](https://github.com/protocolbuffers/protobuf)
- [BouncyCastle](http://www.bouncycastle.org/csharp/)
- [SSH.NET](https://github.com/sshnet/SSH.NET/)
