using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PSIA.Common.Soap.Objects;
using PSIA.CSEC;

namespace Adapter
{
    public class Connection
    {
        private readonly HttpClient _client = new HttpClient();
        
        private string _issueSignature;

        public Connection(string baseAddress)
        {
            _client.BaseAddress = new Uri(baseAddress);
        }
        
        public async Task<bool> CheckProfile()
        {
            var response = await _client.GetAsync("PSIA/profile");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unsuccessful HTTP status code {response.StatusCode}.");
            }

            var serializer = new XmlSerializer(typeof(PsiaProfile));
            var profileInfo = serializer.Deserialize(await response.Content.ReadAsStreamAsync()) as PsiaProfile;
            if (profileInfo == null) throw new Exception("Invalid content returned for profile information.");

            return profileInfo.profileList.Any(profile => profile.psiaProfileName == "PLAIBase");
        }

        public async Task TakeOwnership(string username, string password)
        {
            var byteArray = new UTF8Encoding().GetBytes($"{username}:{password}");
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var response = await _client.GetAsync($"/PSIA/CSEC/deviceOwnership?OwnerGUID={Guid.NewGuid()}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unsuccessful HTTP status code {response.StatusCode}.");
            }

            var serializer = new XmlSerializer(typeof(CSECOwnershipCookie));
            var ownershipInfo = serializer.Deserialize(await response.Content.ReadAsStreamAsync()) as CSECOwnershipCookie;
            if (ownershipInfo == null) throw new Exception("Invalid content returned for ownership information.");
            _issueSignature = ownershipInfo.issuerSignature;
        }
    }
}