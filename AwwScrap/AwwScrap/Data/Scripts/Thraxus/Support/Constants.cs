﻿using System.Collections.Generic;
using Sandbox.ModAPI;

namespace AwwScrap.Support
{
	public static class Constants
	{
		public const string ScrapSuffix = "Scrap";
		public const string ScrapBpSuffix = "ToIngot";
		public const float ScrapScalar = (BaseAwwScrapScalar / 2);
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
			"Tech8x"
		};

		public static readonly List<string> IgnoredBlocks = new List<string>
		{
			"StorageShelf1",
			"StorageShelf2",
			"StorageShelf3"
		};
	}
}
