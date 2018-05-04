using Newtonsoft.Json;

// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var data = DonorDataModel.FromJson(jsonString);

namespace WireJunky.ExtraLife.DonorData
{
    public partial class DonorDataModel
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("participantID")]
        public long ParticipantId { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("donorID")]
        public string DonorId { get; set; }

        [JsonProperty("avatarImageURL")]
        public string AvatarImageUrl { get; set; }

        [JsonProperty("createdDateUTC")]
        public string CreatedDateUtc { get; set; }

        [JsonProperty("teamID")]
        public long TeamId { get; set; }
    }

    public partial class DonorDataModel
    {
        public static DonorDataModel[] FromJson(string json) => JsonConvert.DeserializeObject<DonorDataModel[]>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this DonorDataModel[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}
