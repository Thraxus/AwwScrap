using System.Collections.Generic;
using Sandbox.ModAPI;

namespace AwwScrap.Support
{
	public static class Constants
	{
		public const string ScrapSuffix = "Scrap";
		public const string ScrapBpSuffix = "ToIngot";
		public const string AwwScrapSkitClassName = "AwwScrapSkit";
		public const string AwwScrapAllScrapClassName = "AwwScrapRecycling";
		public const string AwwScrapRecyclerSubtypeName = "AwwScrapRecycler";
		public const float ScrapScalar = (BaseAwwScrapScalar / 2);
		public const float ScrapMassScalar = 0.80f;
		public const float ScrapVolumeScalar = 0.70f;
		public const float ScrapProductionTimeScalar = 0.75f;
		public const float BaseAwwScrapScalar = 0.9f;
		public static readonly float AssemblerMultiplier = MyAPIGateway.Session.SessionSettings.AssemblerEfficiencyMultiplier;

		public static readonly List<string> ScrapReturnsBlacklist = new List<string>
		{
			"Ice"
		};

		public static readonly List<string> ComponentBlacklist = new List<string>
		{
			"ZoneChip"
		};

		public static readonly List<string> DoNotScrap = new List<string>
		{
			"Tech2x",
			"Tech4x",
			"Tech8x",
			"MotorT6",
			"MotorT7",
			"ThrustT6",
			"ThrustT7",
			"MetalPlate_CandyCane",
			"MetalPlate_GoldCoat",
			"WoodPlank",
			"WoodPlanks",
			"WoodLogs",
			"MESThrust",
			"ProprietaryTech",
			"InhibitorChip",
			"EEMPilotSoul",
			"AdminPlate",
			"EngineerPlushie",
            "SabiroidPlushie",
            "BrokenAIDecisionNode",
            "BrokenAIProcessorNode",
            "EmptyAIDecisionNode",
            "EmptyAIProcessorNode",
            "BrokenChip",
            "BrokenEliteChip",
            "BrokenEnhancedChip",
            "BrokenProficientChip",
            "BrokenComputer",
            "BrokenEliteComputer",
            "BrokenEnhancedComputer",
            "BrokenProficientComputer",
            "ConcreteRubble",
            "ConcreteSack",
            "BrokenCapacitor",
            "BrokenDisplay",
            "BrokenEliteSuperCapacitor",
            "BrokenEnhancedSuperCapacitor",
            "BrokenProficientSuperCapacitor",
            "BrokenSuperCapacitor",
            "BrokenTransistor",
            "BrokenEliteHighPressureCompressor",
            "BrokenEnhancedHighPressureCompressor",
            "BrokenHighPressureCompressor",
            "BrokenPressureRegulator",
            "BrokenProficientHighPressureCompressor",
            "BrokenEliteMotor",
            "BrokenEnhancedMotor",
            "BrokenMotor",
            "BrokenProficientMotor",
            "BrokenDetector",
            "BrokenGravityGenerator",
            "BrokenPlasmaGenerator",
            "BrokenRadioCommunication",
            "BrokenWaterTank",
            "BrokenAluminiumMagnesiumPlate",
            "BrokenBerylliumSteelPlate",
            "BrokenBrassPlate",
            "BrokenBulletproofGlass",
            "BrokenCarbonFiber",
            "BrokenCobaltCeramicPlate",
            "BrokenInteriorPlate",
            "BrokenIronPlate",
            "BrokenMetalGrid",
            "BrokenPlatinumIridiumPlate",
            "BrokenSteelPlate",
            "BrokenTitaniumSteelPlate",
            "BrokenTungstenSteelPlate",
            "CobaltCeramicPlate",
            "BrokenLithiumPowerCell",
            "BrokenPowerCell",
            "BrokenSmallLithiumPowerCell",
            "BrokenSmallPowerCell",
            "EmptyLithiumPowerCell",
            "EmptyPowerCell",
            "EmptySmallLithiumPowerCell",
            "EmptySmallPowerCell",
            "BrokenPlutoniumReactor",
            "BrokenPlutoniumUraniumReactor",
            "BrokenReactor",
            "BrokenGirder",
            "BrokenLargeTube",
            "BrokenSmallTube",
            "BrokenEliteSolarCell",
            "BrokenEnhancedSolarCell",
            "BrokenProficientSolarCell",
            "BrokenSolarCell",
            "BrokenStoneBrick",
            "BrokenSpinner",
            "BrokenThrust",
            "RubberRubble",
            "BrokenWoodGirder",
            "BrokenWoodPlanks",
            "WoodGirder",
        };

		public static readonly List<string> IgnoredBlocks = new List<string>
		{
			"StorageShelf1",
			"StorageShelf2",
			"StorageShelf3",
            "EngineerPlushie",
            "SabiroidPlushie",
            "StorageShelf1_3223_USGC",
            "StorageShelf2_3223_USGC",
            "StorageShelf3_3223_USGC"
        };

		public static readonly List<string> IgnoredBlueprintClasses = new List<string>
		{
			"IngotsTitanium_LWTS",
			"LWTSX_Mythium",
			"LWTSX_Legendarium",
			"LWTSX_Rarium",
			"LWTSX_Lead"
		};
	}
}
