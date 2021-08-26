using Serilog;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common.Serilog.Formatters
{
    /// <summary>
    /// Форматтер производящий JSON логи в формате подходящем для потребления
    /// logstash'ем и filebeat'ом.
    /// </summary>
    public class ElasticFormatter : ITextFormatter
    {
        private const string DefaultDateFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        /// <summary>
        /// Свойства поднимаемые в корень сообщения.
        /// Исходное имя свойства => конечное имя свойства в сообщении.
        /// </summary>
        private readonly Dictionary<string, string> _hoistingMap;

        /// <summary>
        /// Маппинг дефолтных названий уровней важности события.
        /// </summary>
        private readonly Dictionary<string, string> _levelsMap;

        private readonly JsonValueFormatter _valueFormatter;
        private readonly string _dateFormat;

        /// <summary>
        /// Конструирует экземпляр <see cref="ElasticFormatter"/>.
        /// </summary>
        /// <param name="dateFmt">формат даты</param>
        /// <param name="mapLevels">использовать ли короткие названия для уровней критичности события</param>
        /// <param name="formatter">преобразователь данных в JSON</param>
        public ElasticFormatter(
            string dateFmt = null,
            bool mapLevels = true,
            JsonValueFormatter formatter = null)
        {
            _dateFormat = dateFmt ?? DefaultDateFormat;
            _valueFormatter = formatter ?? new JsonValueFormatter();

            if (mapLevels)
            {
                _levelsMap = new Dictionary<string, string>
                {
                    { "Verbose", "TRACE" },
                    { "Debug", "DEBUG" },
                    { "Information", "INFO" },
                    { "Warning", "WARN" },
                    { "Error", "ERR" },
                    { "Fatal", "FATAL" },
                };
            }

            // XXX: было бы неплохо указывать эти параметры в конфиге, но
            // Serilog не поддерживает конфигурирование форматеров через конфиг.
            _hoistingMap = new Dictionary<string, string>()
            {
                { "RequestId", "rq.id" },
                { "CorrelationId", "rq.correlationId" },
                { "ConnectionId", "rq.connectionId" },
                { "RequestRemoteIp", "rq.remoteIp" },
                { "RequestHost", "rq.host" },
                { "RequestPath", "rq.path" },
                { "RequestQuery", "rq.query" },
                { "RequestMethod", "rq.method" },
                { "RequestUserAgent", "rq.userAgent" },
                { "ActionName", "mvc.action" },
                { "ActionId", "mvc.actionId" },
                { "ResponseElapsed", "rs.elapsed" },
                { "ResponseStatusCode", "rs.status" },
                { "SourceContext", "sourceContext" }
            };
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            output.Write("{\"ts\":\"");
            output.Write(logEvent.Timestamp.UtcDateTime.ToString(_dateFormat));

            output.Write("\",\"lvl\":\"");
            if (_levelsMap != null && _levelsMap.TryGetValue(logEvent.Level.ToString(), out var level))
                output.Write(level);
            else
                output.Write(logEvent.Level.ToString());

            output.Write("\",\"msg\":");
            JsonValueFormatter.WriteQuotedJsonString(logEvent.MessageTemplate.Render(logEvent.Properties), output);

            // Вывод свойств сообщения.
            if (logEvent.Properties.Count > 0)
                WriteEventProperties(logEvent.Properties, output);

            // Вывод информации об исключении.
            if (logEvent.Exception != null)
            {
                var exceptionText = FormatException(logEvent.Exception);

                output.Write(",\"exception\":");
                JsonValueFormatter.WriteQuotedJsonString(exceptionText, output);
            }

            output.WriteLine("}");
        }

        private void WriteEventProperties(
            IReadOnlyDictionary<string, LogEventPropertyValue> properties,
            TextWriter output)
        {
            var nestedPropCount = 0;
            List<(string Key, LogEventPropertyValue Value)> hoistedProps = null;

            // Свойства связанные с событием складываем в массив props
            // формата [{ key: $propertyName, value: $propertyValue }, ...].
            // Необходимо для предотвращения mapping explosion в es:
            //
            // https://www.elastic.co/guide/en/elasticsearch/reference/current/mapping.html#mapping-limit-settings
            //
            // Есть вариант использования flattened поля в es вместо nested
            // и записывать свойства в обычный словарь, но поддержка поиска
            // по полям этого типа в кибане отсутствует.
            foreach (var (key, value) in properties)
            {
                if (_hoistingMap.TryGetValue(key, out var newName))
                {
                    hoistedProps = hoistedProps ?? new List<(string Key, LogEventPropertyValue Value)>();
                    hoistedProps.Add((newName, value));
                    continue;
                }

                output.Write(nestedPropCount > 0 ? "," : ",\"props\":[");
                WriteKeyValuePairAsObject(key, value, output);

                ++nestedPropCount;
            }

            if (nestedPropCount > 0)
                output.Write("]");

            if (hoistedProps != null)
            {
                for (var i = 0; i < hoistedProps.Count; i++)
                {
                    var (key, value) = hoistedProps[i];
                    output.Write(",");
                    WriteKeyValuePair(key, value, output);
                }
            }
        }

        private void WriteKeyValuePairAsObject(string key, LogEventPropertyValue value, TextWriter output)
        {
            // XXX: рендерится ToString() представление объекта что 
            // приводит к потере инфы при рендеринге классов и структур (зависит
            // от реализации ToString() конкретного объекта) и делает невозможными
            // поиск по вложенным полям в es.
            //
            // Есть вариант сжимать сложные объекты в одно измерение:
            //
            // исходный объект: { foo: 123, bar: { baz: 456, qux: "test" } }
            // результат:
            // $key.foo = 123
            // $key.bar.baz = 456;
            // $key.bar.qux = "test";
            output.Write("{\"key\":");
            JsonValueFormatter.WriteQuotedJsonString(key, output);
            output.Write(",\"value\":");
            JsonValueFormatter.WriteQuotedJsonString(value.ToString(), output);
            output.Write("}");
        }

        private void WriteKeyValuePair(string key, LogEventPropertyValue value, TextWriter output)
        {
            JsonValueFormatter.WriteQuotedJsonString(key, output);
            output.Write(":");
            _valueFormatter.Format(value, output);
        }

        private string FormatException(Exception e) => e.ToString().Replace("\r", "");
    }
}
