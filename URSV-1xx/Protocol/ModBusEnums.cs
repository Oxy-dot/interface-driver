namespace URSV1xx.Protocol
{
    internal enum aCodes : byte
    {
        /// <summary>
        /// Регистры чтения/записи (4XXXXX)
        /// </summary>
        ReadHoldingRegisters = 0x03,
        /// <summary>
        /// Регистры чтения (3XXXXX)
        /// </summary>
        ReadInputRegisters = 0x04,
        /// <summary>
        /// Функция N17 (Чтение информации об адресуемом устройстве)
        /// </summary>
        ReadInformationDevice = 0x11,
        /// <summary>
        /// Чтение архива 
        /// </summary>
        ReadArchive = 0x41
    }
    public enum typeRegister : int
    {
        _int = 0,
        _float = 1,
        _ulong = 2,
        _long = 3,
        //Выводит данные ввиде чч:мн
        _time = 4,
        _ascii = 5,
        _dateTime = 6,
        _longFloat = 7,
        _ns = 8,
    }
}
