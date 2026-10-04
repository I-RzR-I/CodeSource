using System;

namespace Tests.Support
{
    internal sealed class ScanAbortedException : Exception
    {
        public ScanAbortedException()
            : base("OnError asked the scan to stop.")
        {
        }
    }
}
