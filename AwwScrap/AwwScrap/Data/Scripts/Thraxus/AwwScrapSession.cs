using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AwwScrap.Common.BaseClasses;
using AwwScrap.Common.Enums;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using AwwScrap.Support;
using VRage.Collections;

namespace AwwScrap
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	public class AwwScrapCore : BaseSessionComp
	{
		protected override string CompName { get; } = "AwwScrapCore";
		protected override CompType Type { get; } = CompType.Both;
		protected override MyUpdateOrder Schedule { get; } = MyUpdateOrder.NoUpdate;

		private readonly Dictionary<MyBlueprintClassDefinition, List<string>> _blueprintClassOutputs = new Dictionary<MyBlueprintClassDefinition, List<string>>();
		private readonly Dictionary<MyBlueprintClassDefinition, List<string>> _skitOutputs = new Dictionary<MyBlueprintClassDefinition, List<string>>();
		private readonly CachingDictionary<string, ComponentMap> _componentMaps = new CachingDictionary<string, ComponentMap>();
		private readonly MyPhysicalItemDefinition _genericScrap = MyDefinitionManager.Static.GetPhysicalItemDefinition(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Scrap"));
		private readonly MyBlueprintClassDefinition _awwScrapBlueprintClassDefinition = MyDefinitionManager.Static.GetBlueprintClass("AwwScrap");

		private readonly Dictionary<string, MyPhysicalItemDefinition> _scrapDictionary = new Dictionary<string, MyPhysicalItemDefinition>();

		protected override void EarlySetup()
		{
			base.EarlySetup();
			//Run();
			//SetDeconstructItems();
			//HideBadScrap();
		}

		protected override void LateSetup()
		{
			base.LateSetup();
			Run();
			SetDeconstructItems();
			HideBadScrap();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			foreach (var cm in _componentMaps)
			{
				sb.AppendLine(cm.Value.PrintBasicInformation());
			}
			WriteToLog("LateSetup", sb.ToString(), LogType.General);
		}

		private void Run()
		{
			GrabInformation();
			ScourAssemblers();
			EliminateCompoundComponents();
			ScrubBlacklistedScrapReturns();
			ScourRefineries();
			ScourSkits();
			FindCompatibleBlueprints();
			foreach (var fcm in _componentMaps)
			{
				fcm.Value.RunScrapSetup();
			}
			FindCompatibleSkitBlueprints();
			SetupSkits();
			ApplyBlueprintChanges();
		}

		private void GrabInformation()
		{
			foreach (var def in MyDefinitionManager.Static.GetDefinitionsOfType<MyPhysicalItemDefinition>())
			{
				if (!def.Public) continue;
				if (ValidateScrap(def.Id.SubtypeName))
				{
					_scrapDictionary.Add(def.Id.SubtypeName, def);
					continue;
				}
				if (def.Id.TypeId == typeof(MyObjectBuilder_Component))
				{
					if (Constants.ComponentBlacklist.Contains(def.Id.SubtypeName)) continue;
					var compMap = new ComponentMap();
					compMap.SetComponentDefinition(def);
					_componentMaps.Add(def.Id.SubtypeName, compMap);
					_componentMaps.ApplyChanges();
				}
			}
		}
		
		private void ScourAssemblers()
		{
			foreach (var assembler in MyDefinitionManager.Static.GetDefinitionsOfType<MyAssemblerDefinition>())
			{
				if (!assembler.Public) continue;
				foreach (var bpc in assembler.BlueprintClasses)
				{
					if (!bpc.Public) continue;
					foreach (var bpd in bpc)
					{
						if (!bpd.Public) continue;
						if (bpd.Id.SubtypeName.Contains("/"))
							break;
						if (bpd.Results.Length != 1 || !_componentMaps.ContainsKey(bpd.Results[0].Id.SubtypeName))
							continue;
						_componentMaps[bpd.Results[0].Id.SubtypeName].AddComponentPrerequisites(bpd);
						_componentMaps[bpd.Results[0].Id.SubtypeName].AddBlueprint(bpd);
					}
				}
			}
		}

		private void EliminateCompoundComponents()
		{
			var componentMapQueue = new Queue<ComponentMap>();
			do
			{
				if (componentMapQueue.Count > 0)
				{
					ComponentMap map = componentMapQueue.Dequeue();
					map.ReconcileCompoundComponents(_componentMaps);
					_componentMaps.Add(map.GetComponentDefinition().Id.SubtypeName, map);
					_componentMaps.ApplyAdditionsAndModifications();
				}

				foreach (var cm in _componentMaps)
				{
					foreach (var cpr in cm.Value.ComponentPrerequisites)
					{
						if (!_componentMaps.ContainsKey(cpr.Key)) continue;
						componentMapQueue.Enqueue(cm.Value);
						_componentMaps.Remove(cm.Key);
						break;
					}
				}
				_componentMaps.ApplyRemovals();
			} while (componentMapQueue.Count > 0);
		}

		private void ScrubBlacklistedScrapReturns()
		{
			foreach (var cm in _componentMaps)
			{
				cm.Value.ScrubBlacklistedScrapReturns();
			}
		}

		private void ScourRefineries()
		{
			foreach (var refinery in MyDefinitionManager.Static.GetDefinitionsOfType<MyRefineryDefinition>())
			{
				if (!refinery.Public) continue;
				foreach (var bpc in refinery.BlueprintClasses)
				{
					if (!bpc.Public) continue;
					if(!_blueprintClassOutputs.ContainsKey(bpc))
						_blueprintClassOutputs.Add(bpc, new List<string>());
					foreach (var bpd in bpc)
					{
						if (!bpd.Public) continue;
						foreach (var res in bpd.Results)
						{
							if (_blueprintClassOutputs[bpc].Contains(res.Id.SubtypeName)) continue;
							_blueprintClassOutputs[bpc].Add(res.Id.SubtypeName);
						}
					}
				}
			}
		}

		private void ScourSkits()
		{
			foreach (var skit in MyDefinitionManager.Static.GetDefinitionsOfType<MySurvivalKitDefinition>())
			{
				if (!skit.Public) continue;
				foreach (var bpc in skit.BlueprintClasses)
				{
					if (!bpc.Public) continue;
					if (!_skitOutputs.ContainsKey(bpc))
						_skitOutputs.Add(bpc, new List<string>());
					foreach (var bpd in bpc)
					{
						if (!bpd.Public) continue;
						foreach (var res in bpd.Results)
						{
							if (_skitOutputs[bpc].Contains(res.Id.SubtypeName)) continue;
							_skitOutputs[bpc].Add(res.Id.SubtypeName);
						}
					}
				}
			}
		}

		private void FindCompatibleBlueprints()
		{
			foreach (var bco in _blueprintClassOutputs)
			{
				foreach (var cm in _componentMaps)
				{
					bool compatible = true;
					int maxCompatibility = cm.Value.ComponentPrerequisites.Count;
					int falseHits = 0;
					foreach (var pre in cm.Value.ComponentPrerequisites)
					{
						if (bco.Value.Contains(pre.Key)) continue;
						compatible = false;
						falseHits++;
					}
					if (compatible)
					{
						cm.Value.AddCompatibleRefineryBpc(bco.Key, false);
						continue;
					}
					if (falseHits <= maxCompatibility * 0.5f)
						cm.Value.AddCompatibleRefineryBpc(bco.Key, true);
				}
			}
		}

		private void FindCompatibleSkitBlueprints()
		{
			foreach (var sko in _skitOutputs)
			{
				foreach (var cm in _componentMaps)
				{
					bool compatible = true;
					foreach (var pre in cm.Value.ComponentPrerequisites)
					{
						if (!sko.Value.Contains(pre.Key))
							compatible = false;
					}

					if (!compatible) continue;
					MyBlueprintDefinition scrapDef = cm.Value.GetScrapBlueprint();
					if (scrapDef == null) continue;
					cm.Value.SkitCompatible = true;
					_awwScrapBlueprintClassDefinition.AddBlueprint(scrapDef);
				}
			}
		}

		private void SetupSkits()
		{
			foreach (MyCubeBlockDefinition sKitDef in MyDefinitionManager.Static.GetAllDefinitions().OfType<MyCubeBlockDefinition>().Where(myCubeBlockDefinition => myCubeBlockDefinition is MySurvivalKitDefinition))
			{
				((MySurvivalKitDefinition)sKitDef).BlueprintClasses.Add(_awwScrapBlueprintClassDefinition);
				foreach (var cm in _componentMaps)
				{
					if (!cm.Value.SkitCompatible) continue;
					((MySurvivalKitDefinition)sKitDef).InputInventoryConstraint.Add(cm.Value.GetScrapDefinition().Id);
					((MySurvivalKitDefinition)sKitDef).LoadPostProcess();
				}
			}
		}

		private void ApplyBlueprintChanges()
		{
			foreach (var refinery in MyDefinitionManager.Static.GetDefinitionsOfType<MyRefineryDefinition>())
			{
				refinery.LoadPostProcess();
			}
		}
		
		private static bool ValidateScrap(string compName)
		{
			return compName.EndsWith(Constants.ScrapSuffix, StringComparison.OrdinalIgnoreCase) && !compName.Equals(Constants.ScrapSuffix, StringComparison.OrdinalIgnoreCase);
		}

		private void SetDeconstructItems()
		{
			foreach (MyCubeBlockDefinition def in MyDefinitionManager.Static.GetAllDefinitions()
				.OfType<MyCubeBlockDefinition>()
				.Where(myCubeBlockDefinition => myCubeBlockDefinition?.Components != null))
			{
				if (Constants.IgnoredBlocks.Contains(def.Id.SubtypeName)) continue;
				foreach (var comp in def.Components)
				{
					if (!comp.Definition.Public) continue;
					if (Constants.DoNotScrap.Contains(comp.Definition.Id.SubtypeName)) continue;
					if (_componentMaps.ContainsKey(comp.Definition.Id.SubtypeName))
					{
						comp.DeconstructItem = _componentMaps[comp.Definition.Id.SubtypeName].HasValidScrap()
							? _componentMaps[comp.Definition.Id.SubtypeName].GetScrapDefinition()
							: _genericScrap;
						continue;
					}
					comp.DeconstructItem = _genericScrap;
				}
			}
		}

		private void HideBadScrap()
		{
			foreach (var cm in _componentMaps)
			{
				if (!cm.Value.HasValidScrap()) continue;
				_scrapDictionary.Remove(cm.Value.GetScrapDefinition().Id.SubtypeName);
			}

			foreach (var sd in _scrapDictionary)
			{
				sd.Value.Public = false;
			}
		}
	}
}