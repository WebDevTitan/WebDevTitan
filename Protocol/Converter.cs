using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Protocol
{
    public class Converter
    {
        private static Converter m_instance = null;

        public void DecryptData(byte[] data, byte[] deckey)
        {
            if ((deckey != null) && (deckey.Length != 0))
            {
                int length = deckey.Length;
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)(data[i] ^ deckey[i % length]);
                }
            }
        }

        public void EncryptData(byte[] data, byte[] enckey)
        {
            if ((enckey != null) && (enckey.Length != 0))
            {
                int length = enckey.Length;
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)(data[i] ^ enckey[i % length]);
                }
            }
        }

        public byte GetByte(byte[] param)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(param));
            return reader.ReadByte();
        }

        public byte[] GetBytes(byte nParam)
        {
            byte[] buffer = new byte[1];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(buffer));
            writer.Write(nParam);
            writer.Close();
            return buffer;
        }

        public byte[] GetBytes(short nParam)
        {
            byte[] buffer = new byte[2];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(buffer));
            writer.Write(nParam);
            writer.Close();
            return buffer;
        }

        public byte[] GetBytes(int nParam)
        {
            byte[] buffer = new byte[4];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(buffer));
            writer.Write(nParam);
            writer.Close();
            return buffer;
        }

        public byte[] GetBytes(string strParam)
        {
            return Encoding.ASCII.GetBytes(strParam);
        }

        public byte[] GetBytes(ushort nParam)
        {
            byte[] buffer = new byte[2];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(buffer));
            writer.Write(nParam);
            writer.Close();
            return buffer;
        }

        public byte[] GetBytes(uint nParam)
        {
            byte[] buffer = new byte[4];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(buffer));
            writer.Write(nParam);
            writer.Close();
            return buffer;
        }

        public byte[] GetBytes(string strParam, ENCODINGFMT encFMT)
        {
            switch (encFMT)
            {
                case ENCODINGFMT.UNICODE:
                    return Encoding.Unicode.GetBytes(strParam);

                case ENCODINGFMT.UTF7:
                    return Encoding.UTF7.GetBytes(strParam);

                case ENCODINGFMT.UTF8:
                    return Encoding.UTF8.GetBytes(strParam);

                case ENCODINGFMT.UTF32:
                    return Encoding.UTF32.GetBytes(strParam);
            }
            return Encoding.ASCII.GetBytes(strParam);
        }

        public static Converter GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = new Converter();
            }
            return m_instance;
        }

        public short GetInt16(byte[] param)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(param));
            return reader.ReadInt16();
        }

        public int GetInt32(byte[] param)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(param));
            return reader.ReadInt32();
        }

        public string GetString(byte[] param)
        {
            return Encoding.Unicode.GetString(param);
        }

        public string GetString(byte[] param, ENCODINGFMT encFMT)
        {
            switch (encFMT)
            {
                case ENCODINGFMT.UNICODE:
                    return Encoding.Unicode.GetString(param);

                case ENCODINGFMT.UTF7:
                    return Encoding.UTF7.GetString(param);

                case ENCODINGFMT.UTF8:
                    return Encoding.UTF8.GetString(param);

                case ENCODINGFMT.UTF32:
                    return Encoding.UTF32.GetString(param);
            }
            return Encoding.ASCII.GetString(param);
        }

        public ushort GetUInt16(byte[] param)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(param));
            return reader.ReadUInt16();
        }

        public uint GetUInt32(byte[] param)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(param));
            return reader.ReadUInt32();
        }

        // Convert an object to a byte array
        public byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        // Convert a byte array to an Object
        public Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);

            return obj;
        }
    }
}
