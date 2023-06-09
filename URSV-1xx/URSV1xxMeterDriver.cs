namespace URSV1xx
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NLog;
    using XLiFrameWork;
    using CASRVCoreInterfaces;
    using TaskCore;
    using System.Collections.Concurrent;
    using System.ComponentModel.Composition;
    using URSV1xx;
    using URSV1xx.Extensions;
    using URSV1xx.Protocol;
    using URSV1xx.Data;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using URSV1xx.Configuration;
    using System.ServiceModel.Description;
    using System.Data.Common;
    using NLog.LayoutRenderers;

    [Export(typeof(IMeterDriver))]
    [ExportMetadata("DeviceModel", "УРСВ-022")]
    [ExportMetadata("ModuleVersion", "1.1.0")]
    [ExportMetadata("SupportedDevices", new int[] { 21080 })]
    [ExportMetadata("DriverVerion", 0)]
    [ExportMetadata("SupportedCommands", new SupportedCommands[]
    {
        SupportedCommands.DEV_DeviceActivation,
        SupportedCommands.DEV_ReadDeviceParameters,
        SupportedCommands.DEV_TVActivation,
        SupportedCommands.DEV_ReadTVParameters,
        SupportedCommands.DEV_ReadData,
        SupportedCommands.DEV_WriteDeviceParameters,
        SupportedCommands.DEV_WriteTVParameters
    })]
    [ExportMetadata("SupportedDataTypes", new SupportedDataTypes[]
    {
        SupportedDataTypes.ARCH_HOUR,
        SupportedDataTypes.ARCH_DAY,
        SupportedDataTypes.ARCH_MONTH,
        SupportedDataTypes.ARCH_CURRENT,
        SupportedDataTypes.ARCH_EVENTS,
        SupportedDataTypes.ARCH_CURRENT_TOTAL,
        SupportedDataTypes.ARCH_NS,
    })]
    public partial class USRV_1xxMeterDriver : IMeterDriver, IDisposable
    {
        private ModBusTransport _transport;
        private ILogger _logger;
        private List<TV> tvList = new List<TV> { };
        private List<TR> trList = new List<TR> { };
        private List<NS> nsList = new List<NS> { };
        private HashSet<DataInternalUOM> uomList = new HashSet<DataInternalUOM>();

        public Result Control(TaskStruct task)
        {
            throw new NotImplementedException();
        }
        public Result RealTimeRead(TaskStruct task)
        {
            throw new NotImplementedException();
        }

        public DeviceTimeSyncEndStructure SyncTime(TaskStruct task)
        {
            throw new NotImplementedException();
        }
        public DeviceWritePropertiesEndStructure WriteDeviceParameters(TaskStruct task)
        {
            throw new NotImplementedException();
        }

        public TVWritePropertiesEndStructure WriteTVParameters(TaskStruct task)
        {
            throw new NotImplementedException();
        }

        public void Init(Guid taskId, ref ExportLifetimeContext<ITransparentTransport> transportHandler, ref FixedSizedQueue<RealTimeValue> realTimeValues, ref ConcurrentDictionary<Guid, ExportLifetimeContext<IMeterDriver>> realTimeOperationList, byte[] intermediateData, ref Logger logger, ref ConcurrentDictionary<Guid, DateTime> cancellationList)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _transport?.Dispose();
            _transport = new ModBusTransport(logger: logger, cancellationList: cancellationList, stream: transportHandler?.Value, _taskid: taskId);

            _logger.Debug($"Module \"УРСВ-1xx\" was initialized (ID = {taskId}; Assembly = {typeof(USRV_1xxMeterDriver).Assembly.GetName().Version})");
        }

        public DeviceActivationEndStructure DeviceActivation(TaskStruct task)
        {
            if (!StartSeance(task.Arguments, out int errorCode, out byte inAddr, out int NetWorkAdress))
                return new DeviceActivationEndStructure { ErrorCode = errorCode, Status = CurrentTaskStatus.Failed, Parameters = new DeviceActivationParametersStructure { UserID = task.UserID } };
            if (!ReadDeviceParameters(inAddr, out List<PropertiesStructure> deviceParameters))
                return new DeviceActivationEndStructure { ErrorCode = 50090, Status = CurrentTaskStatus.Failed, Parameters = new DeviceActivationParametersStructure { UserID = task.UserID } };
            if (!GetSerialNumber(inAddr, task.Arguments, out string serialNumber, out errorCode))
                return new DeviceActivationEndStructure { ErrorCode = errorCode, Status = CurrentTaskStatus.Failed, Parameters = new DeviceActivationParametersStructure { UserID = task.UserID } };
            else
            {
                List<DeviceActivationMeasuringPointStructure> deviceMeasuringPoints = new List<DeviceActivationMeasuringPointStructure>() { new DeviceActivationMeasuringPointStructure { Name = "1" } };
                return new DeviceActivationEndStructure
                {
                    ErrorCode = 0,
                    Status = CurrentTaskStatus.Success,
                    Properties = deviceParameters,
                    MeasuringPoints = deviceMeasuringPoints,
                    Parameters = new DeviceActivationParametersStructure
                    {
                        ConnectedDeviceID = task.ConnectedDeviceID.Value,
                        UserID = task.UserID,
                        SN = serialNumber,
                        NetWorkAddress = NetWorkAdress,
                        TimeRead = DateTime.UtcNow.ToTimeStamp(),
                    }
                };
            }
        }

        private bool StartSeance(IReadOnlyDictionary<string, string> arguments, out int errorCode, out byte inAddr, out int NetWorkAdress)
        {
            _logger.Debug("Установка сеанса связи с УРСВ-1хх ...");
            if (arguments.TryGetAddress(out inAddr))
                if (_transport.OpenSession(inAddr, out NetWorkAdress))
                    if (inAddr == NetWorkAdress)
                    {
                        errorCode = 0;
                        _logger.Debug("Сеанс связи с УРСВ-1хх установлен");
                        return true;
                    }
            errorCode = 50090;
            NetWorkAdress = 0;
            return false;
        }

        public Result ReadData(TaskStruct task)
        {
            task.Arguments.TryGetValue("MeasurePoint", out string mesPoint);
            var deviceParameters = DeviceConfiguration.LoadDeviceConfiguration(task.ConnectedDeviceID, _logger).ToList();

            var archType = 255;
            switch((SupportedDataTypes?)task.TypeDataCode)
            {
                case SupportedDataTypes.ARCH_HOUR: archType = 0; break;
                case SupportedDataTypes.ARCH_DAY: archType = 1; break;
                case SupportedDataTypes.ARCH_MONTH: archType = 3; break;
                case SupportedDataTypes.ARCH_CURRENT: archType = 4; break;
                case SupportedDataTypes.ARCH_CURRENT_TOTAL: archType = 5; break;
                default:
                    return CreateFailureResult(51020,  archType);
            }
            
            if (!StartSeance(task.Arguments, out int errorCode, out byte inAddr, out int _))
                return CreateFailureResult(errorCode, archType);
            
            if(archType == 4)
            {
                ReadRegisterOneByOne(ModBusParamDescription.ReadCurrent,  inAddr, out Dictionary<string, string> values);

                if (deviceParameters.Where(a => a.ArcType == 30001).Count() == 0)
                    return CreateFailureResult(53430, archType);

                if (ReadData(DateTime.UtcNow, deviceParameters, values, (byte)archType))
                {
                    return CreateSuccessfulResult(tvList, trList, nsList, uomList, archType);
                }
            }else if(archType == 5)
            {

                if (deviceParameters.Where(a => a.ArcType == 30006).Count() == 0)
                    return CreateFailureResult(53430, archType);

                ReadRegisterOneByOne(ModBusParamDescription.ReadCurrentTotal, inAddr, out Dictionary<string, string> values);

                if (ReadData(DateTime.UtcNow, deviceParameters.ToList(), values, (byte)archType))
                {
                    return CreateSuccessfulResult(tvList, trList, nsList, uomList, archType);
                }
            }
            
            return CreateFailureResult(50090, archType);
        }

        public DeviceReadPropertiesEndStructure ReadDeviceParameters(TaskStruct task)
        {
            if (!StartSeance(task.Arguments, out int errorCode, out byte inAddr, out _))
                return new DeviceReadPropertiesEndStructure { ErrorCode = errorCode, Status = CurrentTaskStatus.Failed, Parameters = new DeviceReadPropertiesParametersStructure { UserID = task.UserID } };
            if (!ReadDeviceParameters(inAddr, out List<PropertiesStructure> deviceParameters))
                return new DeviceReadPropertiesEndStructure { ErrorCode = 50090, Status = CurrentTaskStatus.Failed, Parameters = new DeviceReadPropertiesParametersStructure { UserID = task.UserID } };
            if (!GetSerialNumber(inAddr, task.Arguments, out _, out errorCode))
                return new DeviceReadPropertiesEndStructure { ErrorCode = errorCode, Status = CurrentTaskStatus.Failed, Parameters = new DeviceReadPropertiesParametersStructure { UserID = task.UserID } };
            else
            {
                return new DeviceReadPropertiesEndStructure
                {
                    ErrorCode = 0,
                    Status = CurrentTaskStatus.Success,
                    Properties = deviceParameters,
                    Parameters = new DeviceReadPropertiesParametersStructure
                    {
                        ConnectedDeviceID = task.ConnectedDeviceID.Value,
                        UserID = task.UserID,
                        TimeRead = DateTime.UtcNow.ToTimeStamp(),
                    }
                };
            }
        }

        public TVReadPropertiesEndStructure ReadTVParameters(TaskStruct task)
        {
            task.Arguments.TryGetValue("MeasurePoint", out string mesPoint);
            int measurePoint = Convert.ToInt16(mesPoint);
            if (!StartSeance(task.Arguments, out int errorCode, out byte inAddr, out int _))
                return new TVReadPropertiesEndStructure { ErrorCode = errorCode, Status = CurrentTaskStatus.Failed, Parameters = new TVReadWritePropertiesParametersStructure { UserID = task.UserID } };

            else
            {
                return new TVReadPropertiesEndStructure
                {
                    Status = CurrentTaskStatus.Success,
                    ErrorCode = 0,
                    TVProperties = new List<PropertiesStructure> { },
                    TRProperties = new List<TRPropertiesStructure> { },
                    Parameters = new TVReadWritePropertiesParametersStructure
                    {
                        TVID = (Guid)task.TvID,
                        UserID = task.UserID,
                        TimeRead = DateTime.UtcNow.ToTimeStamp(),
                    }
                };
            }
        }
        public TVActivationEndStructure TVActivation(TaskStruct task)
        {
            task.Arguments.TryGetValue("MeasurePoint", out string mesPoint);
            int measurePoint = Convert.ToInt16(mesPoint);
            var configurationParameters = DeviceConfiguration.LoadDeviceConfiguration(task.ConnectedDeviceID, _logger);

            if (!StartSeance(task.Arguments, out int errorCode, out byte inAddr, out int _))
                return new TVActivationEndStructure { ErrorCode = errorCode, Status = CurrentTaskStatus.Failed, Parameters = new TVActivationParametersStructure { UserID = task.UserID } };
            else
            {
                var dataTV = new List<TVActivationDataTVStructure> { };
                var consumptionFields = new HashSet<string> { "V", "M", "G", "Gm" };

                foreach (var pipeParameters in configurationParameters.Where(parameter => parameter.IsPipeParameter && parameter.PipeNumber.HasValue).GroupBy(parameter => parameter.PipeNumber))
                {
                    var activationItem = new TVActivationDataTVStructure()
                    {
                        CountP = pipeParameters.Any(a => a.EldisParameter == "P"),
                        CountT = pipeParameters.Any(a => a.EldisParameter == "t"),
                        CountV = pipeParameters.Any(a => consumptionFields.Contains(a.EldisParameter)),
                        TRNumber = pipeParameters.Key.Value,
                        TypeTR = pipeParameters.Select(parameter => parameter.PipeType).Where(pipeType => pipeType.HasValue).FirstOrDefault() ?? 0,
                    };

                    if (activationItem.CountP || activationItem.CountT || activationItem.CountV)
                    {
                        dataTV.Add(activationItem);
                    }
                }
                return new TVActivationEndStructure
                {
                    Status = CurrentTaskStatus.Success,
                    ErrorCode = 0,
                    TVProperties = new List<PropertiesStructure> { },
                    TRProperties = new List<TRPropertiesStructure> { },
                    DataTV = dataTV,
                    Parameters = new TVActivationParametersStructure
                    {
                        TVID = task.TvID.Value,
                        UserID = task.UserID,
                        TimeRead = DateTime.UtcNow.ToTimeStamp(),
                        SchemeInternalCode = 112,

                    }
                };
            }
        }
        private bool ReadDeviceParameters(byte aAdress, out List<PropertiesStructure> deviceProperties)
        {
            deviceProperties = new List<PropertiesStructure>();

            GetVersionDevice(aAdress, out string vers);
            deviceProperties.Add(new PropertiesStructure { Name = "Vers", Description = "Название и версия прибора", Value = vers });

            if(!ReadRegisterOneByOne(ModBusParamDescription.deviceParams, aAdress, out List<string> values))
                return false;

            values = DistributionLimits(values, new Dictionary<int, int> { { 1, 29 }, { 2, 1 }, { 3, 1 }, { 4, 1 }, { 5, 1 }, { 6, 5 }, { 7, 5 }, { 8, 5 }, { 9, 5 }, { 22, 6 }, { 23, 6 }, { 41, 26 }, { 42, 27 }, { 43, 28 }, { 44, 15 }, { 45, 8 }, { 46, 8 }, { 72, 30 }, { 73, 30 }, { 74, 30 }, { 75, 30 }}).ToList(); //17
            try
            {
                for (int i = 0; i < values.Count; i++)
                {
                    var property = ModBusParamDescription.deviceParams.ElementAt(i);
                    deviceProperties.Add(new PropertiesStructure { Name = property.Name, Description = property.Description, Value = values[i], UOMName = property.UOMName });

                }
            } catch (Exception e) 
            {
                _logger.Trace(e);
                return false;
            }

            //Параметры для сопоставления
            deviceProperties.Add(new PropertiesStructure { Name = "@V+1k", Description = "Объем V+, 1к" });
            deviceProperties.Add(new PropertiesStructure { Name = "@V+2k", Description = "Объем V+, 2к" });

            deviceProperties.Add(new PropertiesStructure { Name = "@V-1k", Description = "Объем V-, 1к" });
            deviceProperties.Add(new PropertiesStructure { Name = "@V-2k", Description = "Объем V-, 2к" });

            deviceProperties.Add(new PropertiesStructure { Name = "@v_1k", Description = "Скорость потока,  м/с, 1к" });
            deviceProperties.Add(new PropertiesStructure { Name = "@v_2k", Description = "Скорость потока,  м/с, 2к" });

            deviceProperties.Add(new PropertiesStructure { Name = "@Q_1k", Description = "Расход, 1к" });
            deviceProperties.Add(new PropertiesStructure { Name = "@Q_2k", Description = "Расход, 2к"  });

            deviceProperties.Add(new PropertiesStructure { Name = "@Тр",Description = "Общее время работы, ч" });
            deviceProperties.Add(new PropertiesStructure { Name = "@Тпр_1k", Description = "Общее время НС, ч, 1к" });
            deviceProperties.Add(new PropertiesStructure { Name = "@Тпр_2k", Description = "Общее время НС, ч, 2к" });

            deviceProperties.Add(new PropertiesStructure { Name = "@НС_1k", Description = "Нештатные ситуации по измерениям 1к" });
            deviceProperties.Add(new PropertiesStructure { Name = "@НС_2k", Description = "Нештатные ситуации по измерениям 2к" });
            return true;
        }

        private bool ReadRegister(byte aAdress,byte aFuncCode, uint aRegister, uint aCount, typeRegister type, out string registerValue)
        {
            if(_transport.SendMessage(aAdress, aFuncCode, new uint[] { aRegister, aCount }.ToByteMessageBody() ))
            {
                if(_transport.ReceiveResponse(type, aFuncCode ,out List<string> val))
                {
                    registerValue = val[0];
                    return true;
                }
            }
            registerValue = null;
            return false;
        }

        private bool ReadRegisterOneByOne(IReadOnlyCollection<ModBusProperties> properties, byte aAdress, out List<string> values)
        {
            values = new List<string> { };
            try
            {
                foreach (var property in properties)
                {
                    if (ReadRegister(aAdress, (byte)property.FuncCode, property.PhysicalAdress, (uint)(property.aCount), property.ParameterType, out string val))
                        values.Add(val);
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex);
                return false;
            }

            return true;
        }

        private bool ReadRegisterOneByOne(IReadOnlyCollection<ModBusProperties> properties, byte aAdress, out Dictionary<string, string> values)
        {
            values = new Dictionary<string, string>{ };
            try
            {
                foreach (var property in properties)
                {
                    if (ReadRegister(aAdress, (byte)property.FuncCode, property.PhysicalAdress, (uint)(property.aCount), property.ParameterType, out string val))
                        values.Add(property.Name, val);
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex);
                return false;
            }

            return true;
        }

        private string[] DistributionLimits(List<string> items, Dictionary<int, int> numberLimits)
        {
            Dictionary<int, string> Annex1 = new Dictionary<int, string>() { { 0, "..." }, { 1, "Старт" } };

            Dictionary<int, string> Annex2 = new Dictionary<int, string>() { { 0, "Отключен" }, { 1, "Логический" }, { 2, "Импульсный" }, { 3, "Частотный" }, };

            Dictionary<int, string> Annex3 = new Dictionary<int, string>() { { 0, "Нет" }, { 1, "Q1+" }, { 2, "Q1-" }, { 3, "|Q1|" }, { 4, "Q2+" }, { 5, "Q2-" }, { 6, "|Q2|" }, };

            Dictionary<int, string> Annex4 = new Dictionary<int, string>() { { 0, "Нет" }, { 1, "V1+" }, { 2, "V1-" }, { 3, "|V1|" }, { 4, "V2+" }, { 5, "V2-" }, { 6, "|V2|" }, };

            Dictionary<int, string> Annex5 = new Dictionary<int, string>() { { 0, "Нет" }, { 1, "Знак пот. 1к" }, { 2, "Нет УЗС 1к" }, { 3, "Q>Qв.у. 1к" }, { 4, "Q<Qн.у. 1к" }, { 5, "Q>Qв.п. 1к" },
                { 6, "Q<Qн.п. 1к" }, { 7, "Q<Qmax 1к" }, { 8, "Знак пот. 2к" }, { 9, "Нету УЗС 2к" }, { 10, "Q>Qв.у. 2к" }, { 11, "Q<Qн.у. 2к" }, { 12, "Q>Qв.п. 2к" }, { 13, "Q<Qн.п. 2к" },{ 14, "Q<Qmax 2к" } };

            Dictionary<int, string> Annex6 = new Dictionary<int, string>() { { 0, "низк." }, { 1, "выс." } };

            Dictionary<int, string> Annex7 = new Dictionary<int, string>() { { 0, "накладные" }, { 1, "врезные" } };

            Dictionary<int, string> Annex8 = new Dictionary<int, string>() { { 0, "пусто" }, { 1, "Z-схема" }, { 2, "V-схема" }, { 3, "Диаметр" }, { 4, "Хорда" }, { 5, "U-колено" } };

            Dictionary<int, string> Annex9 = new Dictionary<int, string>() { { 0, "-" }, { 1, "+" } };

            Dictionary<int, string> Annex10 = new Dictionary<int, string>() { { 0, "откл" }, { 1, "вкл" } };

            Dictionary<int, string> Annex11 = new Dictionary<int, string>() { { 0, "1" }, { 1, "3" }, { 2, "5" }, { 3, "7" }, { 4, "9" }, { 5, "11" }, { 6, "13" }, { 7, "15" } };

            Dictionary<int, string> Annex12 = new Dictionary<int, string>() { { 0, "0.00" }, { 1, "0.04" }, { 2, "0.08" }, { 3, "0.12" }, { 4, "0.16" }, { 5, "0.20" },
                { 6, "0.23" }, { 7, "0.27" }, { 8, "0.31" }, { 9, "0.35" }, { 10, "0.39" }, { 11, "0.43" }, { 12, "0.47" }, { 13, "0.51" },{ 14, "0.55" },
                { 15, "0.59" }, { 16, "0.62" }, { 17, "0.66" }, { 18, "0.66" }, { 19, "0.74" }, { 20, "0.78" },{ 21, "0.82" }, { 22, "0.85" }, { 23, "0.90" },
                { 24, "0.94" }, { 25, "0.98" }, { 26, "1.01" }, { 27, "1.05" }, { 28, "1.09" },{ 29, "1.13" }, { 30, "1.17" }, { 31, "1.21" }, { 32, "1.25" },
                { 33, "1.29" }, { 34, "1.32" }, { 35, "1.37" },{ 36, "1.41" }, { 37, "1.44" }, { 38, "1.48" }, { 39, "1.52" }, { 40, "1.56" }, { 41, "1.60" },
                { 42, "1.64" }, { 43, "1.68" },{ 44, "1.72" }, { 45, "1.76" }, { 46, "1.80" }, { 47, "1.83" }, { 48, "1.87" }, { 49, "1.91" }, { 50, "1.95" }, { 51, "1.99" },
                { 52, "2.03" }, { 53, "2.07" }, { 54, "2.10" }, { 55, "2.14" }, { 56, "2.19" }, { 57, "2.22" }, { 58, "2.26" }, { 59, "2.30" },{ 60, "2.34" },
                { 61, "2.38" }, { 62, "2.42" }, { 63, "2.46" }, { 64, "2.50" }, { 65, "2.53" }, { 66, "2.57" },{ 67, "2.61" }, { 68, "2.65" }, { 69, "2.70" },
                { 70, "2.73" }, { 71, "2.77" }, { 72, "2.81" }, { 73, "2.85" }, { 74, "2.89" },{ 75, "2.93" }, { 76, "2.97" }, { 77, "3.01" }, { 78, "3.05" },
                { 79, "3.08" }, { 80, "3.12" }, { 81, "3.16" },{ 82, "3.20" }, { 83, "3.24" }, { 84, "3.28" }, { 85, "3.32" }, { 86, "3.36" }, { 87, "3.40" },
                { 88, "3.44" }, { 89, "3.48" },{ 90, "3.51" }, { 91, "3.55" },{ 92, "3.60" }, { 93, "3.63" }, { 94, "3.67" }, { 95, "3.71" }, { 96, "3.75" }, { 97, "3.79" },
                { 98, "3.83" }, { 99, "3.87" }, { 100, "3.91" }, { 101, "3.95" }, { 102, "3.98" }, { 103, "4.02" },{ 104, "4.06" }, { 105, "4.10" }, { 106, "4.14" },
                { 107, "4.18" }, { 108, "4.22" }, { 109, "4.26" }, { 110, "4.30" }, { 111, "4.34" },{ 112, "4.38" }, { 113, "4.41" }, { 114, "4.45" }, { 115, "4.49" },
                { 116, "4.53" }, { 117, "4.57" }, { 118, "4.60" },{ 119, "4.64" }, { 120, "4.68" }  };

            Dictionary<int, string> Annex13 = new Dictionary<int, string>() { { 0, "низкое" }, { 1, "высокое" } };

            Dictionary<int, string> Annex14 = new Dictionary<int, string>() { { 0, "м^3/ч" }, { 1, "л/мин" }, { 2, "м^3/c" } };

            Dictionary<int, string> Annex15 = new Dictionary<int, string>() { { 0, "м^3" }, { 1, "л" }, { 2, "м^3" } };

            Dictionary<int, string> Annex16 = new Dictionary<int, string>() { { 0, "Мн-кан." }, { 1, "Мн-луч." } };

            Dictionary<int, string> Annex17 = new Dictionary<int, string>() { { 0, "Рус" }, { 1, "Анг" } };

            Dictionary<int, string> Annex18 = new Dictionary<int, string>() { { 0, "нет" }, { 1, "." }, { 2, ".." }, { 3, "..." } };

            Dictionary<int, string> Annex19 = new Dictionary<int, string>() { { 0, "нет" }, { 1, "да" } };

            Dictionary<int, string> Annex20 = new Dictionary<int, string>() { { 0, "стоп" }, { 1, "пуск" } };

            Dictionary<int, string> Annex21 = new Dictionary<int, string>() { { 0, "..." }, { 1, "Сохр" } };

            Dictionary<int, string> Annex22 = new Dictionary<int, string>() { { 0, "Стоп" }, { 1, "Старт" } };

            Dictionary<int, string> Annex23 = new Dictionary<int, string>() { { 0, "1 канале" }, { 1, "2 канале" } };

            Dictionary<int, string> Annex24 = new Dictionary<int, string>() { { 0, "..." }, { 1, "стоп" }, { 2, "старт" } };

            Dictionary<int, string> Annex25 = new Dictionary<int, string> { { 0, "Без ошибок" }, { 1, "F>Fмакс." }, { 2, "Имп>Норма" }, { 3, "Есть ош." }, { 4, "Нар. границ" }, { 5, "Есть ош." }, { 6, "Есть ош." }, { 7, "Есть ош." }, };

            Dictionary<int, string> Annex26 = new Dictionary<int, string> { { 0, "+" }, { 1, "-" } };

            Dictionary<int, string> Annex27 = new Dictionary<int, string> { { 0, "Работа" }, { 1, "Сервис" }, { 2, "Настройка" } };

            Dictionary<int, string> Annex28 = new Dictionary<int, string> { { 0, "0" }, { 1, "1" }, { 2, "2" } };

            Dictionary<int, string> Annex29 = new Dictionary<int, string> { { 0, "УРСВ-1Х0" }, { 1, "УРСВ-122" } };

            Dictionary<int, string> Annex30 = new Dictionary<int, string> { { 0, "1200" }, { 1, "2400" }, { 2, "4800" }, { 3, "9600" }, { 4, "19200" }, { 5, "38400" }, { 6, "57600" }, { 7, "115200" } };

            Dictionary<int, string> Annex31 = new Dictionary<int, string> { { 0, "0.200" }, { 1, "0.204" }, { 2, "0.208" }, { 3, "0.213" }, { 4, "0.217" }, { 5, "0.222" },
                { 6, "0.227" }, { 7, "0.233" }, { 8, "0.238" }, { 9, "0.244" }, { 10, "0.250" }, { 11, "0.256" }, { 12, "0.2633" }, { 13, "0.270" },{ 14, "0.278" },
                { 15, "0.286" }, { 16, "0.294" }, { 17, "0.303" }, { 18, "0.313" }, { 19, "0.323" }, { 20, "0.333" },{ 21, "0.345" }, { 22, "0.357" }, { 23, "0.370" },
                { 24, "0.385" }, { 25, "0.400" }, { 26, "0.417" }, { 27, "0.435" }, { 28, "0.455" },{ 29, "0.476" }, { 30, "0.500" }, { 31, "0.526" }, { 32, "0.556" },
                { 33, "0.588" }, { 34, "0.625" }, { 35, "0.667" },{ 36, "0.714" }, { 37, "0.769" }, { 38, "0.833" }, { 39, "0.909" }, { 40, "1.000" }, { 41, "1.111" },
                { 42, "1.250" }, { 43, "1.429" },{ 44, "1.667" }, { 45, "2.000" }, };

            List<object> listLimits = new List<object>() { Annex1, Annex2, Annex3, Annex4, Annex5, Annex6, Annex7, Annex8, Annex9, Annex10, Annex11, Annex12, Annex13, Annex14, Annex15, Annex16, Annex17, Annex18, Annex19, Annex20, Annex21, Annex22, Annex23, Annex24, Annex25, Annex26, Annex27, Annex28, Annex29, Annex30, Annex31 };

            List<string> finalNumbers = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                if (numberLimits.ContainsKey(i))
                {
                    if (listLimits.Count() > numberLimits[i])
                    {
                        finalNumbers.Add(((Dictionary<int, string>)listLimits[numberLimits[i]]).Where(a => a.Key == Convert.ToInt16(items[i])).Select(a => a.Value).First());
                    }
                }
                else
                    finalNumbers.Add(items[i]);
            }
            return finalNumbers.ToArray();
        }

        private bool GetSerialNumber(byte aAdress, IReadOnlyDictionary<string, string> aArguments, out string serialNumber, out int errorCode)
        {
            if (ReadRegister(aAdress, (byte)aCodes.ReadHoldingRegisters, 389, 2, typeRegister._ulong, out string serialNum))
            {
                if (aArguments.TryGetValue("SN", out string invSerial))
                {
                    serialNumber = serialNum;
                    if (invSerial.Length < 1 || serialNum == invSerial)
                    {
                        errorCode = 0;
                        return true;

                    }
                    else if (serialNum != invSerial)
                    {
                        errorCode = 52320;
                        return false;
                    }
                } else
                {
                    errorCode = 0;
                    serialNumber = serialNum;
                    return true;
                }
            }
            serialNumber = null;
            errorCode = 50090;
            return false;

        }

        private bool GetVersionDevice(byte aAdress, out string versionDevice)
        {
            if (_transport.SendMessage(aAdress, 0x11, new byte[] { 0 }))
            {
                _transport.ReceiveResponse(typeRegister._ascii, (byte)aCodes.ReadInformationDevice, out List<string> resp);
                versionDevice = resp[0];
                return true;
            }
            versionDevice = null;
            return false;
        }

        private bool ReadData(DateTime currentDeviceDateTime, List<DeviceProperty> properties, Dictionary<string, string> values, byte typeArch)
        {
            SortedList<int, TR> sortedListTR = new SortedList<int, TR>();
            List<string> ns = new List<string>();
            HashSet<string> UOM = new HashSet<string>();
            foreach (var property in properties)
            {
                if (values.ContainsKey(property.DeviceParameter))
                {
                    if(property.UOM.Length > 0)
                        UOM.Add(property.UOM);

                    if (property.IsPipeParameter == true)
                    {
                        int pipeNumber = (int)property.PipeNumber;
                        if (sortedListTR.ContainsKey(pipeNumber))
                        {
                            sortedListTR[pipeNumber].SetProperty(property.EldisParameter, values[property.DeviceParameter], property.UOM);
                        }
                        else
                        {
                            sortedListTR.Add(pipeNumber, new TR { TRNumber = (byte)pipeNumber, ArcType = typeArch, dtMeasure = currentDeviceDateTime });
                            sortedListTR[pipeNumber].SetProperty(property.EldisParameter, values[property.DeviceParameter], property.UOM);
                        }
                    }
                    else
                    {
                        if (tvList.Count > 0)
                        {
                            tvList[0].SetProperty(property.EldisParameter, values[property.DeviceParameter], property.UOM);
                        }
                        else
                        {
                            tvList.Add(new TV { ArcType = typeArch, dtMeasure = currentDeviceDateTime });
                            tvList[0].SetProperty(property.EldisParameter, values[property.DeviceParameter], property.UOM);

                        }
                    }
                }
            }
            trList.AddRange(sortedListTR.Select(a => a.Value).ToArray());

            foreach(var uo in UOM)
                uomList.Add(new DataInternalUOM { ID = uomList.Count > 0 ? uomList.Last().ID + 1 : 0, UOMName = uo });

            if (tvList.Count == 0)
            {
                tvList.Add(new TV { dtMeasure = currentDeviceDateTime, ArcType = typeArch });
            }
            foreach (var tr in trList)
            {
                if (tr.NSCodes != null)
                {
                    if (Convert.ToUInt64(tr.NSCodes) > 0)
                    {
                        tvList[0].NS = true;
                        CheckBitMask(tr.NSCodes, typeArch, tr.TRNumber);
                        return true;
                    }
                    else
                    {
                        tvList[0].NS = false;
                    }
                }
            }
            return true;
        }

        private bool CheckBitMask(string bitmask, byte typeArch, int? trNumber)
        {
            if (Convert.ToInt64(bitmask) == 0)
                return false;

            for (int i = 0; i < bitmask.Length; i++)
                if (bitmask[i] == '1')
                    if (!nsErrorCode(i, DateTime.UtcNow, typeArch, trNumber))
                        return false;
            return true;
        }

        private bool nsErrorCode(int bitNumber, DateTime dtMeasure, byte typeArch,int? trNumber)
        {
            int nsCode = 0;
            switch (bitNumber)
            {
                case 0:
                    nsCode = 134130;
                    break;
                case 1:
                    nsCode = 134140;
                    break;
                case 2:
                    nsCode = 134150;
                    break;
                case 3:
                    nsCode = 134160;
                    break;
                case 4:
                    nsCode = 134170;
                    break;
                case 5:
                    nsCode = 134180;
                    break;
                case 6:
                    nsCode = 134190;
                    break;
                case 7:
                    nsCode = 134200;
                    break;
                case 8:
                    nsCode = 134210;
                    break;
                case 9:
                    nsCode = 134220;
                    break;
                case 10:
                    nsCode = 134230;
                    break;
            }
            try
            {
                nsList.Add(new NS
                {
                    DtMeasure = dtMeasure,
                    NSCode = nsCode,
                    TRNumber = trNumber,
                    TypeArch = typeArch,
                });

                return true;
            }catch(Exception e)
            {
                _logger.Trace(e);
                return false;
            }
        }
        public void Dispose()
        {
            if (_transport != null)
            {
                _transport.Dispose();
                _transport = null;
            }
        }
       
    }
}