namespace URSV1xx.Data
{
    using System;
    internal class TR
    {
        public byte ArcType { get; set; }
        public byte TRNumber { get; set; }
        public DateTime dtMeasure { get; set; }
        public string NSCodes { get; set; }
        public double? G { get; set; }
        public string G_UOM { get; set; }
        public double? Vel { get; set; }
        public string Vel_UOM { get; set; }
        public double? VPlus { get; set; }
        public string VPlus_UOM { get; set; }
        public double? VMinus { get; set; }
        public string VMinus_UOM { get; set; }
        public double? TNS { get; set; }
        public string TNS_UOM { get; set;}
    }
}
