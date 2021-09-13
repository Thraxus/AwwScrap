using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;

namespace AwwScrap
{
	public class ComponentMap
	{
		//public MyBlueprintClassDefinition ScrapBlueprintClass;
		private MyPhysicalItemDefinition _componentDefinition;
		private MyPhysicalItemDefinition _scrapDefinition;
		private MyBlueprintDefinition _scrapBlueprintDefinition;
		public Dictionary<string, MyFixedPoint> ComponentPrerequisites = new Dictionary<string, MyFixedPoint>();
		public List<MyBlueprintClassDefinition> CompatibleBlueprints = new List<MyBlueprintClassDefinition>();
		private MyModContext _modContext;


		public float ResourceCount;
		public bool Tainted;
		private const string ScrapSuffix = "Scrap";
		private const string ScrapBpSuffix = "ToIngot";
		private const float ScrapScalar = 0.9f;
		
		private void SetScrapDefinition()
		{
			if (_componentDefinition == null) return;
			_scrapDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(
				new MyDefinitionId(typeof(MyObjectBuilder_Ore), _componentDefinition.Id.SubtypeName + ScrapSuffix));
			if (_scrapDefinition.Id.SubtypeName != _componentDefinition.Id.SubtypeName + ScrapSuffix)
				_scrapDefinition = null;
		}

		public void SetComponentDefinition(MyPhysicalItemDefinition def, IMyModContext context)
		{
			if (def == null) return;
			_componentDefinition = def;
			if (_scrapDefinition == null)
				SetScrapDefinition();
			if (_modContext == null)
				_modContext = (MyModContext)context;
		}

		public void RunScrapSetup()
		{
			SetScrapAttributes();
			GenerateScrapBlueprint();
			ApplyScrapBlueprint();
		}

		public MyBlueprintDefinition GetScrapBlueprint()
		{
			return _scrapBlueprintDefinition;
		}

		private void SetScrapAttributes()
		{
			if (_componentDefinition == null) return;
			if (_scrapDefinition == null) return;
			_scrapDefinition.Mass = _componentDefinition.Mass * ScrapScalar;
			_scrapDefinition.Volume = _componentDefinition.Volume * ScrapScalar;
		}

