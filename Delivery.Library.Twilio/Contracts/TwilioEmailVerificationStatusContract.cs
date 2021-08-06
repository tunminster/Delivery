using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Delivery.Library.Twilio.Contracts
{
    /// <summary>
    ///  Twilio email version status
    /// </summary>
    public record TwilioEmailVerificationStatusContract
    {
        public string Sid { get; init; } = string.Empty;

        [JsonPropertyName("service_sid")]
        public string ServiceId { get; init; } = string.Empty;

        [JsonPropertyName("account_sid")]
        public string AccountId { get; init; } = string.Empty;

        public string To { get; init; } = string.Empty;

        public string Channel { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;
        
        public bool Valid { get; init; }
        
        [JsonPropertyName("date_created")]
        public DateTimeOffset DateCreated { get; init; }
        
        [JsonPropertyName("date_updated")]
        public DateTimeOffset DateUpdated { get; init; }

        public CarrierContract Lookup { get; init; } = new();
        
        public string? Amount { get; init; }
        
        public string? Payee { get; init; }

        public List<AttemptsContract> SendCodeAttempts { get; init; } = new();

        public string Url { get; init; } = string.Empty;
    }

    public record CarrierContract
    {
        [JsonPropertyName("error_code")]
        public string ErrorCode { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("mobile_country_code")]
        public string MobileCountryCode { get; init; } = string.Empty;

        [JsonPropertyName("mobile_network_code")]
        public string MobileNetworkCode { get; init; } = string.Empty;

        public string Type { get; init; } = string.Empty;
    }

    public record AttemptsContract
    {
        [JsonPropertyName("time")]
        public DateTimeOffset DateAttempted { get; init; }

        public string Channel { get; init; } = string.Empty;
        
        [JsonPropertyName("channel_id")]
        public string? ChannelId { get; init; }
    }
}