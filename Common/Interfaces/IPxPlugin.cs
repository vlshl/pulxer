using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface IPxPlugin
    {
        void OnLoad();
        void OnDestroy();
        PxColumn[] GetColumns();
        object[] GetData();
    }
}
