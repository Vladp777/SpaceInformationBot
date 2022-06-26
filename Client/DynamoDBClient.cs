using Amazon.DynamoDBv2;
using Newtonsoft.Json;
using SpaceInformationBot.Constant;
using SpaceInformationBot.Model;
using System.Text;

namespace SpaceInformationBot.Clients
{
    public class DynamoDBClient
    {
        private HttpClient _httpClient;
        private static string? _adrres;

        public DynamoDBClient() 
        {
            _adrres = Constants.address;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_adrres);   
        }

        public async Task<APOD?> GetInfoAboutUserFavourites(int userID, string url)  
        {
            try
            {
                var response = await _httpClient.GetAsync($"/DynamoDB/getInfoAboutItemFromDB?userID={userID}&url={url}");
                var content = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<APOD>(content);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<APOD>?> GetAllUserDataFromDynamoDB(int userID)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/DynamoDB/getAllUserFavouritesFromDB?userID={userID}");
                var content = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<List<APOD>>(content);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> PostDataToDynamoDB(APOD obj, int userID) 
        {
            var data = new DB_object
            {
                userID = userID,
                //messageID = db_object.messageID,
                title = obj.title,
                explanation = obj.explanation,
                date = obj.date,
                url = obj.url
            };

            var jsonfile = JsonConvert.SerializeObject(data);

            var stringContent = new StringContent(jsonfile, Encoding.UTF8, "application/json");

            try
            {
                var post = await _httpClient.PostAsync($"/DynamoDB/addFavouriteToDB", stringContent);
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
                await _httpClient.DeleteAsync($"/DynamoDB/deleteFromDB?userID={userID}&url={url}");
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
                await _httpClient.DeleteAsync($"/DynamoDB/deleteAllUserDataFromDB?userID={userID}");
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
