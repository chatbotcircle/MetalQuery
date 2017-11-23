using System.Linq;
using LinqKit;
using LuisBot.Query.MetalPrices.Descriptors;
using LuisBot.Query.MetalPrices.Results;
using System.Collections.Generic;
using System;

namespace LuisBot.Query.MetalPrices.Processors
{
    /// <summary>
    /// Handles query processing
    /// </summary>
    public static class QueryProcessor
    {
        /// <summary>
        /// Processes the query.
        /// </summary>
        public static QueryResults Process(QueryDescriptor query)
        {
            var queryResult = new QueryResults();

#pragma warning disable 618
            var predicate = PredicateBuilder.False<price>();
#pragma warning restore 618

            if (!query.IsCostFocused && !query.HasPriceFilter && !query.HasTimeConstraints)
            {
                queryResult.SetResultState(QueryResultType.NonCostFocused);
                return queryResult;
            }

            if (!query.HasMetalNames && !query.HasPriceFilter)
            {
                queryResult.SetResultState(QueryResultType.NoMetalSpecified);
                return queryResult;
            }

            //botEntities is a EntityDataContext
            var dataContext = new botEntities();
            //What is the price of gold
            //What is the current price of gold
            //What is the price of gold now
            //What is the price of gold today
            //  Current price

            //What is the price of gold in 2015
            //What is the price of gold in January 2015
            if (!query.HasPriceFilter)
            {
                foreach (string keyword in query.MetalNames)
                {
                    string temp = keyword;
                    predicate = predicate.Or(p => p.metal.name.Equals(temp));
                }

                if (!query.HasTimeConstraints || query.UseCurrentTimeLine)
                {
                    query.SetTimelineToCurrent();
                }

                foreach (var time in query.TimelineConstraints)
                {
                    predicate = query.TimeLineConstraintLimit == TimelineConstraintLimit.Month ? predicate.And(p => (p.date.Year == time.Year && p.date.Month == time.Month)) : predicate.And(p => (p.date.Year == time.Year));
                }

                queryResult.Prices = dataContext.prices.AsExpandable().Where(predicate);
                queryResult.SetResultState(QueryResultType.Ok);
                return queryResult;
            }

            //When was gold most expensive
            //When was gold cheapest
            //  Open specific metal filter
            //When was gold most expensive in 2015
            //  Restricted specific metal filter
            //What metal was the most expensive in 2015
            //Which metal was the cheapest in 2015
            if (query.HasPriceFilter && query.MetalNames.Count <= 1)
            {
                var keyword = query.MetalNames.FirstOrDefault();
                if (keyword != null)
                {
                    predicate = predicate.Or(p => p.metal.name.Equals(keyword));
                }

                foreach (var time in query.TimelineConstraints)
                {
                    predicate = query.TimeLineConstraintLimit == TimelineConstraintLimit.Month ? predicate.And(p => (p.date.Year == time.Year && p.date.Month == time.Month)) : predicate.And(p => (p.date.Year == time.Year));
                }

                if (query.PriceFilterType == PriceFilterType.Least)
                {
                    queryResult.Prices =
                        dataContext.prices.AsExpandable()
                            .Where(predicate)
                            .OrderBy(t => t.price1)
                            .Take(query.NumberConstraint);
                }
                else if (query.PriceFilterType == PriceFilterType.Most)
                {
                    queryResult.Prices =
                        dataContext.prices.AsExpandable()
                            .Where(predicate)
                            .OrderByDescending(t => t.price1)
                            .Take(query.NumberConstraint);
                }
                else if (query.PriceFilterType == PriceFilterType.Average)
                {
                    DateTime date = query.TimelineConstraints[0].Date;
                    string myMetalName = query.MetalNames[0].ToString();
                    var mymetal = dataContext.metals.Where(x => x.name == myMetalName).Select(x => x.metal_id).SingleOrDefault();
                    decimal averageprice = 0 ;
                    if (query.TimeLineConstraintLimit == TimelineConstraintLimit.Year)
                    {
                         averageprice = dataContext.prices.Where(r => r.metal_id_fk == mymetal).Average(r => r.price1);

                    }
                    else if (query.TimeLineConstraintLimit == TimelineConstraintLimit.Month)
                    {
                         averageprice = dataContext.prices.Where(r => r.metal_id_fk == mymetal && r.date == date).Average(r => r.price1);
                    }
                        queryResult.Average = averageprice;
                    //queryResult.Prices =  (new List<price> { new price { date = query.TimelineConstraints[0], metal = new metal { }, price1 = averageprice, metal_id_fk = mymetal, price_id = 0 }}).AsQueryable() ;
                }

                queryResult.SetResultState(QueryResultType.Ok);
                return queryResult;
            }

            queryResult.SetResultState(QueryResultType.Inconclusive);
            return queryResult;
        }

    }
}