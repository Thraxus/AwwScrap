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

		public string PrintBasicInformation()
		{
			if (!HasValidScrap()) return $"No valid scrap: {_componentDefinition.Id.SubtypeName}";
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0,-2}{1} | {2}", " ", _componentDefinition.Id.SubtypeName, _scrapDefinition.Id.SubtypeName);
			sb.AppendLine();
			sb.AppendFormat("{0,-4}[{1}][{2}][{3:00}][{4:0000}s] ", " ", SkitCompatible ? "T" : "F", HasFalseCompatibleBlueprintClasses ? "T" : "F", (float)_amountProduced, _productionTime);
			foreach (var cpr in ComponentPrerequisites)
			{
				sb.AppendFormat("[{0:00.00}] {1} ", (float)cpr.Value, cpr.Key);
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

		public MyBlueprintDefinition GetScrapBlueprint()
		{
			return _scrapBlueprint;
		}

		public void SetScrapPrivate()
		{
			_scrapDefinition.Public = false;
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

		#region Unused / Debug Only

		public string PrintCompatibleBlueprintClasses()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0,-1}[{1}][{2:00}] {3}", " ", HasFalseCompatibleBlueprintClasses ? "T" : "F", _compatibleBlueprints.Count, _componentDefinition.Id.SubtypeName);
			foreach (var cbp in _compatibleBlueprints)
			{
				sb.AppendFormat("{0,-1}[{1}]", " ", cbp.Id.SubtypeName);
			}
			return sb.ToString();
		}

		public string PrintComponentPrerequisites()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0,-1}[{1:00}] {2}", " ", ComponentPrerequisites.Count, _componentDefinition.Id.SubtypeName);
			foreach (var cpr in ComponentPrerequisites)
			{
				sb.AppendFormat("{0,-1}[{1}]", " ", cpr.Key);
			}
			return sb.ToString();
		}


		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0,-1}[{1}] [{2}]", " ", ComponentPrerequisites.Count, _compatibleBlueprints.Count);
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
			
			return sb.ToString();
		}

		#endregion
	}
}