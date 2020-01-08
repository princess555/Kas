using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Diagnostics;
using System.Collections;
using CTS_Models.DBContext;
using System.Configuration;

namespace DBSyncConsole
{
    class Program
    {
        static SyncWagon WagonSynchronizer = new SyncWagon();
        static List<CtsDbSynchronizer> CtsDbSynchronizers = new List<CtsDbSynchronizer>();
        static List<Thread> threadsCtsDbSynchronizer = new List<Thread>();

        static void Main(string[] args)
        {
            Thread threadDisplay = new Thread(() => Display()) { IsBackground = true };
            threadDisplay.Start();

            Thread threadSyncWagon = new Thread(() => WagonSynchronizer.Handler()) { IsBackground = true };
            threadSyncWagon.Start();

            List<ConnectionStringSettings> remote_objects = ConfigurationManager.ConnectionStrings
                                                 .Cast<ConnectionStringSettings>()
                                                 .Where(a => !a.Name.ToLower().Equals("CentralDbConnection".ToLower()))
                                                 .Where(a => !a.Name.ToLower().Equals("LocalDBConnection".ToLower()))
                                                 .Where(a => !a.Name.ToLower().Equals("LocalSqlServer".ToLower()))
                                                 .Where(a => !a.Name.ToLower().Equals("WagonDB".ToLower()))
                                                 .ToList();


            foreach (ConnectionStringSettings remote_object in remote_objects)
            {
                CtsDbSynchronizer obj = new CtsDbSynchronizer();
                CtsDbSynchronizers.Add(obj);

                Thread threadObj = new Thread(() => obj.Handler(remote_object)) { IsBackground = true, Name = remote_object.Name };
                threadsCtsDbSynchronizer.Add(threadObj);
                threadObj.Start();
            }

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        static void Display()
        {
            while (true)
            {
                try
                {
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine(DateTime.Now);
                    Console.WriteLine();
                    Console.ForegroundColor = WagonSynchronizer.error ? ConsoleColor.Red : ConsoleColor.Green;
                    Console.WriteLine(String.Format("{0,-8} last start [{1}] period execution {2:d6}s  ves_vagon [{3:d10}] vagon_nums [{4:d10}]",
                                                    "Wagon",
                                                    WagonSynchronizer.start_time,
                                                    (int)WagonSynchronizer.lead_time.TotalSeconds,
                                                    WagonSynchronizer.ves_vagon_records,
                                                    WagonSynchronizer.recognition_records));
                    Console.WriteLine();
                    foreach (CtsDbSynchronizer obj in CtsDbSynchronizers)
                    {

                        switch (obj.result)
                        {
                            case 0:
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            case 1:
                                Console.ForegroundColor = ConsoleColor.Green;
                                break;
                            case 2:
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;

                        }

                        string progress = "";

                        foreach (bool step in obj.steps)
                        {
                            if (step)
                            {
                                progress = progress + "-";
                            }
                            else
                            {
                                progress = progress + "_";
                            }
                        }

                        Console.WriteLine(String.Format("{0,-8} last start [{1}] period execution {2:d6}s [{3}]",
                                                        obj.Name_obj,
                                                        obj.start_time,
                                                        (int)obj.lead_time.TotalSeconds,
                                                        progress));
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }
                catch (Exception) { }
                Thread.Sleep(1000);
            }
        }
    }
}
