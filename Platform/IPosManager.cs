using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
    public interface IPosManager
    {
        bool IsReady { get; }
        bool OpenLong(int lots);
        bool OpenShort(int lots);
        bool ClosePos();
        void ClosePosManager();
        int GetPos();
    }
}
