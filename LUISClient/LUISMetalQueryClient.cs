using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Configuration;
using Newtonsoft.Json;
using LuisBot.Responses;
using Microsoft.Bot.Sample.LuisBot;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace LuisBot.LUISClient
{
    /// <summary>
    /// The metal query client
    /// Dispatches all queries to the LUIS API and handles response
    /// </summary>
    public static class LUISMetalQueryClient
    {
        public static async Task<MetalLUISResponse> ParseUserInput(string rawQuery)
        {
            IDialogContext context = null;
            LuisResult result = new LuisResult();
            BasicLuisDialog luis = new BasicLuisDialog();
            //luis.Grreting(context,result).RunSynchronously();

            var escapedQuery = Uri.EscapeDataString(rawQuery);

            using (var client = new HttpClient())
            {
                var endpointAddress = ConfigurationManager.AppSettings["MetalQueryApiEndPoint"];
                string request = $"{endpointAddress}{escapedQuery}";
                var response = await client.GetAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var deserializedResponse = JsonConvert.DeserializeObject<MetalLUISResponse>(jsonResponse);
                    return deserializedResponse;
                }
            }
            return null;
        }
    }
}