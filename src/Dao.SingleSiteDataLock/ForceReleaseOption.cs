namespace Dao.SingleSiteDataLock
{
    public class ForceReleaseOption
    {
        public bool HasPrivilege { get; set; }

        int writerTimeoutSeconds;
        public int WriterTimeoutSeconds
        {
            get => this.writerTimeoutSeconds;
            set => this.writerTimeoutSeconds = value < 0 ? 0 : value;
        }

        int readerTimeoutSeconds;
        public int ReaderTimeoutSeconds
        {
            get => this.readerTimeoutSeconds;
            set => this.readerTimeoutSeconds = value < 0 ? 0 : value;
        }
    }
}