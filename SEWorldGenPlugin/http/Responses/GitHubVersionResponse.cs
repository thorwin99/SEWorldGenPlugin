using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace SEWorldGenPlugin.http.Responses
{
    /// <summary>
    /// Class to deserialize the json response that comes from githubs latest release http request
    /// </summary>
    public class GitHubVersionResponse
    {
        public static JsonRelease Deserialize(Stream json)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(JsonRelease));
            JsonRelease r = (JsonRelease)serializer.ReadObject(json);
            return r;
        }
    }

    /// <summary>
    /// Representation of the latest release json response from github
    /// </summary>
    [DataContract]
    public class JsonRelease
    {
        [DataMember]
        public string url { get; set; }

        [DataMember]
        public string assets_url { get; set; }

        [DataMember]
        public string upload_url { get; set; }

        [DataMember]
        public string html_url { get; set; }

        [DataMember]
        public int id { get; set; }

        [DataMember]
        public string node_id { get; set; }

        [DataMember]
        public string tag_name { get; set; }

        [DataMember]
        public string target_commitish { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public bool draft { get; set; }

        [DataMember]
        public JsonUser author { get; set; }

        [DataMember]
        public bool prerelease { get; set; }

        [DataMember]
        public string created_at { get; set; }

        [DataMember]
        public string published_at { get; set; }

        [DataMember]
        public IList<JsonAsset> assets { get; set; }
    }

    /// <summary>
    /// Representation of the json user object of the json github response
    /// </summary>
    [DataContract]
    public class JsonUser
    {
        [DataMember]
        public string login { get; set; }

        [DataMember]
        public int id { get; set; }

        [DataMember]
        public string node_id { get; set; }

        [DataMember]
        public string avatar_url { get; set; }

        [DataMember]
        public string gravatar_id { get; set; }

        [DataMember]
        public string url { get; set; }

        [DataMember]
        public string html_url { get; set; }

        [DataMember]
        public string followers_url { get; set; }

        [DataMember]
        public string following_url { get; set; }

        [DataMember]
        public string gists_url { get; set; }

        [DataMember]
        public string starred_url { get; set; }

        [DataMember]
        public string subscriptions_url { get; set; }

        [DataMember]
        public string organizations_url { get; set; }

        [DataMember]
        public string repos_url { get; set; }

        [DataMember]
        public string events_url { get; set; }

        [DataMember]
        public string received_events_url { get; set; }

        [DataMember]
        public string type { get; set; }

        [DataMember]
        public bool site_admin { get; set; }
    }

    /// <summary>
    /// Representation of the json asset object in the github latest release json response
    /// </summary>
    [DataContract]
    public class JsonAsset
    {
        [DataMember]
        public string url { get; set; }

        [DataMember]
        public string browser_download_url { get; set; }

        [DataMember]
        public int id { get; set; }

        [DataMember]
        public string node_id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string label { get; set; }

        [DataMember]
        public string state { get; set; }

        [DataMember]
        public string content_type { get; set; }

        [DataMember]
        public int size { get; set; }

        [DataMember]
        public int download_count { get; set; }

        [DataMember]
        public string created_at { get; set; }

        [DataMember]
        public string updated_at { get; set; }

        [DataMember]
        public JsonUser uploader { get; set; }
    }
}
