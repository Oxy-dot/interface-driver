using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CASRVCoreInterfaces;

namespace URSV1xx.Configuration
{
    internal class DeviceProperty
    {
        #region Properties

        /// <summary>
        /// Получает имя группы, к которой относится данный параметр.
        /// </summary>
        public string Group { get; private set; }

        /// <summary>
        /// Получает имя параметра в списке параметров, который возвращается в <see cref="IMeterDriver.DeviceActivation(TaskCore.TaskStruct)"/>.
        /// </summary>
        public string DeviceParameter { get; private set; }

        /// <summary>
        /// Получает имя свойства в Data-структуре, в которую должен быть записан данный параметр.
        /// Если параметр не относится к архиву, данное свойство имеет значение "NULL".
        /// </summary>
        public string EldisParameter { get; private set; }

        /// <summary>
        /// Получает имя единиц измерения, которые используются для данного параметра.
        /// </summary>
        public string UOM { get; private set; }

        /// <summary>
        /// Получает код типа архива, к которому относится данный параметр.
        /// </summary>
        /// <remarks>Если тип архива указан как 0, то значит есть сопоставление со всеми доступными в драйвере типами архива.</remarks>
        public int ArcType { get; private set; }

        /// <summary>
        /// Получает имя магистрали, к которой относится данный параметр.
        /// </summary>
        public string MeasurePoint { get; private set; }

        /// <summary>
        /// Получает значение, показывающее, что параметр относится к трубе.
        /// </summary>
        public bool IsPipeParameter { get; private set; }

        /// <summary>
        /// Получает номер трубы (начиная с 1), к которой относится данный параметр.
        /// Если параметр относится к магистрали, значение свойства выставляется в null.
        /// </summary>
        public int? PipeNumber { get; private set; }

        /// <summary>
        /// Получает тип трубы (начиная с 1), к которой относится данный параметр.
        /// Если параметр относится к магистрали, значение свойства выставляется в null.
        /// </summary>
        public int? PipeType { get; private set; }



        #endregion Properties

        #region Public methods

        /// <summary>
        /// Проверяет, сопоставлен ли текущий параметр устройства с указанным типом архива.
        /// </summary>
        /// <param name="aArchiveType">Тип архива, для которого осуществляется чтение в методе <see cref="CASRVCoreInterfaces.IMeterDriver.ReadData(TaskCore.TaskStruct)"/>.</param>
        /// <returns>true - если текущий параметр конфигурации сопоставлен с архивом; false - если параметр не сопоставлен.</returns>
        public bool IsArcTypeMatch(short? aArchiveType)
        {
            if (ArcType == 0)
            {
                // Значение 0 означает сопоставление со всеми доступными архивами.
                return true;
            }
            else
            {
                return aArchiveType == ArcType;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var stateBuilder = new System.Text.StringBuilder();
            foreach (var property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyValue = property.GetValue(this);

                bool isOutput;
                if (propertyValue is string stringValue)
                {
                    isOutput = stringValue?.Length > 0;
                }
                else if (propertyValue is int)
                {
                    isOutput = true;
                }
                else
                {
                    isOutput = propertyValue is bool;
                }

                if (isOutput)
                {
                    if (stateBuilder.Length > 0)
                    {
                        stateBuilder.Append(" | ");
                    }

                    // Обрезаем имена свойств, заканчивающихся на "Parameter" для уменьшения длины строки
                    if (property.Name.EndsWith("Parameter", StringComparison.OrdinalIgnoreCase))
                    {
                        stateBuilder.Append(property.Name.Substring(0, property.Name.Length - 9));
                    }
                    else
                    {
                        stateBuilder.Append(property.Name);
                    }

                    // Выводим строковые значения в кавычках.
                    if (property.PropertyType == typeof(string))
                    {
                        stateBuilder.Append($" = \"{propertyValue}\"");
                    }
                    else
                    {
                        stateBuilder.Append($" = {propertyValue}");
                    }
                }
            }

            return stateBuilder.ToString();
        }

        /// <summary>
        /// Преобразует данные, сохранённые в объекте типа <see cref="XElement"/> (XML элемент), в объект типа <see cref="DeviceProperty"/>.
        /// </summary>
        /// <param name="aSource">Объект <see cref="XElement"/>, содержащий информацию об объекте <see cref="DeviceProperty"/>.</param>
        /// <returns>Объект типа <see cref="DeviceProperty"/>, извлечённый из указанного XML элемента.</returns>
        public static DeviceProperty Deserialize(XElement aSource)
        {
            if (aSource == null)
            {
                throw new ArgumentNullException(nameof(aSource));
            }
            else if (!aSource.Name.LocalName.Equals("DeviceField", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Ожидается объект <DeviceField>. Текущий элемент: <{aSource.Name}>.", nameof(aSource));
            }
            else if (!aSource.HasElements)
            {
                throw new ArgumentException($"Объект <{aSource.Name}> не содержит дочерних элементов.", nameof(aSource));
            }
            else
            {
                var parameterInfo = new DeviceProperty();

                XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
                foreach (var property in parameterInfo.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var xPropery = aSource.Element(property.Name);
                    if (xPropery != null)
                    {
                        var xNil = xPropery.Attribute(xsi + "nil");
                        if (!bool.TryParse(xNil?.Value, out var isNull))
                        {
                            isNull = false;
                        }

                        if (isNull)
                        {
                            property.SetValue(parameterInfo, null);
                        }
                        else if (property.PropertyType == typeof(bool))
                        {
                            if (bool.TryParse(xPropery.Value, out var booleanValue))
                            {
                                property.SetValue(parameterInfo, booleanValue);
                            }
                        }
                        else if ((property.PropertyType == typeof(int)) || (property.PropertyType == typeof(int?)))
                        {
                            if (int.TryParse(xPropery.Value, out var integerValue))
                            {
                                property.SetValue(parameterInfo, integerValue);
                            }
                        }
                        else if (property.PropertyType == typeof(string))
                        {
                            property.SetValue(parameterInfo, xPropery.Value);
                        }
                    }
                }

                return parameterInfo;
            }
        }

        #endregion Public methods

    }
}
