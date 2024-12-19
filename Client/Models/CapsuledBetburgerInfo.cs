using Protocol;

namespace Project.Models
{
    public class CapsuledBetburgerInfo
    {
        public BetburgerInfo betburgerInfo;
        public PROCESS_RESULT result;

        public CapsuledBetburgerInfo(BetburgerInfo info)
        {
            betburgerInfo = info;
            result = PROCESS_RESULT.SUCCESS; //initial status
        }
    }
}
