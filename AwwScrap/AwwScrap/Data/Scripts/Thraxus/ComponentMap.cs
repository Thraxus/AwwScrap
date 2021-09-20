using System;
using System.Collections.Generic;
using System.Text;
using AwwScrap.Support;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.ModAPI;

namespace AwwScrap
{
	public class ComponentMap
	{
		private MyPhysicalItemDefinition _componentDefinition;
		private MyBlueprintDefinitionBase _componentBlueprint;
		
		private MyPhysicalItemDefinition _scrapDefinition;
		private MyBlueprintDefinition _scrapBlueprint;

		private MyFixedPoint _amountProduced;
		private float _productionTime;
		private float _resourceCount;
		private string _error;

		public readonly Dictionary<string, MyFixedPoint> ComponentPrerequisites = new Dictionary<string, MyFixedPoint>();
		private readonly List<MyBlueprintClassDefinition> _compatibleBlueprints = new List<MyBlueprintClassDefinition>();

		public MyPhysicalItemDefinition GetScrapDefinition()
		{
			return _scrapDefinition;
		}

		public bool HasValidScrap()
		{
			return _scrapDefinition != null && _scrapBlueprint != null;
		}

		public void RunScrapSetup()
		{
			SetScrapAttributes();
			GenerateScrapBlueprint();
			ApplyScrapBlueprint();
		}

		private void RecalculateResourceCount()
		{
			_resourceCount = 0;
			foreach (var cpr in ComponentPrerequisites)
			{
				_resourceCount += (float)cpr.Value;
			}
		}

		public void AddBlueprint(MyBlueprintDefinitionBase bp)
		{
			if (bp.Results.Length != 1) return;
			if (_componentBlueprint != null) return;
			if (bp.Results[0].Id.SubtypeName != _componentDefinition.Id.SubtypeName) return;
			_componentBlueprint = bp;
			_productionTime = bp.BaseProductionTimeInSeconds;
			_amountProduced = bp.Results[0].Amount;
		}

		public float GetAmountProduced()
		{
			return (float)_amountProduced;
		}

		public void ScrubBlacklistedScrapReturns()
		{
			foreach (var srb in Constants.ScrapReturnsBlacklist)
			{
				if (ComponentPrerequisites.ContainsKey(srb))
					ComponentPrerequisites.Remove(srb);
			}
		}

		public void ReconcileCompoundComponents(CachingDictionary<string, ComponentMap> compMap)
		{
			foreach (var map in compMap)
			{
				if (!ComponentPrerequisites.ContainsKey(map.Key)) continue;
				foreach (var cpr in map.Value.ComponentPrerequisites)
				{
					AddToPrerequisites(cpr.Key, cpr.Value * ComponentPrerequisites[map.Key]);
					
				}
				_productionTime += map.Value.GetProductionTime();
				ComponentPrerequisites.Remove(map.Key);
			}
			RecalculateResourceCount();
		}

		public float GetProductionTime()
		{
			return _productionTime;
		}

		public void AddToPrerequisites(string key, MyFixedPoint value)
		{
			if (ComponentPrerequisites.ContainsKey(key))
			{
				ComponentPrerequisites[key] += value;
				_resourceCount += (float)value;
				return;
			}
			ComponentPrerequisites.Add(key, value);
		}

		public void SetComponentDefinition(MyPhysicalItemDefinition def)
		{
			if (def == null) return;
			_componentDefinition = def;
			if (_scrapDefinition == null)
				SetScrapDefinition();
		}
	
		private void SetScrapDefinition()
		{
			_scrapDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(
				new MyDefinitionId(typeof(MyObjectBuilder_Ore), _componentDefinition.Id.SubtypeName + Constants.ScrapSuffix));
			if (_scrapDefinition.Id.SubtypeName == _componentDefinition.Id.SubtypeName + Constants.ScrapSuffix) return;
			_scrapDefinition = null;
		}

		public void AddCompatibleRefineryBpc(MyBlueprintClassDefinition bcd)
		{
			if (_compatibleBlueprints.Contains(bcd)) return;
			_compatibleBlueprints.Add(bcd);
		}
		
		private void SetScrapAttributes()
		{
			if (_componentDefinition == null) return;
			if (_scrapDefinition == null) return;
			_scrapDefinition.Mass = _componentDefinition.Mass * Constants.ScrapScalar;
			_scrapDefinition.Volume = _componentDefinition.Volume * Constants.ScrapScalar;
			_scrapDefinition.MaxStackAmount = MyFixedPoint.MaxValue;
			_scrapDefinition.DisplayNameString = _componentDefinition.DisplayNameText + " " + Constants.ScrapSuffix;
		}

