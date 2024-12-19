using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BetburgerServer.Constant
{
    public class FileWriter
    {
        private string filepath = string.Empty;
        private StreamWriter sw = null;
        public FileWriter(string _filepath)
        {
            filepath = _filepath;
        }
        private ReaderWriterLockSlim lock_ = new ReaderWriterLockSlim();
        public bool WriteRow(string writestr)
        {
            bool result = true;
            lock_.EnterWriteLock();
            try
            {
                sw = new StreamWriter(filepath, true, Encoding.ASCII);
                sw.WriteLine(writestr);
                sw.Close();

            }
            catch
            {
                result = false;
            }
            finally
            {
                lock_.ExitWriteLock();
            }
            return result;
        }
    }

}
