using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using Protocol;

namespace SeastoryServer
{
    
    class NetData
    {
        private const byte SIGNATURE = 0x80;    // 시그네쳐값
        private const int MAX_DATANUM = 255;    // 최대 자료토막수
        private ArrayList m_arrData = new ArrayList();

        /// <summary>
        /// 보낼때 파켓의 식별값
        /// </summary>
        public int SendPacketID
        {
            set
            {
                if (value < 0)
                    value = 0;
                m_nSendPacketID = value;
            }
            get
            {
                return m_nSendPacketID;
            }
        }
        private int m_nSendPacketID = 0;

        /// <summary>
        /// 마지막으로 수신한 파켓의 식별값
        /// </summary>
        public int LastRecvPacketID
        {
            set
            {
                if (value < 0)
                    value = 0;
                m_nLastRecvPacketID = value;
            }
            get
            {
                return m_nLastRecvPacketID;
            }
        }
        private int m_nLastRecvPacketID = 0;

        /// <summary>
        /// 가변자료부의 자료개수
        /// </summary>
        public int DataNum
        {
            get
            {
                return m_arrData.Count;
            }
        }

        private NETMSG_CODE m_nMsgCode = NETMSG_CODE.NETMSG_CLIENTLOGIN;                // 메세지코드
        public NETMSG_CODE MsgCode
        {
            set
            {
                m_nMsgCode = value;
            }
            get
            {
                return m_nMsgCode;
            }
        }

        private ROOM m_nRoom = ROOM.ROOMFIRST;                   // 방번호
        public ROOM RoomNo
        {
            set
            {
                m_nRoom = value;
            }
            get
            {
                return m_nRoom;
            }
        }

        private byte m_nMachine = 0;                // 기계번호
        public byte MachineNo
        {
            set
            {
                m_nMachine = value;
            }
            get
            {
                return m_nMachine;
            }
        }

