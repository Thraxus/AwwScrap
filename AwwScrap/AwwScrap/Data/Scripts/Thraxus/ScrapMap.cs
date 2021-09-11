using Sandbox.Definitions;

namespace AwwScrap
{
	public class ScrapMap
	{
		public MyPhysicalItemDefinition ScrapDef;
		public MyPhysicalItemDefinition CompDef;

		public override string ToString()
		{
			return $"ScrapMap: {ScrapDef.Id.SubtypeName} | {CompDef.Id.SubtypeName}";
		}
	}
}