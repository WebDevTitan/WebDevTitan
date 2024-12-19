using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public delegate void onWriteStatusEvent(string status);
    public class LogMng
    {
        private static LogMng s_instance = null;
        public static LogMng Instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = new LogMng();

                return s_instance;
            }
        }

        public onWriteStatusEvent onWriteStatus;

        public LogMng()
        {

        }
    }
}
