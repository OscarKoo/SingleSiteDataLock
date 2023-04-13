using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dao.SingleSiteDataLock
{
    public static class Extensions
    {
        public static void ParallelForEach<T>(this ICollection<T> collection, Action<T> action)
        {
            switch (collection.Count)
            {
                case 0: return;
                case 1:
                    action(collection.First());
                    return;
                default:
                    Parallel.ForEach(collection, action);
                    return;
            }
        }
    }
}