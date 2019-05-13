using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulxer
{
    public class PosManager : IPosManager
    {
        private ILeechPlatform _platform;
        private int _insID;
        private Order _order;

        public PosManager(ILeechPlatform platform, int insID)
        {
            _platform = platform;
            _insID = insID;
            _platform.OnTick(_insID, OnTick, true);
            _order = null;
        }

        public void ClosePosManager()
        {
            _platform.OnTick(_insID, OnTick, false);
        }

        private void OnTick(DateTime time, decimal price, int lots)
        {
            if (_order == null || _order.Status == OrderStatus.Active) return;
            _order = null;
        }

        public bool OpenLong(int lots)
        {
            if (_order != null) return false; // есть активный ордер
            var hold = _platform.GetHoldingLots(_insID);
            if (hold != 0) return false; // позиция уже открыта (не важно какая)

            _order = _platform.AddBuyOrder(_insID, null, lots);

            return true;
        }

        public bool OpenShort(int lots)
        {
            if (_order != null) return false; // есть активный ордер
            var hold = _platform.GetHoldingLots(_insID);
            if (hold != 0) return false; // позиция уже открыта (не важно какая)

            _order = _platform.AddSellOrder(_insID, null, lots);

            return true;
        }

        public bool ClosePos()
        {
            if (_order != null) return false; // есть активный ордер
            var hold = _platform.GetHoldingLots(_insID);
            if (hold == 0) return false; // позиции нет

            if (hold > 0)
            {
                _order = _platform.AddSellOrder(_insID, null, hold);
            }
            else
            {
                _order = _platform.AddBuyOrder(_insID, null, hold);
            }

            return true;
        }

        public int GetPos()
        {
            return _platform.GetHoldingLots(_insID);
        }

        public bool IsReady
        {
            get
            {
                return _order == null;
            }
        }
    }
}
