using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AwwScrap.Common.BaseClasses;
using AwwScrap.Common.Enums;
using Sandbox.Definitions;
using Sandbox.ModAPI;
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
		protected override CompType Type { get; } = CompType.Server;
		protected override MyUpdateOrder Schedule { get; } = MyUpdateOrder.NoUpdate;

		private readonly Dictionary<MyBlueprintClassDefinition, List<string>> _blueprintClassOutputs = new Dictionary<MyBlueprintClassDefinition, List<string>>();
		private readonly Dictionary<string, MyBlueprintDefinitionBase> _assemblerBlueprints = new Dictionary<string, MyBlueprintDefinitionBase>();
		private readonly Dictionary<string, MyBlueprintDefinitionBase> _refineryBlueprints = new Dictionary<string, MyBlueprintDefinitionBase>();
		private readonly CachingDictionary<string, ComponentMap> _componentMaps = new CachingDictionary<string, ComponentMap>();
		private readonly StringBuilder _report = new StringBuilder();
		MyPhysicalItemDefinition _genericScrap = MyDefinitionManager.Static.GetPhysicalItemDefinition(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Scrap"));

		protected override void EarlySetup()
		{
			//Run();
			base.EarlySetup();
		}
		
		public override void BeforeStart()
		{
			base.BeforeStart();
			Run();
			SetDeconstructItems();
			//Initialize();
		}

		protected override void LateSetup()
		{
			base.LateSetup();

			PrintFinalComponentMaps();
			//PrintRefineryBlueprints();
			//ValidateRefineries();
			//PrintBlueprintClasses();
			//PrintScrapDefManagerOutput();
			//PrintPrerequisiteDefinitions();
			//PrintBlueprints(_assemblerBlueprints);
			//PrintBlueprints(_refineryBlueprints);
			PrintProductionTimes();
		}
		
		private void Run()
		{
			GrabInformation();
			PrintLists();
			//PrintAssemblerBlueprints();
			ScourAssemblers();
			//GetBlueprints(_assemblerBlueprints);
			//PrintPreComponentMapsSimple();
			EliminateCompoundComponents();
			ScrubBlacklistedScrapReturns();
			//PrintRefineryBlueprints();
			ScourRefineries();
			//GetBlueprints(_refineryBlueprints);
			//PrintBlueprintClassOutputs();
			FindCompatibleBlueprints();
			//PrintFinalComponentMaps();
			//BuildScrapDictionary();
			//BuildCompDictionary();
			//PopulateComponentPrerequisites();
			//GetUniqueIngotList();
			//PrintProductionBlockDefinitions();
			foreach (var fcm in _componentMaps)
			{
				fcm.Value.RunScrapSetup();
			}

			//SetBlueprintClasses();

			ApplyBlueprintChanges();


			//PrintBlueprintClassOutputs();

			//Test();
		}

		private readonly Dictionary<string, MyPhysicalItemDefinition> _oreDictionary = new Dictionary<string, MyPhysicalItemDefinition>();
		private readonly Dictionary<string, MyPhysicalItemDefinition> _ingotDictionary = new Dictionary<string, MyPhysicalItemDefinition>();
		private readonly Dictionary<string, MyPhysicalItemDefinition> _scrapDictionary = new Dictionary<string, MyPhysicalItemDefinition>();
		private readonly Dictionary<string, MyPhysicalItemDefinition> _componentDictionary = new Dictionary<string, MyPhysicalItemDefinition>();
		private readonly Dictionary<string, MyPhysicalItemDefinition> _remainingDictionary = new Dictionary<string, MyPhysicalItemDefinition>();

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

				if (def.IsIngot)
				{
					_ingotDictionary.Add(def.Id.SubtypeName, def);
					continue;
				}

				if (def.IsOre)
				{
					_oreDictionary.Add(def.Id.SubtypeName, def);
					continue;
				}

				if(def.Id.TypeId == typeof(MyObjectBuilder_Component))
				{
					if (Constants.ComponentBlacklist.Contains(def.Id.SubtypeName)) continue;
					_componentDictionary.Add(def.Id.SubtypeName, def);
					var compMap = new ComponentMap();
					compMap.SetComponentDefinition(def);
					_componentMaps.Add(def.Id.SubtypeName, compMap);
					_componentMaps.ApplyChanges();
					continue;
				}
				_remainingDictionary.Add(def.Id.SubtypeName, def);
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

		private void PrintProductionTimes()
		{
			foreach (var map in _componentMaps)
			{
				WriteToLog("PPT", $"[{map.Value.GetProductionTime():00.00}] [{map.Value.GetAmountProduced():00.00}] {map.Key}", LogType.General);
			}
		}
		
		private void EliminateCompoundComponents()
		{
			try
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
			catch (Exception e)
			{
				WriteToLog("EliminateCompoundComponents", $"Shit broke..... \n{e}", LogType.General);
			}
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

		private void FindCompatibleBlueprints()
		{
			foreach (var bco in _blueprintClassOutputs)
			{
				foreach (var cm in _componentMaps)
				{
					bool compatible = true;
					foreach (var pre in cm.Value.ComponentPrerequisites)
					{
						if (!bco.Value.Contains(pre.Key))
							compatible = false;
					}
					if (compatible)
						cm.Value.AddCompatibleRefineryBpc(bco.Key);
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
		
		private static void Initialize()
		{
			//MyAPIGateway.Parallel.StartBackground(ScrubCubes);
			//MyAPIGateway.Parallel.StartBackground(SetEfficiency);
			//MyAPIGateway.Parallel.StartBackground(SetAttributes);
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

		private static void ScrubCubes()
		{
			try
			{
				MyPhysicalItemDefinition scrapDef = MyDefinitionManager.Static.GetPhysicalItemDefinition(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Scrap"));
			
				foreach (MyCubeBlockDefinition myCubeBlockDefinition in MyDefinitionManager.Static.GetAllDefinitions().OfType<MyCubeBlockDefinition>().Where(myCubeBlockDefinition => myCubeBlockDefinition?.Components != null))
				{
					if (Statics.IgnoredBlocks.Contains(myCubeBlockDefinition.Id.SubtypeId)) continue;

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
		
		#region Debug Outputs

		private void PrintBlueprintClassOutputs()
		{
			_report.Clear();
			_report.AppendLine();
			_report.AppendLine();
			_report.AppendFormat("{0,-4}[{1}] BlueprintClassOutputs", " ", _blueprintClassOutputs.Count);
			_report.AppendLine();
			_report.AppendLine();

			foreach (var bco in _blueprintClassOutputs)
			{
				_report.AppendFormat("{0,-6}[{1}] BlueprintClass: {2}", " ", bco.Value.Count, bco.Key.Id.SubtypeName);
				_report.AppendLine();
				_report.AppendFormat("{0,-8}", " ");
				foreach (var str in bco.Value)
				{
					_report.AppendFormat("[{0}] ", str);
				}
				_report.AppendLine();
			}
			WriteToLog("PCO", _report.ToString(), LogType.General);
		}

		private void PrintRefineryBlueprints()
		{
			_report.Clear();
			_report.AppendLine();
			_report.AppendLine();
			_report.AppendLine("------------------------------ Refineries ------------------------------");
			_report.AppendLine();
			_report.AppendLine();

			foreach (var refinery in MyDefinitionManager.Static.GetDefinitionsOfType<MyRefineryDefinition>())
			{
				if (!refinery.Public) continue;
				_report.AppendFormat("{0,-2}Refinery: {1}", " ", refinery.Id.SubtypeId);
				_report.AppendLine();
				_report.AppendLine();
				foreach (var bpc in refinery.BlueprintClasses)
				{
					if (!bpc.Public) continue;
					_report.AppendFormat("{0,-4}BPC Subtype: {1}", " ", bpc.Id.SubtypeName);
					_report.AppendLine();
					foreach (var bpd in bpc)
					{
						if (!bpd.Public) continue;
						_report.AppendFormat("{0,-6}BPD Subtype: {1}", " ", bpd.Id.SubtypeName);
						_report.AppendLine();
						_report.AppendFormat("{0,-8}[P]", " ");
						foreach (var pre in bpd.Prerequisites)
						{
							_report.AppendFormat(" [{1:00.00}] {2}", " ", (float)pre.Amount, pre.Id.SubtypeName);
						}
						_report.AppendLine();
						_report.AppendFormat("{0,-8}[R]", " ");
						foreach (var res in bpd.Results)
						{
							_report.AppendFormat(" [{1:00.00}] {2}", " ", (float)res.Amount, res.Id.SubtypeName);
						}
						//if(bpd.Results.Length == 1 && _preComponentMaps.ContainsKey(bpd.Results[0].Id.SubtypeName))
						//	_preComponentMaps[bpd.Results[0].Id.SubtypeName].AddComponentPrerequisites(bpd);
						_report.AppendLine();
					}
					_report.AppendLine();
				}
				_report.AppendLine();
			}
			_report.AppendLine();
			_report.AppendLine("--------------------------- End Refineries -----------------------------");
			_report.AppendLine();

			WriteToLog("Assemblers", _report.ToString(), LogType.General);
		}

		private void PrintBlueprintClasses()
		{
			_report.Clear();
			_report.AppendLine();
			_report.AppendLine();
			_report.AppendFormat("{0,-2}[{1}] Items in Collection", " ", _blueprintClassOutputs.Count);
			_report.AppendLine();
			foreach (var bpc in _blueprintClassOutputs)
			{
				_report.AppendFormat("{0,-4}Blueprint Class: {1}", " ", bpc.Key.Id.SubtypeName);
				_report.AppendLine();
				foreach (var bpd in bpc.Key)
				{
					_report.AppendFormat("{0,-6}Blueprint Definition: {1}", " ", bpd.Id.SubtypeName);
					_report.AppendLine();
					_report.AppendFormat("{0,-8}[P]", " ");
					foreach (var pre in bpd.Prerequisites)
					{
						_report.AppendFormat("[{0:00.00}] <{1} {2}> ", (float)pre.Amount, pre.Id.SubtypeName, pre.Id.TypeId);
					}
					_report.AppendLine();
					_report.AppendFormat("{0,-8}[R]", " ");
					foreach (var res in bpd.Results)
					{
						_report.AppendFormat("[{0:00.00}] <{1} {2}> ", (float)res.Amount, res.Id.SubtypeName, res.Id.TypeId);
					}
					_report.AppendLine();
				}
				_report.AppendLine();
			}
			_report.AppendLine();
			_report.AppendLine();
			WriteToLog("BPC", _report.ToString(), LogType.General);
		}

		private void PrintAssemblerBlueprints()
		{
			_report.Clear();
			_report.AppendLine();
			_report.AppendLine();
			_report.AppendLine("------------------------------ Assemblers ------------------------------");
			_report.AppendLine();
			_report.AppendLine();

			foreach (var assembler in MyDefinitionManager.Static.GetDefinitionsOfType<MyAssemblerDefinition>())
			{
				if (!assembler.Public) continue;
				_report.AppendFormat("{0,-2}Assembler: {1}", " ", assembler.Id.SubtypeId);
				_report.AppendLine();
				_report.AppendLine();
				foreach (var bpc in assembler.BlueprintClasses)
				{
					if (!bpc.Public) continue;
					_report.AppendFormat("{0,-4}BPC Subtype: {1}", " ", bpc.Id.SubtypeName);
					_report.AppendLine();
					foreach (var bpd in bpc)
					{
						if (!bpd.Public) continue;
						_report.AppendFormat("{0,-6}BPD Subtype: {1}", " ", bpd.Id.SubtypeName);
						_report.AppendLine();
						_report.AppendFormat("{0,-8}[P]", " ");
						foreach (var pre in bpd.Prerequisites)
						{
							_report.AppendFormat(" [{1:00.00}] {2}", " ", (float)pre.Amount, pre.Id.SubtypeName);
						}
						_report.AppendLine();
						_report.AppendFormat("{0,-8}[R]", " ");
						foreach (var res in bpd.Results)
						{
							_report.AppendFormat(" [{1:00.00}] {2}", " ", (float)res.Amount, res.Id.SubtypeName);
						}
						_report.AppendLine();
					}
					_report.AppendLine();
				}
				_report.AppendLine();
			}
			_report.AppendLine();
			_report.AppendLine("--------------------------- End Assemblers -----------------------------");
			_report.AppendLine();

			WriteToLog("Assemblers", _report.ToString(), LogType.General);
		}

		private void PrintLists()
		{
			_report.Clear();
			_report.AppendLine();
			_report.AppendLine();
			_report.AppendLine("------------------------------ Lists! ------------------------------");
			_report.AppendLine();
			_report.AppendFormat("{0,-4}Ores: {1}", " ", _oreDictionary.Count);
			_report.AppendLine();

			foreach (var ore in _oreDictionary)
			{
				_report.AppendFormat("{0,-8} {1}", " ", ore.Key);
				_report.AppendLine();
			}

			_report.AppendLine();
			_report.AppendFormat("{0,-4}Ingots: {1}", " ", _ingotDictionary.Count);
			_report.AppendLine();
			foreach (var ingot in _ingotDictionary)
			{
				_report.AppendFormat("{0,-8} {1}", " ", ingot.Key);
				_report.AppendLine();
			}

			_report.AppendLine();
			_report.AppendFormat("{0,-4}Scraps: {1}", " ", _scrapDictionary.Count);
			_report.AppendLine();
			foreach (var scrap in _scrapDictionary)
			{
				_report.AppendFormat("{0,-8} {1}", " ", scrap.Key);
				_report.AppendLine();
			}

			_report.AppendLine();
			_report.AppendFormat("{0,-4}Components: {1}", " ", _componentDictionary.Count);
			_report.AppendLine();
			foreach (var comp in _componentDictionary)
			{
				_report.AppendFormat("{0,-8} {1}", " ", comp.Key);
				_report.AppendLine();
			}

			_report.AppendLine();
			_report.AppendFormat("{0,-4}Remaining: {1}", " ", _remainingDictionary.Count);
			_report.AppendLine();
			foreach (var remainder in _remainingDictionary)
			{
				_report.AppendFormat("{0,-8} {1}", " ", remainder.Key);
				_report.AppendLine();
			}

			_report.AppendLine();
			_report.AppendLine("--------------------------- End Lists ------------------------------");
			_report.AppendLine();

			WriteToLog("Lists", _report.ToString(), LogType.General);
		}

		private void PrintScrapDefManagerOutput()
		{
			foreach (var fcm in _componentMaps)
			{
				WriteToLog("PSD", $"{fcm.Key} - Def in Manager: {fcm.Value.HasDefinitionInManager()}  |  Needs Post Process: {fcm.Value.NeedPostProcess()}", LogType.General);
			}
		}

		private void PrintPreComponentMapsSimple()
		{
			WriteToLog("PPC", $"[{_componentMaps.Count()}] Items in Collection", LogType.General);
			foreach (var component in _componentMaps)
			{
				WriteToLog("PPC", $"{component.Value}", LogType.General);
			}
		}

		private void PrintFinalComponentMaps()
		{
			_report.Clear();
			_report.AppendLine();
			_report.AppendFormat("[{0}] Items in Collection", _componentMaps.Count());
			_report.AppendLine();
			foreach (var component in _componentMaps)
			{
				_report.AppendFormat("{1}", " ", component.Value);
				_report.AppendLine();
			}
			_report.AppendLine();
			WriteToLog("PFC", _report.ToString(), LogType.General);
		}

		#endregion
	}
}