		private void GenerateScrapBlueprint()
		{
			if (_scrapDefinition == null) return;
			if (_compatibleBlueprints.Count <= 0) return;
			try
			{
				_scrapBlueprint = (
					MyBlueprintDefinition)MyDefinitionManager.Static.GetBlueprintDefinition(
					new MyDefinitionId(typeof(MyObjectBuilder_BlueprintDefinition), 
						_componentDefinition.Id.SubtypeName + Constants.ScrapSuffix + Constants.ScrapBpSuffix));
				
				if (_scrapBlueprint == null)
				{
					_error = "          Blueprint was Null!!";
					return;
				}

				var items = new List<MyBlueprintDefinitionBase.Item>();
				foreach (var cpr in ComponentPrerequisites)
				{
					items.Add(
						new MyBlueprintDefinitionBase.Item
						{
							// This will account for items that have more than 1 count of the resulting item, such as Light Bulbs and Armor Plates from IO
							Amount = (MyFixedPoint)((float)(cpr.Value * Constants.ScrapScalar) / (float)_amountProduced),
							Id = new MyDefinitionId(typeof(MyObjectBuilder_Ingot), cpr.Key)
						});
				}

				_scrapBlueprint.Prerequisites = new[]
				{
					new MyBlueprintDefinitionBase.Item
					{
						Amount = 1,
						Id = _scrapDefinition.Id
					}
				};

				_scrapBlueprint.DisplayNameString = _componentDefinition.DisplayNameText + " " + Constants.ScrapSuffix;
				_scrapBlueprint.BaseProductionTimeInSeconds = _productionTime * Constants.ScrapProductionTimeScalar;
				_scrapBlueprint.Results = items.ToArray();
				_scrapBlueprint.Postprocess();
			}
			catch (Exception e)
			{
				_error = e.ToString();
			}
		}
		
		private void ApplyScrapBlueprint()
		{
			if (_compatibleBlueprints.Count <= 0) return;
			if (_scrapBlueprint == null) return;
			foreach (var cbp in _compatibleBlueprints)
			{
				if (!cbp.ContainsBlueprint(_scrapBlueprint))
					cbp.AddBlueprint(_scrapBlueprint);
			}
		}

		public bool HasDefinitionInManager()
		{
			if (_scrapBlueprint == null) return false;
			MyBlueprintDefinitionBase b = MyDefinitionManager.Static.GetBlueprintDefinition(_scrapBlueprint.Id);
			return b != null;
		}

		public bool NeedPostProcess()
		{
			return _scrapBlueprint == null || _scrapBlueprint.PostprocessNeeded;
		}

		public MyPhysicalItemDefinition GetComponentDefinition()
		{
			return _componentDefinition;
		}

		public void AddComponentPrerequisites(MyBlueprintDefinitionBase bpd)
		{
			if (ComponentPrerequisites.Count > 0) return;
			foreach (var pre in bpd.Prerequisites)
			{
				ComponentPrerequisites.Add(pre.Id.SubtypeName, (MyFixedPoint)((float)pre.Amount / Constants.AssemblerMultiplier));
				_resourceCount += (float)pre.Amount;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0,-1}[{1}] [{2}]", " ", ComponentPrerequisites.Count, _compatibleBlueprints.Count);
			sb.AppendFormat("{0,-1}[{1:000.00}] ", " ", _resourceCount);
			sb.AppendLine();
			sb.AppendFormat("{0,-2}Item: {1}", " ", _componentDefinition?.Id.SubtypeName);
			sb.AppendLine();
			sb.AppendFormat("{0,-4}", " ");
			if (ComponentPrerequisites.Count <= 0) return sb.ToString();
			foreach (var pr in ComponentPrerequisites)
			{
				sb.AppendFormat(" [{0:00.00}] {1}", (float)pr.Value, pr.Key);
			}
			sb.AppendLine();
			sb.AppendFormat("{0,-2}Scrap: [{1}] {2}", " ", _scrapDefinition == null ? "N" : _scrapDefinition.IsOre ? "O" : _scrapDefinition.IsIngot ? "I" : "Z", _scrapDefinition == null ? "No Scrap Identified" : _scrapDefinition.Id.SubtypeName);
			sb.AppendLine();
			sb.AppendFormat("{0,-2}ScrapBp: {1}", " ", _scrapBlueprint == null ? "No ScrapBp created" : _scrapBlueprint.Id.SubtypeName);
			sb.AppendLine();
			if (_scrapBlueprint != null)
			{
				sb.AppendFormat("{0,-4}Prerequisites:", " ");
				foreach (var pre in _scrapBlueprint.Prerequisites)
				{
					sb.AppendFormat(" [{0}] {1}", pre.Amount, pre.Id.SubtypeName);
				}
				sb.AppendLine();
				sb.AppendFormat("{0,-4}Results:", " ");
				foreach (var res in _scrapBlueprint.Results)
				{
					sb.AppendFormat(" [{0}] {1}", res.Amount, res.Id.SubtypeName);
				}
				sb.AppendLine();
			}
			sb.AppendFormat("{0,-4}Compatible BPCs:", " ");
			if (_compatibleBlueprints.Count <= 0)
			{
				sb.AppendFormat(" No Compatible BPCs identified.");
				return sb.ToString();
			}
			foreach (var cb in _compatibleBlueprints)
			{
				sb.AppendFormat(" [{0}]", cb.Id.SubtypeId);
			}

			if (!string.IsNullOrEmpty(_error))
				sb.AppendLine($"\nError!!!\n{_error}");

			return sb.ToString();
		}
	}
}