		private void GenerateScrapBlueprint()
		{
			if (_scrapDefinition == null) return;
			if (CompatibleBlueprints.Count <= 0) return;

			//MyObjectBuilder_BlueprintClassDefinition bpcDef = new MyObjectBuilder_BlueprintClassDefinition()
			//{
			//	Id = new SerializableDefinitionId(typeof(MyObjectBuilder_BlueprintClassDefinition),
			//		_componentDefinition.Id.SubtypeName + ScrapSuffix + "Class"),
				
			//};

			//ScrapBlueprintClass = new MyBlueprintClassDefinition
			//{
			//	//Id = new MyDefinitionId(typeof(MyObjectBuilder_BlueprintClassDefinition), _componentDefinition.Id.SubtypeName + ScrapSuffix + "Class"),
			//	Id = bpcDef.Id,
			//	Public = true, 
			//	DisplayNameString = _componentDefinition.Id.SubtypeName + " Recycling",
			//	DescriptionString = _componentDefinition.Id.SubtypeName + " Recycling",
			//	Icons = new [] { "Textures\\GUI\\Icons\\component\\ScrapMetalComponent.dds" },
			//	HighlightIcon = "Textures\\GUI\\Icons\\component\\ScrapMetalComponent.dds",
			//	InputConstraintIcon = "Textures\\GUI\\Icons\\filter_ore.dds",
			//	OutputConstraintIcon = "Textures\\GUI\\Icons\\filter_ingot.dds"
			//};

			//ScrapBlueprintClass.Init(bpcDef, _modContext);

			try
			{
				List<MyBlueprintDefinitionBase.Item> items = new List<MyBlueprintDefinitionBase.Item>();
				foreach (var cpr in ComponentPrerequisites)
				{
					items.Add(
						new MyBlueprintDefinitionBase.Item
						{
							Amount = cpr.Value * ScrapScalar,
							Id = new MyDefinitionId(typeof(MyObjectBuilder_Ingot), cpr.Key)
						});
				}

				List<BlueprintItem> bpItems = new List<BlueprintItem>();
				foreach (var cpr in ComponentPrerequisites)
				{
					bpItems.Add(
						new BlueprintItem
						{
							Amount = (cpr.Value * ScrapScalar).ToString(),
							Id = new MyDefinitionId(typeof(MyObjectBuilder_Ingot), cpr.Key)
						});
				}

				MyObjectBuilder_BlueprintDefinition scrapBpOb = new MyObjectBuilder_BlueprintDefinition
				{
					Id = new SerializableDefinitionId(typeof(MyObjectBuilder_BlueprintDefinition),
						_componentDefinition.Id.SubtypeName + ScrapSuffix + ScrapBpSuffix),
					SubtypeName = _componentDefinition.Id.SubtypeName + ScrapSuffix + ScrapBpSuffix,
					DisplayName = _componentDefinition.DisplayNameText + " " + ScrapSuffix,
					Icons = new[]
					{
						_scrapDefinition.Icons[0]
					},
					BaseProductionTimeInSeconds = 0.5f,
					Public = true,
					AvailableInSurvival = true,
					Prerequisites = new[]
					{
						new BlueprintItem 
						{
							Amount = "1",
							Id = _scrapDefinition.Id
						}
					},
					Results = bpItems.ToArray(), 
					Enabled = true,
					Description = _componentDefinition.DisplayNameText + " " + ScrapSuffix,
				};

				_scrapBlueprintDefinition = new MyBlueprintDefinition
				{
					//Id = new MyDefinitionId(typeof(MyObjectBuilder_BlueprintDefinition), _componentDefinition.Id.SubtypeName + ScrapSuffix + ScrapBpSuffix),
					Id = scrapBpOb.Id,
					DisplayNameString = scrapBpOb.DisplayName,
					Icons = scrapBpOb.Icons,
					//Prerequisites = new[]
					//{
					//	new MyBlueprintDefinitionBase.Item
					//	{
					//		Amount = 1,
					//		//Id = new MyDefinitionId(typeof(MyObjectBuilder_Ore), _scrapDefinition.Id.SubtypeId)
					//		Id = _scrapDefinition.Id
					//	}
					//},
					//Results = items.ToArray(),
					BaseProductionTimeInSeconds = scrapBpOb.BaseProductionTimeInSeconds,
					Public = scrapBpOb.Public,
					AvailableInSurvival = scrapBpOb.AvailableInSurvival,
					ProgressBarSoundCue = scrapBpOb.ProgressBarSoundCue,
					IsPrimary = scrapBpOb.IsPrimary
			};

				MyObjectBuilder_BlueprintDefinition builder = (MyObjectBuilder_BlueprintDefinition)scrapBpOb;
				_scrapBlueprintDefinition.Prerequisites = new MyBlueprintDefinitionBase.Item[builder.Prerequisites.Length];
				for (int i = 0; i < _scrapBlueprintDefinition.Prerequisites.Length; ++i)
				{
					_scrapBlueprintDefinition.Prerequisites[i] = MyBlueprintDefinitionBase.Item.FromObjectBuilder(builder.Prerequisites[i]);
				}
				if (builder.Result != null)
				{
					_scrapBlueprintDefinition.Results = new MyBlueprintDefinitionBase.Item[1];
					_scrapBlueprintDefinition.Results[0] = MyBlueprintDefinitionBase.Item.FromObjectBuilder(builder.Result);
				}
				else
				{
					_scrapBlueprintDefinition.Results = new MyBlueprintDefinitionBase.Item[builder.Results.Length];
					for (int i = 0; i < _scrapBlueprintDefinition.Results.Length; ++i)
					{
						_scrapBlueprintDefinition.Results[i] = MyBlueprintDefinitionBase.Item.FromObjectBuilder(builder.Results[i]);
					}
				}

				//_scrapBlueprintDefinition.Init(scrapBpOb, MyModContext.BaseGame);
				_scrapBlueprintDefinition.Postprocess();
			}
			catch (Exception e)
			{
				_error = e.ToString();
			}

			//ScrapBlueprintClass.AddBlueprint(_scrapBlueprintDefinition);
		}

		private string _error;

		private void ApplyScrapBlueprint()
		{
			if (CompatibleBlueprints.Count <= 0) return;
			if (_scrapBlueprintDefinition == null) return;
			foreach (var cbp in CompatibleBlueprints)
			{
				if (!cbp.ContainsBlueprint(_scrapBlueprintDefinition))
					cbp.AddBlueprint(_scrapBlueprintDefinition);
				_scrapBlueprintDefinition.Postprocess();
				//cbp.Init(cbp.GetObjectBuilder(), _modContext);
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

		public void CopyFrom(ComponentMap map, IMyModContext context)
		{
			SetComponentDefinition(map._componentDefinition, context);
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
				sb.AppendLine($"\nError!!!\n{_error.Length}\n{_error}");

			return sb.ToString();
		}
	}
}