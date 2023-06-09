

namespace URSV1xx
{
    using System.Collections.Generic;
    using URSV1xx.Protocol;
    internal class ModBusParamDescription
    {
        public static IReadOnlyCollection<ModBusProperties> deviceParams { get; } = new List<ModBusProperties>()
        {
            new ModBusProperties { PhysicalAdress = 0x00, Name = "Adress", Description = "Сетевой адрес устройства", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters },
            new ModBusProperties { PhysicalAdress = 0x01, Name = "Speed", Description = "Cкорость обмена по RS232",  ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},

            new ModBusProperties { PhysicalAdress = 0x1F, Name = "TypeOutUnivOutput1", Description = "Тип выхода УНИВ.ВЫХОД 1", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties { PhysicalAdress = 0x20, Name = "TypeOutUnivOutput1", Description = "Тип выхода УНИВ.ВЫХОД 2", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties { PhysicalAdress = 0x21, Name = "TypeOutUnivOutput1", Description = "Тип выхода УНИВ.ВЫХОД 3", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties { PhysicalAdress = 0x22, Name = "TypeOutUnivOutput1", Description = "Тип выхода УНИВ.ВЫХОД 4", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},

            new ModBusProperties { PhysicalAdress = 0x2F, Name = "ActStatUnivOut1", Description = "Активное состояние УНИВ.ВЫХОД 1", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties { PhysicalAdress = 0x30, Name = "ActStatUnivOut2", Description = "Активное состояние УНИВ.ВЫХОД 2", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties { PhysicalAdress = 0x31, Name = "ActStatUnivOut3", Description = "Активное состояние УНИВ.ВЫХОД 3", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties { PhysicalAdress = 0x32, Name = "ActStatUnivOut4", Description = "Активное состояние УНИВ.ВЫХОД 4", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},

            new ModBusProperties { PhysicalAdress = 0x33,Name = "PulseDurUnivOut1", Description = "Длительность импульса, УНИВ.ВЫХОД 1", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties { PhysicalAdress = 0x34,Name = "PulseDurUnivOut2", Description = "Длительность импульса, УНИВ.ВЫХОД 2", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties { PhysicalAdress = 0x35,Name = "PulseDurUnivOut3", Description = "Длительность импульса, УНИВ.ВЫХОД 3", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties { PhysicalAdress = 0x36,Name = "PulseDurUnivOut4", Description = "Длительность импульса, УНИВ.ВЫХОД 4", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},

            new ModBusProperties { PhysicalAdress = 0x37,Name = "KP 0", Description = "Коэффициент преобразования 0", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "имп/м3"},
            new ModBusProperties { PhysicalAdress = 0x39,Name = "KP 1", Description = "Коэффициент преобразования 1", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "имп/м3"},
            new ModBusProperties { PhysicalAdress = 0x3B, Name = "KP 2", Description = "Коэффициент преобразования 2", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "имп/м3"},
            new ModBusProperties { PhysicalAdress = 0x3D, Name = "KP 3", Description = "Коэффициент преобразования 3", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "имп/м3"},

            new ModBusProperties {PhysicalAdress = 0x3F, Name = "KI 0", Description = "Вес импульса 0", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties {PhysicalAdress = 0x41, Name = "KI 1", Description = "Вес импульса 1", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties {PhysicalAdress = 0x43, Name = "KI 2", Description = "Вес импульса 2", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties {PhysicalAdress = 0x45, Name = "KI 3", Description = "Вес импульса 3", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters},

            new ModBusProperties {PhysicalAdress = 0x6F, Name = "SenTypeMainPar1k", Description = "Тип датчика ОСН.ПАРАМ. 1к", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties {PhysicalAdress = 0x70, Name = "SenTypeMainPar2k", Description = "Тип датчика ОСН.ПАРАМ. 2к", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},

            new ModBusProperties {PhysicalAdress = 0x00B3, Name = "K11k", Description = "К1 КАЛИБ.КОЭФ. 1к", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties {PhysicalAdress = 0x00B5, Name = "K12k", Description = "К1 КАЛИБ.КОЭФ. 2к", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties {PhysicalAdress = 0x00B7, Name = "K21k", Description = "К2 КАЛИБ.КОЭФ. 1к", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties {PhysicalAdress = 0x00B9, Name = "K22k", Description = "К2 КАЛИБ.КОЭФ. 2к", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties {PhysicalAdress = 0x00BB, Name = "K31k", Description = "К3 КАЛИБ.КОЭФ. 1к", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties {PhysicalAdress = 0x00BD, Name = "K32k", Description = "К3 КАЛИБ.КОЭФ. 2к", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties {PhysicalAdress = 0x00BF, Name = "Kp1k", Description = "Кп КАЛИБ.КОЭФ. 1к", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties {PhysicalAdress = 0x00C1, Name = "Kp2k", Description = "Кп КАЛИБ.КОЭФ. 2к", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters},

            new ModBusProperties {PhysicalAdress = 0x021A, Name = "Tizg", Description = "Дата изготовления", ParameterType = typeRegister._dateTime, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties {PhysicalAdress = 0x021C, Name = "Tpov", Description = "Дата последней поверки", ParameterType = typeRegister._dateTime, FuncCode = aCodes.ReadHoldingRegisters},

            new ModBusProperties {PhysicalAdress = 0x021E, Name = "ID", Description = "Идентификационный код 32 бит", ParameterType = typeRegister._ulong, FuncCode = aCodes.ReadHoldingRegisters},

            new ModBusProperties {PhysicalAdress = 0x0064, Name = "Density0", Description = "Плотность 0", ParameterType = typeRegister._float, FuncCode = aCodes.ReadInputRegisters, UOMName= "т/м3"},
            new ModBusProperties {PhysicalAdress = 0x0066, Name = "Density1", Description = "Плотность 1", ParameterType = typeRegister._float, FuncCode = aCodes.ReadInputRegisters, UOMName = "т/м3"},
            new ModBusProperties {PhysicalAdress = 0x0068, Name = "Viscosity0", Description = "Вязкость 0", ParameterType = typeRegister._float, FuncCode = aCodes.ReadInputRegisters, UOMName = "сСТ"},
            new ModBusProperties {PhysicalAdress = 0x006A, Name = "Viscosity1", Description = "Вязкость 1", ParameterType = typeRegister._float, FuncCode = aCodes.ReadInputRegisters, UOMName = "сСТ"},
            new ModBusProperties {PhysicalAdress = 0x006C, Name = "CorrectFacVis0", Description = "Поправочный коэффициент для вязкости 0", ParameterType = typeRegister._float, FuncCode = aCodes.ReadInputRegisters},
            new ModBusProperties {PhysicalAdress = 0x006E, Name = "CorrectFacVis1", Description = "Поправочный коэффициент для вязкости 1", ParameterType = typeRegister._float, FuncCode = aCodes.ReadInputRegisters},

            new ModBusProperties {PhysicalAdress = 0x00D2, Name = "WorkMode", Description = "Режим работы", ParameterType = typeRegister._int, FuncCode = aCodes.ReadInputRegisters},

            new ModBusProperties {PhysicalAdress = 0x0111, Name = "countCan", Description = "Количество каналов", ParameterType = typeRegister._int, FuncCode = aCodes.ReadInputRegisters},
            new ModBusProperties {PhysicalAdress = 0x0112, Name = "confDevice", Description = "Конфигурация прибора (число лучей)", ParameterType = typeRegister._int, FuncCode = aCodes.ReadInputRegisters},

            new ModBusProperties {PhysicalAdress = 0x0115, Name = "Konf", Description = "Конфигурация прибора (канальный/лучевой)", ParameterType = typeRegister._int, FuncCode = aCodes.ReadInputRegisters},

            new ModBusProperties {PhysicalAdress = 0x0077, Name = "FlowSignResProc1k", Description = "Знак потока (индекс) ОБРАБ.РЕЗ. 1к", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},

            new ModBusProperties {PhysicalAdress = 0x0077, Name = "FlowSignResProc1k", Description = "Знак потока (индекс) ОБРАБ.РЕЗ. 1к", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties {PhysicalAdress = 0x0078, Name = "FlowSignResProc2k", Description = "Знак потока (индекс) ОБРАБ.РЕЗ. 2к", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters},

            new ModBusProperties {PhysicalAdress = 0x0077, Name = "IntertiTimeResProc1k", Description = "Время инерции, c ОБРАБ.РЕЗ. 1к", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "с"},
            new ModBusProperties {PhysicalAdress = 0x0078, Name = "IntertiTimeResProc2k", Description = "Время инерции, c ОБРАБ.РЕЗ. 2к", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "с"},

            new ModBusProperties {PhysicalAdress = 0x00CB, Name = "Vmax0", Description = "Максимальная скорость потока 0", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "м/с"},
            new ModBusProperties {PhysicalAdress = 0x00CD, Name = "Vmax1", Description = "Максимальная скорость потока 1", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "м/с"},

            new ModBusProperties {PhysicalAdress = 0x00CF, Name = "Otc0", Description = "Отсечка 0", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "м3/ч"},
            new ModBusProperties {PhysicalAdress = 0x00D1, Name = "Otc1", Description = "Отсечка 1", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "м3/ч"},

            new ModBusProperties {PhysicalAdress = 0x00D3, Name = "dT00", Description = "Смещение нуля 0", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "мкс"},
            new ModBusProperties {PhysicalAdress = 0x00D5, Name = "dT01", Description = "Смещение нуля 1", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "мкс"},

            new ModBusProperties {PhysicalAdress = 0x00D7, Name = "AddDelay0", Description = "Дополнительная задержка 0", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "мкс"},
            new ModBusProperties {PhysicalAdress = 0x00D9, Name = "AddDelay1", Description = "Дополнительная задержка 1", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "мкс"},

            new ModBusProperties {PhysicalAdress = 0x00DB, Name = "VelUltraTab0", Description = "Скорость ультразвука табличная 0", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "км/с"},
            new ModBusProperties {PhysicalAdress = 0x00DD, Name = "VelUltraTab1", Description = "Скорость ультразвука табличная 1", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "км/с"},

            new ModBusProperties {PhysicalAdress = 0x00DF, Name = "weightCoeff1k", Description = "Весовой коэфициент КАЛИБ.КОЭФ. 1к", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters},
            new ModBusProperties {PhysicalAdress = 0x00E1, Name = "weightCoeff2k", Description = "Весовой коэфициент КАЛИБ.КОЭФ. 2к", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters},

            new ModBusProperties {PhysicalAdress = 0x00D3, Name = "Ks1k", Description = "Ks КАЛИБ.КОЭФ. 1к", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "мкс"},
            new ModBusProperties {PhysicalAdress = 0x00D5, Name = "Ks2k", Description = "Ks КАЛИБ.КОЭФ. 2к", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "мкс"},

            new ModBusProperties {PhysicalAdress = 0x0103, Name = "EnableMeasurePoint1k", Description = "Включение измерений на канале ОСН.ПАРАМ. 1к", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "мкс"},
            new ModBusProperties {PhysicalAdress = 0x0104, Name = "EnableMeasurePoint2k", Description = "Включение измерений на канале ОСН.ПАРАМ. 2к", ParameterType = typeRegister._int, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "мкс"},

            new ModBusProperties {PhysicalAdress = 0x0165, Name = "NY0", Description = "Нижняя уставка 0", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "м3/ч"},
            new ModBusProperties {PhysicalAdress = 0x0167, Name = "NY1", Description = "Нижняя уставка 1", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "м3/ч"},

            new ModBusProperties {PhysicalAdress = 0x016D, Name = "VY0", Description = "Верхняя уставка 0", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "м3/ч"},
            new ModBusProperties {PhysicalAdress = 0x016F, Name = "VY1", Description = "Верхняя уставка 1", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "м3/ч"},

            new ModBusProperties {PhysicalAdress = 0x01CC, Name = "dQ00", Description = "Смещение нуля по расходу (в поверке) 0", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "м3/ч"},
            new ModBusProperties {PhysicalAdress = 0x01CE, Name = "dQ01", Description = "Смещение нуля по расходу (в поверке) 1", ParameterType = typeRegister._float, FuncCode = aCodes.ReadHoldingRegisters, UOMName = "м3/ч"},

            new ModBusProperties {PhysicalAdress = 0x00C3, Name = "Fskan1k", Description = "Значение текущей частоты при калибровке УСТ.част. 1 канал", ParameterType = typeRegister._int, FuncCode = aCodes.ReadInputRegisters, UOMName = "Мгц"},
            new ModBusProperties {PhysicalAdress = 0x00C4, Name = "Fskan2k", Description = "Значение текущей частоты при калибровке УСТ.част. 2 канал", ParameterType = typeRegister._int, FuncCode = aCodes.ReadInputRegisters, UOMName = "Мгц"},

            new ModBusProperties {PhysicalAdress = 0x00C5, Name = "Frab1k", Description = "Значение наилучшей частоты при калибровке УСТ.част. 1 канал", ParameterType = typeRegister._int, FuncCode = aCodes.ReadInputRegisters, UOMName = "Мгц"},
            new ModBusProperties {PhysicalAdress = 0x00C6, Name = "Frab2k", Description = "Значение наилучшей частоты при калибровке УСТ.част. 2 канал", ParameterType = typeRegister._int, FuncCode = aCodes.ReadInputRegisters, UOMName = "Мгц"},
        };

        public static IReadOnlyCollection<ModBusProperties> ReadCurrent { get; } = new List<ModBusProperties>()
        {
            new ModBusProperties {PhysicalAdress = 0x0028, Name = "НС_1k", ParameterType = typeRegister._ns, FuncCode = aCodes.ReadInputRegisters },
            new ModBusProperties {PhysicalAdress = 0x0029, Name = "НС_2k", ParameterType = typeRegister._ns, FuncCode = aCodes.ReadInputRegisters },
            new ModBusProperties {PhysicalAdress = 0x0010, Name = "Q_1k", ParameterType = typeRegister._float, FuncCode = aCodes.ReadInputRegisters },
            new ModBusProperties {PhysicalAdress = 0x0012, Name = "Q_2k", ParameterType = typeRegister._float, FuncCode = aCodes.ReadInputRegisters },
            new ModBusProperties {PhysicalAdress = 0x0020, Name = "v_1k", ParameterType = typeRegister._float, FuncCode = aCodes.ReadInputRegisters },
            new ModBusProperties {PhysicalAdress = 0x0022, Name = "v_2k", ParameterType = typeRegister._float, FuncCode = aCodes.ReadInputRegisters },

        };
        public static IReadOnlyCollection<ModBusProperties> ReadCurrentTotal { get; } = new List<ModBusProperties>()
        {
            new ModBusProperties {PhysicalAdress = 0x0075, Name = "Тр", ParameterType = typeRegister._time, FuncCode = aCodes.ReadInputRegisters },
            new ModBusProperties {PhysicalAdress = 0x0085, Name = "V+1k", ParameterType = typeRegister._longFloat, FuncCode = aCodes.ReadInputRegisters },
            new ModBusProperties {PhysicalAdress = 0x0089, Name = "V+2k", ParameterType = typeRegister._longFloat, FuncCode = aCodes.ReadInputRegisters },
            new ModBusProperties {PhysicalAdress = 0x008D, Name = "V-1k", ParameterType = typeRegister._longFloat, FuncCode = aCodes.ReadInputRegisters },
            new ModBusProperties {PhysicalAdress = 0x0091, Name = "V-2k", ParameterType = typeRegister._longFloat, FuncCode = aCodes.ReadInputRegisters },
            new ModBusProperties {PhysicalAdress = 0x0077, Name = "Тпр_1k", ParameterType = typeRegister._time, FuncCode = aCodes.ReadInputRegisters },
            new ModBusProperties {PhysicalAdress = 0x0079, Name = "Тпр_2k", ParameterType = typeRegister._time, FuncCode = aCodes.ReadInputRegisters },

        };
    }
}
