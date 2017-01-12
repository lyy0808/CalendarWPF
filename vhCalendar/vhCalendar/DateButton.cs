#region Directives
using System.Windows;
using System.Windows.Controls;
#endregion

namespace vhCalendar
{
    public sealed class DateButton : Button
    {
        #region Constructor
        static DateButton()
        {
            PropertyMetadata isSelectedMetaData = new PropertyMetadata
            {
                DefaultValue = false,
            };
            IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(DateButton), isSelectedMetaData);

            PropertyMetadata isTodaysDateMetaData = new PropertyMetadata
            {
                DefaultValue = false,
            };
            IsTodaysDateProperty = DependencyProperty.Register("IsTodaysDate", typeof(bool), typeof(DateButton), isTodaysDateMetaData);

            PropertyMetadata isBlackOutMetadata = new PropertyMetadata
            {
                DefaultValue = false,
            };
            IsBlackOutProperty = DependencyProperty.Register("IsBlackOut", typeof(bool), typeof(DateButton), isBlackOutMetadata);

            PropertyMetadata isCurrentMonthMetaData = new PropertyMetadata
            {
                DefaultValue = true,
            };
            IsCurrentMonthProperty = DependencyProperty.Register("IsCurrentMonth", typeof(bool), typeof(DateButton), isCurrentMonthMetaData);
        }
        #endregion

        #region DisplayDateStart
        /// <summary>
        /// Gets/Sets the minimum date that is displayed and selected
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty;

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        #endregion

        #region IsBlackOut
        /// <summary>
        /// Gets/Sets button is blacked out
        /// </summary>
        public static readonly DependencyProperty IsBlackOutProperty;

        public bool IsBlackOut
        {
            get { return (bool)GetValue(IsBlackOutProperty); }
            set { SetValue(IsBlackOutProperty, value); }
        }
        #endregion

        #region IsCurrentMonth
        /// <summary>
        /// Gets/Sets button is a member of current month
        /// </summary>
        public static readonly DependencyProperty IsCurrentMonthProperty;

        public bool IsCurrentMonth
        {
            get { return (bool)GetValue(IsCurrentMonthProperty); }
            set { SetValue(IsCurrentMonthProperty, value); }
        }
        #endregion

        #region IsTodaysDate
        /// <summary>
        /// Gets/Sets todays date selection
        /// </summary>
        public static readonly DependencyProperty IsTodaysDateProperty;

        public bool IsTodaysDate
        {
            get { return (bool)GetValue(IsTodaysDateProperty); }
            set { SetValue(IsTodaysDateProperty, value); }
        }
        #endregion
    }
}
