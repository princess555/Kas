using CTS_Models;
using CTS_Models.DBContext;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using StackExchange.Profiling;
using System.Data.SqlClient;
using System.Data;
using System.Data.Linq;
using System.Configuration;
using System.Web.Mvc;
using System.Web.UI;
using System.Diagnostics;


namespace CTS_Analytics.Services
{
    public class CentralDBService
    {
        private readonly CtsDbContext cdb;

        public CentralDBService()
        {
            this.cdb = new CtsDbContext();

        }

        //[OutputCache(Duration = 30, Location = OutputCacheLocation.Server)]
        public decimal GetPlanData(string location, DateTime fromDate, DateTime toDate)
        {
            var planSum = cdb.LocalPlansBWithLocationID
                .Where(l => location.Contains(l.LocationID))
                .Where(d => d.Date >= fromDate && d.Date <= toDate)
                .Select(p => p.Plan)
                .DefaultIfEmpty()
                .Sum();

            if (location == "sar1" || location == "sar3")
                planSum = planSum / 2;

            return planSum;
        }

        //[OutputCache(Duration = 30, Location = OutputCacheLocation.Server)]
        public int GetStuffCount(int location, DateTime fromDate, DateTime toDate)
        {
            var staffnum = cdb.LocalStaffs
                    .Where(s => s.ShopID == location)
                    .Where(d => d.Date <= toDate && d.Date >= fromDate)
                    .Select(v => v.CNT)
                    .DefaultIfEmpty()
                    .Select(int.Parse)
                    .ToArray()
                    .Sum();

            return staffnum;
        }

        //[OutputCache(Duration = 30, Location = OutputCacheLocation.Server)]
        public Location FindLocationByLocationID(string locationId)
        {
            return cdb.Locations.Find(locationId);
        }

        //[OutputCache(Duration = 30, Location = OutputCacheLocation.Server)]
        public TEquip FindEquipmentById<TEquip>(int Id) where TEquip : class, IEquip
        {
            var bd = new CtsEquipContext<TEquip>();
            return bd.DbSet.Find(Id);
        }

        //[OutputCache(Duration = 30, Location = OutputCacheLocation.Server)]
        public DateTime GetStartShiftTime(string locationId)
        {
            var nowTime = TimeSpan.Parse(System.DateTime.Now.ToString("HH:mm:ss"));
            var times = cdb.Shifts
                .Where(l => l.LocationID == locationId)
                .ToArray();
            if (times.Length == 0)
                return DateTime.Today;

            var startTime = times
                .Where(t => t.TimeStart < nowTime)
                .OrderByDescending(c => c.TimeStart)
                .Select(f => f.TimeStart)
                .FirstOrDefault();
            if (startTime == default(TimeSpan))
            {
                nowTime = new TimeSpan(23, 59, 59);
                startTime = times
                .Where(t => t.TimeStart < nowTime)
                .OrderByDescending(c => c.TimeStart)
                .Select(f => f.TimeStart)
                .First();
                return DateTime.Today.AddDays(-1).Add(startTime);
            }

            return DateTime.Today.Add(startTime);
        }

        //[OutputCache(Duration = 30, Location = OutputCacheLocation.Server)]
        public DateTime GetEndShiftTime(string locationId, DateTime startShiftTime)
        {
            var nowTime = TimeSpan.Parse(startShiftTime.ToString("HH:mm:ss"));
            var times = cdb.Shifts
                .Where(l => l.LocationID == locationId)
                .ToArray();
            if (times.Length == 0)
                return startShiftTime.AddHours(24);

            var endTime = times
                .Where(t => t.TimeStart > nowTime)
                .OrderByDescending(c => c.TimeStart)
                .Select(f => f.TimeStart)
                .LastOrDefault();

            if (endTime == default(TimeSpan))
            {
                nowTime = new TimeSpan(0, 00, 1);
                endTime = times
                .Where(t => t.TimeStart > nowTime)
                .OrderByDescending(c => c.TimeStart)
                .Select(f => f.TimeStart)
                .Last();
                return DateTime.Today.AddDays(1).Add(endTime);
            }

            return System.DateTime.Today.Add(endTime);
        }

