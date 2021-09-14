using Sandbox.ModAPI;

namespace AwwScrap.Support
{
	public static class Constants
	{
		public const string ScrapSuffix = "Scrap";
		public const string ScrapBpSuffix = "ToIngot";
		public const float ScrapScalar = (BaseScrapScalar / 2);
		public const float BaseScrapScalar = 0.9f;
		public static float AssemblerMultiplier = MyAPIGateway.Session.SessionSettings.AssemblerEfficiencyMultiplier;
	}
}
