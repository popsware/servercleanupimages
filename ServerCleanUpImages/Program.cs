using beautifier_web.Helpers;
using beautifier_web.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if(args.Length != 3)
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


            List<MyObj> list = new List<MyObj>();
            using (MySqlConnection con = new MySqlConnection(connStr))
            {
                string query = "SELECT "+paramToParse+" FROM "+tableToCheck;
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

            Console.WriteLine("Files in the db:");
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
            Console.WriteLine("Files available:");
            foreach (string fileFullPath in files)
            {
                
                //string contents = File.ReadAllText(file);
                Console.WriteLine(fileFullPath);
            }

            //Console.Write("Enter any key to start cleanup...");
            //Console.ReadKey(true);

            foreach (string fileFullPath in files)
            {
                string fileName = System.IO.Path.GetFileName(fileFullPath);
                Console.WriteLine("parsing " + fileName);


                foreach (MyObj o in list)
                {
                    Console.WriteLine("parsing " + fileName + "=="+o.param);
                    if (o.param.Equals(fileName))
                    {
                        Console.WriteLine("parsing " + fileName+": used in db");
                        goto REPEAT;
                    }
                }


                Console.WriteLine("parsing " + fileName + ": not used in db");
                backupFile(fileName);

                REPEAT:;
            }

            //Console.Write("Please enter your name: ");
            //int input = Console.Read();

        }

        public static void backupFile(string fileName)
        {
            // Use Path class to manipulate file and directory paths.
            string sourceFile = System.IO.Path.Combine(pathToClean, fileName);
            string destFile = System.IO.Path.Combine(targetPath, fileName);


            // To move a file to another location and 
            System.IO.File.Move(sourceFile, destFile);
        }
    }

    public class MyObj: MyModel
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
                Logger.LogObjectError(e.Message, ToString());
                throw e;
            }
        }


    }
}
