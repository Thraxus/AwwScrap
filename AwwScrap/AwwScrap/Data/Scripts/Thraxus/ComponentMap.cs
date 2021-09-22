using System.Collections.Generic;
using System.Text;
using AwwScrap.Support;
using Sandbox.Definitions;
using VRage;
using VRage.Collections;
using VRage.Game;

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

		public bool HasFalseCompatibleBlueprintClasses;
		public bool SkitCompatible;

		public readonly Dictionary<string, MyFixedPoint> ComponentPrerequisites = new Dictionary<string, MyFixedPoint>();
		private readonly List<MyBlueprintClassDefinition> _compatibleBlueprints = new List<MyBlueprintClassDefinition>();

		public MyBlueprintDefinition GetScrapBlueprint()
		{
			return _scrapBlueprint;
		}

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

		public void AddBlueprint(MyBlueprintDefinitionBase bp)
		{
			if (bp.Results.Length != 1) return;
			if (_componentBlueprint != null) return;
			if (bp.Results[0].Id.SubtypeName != _componentDefinition.Id.SubtypeName) return;
			_componentBlueprint = bp;
			_productionTime = bp.BaseProductionTimeInSeconds;
			_amountProduced = bp.Results[0].Amount;
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

		public void AddCompatibleRefineryBpc(MyBlueprintClassDefinition bcd, bool isFalseHit)
		{
			if (_compatibleBlueprints.Contains(bcd)) return;
			if (isFalseHit && !HasFalseCompatibleBlueprintClasses && _compatibleBlueprints.Count > 0) return;
			if (!isFalseHit && HasFalseCompatibleBlueprintClasses)
			{
				_compatibleBlueprints.Clear();
				HasFalseCompatibleBlueprintClasses = false;
				_compatibleBlueprints.Add(bcd);
				return;
			}
			if (!isFalseHit && !HasFalseCompatibleBlueprintClasses)
			{
				_compatibleBlueprints.Add(bcd);
				return;
			}
			HasFalseCompatibleBlueprintClasses = isFalseHit;
			_compatibleBlueprints.Add(bcd);
		}

		private void SetScrapAttributes()
		{
			if (_componentDefinition == null) return;
			if (_scrapDefinition == null) return;
			_scrapDefinition.Mass = _componentDefinition.Mass * Constants.ScrapMassScalar;
			_scrapDefinition.Volume = _componentDefinition.Volume * Constants.ScrapVolumeScalar;
			_scrapDefinition.MaxStackAmount = MyFixedPoint.MaxValue;
			_scrapDefinition.DisplayNameString = _componentDefinition.DisplayNameText + " " + Constants.ScrapSuffix;
		}

		private void GenerateScrapBlueprint()
		{
			if (_scrapDefinition == null) return;
			if (_compatibleBlueprints.Count <= 0) return;
			
			_scrapBlueprint = (
				MyBlueprintDefinition)MyDefinitionManager.Static.GetBlueprintDefinition(
				new MyDefinitionId(typeof(MyObjectBuilder_BlueprintDefinition), 
					_componentDefinition.Id.SubtypeName + Constants.ScrapSuffix + Constants.ScrapBpSuffix));

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
			}
		}

		public override string ToString()
		{
			if (!HasValidScrap()) return $"No valid scrap: {_componentDefinition.Id.SubtypeName}";
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0,-2}{1} | {2} | {3}", " ", _componentDefinition.Id.SubtypeName, _scrapDefinition.Id.SubtypeName, _scrapBlueprint.Id.SubtypeName);
			sb.AppendLine();
			sb.AppendFormat("{0,-4}[{1}][{2}][{3:00}][{4:00.00}s][{5:00.0000}][{6:00.0000}] | ", " ", SkitCompatible ? "T" : "F", HasFalseCompatibleBlueprintClasses ? "T" : "F", (float)_amountProduced, _productionTime, _scrapDefinition.Mass, _scrapDefinition.Volume);
			foreach (var cpr in ComponentPrerequisites)
			{
				sb.AppendFormat("[{0:00.00}] {1} ", (float)cpr.Value, cpr.Key);
			}
			sb.AppendLine();
			sb.AppendFormat("{0,-4}", " ");
			foreach (var sbp in _scrapBlueprint.Prerequisites)
			{
				sb.AppendFormat("[P][{0:00.00}] {1} ", (float)sbp.Amount, sbp.Id.SubtypeName);
			}
			sb.AppendLine();
			sb.AppendFormat("{0,-4}", " ");
			foreach (var sbr in _scrapBlueprint.Results)
			{
				sb.AppendFormat("[R][{0:00.00}] {1} ", (float)sbr.Amount, sbr.Id.SubtypeName);
			}
			sb.AppendLine();
			sb.AppendFormat("{0,-4}", " ");
			foreach (var cbp in _compatibleBlueprints)
			{
				sb.AppendFormat("[{0}] {1} ", cbp.ContainsBlueprint(_scrapBlueprint) ? "T" : "F", cbp.Id.SubtypeName);
			}
			sb.AppendLine();
			return sb.ToString();
		}
	}
}