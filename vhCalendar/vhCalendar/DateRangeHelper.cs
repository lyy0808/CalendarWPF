#region Directives
using System;
using System.ComponentModel;
#endregion

namespace vhCalendar
{
    /// <summary>
    /// An adaptation of wpf Toolkits CalendarDateRange class
    /// </summary>
    public sealed class DateRangeHelper : INotifyPropertyChanged
    {
        #region Fields
        private DateTime _dtEndDate;
        private DateTime _dtStartDate;
        #endregion

        #region Constructors
        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public DateRangeHelper() : this(DateTime.MinValue, DateTime.MaxValue)
        {
        }

        /// <summary>
        /// Rangeless constructor
        /// </summary>
        /// <param name="day"></param>
        public DateRangeHelper(DateTime day) : this(day, day)
        {
        }

        /// <summary>
        /// Range of dates defined in constructor
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public DateRangeHelper(DateTime start, DateTime end)
        {
            _dtStartDate = start;
            _dtEndDate = end;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// End of range
        /// </summary>
        public DateTime End
        {
            get { return CoerceEnd(_dtStartDate, _dtEndDate); }

            set
            {
                DateTime newEnd = CoerceEnd(_dtStartDate, value);
                if (newEnd != End)
                {
                    OnChanging(new DateRangeChangingEventArgs(_dtStartDate, newEnd));
                    _dtEndDate = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("End"));
                }
            }
        }

        /// <summary>
        /// Start of range
        /// </summary>
        public DateTime Start
        {
            get { return _dtStartDate; }

            set
            {
                if (_dtStartDate != value)
                {
                    DateTime oldEnd = End;
                    DateTime newEnd = CoerceEnd(value, _dtEndDate);

                    OnChanging(new DateRangeChangingEventArgs(value, newEnd));

                    _dtStartDate = value;

                    OnPropertyChanged(new PropertyChangedEventArgs("Start"));

                    if (newEnd != oldEnd)
                    {
                        OnPropertyChanged(new PropertyChangedEventArgs("End"));
                    }
                }
            }
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Test if a DateRange is contained in current range
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        internal bool ContainsAny(DateRangeHelper range)
        {
            return (range.End >= this.Start) && (this.End >= range.Start);
        }

        #endregion Internal Methods

        #region Private Methods
        /// <summary>
        /// Fire the DateRangeChanging event
        /// </summary>
        /// <param name="e"></param>
        private void OnChanging(DateRangeChangingEventArgs e)
        {
            EventHandler<DateRangeChangingEventArgs> handler = this.Changing;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        internal event EventHandler<DateRangeChangingEventArgs> Changing;

        /// <summary>
        /// Fire the PropertyChanged event
        /// </summary>
        /// <param name="e"></param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Compare and return latest DateTime in range
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static DateTime CoerceEnd(DateTime start, DateTime end)
        {
            return (DateTime.Compare(start, end) <= 0) ? end : start;
        }
        #endregion
    }
}
