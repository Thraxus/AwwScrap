using System;
using AwwScrap.Common.Enums;
using AwwScrap.Common.Utilities;
using VRage.Game;

namespace AwwScrap.Common.Interfaces
{
	public interface ICommon
	{
		event Action<ICommon> OnClose;
		event Action<string, string, LogType, bool, int, string> OnWriteToLog;

		void Update(ulong tick);

		bool IsClosed { get; }

		void Close();

		void WriteToLog(string caller, string message, LogType type, bool showOnHud = false, int duration = CommonSettings.DefaultLocalMessageDisplayTime, string color = MyFontEnum.Green);
	}
}
