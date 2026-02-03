using System;

namespace AwwScrap.Common.Interfaces
{
    internal interface IClose
    {
        event Action<IClose> OnClose;
        void Close();
    }
}
