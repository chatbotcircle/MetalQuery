using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;
using System.Configuration;
using System.Net;
using LuisBot.Formatters;
using LuisBot.LUISClient;
using LuisBot.Query.MetalPrices.Descriptors;
using LuisBot.Query.MetalPrices.Processors;
using LuisBot.Query.MetalPrices.Results;
using System.Text;
using System.Linq;
using System;
using System.Collections.Generic;
using LuisBot;

namespace Microsoft.Bot.Sample.LuisBot.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly string botName;

        public MessagesController()
        {
            botName = ConfigurationManager.AppSettings["BotName"];
        }
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            // check if activity is of type message
            if (activity.GetActivityType() == ActivityTypes.Message)
            {
                //await Conversation.SendAsync(activity, () => new BasicLuisDialog());
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                //Avoid dead air, respond as typing
                var typing = activity.CreateReply();
                typing.Type = "typing";
                typing.From.Name = botName;
                await connector.Conversations.ReplyToActivityAsync(typing);

                var responseMessage = await HandleCustomMessage(activity);
                var responsePacket = activity.CreateReply(responseMessage);
                responsePacket.TextFormat = "markdown";
                responsePacket.From.Name = botName;
                await connector.Conversations.ReplyToActivityAsync(responsePacket);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            //return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }


        /// <summary>
        /// Handles the custom message. The message will have the query information
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns></returns>
        private static async Task<string> HandleCustomMessage(Activity activity)
        {
            //Get natural language processed by LUIS


            var luisResponse = await LUISMetalQueryClient.ParseUserInput(activity.Text);
            var responseMessage = string.Empty;

            // what is the lowest for gold in 20/10/2017 
            int isDateRange = 0;
            var metals = luisResponse.entities.Where(x => x.type == "metal").Select(x => x.entity).ToList();
            var numbers = luisResponse.entities.Where(x => x.type == "builtin.number").Select(x => x.resolution.value).ToList();

            List<string> dates = new List<string>();
            dates = luisResponse.entities.Where(x => x.type == "builtin.datetimeV2.date").Select(x => x.entity).ToList();

            if (dates.Count == 0)
            {
                dates = luisResponse.entities.Where(x => x.type == "builtin.datetimeV2.daterange").Select(x => x.entity).ToList();
                isDateRange = 1;
            }

            List<DateTime> parsedDates = new List<DateTime>();


            if (isDateRange == 0)
            {
                parsedDates = dates.Select(x => DateTime.Parse(x)).ToList();
            }
            else
            {
                parsedDates = dates.Select(x => x.Length > 4  ? DateTime.Parse(x) : new DateTime(int.Parse(x), 1, 1)).ToList();
            }
           
            var isPriceOriented = luisResponse.entities.Any(x => x.type == "cost");
            var requestsLowest = luisResponse.entities.Any(x => x.type == "lowest");
            var requestsHighest = luisResponse.entities.Any(x => x.type == "highest");
            var requestAverage = luisResponse.entities.Any(x => x.type == "average");

            DateTime maxDate;
            DateTime minDate;
            using (var dataContext = new botEntities())
            {
                maxDate = dataContext.prices.Max(x => x.date);
                minDate = dataContext.prices.Min(x => x.date);
            }

            switch (luisResponse.intents[0].intent)
            {
                case "Greeting":
                    responseMessage = "Hi there, how can I help you?";
                    return responseMessage;


                case "Metal Sales":
                    var showHelp = luisResponse.entities.Any(x => x.type == "help" || x.type == "greeting");
                    // should check intents not entities

                    if (showHelp)
                    {
                        responseMessage = $"Hi I am **Metal Bot**, I can assist you with sales of Metal.  {Environment.NewLine}You can ask me something like 'What is the average sale for 2010?' or 'What was the highest sale in USA,Widwest'.  {Environment.NewLine}**Give me a try!**  {Environment.NewLine}![logo](https://pbs.twimg.com/profile_images/803953567686131712/8qIowSgm_400x400.jpg)";
                        return responseMessage;
                    }


                    if (parsedDates.Any() && parsedDates.All(x => !(minDate <= x && maxDate >= x)))
                    {
                        responseMessage = $"Sorry I only have data from {minDate:MMMM yyyy} to {maxDate:MMMM yyyy}";
                        return responseMessage;
                    }

                    PriceFilterType priceFilter;
                    if (requestsLowest)//Intentionally done first, in case both exist i.e worst expensive
                    {
                        priceFilter = PriceFilterType.Least;
                    }
                    else if (requestsHighest)
                    {
                        priceFilter = PriceFilterType.Most;
                    }
                    else if (requestAverage)
                    {
                        priceFilter = PriceFilterType.Average;
                    }
                    else
                    {
                        priceFilter = PriceFilterType.None;
                    }

                    //Question 'What is the cost of gold in January 2015' has 2015 as part of date and number
                    numbers.RemoveAll(x => x.Length == 4 && dates.Any(d => d.Contains(x)));

                    var hasMonths = false;
                    if (dates.Any())
                    {
                        hasMonths = dates.Any(x => x.Contains('-') || x.Length> 4);
                    }

                    var number = 1;
                    //TODO: What if user gives more than one number
                    if (numbers.Any())
                    {
                        number = int.Parse(numbers.First());
                    }

                    var queryDescriptor = new QueryDescriptor(
                          isPriceOriented,
                          metals,
                          parsedDates,
                          hasMonths ? TimelineConstraintLimit.Month : TimelineConstraintLimit.Year,
                          priceFilter,
                          !dates.Any(),
                          number
                          );
                    var result = QueryProcessor.Process(queryDescriptor);

                    var failedToEvaluate = EvaluateResultStatus(out responseMessage, result.QueryResultType);
                    if (failedToEvaluate) return responseMessage;

                    if(result.Average != 0)
                    {
                        if (hasMonths)
                            responseMessage = string.Format("The average price of {0} for {1} {2} is ${3} ", metals[0], parsedDates[0].ToString("MMMM"),parsedDates[0].Year, result.Average.ToString());
                        else
                            responseMessage = string.Format("The average price of {0} for {1} is ${2} ", metals[0], parsedDates[0].Year, result.Average.ToString()); 
                    }
                    else
                    responseMessage = FormatResult(result.Prices.ToList(), number);
               

                    break;
                case "Price":
                    //what is the price of copper in April 2017?
                    break;
          
                case "Help":
                    responseMessage = "I can help you with finding the price of a metal in a given month, the average price over a year, the highest price and the lowest price.";
                    break;
                case "None":
                    responseMessage = "Sorry, I don't understand, perhaps try something like \"What was the price of gold in April 2017?\"";
                    break;
            }
            return responseMessage;
        }

        /// <summary>
        /// Evaluates the result status and creates a response message.
        /// </summary>
        /// <param name="responseMessage">The response message.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <returns></returns>
        private static bool EvaluateResultStatus(out string responseMessage, QueryResultType resultType)
        {
            switch (resultType)
            {
                case QueryResultType.Inconclusive:
                    responseMessage = $"Sorry I failed to understand your request.";
                    return true;
                case QueryResultType.NoMetalSpecified:
                    responseMessage = $"It appears you did not specify a metal for me to query.";
                    return true;
                case QueryResultType.NonCostFocused:
                    responseMessage = $"It appears you did not ask a question that is cost related.";
                    return true;
            }
            responseMessage = string.Empty;
            return false;
        }

        /// <summary>
        /// Formats the result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="number">The number.</param>
        /// <param name="responseMessage">The response message.</param>
        /// <returns></returns>
        private static string FormatResult(List<price> result, int number)
        {
            var resultMessageBuilder = new StringBuilder();
            for (var index = 0; index < result.Count; index++)
            {
                var mod = string.Empty;
                if (result.Count > 1)
                    mod = number > 1 ? $"{index}. " : "● ";
                var price = result[index];
                resultMessageBuilder.Append($"{mod}{ResultFormatter.FormatResult(price)}  {Environment.NewLine}");
            }
            return resultMessageBuilder.ToString();
        }

        /// <summary>
        /// Handles the system message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}