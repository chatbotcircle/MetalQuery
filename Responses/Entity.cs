
namespace LuisBot.Responses
{
    /// <summary>
    /// Query response entity
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Entity
    {
        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public string entity { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string type { get; set; }

        /// <summary>
        /// Gets or sets the start index.
        /// </summary>
        /// <value>
        /// The start index.
        /// </value>
        public int startIndex { get; set; }

        /// <summary>
        /// Gets or sets the end index.
        /// </summary>
        /// <value>
        /// The end index.
        /// </value>
        public int endIndex { get; set; }

        /// <summary>
        /// Gets or sets the score.
        /// </summary>
        /// <value>
        /// The score.
        /// </value>
        public float score { get; set; }

        /// <summary>
        /// Gets or sets the resolution.
        /// </summary>
        /// <value>
        /// The resolution.
        /// </value>
        public Resolution resolution { get; set; }
    }
}