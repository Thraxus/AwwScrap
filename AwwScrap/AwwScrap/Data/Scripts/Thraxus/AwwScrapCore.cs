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
using VRage.Utils;

namespace AwwScrap
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	public class AwwScrapCore : BaseSessionComp
	{
		protected override string CompName { get; } = "AwwScrapCore";
		protected override CompType Type { get; } = CompType.Both;
		protected override MyUpdateOrder Schedule { get; } = MyUpdateOrder.NoUpdate;

		private readonly Dictionary<MyBlueprintClassDefinition, HashSet<string>> _blueprintClassOutputs = new Dictionary<MyBlueprintClassDefinition, HashSet<string>>();
		private readonly Dictionary<MyBlueprintClassDefinition, HashSet<string>> _skitOutputs = new Dictionary<MyBlueprintClassDefinition, HashSet<string>>();
		private readonly CachingDictionary<string, ComponentMap> _componentMaps = new CachingDictionary<string, ComponentMap>();
		private readonly MyPhysicalItemDefinition _genericScrap = MyDefinitionManager.Static.GetPhysicalItemDefinition(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Scrap"));
		private readonly MyBlueprintClassDefinition _awwScrapSkitBlueprintClassDefinition = MyDefinitionManager.Static.GetBlueprintClass(Constants.AwwScrapSkitClassName);
		private readonly MyBlueprintClassDefinition _awwScrapAllScrapBlueprintClassDefinition = MyDefinitionManager.Static.GetBlueprintClass(Constants.AwwScrapAllScrapClassName);
		private MyRefineryDefinition _awwScrapRefineryDefinition;
		private readonly Dictionary<string, MyPhysicalItemDefinition> _scrapDictionary = new Dictionary<string, MyPhysicalItemDefinition>();
        private readonly HashSet<MyStringHash> _ingots = new HashSet<MyStringHash>();
        private readonly Dictionary<MyStringHash, HashSet<MyStringHash>> _compoundIngots = new Dictionary<MyStringHash, HashSet<MyStringHash>>();

        //private readonly StringBuilder _debugSb = new StringBuilder();

        protected override void SuperEarlySetup()
		{
			base.SuperEarlySetup();
			Run();
			SetDeconstructItems();
			HideBadScrap();
		}

		protected override void LateSetup()
		{
			base.LateSetup();
			var sbValidScrap = new StringBuilder();
            var validScrap = false;
            var sbSkippedScrap = new StringBuilder();
            var skippedScrap = false;
            var sbInvalidScrap = new StringBuilder();
            var sbEasyDefGenerator = new StringBuilder();
            var invalidScrap = false;
			
			sbValidScrap.AppendLine("\n");
            sbValidScrap.AppendFormat("{0,-1}The following valid scrap was created...", " ");
            sbValidScrap.AppendLine("\n");
			
            sbSkippedScrap.AppendFormat("{0,-1}The following valid scrap was intentionally skipped...", " ");
            sbSkippedScrap.AppendLine("\n");
			
            sbInvalidScrap.AppendFormat("{0,-1}The following components did not contain valid scrap...", " ");
            sbInvalidScrap.AppendLine("\n");

            sbEasyDefGenerator.AppendFormat("{0,-1}The following is used by Thraxus for setting up new scrap...", " ");
            sbEasyDefGenerator.AppendLine("\n");

            foreach (var cm in _componentMaps)
            {
                if (cm.Value.HasValidScrap())
                {
                    sbValidScrap.AppendLine(cm.Value.ToString());
                    validScrap = true;
                }
                else if (cm.Value.IntentionallySkipped)
                {
                    sbSkippedScrap.AppendLine(cm.Value.ToString());
                    skippedScrap = true;
                }
				else
                {
                    sbInvalidScrap.AppendLine(cm.Value.ToString());
                    sbEasyDefGenerator.AppendLine(cm.Value.GetEasyDefGeneratorString());
                    invalidScrap = true;
                }
            }

            if (!validScrap) sbValidScrap.AppendLine("  None");
            if (!skippedScrap) sbSkippedScrap.AppendLine("  None");
            if (!invalidScrap)
            {
                sbInvalidScrap.AppendLine("  None");
                sbEasyDefGenerator.Clear();
            };

            sbValidScrap.AppendLine(sbSkippedScrap.ToString());
            sbValidScrap.AppendLine(sbInvalidScrap.ToString());
            sbValidScrap.AppendLine(sbEasyDefGenerator.ToString());
            sbValidScrap.AppendLine();

            WriteGeneral("LateSetup", sbValidScrap.ToString());
			 
            //WriteGeneral("LateSetup", $"\n{_debugSb}");
            //PrintAwwScrapRecyclerStuffs();
        }

		//private void PrintAwwScrapRecyclerStuffs()
		//{
		//	WriteToLog(nameof(LateSetup), $"{_awwScrapRefineryDefinition.BlueprintClasses.Count}", LogType.General);
		//	foreach (var bpc in _awwScrapRefineryDefinition.BlueprintClasses)
		//	{
		//		WriteToLog(nameof(LateSetup), $"", LogType.General);
		//		WriteToLog(nameof(LateSetup), $"{bpc.Id.SubtypeName}", LogType.General);
		//		foreach (var bp in bpc)
		//		{
		//			foreach (var pre in bp.Prerequisites)
		//			{
		//				WriteToLog(nameof(LateSetup), $"[P] [{(float)pre.Amount:00.00}] {pre.Id.SubtypeName}", LogType.General);
		//			}
		//			foreach (var res in bp.Results)
		//			{
		//				WriteToLog(nameof(LateSetup), $"[R] [{(float)res.Amount:00.00}] {res.Id.SubtypeName}", LogType.General);
		//			}
		//		}
		//	}
		//}

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
			SetupAwwScrapRecycler();
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
					var compMap = new ComponentMap(ModContext.ModPath);
					compMap.SetComponentDefinition(def);
					_componentMaps.Add(def.Id.SubtypeName, compMap);
					_componentMaps.ApplyChanges();
				}

                if (def.IsIngot)
                {
                    _ingots.Add(def.Id.SubtypeId);
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
                        CheckBpdAgainstIngots(bpd);
						if (bpd.Results.Length != 1 || !_componentMaps.ContainsKey(bpd.Results[0].Id.SubtypeName))
							continue;
						_componentMaps[bpd.Results[0].Id.SubtypeName].AddBlueprint(bpd);
					}
				}
			}
		}

        private void CheckBpdAgainstIngots(MyBlueprintDefinitionBase bpd)
        {
            foreach (var result in bpd.Results)
            {
                if (!_ingots.Contains(result.Id.SubtypeId)) continue;
				if (!_compoundIngots.ContainsKey(result.Id.SubtypeId))
					_compoundIngots.Add(result.Id.SubtypeId, new HashSet<MyStringHash>());
                foreach (var pre in bpd.Prerequisites)
                {
                    _compoundIngots[result.Id.SubtypeId].Add(pre.Id.SubtypeId);
                }
            }
        }

        private void EliminateCompoundComponents()
		{
			var componentMapQueue = new Queue<ComponentMap>();
            const int breakAfter = 300;
            var currentLoop = 0;
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
                currentLoop++;
            } while (componentMapQueue.Count > 0 && currentLoop < breakAfter );

            if (componentMapQueue.Count <= 0) return;

            var rpt = new StringBuilder();
            rpt.AppendLine();
            rpt.AppendLine();
            rpt.AppendFormat("{0, -2}Error parsing all components.  The Component Map Queue still contained the following {1} item(s).", " ", componentMapQueue.Count);
            rpt.AppendLine();
            rpt.AppendLine();
			foreach (var cmq in componentMapQueue)
                rpt.AppendLine(cmq.ToString());
            WriteGeneral(nameof(EliminateCompoundComponents), rpt.ToString());
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
				if (refinery.Id.SubtypeName == Constants.AwwScrapRecyclerSubtypeName)
				{
					_awwScrapRefineryDefinition = refinery;
                    WriteGeneral(nameof(ScourRefineries), "Found the _awwScrapRefineryDefinition ...");
					continue;
				}
				foreach (var bpc in refinery.BlueprintClasses)
				{
					if (!bpc.Public) continue;
					if(!_blueprintClassOutputs.ContainsKey(bpc))
						_blueprintClassOutputs.Add(bpc, new HashSet<string>());
					foreach (var bpd in bpc)
					{
						if (!bpd.Public) continue;
						foreach (var res in bpd.Results)
						{
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
						_skitOutputs.Add(bpc, new HashSet<string>());
					foreach (var bpd in bpc)
					{
						if (!bpd.Public) continue;
						foreach (var res in bpd.Results)
						{
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
				if (Constants.IgnoredBlueprintClasses.Contains(bco.Key.Id.SubtypeName)) continue;
				foreach (var cm in _componentMaps)
				{
                    bool compatible = true;
					int maxCompatibility = cm.Value.ComponentPrerequisites.Count;
					int falseHits = 0;
                    foreach (var pre in cm.Value.ComponentPrerequisites)
					{
                        if (bco.Value.Contains(pre.Key)) continue;
                        if (CheckCompoundIngots(bco.Value, pre.Key)) continue;
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

        private bool CheckCompoundIngots(ICollection<string> bcoValue, string preKey)
        {
            MyStringHash check = MyStringHash.GetOrCompute(preKey);
			if (!_compoundIngots.ContainsKey(check)) return false;
			foreach (var results in _compoundIngots[check])
            {
                if (bcoValue.Contains(results.ToString())) continue;
                return false;
            }
            return true;
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
					_awwScrapSkitBlueprintClassDefinition.AddBlueprint(scrapDef);
				}
			}
		}

		private void SetupSkits()
		{
			foreach (MyCubeBlockDefinition sKitDef in MyDefinitionManager.Static.GetAllDefinitions().OfType<MyCubeBlockDefinition>().Where(myCubeBlockDefinition => myCubeBlockDefinition is MySurvivalKitDefinition))
			{
				((MySurvivalKitDefinition)sKitDef).BlueprintClasses.Add(_awwScrapSkitBlueprintClassDefinition);
				foreach (var cm in _componentMaps)
				{
					if (!cm.Value.SkitCompatible) continue;
					((MySurvivalKitDefinition)sKitDef).InputInventoryConstraint.Add(cm.Value.GetScrapDefinition().Id);
					((MySurvivalKitDefinition)sKitDef).LoadPostProcess();
				}
			}
		}

		private void SetupAwwScrapRecycler()
		{
			if (_awwScrapRefineryDefinition == null) return;
			foreach (var cm in _componentMaps)
			{
				if (!cm.Value.HasValidScrap()) continue;
				_awwScrapAllScrapBlueprintClassDefinition.AddBlueprint(cm.Value.GetScrapBlueprint());
                WriteGeneral(nameof(ScourRefineries), $"Added: {cm.Value.GetScrapBlueprint().Id.SubtypeName}");
			}
			_awwScrapRefineryDefinition.BlueprintClasses.Clear();
			_awwScrapRefineryDefinition.BlueprintClasses.Add(_awwScrapAllScrapBlueprintClassDefinition);
			_awwScrapRefineryDefinition.LoadPostProcess();
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