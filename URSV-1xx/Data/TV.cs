namespace URSV1xx.Data
{
    using System;
    using System.Linq;

    internal class TV
    {
        public byte ArcType { get; set; }
        public DateTime dtMeasure { get; set; }
        public double? TWork { get; set; }
        public string TWork_UOM { get; set; }
        public bool? NS { get; set; }
    }
}
