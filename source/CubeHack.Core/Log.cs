// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CubeHack
{
    static class Log
    {
        private static BlockingCollection<Tuple<DateTime, string>> _queue = new BlockingCollection<Tuple<DateTime, string>>();

        static Log()
        {
            var thread = new Thread(RunLogger);
            thread.Name = "Logger";
            thread.IsBackground = true;
            thread.Start();
        }

        public static void Info(string message)
        {
            LogInternal(message);
        }

        public static void Info(string format, params object[] args)
        {
            LogInternal(format, args);
        }

        private static void LogInternal(string message)
        {
            _queue.Add(Tuple.Create(DateTime.Now, message));
        }

        private static void LogInternal(string format, params object[] args)
        {
            LogInternal(string.Format(format, args));
        }

        private static void RunLogger()
        {
            try
            {
                var logFilePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "CubeHack",
                    "Logs");

                Directory.CreateDirectory(logFilePath);

                var logFileName = Path.Combine(
                    logFilePath,
                    string.Format(
                        "CubeHack_{0}.log",
                        DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));

                using (var writer = new StreamWriter(logFileName, false, Encoding.UTF8))
                {
                    while (true)
                    {
                        var entry = _queue.Take();
                        Console.WriteLine(entry.Item2);
                        writer.Write(entry.Item1.ToString("HH:mm:ss.fff "));
                        writer.WriteLine(entry.Item2);
                        writer.Flush();
                    }
                }
            }
            catch
            {
            }
        }
    }
}
