using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CASRVCoreInterfaces;
using NLog;
using XLiFrameWork;
using URSV1xx.Protocol;
using URSV1xx.Extensions;

namespace URSV1xx
{
    internal class ModBusTransport
    {
        int ReadTimeoutMs = 16000;

        private readonly ITransparentTransport _stream;

        private ILogger _logger;

        private int millisecondsTimeout = 3000;

        private bool _isDisconnected = false;

        private bool _isCancelled = false;

        private bool _isDisposed = false;
        private uint defaultAttemptsCount = 5;

        private readonly ConcurrentDictionary<Guid, DateTime> _cancellationList;

        private Guid taskId;
        public ModBusTransport(ILogger logger, ConcurrentDictionary<Guid, DateTime> cancellationList, ITransparentTransport stream, Guid _taskid)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cancellationList = cancellationList ?? throw new ArgumentNullException(nameof(cancellationList));
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));

            taskId = _taskid;

            _logger.Debug($"Transport instance for {taskId} was created");
        }
        private byte[] CreateModBusMessage(byte aAdress, byte aFunctionCode, byte[] byteMessage )
        {
            return new Modbus.ModbusRTUPacketStruct(aAdress, aFunctionCode, byteMessage, Enums.Endianness.Little).ToArray();
        }
        
        public bool SendMessage(byte aAdress, byte aFunctionCode, byte[] byteMessage)
        {
            var message = CreateModBusMessage(aAdress, aFunctionCode, byteMessage);

            if (_stream.Active && (message?.Length > 0) && !_isDisconnected)
            {
                try
                {
                    _logger.Trace($"URSV-1xx ({message.Length}) >> {BitConverter.ToString(message)}");
                    _stream.Write(message);

                    Thread.Sleep(millisecondsTimeout);
                    return true;

                }
                catch (NullReferenceException)
                {
                    _isDisconnected = true;
                    throw;
                }
            }
            return false;

        }

        public bool ReceiveResponse(typeRegister type, byte aCode, out List<string> response, int countRegisters = 1)
        {
            var receiveBuffer = new byte[512];
            var responseBuffer = new List<byte>();

            while (_stream.Active && !isCancelled && (ReadTimeoutMs >= 0))
            {
                int dataAvailable = _stream.Available;
                if (dataAvailable > 0)
                {
                    int bytesRead = _stream.Read(receiveBuffer, 0, Math.Min(dataAvailable, receiveBuffer.Length));
                    if (bytesRead > 0)
                    {
                        _logger.Trace($"URSV-1xx ({bytesRead}) << {BitConverter.ToString(receiveBuffer, 0, bytesRead)}");
                        responseBuffer.AddRange(receiveBuffer.Take(bytesRead));

                        if (countRegisters > 1)
                        {
                            if (ProcessResponse(responseBuffer, type, aCode ,out List<string> ans))
                            {
                                response = ans;
                                return true;
                            }
                                
                        }
                        else if (countRegisters == 1)
                        {
                            if(ProcessResponse(responseBuffer, type, aCode, out string ans))
                            {
                                List<string> answ = new List<string>{
                                    ans};
                                response = answ;
                                return true;
                            }
                        }
                    }
                }
                Thread.Sleep(millisecondsTimeout);
                ReadTimeoutMs -= millisecondsTimeout;
            }
            response = null;
            return false;
        }
        /// <summary>
        /// Осуществляет обработку ответа с одним регистром
        /// </summary>
        /// <param name="aResponseMessage"></param>
        /// <param name="type"></param>
        /// <param name="answerBody"></param>
        /// <returns></returns>
        private bool ProcessResponse(List<byte> aResponseMessage, typeRegister type, byte funcCode, out string answerBody)
        {
            var aFuncCode = aResponseMessage[1];
            if (aFuncCode != funcCode)
            {
                answerBody = null;
                return false;
            }
            aResponseMessage.RemoveRange(0,3);
            aResponseMessage.RemoveRange(aResponseMessage.Count-2, 2);
            switch ((int)type)
            {
                case 0:
                    answerBody = aResponseMessage.XFGetUInt16(0, true).ToString();
                    return true;
                case 1:
                    answerBody = aResponseMessage.XFGetSingle(0, true).ToString();
                    return true;
                case 2:
                    answerBody = aResponseMessage.XFGetUInt32(0, true).ToString();
                    return true;
                case 3:
                    answerBody = aResponseMessage.XFGetInt32(0, true).ToString();
                    return true;
                case 4:
                    var value = aResponseMessage.XFGetInt32(0, true);
                    answerBody = $"{Convert.ToDouble(value) / 3600}";
                    return true;
                case 5:
                    aResponseMessage.RemoveRange(aResponseMessage.Count() - 7, 7);
                    answerBody = Encoding.GetEncoding("windows-1251").GetString(aResponseMessage.ToArray());
                    return true;
                case 6:
                    value = aResponseMessage.XFGetInt32(0, true);
                    DateTime dt = new DateTime(1970, 1, 1);
                    answerBody = (dt.AddSeconds(value).ToString());
                    return true;
                case 7:
                    var longPart = aResponseMessage.GetRange(0, 4);
                    var floatPart = aResponseMessage.GetRange(4, 4);

                    answerBody = ((ArrayExtensions.XFGetInt32(longPart, 0, true) + ArrayExtensions.XFGetSingle(floatPart, 0, true)).ToString());
                    return true;
                case 8:
                    answerBody = GenerateBitMask(aResponseMessage.XFGetUInt16(0, true));
                    return true;
            }
            answerBody = null;
            return false;
            
        }
        /// <summary>
        /// Осуществляет обработку ответа с несколькими регистрами
        /// </summary>
        /// <param name="aResponseMessage"></param>
        /// <param name="type"></param>
        /// <param name="answerBody"></param>
        /// <returns>Лист значений</returns>
        private bool ProcessResponse(List<byte> aResponseMessage, typeRegister type, byte funcCode ,out List<string> answerBody)
        {
            answerBody = new List<string>();
            var responseBuffer = aResponseMessage;
            var aFuncCode = responseBuffer[1];

            if (aFuncCode != funcCode)
            {
                answerBody = null;
                return false;
            }

            responseBuffer.RemoveRange(0, 3);
            responseBuffer.RemoveRange(responseBuffer.Count - 2, 2);

            switch ((int)type)
            {
                case 0:
                    for(int i = 0; i < responseBuffer.Count; i += 2)
                    {
                        answerBody.Add(responseBuffer.GetRange(i, 2).XFGetUInt16(0, true).ToString());
                    }
                    return true;
                case 1:
                    for (int i = 0; i < responseBuffer.Count; i += 4)
                    {
                        answerBody.Add(responseBuffer.GetRange(i, 4).XFGetSingle(0, true).ToString());
                    }
                    return true;
                case 2:
                    for (int i = 0; i < responseBuffer.Count; i += 4)
                    {
                        answerBody.Add(responseBuffer.GetRange(i, 4).XFGetUInt32(0, true).ToString());
                    }
                    return true;
                case 3:
                    for (int i = 0; i < responseBuffer.Count; i += 4)
                    {
                        answerBody.Add(responseBuffer.GetRange(i, 4).XFGetInt32(0, true).ToString());
                    }
                    return true;
                case 4:
                    for (int i = 0; i < responseBuffer.Count; i += 4)
                    {
                        var value = responseBuffer.GetRange(i, 4).XFGetInt32(0, true);
                        string date = $"{value / 3600}";
                        answerBody.Add(date);
                    }
                    return true;
                case 6:
                    for (int i = 0; i < responseBuffer.Count; i += 4)
                    {
                        var value = responseBuffer.GetRange(i, 4).XFGetInt32(0, true);
                        DateTime dt = new DateTime(1970,1,1);
                        answerBody.Add(dt.AddSeconds(value).ToString());
                    }
                    return true;
                case 7:
                    for (int i = 0; i < responseBuffer.Count; i += 8)
                    {
                        var longPart = responseBuffer.GetRange(i, 4);
                        var floatPart = responseBuffer.GetRange(i+4, 4);
                        
                        answerBody.Add((ArrayExtensions.XFGetInt32(longPart, 0, true) + ArrayExtensions.XFGetSingle(floatPart, 0, true)).ToString());
                    }
                    return true;
                case 8:
                    for(int i = 0;i < responseBuffer.Count;i+= 2)
                    {
                        answerBody.Add(GenerateBitMask(responseBuffer.GetRange(i, 2).XFGetUInt16(0, true)));
                    }
                    return true;
            }
            return false;
        }
        private string GenerateBitMask(int num)
        {
            string bitMask = Convert.ToString(num, 2);
            if (bitMask.Length >= 12)
            { _logger.Trace("Битовая маска слишком большая"); return null; }
            while (bitMask.Length != 11)
            {
                bitMask = "0" + bitMask;
            }
            return bitMask;
        }
        public bool OpenSession(byte aAdress, out int aNetWorkAdress)
        {
            try
            {
                for (uint attempt = 0; attempt < defaultAttemptsCount && !_isCancelled; attempt++)
                {
                    if (SendMessage(aAdress, (byte)aCodes.ReadHoldingRegisters, new uint[] { 0, 1 }.ToByteMessageBody()))
                    {
                        if (ReceiveResponse(typeRegister._int, (byte)aCodes.ReadHoldingRegisters, out List<string> _aNetWorkAdress))
                        {
                            aNetWorkAdress = Convert.ToInt32(_aNetWorkAdress[0]);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Trace(ex);
            }
            aNetWorkAdress = 0;
            return false;
        }

        public void Dispose()
        {
            if(!_isDisposed)
            {
                _stream.Close();
                _isDisposed = true;

                _logger.Debug($"Transport instance for {taskId} was disposed");
            }

            GC.SuppressFinalize(this);
        }

        protected bool isCancelled
        {
            get
            {
                if (_isCancelled)
                {
                    return true;
                }
                else if (_cancellationList.ContainsKey(taskId))
                {
                    _logger.Debug($"Операция {taskId} отменена пользователем!");
                    _isCancelled = true;

                    _cancellationList.TryRemove(taskId, out _);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
