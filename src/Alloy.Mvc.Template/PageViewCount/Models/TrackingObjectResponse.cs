using System.Collections.Generic;

namespace PageViewCount.Models
{
    public class TrackingObjectResponse
    {
        public int Total { get; set; }
        public int Count { get; set; }
        public List<Item> Items { get; set; }
    }

    public class Epi
    {
        public string contentGuid { get; set; }
        public string language { get; set; }
    }

    public class Payload
    {
        public Epi epi { get; set; }
    }

    public class Item
    {
        public Payload Payload { get; set; }
    }
  
}
