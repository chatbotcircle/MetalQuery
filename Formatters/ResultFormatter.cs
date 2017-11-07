
namespace LuisBot.Formatters
{
    public static class ResultFormatter
    {

        /// <summary>
        /// Formats the result.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static string FormatResult(object entity)
        {
            var price = entity as price;
            return
                $"{price.metal.name} ({price.metal.code}) in {price.date:MMMM yyyy} at **{price.price1:C}**.";
        }

        /// <summary>
        /// Determines whether this instance can format the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if this instance can format the specified target; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanFormat(object target)
        {
            return target is metal;
        }
    }
}