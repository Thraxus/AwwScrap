using AwwScrap.Support;
using AwwScrap.UserConfig.Controller;
using Sandbox.Definitions;
using System.Collections.Generic;
using AwwScrap.Common.BaseClasses;
using VRage.Collections;
using VRage.Game;
using VRage.Utils;
using System.Linq;
using System.Text;
using System;
using AwwScrap.Common.Extensions;
using AwwScrap.UserConfig.Settings;

namespace AwwScrap.Controllers
{
    internal class DefinitionController : BaseLoggingClass
    {
        public readonly CachingDictionary<string, ScrapController> ScrapControllers = new CachingDictionary<string, ScrapController>();

        private readonly Dictionary<MyBlueprintClassDefinition, HashSet<string>> _blueprintClassOutputs = new Dictionary<MyBlueprintClassDefinition, HashSet<string>>();
        private readonly Dictionary<MyBlueprintClassDefinition, HashSet<string>> _skitOutputs = new Dictionary<MyBlueprintClassDefinition, HashSet<string>>();
        
        private readonly MyPhysicalItemDefinition _genericScrap = MyDefinitionManager.Static.GetPhysicalItemDefinition(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Scrap"));
        private readonly MyBlueprintClassDefinition _awwScrapSkitBlueprintClassDefinition = MyDefinitionManager.Static.GetBlueprintClass(Constants.AwwScrapSkitClassName);
        
        private readonly Dictionary<string, MyPhysicalItemDefinition> _scrapDictionary = new Dictionary<string, MyPhysicalItemDefinition>();
        private readonly HashSet<MyStringHash> _ingots = new HashSet<MyStringHash>();
        private readonly Dictionary<MyStringHash, HashSet<MyStringHash>> _compoundIngots = new Dictionary<MyStringHash, HashSet<MyStringHash>>();
        
        private readonly string _modPath;
        private readonly SettingsController _settingsController;
        
        
        public DefinitionController(SettingsController settingsController, string modPath)
        {
            _settingsController = settingsController;
            _modPath = modPath;
        }

        public void Init()
        {
            GrabInformation();
            ScourAssemblers();
            EliminateCompoundComponents();
            ScrubBlacklistedScrapReturns();
            ScourRefineries();
            ScourSkits();
            FindCompatibleBlueprints();
            foreach (var fcm in ScrapControllers)
            {
                fcm.Value.RunScrapSetup();
            }
            FindCompatibleSkitBlueprints();
            SetupSkits();
            ApplyBlueprintChanges();
            SetDeconstructItems();
            HideBadScrap();
        }

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
                if (def.Id.TypeId == typeof(MyObjectBuilder_Component))
                {
                    if (Constants.ComponentBlacklist.Contains(def.Id.SubtypeName)) continue;
                    var compMap = new ScrapController(_modPath);
                    compMap.SetComponentDefinition(def);
                    ScrapControllers.Add(def.Id.SubtypeName, compMap);
                    ScrapControllers.ApplyChanges();
                }

                if (def.IsIngot)
                {
                    _ingots.Add(def.Id.SubtypeId);
                }
            }
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
                        if (bpd.Id.SubtypeName.Contains("/"))
                            break;
                        CheckBpdAgainstIngots(bpd);
                        if (bpd.Results.Length != 1 || !ScrapControllers.ContainsKey(bpd.Results[0].Id.SubtypeName))
                            continue;
                        ScrapControllers[bpd.Results[0].Id.SubtypeName].AddBlueprint(bpd);
                    }
                }
            }
        }

        private void CheckBpdAgainstIngots(MyBlueprintDefinitionBase bpd)
        {
            foreach (var result in bpd.Results)
            {
                if (!_ingots.Contains(result.Id.SubtypeId)) continue;
                if (!_compoundIngots.ContainsKey(result.Id.SubtypeId))
                    _compoundIngots.Add(result.Id.SubtypeId, new HashSet<MyStringHash>());
                foreach (var pre in bpd.Prerequisites)
                {
                    _compoundIngots[result.Id.SubtypeId].Add(pre.Id.SubtypeId);
                }
            }
        }

        private void EliminateCompoundComponents()
        {
            var componentMapQueue = new Queue<ScrapController>();
            const int breakAfter = 300;
            var currentLoop = 0;
            do
            {
                if (componentMapQueue.Count > 0)
                {
                    ScrapController map = componentMapQueue.Dequeue();
                    map.ReconcileCompoundComponents(ScrapControllers);
                    ScrapControllers.Add(map.GetComponentDefinition().Id.SubtypeName, map);
                    ScrapControllers.ApplyAdditionsAndModifications();
                }

                foreach (var cm in ScrapControllers)
                {
                    foreach (var cpr in cm.Value.ComponentPrerequisites)
                    {
                        if (!ScrapControllers.ContainsKey(cpr.Key)) continue;
                        componentMapQueue.Enqueue(cm.Value);
                        ScrapControllers.Remove(cm.Key);
                        break;
                    }
                }
                ScrapControllers.ApplyRemovals();
                currentLoop++;
            } while (componentMapQueue.Count > 0 && currentLoop < breakAfter);

            if (componentMapQueue.Count <= 0) return;

            var rpt = new StringBuilder();
            rpt.AppendLine();
            rpt.AppendLine();
            rpt.AppendFormat("{0, -2}Error parsing all components.  The Component Map Queue still contained the following {1} item(s).", " ", componentMapQueue.Count);
            rpt.AppendLine();
            rpt.AppendLine();
            foreach (var cmq in componentMapQueue)
                rpt.AppendLine(cmq.ToString());
            WriteGeneral(nameof(EliminateCompoundComponents), rpt.ToString());
        }

        private void ScrubBlacklistedScrapReturns()
        {
            foreach (var cm in ScrapControllers)
            {
                cm.Value.ScrubBlacklistedScrapReturns();
            }
        }

        private void ScourRefineries()
        {
            foreach (var refinery in MyDefinitionManager.Static.GetDefinitionsOfType<MyRefineryDefinition>())
            {
                if (!refinery.Public || refinery.Id.SubtypeName.Contains("StoneIncinerator")) continue;
                //if (refinery.Id.SubtypeName == Constants.AwwScrapRecyclerSubtypeName)
                //{
                //    _awwScrapRefineryDefinition = refinery;
                //    WriteGeneral(nameof(ScourRefineries), "Found the _awwScrapRefineryDefinition ...");
                //    continue;
                //}
                foreach (var bpc in refinery.BlueprintClasses)
                {
                    if (!bpc.Public) continue;
                    if (!_blueprintClassOutputs.ContainsKey(bpc))
                        _blueprintClassOutputs.Add(bpc, new HashSet<string>());
                    foreach (var bpd in bpc)
                    {
                        if (!bpd.Public) continue;
                        foreach (var res in bpd.Results)
                        {
                            _blueprintClassOutputs[bpc].Add(res.Id.SubtypeName);
                        }
                    }
                }
            }
        }

        private void ScourSkits()
        {
            foreach (var skit in MyDefinitionManager.Static.GetDefinitionsOfType<MySurvivalKitDefinition>())
            {
                if (!skit.Public) continue;
                foreach (var bpc in skit.BlueprintClasses)
                {
                    if (!bpc.Public) continue;
                    if (!_skitOutputs.ContainsKey(bpc))
                        _skitOutputs.Add(bpc, new HashSet<string>());
                    foreach (var bpd in bpc)
                    {
                        if (!bpd.Public) continue;
                        foreach (var res in bpd.Results)
                        {
                            _skitOutputs[bpc].Add(res.Id.SubtypeName);
                        }
                    }
                }
            }
        }

        private void FindCompatibleBlueprints()
        {
            foreach (var bco in _blueprintClassOutputs)
            {
                if (Constants.IgnoredBlueprintClasses.Contains(bco.Key.Id.SubtypeName)) continue;
                foreach (var cm in ScrapControllers)
                {
                    var compatible = true;
                    int maxCompatibility = cm.Value.ComponentPrerequisites.Count;
                    var falseHits = 0;
                    foreach (var pre in cm.Value.ComponentPrerequisites)
                    {
                        if (bco.Value.Contains(pre.Key)) continue;
                        if (CheckCompoundIngots(bco.Value, pre.Key)) continue;
                        compatible = false;
                        falseHits++;
                    }

                    // Unfortunate but necessary workaround for IO and the fact it let the "Ingots" BPC exist but not use it.
                    if (!compatible && 
                        bco.Key.Id.SubtypeName == "RefineryIngots" &&
                        cm.Value.GetComponentDefinition().Context.ModId == "2344068716.sbm" &&
                        (cm.Value.GetComponentDefinition()?.Id.SubtypeName == "Thrust" ||
                         cm.Value.GetComponentDefinition()?.Id.SubtypeName == "Explosives") &&
                        !string.IsNullOrWhiteSpace(cm.Value.GetComponentDefinition().Context.ModId))
                    {
                        cm.Value.AddCompatibleRefineryBpc(bco.Key, false);
                        continue;
                    }

                    if (compatible)
                    {
                        cm.Value.AddCompatibleRefineryBpc(bco.Key, false);
                        continue;
                    }
                    if (falseHits <= maxCompatibility * 0.5f)
                        cm.Value.AddCompatibleRefineryBpc(bco.Key, true);
                }
            }
        }

        private bool CheckCompoundIngots(ICollection<string> bcoValue, string preKey)
        {
            MyStringHash check = MyStringHash.GetOrCompute(preKey);
            HashSet<MyStringHash> compoundIngot;
            if (!_compoundIngots.TryGetValue(check, out compoundIngot)) return false;
            foreach (var results in compoundIngot)
            {
                if (bcoValue.Contains(results.ToString())) continue;
                return false;
            }
            return true;
        }

        private void FindCompatibleSkitBlueprints()
        {
            if (!DefaultSettings.SurvivalKitRecycling) return;

            foreach (var sko in _skitOutputs)
            {
                foreach (var cm in ScrapControllers)
                {
                    bool compatible = true;
                    foreach (var pre in cm.Value.ComponentPrerequisites)
                    {
                        if (!sko.Value.Contains(pre.Key))
                            compatible = false;
                    }

                    if (!compatible) continue;
                    MyBlueprintDefinition scrapDef = cm.Value.GetScrapBlueprint();
                    if (scrapDef == null) continue;
                    cm.Value.SkitCompatible = true;
                    _awwScrapSkitBlueprintClassDefinition.AddBlueprint(scrapDef);
                }
            }
        }

        private void SetupSkits()
        {
            if (!DefaultSettings.SurvivalKitRecycling) return;

            foreach (MyCubeBlockDefinition sKitDef in 
                     MyDefinitionManager.Static.GetAllDefinitions()
                         .OfType<MyCubeBlockDefinition>()
                         .Where(
                             myCubeBlockDefinition => myCubeBlockDefinition is MySurvivalKitDefinition))
            {
                ((MySurvivalKitDefinition)sKitDef).BlueprintClasses.Add(_awwScrapSkitBlueprintClassDefinition);
                foreach (var cm in ScrapControllers)
                {
                    if (!cm.Value.SkitCompatible) continue;
                    ((MySurvivalKitDefinition)sKitDef).InputInventoryConstraint.Add(cm.Value.GetScrapDefinition().Id);
                    ((MySurvivalKitDefinition)sKitDef).LoadPostProcess();
                }
            }
        }

        

        private void ApplyBlueprintChanges()
        {
            foreach (var refinery in MyDefinitionManager.Static.GetDefinitionsOfType<MyRefineryDefinition>())
            {
                refinery.LoadPostProcess();
            }
        }

        private static bool ValidateScrap(string compName)
        {
            return compName.EndsWith(Constants.ScrapSuffix, StringComparison.OrdinalIgnoreCase) && !compName.Equals(Constants.ScrapSuffix, StringComparison.OrdinalIgnoreCase);
        }

        private void SetDeconstructItems()
        {
            foreach (MyCubeBlockDefinition def in MyDefinitionManager.Static.GetAllDefinitions()
                .OfType<MyCubeBlockDefinition>()
                .Where(myCubeBlockDefinition => myCubeBlockDefinition?.Components != null))
            {
                if (Constants.IgnoredBlocks.Contains(def.Id.SubtypeName)) continue;
                foreach (var comp in def.Components)
                {
                    if (!comp.Definition.Public) continue;
                    if (Constants.DoNotScrap.Contains(comp.Definition.Id.SubtypeName)) continue;
                    if (ScrapControllers.ContainsKey(comp.Definition.Id.SubtypeName))
                    {
                        comp.DeconstructItem = ScrapControllers[comp.Definition.Id.SubtypeName].HasValidScrap()
                            ? ScrapControllers[comp.Definition.Id.SubtypeName].GetScrapDefinition()
                            : _genericScrap;
                        continue;
                    }
                    if (!_settingsController.ScrapUnknownItems) continue;
                    comp.DeconstructItem = _genericScrap;
                }
            }
        }

        private void HideBadScrap()
        {
            foreach (var cm in ScrapControllers)
            {
                if (!cm.Value.HasValidScrap()) continue;
                _scrapDictionary.Remove(cm.Value.GetScrapDefinition().Id.SubtypeName);
            }

            foreach (var sd in _scrapDictionary)
            {
                sd.Value.Public = false;
            }
        }

    }
}
