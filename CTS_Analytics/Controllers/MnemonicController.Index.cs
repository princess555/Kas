﻿using CTS_Analytics.Models.Mnemonic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CTS_Models;
using CTS_Models.DBContext;
using CTS_Analytics.Helpers;
using Newtonsoft.Json;
using CTS_Core;
using CTS_Analytics.Services;

namespace CTS_Analytics.Controllers
{
    [CtsAuthorize(Roles = Roles.AnalyticsRoleName)]
    public partial class MnemonicController : CtsAnalController
    {
        class SkipIndexData
        {
            public string LocationID { get; set; }
            public float ProdData { get; set; }
            public string LiftingID { get; set; }
        }

        class WagonIndexData
        {
            public string LocationID { get; set; }
            public string Direction { get; set; }
            public float? Brutto { get; set; }
            public string ToDest { get; set; }
        }

        class StocpileIndexData
        {
            public string LocationID { get; set; }
            public DateTime TransferTimeStamp { get; set; }
            public double? TotalQuantity { get; set; }
        }

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private string[] _locations = new string[] { "shah", "kuz", "kost", "abay", "len", "sar1", "sar3", "kaz", "tent" };

        public ActionResult Index()
        {
            var model = GetMnemoticMainModel();
            return View(model);
        }

        public ActionResult kotel()
        {
            var model = new MnemonicMainKotel();
            model.shah = Builder.GetMineKotelModel("shah");
            model.kuz = Builder.GetMineKotelModel("kuz");
            model.abay = Builder.GetMineKotelModel("abay");
            model.len = Builder.GetMineKotelModel("len");
            model.sar1 = Builder.GetMineKotelModel("sar1");
            model.sar3 = Builder.GetMineKotelModel("sar3");
            model.kaz = Builder.GetMineKotelModel("kaz");
            model.tent = Builder.GetMineKotelModel("tent");
            model.kost = Builder.GetMineKotelModel("kost");
            return View(model);
        }

        public ActionResult Map()
        {
            var fromDate = GetDateFromCookie("fromdate");
            var toDate = GetDateFromCookie("todate");
            var serializedParent = JsonConvert.SerializeObject(GetMnemoticMainModel()); // https://stackoverflow.com/questions/988658/unable-to-cast-from-parent-class-to-child-class
            MnemonicMainMap model = JsonConvert.DeserializeObject<MnemonicMainMap>(serializedParent); // Because I am lazy - Nobody care about it dude=)
            model.AlarmDict = new Dictionary<string, AlarmCountModel>();
            Parallel.ForEach(_locations, (location) =>
            {
                var alarmsStatistics = AlarmHepler.GetMineAlarmCount(location, fromDate, toDate);
                var locationName = GetLocationNameOnCurrentLanguate(location);
                model.AlarmDict.Add(locationName, alarmsStatistics);
            });

            return View(model);
        }

        public string GetWagonWeightDetails(int weightScaleId)
        {
            var fromDate = GetDateFromCookie("fromdate");
            var toDate = GetDateFromCookie("todate");
            using (var cdb = new CtsDbContext())
            {
                var weightTransfers = cdb.WagonTransfers
                    .Where(d => d.TransferTimeStamp >= fromDate && d.TransferTimeStamp <= toDate)
                    .Where(t => t.EquipID == weightScaleId);

                var periodWeight = weightTransfers.Sum(s => s.Netto);
                var trainWeight = weightTransfers
                    .OrderByDescending(t => t.TransferTimeStamp)
                    .GroupBy(v => v.LotName)
                    .FirstOrDefault()?
                    .Sum(s => s.Netto);

                return " <div class='block-info green' id='bi1'>" +
                    "<div class='title'>" +
                    "</div>" +
                    "<div class='line'>" +
                      "<span class='name'>" + Resources.ResourceMnemonic.WeightConclusionIn1Train + "</span>" +
                      "<div class='count'>" + (int?)trainWeight + "</div>" +
                      "<span class='caption'>" + Resources.ResourceMnemonic.Tonns + "</span>" +
                      "<div class='clearfix'></div>" +
                    "</div>" +
                    "<div class='line'>" +
                        "<span class='name'>" + Resources.ResourceMnemonic.WeightConclusionWeightPerPeriod + "</span>" +
                        "<div class='count'>" + (int?)periodWeight + "</div>" +
                        "<span class='caption'>" + Resources.ResourceMnemonic.Tonns + "</span>" +
                        "<div class='clearfix'></div>" +
                    "</div>" +
                "</div>";
            }
        }

        public ActionResult MapGps()
        {
            var model = GetMnemoticMainModel();
            return View(model);
        }

