using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTS_Models.WagonDB;
using System.Data.Entity;
using CTS_Models.DBContext;
using System.Configuration;
using CTS_Models;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Collections;
using System.IO.Pipes;
using System.IO;
using System.Diagnostics;
using System.Data.SqlClient;

namespace DBSyncConsole
{
    public struct Data
    {
        private string name;
        private string status;
        private DateTime time;
        private int quantity;
        private string comment;

        public int Quantity { get { return quantity; } set { quantity = value; } }
        public string Comment { get { return comment; } set { comment = value; } }
        public string Name { get { return name; } set { name = value; } }
        public string Status { get { return status; } set { status = value; } }
        public DateTime Time { get { return time; } set { time = value; } }

        public Data(string name, string status, DateTime time, int quantity, string comment)
        {
            this.name = name;
            this.status = status;
            this.time = time;
            this.quantity = quantity;
            this.comment = comment;
        }
    }

    class SyncWagon
    {

        public int ves_vagon_records = 0;
        public int recognition_records = 0;

        public DateTime start_time = DateTime.Now;
        public TimeSpan lead_time = new TimeSpan(0);
        public bool error = false;

        private void WriteInFile()
        {
            ////Отправляем данные 
            ////PipeClient.Send(listSync.ToString(), "", 1000);


            ////запись в файл
            //string path = @"C:\Users\TigranVChi\Desktop\Korol\New folder\24.07.19\CTS_Github\DBSyncConsole\SyncData.txt";

            //try
            //{
            //    using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate)))
            //    {
            //        foreach (var item in listSync)
            //        {
            //            writer.Write(item.Name);
            //            writer.Write(item.Status);
            //            writer.Write(item.Time.ToString());
            //            writer.Write(item.Quantity);
            //            writer.Write(item.Comment);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
        }

