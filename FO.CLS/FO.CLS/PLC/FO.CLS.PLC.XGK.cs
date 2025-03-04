using FO.CLS.LOG;
using System;
using System.Net;
using System.Net.Sockets;

namespace FO.CLS.PLC
{
    public class XGK
    {
        private object lockObject = new object();

        public string plc_ip;
        public int plc_port;
        public string plc_name;

        #region 상수 및 변수
        private const int READBUFFERSIZE = 65536;
        private const int SENDBUFFERSIZE = 50;

        private TcpClient _tcpClient = null;

        public DateTime lastCommTime = new DateTime(2020,01,01,00,00,00);

        // 로그
        Write fOCLSLOGWrite = new Write(null);
        #endregion

        #region 생성자
        public XGK(string ip, string name = "")
        {
            this.plc_ip = ip;
            this.plc_port = 2004;
            this.plc_name = name;
        }
        #endregion

        #region PLC 연동 관련 메서드

        /// <summary>
        /// PLC 연결
        /// </summary>
        /// <param name="sIP">ip address</param>
        /// <param name="iPORT">port</param>
        /// <returns></returns>
        public bool Connect(string sIP, int iPORT)
        {
            try
            {
                if(_tcpClient == null || _tcpClient.Client == null || _tcpClient.Connected == false)
                {
                    _tcpClient = new TcpClient(AddressFamily.InterNetwork);

                    _tcpClient.ReceiveTimeout = 1000;
                    _tcpClient.SendTimeout = 1000;

                    _tcpClient.Connect(IPAddress.Parse(sIP), iPORT);
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return _tcpClient.Connected;
        }

        public bool Connect()
        {
            return Connect(plc_ip, plc_port);
        }

        /// <summary>
        /// PLC 연결 종료
        /// </summary>
        public void Close()
        {
            try
            {
                if(_tcpClient != null && _tcpClient.Client != null && _tcpClient.Client != null)
                {
                    if(_tcpClient.Connected)
                    {
                        _tcpClient.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// PLC 연동 체크
        /// </summary>
        /// <returns></returns>
        public bool ConnectedCheck()
        {
            if(_tcpClient != null && _tcpClient?.Client != null)
            {
                return _tcpClient.Connected;
            }
            else
            {
                return false;
            }
        }

        #endregion




        /// <summary>
        /// 개별읽기
        /// 16개 까지만 읽을수 있음
        /// ReadPlc(new string[] { "%DW0030", "%DW031", "%DW32", "%RW80" });
        /// </summary>  ReadPlc(new string[] { "D0030", "D031", "D32", "R80" });
        /// <param name="address"></param>
        /// <returns></returns>
        public int[] ReadPlcWord(string[] address)
        {
            lock(lockObject)
            {

                int[] result = new int[] { };
                try
                {

                    if(address.Length > 16)
                        return result;

                    NetworkStream _stream = _tcpClient.GetStream();

                    byte[] sendData = new byte[SENDBUFFERSIZE*10]; // 배열 충분히 크게 만들기
                    Array.Clear(sendData, 0, sendData.Length);

                    //LSIS-XGT
                    sendData[0] = 0x4C;
                    sendData[1] = 0x53;
                    sendData[2] = 0x49;
                    sendData[3] = 0x53;
                    sendData[4] = 0x2D;
                    sendData[5] = 0x58;
                    sendData[6] = 0x47;
                    sendData[7] = 0x54;
                    sendData[13] = 0x33;
                    sendData[18] = 0x00;
                    sendData[20] = 0x54;
                    sendData[22] = 0x02;

                    // 블럭수
                    sendData[26] = Convert.ToByte(address.Length);

                    // 읽을 메모리
                    int indexBuffer = 28;
                    for(int i = 0; i < address.Length; i++)
                    {
                        string workAddress = "%" + address[i].Substring(0,1) + "W" + address[i].Substring(1,address[i].Length-1);

                        // 주소의 길이
                        sendData[indexBuffer] = Convert.ToByte(workAddress.Length);

                        // 주소명 복사
                        byte[] srcArray  = System.Text.Encoding.Default.GetBytes(workAddress);
                        srcArray.CopyTo(sendData, indexBuffer + 2);

                        indexBuffer += workAddress.Length + 2;
                    }

                    // 명령어부터 끝까지 길이
                    sendData[16] = Convert.ToByte(indexBuffer - 20);

                    // 체크섬
                    int num = 0;
                    for(int i = 0; i <= 18; i++)
                    {
                        num += (int)sendData[i];

                        if(num > (int)byte.MaxValue)
                            num -= 256;
                    }
                    sendData[19] = Convert.ToByte(num);


                    _stream.Write(sendData, 0, indexBuffer);
                    _stream.Flush();


                    byte[] plcReadData = new byte[READBUFFERSIZE];
                    _stream.Read(plcReadData, 0, _tcpClient.ReceiveBufferSize);

                    result = new int[address.Length];

                    for(int i = 0; i < result.Length; i++)
                    {
                        int ArryIndex = 32 + (i * 4);

                        string convertResult = Convert.ToString(Convert.ToInt32(plcReadData[ArryIndex + 1]), 16).PadLeft(2, '0')
                                             + Convert.ToString(Convert.ToInt32(plcReadData[ArryIndex]), 16).PadLeft(2, '0');


                        result[i] = Convert.ToInt32(convertResult, 16);
                    }

                    lastCommTime = DateTime.Now;

                    return result;
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }

        public int[] ReadPlcWord(string pAddress, int count)
        {
            lock(lockObject)
            {
                int[] result = new int[] { };
                try
                {

                    NetworkStream _stream = _tcpClient.GetStream();

                    byte[] sendData = new byte[SENDBUFFERSIZE*10]; // 배열 충분히 크게 만들기
                    Array.Clear(sendData, 0, sendData.Length);

                    //LSIS-XGT
                    sendData[0] = 0x4C;
                    sendData[1] = 0x53;
                    sendData[2] = 0x49;
                    sendData[3] = 0x53;
                    sendData[4] = 0x2D;
                    sendData[5] = 0x58;
                    sendData[6] = 0x47;
                    sendData[7] = 0x54;
                    sendData[13] = 0x33;
                    sendData[18] = 0x00;
                    sendData[20] = 0x54;
                    sendData[22] = 0x14;

                    // 블럭수
                    sendData[26] = 1;

                    // 주소명 조합
                    string memory = pAddress.Substring(0,1);
                    pAddress = pAddress.Substring(1, pAddress.Length - 1);
                    string address = "%" + memory + "B" + (Convert.ToInt32(pAddress)*2).ToString();

                    // 변수명 길이
                    int addrLen = address.Length;
                    sendData[28] = Convert.ToByte(addrLen);

                    // 주소명 복사
                    byte[] srcArray  = System.Text.Encoding.Default.GetBytes(address);
                    srcArray.CopyTo(sendData, 30);

                    // 명령어부터 끝까지 길이
                    sendData[16] = Convert.ToByte(10 + addrLen + 2);

                    // 읽을 갯수
                    sendData[30 + addrLen] = Convert.ToByte(count * 2);

                    // 체크섬
                    int num = 0;
                    for(int i = 0; i <= 18; i++)
                    {
                        num += (int)sendData[i];

                        if(num > (int)byte.MaxValue)
                            num -= 256;
                    }
                    sendData[19] = Convert.ToByte(num);


                    _stream.Write(sendData, 0, 30 + addrLen + 2);
                    _stream.Flush();


                    byte[] plcReadData = new byte[READBUFFERSIZE];
                    _stream.Read(plcReadData, 0, _tcpClient.ReceiveBufferSize);

                    result = new int[count];

                    for(int i = 0; i < result.Length; i++)
                    {
                        int ArryIndex = 32 + (i * 2);

                        string convertResult = Convert.ToString(Convert.ToInt32(plcReadData[ArryIndex + 1]), 16).PadLeft(2, '0')
                                             + Convert.ToString(Convert.ToInt32(plcReadData[ArryIndex]), 16).PadLeft(2, '0');


                        result[i] = Convert.ToInt32(convertResult, 16);
                    }

                    lastCommTime = DateTime.Now;

                    return result;
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 개별쓰기
        /// 16개 까지만 쓸수있음
        /// WritePlcWord(new string[] { "%DW92" , "%DW0050"}, new int[] { 13, 99 });
        /// </summary>
        /// <param name="_sDataAddress">메모리 번지</param>
        /// <param name="_iWriteData">데이터</param>
        /// <returns></returns>
        public bool WritePlcWord(string[] address, int[] data)
        {
            lock(lockObject)
            {
                try
                {
                    NetworkStream _stream = _tcpClient.GetStream();

                    byte[] sendData = new byte[SENDBUFFERSIZE*10]; // 배열 충분히 크게 만들기

                    //LSIS-XGT
                    sendData[0] = 0x4C;
                    sendData[1] = 0x53;
                    sendData[2] = 0x49;
                    sendData[3] = 0x53;
                    sendData[4] = 0x2D;
                    sendData[5] = 0x58;
                    sendData[6] = 0x47;
                    sendData[7] = 0x54;
                    sendData[13] = 0x33;
                    sendData[18] = 0x00;
                    sendData[20] = 0x58;
                    sendData[22] = 0x02;

                    // 블럭수
                    sendData[26] = Convert.ToByte(address.Length);

                    // 주소
                    int indexBuffer = 28;
                    for(int i = 0; i < address.Length; i++)
                    {
                        string workAddress = "%" + address[i].Substring(0,1) + "W" + address[i].Substring(1,address[i].Length-1);

                        // 주소명 길이
                        sendData[indexBuffer] = Convert.ToByte(workAddress.Length);

                        // 주소명 복사
                        byte[] srcArray  = System.Text.Encoding.Default.GetBytes(workAddress);
                        srcArray.CopyTo(sendData, indexBuffer + 2);

                        indexBuffer += workAddress.Length + 2;
                    }

                    for(int i = 0; i < data.Length; i++)
                    {
                        // 쓸 데이터 크기 - WORD라 2
                        sendData[indexBuffer] = 2;

                        // 데이터 - HI LOW 반전
                        sendData[indexBuffer + 2] = Convert.ToByte(data[i] & 0xff);
                        sendData[indexBuffer + 3] = Convert.ToByte((data[i] >> 8) & 0xff);

                        indexBuffer += 4;
                    }

                    // 명령어부터 끝까지 길이
                    sendData[16] = Convert.ToByte(indexBuffer - 20);

                    // 체크섬
                    int num = 0;
                    for(int i = 0; i <= 18; i++)
                    {
                        num += (int)sendData[i];

                        if(num > (int)byte.MaxValue)
                            num -= 256;
                    }
                    sendData[19] = Convert.ToByte(num);


                    _stream.Write(sendData, 0, indexBuffer);
                    _stream.Flush();


                    byte[] plcReadData = new byte[READBUFFERSIZE];
                    int receiveLength = _stream.Read(plcReadData, 0, _tcpClient.ReceiveBufferSize);

                    if(receiveLength == 0)
                    {
                        return false;
                    }

                    return true;
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }

        public bool WritePlcWord(string pAddress, int data)
        {
            return WritePlcWord(pAddress, new int[] { data });
        }

        public bool WritePlcWord(string pAddress, int[] data)
        {
            lock(lockObject)
            {
                try
                {
                    NetworkStream _stream = _tcpClient.GetStream();

                    byte[] sendData = new byte[SENDBUFFERSIZE*10]; // 배열 충분히 크게 만들기
                    Array.Clear(sendData, 0, sendData.Length);

                    //LSIS-XGT
                    sendData[0] = 0x4C;
                    sendData[1] = 0x53;
                    sendData[2] = 0x49;
                    sendData[3] = 0x53;
                    sendData[4] = 0x2D;
                    sendData[5] = 0x58;
                    sendData[6] = 0x47;
                    sendData[7] = 0x54;
                    sendData[13] = 0x33;
                    sendData[18] = 0x00;
                    sendData[20] = 0x58;
                    sendData[22] = 0x14;

                    // 블럭수
                    sendData[26] = 1;

                    // 주소명 조합
                    string memory = pAddress.Substring(0,1);
                    pAddress = pAddress.Substring(1, pAddress.Length - 1);
                    string address = "%" + memory + "B" + (Convert.ToInt32(pAddress)*2).ToString();

                    // 변수명 길이
                    int addrLen = address.Length;
                    sendData[28] = Convert.ToByte(addrLen);

                    // 주소명 복사
                    byte[] srcArray  = System.Text.Encoding.Default.GetBytes(address);
                    srcArray.CopyTo(sendData, 30);

                    // 명령어부터 끝까지 길이
                    sendData[16] = Convert.ToByte(10 + addrLen + 2 + data.Length * 2);

                    // 쓸 갯수
                    sendData[30 + addrLen] = Convert.ToByte(data.Length * 2);

                    for(int i = 0; i < data.Length; i++)
                    {
                        sendData[30 + addrLen + 2 + i * 2 + 0] = Convert.ToByte((data[i]) & 0xff);
                        sendData[30 + addrLen + 2 + i * 2 + 1] = Convert.ToByte((data[i] >> 8) & 0xff);

                    }

                    // 체크섬
                    int num = 0;
                    for(int i = 0; i <= 18; i++)
                    {
                        num += (int)sendData[i];

                        if(num > (int)byte.MaxValue)
                            num -= 256;
                    }
                    sendData[19] = Convert.ToByte(num);


                    _stream.Write(sendData, 0, 30 + addrLen + 2 + data.Length * 2);
                    _stream.Flush();


                    byte[] plcReadData = new byte[READBUFFERSIZE];
                    int receiveLength = _stream.Read(plcReadData, 0, _tcpClient.ReceiveBufferSize);

                    if(receiveLength == 0)
                    {
                        return false;
                    }

                    return true;
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }


        public bool writePlcBitAfterReadWord(string address, int pos, int data)
        {
            int[] tarray = ReadPlcWord(address, 1);

            if(tarray != null && tarray.Length == 1)
            {
                if(data == 1)
                    tarray[0] |= (0x01 << pos);
                else
                    tarray[0] &= ~(0x01 << pos);

                WritePlcWord(address, tarray);
            }

            return false;
        }

    }
}
