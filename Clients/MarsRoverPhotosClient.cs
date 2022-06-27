using Amazon.DynamoDBv2;
using Newtonsoft.Json;
using SpaceInformationBot.Constant;
using SpaceInformationBot.Model;
using System.Text;

namespace SpaceInformationBot.Clients
{
    public class MarsRoverPhotosClient
    {
        private HttpClient _httpClient;
        private static string? _adrres;

        public MarsRoverPhotosClient() 
        {
            _adrres = Constants.address;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_adrres);   
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

        public async Task<MarsPhotoDB?> GetInfoAboutUserFavourites(int userID, string url)  
        {
            try
            {
                var response = await _httpClient.GetAsync($"/MarsRoverPhotos/getInfoAboutMarsPhotoFromDB?userID={userID}&url={url}");
                var content = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<MarsPhotoDB>(content);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<MarsPhotoDB>?> GetAllUserDataFromDynamoDB(int userID)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/MarsRoverPhotos/getAllUserFavouriteMarsPhotosFromDB?userID={userID}");
                var content = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<List<MarsPhotoDB>>(content);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> PostDataToDynamoDB(Photos obj, int userID) 
        {
            var data = new MarsPhotoDB
            {
                userID = userID,
                roverName = obj.rover.name,
                cameraName = obj.camera.full_name,
                earth_date = obj.earth_date,
                url = obj.img_src
            };

            var jsonfile = JsonConvert.SerializeObject(data);

            var stringContent = new StringContent(jsonfile, Encoding.UTF8, "application/json");

            try
            {
                var post = await _httpClient.PostAsync($"/MarsRoverPhotos/addFavouriteMarsPhotosToDB", stringContent);
            }
            catch(Exception)
            {
                return false;
            }

            return true;      
        }
        public async Task<bool> PostDataToDynamoDB(MarsPhotoDB obj, int userID)
        {
            //var data = new MarsPhotoDB
            //{
            //    userID = userID,
            //    roverName = obj.rover.name,
            //    cameraName = obj.camera.full_name,
            //    earth_date = obj.earth_date,
            //    url = obj.img_src
            //};

            var jsonfile = JsonConvert.SerializeObject(obj);

            var stringContent = new StringContent(jsonfile, Encoding.UTF8, "application/json");

            try
            {
                var post = await _httpClient.PostAsync($"/MarsRoverPhotos/addFavouriteMarsPhotosToDB", stringContent);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        public async Task<bool> DeleteDataFromDynamoDB(int userID, string url) 
        {
            try
            {
                await _httpClient.DeleteAsync($"/MarsRoverPhotos/deleteMarsPhotoFromDB?userID={userID}&url={url}");
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    
        public async Task<bool> DeleteAllUserDataFromDynamoDB(int userID)
        {
            try
            {
                await _httpClient.DeleteAsync($"/MarsRoverPhotos/deleteAllUserMarsPhotosFromDB?userID={userID}");
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
