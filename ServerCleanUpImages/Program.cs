using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace ServerCleanUpImages
{
    class Program
    {
        static string connStr = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;//Your assembly also needs a reference to System.Configuration.dll
        static string pathToClean;
        static string targetPath;
        static void Main(string[] args)
        {
            /* posts
            pathToClean = "C:/inetpub/wwwroot/beautifier/images/posts";
            targetPath = pathToClean+"/cleanup";
            string tableToCheck = "beautifier.post";
            string paramToParse = "image";
            */
            /* postpics
            pathToClean = "C:/inetpub/wwwroot/beautifier/images/postpics";
            targetPath = pathToClean + "/cleanup";
            string tableToCheck = "beautifier.postpic";
            string paramToParse = "link";
            */
            if (args.Length != 3)
            {
                Console.WriteLine("arg 0: path of images");
                Console.WriteLine("arg 1: dbname.tablename");
                Console.WriteLine("arg 2: parameter name holding the image filename");
                Console.WriteLine("Please use the cmd with the required arguments.");
                Console.ReadKey(true);
                return;
            }
            pathToClean = args[0];
            targetPath = pathToClean + "/cleanup";
            string tableToCheck = args[1];
            string paramToParse = args[2];

            Console.Title = tableToCheck + " cleanup...";

            List<MyObj> list = new List<MyObj>();
            using (MySqlConnection con = new MySqlConnection(connStr))
            {
                string query = "SELECT " + paramToParse + " FROM " + tableToCheck;
                using (MySqlCommand cmd = new MySqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            list.Add(new MyObj(sdr, paramToParse));
                        }
                    }
                    con.Close();
                }
            }

            Logger.LogAction("DB_IMGS", "found " + list.Count + " images in the DB");
            foreach (MyObj o in list)
            {
                Console.WriteLine(o.param);
            }

            // To copy a folder's contents to a new location:
            // Create a new target folder, if necessary.
            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }

            IEnumerable<string> files = Directory.EnumerateFiles(pathToClean);//Directory.EnumerateFiles(pathToClean, "*.xml")
            Logger.LogAction("FILE_FOUND", "files found:");
            foreach (string fileFullPath in files)
            {

                //string contents = File.ReadAllText(file);
                Logger.LogAction("FILE_FOUND", fileFullPath);
            }


            Logger.LogAction("PARSE_FILE", "parsing starting");


            //Console.Write("Enter any key to start cleanup...");
            //Console.ReadKey(true);

            foreach (string fileFullPath in files)
            {
                string fileName = System.IO.Path.GetFileName(fileFullPath);
                Logger.LogAction("PARSE_FILE", "parsing " + fileName);


                foreach (MyObj o in list)
                {
                    if (o.param.Equals(fileName))
                    {
                        Logger.LogAction("PARSE_FILE", "parsing " + fileName + ": used in db");
                        goto REPEAT;
                    }
                }


                Logger.LogAction("PARSE_FILE", "parsing " + fileName + ": not used in db");
                backupFile(fileName);

            REPEAT:;
            }

            Logger.LogAction("PARSE_FILE", "parsing complete");

            //Console.Write("Please enter your name: ");
            //int input = Console.Read();

        }

        public static void backupFile(string fileName)
        {
            try
            {
                // Use Path class to manipulate file and directory paths.
                string sourceFile = System.IO.Path.Combine(pathToClean, fileName);
                string destFile = System.IO.Path.Combine(targetPath, fileName);

                // To move a file to another location and 
                System.IO.File.Move(sourceFile, destFile);
            }
            catch (Exception)
            {
                Logger.LogAction("MOVE_FILE", "file move failed, you might need to run command as an admin");

            }
        }
    }

    public class MyObj
    {
        public string param { get; set; }

        public MyObj()
        {
        }
        public MyObj(MySqlDataReader sdr, string paramToParse)
        {
            try
            {
                param = sdr[paramToParse].ToString();
            }
            catch (Exception e)
            {
                Logger.LogAction("OBJERR", e.Message + " - " + JsonConvert.SerializeObject(this));
                throw;
            }
        }

        public override string ToString()
        {
            string r = "";
            r += GetType();
            r += "[";
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(this);
                r += name + "=" + value + ", ";
            }
            r += "]";
            return r;
        }
    }
    static class Logger
    {
        private static string sPathName = ConfigurationManager.AppSettings["Log"];   //System.Web.HttpContext.Current.Server.MapPath("/Logs/");
        private static string getTimestamp()
        {
            return DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
        }
        internal static void LogAction(string loglevel, string action)
        {
            string logfiletype = "LOG";
            Console.WriteLine(action);
            Trace.WriteLine(logfiletype + ": " + action);
            string filename = sPathName + DateTime.Now.ToString("yyyyMMdd") + "_" + logfiletype + ".txt";
            StreamWriter sw = new StreamWriter(filename, true);
            sw.WriteLine(getTimestamp() + " : " + loglevel + " - " + action);
            sw.Flush();
            sw.Close();
        }
    }
}
