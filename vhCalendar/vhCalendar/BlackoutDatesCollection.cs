#region Directives
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
#endregion

namespace vhCalendar
{
    /// <summary>
    /// An adaptation of wpf Tookits' CalendarBlackoutDatesCollection class
    /// </summary>
    public sealed class BlackoutDatesCollection : ObservableCollection<DateRangeHelper>
    {
        #region Fields
        private Thread _dspThread;
        private Calendar _cldOwner;
        #endregion

        #region Constructor
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="owner">Calendar object</param>
        public BlackoutDatesCollection(Calendar owner)
        {
            _cldOwner = owner;
            this._dspThread = Thread.CurrentThread;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Search for a date in the collection
        /// </summary>
        /// <param name="date">date search object</param>
        /// <returns></returns>
        public bool Contains(DateTime date)
        {
            for (int i = 0; i < Count; i++)
            {
                if (DateInRange(date, this[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tests for a date within a specified range
        /// </summary>
        /// <param name="date">date search object</param>
        /// <param name="start">start of range</param>
        /// <param name="end">end of range</param>
        /// <returns>bool</returns>
        public bool DateInRange(DateTime date, DateTime start, DateTime end)
        {
            if (DateTime.Compare(date.Date, start.Date) > -1 && DateTime.Compare(date.Date, end.Date) < 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tests for a date within a specified range
        /// </summary>
        /// <param name="date">date search object</param>
        /// <param name="range">date range</param>
        /// <returns>bool</returns>
        public bool DateInRange(DateTime date, DateRangeHelper range)
        {
            return DateInRange(date, range.Start, range.End);
        }

        /// <summary>
        /// Tests for range of date objects
        /// </summary>
        /// <param name="start">start of range</param>
        /// <param name="end">end of range</param>
        /// <returns>bool</returns>
        public bool Contains(DateTime start, DateTime end)
        {
            DateTime rangeStart, rangeEnd;
            int n = Count;

            if (DateTime.Compare(end, start) > -1)
            {
                rangeStart = start.Date;
                rangeEnd = end.Date;
            }
            else
            {
                rangeStart = end.Date;
                rangeEnd = start.Date;
            }

            for (int i = 0; i < n; i++)
            {
                if (DateTime.Compare(this[i].Start, rangeStart) == 0 && DateTime.Compare(this[i].End, rangeEnd) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tests if collection contains any date within a range
        /// </summary>
        /// <param name="range">date range</param>
        /// <returns>bool</returns>
        public bool ContainsAny(DateRangeHelper range)
        {
            foreach (DateRangeHelper item in this)
            {
                if (item.ContainsAny(range))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Clear items from the collection
        /// </summary>
        protected override void ClearItems()
        {
            if (IsValidThread())
            {
                foreach (DateRangeHelper item in Items)
                {
                    UnRegisterItem(item);
                }

                base.ClearItems();
                this._cldOwner.UpdateCalendar();
            }
        }

        /// <summary>
        /// Insert an item into the collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void InsertItem(int index, DateRangeHelper item)
        {
            if (IsValidThread())
            {
                if (IsValid(item))
                {
                    RegisterItem(item);
                    base.InsertItem(index, item);
                    _cldOwner.UpdateCalendar();
                }
            }
        }

        /// <summary>
        /// Remove an item from the collection
        /// </summary>
        /// <param name="index"></param>
        protected override void RemoveItem(int index)
        {
            if (IsValidThread())
            {
                if (index >= 0 && index < this.Count)
                {
                    UnRegisterItem(Items[index]);
                }

                base.RemoveItem(index);
                _cldOwner.UpdateCalendar();
            }
        }

        /// <summary>
        /// Set item in collection to a value
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void SetItem(int index, DateRangeHelper item)
        {
            if (!IsValidThread())
            {
                if (IsValid(item))
                {
                    DateRangeHelper oldItem = null;
                    if (index >= 0 && index < this.Count)
                    {
                        oldItem = Items[index];
                    }

                    base.SetItem(index, item);

                    UnRegisterItem(oldItem);
                    RegisterItem(Items[index]);

                    _cldOwner.UpdateCalendar();
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Register the DateRangeChanging event handler
        /// </summary>
        /// <param name="item"></param>
        private void RegisterItem(DateRangeHelper item)
        {
            if (item != null)
            {
                item.Changing += new EventHandler<DateRangeChangingEventArgs>(Item_Changing);
                item.PropertyChanged += new PropertyChangedEventHandler(Item_PropertyChanged);
            }
        }

        /// <summary>
        /// UnRegister the DateRangeChanging event handler
        /// </summary>
        private void UnRegisterItem(DateRangeHelper item)
        {
            if (item != null)
            {
                item.Changing -= new EventHandler<DateRangeChangingEventArgs>(Item_Changing);
                item.PropertyChanged -= new PropertyChangedEventHandler(Item_PropertyChanged);
            }
        }

        /// <summary>
        /// DateRangeChanging event notification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Item_Changing(object sender, DateRangeChangingEventArgs e)
        {
            DateRangeHelper item = sender as DateRangeHelper;
            if (item != null)
            {
                if (!IsValid(e.Start, e.End))
                {
                    throw new ArgumentOutOfRangeException("DateTime Item out of range");
                }
            }
        }

        /// <summary>
        /// PropertyChanged event notification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is DateRangeHelper)
            {
                _cldOwner.UpdateCalendar();
            }
        }

        /// <summary>
        /// Test if a DateRange contains valid members
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool IsValid(DateRangeHelper item)
        {
            return IsValid(item.Start, item.End);
        }

        /// <summary>
        /// Test if a DateTime range is valid
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private bool IsValid(DateTime start, DateTime end)
        {
            foreach (object child in _cldOwner.SelectedDates)
            {
                DateTime? day = child as DateTime?;
                Debug.Assert(day != null);
                if (DateInRange(day.Value, start, end))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests for a local thread, multithreaded collections are not supported
        /// </summary>
        /// <returns>bool</returns>
        private bool IsValidThread()
        {
            return Thread.CurrentThread == this._dspThread;
        }

        #endregion
    }
}
