using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// Named objects interface
    /// </summary>
    public interface INamed
    {
        string GetName();
    }

    /// <summary>
    /// Named ValueRow source.
    /// Add name, guid and owner.
    /// </summary>
    public class ValueRowSource
    {
        private string _guid = "";
        private string _name = "";
        private INamed _named = null;
        private ValueRow _valueRow = null;
        private object _owner = null;

        /// <summary>
        /// Named ValueRow source
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="vr"></param>
        /// <param name="owner"></param>
        public ValueRowSource(string guid, string name, ValueRow vr, object owner)
        {
            _guid = guid;
            _name = name;
            _valueRow = vr;
            _owner = owner;
        }

        /// <summary>
        /// ValueRow source with dynamic name
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="named"></param>
        /// <param name="vr"></param>
        /// <param name="owner"></param>
        public ValueRowSource(string guid, INamed named, ValueRow vr, object owner)
        {
            _guid = guid;
            _named = named;
            _valueRow = vr;
            _owner = owner;
        }

        /// <summary>
        /// Guid for identify
        /// </summary>
        public string Guid
        {
            get
            {
                return _guid;
            }
        }

        /// <summary>
        /// Source name
        /// </summary>
        public string Name
        {
            get
            {
                if (_named != null) return _named.GetName();
                return _name;
            }
        }

        /// <summary>
        /// ValueRow object
        /// </summary>
        public ValueRow ValueRow
        {
            get
            {
                return _valueRow;
            }
        }

        /// <summary>
        /// Owner object
        /// </summary>
        public object Owner
        {
            get
            {
                return _owner;
            }
        }
    }
}