        public IQueryable<TTransfer> GetTransfersFirst<TTransfer>(int Id, DateTime fromDate, DateTime toDate) where TTransfer : class, ITransfer
        {
            var db = new CtsTransferContext<TTransfer>();
            var db2 = db.DbSet
                        .Where(s => s.EquipID == Id)
                        .Where(d => d.TransferTimeStamp > fromDate && d.TransferTimeStamp < toDate)
                        .Where(v => v.IsValid == true)
                        .OrderByDescending(t => t.TransferTimeStamp)
                        .AsNoTracking();

            foreach (var item in db2)
            {
                Debug.WriteLine("Time: " + item.TransferTimeStamp);
            }
            return db2;
        }

        public IQueryable<TTransfer> GetTransfers<TTransfer>(int Id, DateTime fromDate, DateTime toDate) where TTransfer : class, ITransfer
        {
            var db = new CtsTransferContext<TTransfer>();

            var db2 = db.DbSet
                 .Where(s => s.EquipID == Id)
                 .Where(d => d.TransferTimeStamp >= fromDate && d.TransferTimeStamp <= toDate)
                 .Where(v => v.IsValid == true)
                 .OrderByDescending(t => t.TransferTimeStamp)
                 .AsNoTracking();
            //using (CtsDbContext db = new CtsDbContext())
            //{
            //    var search = db.BeltTransfers.Where(x => x.)
            //}



            foreach (var item in db2)
            {
                Debug.WriteLine(item.TransferTimeStamp);
            }

            return db2;
        }

        public IQueryable<TTransfer> GetTransfersT<TTransfer>(int Id, DateTime fromDate, DateTime toDate) where TTransfer : class, ITransfer
        {
            var db = new CtsTransferContext<TTransfer>();
            var db2 = db.DbSet
                 .Where(s => s.EquipID == Id)
                 .Where(d => d.TransferTimeStamp >= fromDate && d.TransferTimeStamp <= toDate)
                 //.Where(v => v.IsValid == true)
                 .OrderByDescending(t => t.TransferTimeStamp)
                 .AsNoTracking();

            foreach (var item in db2)
            {
                Debug.WriteLine(item.TransferTimeStamp);
            }
            return db2;
        }

        public IQueryable<TTransfer> GetTransfersTime<TTransfer>(int Id, DateTime fromDate, DateTime toDate) where TTransfer : class, ITransfer
        {
            var db = new CtsTransferContext<TTransfer>();
            return db.DbSet
               .Where(s => s.EquipID == Id)
               .Where(d => d.TransferTimeStamp <= fromDate && d.TransferTimeStamp <= toDate);

        }

        [OutputCache(Duration = 30, Location = OutputCacheLocation.Server)]
        public IQueryable<WagonTransfer> GetWagonTransfersSendFromMine(Location toDestLocation, DateTime fromDate, DateTime toDate, string fromMineId)
        {
            var db = new CtsTransferContext<WagonTransfer>();
            return db.DbSet
            .Where(t => t.Equip.Location.ID == fromMineId && t.IsValid == true)
            .Where(t => t.ToDest.Contains(toDestLocation.LocationName) || t.ToDest == toDestLocation.ID)
            .Where(t => t.Direction == CTS_Core.ProjectConstants.WagonDirection_FromObject)
            .Where(t => t.TransferTimeStamp >= fromDate && t.TransferTimeStamp <= toDate)
            .AsNoTracking();
        }

        //[OutputCache(Duration = 30, Location = OutputCacheLocation.Server)]
        public IQueryable<WagonTransfer> GetWagonTransfersArrivedFromMine(string fromMineId, DateTime fromDate, DateTime toDate, string arrivedLocationId)
        {
            var db = new CtsTransferContext<WagonTransfer>();
            var test = db.DbSet
            .Where(t => t.Equip.Location.ID == arrivedLocationId && t.IsValid == true)
            .Where(t => t.TransferTimeStamp >= fromDate && t.TransferTimeStamp <= toDate).ToArray();
            return db.DbSet
            .Where(t => t.Equip.Location.ID == arrivedLocationId && t.IsValid == true)
            .Where(t => t.FromDestID == fromMineId)
            //.Where(t => t.Direction == CTS_Core.ProjectConstants.WagonDirection_ToObject)
            .Where(t => t.TransferTimeStamp >= fromDate && t.TransferTimeStamp <= toDate)
            .AsNoTracking();
        }

