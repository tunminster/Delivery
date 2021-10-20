using System.Collections.Generic;
using Newtonsoft.Json;

namespace Delivery.Domain.Contracts.V1.RestContracts.DistanceMatrix
{
    public record DistanceMatrixResponseContract
    {
        [JsonProperty("destination_addresses")]
        public List<string> DestinationAddresses { get; init; } = new();

        [JsonProperty("origin_addresses")] 
        public List<string> OriginAddresses { get; init; } = new();
        
        public List<Row> Rows { get; init; }
        public string Status { get; init; }
    }
    
    public class Row
    {
        public List<Element> Elements { get; init; }
    }
    
    public class Element
    {
        public ValueContract Distance { get; set; }
        public ValueContract Duration { get; set; }
        public string Status { get; set; }
    }

    public record ValueContract
    {
        public string Text { get; init; } = string.Empty;
        public int Value { get; init; }
    }

    
}