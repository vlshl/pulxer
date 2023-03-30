using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface ITickSubscribe
    {
        void Subscribe(object subscriber, int insID, OnTickEH onTick);
        void Unsubscribe(object subscriber, int insID);
        void UnsubscribeAll();
    }
}
