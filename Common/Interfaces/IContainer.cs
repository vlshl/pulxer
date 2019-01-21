using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface IContainer
    {
        T Resolve<T>();
        object Resolve(Type type);
        void Register<TFrom, TTo>() where TTo : TFrom;
        void RegisterInstance(Type type, object instance);
    }
}
