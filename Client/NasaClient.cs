using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceInformationBot.Model;
using SpaceInformationBot.Constant;
using Newtonsoft.Json;

namespace SpaceInformationBot.Client
{
    public class SpaceClient
    {
        private HttpClient _httpClient;
        private static string? _adrres;
        //private static string _apikey;

        public SpaceClient()
        {
            _adrres = Constants.address;
            //_apikey = Constants.apikey;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_adrres);
        }

        public async Task<APOD> GetAPODAsync()
        {
            //var response = await _httpClient.GetAsync($"/planetary/apod?api_key={_apikey}");
            var response = await _httpClient.GetAsync($"/APOD/apod");
            var content = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<APOD>(content);
            return result;
        }

        public async Task<APOD> GetAPODAsync(string date)
        {
            //var response = await _httpClient.GetAsync($"/planetary/apod?api_key={_apikey}");
            var response = await _httpClient.GetAsync($"/APOD/apodbydate?date={date}");
            var content = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<APOD>(content);
            return result;
        }
        public async Task<MarsRoverPhotos> GetMarsPhotosAsync(string date, string camera, int page = 1)
        {
            var response = await _httpClient.GetAsync($"/MarsRoverPhotos/getMarsPhotos?date={date}&camera={camera}&page={page}");
            //if (camera == "all")
            //    response = await _httpClient.GetAsync($"/MarsRoverPhotos?date={date}&page={page}");

            var content = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<MarsRoverPhotos>(content);
            return result;
        }
    }
}
