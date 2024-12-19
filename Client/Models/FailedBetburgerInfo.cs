using System;
using Protocol;

namespace Project.Models
{
    public class FailedBetburgerInfo
    {
        public BetburgerInfo betburgerInfo;
        public int FailedCount;
        public DateTime NextRetryTime;

        public FailedBetburgerInfo(BetburgerInfo info)
        {
            betburgerInfo = info;
            FailedCount = 0;
            NextRetryTime = DateTime.MinValue;
        }
    }
}
