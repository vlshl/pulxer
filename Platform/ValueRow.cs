using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
    public delegate void ValueRowChangeEventHandler(ValueRow vr, bool isReset);

    /// <summary>
    /// The notifying decimal values list.
    /// Add, AddRange and Clear operation available.
    /// </summary>
    public class ValueRow
    {
        /// <summary>
        /// Change list event handler
        /// </summary>
        public event ValueRowChangeEventHandler Change;

        private List<decimal?> _vls;
        private bool _isEventEnable = true;
        private bool _isPendingChanges = false;
        private bool _isPendingReset = false;

        public ValueRow()
        {
            _vls = new List<decimal?>();
        }

        /// <summary>
        /// Get value by index or null for incorrect index
        /// </summary>
        /// <param name="i">Index</param>
        /// <returns></returns>
        public decimal? this[int i]
        {
            get
            {
                if (i < 0 || i >= _vls.Count) return null;
                return _vls[i];
            }
        }

        /// <summary>
        /// Values list
        /// </summary>
        public List<decimal?> Values
        {
            get
            {
                return _vls;
            }
        }

        /// <summary>
        /// Add a new decimal value to list
        /// </summary>
        /// <param name="v"></param>
        public void Add(decimal? v)
        {
            _vls.Add(v);
            RaiseChangeEvent(false);
        }

        /// <summary>
        /// Add new values to list
        /// </summary>
        /// <param name="vs"></param>
        public void AddRange(decimal?[] vs)
        {
            _vls.AddRange(vs);
            RaiseChangeEvent(false);
        }

        /// <summary>
        /// Clear list
        /// </summary>
        public void Clear()
        {
            _vls.Clear();
            RaiseChangeEvent(true);
        }

        /// <summary>
        /// Count values
        /// </summary>
        public int Count
        {
            get
            {
                return _vls.Count;
            }
        }

        /// <summary>
        /// The last value index or -1 for empty list
        /// </summary>
        public int LastIndex
        {
            get
            {
                if (Count > 0) return Count - 1; else return -1;
            }
        }

        /// <summary>
        /// The last value in the list
        /// </summary>
        public decimal? LastValue
        {
            get
            {
                if (_vls.Count == 0) return null;
                return _vls[_vls.Count - 1];
            }
        }

        /// <summary>
        /// Suspend raising events
        /// </summary>
        public void SuspendEvents()
        {
            _isEventEnable = false;
        }

        /// <summary>
        /// Resume raising events and raise pending events
        /// </summary>
        public void ResumeEvents()
        {
            _isEventEnable = true;
            if (_isPendingChanges) RaiseChangeEvent(_isPendingReset);
            _isPendingChanges = _isPendingReset = false;
        }

        private void RaiseChangeEvent(bool isReset)
        {
            if (_isEventEnable)
            {
                if (Change != null) Change(this, isReset);
            }
            else
            {
                _isPendingChanges = true;
                if (isReset) _isPendingReset = true;
            }
        }
    }
}
