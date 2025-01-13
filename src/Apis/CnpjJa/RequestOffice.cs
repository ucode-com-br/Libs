using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UCode.Apis.CnpjJa
{
    public class RequestOffice
    {
        public RequestOffice(string taxId)
        {
            ArgumentNullException.ThrowIfNull(taxId);

            this.TaxId = taxId;
        }

        [JsonPropertyName("taxId")]
        public string TaxId
        {
            get; private set;
        }

        [JsonPropertyName("simples")]
        public bool? Simples
        {
            get; set;
        }

        [JsonPropertyName("simplesHistory")]
        public bool? SimplesHistory
        {
            get; set;
        }

        [JsonPropertyName("registrations")]
        public List<string> Registrations
        {
            get; set;
        }

        [JsonPropertyName("suframa")]
        public bool? Suframa
        {
            get; set;
        }

        [JsonPropertyName("geocoding")]
        public bool? Geocoding
        {
            get; set;
        }

        [JsonPropertyName("links")]
        public List<string> Links
        {
            get; set;
        }

        [JsonPropertyName("strategy")]
        public string Strategy
        {
            get; set;
        }

        [JsonPropertyName("maxAge")]
        public int? MaxAge
        {
            get; set;
        }

        [JsonPropertyName("maxStale")]
        public int? MaxStale
        {
            get; set;
        }

        [JsonPropertyName("sync")]
        public bool? Sync
        {
            get; set;
        }

        
    }

}