        private MnemonicMain GetMnemoticMainModel()
        {
            var model = new MnemonicMain();
            var fromDate = GetDateFromCookie("fromdate");
            var toDate = GetDateFromCookie("todate");
            try
            {
                var prodFactDataTask = Task.Run(() =>
                {
                    var cdb = new CtsDbContext();

                    var prodFactData = cdb.SkipTransfers.Where(d => d.TransferTimeStamp >= fromDate && d.TransferTimeStamp <= toDate).Where(v => v.IsValid == true).Select(s => new SkipIndexData()
                    {
                        LocationID = s.Equip.Location.ID,
                        ProdData = s.SkipWeight,
                        LiftingID = s.LiftingID
                    }).ToArray();
                    model.Kuz.ProdFact = GetProductionData("kuz", prodFactData);
                    model.Kost.ProdFact = GetProductionData("kost", prodFactData);
                    model.Abay.ProdFact = GetProductionData("abay", prodFactData);
                    model.Len.ProdFact = GetProductionData("len", prodFactData);
                    model.Sar1.ProdFact = GetProductionData("sar1", prodFactData);
                    model.Sar3.ProdFact = GetProductionData("sar3", prodFactData);
                    model.Kaz.ProdFact = GetProductionData("kaz", prodFactData);
                    model.Shah.ProdFact = GetProductionData("shah", prodFactData);
                    model.Tent.ProdFact = (int)Builder.GetBeltScaleModel(21).ProductionPerTimeInterval;
                    model.Tent.ProdFact = (int)Builder.GetBeltScaleModel(25).ProductionPerTimeInterval;
                });
                var planDataTask = Task.Run(() =>
                {
                    var service = new CentralDBService();
                    model.Kuz.ProdPlan = service.GetPlanData("kuz", fromDate, toDate);
                    model.Kost.ProdPlan = service.GetPlanData("kost", fromDate, toDate);
                    model.Abay.ProdPlan = service.GetPlanData("abay", fromDate, toDate);
                    model.Len.ProdPlan = service.GetPlanData("len", fromDate, toDate);
                    model.Sar1.ProdPlan = service.GetPlanData("sar1", fromDate, toDate);
                    model.Sar3.ProdPlan = service.GetPlanData("sar3", fromDate, toDate);
                    model.Kaz.ProdPlan = service.GetPlanData("kaz", fromDate, toDate);
                    model.Shah.ProdPlan = service.GetPlanData("shah", fromDate, toDate);
                    model.Tent.ProdPlan = service.GetPlanData("tent", fromDate, toDate);
                });
                var shippedDataTask = Task.Run(() =>
                {
                    var cdb = new CtsDbContext();

                    var shippedData = cdb.WagonTransfers.Where(d => d.TransferTimeStamp >= fromDate && d.TransferTimeStamp <= toDate).Where(v => v.IsValid == true).Select(s => new WagonIndexData()
                    {
                        LocationID = s.Equip.LocationID,
                        Direction = s.Direction,
                        Brutto = s.Brutto,
                        ToDest = s.ToDest
                    }).ToArray();
                    model.Kuz.Shipped = GetShippedData("kuz", shippedData);
                    model.Kost.Shipped = GetShippedData("kost", shippedData);
                    model.Abay.Shipped = GetShippedData("abay", shippedData);
                    model.Len.Shipped = GetShippedData("len", shippedData);
                    model.Sar1.Shipped = GetShippedData("sar1", shippedData);
                    model.Sar3.Shipped = GetShippedData("sar3", shippedData);
                    model.Kaz.Shipped = GetShippedData("kaz", shippedData);
                    model.Shah.Shipped = GetShippedData("shah", shippedData);
                    model.Tent.Shipped = GetShippedData("tent", shippedData);

                    model.COF.Shipped = GetShippedData("cofv", shippedData);

                    model.StAbay.Outcome = GetShippedData("sabay", shippedData);
                    model.StDubov.Outcome = GetShippedData("sdub", shippedData);
                    model.StRaspor.Outcome = GetShippedData("srasp", shippedData);
                    model.StUgler.Outcome = GetShippedData("sugl", shippedData);

                    model.StAbay.Income = GetStationIncome(shippedData, cdb.Locations.Find("sabay").LocationName);
                    model.StDubov.Income = GetStationIncome(shippedData, cdb.Locations.Find("sdub").LocationName);
                    model.StRaspor.Income = GetStationIncome(shippedData, cdb.Locations.Find("srasp").LocationName);
                    model.StUgler.Income = GetStationIncome(shippedData, cdb.Locations.Find("sugl").LocationName);

                });

                var atStockpileTask = Task.Run(() =>
                {
                    var cdb = new CtsDbContext();

                    var skladData = cdb.WarehouseTransfers.Where(d => d.TransferTimeStamp >= fromDate && d.TransferTimeStamp <= toDate).Select(s => new StocpileIndexData()
                    {
                        LocationID = s.Warehouse.LocationID,
                        TransferTimeStamp = s.TransferTimeStamp,
                        TotalQuantity = s.TotalQuantity
                    }).ToArray();
                    model.Kuz.AtStockpile = GetStockpileValue("kuz", skladData);
                    model.Kost.AtStockpile = GetStockpileValue("kost", skladData);
                    model.Abay.AtStockpile = GetStockpileValue("abay", skladData);
                    model.Len.AtStockpile = GetStockpileValue("len", skladData);
                    model.Sar1.AtStockpile = GetStockpileValue("sar1", skladData);
                    model.Sar3.AtStockpile = GetStockpileValue("sar3", skladData);
                    model.Kaz.AtStockpile = GetStockpileValue("kaz", skladData);
                    model.Shah.AtStockpile = GetStockpileValue("shah", skladData);
                    model.Tent.AtStockpile = GetStockpileValue("tent", skladData);

                });

                var warningTask = Task.Run(() =>
                {
                    AlarmHepler.GetMineAlarmLevel(model.Kuz, fromDate, toDate);
                    AlarmHepler.GetMineAlarmLevel(model.Kost, fromDate, toDate);
                    AlarmHepler.GetMineAlarmLevel(model.Abay, fromDate, toDate);
                    AlarmHepler.GetMineAlarmLevel(model.Len, fromDate, toDate);
                    AlarmHepler.GetMineAlarmLevel(model.Sar1, fromDate, toDate);
                    AlarmHepler.GetMineAlarmLevel(model.Sar3, fromDate, toDate);
                    AlarmHepler.GetMineAlarmLevel(model.Kaz, fromDate, toDate);
                    AlarmHepler.GetMineAlarmLevel(model.Shah, fromDate, toDate);
                    AlarmHepler.GetMineAlarmLevel(model.Tent, fromDate, toDate);
                });

                Task.WaitAll(prodFactDataTask, planDataTask, shippedDataTask, atStockpileTask, warningTask);
                model.Abay.MineName = GetLocationNameOnCurrentLanguate("abay");
                model.Kaz.MineName = GetLocationNameOnCurrentLanguate("kaz");
                model.Kost.MineName = GetLocationNameOnCurrentLanguate("kost");
                model.Kuz.MineName = GetLocationNameOnCurrentLanguate("kuz");
                model.Len.MineName = GetLocationNameOnCurrentLanguate("len");
                model.Sar1.MineName = GetLocationNameOnCurrentLanguate("sar1");
                model.Sar3.MineName = GetLocationNameOnCurrentLanguate("sar3");
                model.Shah.MineName = GetLocationNameOnCurrentLanguate("shah");
                model.Tent.MineName = GetLocationNameOnCurrentLanguate("tent");
                model.StAbay.StationName = GetLocationNameOnCurrentLanguate("sabay");
                model.StAbay.StationID = "sabay";
                model.StDubov.StationName = GetLocationNameOnCurrentLanguate("sdub");
                model.StAbay.StationID = "sbub";
                model.StRaspor.StationName = GetLocationNameOnCurrentLanguate("srasp");
                model.StAbay.StationID = "srasp";
                model.StUgler.StationName = GetLocationNameOnCurrentLanguate("sugl");
                model.StAbay.StationID = "sugl";
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception getting data for GetMnemoticMainModel");
            }

            return model;
        }