        /// <summary>
        /// 수신된 자료를 분석한다.
        /// </summary>
        /// <param name="netdata">수신된 바이트형식의 자료</param>
        public void AnalyzeData(byte[] netdata)
        {
            BinaryReader binReader = new BinaryReader(new MemoryStream(netdata));
            try
            {
                int nDataAmnt = 0;  // 가변자료토막수
                TypeCode typecode;  // 자료토막의 형
                object obj = null;

                if (binReader.ReadByte() != SIGNATURE)    // 시그네쳐검사
                {
                    throw (new Exception("시그네쳐 맞지 않음"));
                }
                this.SendPacketID = binReader.ReadInt32();              // 전송파켓식별값
                this.LastRecvPacketID = binReader.ReadInt32();          // 가장 최근수신한 파켓값
                this.m_nRoom = (ROOM)binReader.ReadByte();              // 방번호
                this.m_nMachine = binReader.ReadByte();                 // 기계번호
                this.MsgCode = (NETMSG_CODE)binReader.ReadByte();       // 메세지코드
                nDataAmnt = binReader.ReadByte();                       // 자료토막수
                for (int i = 0; i < nDataAmnt; i++)
                {
                    typecode = (TypeCode)binReader.ReadByte();
                    obj = ReadObject(typecode, binReader);
                    if (obj != null)
                    {
                        m_arrData.Add(obj);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 자료토막을 해당 인덱스에 삽입한다.
        /// </summary>
        /// <param name="i">설정인덱스</param>
        /// <param name="obj">자료오브젝트</param>
        public void InsertData(int i, object obj)
        {
            if (i > this.DataNum)
            {
                throw (new Exception("설정인덱스번호가 틀림."));
            }
            m_arrData.Insert(i, obj);
        }

        /// <summary>
        /// 자료토막을 맨 뒤에 추가한다.
        /// </summary>
        /// <param name="value">자료오브젝트</param>
        public void Append(object value)
        {
            m_arrData.Add(value);
        }

        /// <summary>
        /// 해당 인덱스의 자료오브젝트를 얻는다.
        /// </summary>
        /// <param name="i">인덱스번호</param>
        /// <returns>자료오브젝트</returns>
        public object GetData(int i)
        {
            if (i >= this.DataNum)
            {
                throw (new Exception("얻으려는 자료토막 번호가 맞지 않습니다."));
            }
            return m_arrData[i];
        }

        /// <summary>
        /// 첫번째 자료토막의 오브젝트를 얻는다.
        /// 얻은 자료는 배렬에서 삭제된다.
        /// </summary>
        /// <returns>자료오브젝트</returns>
        public object Pop()
        {
            if (this.DataNum == 0)
                throw new Exception("자료가 없습니다.");

            object obj = (object)m_arrData[0];
            m_arrData.RemoveAt(0);

            return obj;
        }

        /// <summary>
        /// 해당 인덱스의 자료오브젝트를 얻는다.
        /// 얻은 자료는 배렬에서 삭제된다.
        /// </summary>
        /// <param name="i">인덱스번호</param>
        /// <returns>자료오브젝트</returns>
        public object Pop(int i)
        {
            if (i >= this.DataNum)
            {
                throw (new Exception("얻으려는 자료토막 번호가 맞지 않습니다."));
            }

            object obj = (object)m_arrData[i];
            m_arrData.RemoveAt(i);
            return obj;
        }

        /// <summary>
        /// 설정한 자료들을 정리하여 통신규약에 맞는 파켓완충기를 만든다.
        /// </summary>
        /// <returns>파켓자료</returns>
        public byte[] GetPacketData()
        {
            MemoryStream mstream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(mstream);

            writer.Write(SIGNATURE);                // 식별코드
            writer.Write(this.SendPacketID);        // 전송파켓식별값
            writer.Write(this.LastRecvPacketID);    // 최근수신한 파켓식별값
            writer.Write((byte)m_nRoom);            // 방번호값
            writer.Write(m_nMachine);               // 기계번호값
            writer.Write((byte)MsgCode);            // 메세지코드
            writer.Write((byte)this.DataNum);       // 자료토막개수
            for (int i = 0; i < this.DataNum; i++)
            {
                WriteObject(m_arrData[i], writer);
            }
            writer.Close();
            return mstream.ToArray();
        }

        /// <summary>
        /// 자료들을 모두 삭제한다.
        /// </summary>
        public void ResetData()
        {
            m_arrData = new ArrayList();
        }

        /// <summary>
        /// 바이트스트림에 오브젝트를 자료형에 맞게 써넣는다.
        /// </summary>
        /// <param name="obj">써넣으려는 오브젝트</param>
        /// <param name="writer">쓰기스트림</param>
        private void WriteObject(Object obj, BinaryWriter writer)
        {
            TypeCode tcode = Type.GetTypeCode(obj.GetType());
            byte[] data;

            writer.Write((byte)tcode);

            switch (tcode)
            {
                case TypeCode.Object:
                    if (obj.GetType().Name != "Byte[]")
                    {
                        throw (new Exception("자료형이 맞지 않습니다.(오브젝트형은 바이트배렬형이여야 합니다.)"));
                    }
                    data = (byte[])obj;
                    writer.Write(data.Length);
                    if (data.Length > 0)
                        writer.Write(data);
                    break;
                case TypeCode.Boolean:
                    writer.Write((Boolean)obj);
                    break;
                case TypeCode.Char:
                    writer.Write((Char)obj);
                    break;
                case TypeCode.SByte:
                    writer.Write((SByte)obj);
                    break;
                case TypeCode.Byte:
                    writer.Write((Byte)obj);
                    break;
                case TypeCode.Int16:
                    writer.Write((Int16)obj);
                    break;
                case TypeCode.UInt16:
                    writer.Write((UInt16)obj);
                    break;
                case TypeCode.Int32:
                    writer.Write((Int32)obj);
                    break;
                case TypeCode.UInt32:
                    writer.Write((UInt32)obj);
                    break;
                case TypeCode.Int64:
                    writer.Write((Int64)obj);
                    break;
                case TypeCode.UInt64:
                    writer.Write((UInt64)obj);
                    break;
                case TypeCode.Single:
                    writer.Write((Single)obj);
                    break;
                case TypeCode.Double:
                    writer.Write((Double)obj);
                    break;
                case TypeCode.String:
                    {
                        data = Converter.GetInstance().GetBytes((string)obj);
                        writer.Write(data.Length);

                        if (data.Length > 0)
                            writer.Write(data);
                    }
                    break;
                case TypeCode.DateTime:
                    obj = null;
                    break;
            }
        }

        /// <summary>
        /// 바이트스트림으로부터 자료형에 맞는 오브젝트를 읽어낸다.
        /// </summary>
        /// <param name="typecode">읽으려는 오브젝트의 자료형</param>
        /// <param name="reader">읽기스트림</param>
        /// <returns>읽어낸 오브젝트</returns>
        private object ReadObject(TypeCode typecode, BinaryReader reader)
        {
            if (typecode < (TypeCode)1 || typecode > (TypeCode)18)
                return null;

            object obj = null;

            switch (typecode)
            {
                case TypeCode.Object:
                    int len = reader.ReadInt32();
                    if (len == 0)
                        obj = new byte[0];
                    else
                        obj = reader.ReadBytes(len);
                    break;
                case TypeCode.Boolean:
                    obj = reader.ReadBoolean();
                    break;
                case TypeCode.SByte:
                    obj = reader.ReadSByte();
                    break;
                case TypeCode.Byte:
                    obj = reader.ReadByte();
                    break;
                case TypeCode.Char:
                    obj = reader.ReadChar();
                    break;
                case TypeCode.Int16:
                    obj = reader.ReadInt16();
                    break;
                case TypeCode.UInt16:
                    obj = reader.ReadUInt16();
                    break;
                case TypeCode.Int32:
                    obj = reader.ReadInt32();
                    break;
                case TypeCode.UInt32:
                    obj = reader.ReadUInt32();
                    break;
                case TypeCode.Int64:
                    obj = reader.ReadInt64();
                    break;
                case TypeCode.UInt64:
                    obj = reader.ReadUInt64();
                    break;
                case TypeCode.Single:
                    obj = reader.ReadSingle();
                    break;
                case TypeCode.Double:
                    obj = reader.ReadDouble();
                    break;
                case TypeCode.String:
                    int strlen = reader.ReadInt32();
                    if (strlen == 0)
                        obj = "";
                    else
                        obj = Converter.GetInstance().GetString(reader.ReadBytes(strlen));
                    break;
                case TypeCode.DateTime:
                    obj = null;
                    break;
            }

            return obj;
        }
    }
}
