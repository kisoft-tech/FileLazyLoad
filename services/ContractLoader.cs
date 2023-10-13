using MainApp.common;
using MainApp.models;
using Newtonsoft.Json;

namespace MainApp.services
{
    public class ContractLoader : IContractLoader
    {
        private readonly Func<JsonTextReader> _readerProvider;

        public ContractLoader(Func<JsonTextReader> readerProvider)
        {
            _readerProvider = readerProvider ?? throw new ArgumentNullException(nameof(readerProvider));
        }

        public Contract LoadContract()
        {
            try
            {
                var contract = new Contract
                {
                    Coverage = new List<Coverage>()
                };

                using (var jsonTextReader = _readerProvider.Invoke())
                {
                    var jsonSerializer = new JsonSerializer();

                    while (jsonTextReader.Read())
                    {
                        switch (jsonTextReader.TokenType)
                        {
                            case JsonToken.PropertyName:
                                switch (jsonTextReader.Value?.ToString())
                                {
                                    case "Coverage":
                                        jsonTextReader.Read();
                                        if (jsonTextReader.TokenType == JsonToken.StartArray)
                                        {
                                            // Read each coverage object individually
                                            while (jsonTextReader.Read() && jsonTextReader.TokenType != JsonToken.EndArray)
                                            {
                                                var coverage = jsonSerializer.Deserialize<Coverage>(jsonTextReader);
                                                if (coverage != null) contract.Coverage.Add(coverage);
                                            }
                                        }
                                        else
                                        {
                                            throw new JsonException("Coverage property must be an array");
                                        }
                                        break;
                                    case "MaxAmount":
                                        jsonTextReader.Read();
                                        contract.MaxAmount = Convert.ToInt32(jsonTextReader.Value);
                                        break;
                                }
                                continue;
                            case JsonToken.None:
                            case JsonToken.StartObject:
                            case JsonToken.StartArray:
                            case JsonToken.StartConstructor:
                            case JsonToken.Comment:
                            case JsonToken.Raw:
                            case JsonToken.Integer:
                            case JsonToken.Float:
                            case JsonToken.String:
                            case JsonToken.Boolean:
                            case JsonToken.Null:
                            case JsonToken.Undefined:
                            case JsonToken.EndObject:
                            case JsonToken.EndArray:
                            case JsonToken.EndConstructor:
                            case JsonToken.Date:
                            case JsonToken.Bytes:
                                continue;
                            default:
                                break;
                        }
                    }
                }

                if (contract.Coverage == null || contract.MaxAmount <= 0)
                {
                    throw new AggregateException("Invalid contract data");
                }

                return contract;
            }
            catch (IOException ex)
            {
                throw new IOException($"An IO exception occurred while reading the file: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new JsonException($"An error occurred while deserializing the JSON data: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new AggregateException($"An error occurred: {ex.Message}", ex);
            }
        }
    }
}
