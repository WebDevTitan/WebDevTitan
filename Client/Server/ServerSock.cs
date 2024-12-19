using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Protocol;

namespace Project.Server
{
    public delegate void ProcessPacket(NetPacket NetData, object param);
    public class ServerSock
    {
        private const int MAX_BUFFER_SIZE = 10240;
        public Socket BaseSock
        {
            get;
            set;
        }

        public IPAddress ServerIP { get; set; }
        public int Port { get; set; }
        public bool AsyncSend
        {
            get;
            set;
        }
        public DateTime ConnectedTime
        {
            get;
            set;
        }
        public int ConnectionDeadSeconds
        {
            get
            {
                TimeSpan span = DateTime.Now.Subtract(m_lastReceivedTime);
                return (int)span.TotalSeconds;
            }
        }
        private DateTime m_lastReceivedTime = DateTime.Now;

        public AsyncCallback FuncAsyncConnect
        {
            get;
            set;
        }
        public AsyncCallback FuncAsyncReceive
        {
            get;
            set;
        }
        public AsyncCallback FuncAsyncDisconnect
        {
            get;
            set;
        }
        public AsyncCallback FuncAsyncSend
        {
            get;
            set;
        }

        public object AsyncParam
        {
            get;
            set;
        }

        public ProcessPacket FuncAnalyzeData
        {
            get;
            set;
        }

        public EndPoint RemoteEndPoint
        {
            get
            {
                if (this.BaseSock == null)
                    return null;
                return this.BaseSock.RemoteEndPoint;
            }
        }
        private byte[] _receiveBuffer = new byte[MAX_BUFFER_SIZE];
        private List<byte> _totalByteBuffer = new List<byte>();
        public byte[] Buff = new byte[0x400];

        public ServerSock()
        {

        }

        public ServerSock(IPAddress serverip, ushort nPort)
            : this()
        {
            try
            {
                ServerIP = serverip;
                Port = nPort;
            }
            catch
            {
            }
        }

        public ServerSock(string strServerIP, ushort nPort)
            : this()
        {
            try
            {
                ServerIP = IPAddress.Parse(strServerIP);
                Port = nPort;
            }
            catch
            {
            }
        }

        public void BeginReceive()
        {
            try
            {
                this.ConnectedTime = DateTime.Now;
                this.m_lastReceivedTime = DateTime.Now;
                if (this.FuncAsyncReceive == null)
                    this.FuncAsyncReceive = new AsyncCallback(this.OnReceive);

                if (this.BaseSock != null)
                    this.BaseSock.BeginReceive(_receiveBuffer, 0, MAX_BUFFER_SIZE, SocketFlags.None, this.FuncAsyncReceive, this.AsyncParam);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ClientSocket::BeginReceive Error: ", ex.Message);
            }
        }

