using AwwScrap.Common.BaseClasses;
using AwwScrap.Common.Enums;
using AwwScrap.Common.Factions.Models;
using AwwScrap.Common.Reporting;
using VRage.Game.Components;

namespace AwwScrap.Common
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, priority: int.MinValue + 1)]
	public class CommonCore : BaseSessionComp
	{
		protected override string CompName { get; } = "CommonCore";
		protected override CompType Type { get; } = CompType.Server;
		protected override MyUpdateOrder Schedule { get; } = MyUpdateOrder.NoUpdate;

		protected override void SuperEarlySetup()
		{
			base.SuperEarlySetup();
		}

		protected override void LateSetup()
		{
			base.LateSetup();
			FactionDictionaries.Initialize();
			WriteToLog($"{CompName} - Basic Game Information", $"{BasicGameInformation.Report()}", LogType.General);
			WriteToLog($"{CompName} - Factions", $"{FactionDictionaries.Report()}", LogType.General);
		}
	}
}
