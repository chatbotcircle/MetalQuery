using System.Linq;

namespace LuisBot.Query.MetalPrices.Results
{
    public class QueryResults
    {
        public void SetResultState(QueryResultType queryResultType)
        {
            this.QueryResultType = queryResultType;
        }

        public QueryResultType QueryResultType { get; set; }

        public IQueryable<price> Prices { get; set; }

        public decimal Average { get; set; }
    }
}