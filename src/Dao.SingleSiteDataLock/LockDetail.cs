using System;
using System.Collections.Generic;

namespace Dao.SingleSiteDataLock
{
    public class LockDetail
    {
        public string Key { get; set; }
        public LockView LockView { get; set; }
    }

    public class LockView
    {
        public string WriterUser { get; set; }
        public DateTime? WriterTime { get; set; }

        public List<ReaderLockDetail> ReaderView { get; set; }
    }

    public class ReaderLockDetail
    {
        public string ReaderUser { get; set; }
        public DateTime ReaderTime { get; set; }
    }
}