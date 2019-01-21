using System;

namespace Common.Interfaces
{
    /// <summary>
    /// Lazy action manager interface
    /// </summary>
    public interface ILazyActionManager
    {
        /// <summary>
        /// Add action with tag
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="action"></param>
        void AddAction(object tag, Action action);

        /// <summary>
        /// Add action
        /// </summary>
        /// <param name="action"></param>
        void AddAction(Action action);

        /// <summary>
        /// Do all added actions
        /// </summary>
        void DoActions();
    }
}
