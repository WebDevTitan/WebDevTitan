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
        private const byte SIGNATURE = 0x80;    // �ñ׳��İ�
        private const int MAX_DATANUM = 255;    // �ִ� �ڷ��丷��
        private ArrayList m_arrData = new ArrayList();

        /// <summary>
        /// ������ ������ �ĺ���
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
        /// ���������� ������ ������ �ĺ���
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
        /// �����ڷ���� �ڷᰳ��
        /// </summary>
        public int DataNum
        {
            get
            {
                return m_arrData.Count;
            }
        }

        private NETMSG_CODE m_nMsgCode = NETMSG_CODE.NETMSG_CLIENTLOGIN;                // �޼����ڵ�
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

        private ROOM m_nRoom = ROOM.ROOMFIRST;                   // ���ȣ
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

        private byte m_nMachine = 0;                // ����ȣ
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
        /// ���ŵ� �ڷḦ �м��Ѵ�.
        /// </summary>
        /// <param name="netdata">���ŵ� ����Ʈ������ �ڷ�</param>
        public void AnalyzeData(byte[] netdata)
        {
            BinaryReader binReader = new BinaryReader(new MemoryStream(netdata));
            try
            {
                int nDataAmnt = 0;  // �����ڷ��丷��
                TypeCode typecode;  // �ڷ��丷�� ��
                object obj = null;

                if (binReader.ReadByte() != SIGNATURE)    // �ñ׳��İ˻�
                {
                    throw (new Exception("�ñ׳��� ���� ����"));
                }
                this.SendPacketID = binReader.ReadInt32();              // �������Ͻĺ���
                this.LastRecvPacketID = binReader.ReadInt32();          // ���� �ֱټ����� ���ϰ�
                this.m_nRoom = (ROOM)binReader.ReadByte();              // ���ȣ
                this.m_nMachine = binReader.ReadByte();                 // ����ȣ
                this.MsgCode = (NETMSG_CODE)binReader.ReadByte();       // �޼����ڵ�
                nDataAmnt = binReader.ReadByte();                       // �ڷ��丷��
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
        /// �ڷ��丷�� �ش� �ε����� �����Ѵ�.
        /// </summary>
        /// <param name="i">�����ε���</param>
        /// <param name="obj">�ڷ������Ʈ</param>
        public void InsertData(int i, object obj)
        {
            if (i > this.DataNum)
            {
                throw (new Exception("�����ε�����ȣ�� Ʋ��."));
            }
            m_arrData.Insert(i, obj);
        }

        /// <summary>
        /// �ڷ��丷�� �� �ڿ� �߰��Ѵ�.
        /// </summary>
        /// <param name="value">�ڷ������Ʈ</param>
        public void Append(object value)
        {
            m_arrData.Add(value);
        }

        /// <summary>
        /// �ش� �ε����� �ڷ������Ʈ�� ��´�.
        /// </summary>
        /// <param name="i">�ε�����ȣ</param>
        /// <returns>�ڷ������Ʈ</returns>
        public object GetData(int i)
        {
            if (i >= this.DataNum)
            {
                throw (new Exception("�������� �ڷ��丷 ��ȣ�� ���� �ʽ��ϴ�."));
            }
            return m_arrData[i];
        }

        /// <summary>
        /// ù��° �ڷ��丷�� ������Ʈ�� ��´�.
        /// ���� �ڷ�� ��Ŀ��� �����ȴ�.
        /// </summary>
        /// <returns>�ڷ������Ʈ</returns>
        public object Pop()
        {
            if (this.DataNum == 0)
                throw new Exception("�ڷᰡ �����ϴ�.");

            object obj = (object)m_arrData[0];
            m_arrData.RemoveAt(0);

            return obj;
        }

        /// <summary>
        /// �ش� �ε����� �ڷ������Ʈ�� ��´�.
        /// ���� �ڷ�� ��Ŀ��� �����ȴ�.
        /// </summary>
        /// <param name="i">�ε�����ȣ</param>
        /// <returns>�ڷ������Ʈ</returns>
        public object Pop(int i)
        {
            if (i >= this.DataNum)
            {
                throw (new Exception("�������� �ڷ��丷 ��ȣ�� ���� �ʽ��ϴ�."));
            }

            object obj = (object)m_arrData[i];
            m_arrData.RemoveAt(i);
            return obj;
        }

        /// <summary>
        /// ������ �ڷ���� �����Ͽ� ��űԾ࿡ �´� ���Ͽ���⸦ �����.
        /// </summary>
        /// <returns>�����ڷ�</returns>
        public byte[] GetPacketData()
        {
            MemoryStream mstream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(mstream);

            writer.Write(SIGNATURE);                // �ĺ��ڵ�
            writer.Write(this.SendPacketID);        // �������Ͻĺ���
            writer.Write(this.LastRecvPacketID);    // �ֱټ����� ���Ͻĺ���
            writer.Write((byte)m_nRoom);            // ���ȣ��
            writer.Write(m_nMachine);               // ����ȣ��
            writer.Write((byte)MsgCode);            // �޼����ڵ�
            writer.Write((byte)this.DataNum);       // �ڷ��丷����
            for (int i = 0; i < this.DataNum; i++)
            {
                WriteObject(m_arrData[i], writer);
            }
            writer.Close();
            return mstream.ToArray();
        }

        /// <summary>
        /// �ڷ���� ��� �����Ѵ�.
        /// </summary>
        public void ResetData()
        {
            m_arrData = new ArrayList();
        }

        /// <summary>
        /// ����Ʈ��Ʈ���� ������Ʈ�� �ڷ����� �°� ��ִ´�.
        /// </summary>
        /// <param name="obj">��������� ������Ʈ</param>
        /// <param name="writer">���⽺Ʈ��</param>
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
                        throw (new Exception("�ڷ����� ���� �ʽ��ϴ�.(������Ʈ���� ����Ʈ������̿��� �մϴ�.)"));
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
        /// ����Ʈ��Ʈ�����κ��� �ڷ����� �´� ������Ʈ�� �о��.
        /// </summary>
        /// <param name="typecode">�������� ������Ʈ�� �ڷ���</param>
        /// <param name="reader">�б⽺Ʈ��</param>
        /// <returns>�о ������Ʈ</returns>
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