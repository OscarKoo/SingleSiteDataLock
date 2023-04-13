using System;
using System.Collections.Generic;
using System.Linq;

namespace Dao.SingleSiteDataLock
{
    public class ScopedLockContext : IDisposable
    {
        readonly HashSet<LockIdentifier> session = new HashSet<LockIdentifier>();

        public static readonly ForceReleaseOption DefaultForceReleaseOption = new ForceReleaseOption { TimeoutSeconds = 600 };

        public void Dispose() => this.session.Clear();

        public bool TryLock(string category, string key, string user, ForceReleaseOption option = null)
        {
            var identifier = new LockIdentifier(category, key);
            if (!SingleSiteDataLock.TryLock(identifier, user, option ?? DefaultForceReleaseOption))
            {
                Revert(user);
                return false;
            }

            this.session.Add(identifier);
            return true;
        }

        void Revert(string user)
        {
            this.session.ParallelForEach(w => SingleSiteDataLock.Release(w, user, new ForceReleaseOption()));
            this.session.Clear();
        }

        public bool IsLocked(string category, string key, string user, ForceReleaseOption option = null) =>
            SingleSiteDataLock.IsLocked(new LockIdentifier(category, key), user, option ?? DefaultForceReleaseOption);

        public bool Release(string category, string key, string user, ForceReleaseOption option = null)
        {
            var identifier = new LockIdentifier(category, key);
            var released = SingleSiteDataLock.Release(identifier, user, option ?? DefaultForceReleaseOption);
            this.session.Remove(identifier);
            return released;
        }

        public void ReleaseAll(string user, ForceReleaseOption option = null)
        {
            SingleSiteDataLock.ReleaseAll(user, option ?? DefaultForceReleaseOption);
            this.session.Clear();
        }

        public List<LockDetail> GetView() =>
            SingleSiteDataLock.Locks.Select(s => new LockDetail
            {
                Category = s.Key.Category,
                Key = s.Key.Key,
                User = s.Value.User,
                Time = s.Value.Time
            }).ToList();
    }
}