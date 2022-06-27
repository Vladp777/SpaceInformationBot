using Amazon.DynamoDBv2;
using Newtonsoft.Json;
using SpaceInformationBot.Constant;
using SpaceInformationBot.Model;
using System.Text;

namespace SpaceInformationBot.Clients
{
    public class MarsPhotoDBClient
    {
        private HttpClient _httpClient;
        private static string? _adrres;

        public MarsPhotoDBClient() 
        {
            _adrres = Constants.address;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_adrres);   
        }

        public async Task<MarsPhotoDB?> GetInfoAboutUserFavourites(int userID, string url)  
        {
            try
            {
                var response = await _httpClient.GetAsync($"/MarsPhotoDB/getInfoAboutMarsPhotoFromDB?userID={userID}&url={url}");
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
                var response = await _httpClient.GetAsync($"/MarsPhotoDB/getAllUserFavouriteMarsPhotosFromDB?userID={userID}");
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
                var post = await _httpClient.PostAsync($"/MarsPhotoDB/addFavouriteMarsPhotosToDB", stringContent);
            }
            catch(Exception)
            {
                return false;
            }

            return true;      
        }

        public async Task<bool> DeleteDataFromDynamoDB(int userID, string url) 
        {
            try
            {
                await _httpClient.DeleteAsync($"/MarsPhotoDB/deleteMarsPhotoFromDB?userID={userID}&url={url}");
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
                await _httpClient.DeleteAsync($"/MarsPhotoDB/deleteAllUserMarsPhotosFromDB?userID={userID}");
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
