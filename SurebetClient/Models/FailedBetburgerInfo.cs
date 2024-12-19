using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