        private int GetProductionData(string location, IEnumerable<SkipIndexData> data)
        {
            var skipTransfers = data.Where(s => s.LocationID == location);
            // var sum = skipTransfers?.Sum(st => st.SkipWeight); // Leave old calculation for a while
            var sum = skipTransfers?.Select(st => int.Parse(st.LiftingID) * st.ProdData).Sum(); // TODO Change to this calculation when ready
            return (int)sum;
        }

        private int GetShippedData(string location, IEnumerable<WagonIndexData> data)
        {
            var temp = data
                .Where(t => t.LocationID.Equals(location))
                .Where(tr => tr.Direction == CTS_Core.ProjectConstants.WagonDirection_FromObject);
            return (int)temp?.Sum(tr => tr.Brutto);
        }

        private float GetStationIncome(IEnumerable<WagonIndexData> data, string locationName = "")
        {
            return data.Where(d => d.ToDest == locationName)?.Sum(tr => tr.Brutto) ?? 0;
        }

        private int GetStockpileValue(string locationName, IEnumerable<StocpileIndexData> data)
        {
            var temp = data.Select(d => d.LocationID);
            var localData = data.Where(t => t.LocationID.Equals(locationName));
            if (localData.Count() > 0)
                return (int)localData
                    .OrderBy(l => l.TransferTimeStamp)
                    .Select(f => f.TotalQuantity)
                    .LastOrDefault();

            return 0;
        }
    }
}