        //[OutputCache(Duration = 30, Location = OutputCacheLocation.Server)]
        public IQueryable<WagonTransfer> GetWagonTransfersIncludeLocations(int? wagonScaleID, DateTime fromDate, DateTime toDate)
        {
            return cdb.WagonTransfers.Include(w => w.Equip.Location)
                .Where(s => wagonScaleID != null ? s.EquipID == wagonScaleID : true)
                .Where(d => d.TransferTimeStamp >= fromDate && d.TransferTimeStamp <= toDate)
                .Where(v => v.IsValid == true)
                .OrderByDescending(t => t.TransferTimeStamp)
                .AsNoTracking();
        }

        //[OutputCache(Duration = 30, Location = OutputCacheLocation.Server)]
        public IQueryable<WarehouseTransfer> GetWarehouseTransfers(int Id, DateTime fromDate, DateTime toDate)
        {
            return cdb.WarehouseTransfers
               .Where(s => s.WarehouseID == Id)
               .Where(d => d.TransferTimeStamp >= fromDate && d.TransferTimeStamp <= toDate)
               .OrderByDescending(t => t.TransferTimeStamp)
               .AsNoTracking();
        }

        //[OutputCache(Duration = 30, Location = OutputCacheLocation.Server)]
        public IQueryable<Location> GetAlllocaitons()
        {
            return cdb.Locations;
        }

        //[OutputCache(Duration = 30, Location = OutputCacheLocation.Server)]
        public int GetLocationIncome(string locationId, DateTime fromDate, DateTime toDate)
        {
            return (int)cdb.WagonTransfers
                .Where(t => t.TransferTimeStamp >= fromDate && t.TransferTimeStamp <= toDate)
                .Where(d => d.ToDest == locationId && d.IsValid == true)
                .Select(t => t.Brutto)
                .DefaultIfEmpty()
                .Sum();
        }

        //[OutputCache(Duration = 30, Location = OutputCacheLocation.Server)]
        public int GetLocationOutcome(string locationId, DateTime fromDate, DateTime toDate)
        {
            return (int)cdb.WagonTransfers
                .Where(t => t.TransferTimeStamp >= fromDate && t.TransferTimeStamp <= toDate)
                .Where(d => d.Equip.Location.ID == locationId && d.IsValid == true)
                .Select(t => t.Brutto)
                .DefaultIfEmpty()
                .Sum();
        }

        [OutputCache(Duration = 30, Location = OutputCacheLocation.Server)]
        public int GetWagonScalesIdOnLocation(string locationId)
        {
            return cdb.WagonScales
                .Where(m => m.Location.ID == locationId)
                .FirstOrDefault()?
                .ID ?? 1;
        }

        /// <summary>
        /// Получение угля, котельная
        /// </summary>
        /// <param name="convId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public float GetBoilerConveyorProduction(int convId, DateTime fromDate, DateTime toDate)
        {
            var producationPerTimeInterval = cdb.InternalTransfers
                                    .Where(s => s.EquipID == convId)
                                    .Where(d => d.TransferTimeStamp >= fromDate && d.TransferTimeStamp <= toDate)
                                    .Where(v => v.IsValid == true).OrderByDescending(t => t.TransferTimeStamp)
                                    .Select(t => t.LotQuantity).DefaultIfEmpty(0);
            return producationPerTimeInterval.Sum().GetValueOrDefault();
        }

        struct normTable
        {
            public DateTime td;
            public int norm;
            public float weight_min;
        }
        struct min_month
        {
           public double minutes;
           public DateTime ts;
        }

