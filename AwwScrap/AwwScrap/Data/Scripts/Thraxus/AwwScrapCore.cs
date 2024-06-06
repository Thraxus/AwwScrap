using System.Text;
using AwwScrap.Common.BaseClasses;
using AwwScrap.Common.Enums;
using AwwScrap.Controllers;
using VRage.Game.Components;
using AwwScrap.UserConfig.Controller;
using AwwScrap.UserConfig.Settings;
using Sandbox.Game.Gui;

namespace AwwScrap
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	public class AwwScrapCore : BaseSessionComp
	{
		protected override string CompName { get; } = "AwwScrapCore";
		protected override CompType Type { get; } = CompType.Both;
		protected override MyUpdateOrder Schedule { get; } = MyUpdateOrder.BeforeSimulation;

        //private MyRefineryDefinition _awwScrapRefineryDefinition;
        //private readonly MyBlueprintClassDefinition _awwScrapAllScrapBlueprintClassDefinition = MyDefinitionManager.Static.GetBlueprintClass(Constants.AwwScrapAllScrapClassName);
        
        private SettingsController _settingsController;
        private DefinitionController _definitionController;

        protected override void Unload()
        {
            _definitionController.OnWriteToLog -= WriteGeneral;
            base.Unload();
        }

        protected override void SuperEarlySetup()
		{
			base.SuperEarlySetup();
            _settingsController = new SettingsController(ModContext.ModName);
            _settingsController.Initialize();
            _definitionController = new DefinitionController(_settingsController, ModContext.ModPath);
			_definitionController.OnWriteToLog += WriteGeneral;
			_definitionController.Init();
        }

        protected override void UpdateBeforeSim()
        {
            base.UpdateBeforeSim();
            MyHud.BlockInfo.HideComponentLossNotification();
        }

        protected override void LateSetup()
        {
            base.LateSetup();
			var sbValidScrap = new StringBuilder();
            var validScrap = false;
            var sbSkippedScrap = new StringBuilder();
            var skippedScrap = false;
            var sbInvalidScrap = new StringBuilder();
            var sbEasyDefGenerator = new StringBuilder();
            var invalidScrap = false;
			
			sbValidScrap.AppendLine("\n");
            sbValidScrap.AppendFormat("{0,-1}The following valid scrap was created...", " ");
            sbValidScrap.AppendLine("\n");
			
            sbSkippedScrap.AppendFormat("{0,-1}The following valid scrap was intentionally skipped...", " ");
            sbSkippedScrap.AppendLine("\n");
			
            sbInvalidScrap.AppendFormat("{0,-1}The following components did not contain valid scrap...", " ");
            sbInvalidScrap.AppendLine("\n");

            sbEasyDefGenerator.AppendFormat("{0,-1}The following is used by Thraxus for setting up new scrap...", " ");
            sbEasyDefGenerator.AppendLine("\n");

            foreach (var cm in _definitionController.ScrapControllers)
            {
                if (cm.Value.HasValidScrap())
                {
                    sbValidScrap.AppendLine(cm.Value.ToString());
                    validScrap = true;
                }
                else if (cm.Value.IntentionallySkipped)
                {
                    sbSkippedScrap.AppendLine(cm.Value.ToString());
                    skippedScrap = true;
                }
				else
                {
                    sbInvalidScrap.AppendLine(cm.Value.ToString());
                    sbEasyDefGenerator.AppendLine(cm.Value.GetEasyDefGeneratorString());
                    invalidScrap = true;
                }
            }

            if (!validScrap) sbValidScrap.AppendLine("  None");
            if (!skippedScrap) sbSkippedScrap.AppendLine("  None");
            if (!invalidScrap)
            {
                sbInvalidScrap.AppendLine("  None");
                sbEasyDefGenerator.Clear();
            };

            sbValidScrap.AppendLine(sbSkippedScrap.ToString());
            sbValidScrap.AppendLine(sbInvalidScrap.ToString());
            sbValidScrap.AppendLine(sbEasyDefGenerator.ToString());
            sbValidScrap.AppendLine(DefaultSettings.PrintSettings().ToString());
            sbValidScrap.AppendLine();

            WriteGeneral("LateSetup", sbValidScrap.ToString());
        }

        //private void PrintAwwScrapRecyclerStuffs()
        //{
        //	WriteToLog(nameof(LateSetup), $"{_awwScrapRefineryDefinition.BlueprintClasses.Count}", LogType.General);
        //	foreach (var bpc in _awwScrapRefineryDefinition.BlueprintClasses)
        //	{
        //		WriteToLog(nameof(LateSetup), $"", LogType.General);
        //		WriteToLog(nameof(LateSetup), $"{bpc.Id.SubtypeName}", LogType.General);
        //		foreach (var bp in bpc)
        //		{
        //			foreach (var pre in bp.Prerequisites)
        //			{
        //				WriteToLog(nameof(LateSetup), $"[P] [{(float)pre.Amount:00.00}] {pre.Id.SubtypeName}", LogType.General);
        //			}
        //			foreach (var res in bp.Results)
        //			{
        //				WriteToLog(nameof(LateSetup), $"[R] [{(float)res.Amount:00.00}] {res.Id.SubtypeName}", LogType.General);
        //			}
        //		}
        //	}
        //}

        //private void SetupAwwScrapRecycler()
        //{
        //    if (_awwScrapRefineryDefinition == null) return;
        //    foreach (var cm in _componentMaps)
        //    {
        //        if (!cm.Value.HasValidScrap()) continue;
        //        _awwScrapAllScrapBlueprintClassDefinition.AddBlueprint(cm.Value.GetScrapBlueprint());
        //        WriteGeneral(nameof(ScourRefineries), $"Added: {cm.Value.GetScrapBlueprint().Id.SubtypeName}");
        //    }
        //    _awwScrapRefineryDefinition.BlueprintClasses.Clear();
        //    _awwScrapRefineryDefinition.BlueprintClasses.Add(_awwScrapAllScrapBlueprintClassDefinition);
        //    _awwScrapRefineryDefinition.LoadPostProcess();
        //}


    }
}