        private void GetAsyncWagon(DateTime from_ts, DateTime to_ts)
        {
            try
            {
                List<WagonScale> wagonScales;
                List<Item> items;

                using (CtsDbContext centralDB = new CtsDbContext())
                {
                    wagonScales = centralDB.WagonScales.Include(m => m.Location).ToList();
                    items = centralDB.Items.Include(m => m.Location).ToList();
                }

                var transfer = new List<ves_vagon>();
                var accept = new List<WagonTransfer>();

                using (var vesWagon = new WagonDBcontext())
                {
                    transfer = vesWagon.ves_vagon/*.Where(x => x.id_operator != null)*/
                                                .Where(x => x.id_operator != 0)
                                                .Where(x => x.sync != 1)
                                                .Where(x => x.date_time_brutto >= from_ts)
                                                .Where(x => x.date_time_brutto <= to_ts)
                                                .Include(m => m.scales).DefaultIfEmpty()
                                                .Include(h => h.napravlenie).DefaultIfEmpty()
                                                .Include(n => n.otpravl).DefaultIfEmpty()
                                                .Include(k => k.poluch).DefaultIfEmpty()
                                                .ToList();
                }

                if (transfer.Count > 0)
                {
                    using (var centralDB = new CtsDbContext())
                    {
                        for (ves_vagon_records = transfer.Count;
                             ves_vagon_records > 0;
                             ves_vagon_records--)
                        {
                            ves_vagon trans = transfer[transfer.Count - ves_vagon_records];

                            if (trans != null)
                            {
                                var scale = GetCTSWagonScale(trans.scales.name, wagonScales);
                                var item = items.Where(x => x.Name == trans.gruz).FirstOrDefault();

                                var transfers = new WagonTransfer()
                                {
                                    ID = trans.id.ToString(),
                                    TransferTimeStamp = trans.date_time_brutto,
                                    LasEditDateTime = DateTime.Now,
                                    OperatorName = "DBSync",
                                    LotName = trans.id_sostav.ToString() == "" ? "???" : trans.id_sostav.ToString(),
                                    SublotName = trans.vagon_num,
                                    OrderNumber = trans.nakladn,
                                    Tare = (float)trans.ves_tara / 1000,
                                    Brutto = (float)trans.ves_brutto / 1000,
                                    Netto = (float)trans.ves_netto / 1000,
                                    NettoByOrder = (float)trans.ves_netto_docs / 1000,
                                    EquipID = (scale != null) ? scale.ID : 1,
                                    ItemID = (item != null) ? item.ID : 1,
                                    Direction = trans.napravlenie.display_name ?? "",
                                    IsValid = true,
                                    Status = 0
                                };

                                //if (trans.otpravl != null)
                                //{
                                //    transfers.FromDestID = (trans.otpravl.name == "0") ? "???" : trans.otpravl.name;
                                //}

                                //if (trans.poluch != null)
                                //{
                                //    transfers.ToDest = (trans.poluch.display_name == "0") ? "???" : trans.poluch.display_name;
                                //}

                                centralDB.WagonTransfers.AddOrUpdate(transfers);
                                centralDB.SaveChanges();

                                using (var wagDB = new WagonDBcontext())
                                {
                                    var originalTransfer = wagDB.ves_vagon.Find(Int32.Parse(transfers.ID));
                                    if (originalTransfer != null)
                                    {
                                        originalTransfer.sync = 1;
                                        wagDB.Entry(originalTransfer).State = EntityState.Modified;
                                    }
                                    wagDB.SaveChanges();
                                }

                            }
                        }
                    }
                }
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }

        private bool SyncWagonRecognition()
        {
            var recogns = new List<Recogn>();
            var nums = new List<WagonNumsCache>();

            try
            {
                using (CtsDbContext centralDB = new CtsDbContext())
                {
                    try
                    {
                        recogns.AddRange(centralDB.Recogn.ToList()); //распознование
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                string timeDate = null;

                using (var wagDB = new WagonDBcontext())
                {
                    for (recognition_records = recogns.Count;
                        recognition_records > 0;
                        recognition_records--)
                    {

                        Recogn r = recogns[recogns.Count - recognition_records];

                        nums.AddRange(wagDB.vagon_nums.Where(x => x.recognid == r.ID).OrderByDescending(x => x.date_time).Take(50)
                            .Select(x => new WagonNumsCache
                            {
                                ID = x.id,
                                Date_time = x.date_time,
                                RecognID = x.recognid == null ? 123123123 : x.recognid,
                                Id_sostav = x.id_sostav == null ? 123123123 : x.id_sostav,
                                Number = x.number.Equals(null) ? "???" : x.number,
                                Number_operator = x.number_operator == null ? "???" : x.number_operator,
                                Id_operator = x.id_operator == null ? 123123123 : x.id_operator,
                                Camera = x.camera == null ? "???" : x.camera,
                            }).ToList());

                        timeDate = nums.Select(x => x.Date_time).ToString();

                    }

                }

                if (nums.Any())
                {
                    using (CtsDbContext centralDB = new CtsDbContext())
                    {
                        //using (var transaction = centralDB.Database.BeginTransaction())
                        //{
                        centralDB.WagonNumsCache.RemoveRange(centralDB.WagonNumsCache);
                        centralDB.WagonNumsCache.AddRange(nums);
                        centralDB.SaveChanges();
                        //    transaction.Commit();
                        //}
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        private WagonScale GetCTSWagonScale(string wagonDbScaleName, IEnumerable<WagonScale> ctsWagonScales)
        {
            switch (wagonDbScaleName.ToLower())
            {
                case "cofv_r1":
                    return ctsWagonScales.FirstOrDefault(s => s.ID == 17); // ID=17 => ЮГ1
                case "cofv_r2":
                    return ctsWagonScales.FirstOrDefault(s => s.ID == 18); // ID=18 => ЮГ2
                case "cofv_r3":
                    return ctsWagonScales.FirstOrDefault(s => s.ID == 11); // ID=18 => Север
                default:
                    return ctsWagonScales.FirstOrDefault(x => x.LocationID == wagonDbScaleName);
            }
        }


        public void Handler()
        {
            Int32.TryParse(ConfigurationManager.AppSettings["WagonDBSyncDepth_Days"], out int syncDepth);
            Int32.TryParse(ConfigurationManager.AppSettings["WagonDBSyncDepth_RevisionMonths"], out int syncDepth_rev);

            int month_counter = 0;

            while (true)
            {

                try
                {
                    start_time = DateTime.Now;

                    DateTime from_ts;
                    DateTime to_ts;

                    var dueDate = syncDepth == 0 ? default(DateTime) : DateTime.Now.AddDays(syncDepth * -1);

                    from_ts = dueDate;
                    to_ts = DateTime.Now;
                    Debug.WriteLine(string.Format("GetAsyncWagon from {0} to {1}", from_ts, to_ts));
                    GetAsyncWagon(from_ts, to_ts);

                    if (!SyncWagonRecognition())
                    {
                        throw new System.ArgumentException("SyncWagonRecognition", "original");
                    }

                    from_ts = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(month_counter + 1 * (-1));
                    to_ts = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(month_counter * (-1));
                    Debug.WriteLine(string.Format("GetAsyncWagon from {0} to {1}", from_ts, to_ts));
                    GetAsyncWagon(from_ts, to_ts); //revision

                    month_counter++;

                    if (month_counter > syncDepth_rev)
                    {
                        month_counter = 1;
                    }

                    lead_time = DateTime.Now.Subtract(start_time);
                    error = false;
                }
                catch (Exception)
                {
                    error = true;
                }

                System.Threading.Thread.Sleep(60000);
            }
        }
    }
}

