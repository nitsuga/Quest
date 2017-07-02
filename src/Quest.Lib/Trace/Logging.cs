using System;
using System.Diagnostics;

namespace Quest.Lib.Trace
{
    public static class Logger
    {
        static bool initialised = false;

        public static void Write(Exception ex)
        {
            Write(ex.ToString(), TraceEventType.Error, "");
        }

        public static void Write(string message)
        {
            Write(message, TraceEventType.Information, "");
        }

        public static void Write(string message, int jobId, TraceEventType type = TraceEventType.Information, string source = null)
        {
            //TODO: write to job log
            Write(message, type, source);
        }

        public static void Write(string message, string source = null, TraceEventType type = TraceEventType.Information)
        {
            Write(message, type, source);
        }

        public static void Write(string message, TraceEventType type = TraceEventType.Information, string source = null)
        {
            if (source == null)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(1);
                source = sf.GetMethod().DeclaringType.Name + ":" + sf.GetMethod().Name;
            }

            switch (type)
            {
                case TraceEventType.Information:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case TraceEventType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case TraceEventType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case TraceEventType.Critical:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case TraceEventType.Start:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case TraceEventType.Stop:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }

            string msg = $"{DateTime.Now} [{source,20}] {message}";
            Console.WriteLine(msg);
            Debug.WriteLine(msg);

            // start logging straight away
            if (!initialised)
            {
                initialised = true;
            }
        }

    }

}
