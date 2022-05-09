using System.Text.Json.Serialization;

namespace MMBotGA.dto
{
    public class McaStrategy : IStrategy
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("buyStrength")]
        public double BuyStrength { get; set; }

        [JsonPropertyName("sellStrength")]
        public double SellStrength { get; set; }

        [JsonPropertyName("initBet")]
        public double InitBet { get; set; }

        [JsonPropertyName("invert_proxy")]
        public bool InvertProxy { get; set; }
    }
}
