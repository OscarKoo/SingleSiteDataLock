using System.Collections.Concurrent;
using System.Linq;

namespace Dao.SingleSiteDataLock
{
    static class SingleSiteDataLock
    {
        internal static readonly ConcurrentDictionary<LockIdentifier, LockObject> Locks = new ConcurrentDictionary<LockIdentifier, LockObject>();

        internal static bool TryLock(LockIdentifier identifier, string user, ForceReleaseOption option) =>
            Locks.AddOrUpdate(identifier, k => new LockObject(k, user), (k, v) =>
            {
                v.IsMyLock(user, option);
                return v;
            }).IsMyLock(user, option);

        internal static bool IsLocked(LockIdentifier identifier, string user, ForceReleaseOption option) =>
            Locks.TryGetValue(identifier, out var locked) && !locked.IsMyLock(user, option);

        internal static bool Release(LockIdentifier identifier, string user, ForceReleaseOption option)
        {
            if (!TryLock(identifier, user, option))
                return false;

            Locks.TryRemove(identifier, out _);
            return true;
        }

        internal static void ReleaseAll(string user, ForceReleaseOption option) =>
            Locks.Values.Where(w => w.IsMyLock(user, option)).ToList()
                .ParallelForEach(o => Locks.TryRemove(o.Identifier, out _));
    }
}