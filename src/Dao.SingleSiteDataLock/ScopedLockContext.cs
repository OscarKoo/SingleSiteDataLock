using System;
using System.Collections.Generic;
using System.Linq;

namespace Dao.SingleSiteDataLock
{
    public class ScopedLockContext : IDisposable
    {
        readonly Dictionary<string, HashSet<LockIdentifier>> session = new Dictionary<string, HashSet<LockIdentifier>>(StringComparer.OrdinalIgnoreCase);

        public static readonly ForceReleaseOption DefaultForceReleaseOption = new ForceReleaseOption { TimeoutSeconds = 600 };

        public void Dispose() => this.session.Clear();

        public bool TryLock(string category, string key, string user, ForceReleaseOption option = null)
        {
            if (user == null)
                user = string.Empty;

            var identifier = new LockIdentifier(category, key);
            if (!SingleSiteDataLock.TryLock(identifier, user, option ?? DefaultForceReleaseOption))
            {
                Revert(user);
                return false;
            }

            this.session[user].Add(identifier);
            return true;
        }

        public void Revert(string user)
        {
            if (user == null)
                user = string.Empty;

            if (!this.session.TryGetValue(user, out var identifiers))
                return;

            identifiers.ParallelForEach(w => SingleSiteDataLock.Release(w, user, new ForceReleaseOption()));
            this.session.Remove(user);
        }

        public bool IsLocked(string category, string key, string user, ForceReleaseOption option = null) =>
            SingleSiteDataLock.IsLocked(new LockIdentifier(category, key), user, option ?? DefaultForceReleaseOption);

        public bool Release(string category, string key, string user, ForceReleaseOption option = null)
        {
            if (user == null)
                user = string.Empty;

            var identifier = new LockIdentifier(category, key);
            var released = SingleSiteDataLock.Release(identifier, user, option ?? DefaultForceReleaseOption);
            if (this.session.TryGetValue(user, out var identifiers))
            {
                identifiers.Remove(identifier);
                if (identifiers.Count == 0)
                    this.session.Remove(user);
            }

            return released;
        }

        public void ReleaseAll(string user, ForceReleaseOption option = null)
        {
            if (user == null)
                user = string.Empty;

            SingleSiteDataLock.ReleaseAll(user, option ?? DefaultForceReleaseOption);
            this.session.Remove(user);
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