namespace Common.Interfaces
{
    /// <summary>
    /// DependencyManager intercace.
    /// </summary>
    public interface IDependencyManager
    {
        void SingleDepend(object obj, object fromObj);
        void Depend(object obj, object fromObj);
        void Undepend(object obj);
        bool AnyDependsFrom(object obj);
    }
}
