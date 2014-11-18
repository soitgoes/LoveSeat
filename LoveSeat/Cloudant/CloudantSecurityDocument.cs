using System.Collections.Generic;
using System.Linq;
using LoveSeat.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoveSeat.Cloudant
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CloudantSecurityDocument : IBaseObjectMin, IBaseResponse
    {
        private CloudantSecuritySection cloudantSecuritySection = new CloudantSecuritySection();
        private bool ok = true;

        public CloudantSecurityDocument()
        {
            Id = "_security";
        }

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("_rev", NullValueHandling = NullValueHandling.Ignore)]
        public string Rev { get; set; }

        [JsonProperty("cloudant")]
        public CloudantSecuritySection CloudantSecuritySection
        {
            get { return cloudantSecuritySection; }
            set { cloudantSecuritySection = value; }
        }

        [JsonProperty("ok")]
        public bool Ok
        {
            get { return ok; }
            set { ok = value; }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class CloudantSecuritySection
    {
        private IDictionary<string, JToken> userRoleAssignments = new Dictionary<string, JToken>();

        public CloudantSecuritySection()
        {

        }

        [JsonExtensionData()]
        private IDictionary<string, JToken> UserRoleAssignments
        {
            get { return userRoleAssignments; }
            set { userRoleAssignments = value; }
        }

        public string[] GetAssignment(string username)
        {
            return ((JArray) userRoleAssignments[username]).ToObject<string[]>();
        }

        public void AddUser(string username, params string[] roles)
        {
            userRoleAssignments.Add(username, new JArray(roles));
        }

    }
}