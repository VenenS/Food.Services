using Food.Data.Entities;
using System.Collections.Generic;

namespace Food.Data
{
    /// <summary>
    /// Сравнивает любые сущности по long Id
    /// </summary>
    public class EntityComparer<T> : IEqualityComparer<T> where T : EntityBaseDeletable<long>
    {
        public bool Equals(T x, T y) => x.Id == y.Id;

        public int GetHashCode(T obj) => obj.Id.GetHashCode();
    }
}
