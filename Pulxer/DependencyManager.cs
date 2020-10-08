using Common.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Pulxer
{
    /// <summary>
    /// Диспетчер зависимости объектов друг от друга
    /// </summary>
    public class DependencyManager : IDependencyManager
    {
        private Dictionary<object, List<object>> _obj_refs = new Dictionary<object, List<object>>();

        /// <summary>
        /// Зависит ли кто-то от объекта?
        /// </summary>
        /// <param name="depFrom"></param>
        /// <returns></returns>
        public bool AnyDependsFrom(object depFrom)
        {
            if (!_obj_refs.ContainsKey(depFrom)) return false;

            return _obj_refs[depFrom].Any();
        }

        /// <summary>
        /// Объект obj зависит от depFrom.
        /// Повторный вызов с другим depFrom1 снимает зависимость от depFrom.
        /// </summary>
        /// <param name="obj">Исходный объект</param>
        /// <param name="depFrom">Зависит от объекта</param>
        public void SingleDepend(object obj, object depFrom)
        {
            Undepend(obj);
            Depend(obj, depFrom);
        }

        /// <summary>
        /// Объект obj зависит от depFrom.
        /// Повторный вызов с другим depFrom1 добавляет новую зависимость от depFrom1.
        /// </summary>
        /// <param name="obj">Исходный объект</param>
        /// <param name="depFrom">Зависит от объекта</param>
        public void Depend(object obj, object depFrom)
        {
            if (!_obj_refs.ContainsKey(depFrom))
            {
                _obj_refs.Add(depFrom, new List<object>());
            }
            _obj_refs[depFrom].Add(obj);
        }

        /// <summary>
        /// Снятие всех зависимостей с объекта. 
        /// Объект больше ни от кого не зависит.
        /// </summary>
        /// <param name="obj"></param>
        public void Undepend(object obj)
        {
            foreach (var key in _obj_refs.Keys)
            {
                _obj_refs[key].Remove(obj);
            }
        }
    }
}
