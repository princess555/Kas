namespace CTS_Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCompanyMigration : DbMigration
    {
        public override void Up()
        {
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WarehouseTransfers", "WarehouseID", "dbo.Warehouses");
            DropForeignKey("dbo.WarehouseMeasures", "WarehouseID", "dbo.Warehouses");
            DropForeignKey("dbo.Warehouses", "LocationID", "dbo.Locations");
            DropForeignKey("dbo.WagonTransfers", "ItemID", "dbo.Items");
            DropForeignKey("dbo.WagonTransfers", "FromDestID", "dbo.Locations");
            DropForeignKey("dbo.WagonTransfers", "WagonScaleID", "dbo.WagonScales");
            DropForeignKey("dbo.WagonTransfers", "WagonAnalysisID", "dbo.WagonAnalysis");
            DropForeignKey("dbo.WagonScales", "LocationID", "dbo.Locations");
            DropForeignKey("dbo.WagonNumsCaches", "RecognID", "dbo.Recogns");
            DropForeignKey("dbo.VehiTransfers", "ItemID", "dbo.Items");
            DropForeignKey("dbo.VehiTransfers", "FromDestID", "dbo.Locations");
            DropForeignKey("dbo.VehiTransfers", "VehiScaleID", "dbo.VehiScales");
            DropForeignKey("dbo.VehiScales", "LocationID", "dbo.Locations");
            DropForeignKey("dbo.SkipWeights", "SkipID", "dbo.Skips");
            DropForeignKey("dbo.SkipTransfers", "SkipID", "dbo.Skips");
            DropForeignKey("dbo.SkipTransfers", "SkipAnalysisID", "dbo.SkipAnalysis");
            DropForeignKey("dbo.Skips", "LocationID", "dbo.Locations");
            DropForeignKey("dbo.Shifts", "LocationID", "dbo.Locations");
            DropForeignKey("dbo.RockUtilTransfers", "RockUtilID", "dbo.RockUtils");
            DropForeignKey("dbo.RockUtils", "LocationID", "dbo.Locations");
            DropForeignKey("dbo.Recogns", "LocationID", "dbo.Locations");
            DropForeignKey("dbo.MiningAnalysis", "LocationID", "dbo.Locations");
            DropForeignKey("dbo.BeltTransfers", "ItemID", "dbo.Items");
            DropForeignKey("dbo.Items", "LocationID", "dbo.Locations");
            DropForeignKey("dbo.BeltTransfers", "BeltScaleID", "dbo.BeltScales");
            DropForeignKey("dbo.BeltTransfers", "BeltAnalysisID", "dbo.BeltAnalysis");
            DropForeignKey("dbo.CtsUserCtsRoles1", "CtsRole_RoleName", "dbo.CtsRoles");
            DropForeignKey("dbo.CtsUserCtsRoles1", new[] { "CtsUser_Login", "CtsUser_Domain" }, "dbo.CtsUsers");
            DropForeignKey("dbo.BoilerConsNorms", "ID", "dbo.Locations");
            DropForeignKey("dbo.BeltScales", "ToInnerDestID", "dbo.InnerDestinations");
            DropForeignKey("dbo.BeltScales", "LocationID", "dbo.Locations");
            DropForeignKey("dbo.BeltScales", "FromInnerDestID", "dbo.InnerDestinations");
            DropForeignKey("dbo.InnerDestinations", "LocationID", "dbo.Locations");
            DropIndex("dbo.CtsUserCtsRoles1", new[] { "CtsRole_RoleName" });
            DropIndex("dbo.CtsUserCtsRoles1", new[] { "CtsUser_Login", "CtsUser_Domain" });
            DropIndex("dbo.WarehouseTransfers", new[] { "WarehouseID" });
            DropIndex("dbo.Warehouses", new[] { "LocationID" });
            DropIndex("dbo.WarehouseMeasures", new[] { "WarehouseID" });
            DropIndex("dbo.WagonTransfers", new[] { "ItemID" });
            DropIndex("dbo.WagonTransfers", new[] { "FromDestID" });
            DropIndex("dbo.WagonTransfers", new[] { "WagonAnalysisID" });
            DropIndex("dbo.WagonTransfers", new[] { "WagonScaleID" });
            DropIndex("dbo.WagonScales", new[] { "LocationID" });
            DropIndex("dbo.WagonNumsCaches", new[] { "RecognID" });
            DropIndex("dbo.VehiTransfers", new[] { "ItemID" });
            DropIndex("dbo.VehiTransfers", new[] { "FromDestID" });
            DropIndex("dbo.VehiTransfers", new[] { "VehiScaleID" });
            DropIndex("dbo.VehiScales", new[] { "LocationID" });
            DropIndex("dbo.SkipWeights", new[] { "SkipID" });
            DropIndex("dbo.SkipTransfers", new[] { "SkipAnalysisID" });
            DropIndex("dbo.SkipTransfers", new[] { "SkipID" });
            DropIndex("dbo.Skips", new[] { "LocationID" });
            DropIndex("dbo.Shifts", new[] { "LocationID" });
            DropIndex("dbo.RockUtilTransfers", new[] { "RockUtilID" });
            DropIndex("dbo.RockUtils", new[] { "LocationID" });
            DropIndex("dbo.Recogns", new[] { "LocationID" });
            DropIndex("dbo.MiningAnalysis", new[] { "LocationID" });
            DropIndex("dbo.Items", new[] { "LocationID" });
            DropIndex("dbo.BeltTransfers", new[] { "BeltAnalysisID" });
            DropIndex("dbo.BeltTransfers", new[] { "ItemID" });
            DropIndex("dbo.BeltTransfers", new[] { "BeltScaleID" });
            DropIndex("dbo.BoilerConsNorms", new[] { "ID" });
            DropIndex("dbo.InnerDestinations", new[] { "LocationID" });
            DropIndex("dbo.BeltScales", new[] { "ToInnerDestID" });
            DropIndex("dbo.BeltScales", new[] { "FromInnerDestID" });
            DropIndex("dbo.BeltScales", new[] { "LocationID" });
            DropTable("dbo.CtsUserCtsRoles1");
            DropTable("dbo.WarehouseTransfers");
            DropTable("dbo.Warehouses");
            DropTable("dbo.WarehouseMeasures");
            DropTable("dbo.WagonTransfers");
            DropTable("dbo.WagonScales");
            DropTable("dbo.WagonNumsCaches");
            DropTable("dbo.WagonAnalysis");
            DropTable("dbo.VehiTransfers");
            DropTable("dbo.VehiScales");
            DropTable("dbo.SkipWeights");
            DropTable("dbo.SkipTransfers");
            DropTable("dbo.Skips");
            DropTable("dbo.SkipAnalysis");
            DropTable("dbo.Shifts");
            DropTable("dbo.RockUtilTransfers");
            DropTable("dbo.RockUtils");
            DropTable("dbo.Recogns");
            DropTable("dbo.RailWeighbridges");
            DropTable("dbo.OraclePlanB");
            DropTable("dbo.OracleStaff");
            DropTable("dbo.OrcalePlanWithShop_ID");
            DropTable("dbo.MiningAnalysis");
            DropTable("dbo.LocalStaff");
            DropTable("dbo.LocalPlanWithLocationID");
            DropTable("dbo.LocalPlanBWithLocationID");
            DropTable("dbo.LocalPlanB");
            DropTable("dbo.LocalPlan");
            DropTable("dbo.Items");
            DropTable("dbo.BeltTransfers");
            DropTable("dbo.CtsUserCtsRoles");
            DropTable("dbo.CtsUsers");
            DropTable("dbo.CtsRoles");
            DropTable("dbo.BoilerConsNormNews");
            DropTable("dbo.BoilerConsNorms");
            DropTable("dbo.Locations");
            DropTable("dbo.InnerDestinations");
            DropTable("dbo.BeltScales");
            DropTable("dbo.BeltAnalysis");
            DropTable("dbo.AlarmComments");
        }
    }
}
