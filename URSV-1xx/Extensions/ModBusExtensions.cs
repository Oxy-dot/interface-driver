
namespace URSV1xx.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using XLiFrameWork;
    internal static class ModBusExtensions
    {
        /// <summary>
        /// Генерирует правильный массив байтов из массива чисел, нужный для генерации сообщений к устройству
        /// </summary>
        /// <param name="arrayValues">Массив чисел</param>
        /// <returns></returns>
        public static byte[] ToByteMessageBody(this uint[] arrayValues)
        {
            List<byte> finalArray = new List<byte>();
            for(int i = 0; i < arrayValues.Count(); i++)
            {
                finalArray.XFAddUInt16((ushort)arrayValues[i], Enums.Endianness.Little);
            }

            return finalArray.ToArray();
        }
        public static bool SetProperty(this object instance, string propertyName, object propertyValue, string propertyUOMValue)
        {
            if (propertyName.Length != 0)
            {
                if (SetProperty(instance, propertyName, propertyValue))
                {
                    if (propertyName.Contains("НС_"))
                    {
                        return true;
                    }
                    propertyName = propertyName += "_UOM";

                    if(SetProperty(instance, propertyName, propertyUOMValue))
                        return true;
                }
            }
            return false;
        }
        private static bool SetProperty(this object instance, string propertyName, object propertyValue  )
        {
            if(propertyName.Length != 0)
            {
                var property = instance.GetType().GetProperty(propertyName);
                if(property != null)
                {
                    if(property.PropertyType == typeof(double?))
                    {
                        property.SetValue(instance, Convert.ToDouble(propertyValue));
                        return true;
                    }else if(property.PropertyType == typeof(string))
                    {
                        property.SetValue(instance, Convert.ToString(propertyValue));
                        return true;
                    }
                    
                }
            }
            
            return false;
        }
        
    }
}
