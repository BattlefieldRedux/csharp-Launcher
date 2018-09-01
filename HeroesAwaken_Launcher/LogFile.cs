using System;
using System.IO;

namespace OSHeroes
{
    class LogFile
    {
        public static StreamWriter sw = new StreamWriter("logs.txt", true);
        public static void CreatFile()
        {
            if (!File.Exists("logs.txt"))
            {
                File.Create("logs.txt");
            }
        }

        public static void AppendLogFile(string text)
        {
                sw.WriteLine("\n"+DateTime.Now + " | " + text);
        }
    }
}
