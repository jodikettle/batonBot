namespace SharedBaton.Models
{
    using Newtonsoft.Json;

    public class PullRequest
    {
        public int number;

        public string body;

        public bool merged;

        public string mergable_state;

        public HeadInfo head;

        public string GetMergeDecriptionString()
        {
            var index = this.body.IndexOf("### Testing");
            return this.body.Substring(0, index);
        }
    }

    public class HeadInfo
    {
        [JsonProperty("ref")]
        public string reference;
    }
}
