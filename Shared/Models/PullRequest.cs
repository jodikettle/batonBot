namespace SharedBaton.Models
{
    using Newtonsoft.Json;

    public class PullRequest
    {
        public int number;

        public string title;

        public string body;

        public bool merged;

        public string mergeable_state;

        public HeadInfo head;

        public string GetMergeDescriptionString()
        {
            var index = this.body.IndexOf("### Testing");

            return index == -1 ? this.body.Substring(0, index) : string.Empty;
        }
    }

    public class HeadInfo
    {
        [JsonProperty("ref")]
        public string reference;
    }
}
