#define Mca //Gamma, Mca
using System.Text.Json.Serialization;

namespace MMBotGA.dto
{
    public class Info
    {
#if Gamma
        [JsonPropertyName("Budget.current")]
        public double BudgetCurrent { get; set; }

        [JsonPropertyName("Budget.max")]
        public double BudgetMax { get; set; }

        [JsonPropertyName("Budget.not_traded")]
        public double BudgetNotTraded { get; set; }

        [JsonPropertyName("Position")]
        public double Position { get; set; }

        [JsonPropertyName("Price.last")]
        public double PriceLast { get; set; }

        [JsonPropertyName("Price.neutral")]
        public double PriceNeutral { get; set; }
#elif Mca
        [JsonPropertyName("Assets")]
        public double Assets { get; set; }

        [JsonPropertyName("Budget")]
        public double Budget { get; set; }

        [JsonPropertyName("Currency")]
        public double Currency { get; set; }

        [JsonPropertyName("Enter price")]
        public double EnterPrice { get; set; }

        [JsonPropertyName("Enter price sum")]
        public double EnterPriceSum { get; set; }
#endif
    }
}
