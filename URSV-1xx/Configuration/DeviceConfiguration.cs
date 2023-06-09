using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CASRVCoreInterfaces;


namespace URSV1xx.Configuration
{
    internal class DeviceConfiguration
    {
        public static IReadOnlyCollection<DeviceProperty> LoadDeviceConfiguration(Guid? deviceID, NLog.ILogger logger)
        {
            var intermediate = LoadIntermediateData(deviceID ?? throw new ArgumentNullException(nameof(deviceID)), new EndpointAddress("http://185.149.241.79:10104"), logger);
            if (intermediate?.Length > 16)
            {
                XDocument xDocument;
                try
                {
                    xDocument = XDocument.Parse(Encoding.UTF8.GetString(intermediate));
                }
                catch (System.Xml.XmlException ex)
                {
                    logger?.Debug(ex, $"Невозможно загрузить XML-конфигурацию устройства {{{deviceID}}} в кодировке UTF-8.");
                    xDocument = XDocument.Parse(Encoding.Unicode.GetString(intermediate));
                }

                var xDeviceFields = xDocument.Element("ArrayOfDeviceField")?.Elements("DeviceField");
                if (xDeviceFields != null)
                {
                    var deviceProperties = new List<DeviceProperty>();
                    foreach (var xDeviceField in xDeviceFields)
                    {
                        // Note: Параметры, имеющие значение "NULL" - не сопоставлены c АИИС "Элдис".
                        //       Они необходимы только для конфигурационных утилит, в драйвере их обработка не требуется.
                        var deviceProperty = DeviceProperty.Deserialize(xDeviceField);
                        if (!deviceProperty.EldisParameter.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                        {
                            deviceProperties.Add(deviceProperty);
                        }
                    }

                    if (deviceProperties?.Count > 0)
                    {
                        if (logger != null)
                        {
                            logger.Debug($"Загружено {deviceProperties.Count} записей о параметрах устройства {{{deviceID}}} из XML-конфигурации");
#if DEBUG
                            foreach (var property in deviceProperties)
                            {
                                logger.Debug($"\t - {property}");
                            }
#endif
                        }

                        return deviceProperties;
                    }
                }
            }

            logger?.Warn($"Не удалось загрузить XML-конфигурация устройства {{{deviceID}}}...");
            return new List<DeviceProperty>();
        }

        /// <summary>
        /// Загружает файл из внутреннего хранилища АИИС "Элдис" для указанного устройства, обёртку над WebAPI функцией <see cref="LoadIntermediateDataByDeviceID(EndpointAddress, Guid)"/>.
        /// Данный метод перехватывает все внутренние исключения и записывает их в компонент <see cref="NLog.ILogger"/>, если он указан в параметрах.
        /// </summary>
        /// <param name="deviceID">Идентификатор устройства (типа <see cref="Guid"/>), конфигурация которого должна быть получена.</param>
        /// <param name="endpoint">Объект типа <see cref="EndpointAddress"/>, представляющий адрес сервера АИИС "Элдис", с которого должны быть получены данные.</param>
        /// <param name="logger">Компонент для вывода сообщений в лог (может иметь значение null).</param>
        /// <returns>Байтовый буфер, содержащий XML конфигурацию устройства, полученную из АИИС "Элдис"; null - в случае возникновения ошибки при получении данных с сервера АИИС "Элдис".</returns>
        private static byte[] LoadIntermediateData(Guid deviceID, EndpointAddress endpoint, NLog.ILogger logger)
        {
            if (endpoint == null)
            {
                logger?.Debug($"Не задан адрес сервера для загрузки данных для устройства {{{deviceID}}}.");
            }
            else
            {
                for (byte attempt = 0; attempt < 3; ++attempt)
                {
                    try
                    {
                        var decompressedData = LoadIntermediateDataByDeviceID(endpoint, deviceID);
                        if (decompressedData?.Length > 0)
                        {
                            return decompressedData;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.Debug(ex, $"Не могу загрузить данные для устройства {{{deviceID}}} из внутреннего хранилища!");
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Загружает файл из внутреннего хранилища АИИС "Элдис" для указанного устройства, используя WebAPI функцию "LoadIntermediateDataByConnectedDeviceIDv2".
        /// </summary>
        /// <param name="endpoint">Объект типа <see cref="EndpointAddress"/>, представляющий адрес сервера АИИС "Элдис", с которого должны быть получены данные.</param>
        /// <param name="deviceID">Идентификатор устройства (типа <see cref="Guid"/>), конфигурация которого должна быть получена.</param>
        /// <returns>Байтовый буфер, содержащий XML конфигурацию устройства, полученную из АИИС "Элдис"; null - в случае, если с сервера АИИС "Элдис" была получена пустая конфигурация.</returns>
        /// <remarks>Используемый ранее интерфейс "IInformationService" для получения данных не подходит из-за того, что внутри стандартного <see cref="ChannelFactory{TChannel}"/> используется XML-сериализация, не поддерживаемая на Linux-платформах.</remarks>
        private static byte[] LoadIntermediateDataByDeviceID(EndpointAddress endpoint, Guid deviceID)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }
            else
            {
                using (var client = new WebClient())
                {
                    var requestUri = new Uri(endpoint.Uri, $"/LoadIntermediateDataByConnectedDeviceIDv2/{deviceID}");

                    var downloadedData = client.DownloadData(requestUri);
                    if (downloadedData?.Length > 0)
                    {
                        return downloadedData.DecompressData();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

    }
}
