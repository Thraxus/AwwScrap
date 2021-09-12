using System.Collections.Generic;
using System.Text;
using Sandbox.Definitions;
using VRage;
using VRage.Game;

namespace AwwScrap
{
	public class ComponentMap
	{
		private MyPhysicalItemDefinition _componentDefinition;
		private MyPhysicalItemDefinition _scrapDefinition;
		public Dictionary<string, MyFixedPoint> ComponentPrerequisites = new Dictionary<string, MyFixedPoint>();
		public List<MyBlueprintClassDefinition> CompatibleBlueprints = new List<MyBlueprintClassDefinition>();
		public float ResourceCount;
		public bool Tainted;
		private const string ScrapSuffix = "Scrap";

		private void SetScrapDefinition()
		{
			if (_componentDefinition == null) return;
			_scrapDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(
				new MyDefinitionId(typeof(MyObjectBuilder_Ore), _componentDefinition.Id.SubtypeName + ScrapSuffix));
			if (_scrapDefinition.Id.SubtypeName != _componentDefinition.Id.SubtypeName + ScrapSuffix)
				_scrapDefinition = null;
		}

		public void SetComponentDefinition(MyPhysicalItemDefinition def)
		{
			if (def == null) return;
			_componentDefinition = def;
			if (_scrapDefinition == null)
				SetScrapDefinition();
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
				ComponentPrerequisites.Add(pre.Id.SubtypeName, pre.Amount);
				ResourceCount += (float)pre.Amount;
			}
		}
		
		public void CheckForTaintedPrerequisites(Dictionary<string, MyPhysicalItemDefinition> validationDictionary)
		{
			foreach (var component in ComponentPrerequisites)
			{
				if (validationDictionary.ContainsKey(component.Key)) Tainted = true;
			}
		}

		public void CopyFrom(ComponentMap map)
		{
			SetComponentDefinition(map._componentDefinition);
			foreach (var pre in map.ComponentPrerequisites)
			{
				if (ComponentPrerequisites.ContainsKey(pre.Key))
					ComponentPrerequisites[pre.Key] += pre.Value;
				else ComponentPrerequisites.Add(pre.Key, pre.Value);
				ResourceCount += (float)pre.Value;
			}
		}

		public void AddCompatibleRefineryBpc(MyBlueprintClassDefinition bcd)
		{
			if (CompatibleBlueprints.Contains(bcd)) return;
			CompatibleBlueprints.Add(bcd);
		}

		public void AddToPrerequisites(string key, MyFixedPoint value)
		{
			if (ComponentPrerequisites.ContainsKey(key))
			{
				ComponentPrerequisites[key] += value;
				ResourceCount += (float)value;
				return;
			}
			ComponentPrerequisites.Add(key, value);
			ResourceCount += (float)value;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0,-1}[{1}] [{2}]", " ", ComponentPrerequisites.Count, CompatibleBlueprints.Count);
			sb.AppendFormat("{0,-1}[{1:000.00}] [{2}]", " ", ResourceCount, Tainted ? "T" : "F");
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
			sb.AppendFormat("{0,-2}Scrap: {1}", " ", _scrapDefinition == null ? "No Scrap Identified" : _scrapDefinition.Id.SubtypeName);
			sb.AppendLine();
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
			return sb.ToString();
		}
	}
}