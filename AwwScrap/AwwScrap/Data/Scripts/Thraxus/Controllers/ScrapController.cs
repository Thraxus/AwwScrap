using System.Collections.Generic;
using System.Text;
using AwwScrap.Common.Extensions;
using AwwScrap.Support;
using AwwScrap.UserConfig.Settings;
using Sandbox.Definitions;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.ObjectBuilders;

namespace AwwScrap.Controllers
{
	public class ScrapController
	{
		private MyPhysicalItemDefinition _componentDefinition;
		private MyBlueprintDefinitionBase _componentBlueprint;
		
		private MyPhysicalItemDefinition _scrapDefinition;
		private MyBlueprintDefinition _scrapBlueprint;

		public MyFixedPoint AmountProduced;
		private float _productionTime;

        public bool IntentionallySkipped;
		public bool HasFalseCompatibleBlueprintClasses;
		public bool SkitCompatible;

		public readonly Dictionary<string, MyFixedPoint> ComponentPrerequisites = new Dictionary<string, MyFixedPoint>();
        private readonly Dictionary<string, MyObjectBuilderType> _prerequisiteTypeMap = new Dictionary<string, MyObjectBuilderType>();
        private readonly List<MyBlueprintClassDefinition> _compatibleBlueprints = new List<MyBlueprintClassDefinition>();

		private const string GenericScrapOverlay = "\\Textures\\GUI\\Icons\\Components\\ScrapOverlayOutlineRed.dds";
		private readonly string _fullOverlayIcon;

        private readonly StringBuilder _writeMeLast = new StringBuilder();

        public ScrapController(string modPath)
		{
			_fullOverlayIcon = modPath + GenericScrapOverlay;
		}

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
            if (Constants.DoNotScrap.Contains(_componentDefinition.Id.SubtypeName))
            {
                IntentionallySkipped = true;
				return;
            }
            SetScrapAttributes();
			GenerateScrapBlueprint();
			ApplyScrapBlueprint();
		}

		public void ScrubBlacklistedScrapReturns()
		{
			foreach (var srb in Constants.ScrapReturnsBlacklist)
			{
				if (ComponentPrerequisites.ContainsKey(srb))
					ComponentPrerequisites.Remove(srb);
			}
		}

        public float GetProductionTime()
		{
			return _productionTime;
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
			bool hasCustomIcon = false;

			string[] icons = new string[_scrapDefinition.Icons.Length + 1];
			for (var i = 0; i < _scrapDefinition.Icons.Length; i++)
			{
				if (string.IsNullOrEmpty(_scrapDefinition.Icons[i])) continue;
				icons[i] = _scrapDefinition.Icons[i];
				if (_scrapDefinition.Icons[i].EndsWith("generic_scrap.dds"))
				{
					icons[i] = _componentDefinition.Icons[0];
					continue;
				}
				hasCustomIcon = true;
			}

			if (!hasCustomIcon)
			{
				icons[_scrapDefinition.Icons.Length] = _fullOverlayIcon;
				_scrapDefinition.Icons = icons;
			}

			_scrapDefinition.Mass = _componentDefinition.Mass * DefaultSettings.ScrapMassScalar;
			_scrapDefinition.Volume = _componentDefinition.Volume * DefaultSettings.ScrapVolumeScalar;
			_scrapDefinition.MaxStackAmount = MyFixedPoint.MaxValue;
			_scrapDefinition.DisplayNameString = _componentDefinition.DisplayNameText + " " + Constants.ScrapSuffix;
		}

