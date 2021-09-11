using System.Collections.Generic;
using System.Text;
using Sandbox.Definitions;

namespace AwwScrap
{
	public class ComponentMap
	{
		public MyPhysicalItemDefinition ComponentDefinition;
		public ComponentPrerequisites DesiredPrerequisites = new ComponentPrerequisites();
		public List<ComponentPrerequisites> AllComponentPrerequisiteOptions = new List<ComponentPrerequisites>();
		public bool Tainted;


		public void AddComponentPrerequisites(MyBlueprintDefinitionBase bpd)
		{
			//if (DesiredPrerequisites.Prerequisites.Count > 0) return;
			//ComponentPrerequisites cp = new ComponentPrerequisites();
			//cp.AddPrerequisite(bpd);


			if (AllComponentPrerequisiteOptions.Count == 0)
			{
				ComponentPrerequisites cp = new ComponentPrerequisites();
				cp.AddPrerequisite(bpd);
				AllComponentPrerequisiteOptions.Add(cp);
				DesiredPrerequisites.CopyFrom(cp);
				return;
			}
			
			int i = 0;
			foreach (var cp in AllComponentPrerequisiteOptions)
			{
				if (cp.CompareTo(bpd))
					i++;
			}
			if (i == AllComponentPrerequisiteOptions.Count) return;
			var cp2 = new ComponentPrerequisites();
			cp2.AddPrerequisite(bpd);
			AllComponentPrerequisiteOptions.Add(cp2);
		}

		public void CheckForTaintedPrerequisites(Dictionary<string, MyPhysicalItemDefinition> validationDictionary)
		{
			foreach (var component in DesiredPrerequisites.Prerequisites)
			{
				if (validationDictionary.ContainsKey(component.Key)) Tainted = true;
			}
		}

		public void CopyFrom(ComponentMap map)
		{
			ComponentDefinition = map.ComponentDefinition;
			DesiredPrerequisites.CopyFrom(map.DesiredPrerequisites);
		}

		public string ToStringSimple()
		{
			StringBuilder sb = new StringBuilder();
			//sb.AppendFormat("{0,-2} [{1}] [{2, -18}]", " ", AllComponentPrerequisiteOptions.Count > 1 ? "T" : "F", ComponentDefinition.Id.SubtypeName);
			sb.AppendFormat("{0,-2} [{1, -18}]", " ", ComponentDefinition.Id.SubtypeName);
			sb.AppendFormat("{0,-2} [{1:000.00}] [{2}]", " ", DesiredPrerequisites.ResourceCount, DesiredPrerequisites.Tainted ? "T" : "F");
			if (DesiredPrerequisites.Prerequisites.Count <= 0) return sb.ToString();
			foreach (var dpr in DesiredPrerequisites.Prerequisites)
			{
				sb.AppendFormat("{0,-1} [{1:00.00}] {2, -18}", " ", (float)dpr.Value, dpr.Key);
			}
			return sb.ToString();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendLine();
			sb.AppendLine("------------------ Comp Map -------------------");
			sb.AppendLine();
			sb.AppendFormat("{0,-4} [{1}] {2}", " ", AllComponentPrerequisiteOptions.Count > 1 ? "T" : "F", ComponentDefinition.Id.SubtypeName);
			sb.AppendLine();
			sb.AppendLine();
			sb.AppendFormat("{0,-6} [{1:00.00}] [{2}] Desired Prerequisites", " ", DesiredPrerequisites.ResourceCount, DesiredPrerequisites.Tainted ? "T" : "F");
			sb.AppendLine();
			sb.AppendLine();
			foreach (var dpr in DesiredPrerequisites.Prerequisites)
			{
				sb.AppendFormat("{0,-8} [{1:00.00}] {2}", " ", (float)dpr.Value, dpr.Key);
				sb.AppendLine();
			}
			sb.AppendLine();
			sb.AppendLine();
			sb.AppendFormat("{0,-10} [{1}] All Available Prerequisites", " ", AllComponentPrerequisiteOptions.Count);
			sb.AppendLine();
			sb.AppendLine();
			foreach (var cp in AllComponentPrerequisiteOptions)
			{
				foreach (var cpp in cp.Prerequisites)
				{
					sb.AppendFormat("{0,-12} [{1:00.00}] {2}", " ", (float)cpp.Value, cpp.Key);
					sb.AppendLine();
				}
				sb.AppendLine();
			}
			sb.AppendLine();

			//if (BlueprintDefinitions.Count > 1)
			//{
			//	sb.AppendLine();
			//	sb.AppendFormat("{0,-12} All Definitions [{1}]", " ", BlueprintDefinitions.Count);
			//	sb.AppendLine();
			//	sb.AppendLine();
			//	foreach (MyBlueprintDefinitionBase def in BlueprintDefinitions)
			//	{
			//		sb.AppendFormat("{0,-16} {1}", " ", def.Id.SubtypeId);
			//		sb.AppendLine();
			//		foreach (var pre in def.Prerequisites)
			//		{
			//			sb.AppendFormat("{0,-20} [{1:00.00}] {2}", " ", (float)pre.Amount, pre.Id.SubtypeId);
			//			sb.AppendLine();
			//		}
			//	}
			//	sb.AppendLine();
			//}

			sb.AppendLine("---------------- End Comp Map -----------------");
			return sb.ToString();
		}
	}
}