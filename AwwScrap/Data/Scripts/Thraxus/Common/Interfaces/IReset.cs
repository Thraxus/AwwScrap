using System;

namespace AwwScrap.Common.Interfaces
{
    internal interface IReset
    {
        event Action<IReset> OnReset;
        void Reset();
    }
}
