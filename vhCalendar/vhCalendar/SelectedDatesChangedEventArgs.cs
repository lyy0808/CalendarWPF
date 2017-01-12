using System;
using System.Windows;
using System.Collections.ObjectModel;

namespace vhCalendar
{
    /// <summary>
    /// Routed event args for SelectedDatesChanged
    /// </summary>
    public class SelectedDatesChangedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Constructor for the event args
        /// </summary>
        /// <param name="routedEvent">The event for which the args will be passed</param>
        public SelectedDatesChangedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
        /// <summary>
        /// Gets/Sets the new date collection
        /// </summary>
        public ObservableCollection<DateTime> NewDates { get; set; }
        /// <summary>
        /// Gets/Sets the old date collection
        /// </summary>
        public ObservableCollection<DateTime> OldDates { get; set; }
    }

    /// <summary>
    /// Delegate for the SelectedDatesChanged event
    /// </summary>
    /// <param name="sender">The object that raised the event</param>
    /// <param name="e">Event arguments for the SelectedDatesChanged event</param>
    public delegate void SelectedDatesChangedEventHandler(object sender, SelectedDatesChangedEventArgs e);
}
