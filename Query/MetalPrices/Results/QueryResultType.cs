using System;

namespace LuisBot.Query.MetalPrices.Results
{
    [Flags]
    public enum QueryResultType
    {
        Inconclusive,
        Ok,
        NoMetalSpecified,
        NonCostFocused,
    }
}