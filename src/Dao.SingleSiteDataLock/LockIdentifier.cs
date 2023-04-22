using System;

namespace Dao.SingleSiteDataLock
{
    class LockIdentifier
    {
        internal LockIdentifier(string category, string key)
        {
            Category = category ?? string.Empty;
            Key = key ?? string.Empty;
        }

        internal string Category { get; }
        internal string Key { get; }

        public override int GetHashCode()
        {
            var hash1 = StringComparer.OrdinalIgnoreCase.GetHashCode(Category);
            var hash2 = StringComparer.OrdinalIgnoreCase.GetHashCode(Key);
            return hash1.CombineHash(hash2);
        }

        public override bool Equals(object obj) =>
            obj is LockIdentifier identifier
            && string.Equals(Category, identifier.Category, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Key, identifier.Key, StringComparison.OrdinalIgnoreCase);
    }
}