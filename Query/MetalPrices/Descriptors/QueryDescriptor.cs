using System;
using System.Collections.Generic;
using System.Linq;

namespace LuisBot.Query.MetalPrices.Descriptors
{
    /// <summary>
    /// Query descriptor
    /// On processing the natural language with LUIS the results are then used to populate this class
    /// The Query processor uses the information here to determine what to query from the database
    /// </summary>
    public class QueryDescriptor
    {
        #region Ctors

        ///TODO: Consider making a factory for this
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryDescriptor"/> class.
        /// </summary>
        /// <param name="isCostFocused">if set to <c>true</c> [cost focused].</param>
        /// <param name="metalNames">The metal names.</param>
        /// <param name="timelineConstraints">The timeline constraints.</param>
        /// <param name="timelineConstraintLimit">The timeline constraint limit.</param>
        /// <param name="priceFilterType">Type of the price filter.</param>
        /// <param name="useUseCurrentTimeline">if set to <c>true</c> [use current timeline].</param>
        /// <param name="numberConstraint">The number constraint.</param>
        public QueryDescriptor(bool isCostFocused, List<string> metalNames, List<DateTime> timelineConstraints, TimelineConstraintLimit timelineConstraintLimit, PriceFilterType priceFilterType, bool useUseCurrentTimeline = false, int numberConstraint = 1)
        {
            this.IsCostFocused = isCostFocused;
            this.MetalNames = metalNames;
            this.TimeLineConstraintLimit = timelineConstraintLimit;
            this.TimelineConstraints = timelineConstraints;
            this.PriceFilterType = priceFilterType;
            this.NumberConstraint = numberConstraint;
            this.UseCurrentTimeLine = useUseCurrentTimeline;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the time line constraint limit.
        /// </summary>
        /// <value>
        /// The time line constraint limit.
        /// </value>
        public TimelineConstraintLimit TimeLineConstraintLimit { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is cost focused.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is cost focused; otherwise, <c>false</c>.
        /// </value>
        public bool IsCostFocused { get; private set; }

        /// <summary>
        /// Gets the number constraint.
        /// </summary>
        /// <value>
        /// The number constraint.
        /// </value>
        public int NumberConstraint { get; private set; }

        /// <summary>
        /// Gets the type of the price filter.
        /// </summary>
        /// <value>
        /// The type of the price filter.
        /// </value>
        public PriceFilterType PriceFilterType { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has time constraints.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has time constraints; otherwise, <c>false</c>.
        /// </value>
        public bool HasTimeConstraints => this.TimelineConstraints.Any();

        /// <summary>
        /// Gets a value indicating whether this instance has price filter.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has price filter; otherwise, <c>false</c>.
        /// </value>
        public bool HasPriceFilter => this.PriceFilterType != PriceFilterType.None;

        /// <summary>
        /// Gets a value indicating whether this instance has metal names.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has metal names; otherwise, <c>false</c>.
        /// </value>
        public bool HasMetalNames => this.MetalNames.Any();

        /// <summary>
        /// Gets the timeline constraints.
        /// </summary>
        /// <value>
        /// The timeline constraints.
        /// </value>
        public List<DateTime> TimelineConstraints { get; private set; }

        /// <summary>
        /// Gets the metal names.
        /// </summary>
        /// <value>
        /// The metal names.
        /// </value>
        public List<string> MetalNames { get; }

        /// <summary>
        /// Gets a value indicating whether [current time line].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [current time line]; otherwise, <c>false</c>.
        /// </value>
        public bool UseCurrentTimeLine { get; }

        #endregion

        /// <summary>
        /// Sets the timeline to current.
        /// </summary>
        public void SetTimelineToCurrent()
        {
            this.TimelineConstraints = new List<DateTime>()
            {
                new DateTime(DateTime.Today.Year,DateTime.Today.Month,1)
            };
            this.TimeLineConstraintLimit = TimelineConstraintLimit.Month;
        }
    }
}