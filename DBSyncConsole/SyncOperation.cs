using System;
using CTS_Models;
using CTS_Models.DBContext;
using CTS_Models.Others;
using CTS_Models.Wagon;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RailWeighbridge;

namespace DBSyncConsole
{
    public class CtsDbSynchronizer
    {
        static Logger log = new Logger();
        public string Name_obj = "???";

        public DateTime start_time = DateTime.Now;
        public TimeSpan lead_time = new TimeSpan(0);
        public int result = 0;
        public bool[] steps = new bool[19];

        private static bool SyncFromLocalToCental<TTransfer, TEquip>(ConnectionStringSettings connectionStringSettings)
                where TTransfer : class, ITransfer
                where TEquip : class, IEquip
        {
            List<TTransfer> localTransferList = new List<TTransfer>();
            int[] scalesArray;
            var scalesTime = new Dictionary<int, DateTime>();

            try
            {
                using (CtsEquipContext<TEquip> centralDB = new CtsEquipContext<TEquip>("centralDBConnection"))
                {
                    scalesArray = centralDB.DbSet.Where(x => x.LocationID == connectionStringSettings.Name.ToString()).Select(m => m.ID).ToArray();
                }

                using (CtsTransferContext<TTransfer> centralDB = new CtsTransferContext<TTransfer>("centralDBConnection"))
                {
                    foreach (var scale in scalesArray)
                    {
                        DateTime lastTime = new DateTime(2018, 01, 01);
                        var transfer = centralDB.DbSet.Where(x => x.EquipID == scale).OrderByDescending(t => t.TransferTimeStamp).FirstOrDefault();
                        if (transfer != null)
                        {
                            lastTime = transfer.TransferTimeStamp;
                        }
                        scalesTime.Add(scale, lastTime);
                    }

                }

                List<DateTime> time = new List<DateTime>();

                using (CtsTransferContext<TTransfer> localDB = new CtsTransferContext<TTransfer>(connectionStringSettings.ConnectionString))
                {
                    foreach (var scale in scalesTime)
                    {
                        localTransferList.AddRange(localDB.DbSet.Where(s => s.EquipID == scale.Key)
                                            .Where(x => x.OperatorName == "System Platform").Where(t => t.TransferTimeStamp > scale.Value));

                        time = localDB.DbSet.Select(x => x.TransferTimeStamp).ToList();
                    }
                }

                if (localTransferList.Count != 0)
                {
                    using (CtsTransferContext<TTransfer> centralDB = new CtsTransferContext<TTransfer>("centralDBConnection"))
                    {
                        foreach (var t in localTransferList)
                        {
                            TTransfer transfer = t;
                            transfer.LasEditDateTime = System.DateTime.Now;
                            centralDB.DbSet.Add(transfer);
                        }
                        centralDB.SaveChanges();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                lock (log)
                {
                    log.Message(ex);
                }
                return false;
            }
        }

        private static bool SyncFromCentralToLocal<TTransfer, TEquip>(ConnectionStringSettings connectionStringSettings)
          where TTransfer : class, ITransfer
          where TEquip : class, IEquip
        {
            List<TTransfer> centralTransferList = new List<TTransfer>();
            string stringForLogger = "";
            int[] scalesArray;

            try
            {
                List<DateTime> time = new List<DateTime>();

                using (CtsEquipContext<TEquip> centralDB = new CtsEquipContext<TEquip>("centralDBConnection"))
                {
                    scalesArray = centralDB.DbSet.Where(x => x.LocationID == connectionStringSettings.Name.ToString()).Select(m => m.ID).ToArray();
                }

                using (CtsTransferContext<TTransfer> centralDB = new CtsTransferContext<TTransfer>("centralDBConnection"))
                {
                    centralTransferList.AddRange(centralDB.DbSet.Where(x => scalesArray.Contains((int)x.EquipID))
                                                                .Where(n => n.OperatorName != "System Platform")
                                                                .Where(x => x.TransferTimeStamp > DbFunctions.AddDays(System.DateTime.Now, -2)));
                }

                if (centralTransferList.Count != 0)
                {
                    using (CtsTransferContext<TTransfer> localDB = new CtsTransferContext<TTransfer>(connectionStringSettings.ConnectionString))
                    {
                        using (var transaction = localDB.Database.BeginTransaction())
                        {
                            foreach (var t in centralTransferList)
                            {
                                if (t is IHaveAnalysis transfer)
                                {
                                    transfer.AnalysisID = null;
                                }
                                localDB.DbSet.AddOrUpdate(t as TTransfer);
                                stringForLogger = String.Concat(stringForLogger, t.ID, ";");
                            }

                            localDB.SaveChanges();
                            transaction.Commit();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (log)
                {
                    log.Message(ex);
                }
                return false;
            }
        }

        private static bool SyncFromCentralToLocalAndDeleteWarehouseTransfers(ConnectionStringSettings connectionStringSettings)
        {
            List<WarehouseTransfer> centralTransferList = new List<WarehouseTransfer>();
            string stringForLogger = "";

            try
            {
                using (CtsDbContext centralDB = new CtsDbContext())
                {
                    centralTransferList.AddRange(centralDB.WarehouseTransfers.Where(x => x.Warehouse.LocationID == connectionStringSettings.Name.ToString())
                                                                .Where(x => x.TransferTimeStamp > DbFunctions.AddDays(System.DateTime.Today, -2)));
                }

                if (centralTransferList.Count != 0)
                {
                    using (CtsDbContext localDB = new CtsDbContext(connectionStringSettings.ConnectionString))
                    {
                        using (var transaction = localDB.Database.BeginTransaction())
                        {
                            foreach (var t in centralTransferList)
                            {
                                localDB.WarehouseTransfers.AddOrUpdate(t);
                                stringForLogger = String.Concat(stringForLogger, t.ID, ";");
                            }

                            List<WarehouseTransfer> localTransferList = new List<WarehouseTransfer>();
                            localTransferList.AddRange(localDB.WarehouseTransfers.Where(x => x.TransferTimeStamp <= DbFunctions.AddDays(System.DateTime.Today, -3)));
                            localDB.WarehouseTransfers.RemoveRange(localTransferList);

                            localDB.SaveChanges();
                            transaction.Commit();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (log)
                {
                    log.Message(ex);
                }
                return false;
            }
        }

        private static bool SyncFromCentralToLocalAndDeleteWagonNumsCache(ConnectionStringSettings connectionStringSettings)
        {
            var nums = new List<WagonNumsCache>();

            try
            {
                using (CtsDbContext centralDB = new CtsDbContext())
                {
                    nums.AddRange(centralDB.WagonNumsCache.Where(x => x.Recogn.LocationID == connectionStringSettings.Name.ToString()).ToList());
                }

                if (nums.Any())
                {
                    using (CtsDbContext localDB = new CtsDbContext(connectionStringSettings.ConnectionString))
                    {
                        using (var transaction = localDB.Database.BeginTransaction())
                        {
                            localDB.WagonNumsCache.RemoveRange(localDB.WagonNumsCache);
                            localDB.WagonNumsCache.AddRange(nums);
                            localDB.SaveChanges();
                            transaction.Commit();
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                lock (log)
                {
                    log.Message(ex);
                }
                return false;
            }
        }

        private static bool SyncDictionaries<T>(ConnectionStringSettings connectionStringSettings) where T : class
        {
            List<T> items = new List<T>();

            try
            {
                using (CtsUniversalContext<T> centralDB = new CtsUniversalContext<T>("centralDBConnection"))
                {
                    items.AddRange(centralDB.DbSet.ToList());
                }

                if (items.Count != 0)
                {
                    using (CtsUniversalContext<T> localDB = new CtsUniversalContext<T>(connectionStringSettings.ConnectionString))
                    {
                        //using (var transaction = localDB.Database.BeginTransaction())
                        //{
                        foreach (var item in items)
                        {
                            localDB.DbSet.AddOrUpdate(item);
                        }

                        localDB.SaveChanges();
                        //transaction.Commit();
                        //}
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (log)
                {
                    log.Message(ex);
                }
                return false;
            }

            //catch (DbEntityValidationException ex)
            //{
            //    foreach (var eve in ex.EntityValidationErrors)
            //    {
            //        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
            //            eve.Entry.Entity.GetType().Name, eve.Entry.State);
            //        foreach (var ve in eve.ValidationErrors)
            //        {
            //            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
            //                ve.PropertyName, ve.ErrorMessage);
            //        }
            //    }
            //    throw;

            //    Console.WriteLine(ex.Message);
            //    return false;
            //}
        }

        private static bool RemoveOldLocal<T>(ConnectionStringSettings connectionStringSettings) where T : class, ITransfer
        {
            List<T> localTransferList = new List<T>();
            string stringForLogger = "";

            try
            {
                using (CtsTransferContext<T> localDB = new CtsTransferContext<T>(connectionStringSettings.ConnectionString))
                {
                    localTransferList.AddRange(localDB.DbSet.Where(x => x.TransferTimeStamp <= DbFunctions.AddDays(System.DateTime.Now, -3)));
                    localDB.DbSet.RemoveRange(localTransferList);
                    localDB.SaveChanges();
                    foreach (var t in localTransferList)
                    {
                        stringForLogger = String.Concat(stringForLogger, t.ID, ";");
                    }
                }
              
                return true;
            }

            catch (Exception ex)
            {
                lock (log)
                {
                    log.Message(ex);
                }
                return false;
            }
        }

        private static void SyncOperation()
        {
            var SyncTasks = new List<Task>();

            while (true)

            {
                foreach (ConnectionStringSettings connection in System.Configuration.ConfigurationManager.ConnectionStrings)
                {
                    if ((connection.Name != "CentralDBConnection") && (connection.Name != "LocalDBConnection")
                        && (connection.Name != "LocalSqlServer") && (connection.Name != "WagonDB"))
                    {
                        SyncTasks.Add(Task.Factory.StartNew(() =>
                        {
                            SyncDictionaries<Location>(connection);
                            SyncDictionaries<Item>(connection);
                            SyncDictionaries<InnerDestination>(connection);
                            SyncDictionaries<WagonScale>(connection);
                            SyncDictionaries<VehiScale>(connection);
                            SyncDictionaries<BeltScale>(connection);
                            SyncDictionaries<Skip>(connection);
                            SyncDictionaries<RockUtil>(connection);
                            SyncDictionaries<Warehouse>(connection);
                            SyncDictionaries<Recogn>(connection);
                            SyncDictionaries<BoilerConsNormNews>(connection);
                            SyncDictionaries<Shift>(connection);
                           
                            if (SyncFromLocalToCental<WagonTransfer, WagonScale>(connection))
                            {
                                //RemoveOldLocal<WagonTransfer>(connection);
                            }

                            SyncFromCentralToLocal<WagonTransfer, WagonScale>(connection);

                            if (SyncFromLocalToCental<VehiTransfer, VehiScale>(connection))
                            {
                                //RemoveOldLocal<VehiTransfer>(connection);
                            }

                            SyncFromCentralToLocal<VehiTransfer, VehiScale>(connection);

                            if (SyncFromLocalToCental<BeltTransfer, BeltScale>(connection))
                            {
                                //RemoveOldLocal<BeltTransfer>(connection);
                            }
                            SyncFromCentralToLocal<BeltTransfer, BeltScale>(connection);

                            if (SyncFromLocalToCental<SkipTransfer, Skip>(connection))
                            {
                                //RemoveOldLocal<SkipTransfer>(connection);
                            }
                            SyncFromCentralToLocal<SkipTransfer, Skip>(connection);

                            SyncFromCentralToLocal<RockUtilTransfer, RockUtil>(connection);

                            SyncFromCentralToLocalAndDeleteWarehouseTransfers(connection);

                            SyncFromCentralToLocalAndDeleteWagonNumsCache(connection);
                        }));
                    }
                }
                Task.WaitAll(SyncTasks.ToArray());
            }
        }

        public void Handler(ConnectionStringSettings connection)
        {
            Name_obj = connection.Name;

            while (true)
            {
                try
                {
                    start_time = DateTime.Now;

                    result = 0;
                    for (int i = 0; i < steps.Length; i++)
                    {
                        steps[i] = false;
                    }

                    steps[0] = SyncDictionaries<Location>(connection);
                    steps[1] = SyncDictionaries<Item>(connection);
                    steps[2] = SyncDictionaries<InnerDestination>(connection);
                    steps[3] = SyncDictionaries<WagonScale>(connection);
                    steps[4] = SyncDictionaries<VehiScale>(connection);
                    steps[5] = SyncDictionaries<BeltScale>(connection);
                    steps[6] = SyncDictionaries<Skip>(connection);
                    steps[7] = SyncDictionaries<RockUtil>(connection);
                    steps[8] = SyncDictionaries<Warehouse>(connection);
                    steps[9] = SyncDictionaries<Recogn>(connection);
                    steps[10] = SyncDictionaries<BoilerConsNormNews>(connection);
                    steps[11] = SyncDictionaries<Shift>(connection);
                   
                    steps[12] = SyncFromCentralToLocal<WagonTransfer, WagonScale>(connection);
                    if (steps[12]) RemoveOldLocal<WagonTransfer>(connection);

                    steps[13] = SyncFromLocalToCental<VehiTransfer, VehiScale>(connection);
                    if (steps[13]) RemoveOldLocal<VehiTransfer>(connection);

                    steps[14] = SyncFromLocalToCental<BeltTransfer, BeltScale>(connection);
                    if (steps[14]) RemoveOldLocal<BeltTransfer>(connection);

                    steps[15] = SyncFromLocalToCental<SkipTransfer, Skip>(connection);
                    if (steps[15]) RemoveOldLocal<SkipTransfer>(connection);

                    steps[16] = SyncFromCentralToLocal<RockUtilTransfer, RockUtil>(connection);

                    steps[17] = SyncFromCentralToLocalAndDeleteWarehouseTransfers(connection);
                    steps[18] = SyncFromCentralToLocalAndDeleteWagonNumsCache(connection);

                    lead_time = DateTime.Now.Subtract(start_time);

                    foreach (bool step in steps)
                    {
                        if (!step) result = 2;
                    }

                    if (result != 2) result = 1;

                }
                catch(Exception ex)
                {
                    lock (log)
                    {
                        log.Message(ex);
                    }
                }
                //catch (DbEntityValidationException e)
                //{
                //    result = 2;

                //    foreach (var eve in e.EntityValidationErrors)
                //    {
                //        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                //            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                //        foreach (var ve in eve.ValidationErrors)
                //        {
                //            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                //                ve.PropertyName, ve.ErrorMessage);
                //        }
                //    }
                //    throw;
                //}
                System.Threading.Thread.Sleep(60000);
            }
        }
    }
}
