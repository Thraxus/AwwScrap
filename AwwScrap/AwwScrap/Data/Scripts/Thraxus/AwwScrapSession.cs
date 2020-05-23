using System;
using System.Linq;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using AwwScrap.Support;
using Sandbox.Common.ObjectBuilders;
using VRage.Utils;

namespace AwwScrap
{
	[MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
	public class AwwScrapSession : MySessionComponentBase
	{
		public override void LoadData()
		{
			base.LoadData();
			SetSurvivalKitMenu();
		}

		public override void BeforeStart()
		{
			base.BeforeStart();
			Initialize();
		}
		
		private static void Initialize()
		{
			MyAPIGateway.Parallel.StartBackground(ScrubCubes);
			MyAPIGateway.Parallel.StartBackground(SetEfficiency);
			MyAPIGateway.Parallel.StartBackground(SetAttributes);
		}

		private static void ScrubCubes()
		{
			try
			{
				MyPhysicalItemDefinition scrapDef = MyDefinitionManager.Static.GetPhysicalItemDefinition(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Scrap"));
			
				foreach (MyCubeBlockDefinition myCubeBlockDefinition in MyDefinitionManager.Static.GetAllDefinitions().OfType<MyCubeBlockDefinition>().Where(myCubeBlockDefinition => myCubeBlockDefinition?.Components != null))
				{
					foreach (MyCubeBlockDefinition.Component component in myCubeBlockDefinition.Components)
					{
						if (!component.Definition.Public)
							continue;

						string subtypeName;
						if (Statics.ComponentDictionary.TryGetValue(component.Definition.Id.SubtypeId, out subtypeName))
						{
							component.DeconstructItem = MyDefinitionManager.Static.GetPhysicalItemDefinition(new MyDefinitionId(typeof(MyObjectBuilder_Ore), subtypeName));
							continue;
						}

						if (Statics.SkipTieredTech)
							if (component.Definition.Id.SubtypeId.ToString() == "Tech2x" || component.Definition.Id.SubtypeId.ToString() == "Tech4x" || component.Definition.Id.SubtypeId.ToString() == "Tech8x")
								continue;
						component.DeconstructItem = scrapDef;
					}
				}
			}
			catch (Exception e)
			{
				MyLog.Default.WriteLine($"AwwScrap: ScrubCubes - Boom!!! {e}");
			}
		}

		private static void SetAttributes()
		{
			try
			{
				foreach (MyPhysicalItemDefinition item in MyDefinitionManager.Static.GetPhysicalItemDefinitions())
				{
					ScrapAttributes scrap;
					if (!Statics.ScrapAttributesDictionary.TryGetValue(item.Id.SubtypeId, out scrap))
						continue;
					item.Mass = scrap.Mass;
					item.Volume = scrap.Volume / 1000;
				}
			}
			catch (Exception e)
			{
				MyLog.Default.WriteLine($"AwwScrap: SetAttributes - Boom!!! {e}");
			}
		}

		private static void SetEfficiency()
		{
			try
			{
				// This loop accounts for World Settings for the Assembler Efficiency Modifier (x1, x3, x10)
				foreach (MyBlueprintDefinitionBase myBlueprintDefinitionBase in Statics.AwwScrapSubTypeIds.Select(
					subtype => MyDefinitionManager.Static.GetBlueprintDefinition(
						new MyDefinitionId(typeof(MyObjectBuilder_BlueprintDefinition), subtype))))
				{
					for (int index = 0; index < myBlueprintDefinitionBase.Results.Length; index++)
					{   // MyFixedPoint can't do /= operations, so have to do a work around
						float f = (float)myBlueprintDefinitionBase.Results[index].Amount;
						f /= MyAPIGateway.Session.SessionSettings.AssemblerEfficiencyMultiplier;
						myBlueprintDefinitionBase.Results[index].Amount = (MyFixedPoint)f;
					}
				}
				MyRefineryDefinition basicRefinery = MyDefinitionManager.Static.GetCubeBlockDefinition(new MyDefinitionId(typeof(MyObjectBuilder_Refinery), "Blast Furnace")) as MyRefineryDefinition;
				if (basicRefinery != null) basicRefinery.MaterialEfficiency = 1.0f;
			}
			catch (Exception e)
			{
				MyLog.Default.WriteLine($"AwwScrap: SetEfficiency - Boom!!! {e}");
			}
		}

		private static void SetSurvivalKitMenu()
		{
			try
			{
				MyBlueprintClassDefinition awwScrap = MyDefinitionManager.Static.GetBlueprintClass("AwwScrap");
				foreach (MyCubeBlockDefinition sKitDef in MyDefinitionManager.Static.GetAllDefinitions().OfType<MyCubeBlockDefinition>().Where(myCubeBlockDefinition => myCubeBlockDefinition is MySurvivalKitDefinition))
				{
					((MySurvivalKitDefinition)sKitDef).BlueprintClasses.Add(awwScrap);
					foreach (string x in Statics.SKitScrapList)
					{
						((MySurvivalKitDefinition)sKitDef).InputInventoryConstraint.Add(new MyDefinitionId(typeof(MyObjectBuilder_Ore), x));
					}
				}
			}
			catch (Exception e)
			{
				MyLog.Default.WriteLine($"AwwScrap: SetSurvivalKitMenu - Boom!!! {e}");
			}
		}
	}
}