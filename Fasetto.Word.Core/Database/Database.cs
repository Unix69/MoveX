using System;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Movex.Network;
using System.Collections.Generic;

namespace Movex.View.Core
{
    public static class Database
    {

        /// <summary>
        /// Get the values of the local database
        /// <param name="user">User (of Movex.Network Project), used to get network informations like: Environment Machine Name, IPaddress Machine</param>
        /// <returns>Dictionary (with values), null (in case of error)</returns>
        /// </summary>
        public static Dictionary<string, string> GetValues()
        {
            // Initialize the return value
            var dictionary = new Dictionary<string, string>();

            // Set default values
            var DbFile = @"Database.txt";
            var currWorkingDirectory = Directory.GetCurrentDirectory();
            var DbPath = Path.Combine(currWorkingDirectory, DbFile);
            
            // Check if the file exists and act consequently
            if (!File.Exists(DbPath))
            {

                if (!Directory.Exists(currWorkingDirectory + @"\Downloads\"))
                {
                    Directory.CreateDirectory(currWorkingDirectory + @"\Downloads\");
                }

                if (!Directory.Exists(currWorkingDirectory + @"\ProfilePictures\"))
                {
                    Directory.CreateDirectory(currWorkingDirectory + @"\ProfilePictures\");
                }

                // Create the file (the database) and put default values
                try
                {
                    using (var fs = File.Create(DbPath))
                    {
                        var defaultName = System.Environment.MachineName;
                        var info = new UTF8Encoding(true).GetBytes("Name=" + defaultName + "\r\n");
                        fs.Write(info, 0, info.Length);
                        dictionary["Name"] = defaultName;
                        
                        var defaultMessage = "Hey, there! I'm using Movex!";
                        info = new UTF8Encoding(true).GetBytes("Message=" + defaultMessage + "\r\n");
                        fs.Write(info, 0, info.Length);
                        dictionary["Message"] = defaultMessage;

                        var ip = new NetworkInformation().GetLocalNetworkInformation()[0];
                        info = new UTF8Encoding(true).GetBytes("IpAddress=" + ip.ToString() + "\r\n");
                        fs.Write(info, 0, info.Length);
                        dictionary["IpAddress"] = ip.ToString();
                        
                        var absolutePathProfilePicture = currWorkingDirectory + @"\Images\Icons\profile.png";
                        info = new UTF8Encoding(true).GetBytes("ProfilePicture=" + absolutePathProfilePicture + "\r\n");
                        fs.Write(info, 0, info.Length);
                        dictionary["ProfilePicture"] = absolutePathProfilePicture;

                        var DownloadDefaultFolder = currWorkingDirectory + @"\Downloads\";
                        info = new UTF8Encoding(true).GetBytes("DownloadDefaultFolder=" + DownloadDefaultFolder + "\r\n");
                        fs.Write(info, 0, info.Length);
                        dictionary["DownloadDefaultFolder"] = DownloadDefaultFolder;

                        var PrivateMode = "False";
                        info = new UTF8Encoding(true).GetBytes("PrivateMode=" + PrivateMode + "\r\n");
                        fs.Write(info, 0, info.Length);
                        dictionary["PrivateMode"] = PrivateMode;

                        var AutomaticReception = "False";
                        info = new UTF8Encoding(true).GetBytes("AutomaticReception=" + AutomaticReception + "\r\n");
                        fs.Write(info, 0, info.Length);
                        dictionary["AutomaticReception"] = AutomaticReception;

                        var AutomaticSave = "False";
                        info = new UTF8Encoding(true).GetBytes("AutomaticSave=" + AutomaticSave + "\r\n");
                        fs.Write(info, 0, info.Length);
                        dictionary["AutomaticSave"] = AutomaticSave;
                    }

                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    return null;
                }
            }
            else
            {
                // The file exists (the database exists) so: load the values
                try
                {
                    using (var sr = File.OpenText(DbPath))
                    {
                        var s = "";
                        while ((s = sr.ReadLine()) != null)
                        {

                            var fields = s.Split('=');
                            if (fields[0].Equals("ProfilePicture"))
                            {
                                dictionary["ProfilePicture"] = fields[1];
                            }
                            else if (fields[0].Equals("Name"))
                            {
                                dictionary["Name"] = fields[1];
                            }
                            else if (fields[0].Equals("Message"))
                            {
                                dictionary["Message"] = fields[1];
                            }
                            else if (fields[0].Equals("DownloadDefaultFolder"))
                            {
                                dictionary["DownloadDefaultFolder"] = fields[1];
                            }
                            else if (fields[0].Equals("PrivateMode"))
                            {
                                dictionary["PrivateMode"] = fields[1];
                            }
                            else if (fields[0].Equals("AutomaticSave"))
                            {
                                dictionary["AutomaticSave"] = fields[1];
                            }
                            else if (fields[0].Equals("AutomaticReception"))
                            {
                                dictionary["AutomaticReception"] = fields[1];
                            }



                        }

                        // Set the IP Address.
                        // This is the unique parameter which is always
                        // variable and cannot be read from the localDB.
                        // Anyway it is however stored for logging purposes.
                        dictionary["IpAddress"] = new NetworkInformation().GetLocalNetworkInformation().ToString();

                    }
                }
                catch (Exception excep)
                {
                    Console.WriteLine(excep.Message);
                    return null;
                }

            }

            // Return the dictionary filled either in the if clause or in the else clause
            return dictionary;
        }

        public static void UpdateLocalDB(string nameOfParameter, string value)
        {
            var DbFile = @"Database.txt";
            var currWorkingDirectory = Directory.GetCurrentDirectory();
            var DbPath = Path.Combine(currWorkingDirectory, DbFile);
            var tmpFile = Path.Combine(currWorkingDirectory, "temp.txt");

            try
            {
                var sw = File.Create(tmpFile);
                using (var sr = File.OpenText(DbPath))
                {
                    var s = "";
                    var found = false;
                    while ((s = sr.ReadLine()) != null)
                    {
                        var fields = s.Split('=');

                        if (fields[0].Equals(nameOfParameter))
                        {
                            // Substitute fields1 with value and stop
                            var info = new UTF8Encoding(true).GetBytes(fields[0] + "=" + value + "\r\n");
                            sw.Write(info, 0, info.Length);
                            found = true;
                        }
                        else
                        {
                            // Rewrite the line as it is
                            var info = new UTF8Encoding(true).GetBytes(s + "\r\n");
                            sw.Write(info, 0, info.Length);
                        }

                    }

                    if (!found)
                    {
                        var info = new UTF8Encoding(true).GetBytes(nameOfParameter + "=" + value);
                        sw.Write(info, 0, info.Length);
                    }
                    sr.Close();
                }
                sw.Close();
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.Message);
                return;
            }

            try
            {
                // Delete the older Database file
                File.Delete(DbPath);

                // Copy the file to a new one called "Database.txt"
                File.Copy(tmpFile, "Database.txt");

                // Delete the tmp file
                File.Delete(tmpFile);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                return;
            }


        }

        public static void Drop()
        {
            var DbFile = @"Database.txt";
            var currWorkingDirectory = Directory.GetCurrentDirectory();
            var DbPath = Path.Combine(currWorkingDirectory, DbFile);

            // Delete the actual "database.txt"
            if (File.Exists(DbPath)) { File.Delete(DbPath); }
        }

        

    }
}
