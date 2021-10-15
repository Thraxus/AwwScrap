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
			"MetalPlate_GoldCoat"
		};

		public static readonly List<string> IgnoredBlocks = new List<string>
		{
			"StorageShelf1",
			"StorageShelf2",
			"StorageShelf3"
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