        /// <summary>
        /// Нормы потребления
        /// </summary>
        /// <param name="mineId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public double GetConsumptionNorm(string mineId, DateTime fromDate, DateTime toDate)
        {
            List<DateTime> dateCount = new List<DateTime>();
            var d = new Hashtable();
            int[] month = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

            List<normTable> nTable = new List<normTable>();

            double countBorms = 0;
            int[] days = new int[366];
            float norm;
            days = GetCountDates(fromDate, toDate);

            int[] months = GetCountDays(fromDate, toDate);

            try
            {
                
                string connectionString = ConfigurationManager.ConnectionStrings["CentralDbConnection"].ConnectionString;
                string sqlExp = $"SELECT* FROM[CoalTracking].[dbo].[BoilerConsNormNews] b WHERE IDLocations = '{mineId}' AND b.TimeData >= '{new DateTime(fromDate.Year, fromDate.Month, 1).ToString("yyyy-MM-dd")}' AND b.TimeData <= '{new DateTime(toDate.Year, toDate.Month, 1).ToString("yyyy-MM-dd")}'";
                DataContext db = new DataContext(connectionString);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand(sqlExp, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows) // если есть данные
                    {
                        while (reader.Read()) // построчно считываем данные
                        {
                            object cornNurm = reader.GetValue(1);
                            object timeData = reader.GetValue(2);

                            d.Add(timeData, cornNurm);
                            nTable.Add(new normTable()
                            {
                                td = Convert.ToDateTime(timeData),
                                norm = Convert.ToInt32(cornNurm),
                                weight_min = 0
                            });
                        }
                        reader.Close();
                        connection.Close();
                    }

                    normTable var = new normTable();
                    //float countDays = 0;

                    for (int i = 0; i <= nTable.Count - 1; i++)
                    {
                        var = nTable[i];
                        var.weight_min = (Convert.ToSingle(var.norm) / (DateTime.DaysInMonth(var.td.Year, var.td.Month) * 60 * 24));//вес в минуту
                        nTable[i] = var;
                    }

                    min_month[] test = GetMinutes(fromDate, toDate);

                    foreach (var item in test)
                    {

                        normTable result = nTable.Find(x => x.td == item.ts);

                        if (!result.Equals(null))
                        {
                            countBorms += item.minutes * result.weight_min;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return countBorms;
        }

        private int[] GetCountDates(DateTime start, DateTime finish)
        {
            List<int> result = new List<int>();

            while (true)
            {
                var currentDayInStart = DateTime.DaysInMonth(start.Year, start.Month);
                var currentDayInFinish = DateTime.DaysInMonth(finish.Year, finish.Month);

                if (start.AddDays(currentDayInStart - start.Day) > finish)
                {
                    result.Add(Convert.ToInt32(finish.Day));
                    break;
                }
                else
                {
                    result.Add(Convert.ToInt32(currentDayInStart - start.Day) + 1);
                    break;
                }

                
                
                start = start.AddDays(currentDayInStart - start.Day + 1);

            }


            return result.ToArray();
        }

        private int[] GetCountDays(DateTime from, DateTime to)
        {
            //int[] months = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            int[] months = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            List<int> monthsBuffer = new List<int>();
            int fromm = from.Month;

            for (int i = 1; i < months.Length; i++)
            {
                //if (months[i] == 0)
                //{
                //    monthsBuffer.Add(0);
                ////}
                //if()
                //{

                //}
              if (to.Month >= months[i] && months[i] <= to.Month)
                {
                    monthsBuffer.Add(DateTime.DaysInMonth(from.Year, months[i]));
                }
            }
            return monthsBuffer.ToArray();
        }


        private min_month[] GetMinutes(DateTime from, DateTime to)
        {

            List <min_month> result = new List<min_month>();


            if (to > from)
            {

                int years = to.Year - from.Year;

                switch (years)
                {

                    case 0:
                        {

                            result.AddRange(GetDays(from, to));
                            break;
                        }

                    case 1:
                        {
                            result.AddRange(GetDays(from, new DateTime(from.Year, 12, 31)));
                            result.AddRange(GetDays(new DateTime(to.Year, 1 , 1), to));
                            break;
                        }

                    default:
                        {
                            result.AddRange(GetDays(from, new DateTime(from.Year, 12, 31)));
                            for (int i = from.Year + 1; i <= to.Year-1; i++)
                            {
                                result.AddRange(GetDays(new DateTime(i, 1, 1), new DateTime(i, 12, 31)));
                            }
                            result.AddRange(GetDays(new DateTime(to.Year, 1, 1), to));

                            break;
                        }
                }


            }

            return result.ToArray();
        }


        private min_month[] GetDays(DateTime from, DateTime to)
        {
            List<min_month> result = new List<min_month>();

            if (to.Year == from.Year)
            {

                int months = to.Month - from.Month;

                switch (months)
                {
                    case 0:
                        {

                            result.Add(new min_month { minutes = to.Subtract(from).TotalMinutes, ts = new DateTime(from.Year, from.Month, 1) });
                            break;
                        }
                    case 1:
                        {
                            result.Add(new min_month { minutes = to.Subtract(new DateTime(to.Year, to.Month, 1)).TotalMinutes, ts = new DateTime(to.Year, to.Month, 1) });
                            result.Add(new min_month { minutes =  new DateTime(from.Year, from.Month, DateTime.DaysInMonth(from.Year, from.Month)).Subtract(from).TotalMinutes, ts = new DateTime(from.Year, from.Month, 1) });
                            break;
                        }
                    default:
                        {
                            result.Add(new min_month { minutes = to.Subtract(new DateTime(to.Year, to.Month, 1)).TotalMinutes, ts = new DateTime(to.Year, to.Month, 1) });
                            for (int i = from.Month+1; i <= to.Month-1; i++)
                            {
                                result.Add(new min_month { minutes = DateTime.DaysInMonth(from.Year, i)*60*24, ts = new DateTime(from.Year, i, 1) });
                            }
                            result.Add(new min_month { minutes = new DateTime(from.Year, from.Month, DateTime.DaysInMonth(from.Year, from.Month)).Subtract(from).TotalMinutes, ts = new DateTime(from.Year, from.Month, 1) });
                            break;
                        }
                }

            }

            return result.ToArray();
        }


    }
}

//int[] countM = n;

//private float GetNormsFirst(DataTable table, DateTime fromDate, DateTime toDate, int[] days, string mineId/*, int countMonth*/)
//{
//    float normF = 0;
//    try
//    {
//        string corn, loc;
//        DateTime dates;
//        List<DateTime> dateCount = new List<DateTime>();
//        int[] months = new int[12];
//        List<string> weight = new List<string>();


//        foreach (DataRow row in table.Rows)
//        {
//            dates = Convert.ToDateTime(row["date"]);
//            loc = row["IDLocations"].ToString();
//            corn = row["CornsNorm"].ToString();


//            using (CtsDbContext db = new CtsDbContext())
//            {
//                var count = db.BoilerConsNormNew.Where(x => x.TimeData );
//            }

//            if (dates <= fromDate || dates <= toDate)
//            {

//                //    counter++;
//                //}
//                for (int i = fromDate.Year; i <= toDate.Year; i++)
//                {
//                    for (int j = 1; j <= toDate.Month; j++)
//                    {

//                        months[j] = DateTime.DaysInMonth(i, j);
//                        weight.Add(corn);
//                    }
//                }

//                //var currentDayInFinish = DateTime.DaysInMonth(dateTo.Year, dateTo.Month);

//                for (int i = 0; i < months.Length; i++)
//                {
//                    if (months[i] == 0)
//                    {
//                        continue;
//                    }
//                    else
//                    {
//                        normF += (Convert.ToSingle(weight) / months[i]) * days[i];

//                    }
//                }
//            }
//        }
//        Debug.WriteLine(normF);
//    }
//    catch (Exception ex)
//    {

//    }
//    return normF;

//}

//private float GetNormsSecond(DataTable table, DateTime date, int[] sumDaysInMonth, string mineID)
//{
//    float normS = 0;
//    try
//    {
//        foreach (DataRow row in table.Rows)
//        {
//            string corn = row["CornsNorm"].ToString();
//            string dates = Convert.ToDateTime(row["date"]).ToString();
//            string loc = row["IDLocations"].ToString();

//            if (mineID == loc)
//            {
//                if (Convert.ToDateTime(dates).Month == Convert.ToDateTime(date).Month
//                    //&& Convert.ToDateTime(dates).Day == Convert.ToDateTime(date).Day
//                    && Convert.ToDateTime(dates).Year == Convert.ToDateTime(date).Year)
//                {
//                    if (corn == "")
//                    {
//                        corn = "0";
//                        normS = Convert.ToSingle(corn) / sumDaysInMonth[1];
//                        break;
//                    }
//                    else
//                    {
//                        normS = Convert.ToSingle(corn) / sumDaysInMonth[1];
//                        break;
//                    }
//                }
//            }
//        }
//    }
//    catch (Exception ex)
//    { }
//    return normS;
//}


//    }
//}