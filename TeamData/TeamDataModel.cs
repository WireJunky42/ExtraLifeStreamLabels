using System;
using Newtonsoft.Json;

namespace WireJunky.ExtraLife.TeamData
{
    public partial class TeamDataModel: IDisposable
    {
        [JsonProperty("fundraisingGoal")]
        public double FundraisingGoal { get; set; }

        [JsonProperty("eventName")]
        public string EventName { get; set; }

        [JsonProperty("avatarImageURL")]
        public string AvatarImageUrl { get; set; }

        [JsonProperty("createdDateUTC")]
        public string CreatedDateUtc { get; set; }

        [JsonProperty("eventID")]
        public long EventId { get; set; }

        [JsonProperty("sumDonations")]
        public double SumDonations { get; set; }

        [JsonProperty("teamID")]
        public long TeamId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("numDonations")]
        public long NumDonations { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public partial class TeamDataModel
    {
        public static TeamDataModel FromJson(string json) => JsonConvert.DeserializeObject<TeamDataModel>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this TeamDataModel self) => JsonConvert.SerializeObject(self, Converter.Settings);
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
