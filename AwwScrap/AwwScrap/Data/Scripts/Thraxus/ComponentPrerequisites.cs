using System.Collections.Generic;
using Sandbox.Definitions;
using VRage;

namespace AwwScrap
{
	public class ComponentPrerequisites
	{
		
		public Dictionary<string, MyFixedPoint> Prerequisites = new Dictionary<string, MyFixedPoint>();
		public float ResourceCount;
		public bool Tainted;

		public void AddPrerequisite(MyBlueprintDefinitionBase bpd)
		{
			foreach (var pre in bpd.Prerequisites)
			{
				Prerequisites.Add(pre.Id.SubtypeName, pre.Amount);
				ResourceCount += (float)pre.Amount;
			}
		}

		public void AddToPrerequisites(string key, MyFixedPoint value)
		{
			if (Prerequisites.ContainsKey(key))
			{
				Prerequisites[key] += value;
				ResourceCount += (float)value;
				return;
			}
			Prerequisites.Add(key, value);
			ResourceCount += (float)value;
		}

		public void CopyFrom(ComponentPrerequisites from)
		{
			foreach (var pre in from.Prerequisites)
			{
				if (Prerequisites.ContainsKey(pre.Key))
					Prerequisites[pre.Key] += pre.Value;
				else Prerequisites.Add(pre.Key, pre.Value);
				ResourceCount += (float)pre.Value;
			}
		}

		public bool CompareTo(ComponentPrerequisites compare)
		{
			if (compare.Prerequisites.Count != Prerequisites.Count) return false;
			foreach (var component in Prerequisites)
			{
				if (!compare.Prerequisites.ContainsKey(component.Key)) return false;
				if (compare.Prerequisites[component.Key] != component.Value) return false;
			}
			return true;
		}

		public bool CompareTo(MyBlueprintDefinitionBase bpd)
		{
			if (bpd.Prerequisites.Length != Prerequisites.Count) return false;
			foreach (var pre in Prerequisites)
			{
				if (!Prerequisites.ContainsKey(pre.Key)) return false;
				if (Prerequisites[pre.Key] != pre.Value) return false;
			}
			return true;
		}
	}
}