        protected virtual void AnalyzePacket(NetPacket netPacket, object param)
        {
            if (!(param is UserInfo))
            {
                return;
            }

            UserInfo client = (UserInfo)param;
            if (netPacket == null)
            {
                if (client.Status == USERSTATUS.NOLOGIN_STATUS)
                    UserMng.GetInstance().SendToolClose();
            }

            try
            {
                if (netPacket.MsgCode == (ushort)NETMSG_CODE.NETMSG_HEARTBEAT)
                {
                    LogMng.Instance.onWriteStatus(string.Format("[{0}] Got heart beat from server!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    return;
                }
                else if (UserMng.GetInstance().Process.Length - 0xA0 > ((ushort)netPacket.MsgCode))
                {
                    UserMng.GetInstance().Process[(ushort)netPacket.MsgCode - 0xA0](netPacket, client);
                }
            }
            catch (Exception e)
            {

            }
        }

        public bool ConnectToServer()
        {
            if ((this.ServerIP == null) || (this.Port == 0))
            {
                return false;
            }
            if (this.FuncAsyncConnect == null)
            {
                this.FuncAsyncConnect = new AsyncCallback(this.OnConnect);
            }
            this.BaseSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.BaseSock.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            IPEndPoint remoteEP = new IPEndPoint(this.ServerIP, this.Port);
            this.Buff = new byte[0x400];
            this.BaseSock.BeginConnect(remoteEP, this.FuncAsyncConnect, this.AsyncParam);
            return true;
        }


        public void Disconnect()
        {
            lock (this)
            {
                if (this.BaseSock != null)
                {
                    Socket sock = this.BaseSock;
                    this.BaseSock = null;
                    if (this.FuncAsyncDisconnect == null)
                    {
                        this.FuncAsyncDisconnect = new AsyncCallback(this.OnDisconnect);
                    }
                    try
                    {
                        sock.BeginDisconnect(false, this.FuncAsyncDisconnect, sock);
                    }
                    catch
                    {
                        this.FuncAsyncDisconnect(null);
                    }
                }
            }
        }


        public virtual void OnConnect(IAsyncResult ar)
        {
            this.BaseSock.EndConnect(ar);
            if (this.FuncAsyncReceive == null)
            {
                this.FuncAsyncReceive = new AsyncCallback(this.OnReceive);
            }
            if (this.FuncAnalyzeData == null)
            {
                this.FuncAnalyzeData = new ProcessPacket(this.AnalyzePacket);
            }
            if (this.AsyncParam == null)
            {
                this.AsyncParam = this;
            }

            //this.BaseSock.BeginReceive(this._receiveBuffer, 0, MAX_BUFFER_SIZE, SocketFlags.None, this.FuncAsyncReceive, this.AsyncParam);
            BeginReceive();
        }

        public virtual void OnDisconnect(IAsyncResult ar)
        {
            try
            {
                Socket sock = this.BaseSock;
                if (sock != null)
                {
                    sock.EndDisconnect(ar);
                }
            }
            catch
            {
            }
        }

        public virtual void OnReceive(IAsyncResult ar)
        {
            int nReadBytes = 0;
            Socket sock = this.BaseSock;
            if (sock == null)
                return;

            m_lastReceivedTime = DateTime.Now;
            try
            {
                nReadBytes = this.BaseSock.EndReceive(ar);
            }
            catch
            {

            }
            if (nReadBytes == 0)
            {
                if (FuncAnalyzeData != null)
                    this.FuncAnalyzeData(null, this.AsyncParam);
                return;
            }
            _totalByteBuffer.AddRange(SubArray(_receiveBuffer, 0, nReadBytes));
            int headerLen = NetPacket.SIGNATURE_LEN + NetPacket.MSGCODE_LEN + NetPacket.BODY_LEN;

            int pointer = 0;
            while (_totalByteBuffer.Count >= headerLen)
            {
                try
                {
                    pointer = 0;
                    int signature1 = _totalByteBuffer[pointer++];
                    if (signature1 != NetPacket.SIGNATURE1)
                        continue;

                    int signature2 = _totalByteBuffer[pointer++];
                    if (signature2 != NetPacket.SIGNATURE2)
                        continue;

                    int msgCode = readShort(_totalByteBuffer, ref pointer);
                    int bodyLen = readInt(_totalByteBuffer, ref pointer);

                    if (_totalByteBuffer.Count - pointer < bodyLen)
                    {
                        pointer -= headerLen;
                        break;
                    }
                    if (bodyLen <= MAX_BUFFER_SIZE)
                    {
                        NetPacket netPacket = NetPacket.ParsePacket(msgCode, SubArray<byte>(_totalByteBuffer.ToArray(), pointer, bodyLen));
                        if (netPacket != null && FuncAnalyzeData != null)
                            FuncAnalyzeData(netPacket, this.AsyncParam);

                        pointer += bodyLen;
                    }
                }
                catch
                {

                }
                finally
                {
                    _totalByteBuffer.RemoveRange(0, pointer);
                }
            }
            this.BeginReceive();
        }

        public virtual void OnSend(IAsyncResult ar)
        {
            try
            {
                if (ar != null)
                {
                    ((Socket)ar.AsyncState).EndSend(ar);
                }
            }
            catch
            {
            }
        }

        private int readShort(List<byte> byteBuffer, ref int pointer)
        {
            ushort ret = BitConverter.ToUInt16(byteBuffer.ToArray(), pointer);
            pointer += 2;
            return ret;
        }

        private int readInt(List<byte> byteBuffer, ref int pointer)
        {
            int ret = BitConverter.ToInt32(byteBuffer.ToArray(), pointer);
            pointer += 4;
            return ret;
        }

        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public void SendData(byte[] data)
        {
            if (this.BaseSock == null || data.Length == 0)
            {
                return;
            }


            if (FuncAsyncSend == null)
                FuncAsyncSend = new AsyncCallback(this.OnSend);

            if (AsyncSend)
            {
                BaseSock.BeginSend(data, 0, data.Length, SocketFlags.None, this.FuncAsyncSend, this.AsyncParam);
            }
            else
            {
                BaseSock.Send(data);
            }
        }
    }
}
