using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Dao.SingleSiteDataLock
{
    static class SingleSiteDataLock
    {
        internal static readonly ConcurrentDictionary<string, LockObject> Locks = new ConcurrentDictionary<string, LockObject>(StringComparer.OrdinalIgnoreCase);

        static LockObject GetLock(string key) => Locks.GetOrAdd(key, k => new LockObject(k));

        internal static bool TryWriterLock(string key, string user, ForceReleaseOption option, out bool updated) =>
            GetLock(key).TryWriterLock(user, option, out updated);

        internal static bool TryReaderLock(string key, string user, ForceReleaseOption option, out bool updated) =>
            GetLock(key).TryReaderLock(user, option, out updated);

        internal static bool IsWriterLocked(string key, string user, ForceReleaseOption option) =>
            Locks.TryGetValue(key, out var locked) && !locked.CanWrite(user, option);

        internal static bool ReleaseWriterLock(string key, string user, ForceReleaseOption option) =>
            GetLock(key).TryReleaseWriterLock(user, option, Locks);

        internal static bool ReleaseReaderLock(string key, string user, ForceReleaseOption option) =>
            GetLock(key).TryReleaseReaderLock(user, option, Locks);

        internal static void ReleaseAll(string user, ForceReleaseOption option) =>
            Locks.Keys.ToList().ParallelForEach(e => Locks[e].TryReleaseBothLocks(user, option, Locks));
    }
}