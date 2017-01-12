using System;

namespace vhCalendar
{
    /// <summary>
    /// DateRangeChanging event arguments
    /// </summary>
    internal class DateRangeChangingEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor for DateRangeChanging EventArgs
        /// </summary>
        /// <param name="start">Start Date</param>
        /// <param name="end">End Date</param>
        public DateRangeChangingEventArgs(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }
        /// <summary>
        /// Gets/Sets the start date that was set
        /// </summary>
        public DateTime Start { get; set; }
        /// <summary>
        /// Gets/Sets the end date that was set
        /// </summary>
        public DateTime End { get; set; }
    }
}
