using System;
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
		private MyPhysicalItemDefinition _scrapDefinition;
		private MyBlueprintDefinition _scrapBlueprintDefinition;
		
		private float _resourceCount;
		private string _error;

		public Dictionary<string, MyFixedPoint> ComponentPrerequisites = new Dictionary<string, MyFixedPoint>();
		public List<MyBlueprintClassDefinition> CompatibleBlueprints = new List<MyBlueprintClassDefinition>();

		private void RecalculateResourceCount()
		{
			_resourceCount = 0;
			foreach (var cpr in ComponentPrerequisites)
			{
				_resourceCount += (float)cpr.Value;
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
				ComponentPrerequisites.Remove(map.Key);
			}
			RecalculateResourceCount();
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
			if (CompatibleBlueprints.Contains(bcd)) return;
			CompatibleBlueprints.Add(bcd);
		}
		
		public void RunScrapSetup()
		{
			SetScrapAttributes();
			GenerateScrapBlueprint();
			ApplyScrapBlueprint();
		}

		private void SetScrapAttributes()
		{
			if (_componentDefinition == null) return;
			if (_scrapDefinition == null) return;
			_scrapDefinition.Mass = _componentDefinition.Mass * Constants.ScrapScalar;
			_scrapDefinition.Volume = _componentDefinition.Volume * Constants.ScrapScalar;
			_scrapDefinition.MaxStackAmount = MyFixedPoint.MaxValue;
		}

		private void GenerateScrapBlueprint()
		{
			if (_scrapDefinition == null) return;
			if (CompatibleBlueprints.Count <= 0) return;
			try
			{
				_scrapBlueprintDefinition = (
					MyBlueprintDefinition)MyDefinitionManager.Static.GetBlueprintDefinition(
					new MyDefinitionId(typeof(MyObjectBuilder_BlueprintDefinition), 
						_componentDefinition.Id.SubtypeName + Constants.ScrapSuffix + Constants.ScrapBpSuffix));
				
				if (_scrapBlueprintDefinition == null)
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
							Amount = cpr.Value * Constants.ScrapScalar,
							Id = new MyDefinitionId(typeof(MyObjectBuilder_Ingot), cpr.Key)
						});
				}

				_scrapBlueprintDefinition.Prerequisites = new[]
				{
					new MyBlueprintDefinitionBase.Item
					{
						Amount = 1,
						Id = _scrapDefinition.Id
					}
				};

				_scrapBlueprintDefinition.Results = items.ToArray();
				_scrapBlueprintDefinition.Postprocess();
			}
			catch (Exception e)
			{
				_error = e.ToString();
			}
		}
		
		private void ApplyScrapBlueprint()
		{
			if (CompatibleBlueprints.Count <= 0) return;
			if (_scrapBlueprintDefinition == null) return;
			foreach (var cbp in CompatibleBlueprints)
			{
				if (!cbp.ContainsBlueprint(_scrapBlueprintDefinition))
					cbp.AddBlueprint(_scrapBlueprintDefinition);
			}
		}

		public bool HasDefinitionInManager()
		{
			if (_scrapBlueprintDefinition == null) return false;
			MyBlueprintDefinitionBase b = MyDefinitionManager.Static.GetBlueprintDefinition(_scrapBlueprintDefinition.Id);
			return b != null;
		}

		public bool NeedPostProcess()
		{
			return _scrapBlueprintDefinition == null || _scrapBlueprintDefinition.PostprocessNeeded;
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
			sb.AppendFormat("{0,-1}[{1}] [{2}]", " ", ComponentPrerequisites.Count, CompatibleBlueprints.Count);
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
			sb.AppendFormat("{0,-2}ScrapBp: {1}", " ", _scrapBlueprintDefinition == null ? "No ScrapBp created" : _scrapBlueprintDefinition.Id.SubtypeName);
			sb.AppendLine();
			if (_scrapBlueprintDefinition != null)
			{
				sb.AppendFormat("{0,-4}Prerequisites:", " ");
				foreach (var pre in _scrapBlueprintDefinition.Prerequisites)
				{
					sb.AppendFormat(" [{0}] {1}", pre.Amount, pre.Id.SubtypeName);
				}
				sb.AppendLine();
				sb.AppendFormat("{0,-4}Results:", " ");
				foreach (var res in _scrapBlueprintDefinition.Results)
				{
					sb.AppendFormat(" [{0}] {1}", res.Amount, res.Id.SubtypeName);
				}
				sb.AppendLine();
			}
			sb.AppendFormat("{0,-4}Compatible BPCs:", " ");
			if (CompatibleBlueprints.Count <= 0)
			{
				sb.AppendFormat(" No Compatible BPCs identified.");
				return sb.ToString();
			}
			foreach (var cb in CompatibleBlueprints)
			{
				sb.AppendFormat(" [{0}]", cb.Id.SubtypeId);
			}

			if (!string.IsNullOrEmpty(_error))
				sb.AppendLine($"\nError!!!\n{_error}");

			return sb.ToString();
		}
	}
}