		private void GenerateScrapBlueprint()
		{
            //_writeMeLast.AppendLine($"    GenerateScrapBlueprint(): [{(_scrapDefinition != null ? "T" : "F")}][{(_compatibleBlueprints.Count > 0 ? "T" : "F")}]");
            if (_scrapDefinition == null) return;
			if (_compatibleBlueprints.Count <= 0) return;
			
			_scrapBlueprint = 
				(MyBlueprintDefinition)MyDefinitionManager.Static.GetBlueprintDefinition(
				new MyDefinitionId(typeof(MyObjectBuilder_BlueprintDefinition), 
					_componentDefinition.Id.SubtypeName + Constants.ScrapSuffix + Constants.ScrapBpSuffix));
            
			if (_scrapBlueprint == null) return;

			var items = new List<MyBlueprintDefinitionBase.Item>();
			foreach (var cpr in ComponentPrerequisites)
			{
				items.Add(
					new MyBlueprintDefinitionBase.Item
					{
						// This will account for items that have more than 1 count of the resulting item, such as Light Bulbs and Armor Plates from IO
						Amount = (MyFixedPoint)((float)(cpr.Value * DefaultSettings.ScrapScalar) / (float)AmountProduced),
						Id = new MyDefinitionId(GetPrerequisiteType(cpr.Key), cpr.Key)
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

			var icons = (string[])_scrapDefinition.Icons.Clone();
			
			_scrapBlueprint.Icons = icons;
			_scrapBlueprint.DisplayNameString = _componentDefinition.DisplayNameText + " " + Constants.ScrapSuffix;
			_scrapBlueprint.BaseProductionTimeInSeconds = _productionTime * DefaultSettings.ScrapProductionTimeScalar;
			_scrapBlueprint.Results = items.ToArray();
			_scrapBlueprint.Postprocess();
		}

        private MyObjectBuilderType GetPrerequisiteType(string subtypeId)
        {
            MyObjectBuilderType type;
            if (_prerequisiteTypeMap.TryGetValue(subtypeId, out type)) return type;
            return typeof(MyObjectBuilder_Ingot);
        }

		private void ApplyScrapBlueprint()
		{
			if (_compatibleBlueprints.Count <= 0) return;
			if (_scrapBlueprint == null) return;
            if (_scrapBlueprint.Results == null || _scrapBlueprint.Results.Length <= 0)
            {
                _scrapBlueprint = null;
                _scrapDefinition = null;
                return;
            }
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

        public void AddToPrerequisites(string key, MyFixedPoint value)
        {
            if (ComponentPrerequisites.ContainsKey(key))
            {
                ComponentPrerequisites[key] += value;
                return;
            }
            ComponentPrerequisites.Add(key, value);
        }
		
        public void AddBlueprint(MyBlueprintDefinitionBase bpd)
        {
            if (bpd.Results.Length != 1) return;
            if (_componentBlueprint != null) return;
            if (bpd.Results[0].Id.SubtypeName != _componentDefinition.Id.SubtypeName) return;
            _componentBlueprint = bpd;
            _productionTime = bpd.BaseProductionTimeInSeconds;
            AmountProduced = bpd.Results[0].Amount;
			AddComponentPrerequisites(bpd);
        }

        private void AddComponentPrerequisites(MyBlueprintDefinitionBase bpd)
		{
			if (ComponentPrerequisites.Count > 0) return;
			foreach (var pre in bpd.Prerequisites)
            {
                MyFixedPoint prerequisite;
                if (ComponentPrerequisites.TryGetValue(pre.Id.SubtypeName, out prerequisite))
                {
                    if (_multipleComponentPrerequisitesDetected == null)
                    {
                        _multipleComponentPrerequisitesDetected = new StringBuilder();
                        _multipleComponentPrerequisitesDetected.AppendFormat("{0,-4}Multiple Component Prerequisites Detected!", " ");
                        _multipleComponentPrerequisitesDetected.AppendLine();
                        _multipleComponentPrerequisitesDetected.AppendFormat("{0,-6}[{1:F4}] {2}", " ", (float)prerequisite, pre.Id.SubtypeName);
                        _multipleComponentPrerequisitesDetected.AppendLine();
					}
					_multipleComponentPrerequisitesDetected.AppendFormat("{0,-6}[{1:F4}] {2}", " ", (float)pre.Amount / Constants.AssemblerMultiplier, pre.Id.SubtypeName);
					_multipleComponentPrerequisitesDetected.AppendLine();
					continue;
                }
                AddToPrerequisites(pre.Id.SubtypeName, (MyFixedPoint)((float)pre.Amount / Constants.AssemblerMultiplier));
                AddToPrerequisiteTypeMap(pre.Id.SubtypeName, pre.Id.TypeId);

            }
		}

        private void AddToPrerequisiteTypeMap(string subtypeName, MyObjectBuilderType typeId)
        {
            if (_prerequisiteTypeMap.ContainsKey(subtypeName)) return;
			_prerequisiteTypeMap.Add(subtypeName, typeId);
        }

        public void ReconcileCompoundComponents(CachingDictionary<string, ScrapController> compMap)
        {
            foreach (var map in compMap)
            {
                if (!ComponentPrerequisites.ContainsKey(map.Key)) continue;
                foreach (var cpr in map.Value.ComponentPrerequisites)
                {
                    AddToPrerequisites(cpr.Key, cpr.Value * ((float)ComponentPrerequisites[map.Key] / (float)map.Value.AmountProduced));
                }
                _productionTime += map.Value.GetProductionTime();
                ComponentPrerequisites.Remove(map.Key);
            }
        }

        private StringBuilder _multipleComponentPrerequisitesDetected;

        public string GetEasyDefGeneratorString()
		{
			return $"\"{_componentDefinition.Id.SubtypeName}\",";
        }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0,-2}Component Origin: [{1} ({2})]", " ", string.IsNullOrEmpty(_componentDefinition.Context.ModName) ? "Vanilla" : _componentDefinition.Context.ModName, string.IsNullOrEmpty(_componentDefinition.Context.ModId) ? "Base Game" : _componentDefinition.Context.ModId);
			sb.AppendLine();
			if (!HasValidScrap())
			{
				sb.AppendFormat("{0,-4}{1}[{2}][{3}] {4}", " ", 
					IntentionallySkipped ? 
						"Scrap Intentionally Skipped for: " : 
						"No valid scrap: ",
                    (GetScrapDefinition() != null ? "T" : "F"), 
                    (GetScrapBlueprint() != null ? "T" : "F"),
                    _componentDefinition.Id.SubtypeName);
				sb.AppendLine();
                if (_multipleComponentPrerequisitesDetected != null)
                {
                    sb.Append(_multipleComponentPrerequisitesDetected);
                }
                sb.Append(_writeMeLast);
                return sb.ToString();
			}
			
			sb.AppendFormat("{0,-2}{1} | {2} | {3}", " ", _componentDefinition.Id.SubtypeName, _scrapDefinition.Id.SubtypeName, _scrapBlueprint.Id.SubtypeName);
			sb.AppendLine();
			sb.AppendFormat("{0,-4}[{1}][{2}][{3:00}][{4:00.00}s][{5:00.0000}][{6:00.0000}][{7:D3}] | ", " ", SkitCompatible.ToSingleChar(), HasFalseCompatibleBlueprintClasses.ToSingleChar(), (float)AmountProduced, _productionTime, _scrapDefinition.Mass, _scrapDefinition.Volume, _scrapBlueprint.Results.Length);
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
				sb.AppendFormat("[{0}] {1} ", cbp.ContainsBlueprint(_scrapBlueprint).ToSingleChar(), cbp.Id.SubtypeName);
			}
			sb.AppendLine();
            sb.AppendFormat("{0,-4}Component Icon(s)", " ");
            sb.AppendLine();
			foreach (var icon in _componentDefinition.Icons)
			{
				sb.AppendFormat("{0,-6}{1}", " ", string.IsNullOrEmpty(icon) ? "icon was empty" : icon);
				sb.AppendLine();
			}
            sb.AppendFormat("{0,-4}Scrap Icon(s)", " ");
            sb.AppendLine();
			foreach (var icon in _scrapDefinition.Icons)
			{
				sb.AppendFormat("{0,-6}{1}", " ", string.IsNullOrEmpty(icon) ? "icon was empty" : icon);
				sb.AppendLine();
			}
            if (_multipleComponentPrerequisitesDetected != null)
            {
                sb.Append(_multipleComponentPrerequisitesDetected);
            }
			sb.Append(_writeMeLast);
			return sb.ToString();
		}
	}
}