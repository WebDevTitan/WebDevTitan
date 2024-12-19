using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Project.Helphers;
using Protocol;

namespace Project.Server
{
    public class NetPacket
    {
        public const byte SIGNATURE1 = 0x86;
        public const byte SIGNATURE2 = 0x7b;
        private const int MAX_DATANUM = 255;

        public const int SIGNATURE_LEN = 2;
        public const int MSGTYPE_LEN = 1;
        public const int MSGCLASS_LEN = 1;
        public const int MSGCODE_LEN = 2;
        public const int BODY_LEN = 4;
        public const int METANUM_LEN = 1;

        private ArrayList m_arrData = new ArrayList();

        /// <summary>
        /// Identification value of the packet when sending
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
        /// The identifier of the last received packet
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
        /// Number of data in variable data section
        /// </summary>
        public int DataNum
        {
            get
            {
                return m_arrData.Count;
            }
        }

        private ushort m_nMsgCode = (ushort)NETMSG_CODE.NETMSG_CLIENTLOGIN;                // message code
        public ushort MsgCode
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

        /// <summary>
        /// Analyze the received data.
        /// </summary>
        /// <param name="NetPacket">Received data in byte format</param>
        public static NetPacket ParsePacket(int msgCode, byte[] NetPacket)
        {
            BinaryReader binReader = new BinaryReader(new MemoryStream(NetPacket));
            try
            {
                NetPacket netPacket = new NetPacket();
                netPacket.MsgCode = (ushort)msgCode;

                int nDataCount = binReader.ReadInt16();
                List<int> dataTypes = new List<int>();
                for (int i = 0; i < nDataCount; i++)
                {
                    int nMetaDataInfo = binReader.ReadInt16();
                    dataTypes.Add(nMetaDataInfo & 0xFF);
                }
                netPacket.m_arrData.Clear();
                for (int i = 0; i < dataTypes.Count; i++)
                {
                    switch (dataTypes[i])
                    {
                        case 0:
                            UInt32 uintData = binReader.ReadUInt32();
                            netPacket.m_arrData.Add(uintData);
                            break;
                        case 1:
                            UInt16 strLen = binReader.ReadUInt16();
                            string strData = Encoding.UTF8.GetString(binReader.ReadBytes(strLen));
                            netPacket.m_arrData.Add(strData);
                            break;
                        case 2:
                            Int32 intData = binReader.ReadInt32();
                            netPacket.m_arrData.Add(intData);
                            break;
                        case 3:
                            UInt16 ushortData = binReader.ReadUInt16();
                            netPacket.m_arrData.Add(ushortData);
                            break;
                        case 4:
                            Int16 shortData = binReader.ReadInt16();
                            netPacket.m_arrData.Add(shortData);
                            break;
                        case 5:
                            float floatData = binReader.ReadSingle();
                            netPacket.m_arrData.Add(floatData);
                            break;
                        case 6:
                            double doubleData = binReader.ReadDouble();
                            netPacket.m_arrData.Add(doubleData);
                            break;
                        case 7:
                            Boolean booleanData = binReader.ReadBoolean();
                            netPacket.m_arrData.Add(booleanData);
                            break;
                        case 8:
                            Byte byteData = binReader.ReadByte();
                            netPacket.m_arrData.Add(byteData);
                            break;
                        case 9:
                            UInt16 objLen = binReader.ReadUInt16();
                            byte[] objData = binReader.ReadBytes(objLen);
                            netPacket.m_arrData.Add(objData);
                            break;
                    }
                }
                return netPacket;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// Insert a piece of data into the given index.
        /// </summary>
        /// <param name="i">set index</param>
        /// <param name="obj">data object  </param>
        public void InsertData(int i, object obj)
        {
            if (i > this.DataNum)
            {
                throw (new Exception("The configuration index number is incorrect."));
            }
            m_arrData.Insert(i, obj);
        }

        /// <summary>
        /// Add a piece of data to the end.
        /// </summary>
        /// <param name="value">Data object</param>
        public void Append(object value)
        {
            m_arrData.Add(value);
        }

        /// <summary>
        /// Get the data object at the given index.
        /// </summary>
        /// <param name="i">Index number</param>
        /// <returns>Data object</returns>
        public object GetData(int i)
        {
            if (i >= this.DataNum)
            {
                throw (new Exception("The number of the data fragment you are trying to retrieve is incorrect."));
            }
            return m_arrData[i];
        }

        /// <summary>
        /// Get the object of the first piece of data.
        /// The obtained data is deleted from the array.
        /// </summary>
        /// <returns>Data object</returns>
        public object Pop()
        {
            if (this.DataNum == 0)
                throw new Exception("there is no data.");

            object obj = (object)m_arrData[0];
            m_arrData.RemoveAt(0);

            return obj;
        }

        /// <summary>
        /// Get the data object at the given index.
        /// The obtained data is deleted from the array.
        /// </summary>
        /// <param name="i">Index number</param>
        /// <returns>Data object</returns>
        public object Pop(int i)
        {
            if (i >= this.DataNum)
            {
                throw (new Exception("The number of the data fragment you are trying to retrieve is incorrect."));
            }

            object obj = (object)m_arrData[i];
            m_arrData.RemoveAt(i);
            return obj;
        }

        /// <summary>
        /// Organize the set data to create a packet buffer that complies with the communication protocol.
        /// </summary>
        /// <returns>Pocket data</returns>
        public byte[] GetPacketData()
        {
            byte[] buff;
            MemoryStream mstream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(mstream);

            writer.Write(SIGNATURE1);
            writer.Write(SIGNATURE2);
            writer.Write((ushort)MsgCode);

            int nBodyLen = 0;
            byte[] metaData = GetMetaDataBytes(out nBodyLen);
            nBodyLen += (metaData.Length + 2);

            writer.Write(nBodyLen);
            writer.Write((ushort)m_arrData.Count);
            writer.Write(metaData);
            for (int i = 0; i < m_arrData.Count; i++)
            {
                WriteObject(m_arrData[i], writer);
            }
            buff = new byte[mstream.Length];
            mstream.Seek(0, SeekOrigin.Begin);
            mstream.Read(buff, 0, buff.Length);
            writer.Close();
            return buff;
        }

        private byte[] GetMetaDataBytes(out int bodyLength)
        {
            List<ushort> metaData = new List<ushort>();
            bodyLength = 0;
            for (int i = 0; i < m_arrData.Count; i++)
            {
                object obj = m_arrData[i];
                TypeCode tcode = Type.GetTypeCode(obj.GetType());
                switch (tcode)
                {
                    case TypeCode.Boolean:
                        metaData.Add(7);
                        bodyLength++;
                        break;
                    case TypeCode.SByte:
                        metaData.Add(8);
                        bodyLength++;
                        break;
                    case TypeCode.Byte:
                        metaData.Add(8);
                        bodyLength++;
                        break;
                    case TypeCode.Int16:
                        metaData.Add(4);
                        bodyLength += 2;
                        break;
                    case TypeCode.UInt16:
                        metaData.Add(3);
                        bodyLength += 2;
                        break;
                    case TypeCode.Int32:
                        metaData.Add(2);
                        bodyLength += 4;
                        break;
                    case TypeCode.UInt32:
                        metaData.Add(0);
                        bodyLength += 4;
                        break;
                    case TypeCode.Single:
                        metaData.Add(5);
                        bodyLength += 4;
                        break;
                    case TypeCode.Double:
                        metaData.Add(6);
                        bodyLength += 8;
                        break;
                    case TypeCode.String:
                        metaData.Add(1);
                        bodyLength += (2 + Encoding.UTF8.GetBytes((string)obj).Length);
                        break;
                    case TypeCode.Object:
                        metaData.Add(9);
                        bodyLength += (2 + ((byte[])obj).Length);
                        break;
                }
            }
            byte[] retData = new byte[metaData.Count * 2];
            Buffer.BlockCopy(metaData.ToArray(), 0, retData, 0, retData.Length);
            return retData;
        }

        /// <summary>
        /// Delete all data.
        /// </summary>
        public void ResetData()
        {
            m_arrData = new ArrayList();
        }

        /// <summary>
        /// Writes an object to a byte stream according to its data type.
        /// </summary>
        /// <param name="obj">The object you want to insert</param>
        /// <param name="writer">Writing Stream</param>
        private void WriteObject(Object obj, BinaryWriter writer)
        {
            TypeCode tcode = Type.GetTypeCode(obj.GetType());
            byte[] data;
            switch (tcode)
            {
                case TypeCode.Object:
                    if (obj.GetType().Name != "Byte[]")
                    {
                        throw (new Exception("The data type is incorrect. (The object type must be a byte array type.)"));
                    }
                    data = (byte[])obj;
                    writer.Write((UInt16)data.Length);
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
                        data = Utils.GetBytes((string)obj, ENCODINGFMT.UTF8);
                        writer.Write((UInt16)data.Length);

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
        /// Reads an object of the appropriate type from a byte stream..
        /// </summary>
        /// <param name="typecode">The data type of the object to be read</param>
        /// <param name="reader">Reading Stream</param>
        /// <returns>Read object</returns>
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
                        obj = Utils.GetString(reader.ReadBytes(strlen), ENCODINGFMT.UTF8);
                    break;
                case TypeCode.DateTime:
                    obj = null;
                    break;
            }

            return obj;
        }
    }
}
