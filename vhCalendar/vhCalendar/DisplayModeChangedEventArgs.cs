using System;
using System.Windows;

namespace vhCalendar
{
    /// <summary>
    /// Routed event args for DisplayModeChanged
    /// </summary>
    public class DisplayModeChangedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Constructor for the DisplayModeChanged event args
        /// </summary>
        /// <param name="routedEvent">The event for which the args will be passed</param>
        public DisplayModeChangedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
        /// <summary>
        /// Gets/Sets the new date that was set
        /// </summary>
        public DisplayType NewMode { get; set; }
        /// <summary>
        /// Gets/Sets the old date that was set
        /// </summary>
        public DisplayType OldMode { get; set; }
    }

    /// <summary>
    /// Delegate for the DisplayModeChanged event
    /// </summary>
    /// <param name="sender">The object that raised the event</param>
    /// <param name="e">Event arguments for the DisplayModeChanged event</param>
    public delegate void DisplayModeChangedEventHandler(object sender, DisplayModeChangedEventArgs e);
}
