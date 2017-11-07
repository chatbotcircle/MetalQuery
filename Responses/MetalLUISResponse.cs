using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.Responses
{

    ///TODO: Configure serializer to user C# casing standard
    /// <summary>
    /// The query response message
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MetalLUISResponse
    {
        /// <summary>
        /// Gets or sets the query.
        /// </summary>
        /// <value>
        /// The query.
        /// </value>
        public string query { get; set; }

        /// <summary>
        /// Gets or sets the intents.
        /// </summary>
        /// <value>
        /// The intents.
        /// </value>
        public Intent[] intents { get; set; }

        /// <summary>
        /// Gets or sets the entities.
        /// </summary>
        /// <value>
        /// The entities.
        /// </value>
        public Entity[] entities { get; set; }
    }
}