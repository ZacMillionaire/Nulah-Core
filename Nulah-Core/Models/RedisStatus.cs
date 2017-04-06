using System;

namespace NulahCore.Models
{
    public class RedisStatus
    {
        public int UptimeInSeconds { get; set; }
        public int UsedMemory { get; set; }
        public int UsedMemoryRss { get; set; }
        public int UsedMemoryPeak { get; set; }
        public int TotalSystemMemory { get; set; }
        public int TotalConnectionsReceived { get; set; }
        public int TotalCommandsProcessed { get; set; }
        public int ExpiredKeys { get; set; }
        public int KeyspaceHits { get; set; }
        public int KeyspaceMisses { get; set; }
        public float UsedCpuSys { get; set; }           // in seconds
        public float UsedCpuUser { get; set; }          // in seconds
        public float UsedCpuSysChildren { get; set; }   // in seconds
        public float UsedCpuUserChildren { get; set; }  // in seconds
        public DateTime Updated { get; set; }
        public string NulahStatusVersion { get; set; }
    }
}
