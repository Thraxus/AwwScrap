using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AwwScrap.Common.BaseClasses;
using AwwScrap.Common.Enums;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage;
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

		private readonly Dictionary<string, ScrapMap> _scrapMaps = new Dictionary<string, ScrapMap>();
		private readonly CachingDictionary<string, ComponentMap> _preComponentMaps = new CachingDictionary<string, ComponentMap>();
		private readonly Dictionary<string, ComponentMap> _finalComponentMaps = new Dictionary<string, ComponentMap>();
		private readonly StringBuilder _report = new StringBuilder();
		private const string ScrapSuffix = "Scrap";

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

		protected override void LateSetup()
		{
			base.LateSetup();
			GrabInformation();
			//PrintAssemblerBlueprints();
			ScourAssemblers();
			//PrintPreComponentMapsSimple();
			IdentifyTaintedComponentMaps();
			//PrintRefineryBlueprints();
			ScourRefineries();
			PrintBlueprintClassOutputs();
			FindCompatibleBlueprints();
			//PrintFinalComponentMaps();
			//BuildScrapDictionary();
			//BuildCompDictionary();
			//PopulateComponentPrerequisites();
			//GetUniqueIngotList();
			//PrintProductionBlockDefinitions();
			PrintFinalComponentMaps();
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
					if (def.Id.SubtypeName == "ZoneChip") continue;
					_componentDictionary.Add(def.Id.SubtypeName, def);
					var compMap = new ComponentMap();
					compMap.SetComponentDefinition(def);
					_preComponentMaps.Add(def.Id.SubtypeName, compMap);
					_preComponentMaps.ApplyChanges();
					continue;
				}
				_remainingDictionary.Add(def.Id.SubtypeName, def);
			}

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
						if (bpd.Results.Length == 1 && _preComponentMaps.ContainsKey(bpd.Results[0].Id.SubtypeName))
							_preComponentMaps[bpd.Results[0].Id.SubtypeName].AddComponentPrerequisites(bpd);
					}
				}
			}
		}

		private void PrintPreComponentMapsSimple()
		{
			WriteToLog("PPC", $"[{_preComponentMaps.Count()}] Items in Collection", LogType.General);
			foreach (var component in _preComponentMaps)
			{
				WriteToLog("PPC", $"{component.Value}", LogType.General);
			}
		}

		private void PrintFinalComponentMaps()
		{
			var sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("[{0}] Items in Collection", _finalComponentMaps.Count);
			sb.AppendLine();
			foreach (var component in _finalComponentMaps)
			{
				sb.AppendFormat("{1}", " ", component.Value);
				sb.AppendLine();
			}
			sb.AppendLine();
			WriteToLog("PFC", sb.ToString(), LogType.General);
		}

		private void IdentifyTaintedComponentMaps()
		{
			//	1) Filter out all non-tainted components
			foreach (var preComp in _preComponentMaps)
			{
				preComp.Value.CheckForTaintedPrerequisites(_componentDictionary);
				if (preComp.Value.Tainted) continue;
				ComponentMap map = new ComponentMap();
				map.CopyFrom(preComp.Value);
				_finalComponentMaps.Add(preComp.Key, map);
				_preComponentMaps.Remove(preComp.Key);
			}
			_preComponentMaps.ApplyRemovals();

			if (!_preComponentMaps.Any()) return;

			//	2) Take each tainted component and fix the taint
			
			const int iterationCap = 50;
			int iterationCount = 0;
			
			try
			{
				do
				{
					iterationCount++;
					foreach (var preComp in _preComponentMaps)
					{
						bool tainted = false;
						//WriteToLog("ITC", $"Finalizing: [{iterationCount:000}] [{(iterationCount >= iterationCap ? "T" : "F")}] [{(iterationCap > iterationCount ? "T" : "F")}] {preComp.Value.ToStringSimple()}", LogType.General);
						foreach (var pre in preComp.Value.ComponentPrerequisites)
						{
							if (!_preComponentMaps.ContainsKey(pre.Key)) continue;
							tainted = true;
							break;
						}
						if (tainted)
							continue;
						var map = new ComponentMap();
						map.SetComponentDefinition(preComp.Value.GetComponentDefinition());
						foreach (var pre in preComp.Value.ComponentPrerequisites)
						{
							if (_finalComponentMaps.ContainsKey(pre.Key))
							{
								foreach (var fPre in _finalComponentMaps[pre.Key].ComponentPrerequisites)
								{
									map.AddToPrerequisites(fPre.Key, fPre.Value * pre.Value);
								}
								continue;
							}
							map.AddToPrerequisites(pre.Key, pre.Value);
						}
						_preComponentMaps.Remove(preComp.Key);
						_finalComponentMaps.Add(preComp.Key, map);
					}
					_preComponentMaps.ApplyRemovals();
				} while (_preComponentMaps.Any() && iterationCount <= iterationCap);
			}
			catch (Exception e)
			{
				WriteToLog("IdentifyTaintedComponentMaps", $"Shit broke..... \n{e}", LogType.General);
			}
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

		private readonly Dictionary<MyBlueprintClassDefinition, List<string>> _blueprintClassOutputs =
			new Dictionary<MyBlueprintClassDefinition, List<string>>();

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

		private void FindCompatibleBlueprints()
		{
			foreach (var bco in _blueprintClassOutputs)
			{
				foreach (var fcm in _finalComponentMaps)
				{
					bool compatible = true;
					foreach (var pre in fcm.Value.ComponentPrerequisites)
					{
						if (!bco.Value.Contains(pre.Key))
							compatible = false;
					}
					if (compatible)
						fcm.Value.AddCompatibleRefineryBpc(bco.Key);
				}
			}
		}

		//private void BuildScrapDictionary()
		//{
		//	//	1) Build ScrapMap to map scrap to component definitions
		//	foreach (var def in MyDefinitionManager.Static.GetDefinitionsOfType<MyPhysicalItemDefinition>())
		//	{
		//		if (!ValidateScrap(def.Id.SubtypeName)) continue;
		//		var map = new ScrapMap
		//		{
		//			ScrapDef = def,
		//			CompDef = MyDefinitionManager.Static.GetPhysicalItemDefinition(
		//				new MyDefinitionId(typeof(MyObjectBuilder_Component),
		//					ScrubSuffix(def.Id.SubtypeName, ScrapSuffix))) //.Substring(0, def.Id.SubtypeName.Length - ScrapSuffix.Length)))
		//		};
		//		if (map.CompDef == null || map.CompDef.Id.SubtypeName == "SemiAutoPistolMagazine") continue;
		//		_scrapMaps.Add(map.ScrapDef.Id.SubtypeName, map);
		//		WriteToLog("BuildScrapDictionary", $"Def: {map}", LogType.General);
		//	}
		//}

		//private void BuildCompDictionary()
		//{
		//	foreach (var scrap in _scrapMaps)
		//	{
		//		if (_preComponentMaps.ContainsKey(scrap.Value.CompDef.Id.SubtypeName)) continue;
		//		var cmp = new ComponentMap()
		//		{
		//			ComponentDefinition = scrap.Value.CompDef
		//		};
		//		_preComponentMaps.Add(scrap.Value.CompDef.Id.SubtypeName, cmp);
		//	}

		//	foreach (var comp in _preComponentMaps)
		//	{
		//		WriteToLog("BuildCompDictionary", $"{comp.Key}", LogType.General);
		//	}
		//}
		
		//private void PopulateComponentPrerequisites()
		//{
		//	foreach (MyProductionBlockDefinition def in MyDefinitionManager.Static.GetDefinitionsOfType<MyProductionBlockDefinition>())
		//	{
		//		foreach (MyBlueprintClassDefinition bpc in def.BlueprintClasses)
		//		{
		//			foreach (MyBlueprintDefinitionBase bpd in bpc)
		//			{
		//				if (bpd.Results.Length != 1) continue;
		//				if (!_preComponentMaps.ContainsKey(bpd.Results[0].Id.SubtypeName)) continue;
		//				if (bpd.Results[0].Amount != 1) continue;
		//				_preComponentMaps[bpd.Results[0].Id.SubtypeName].AddComponentPrerequisites(bpd);
		//			}
		//		}
		//	}

		//	foreach (var comp in _preComponentMaps)
		//	{
		//		WriteToLog("PopulateComponentBlueprintClasses", $"{comp.Value}", LogType.General);
		//	}
		//}

		//private readonly List<string> _ingots = new List<string>();

		
		//private void GetUniqueIngotList()
		//{
		//	foreach (var comp in _componentMaps)
		//	{
		//		foreach (var pre in comp.Value.Prerequisites)
		//		{
		//			if (_ingots.Contains(pre.Key)) continue;
		//			_ingots.Add(pre.Key);
		//		}

		//	}

		//	foreach (var ingot in _ingots)
		//	{
		//		WriteToLog("GetUniqueIngotList", $"{ingot}", LogType.General);
		//	}

		//	WriteToLog("Spacer", $"\n", LogType.General);

		//	foreach (var re in MyDefinitionManager.Static.GetDefinitionsOfType<MyRefineryDefinition>())
		//	{
		//		WriteToLog("Refinery", $"{re.Id.SubtypeId}", LogType.General);
		//		WriteToLog("Spacer", $"", LogType.General);
		//		foreach (var bpc in re.BlueprintClasses)
		//		{
		//			WriteToLog("Refinery", $"    {bpc.Id.SubtypeName}", LogType.General);
		//			WriteToLog("Spacer", $"", LogType.General);
		//			foreach (var bpd in bpc)
		//			{
		//				WriteToLog("Refinery", $"        {bpd.Id.SubtypeName}", LogType.General);
		//				foreach (var pre in bpd.Prerequisites)
		//				{
		//					WriteToLog("Refinery", $"            [P] [{(float)pre.Amount:00.00}] {pre.Id.SubtypeName}",
		//						LogType.General);
		//				}

		//				foreach (var res in bpd.Results)
		//				{
		//					WriteToLog("Refinery", $"            [R] [{(float)res.Amount:00.00}] {res.Id.SubtypeName}",
		//						LogType.General);
		//				}
		//				WriteToLog("Spacer", $"", LogType.General);
		//			}
		//		}
		//	}

		//	WriteToLog("Spacer", $"\n", LogType.General);

		//	foreach (var ass in MyDefinitionManager.Static.GetDefinitionsOfType<MyAssemblerDefinition>())
		//	{
		//		WriteToLog("Assembler", $"{ass.Id.SubtypeId}", LogType.General);
		//		WriteToLog("Spacer", $"", LogType.General);
		//		foreach (var bpc in ass.BlueprintClasses)
		//		{
		//			WriteToLog("Assembler", $"    {bpc.Id.SubtypeName}", LogType.General);
		//			WriteToLog("Spacer", $"", LogType.General);
		//			foreach (var bpd in bpc)
		//			{
		//				WriteToLog("Assembler", $"        {bpd.Id.SubtypeName}", LogType.General);
		//				foreach (var pre in bpd.Prerequisites)
		//				{
		//					WriteToLog("Assembler", $"            [P] [{(float)pre.Amount:00.00}] {pre.Id.SubtypeName}",
		//						LogType.General);
		//				}

		//				foreach (var res in bpd.Results)
		//				{
		//					WriteToLog("Assembler", $"            [R] [{(float)res.Amount:00.00}] {res.Id.SubtypeName}",
		//						LogType.General);
		//				}
		//				WriteToLog("Spacer", $"", LogType.General);
		//			}
		//		}

		//	}
		//}

		private void PopulateIngotPrerequisites()
		{
			foreach (MyProductionBlockDefinition def in MyDefinitionManager.Static.GetDefinitionsOfType<MyProductionBlockDefinition>())
			{
				foreach (MyBlueprintClassDefinition bpc in def.BlueprintClasses)
				{
					foreach (MyBlueprintDefinitionBase bpd in bpc)
					{
						if (bpd.Results.Length != 1) continue;
						if (!_preComponentMaps.ContainsKey(bpd.Results[0].Id.SubtypeName)) continue;
						if (bpd.Results[0].Amount != 1) continue;
						_preComponentMaps[bpd.Results[0].Id.SubtypeName].AddComponentPrerequisites(bpd);
					}
				}
			}

			foreach (var comp in _preComponentMaps)
			{
				WriteToLog("PopulateComponentBlueprintClasses", $"{comp.Value}", LogType.General);
			}
		}

		private static bool ValidateScrap(string compName)
		{
			return compName.EndsWith(ScrapSuffix, StringComparison.OrdinalIgnoreCase) && !compName.Equals(ScrapSuffix, StringComparison.OrdinalIgnoreCase);
		}

		private string ScrubSuffix(string scrub, string value)
		{
			return !scrub.Contains(value) ? scrub : scrub.Substring(0, scrub.Length - value.Length);
		}

		

		private void PrintProductionBlockDefinitions()
		{
			_report.Clear();

			_report.AppendLine("\n");
			_report.AppendLine("** MyBlueprintClassDefinition Rundown **");
			_report.AppendLine();

			List<MyBlueprintClassDefinition> bpClasses = new List<MyBlueprintClassDefinition>();
			
			foreach (var def in MyDefinitionManager.Static.GetDefinitionsOfType<MyProductionBlockDefinition>())
			{
				foreach (var bpcl in def.BlueprintClasses)
				{
					bpClasses.Add(bpcl);
				}
			}

			//foreach (var bpClass in MyDefinitionManager.Static.GetDefinitionsOfType<MyBlueprintClassDefinition>())
			foreach (var bpClass in bpClasses)
			{
				_report.AppendLine();
				_report.AppendFormat("{0,-8}Blueprint Class: {1}", " ", bpClass.Id.SubtypeName);
				_report.AppendLine();
				foreach (var bp in bpClass)
				{
					_report.AppendFormat("{0,-12}SubtypeId: {1}", " ", bp.Id.SubtypeId);
					_report.AppendLine();

					foreach (var pre in bp.Prerequisites)
					{
						_report.AppendFormat("{0,-16}PreRequisite: [{1}] {2}", " ", pre.Amount, pre.Id.SubtypeId);
						_report.AppendLine();
					}

					foreach (var res in bp.Prerequisites)
					{
						_report.AppendFormat("{0,-16}Result: [{1}] {2}", " ", res.Amount, res.Id.SubtypeId);
						_report.AppendLine();
					}
				}
			}

			//foreach (var def in MyDefinitionManager.Static.GetDefinitionsOfType<MyProductionBlockDefinition>())
			//{
			//	_report.AppendLine();
			//	_report.AppendFormat("{0,-4}Definition Type: {1}", " ", def);
			//	_report.AppendLine();

			//	_report.AppendLine();
			//	_report.AppendFormat("{0,-8}Blueprint Class: {1}", " ", def);
			//	_report.AppendLine();
			//	foreach (var bpc in def.BlueprintClasses)
			//	{
			//		_report.AppendFormat("{0,-12}Definition Key: {1}", " ", bpc);
			//		_report.AppendLine();
			//		_report.AppendFormat("{0,-12}TypeId: {1,-30} SubtypeId: {2} ", " ", bpc.Id, bpc.Id.SubtypeId);
			//		_report.AppendLine("\n");
			//	}
			//}

			WriteToLog("", _report.ToString(), LogType.General);
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
				//MyRefineryDefinition basicRefinery = MyDefinitionManager.Static.GetCubeBlockDefinition(new MyDefinitionId(typeof(MyObjectBuilder_Refinery), "Blast Furnace")) as MyRefineryDefinition;
				//if (basicRefinery != null) basicRefinery.MaterialEfficiency = 1.0f;
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