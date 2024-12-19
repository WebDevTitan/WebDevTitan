using PinnacleWrapper;
using PinnacleWrapper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.Controller
{
    public class BPScanner
    {
        PinnacleClient client = null;

        public BPScanner()
        {
            client = new PinnacleClient("MG1807553", "Pe040590@#", "GBP", OddsFormat.DECIMAL , null);

        }

        public async Task GetPinnacleLive()
        {
            try
            {
                var response = await client.GetInRunning();
            }
            catch(Exception e)
            {

            }
        }
    }
}
