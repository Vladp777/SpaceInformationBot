using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NASAInformationBot.Constant;
using NASAInformationBot.Model;
using Newtonsoft.Json;

namespace NASAInformationBot.Client
{
    public class APODClient
    {
        private HttpClient _httpClient;
        private static string _adrres;
        //private static string _apikey;

        public APODClient()
        {
            _adrres = Constants.address;
            //_apikey = Constants.apikey;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_adrres);
        }

        public async Task<APODModel> GetAPODAsync()
        {
            //var response = await _httpClient.GetAsync($"/planetary/apod?api_key={_apikey}");
            var response = await _httpClient.GetAsync($"/APOD");
            var content = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<APODModel>(content);
            return result;
        }

        public async Task<APODModel> GetAPODAsync(string date)
        {
            //var response = await _httpClient.GetAsync($"/planetary/apod?api_key={_apikey}");
            var response = await _httpClient.GetAsync($"/APODbyDate?date={date}");
            var content = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<APODModel>(content);
            return result;
        }
    }
}
