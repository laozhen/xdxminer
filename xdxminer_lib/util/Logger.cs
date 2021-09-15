using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace xdxminer_lib.util
{

    public enum Level
    {
        Info, Error, Debug, Warn
    }

    public class Logger
    {
        public static readonly ConcurrentQueue<string> logs = new ConcurrentQueue<string>();

        public static void addToLog (string str)
        {
            if (logs.Count<100000)
            {
                logs.Enqueue(str);
            }
        }

        private string name;

        public Logger (string name)
        {
            this.name = name;
        }
        public Logger ()
        {

        }

        public void log(Level level, string text, params object[] args)
        {
            log(level, String.Format(text, args));
        }


        public void log(Level level, string text)
        {
            string outputText = DateTime.Now + " " +  this.name + " [" + level + "] " + text;
            if (level != Level.Debug && !"dev".Equals(name))
            {
                addToLog(outputText);
            }
           
            Debug.WriteLine(outputText);


        }

        public void banner (Level level, string format, params object[] args)
        {
            log(level, "*****************************************************************************");
            log(level,format,args);
            log(level, "*****************************************************************************");
        }

        public void info(string format, params object[] args)
        {
            log(Level.Info, format, args);
        }

        public void debug(string format)
        {
            log(Level.Debug, format);
        }

        public void debug(string format, params object[] args)
        {
            log(Level.Debug, format, args);
        }




        public void error(string format, params object[] args)
        {
            log(Level.Error, format, args);
        }

        public void error(string format)
        {
            log(Level.Error, format);
        }

        public void warn(string format, params object[] args)
        {
            log(Level.Warn, format, args);
        }

        public void warn(string format)
        {
            log(Level.Warn, format);
        }

    }
}
