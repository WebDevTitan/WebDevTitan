using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

using Protocol;

//Created by Foresight(2016.10.09)
namespace SeastoryServer
{
    public delegate void ProcessPacket(NetPacket NetData, object param);
    public class ClientSocket
    {
        private const int MAX_BUFFER_SIZE = 10240;
        public Socket BaseSock
        {
            get;
            set;
        }
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
        public ClientSocket(Socket sock)
        {
            sock.ReceiveTimeout = 1000;
            sock.SendTimeout = 500;
            sock.NoDelay = true;

            this.BaseSock = sock;
            this.AsyncParam = sock;            
        }

        public ClientSocket(Socket sock, int nBuffSize)
            : this(sock)
        {
            this.BaseSock = sock;
        }

        public ClientSocket(Socket sock, int nBuffSize, AsyncCallback func)
            : this(sock, nBuffSize)
        {
            this.FuncAsyncReceive = func;
        }

        public ClientSocket(Socket sock, int nBuffSize, AsyncCallback func, object param)
            : this(sock, nBuffSize, func)
        {
            this.AsyncParam = param;
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

        virtual public void OnReceive(IAsyncResult ar)
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
                    }
                }
                catch(Exception e)
                {

                }
                finally
                {
                    _totalByteBuffer.RemoveRange(0, pointer);
                }
            }
            this.BeginReceive();
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
        virtual public void OnSend(IAsyncResult ar)
        {
            try
            {
                Socket sock = this.BaseSock;
                sock.EndSend(ar);
            }
            catch
            {
            }
        }
        public void Close()
        {

        }
    }
}
