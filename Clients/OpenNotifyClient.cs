using SpaceInformationBot.Constant;
using SpaceInformationBot.Model;
using Newtonsoft.Json;

namespace SpaceInformationBot.Clients
{
    public class OpenNotifyClient
    {
        private HttpClient _httpClient;
        private static string? _adrres;
        

        public OpenNotifyClient()
        {
            _adrres = Constants.address;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_adrres);
        }
        public async Task<LocationOfISS?> GetLocationAsync()
        {
            var response = await _httpClient.GetAsync("/ISS/locationISS");
            var content = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<LocationOfISS>(content);
            return result;
        }

        public async Task<PeopleInSpace?> GetPeopleInSpaceAsync()
        {
            var response = await _httpClient.GetAsync("/ISS/numberOfPeopleInSpace");
            var content = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<PeopleInSpace>(content);

            return result;
        }

    }
}
