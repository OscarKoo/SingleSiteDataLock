using System.Collections.Concurrent;
using System.Linq;

namespace Dao.SingleSiteDataLock
{
    static class SingleSiteDataLock
    {
        internal static readonly ConcurrentDictionary<string, LockObject> Locks = new ConcurrentDictionary<string, LockObject>();

        internal static bool TryWriterLock(string key, string user, ForceReleaseOption option, out bool updated) =>
            Locks.GetOrAdd(key, k => new LockObject(k)).TryWriterLock(user, option, out updated);

        internal static bool TryReaderLock(string key, string user, ForceReleaseOption option, out bool updated) =>
            Locks.GetOrAdd(key, k => new LockObject(k)).TryReaderLock(user, option, out updated);

        internal static bool IsWriterLocked(string key, string user, ForceReleaseOption option) =>
            Locks.TryGetValue(key, out var locked) && !locked.CanWrite(user, option);

        internal static bool ReleaseWriterLock(string key, string user, ForceReleaseOption option) =>
            Locks.GetOrAdd(key, k => new LockObject(k)).TryReleaseWriterLock(user, option, Locks);

        internal static bool ReleaseReaderLock(string key, string user, ForceReleaseOption option) =>
            Locks.GetOrAdd(key, k => new LockObject(k)).TryReleaseReaderLock(user, option, Locks);

        internal static void ReleaseAll(string user, ForceReleaseOption option) =>
            Locks.Keys.ToList().ParallelForEach(e => Locks[e].TryReleaseBothLocks(user, option, Locks));
    }
}