using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.Responses
{
    /// <summary>
    /// Query response intent
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Intent
    {
        /// <summary>
        /// Gets or sets the intent.
        /// </summary>
        /// <value>
        /// The intent.
        /// </value>
        public string intent { get; set; }

        /// <summary>
        /// Gets or sets the score.
        /// </summary>
        /// <value>
        /// The score.
        /// </value>
        public float score { get; set; }
    }
}