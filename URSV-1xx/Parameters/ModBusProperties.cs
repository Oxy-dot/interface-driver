namespace URSV1xx
{
    using URSV1xx.Protocol;
    internal class ModBusProperties
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string UOMName { get; set; }
        public uint PhysicalAdress { get; set; }
        public aCodes FuncCode { get; set; }
        public typeRegister ParameterType { get; set; }
        public uint aCount => (uint)(ParameterType == 0 || (uint)ParameterType == 8 ? 1 : 1 <= (uint)ParameterType && (uint)ParameterType <= 4 ? 2 : (uint)ParameterType == 6 || (uint)ParameterType == 7 ? 4 : 0);
    }
}
