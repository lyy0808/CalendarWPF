

#region Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Documents;
#endregion

namespace vhCalendar
{
    [DefaultEvent("SelectedDateChanged"),
    DefaultProperty("DisplayDate"),
    TemplatePart(Name = "Part_HeaderBorder", Type = typeof(Border)),
    TemplatePart(Name = "Part_TitleButton", Type = typeof(DateButton)),
    TemplatePart(Name = "Part_NextButton", Type = typeof(RepeatButton)),
    TemplatePart(Name = "Part_PreviousButton", Type = typeof(RepeatButton)),
    TemplatePart(Name = "Part_MonthContainer", Type = typeof(Grid)),
    TemplatePart(Name = "Part_DayGrid", Type = typeof(UniformGrid)),
    TemplatePart(Name = "Part_WeekGrid", Type = typeof(UniformGrid)),
    TemplatePart(Name = "Part_MonthGrid", Type = typeof(UniformGrid)),
    TemplatePart(Name = "Part_DecadeGrid", Type = typeof(UniformGrid)),
    TemplatePart(Name = "Part_YearGrid", Type = typeof(UniformGrid)),
    TemplatePart(Name = "Part_CurrentDatePanel", Type = typeof(StackPanel)),
    TemplatePart(Name = "Part_CurrentDateText", Type = typeof(TextBlock)),
    TemplatePart(Name = "Part_AnimationContainer", Type = typeof(Grid)),
    TemplatePart(Name = "Part_FooterContainer", Type = typeof(Grid))]
    public class Calendar : Control, INotifyPropertyChanged
    {
        #region Property Name Constants
        /// <summary>
        /// Property names as string constants
        /// </summary>
        private const string CurrentlySelectedDatePropName = "CurrentlySelectedDateString";
        private const string DisplayModePropName = "DisplayMode";
        private const string DisplayDateStartPropName = "DisplayDateStart";
        private const string FooterVisibilityPropName = "FooterVisibility";
        private const string TodayHighlightedPropName = "IsTodayHighlighted";
        private const string SelectionModePropName = "SelectionMode";
        private const string ShowDaysOfAllMonthsPropName = "ShowDaysOfAllMonths";
        private const string ThemePropName = "Theme";
        private const string WeekColumnVisibilityPropName = "WeekColumnVisibility";
        private const string HeaderHeightPropName = "HeaderHeight";
        private const string HeaderFontSizePropName = "HeaderFontSize";
        private const string HeaderFontWeightPropName = "HeaderFontWeight";
        private const string AllowDragPropName = "AllowDrag";
        private const string AdornDragPropName = "AdornDrag";
        private const string IsAnimatedPropName = "IsAnimated";
        #endregion

        #region Theme Declarations
        /// <summary>
        /// Operating System Themes
        /// </summary>
        public static ThemePath AeroNormalColorTheme = new ThemePath(Themes.AeroNormal.ToString(), "/vhCalendar;component/Themes/Aero.NormalColor.xaml");
        public static ThemePath ClassicTheme = new ThemePath(Themes.Classic.ToString(), "/vhCalendar;component/Themes/Classic.xaml");
        public static ThemePath LunaHomesteadTheme = new ThemePath(Themes.LunaHomestead.ToString(), "/vhCalendar;component/Themes/Luna.Homestead.xaml");
        public static ThemePath LunaNormalTheme = new ThemePath(Themes.LunaNormal.ToString(), "/vhCalendar;component/Themes/Luna.NormalColor.xaml");
        public static ThemePath LunaMetallicTheme = new ThemePath(Themes.LunaMetallic.ToString(), "/vhCalendar;component/Themes/Luna.Metallic.xaml");
        public static ThemePath RoyaleTheme = new ThemePath(Themes.Royale.ToString(), "/vhCalendar;component/Themes/Royale.xaml");
        public static ThemePath ZuneTheme = new ThemePath(Themes.Zune.ToString(), "/vhCalendar;component/Themes/Zune.xaml");
        /// <summary>
        /// Custom Themes
        /// </summary>
        public static ThemePath OfficeBlack = new ThemePath(Themes.OfficeBlack.ToString(), "/vhCalendar;component/Themes/Office.Black.xaml");
        public static ThemePath OfficeBlue = new ThemePath(Themes.OfficeBlue.ToString(), "/vhCalendar;component/Themes/Office.Blue.xaml");
        public static ThemePath OfficeSilver = new ThemePath(Themes.OfficeSilver.ToString(), "/vhCalendar;component/Themes/Office.Silver.xaml");
        #endregion

        #region MonthList Enum
        /// <summary>
        /// Day of week enumeration
        /// </summary>
        private enum DayOfWeek
        {
            周末 = 1,
            周一,
            周二,
            周三,
            周四,
            周五,
            周六
        }

        /// <summary>
        /// Month enumeration
        /// </summary>
        private enum MonthList
        {
            一月 = 1,
            二月,
            三月,
            四月,
            五月,
            六月,
            七月,
            八月,
            九月,
            十月,
            十一月,
            十二月
        }
        #endregion

        #region Fields
        Point _dragStart = new Point();
        Point _currentPos = new Point();
        DragData _dragData;
        DispatcherTimer _dispatcherTimer;
        private DateButton[,] _btnMonthMode = new DateButton[6, 7];
        private DateButton[,] _btnYearMode = new DateButton[4, 3];
        private DateButton[] _btnDecadeMode = new DateButton[10];
        private BlackoutDatesCollection _blackoutDates;
        private Dictionary<string, ResourceDictionary> _rdThemeDictionaries;
        #endregion

        #region Private Properties
        private bool HasInitialized { get; set; }
        private bool IsDragging { get; set; }
        private bool IsDragImage { get; set; }
        private bool IsAnimating { get; set; }
        private bool IsMoveForward { get; set; }
        private Window ParentWindow { get; set; }
        #endregion

        #region Internal Event Handlers
        /// <summary>
        /// Drag Timer callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                StopDragTimer();
            }
        }

        /// <summary>
        /// Listen for escape key to cancel drag op
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParentWindow_PreviewQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (e.EscapePressed)
            {
                StopDragTimer();
                e.Action = DragAction.Cancel;
            }
        }

        /// <summary>
        /// Get drag feedback from parent window for adorn window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParentWindow_PreviewGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (_dragData.Adorner != null)
            {

                Point pt = new Point(_currentPos.X + 16, _currentPos.Y + 16);
                _dragData.Adorner.Offset = pt;
            }

            Mouse.SetCursor(Cursors.Arrow);
            e.UseDefaultCursors = false;
            e.Handled = true;
        }

        /// <summary>
        /// Used to get current mouse coords in adorned drag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParentWindow_PreviewDragOver(object sender, DragEventArgs e)
        {
            _currentPos = e.GetPosition(this);
        }

        /// <summary>
        /// Store initial cursor position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void element_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ResetDragData();
            _dragStart = e.GetPosition(null);
        }

        /// <summary>
        /// Test for drag op and start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void element_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (AllowDrag)
            {
                // Get the current mouse position
                _currentPos = e.GetPosition(null);
                Vector diff = _dragStart - _currentPos;

                if (e.LeftButton == MouseButtonState.Pressed &&
                    Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                        Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (!IsDragging)
                    {
                        IsDragging = true;
                        DateButton button = sender as DateButton;
                        DateTime data = (DateTime)button.Tag;

                        if (AdornDrag)
                        {
                            _dragData = new DragData(button, data, CreateDragWindow(button));
                            if (_dragData.Parent != null && _dragData.Data != null)
                            {
                                this.AllowDrop = true;
                                AddDragEventHandlers();
                                DataObject dragData = new DataObject("DateTime", _dragData.Data);
                                DragDrop.DoDragDrop(_dragData.Parent, dragData, DragDropEffects.Copy);
                                StartDragTimer();
                            }
                        }
                        else
                        {
                            _dragData = new DragData(button, data, null);
                            if (_dragData.Parent != null && _dragData.Data != null)
                            {
                                DataObject dragData = new DataObject("DateTime", _dragData.Data);
                                DragDrop.DoDragDrop(_dragData.Parent, dragData, DragDropEffects.Copy);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fires when a DateButton is clicked in Decade view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void decadeModeButton_Click(object sender, RoutedEventArgs e)
        {
            DateButton button = (DateButton)sender;

            if ((int)button.Tag > 0)
            {
                this.DisplayDate = new DateTime((int)button.Tag, 1, 1);
                this.DisplayMode = DisplayType.Year;
            }
        }

        /// <summary>
        /// Fires when when the margin animation has completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void marginAnimation_Completed(object sender, EventArgs e)
        {
            Grid grdScroll = (Grid)FindElement("Part_ScrollGrid");

            if (grdScroll != null)
            {
                grdScroll.Visibility = Visibility.Collapsed;
            }

            IsAnimating = false;
        }

        /// <summary>
        /// Fires when a DateButton is clicked in the Month view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void monthModeButton_Click(object sender, RoutedEventArgs e)
        {
            DateButton button = (DateButton)sender;
            DateTime date = (DateTime)button.Tag;
            int row, column;

            if (SelectionMode == SelectionType.Single)
            {
                ClearSelectedDates(false);
                SelectedDate = date;
                button.IsSelected = true;
            }
            else if (SelectionMode == SelectionType.Multiple)
            {
                ObservableCollection<DateTime> oldDates = SelectedDates;
                if (SelectedDates.Contains(date))
                {
                    SelectedDates.Remove(date);
                    button.IsSelected = false;
                }
                else
                {
                    SelectedDates.Add(date);
                    button.IsSelected = true;
                }
                OnSelectedDatesChanged(this, new DependencyPropertyChangedEventArgs(SelectedDatesProperty, oldDates, SelectedDates));
            }
            else if (SelectionMode == SelectionType.Week)
            {
                MonthModeDateToRowColumn(date, out row, out column);
                // probably not the right way to do this but..
                ObservableCollection<DateTime> oldDates = new ObservableCollection<DateTime>();
                foreach (DateTime dt in SelectedDates)
                {
                    oldDates.Add(dt);
                }

                for (int i = 0; i < 7; i++)
                {
                    date = (DateTime)_btnMonthMode[row, i].Tag;
                    if (SelectedDates.Contains(date))
                    {
                        SelectedDates.Remove(date);
                        _btnMonthMode[row, i].IsSelected = false;
                    }
                    else
                    {
                        SelectedDates.Add(date);
                        _btnMonthMode[row, i].IsSelected = true;
                    }
                }
                OnSelectedDatesChanged(this, new DependencyPropertyChangedEventArgs(SelectedDatesProperty, oldDates, SelectedDates));
            }
        }

        /// <summary>
        /// Fires when when the next button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAnimating)
            {
                return;
            }

            int month = this.DisplayDate.Month;
            int year = this.DisplayDate.Year;

            try
            {
                DateTime newDisplayDate = DisplayDate;
                if (this.DisplayMode == DisplayType.Month)
                {
                    if (month == 12)
                    {
                        newDisplayDate = new DateTime(year + 1, 1, 1);
                    }
                    else
                    {
                        newDisplayDate = new DateTime(year, month + 1, 1);
                    }
                }
                else if (this.DisplayMode == DisplayType.Year)
                {
                    newDisplayDate = new DateTime(DisplayDate.Year + 1, 1, 1);
                }
                else if (this.DisplayMode == DisplayType.Decade)
                {
                    newDisplayDate = new DateTime(year - year % 10 + 10, 1, 1);
                }

                IsMoveForward = true;

                if (newDisplayDate >= DisplayDateStart && newDisplayDate <= DisplayDateEnd)
                {
                    this.DisplayDate = newDisplayDate;
                }
            }
            catch (ArgumentOutOfRangeException) { }
        }
        
        /// <summary>
        /// Fires when when the previous button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void previousButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAnimating)
            {
                return;
            }

            int month = this.DisplayDate.Month;
            int year = this.DisplayDate.Year;

            try
            {
                DateTime newDisplayDate = DisplayDate;

                if (this.DisplayMode == DisplayType.Month)
                {
                    if (month == 1)
                    {
                        newDisplayDate = new DateTime(year - 1, 12, DateTime.DaysInMonth(year - 1, 12));
                    }
                    else
                    {
                        newDisplayDate = new DateTime(year, month - 1, DateTime.DaysInMonth(year, month - 1));
                    }
                }
                else if (this.DisplayMode == DisplayType.Year)
                {
                    newDisplayDate = new DateTime(year - 1, 12, DateTime.DaysInMonth(year - 1, 12));
                }
                else if (this.DisplayMode == DisplayType.Decade)
                {
                    newDisplayDate = new DateTime(year - year % 10 - 1, 12, DateTime.DaysInMonth(year - year % 10 - 1, 12));
                }

                IsMoveForward = false;

                if (newDisplayDate >= DisplayDateStart && newDisplayDate <= DisplayDateEnd)
                {
                    DisplayDate = newDisplayDate;
                }
            }
            catch (ArgumentOutOfRangeException) { }
        }

        /// <summary>
        /// Fires when when the display transition animation has completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stbCalenderTransition_Completed(object sender, EventArgs e)
        {
            IsAnimating = false;
        }

        /// <summary>
        /// Fires when when the title button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void titleButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DisplayMode == DisplayType.Month)
            {
                this.DisplayMode = DisplayType.Year;
            }
            else if (this.DisplayMode == DisplayType.Year)
            {
                this.DisplayMode = DisplayType.Decade;
            }
        }

        /// <summary>
        /// Fires when a DateButton is clicked in Year view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void yearModeButton_Click(object sender, RoutedEventArgs e)
        {
            DateButton button = (DateButton)sender;

            if ((int)button.Tag > 0)
            {
                int month = (int)button.Tag;
                this.DisplayDate = new DateTime(this.DisplayDate.Year, month, 1);
                this.DisplayMode = DisplayType.Month;
            }
        }
        #endregion

        #region Constructors
        public Calendar()
        {
            // register inbuilt themes
            RegisterAttachedThemes();
            // load aero default
            //LoadDefaultTheme();
            SelectedDates = new ObservableCollection<DateTime>();
            this._blackoutDates = new BlackoutDatesCollection(this);
        }

        static Calendar()
        {
            // override style
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Calendar), new FrameworkPropertyMetadata(typeof(Calendar)));


            /*** Property Declarations ***/
            // AllowDrag
            UIPropertyMetadata allowDragMetaData = new UIPropertyMetadata
            {
                DefaultValue = false,
                PropertyChangedCallback = new PropertyChangedCallback(OnAllowDragChanged),
            };
            AllowDragProperty = DependencyProperty.Register("AllowDrag", typeof(bool), typeof(Calendar), allowDragMetaData);

            // AdornDrag
            UIPropertyMetadata adornDragMetaData = new UIPropertyMetadata
            {
                DefaultValue = false,
                CoerceValueCallback = new CoerceValueCallback(CoerceAdornDrag),
                PropertyChangedCallback = new PropertyChangedCallback(OnAdornChanged),
            };
            AdornDragProperty = DependencyProperty.Register("AdornDrag", typeof(bool), typeof(Calendar), adornDragMetaData);

            // HeaderFontSize
            FrameworkPropertyMetadata headerFontSizeMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = (double)12,
                CoerceValueCallback = new CoerceValueCallback(CoerceHeaderFontSize),
                PropertyChangedCallback = new PropertyChangedCallback(OnHeaderFontSizeChanged),
                AffectsRender = true, 
                AffectsMeasure = true
            };
            HeaderFontSizeProperty = DependencyProperty.Register("HeaderFontSize", typeof(double), typeof(Calendar), headerFontSizeMetaData);

            // HeaderFontWeight
            FrameworkPropertyMetadata headerFontWeightMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = FontWeights.Bold,
                PropertyChangedCallback = new PropertyChangedCallback(OnHeaderFontWeightChanged),
                AffectsRender = true
            };
            HeaderFontWeightProperty = DependencyProperty.Register("HeaderFontWeight", typeof(FontWeight), typeof(Calendar), headerFontWeightMetaData);

            // IsAnimated 
            FrameworkPropertyMetadata isAnimatedMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = true,
                PropertyChangedCallback = new PropertyChangedCallback(OnIsAnimatedChanged),
            };
            IsAnimatedProperty = DependencyProperty.Register("IsAnimated", typeof(bool), typeof(Calendar), isAnimatedMetaData);

            // IsTodayHighlighted
            FrameworkPropertyMetadata isTodayHighlightedMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = true,
                PropertyChangedCallback = new PropertyChangedCallback(OnTodayHighlightedChanged),
                AffectsRender = true
            };
            IsTodayHighlightedProperty = DependencyProperty.Register("IsTodayHighlighted", typeof(bool), typeof(Calendar), isTodayHighlightedMetaData);

            // HeaderHeight
            FrameworkPropertyMetadata headerHeightMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = (double)20,
                PropertyChangedCallback = new PropertyChangedCallback(OnHeaderHeightChanged),
                CoerceValueCallback = new CoerceValueCallback(CoerceHeaderHeight),
                AffectsRender = true,
                AffectsMeasure = true
            };
            HeaderHeightProperty = DependencyProperty.Register("HeaderHeight", typeof(double), typeof(Calendar), headerHeightMetaData);

            // ShowDaysOfAllMonths
            FrameworkPropertyMetadata showDaysOfAllMonthsMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = true,
                PropertyChangedCallback = new PropertyChangedCallback(OnShowDaysOfAllMonthsChanged),
                AffectsRender = true
            };
            ShowDaysOfAllMonthsProperty = DependencyProperty.Register("ShowDaysOfAllMonths", typeof(bool), typeof(Calendar), showDaysOfAllMonthsMetaData);

            // FooterVisibility
            FrameworkPropertyMetadata footerVisibilityMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = Visibility.Visible,
                PropertyChangedCallback = new PropertyChangedCallback(OnFooterVisibilityChanged),
                AffectsRender = true,
                AffectsMeasure = true
            };
            FooterVisibilityProperty = DependencyProperty.Register("FooterVisibility", typeof(Visibility), typeof(Calendar), footerVisibilityMetaData);

            // WeekColumnVisibility
            FrameworkPropertyMetadata weekColumnVisibilityMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = Visibility.Visible,
                PropertyChangedCallback = new PropertyChangedCallback(OnWeekColumnVisibilityChanged),
                AffectsRender = true,
                AffectsMeasure = true
            };
            WeekColumnVisibilityProperty = DependencyProperty.Register("WeekColumnVisibility", typeof(Visibility), typeof(Calendar), weekColumnVisibilityMetaData);

            // DisplayMode
            FrameworkPropertyMetadata displayModeMetaData = new FrameworkPropertyMetadata
            {
                PropertyChangedCallback = new PropertyChangedCallback(OnDisplayModeChanged)
            };
            DisplayModeProperty = DependencyProperty.Register("DisplayMode", typeof(DisplayType), typeof(Calendar), displayModeMetaData);
            
            // DisplayDate
            UIPropertyMetadata displayDateMetaData = new UIPropertyMetadata
            {
                DefaultValue = DateTime.Today,
                PropertyChangedCallback = new PropertyChangedCallback(OnModeChanged),
                CoerceValueCallback = new CoerceValueCallback(CoerceDateToBeInRange)
            };
            DisplayDateProperty = DependencyProperty.Register("DisplayDate", typeof(DateTime), typeof(Calendar), displayDateMetaData);

            // DisplayDateStart
            UIPropertyMetadata displayDateStartMetaData = new UIPropertyMetadata
            {
                DefaultValue = new DateTime(1, 1, 1),
                PropertyChangedCallback = new PropertyChangedCallback(OnDisplayDateStartChanged),
                CoerceValueCallback = new CoerceValueCallback(CoerceDisplayDateStart),
            };
            DisplayDateStartProperty = DependencyProperty.Register("DisplayDateStart", typeof(DateTime), typeof(Calendar), displayDateStartMetaData);

            // DisplayDateEnd
            UIPropertyMetadata displayDateEndMetaData = new UIPropertyMetadata
            {
                DefaultValue = new DateTime(2199, 1, 1),
                CoerceValueCallback = new CoerceValueCallback(CoerceDisplayDateEnd),
                PropertyChangedCallback = new PropertyChangedCallback(OnDisplayDateEndChanged)
            };
            DisplayDateEndProperty = DependencyProperty.Register("DisplayDateEnd", typeof(DateTime), typeof(Calendar), displayDateEndMetaData);

            // SelectedDate
            UIPropertyMetadata selectedDateMetaData = new UIPropertyMetadata
            {
                DefaultValue = DateTime.Now,
                PropertyChangedCallback = new PropertyChangedCallback(OnSelectedDateChanged)
            };
            SelectedDateProperty = DependencyProperty.Register("SelectedDate", typeof(DateTime), typeof(Calendar), selectedDateMetaData);

            // SelectedDates
            UIPropertyMetadata selectedDatesMetaData = new UIPropertyMetadata
            {
                DefaultValue = null,
                PropertyChangedCallback = new PropertyChangedCallback(OnSelectedDatesChanged),
                CoerceValueCallback = new CoerceValueCallback(CoerceDatesChanged),
            };

            // SelectedDates
            SelectedDatesProperty = DependencyProperty.Register("CurrentlySelectedDates", typeof(ObservableCollection<DateTime>),
                typeof(Calendar), new UIPropertyMetadata(null, 
                    delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e)
                    { 
                        Calendar cld = (Calendar)sender;
                        INotifyCollectionChanged collection = e.NewValue as INotifyCollectionChanged;
                        if (collection != null)
                        {
                            collection.CollectionChanged +=
                                delegate { cld.OnPropertyChanged(new PropertyChangedEventArgs(CurrentlySelectedDatePropName)); };
                        }
                        cld.OnPropertyChanged(new PropertyChangedEventArgs(CurrentlySelectedDatePropName));
                    }
            ));

            // SetTheme
            FrameworkPropertyMetadata themeMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = Themes.AeroNormal.ToString(),
                CoerceValueCallback = new CoerceValueCallback(CoerceThemeChange),
                PropertyChangedCallback = new PropertyChangedCallback(OnThemeChanged),
                AffectsRender = true,
                AffectsMeasure = true
            };
            ThemeProperty = DependencyProperty.Register("Theme", typeof(string), typeof(Calendar), themeMetaData);

            // SelectionMode
            UIPropertyMetadata selectionModeMetaData = new UIPropertyMetadata
                (SelectionType.Single, 
                    delegate(DependencyObject sender, DependencyPropertyChangedEventArgs e) 
                    { 
                    }, 
                CoerceSelectionMode);
            SelectionModeProperty = DependencyProperty.Register("SelectionMode", typeof(SelectionType), typeof(Calendar), selectionModeMetaData);
        }
        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Event raised when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed event
        /// </summary>
        /// <param name="e">The arguments to pass</param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Applies template and initial bindings
        /// </summary>
        public override void OnApplyTemplate()
        {
            // apply the template
            base.OnApplyTemplate();
            // template has been loaded
            HasInitialized = true;
            // set minimum size constraints
            this.MinWidth = 150;
            this.MinHeight = 150;

            // adds events and bindings
            SetBindings();

            // initialize buttons for display views
            InitializeMonth();
            InitializeYear();
            InitializeDecade();

            // set the default display view
            this.UpdateCalendar();

            // add bindings
            Grid grdFooterContainer = (Grid)FindElement("Part_FooterContainer");
            // property to element bindings
            if (grdFooterContainer != null)
            {
                // bind the footer grid visibility to the property
                Binding bindIsFooterVisible = new Binding();
                bindIsFooterVisible.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
                bindIsFooterVisible.Path = new PropertyPath("FooterVisibility");
                bindIsFooterVisible.Mode = BindingMode.TwoWay;
                grdFooterContainer.SetBinding(Grid.VisibilityProperty, bindIsFooterVisible);
            }
            UniformGrid grdWeek = (UniformGrid)FindElement("Part_WeekGrid");
            if (grdWeek != null)
            {
                // bind the week column grid visibility to the property
                Binding bindIsWeekVisible = new Binding();
                bindIsWeekVisible.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
                bindIsWeekVisible.Path = new PropertyPath("WeekColumnVisibility");
                bindIsWeekVisible.Mode = BindingMode.TwoWay;
                grdWeek.SetBinding(Grid.VisibilityProperty, bindIsWeekVisible);
            }

            Border brdHeaderBorder = (Border)FindElement("Part_HeaderBorder");
            if (brdHeaderBorder != null)
            {
                // bind the footer grid visibility to the property
                Binding bindHeaderHeight = new Binding();
                bindHeaderHeight.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
                bindHeaderHeight.Path = new PropertyPath("HeaderHeight");
                bindHeaderHeight.Mode = BindingMode.TwoWay;
                brdHeaderBorder.SetBinding(Border.HeightProperty, bindHeaderHeight);
            }
            // store parent window
            if (!IsDesignTime)
            {
                FindParentWindow();
            }
        }

        /// <summary>
        /// Sets the Day Name and Display name strings based on overall size of control
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            UniformGrid grdDay = (UniformGrid)FindElement("Part_DayGrid");

            if (grdDay != null)
            {
                for (int i = 0; i < 7; i++)
                {
                    Label lbl = (Label)grdDay.Children[i];

                    if (availableSize.Width < 180)
                    {
                        lbl.Content = lbl.Tag.ToString().Substring(0, 2);
                    }
                    else if (availableSize.Width < 430)
                    {
                        lbl.Content = lbl.Tag.ToString().Substring(0, 3);
                    }
                    else
                    {
                        lbl.Content = lbl.Tag.ToString();
                    }
                }
            }
            return availableSize;
        }

        /// <summary>
        /// Not used
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        private double GetDayStringLength(Size availableSize)
        {
            UniformGrid grdDay = (UniformGrid)FindElement("Part_DayGrid");

            if (grdDay != null)
            {
                Label lblMeasure = (Label)grdDay.Children[3];
                Size szCompare = new Size(availableSize.Width / 7, availableSize.Height);

                lblMeasure.Content = lblMeasure.Tag.ToString();
                lblMeasure.Measure(availableSize);
                double width = lblMeasure.DesiredSize.Width;

                if (lblMeasure.DesiredSize.Width < szCompare.Width)
                {
                    return -1;
                }

                lblMeasure.Content = lblMeasure.Tag.ToString().Substring(0, 3);
                lblMeasure.Measure(availableSize);
                width = lblMeasure.DesiredSize.Width;
                if (width < szCompare.Width)
                {
                    return 3;
                }
                else
                {
                    return 2;
                }
            }
            return 0;
        }
        #endregion

        #region Properties
        #region AllowDrag
        /// <summary>
        /// Gets/Sets dragging capability
        /// </summary>
        public static readonly DependencyProperty AllowDragProperty;

        public bool AllowDrag
        {
            get { return (bool)this.GetValue(AllowDragProperty); }
            set { this.SetValue(AllowDragProperty, value); }
        }

        private static void OnAllowDragChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = d as Calendar;
            vc.OnPropertyChanged(new PropertyChangedEventArgs(AllowDragPropName));
        }
        #endregion

        #region AdornDrag
        /// <summary>
        /// Gets/Sets drag usese adorned window
        /// </summary>
        public static readonly DependencyProperty AdornDragProperty;

        public bool AdornDrag
        {
            get { return (bool)this.GetValue(AdornDragProperty); }
            set { this.SetValue(AdornDragProperty, value); }
        }

        private static object CoerceAdornDrag(DependencyObject d, object o)
        {
            Calendar vc = d as Calendar;

            if (vc.ParentWindow == null && !vc.IsDesignTime && vc.HasInitialized)
            {
                bool value = false;
                return value;
            }
            return o;
        }

        private static void OnAdornChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = d as Calendar;
            vc.OnPropertyChanged(new PropertyChangedEventArgs(AdornDragPropName));
        }
        #endregion

        #region BlackoutDates
        /// <summary>
        /// 
        /// </summary>
        public BlackoutDatesCollection BlackoutDates
        {
            get { return _blackoutDates; }
        }
        #endregion BlackoutDates

        #region DisplayDate
        /// <summary>
        /// Gets/Sets the date that is being displayed in the calendar
        /// </summary>
        public static readonly DependencyProperty DisplayDateProperty;

        public DateTime DisplayDate
        {
            get { return (DateTime)this.GetValue(DisplayDateProperty); }
            set { this.SetValue(DisplayDateProperty, value); }
        }

        private static object CoerceDateToBeInRange(DependencyObject d, object o)
        {
            Calendar vc = d as Calendar;
            DateTime value = (DateTime)o;

            if (value < vc.DisplayDateStart)
            {
                return vc.DisplayDateStart;
            }
            if (value > vc.DisplayDateEnd)
            {
                return vc.DisplayDateEnd;
            }
            return o;
        }
        #endregion

        #region DisplayMode
        /// <summary>
        /// Gets/Sets the calender display mode
        /// </summary>
        public static readonly DependencyProperty DisplayModeProperty;

        public DisplayType DisplayMode
        {
            get { return (DisplayType)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }

        private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = d as Calendar;
            OnModeChanged(d, new DependencyPropertyChangedEventArgs());

            //raise the DisplayModeChanged event
            DisplayModeChangedEventArgs args =
                new DisplayModeChangedEventArgs(DisplayModeChangedEvent);

            args.NewMode = (DisplayType)e.NewValue;
            args.OldMode = (DisplayType)e.OldValue;
            vc.RaiseEvent(args);

            //raise the PropertyChanged event
            vc.OnPropertyChanged(new PropertyChangedEventArgs(DisplayModePropName));
        }

        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = d as Calendar;
            vc.SetCalendar();
        }

        /// <summary>
        /// Event for the DateSelectionChanged raised when the date changes
        /// </summary>
        public static readonly RoutedEvent DisplayModeChangedEvent = EventManager.RegisterRoutedEvent("DisplayModeChanged",
            RoutingStrategy.Bubble, typeof(DisplayModeChangedEventHandler), typeof(Calendar));

        /// <summary>
        /// Event for the DateSelectionChanged raised when the date changes
        /// </summary>
        public event DisplayModeChangedEventHandler DisplayModeChanged
        {
            add { AddHandler(DisplayModeChangedEvent, value); }
            remove { RemoveHandler(DisplayModeChangedEvent, value); }
        }
        #endregion

        #region DisplayDateStart
        /// <summary>
        /// Gets/Sets the minimum date that is displayed and selected
        /// </summary>
        public static readonly DependencyProperty DisplayDateStartProperty;

        public DateTime DisplayDateStart
        {
            get { return (DateTime)GetValue(DisplayDateStartProperty); }
            set { SetValue(DisplayDateStartProperty, value); }
        }

        private static void OnDisplayDateStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = d as Calendar;

            vc.CoerceValue(DisplayDateEndProperty);
            vc.CoerceValue(SelectedDateProperty);
            vc.CoerceValue(DisplayDateProperty);

            vc.OnPropertyChanged(new PropertyChangedEventArgs(DisplayDateStartPropName));
            OnModeChanged(d, new DependencyPropertyChangedEventArgs());
        }

        private static object CoerceDisplayDateStart(DependencyObject d, object o)
        {
            Calendar vc = d as Calendar;
            DateTime value = (DateTime)o;
            return o;
        }
        #endregion

        #region DisplayDateEnd
        /// <summary>
        /// Gets/Sets the maximum date that is displayed and selected
        /// </summary>
        public static readonly DependencyProperty DisplayDateEndProperty;

        public DateTime DisplayDateEnd
        {
            get { return (DateTime)GetValue(DisplayDateEndProperty); }
            set { SetValue(DisplayDateEndProperty, value); }
        }

        private static void OnDisplayDateEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = d as Calendar;

            vc.CoerceValue(SelectedDateProperty);
            vc.CoerceValue(DisplayDateProperty);

            OnModeChanged(d, new DependencyPropertyChangedEventArgs());
        }

        private static object CoerceDisplayDateEnd(DependencyObject d, object o)
        {
            Calendar vc = d as Calendar;
            DateTime value = (DateTime)o;

            if (value < vc.DisplayDateStart)
            {
                return vc.DisplayDateStart;
            }

            return o;
        }
        #endregion

        #region FooterVisibility
        /// <summary>
        /// Gets/Sets the footer visibility
        /// </summary>
        public static readonly DependencyProperty FooterVisibilityProperty;

        public Visibility FooterVisibility
        {
            get { return (Visibility)GetValue(FooterVisibilityProperty); }
            set { SetValue(FooterVisibilityProperty, value); }
        }

        private static void OnFooterVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = d as Calendar;
            Grid grdFooterContainer = (Grid)vc.FindElement("Part_FooterContainer");

            if (grdFooterContainer != null)
            {
                if ((Visibility)e.NewValue == Visibility.Visible)
                {
                    grdFooterContainer.Visibility = Visibility.Visible;
                    vc.UpdateCalendar();
                }
                else
                {
                    grdFooterContainer.Visibility = Visibility.Collapsed;
                    vc.UpdateCalendar();
                }

                vc.OnPropertyChanged(new PropertyChangedEventArgs(FooterVisibilityPropName));
            }
        }
        #endregion

        #region HeaderFontSize
        /// <summary>
        /// Gets/Sets the title button font size
        /// </summary>
        public static readonly DependencyProperty HeaderFontSizeProperty;

        public double HeaderFontSize
        {
            get { return (double)this.GetValue(HeaderFontSizeProperty); }
            set { this.SetValue(HeaderFontSizeProperty, value); }
        }

        private static object CoerceHeaderFontSize(DependencyObject d, object o)
        {
            Calendar vc = d as Calendar;
            double value = (double)o;

            if (value > 16)
            {
                return 16;
            }
            if (value < 7)
            {
                return 7;
            }

            return o;
        }

        private static void OnHeaderFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = d as Calendar;
            DateButton btnTitle = (DateButton)vc.FindElement("Part_TitleButton");

            if (btnTitle != null)
            {
                btnTitle.FontSize = (double)e.NewValue;
                vc.OnPropertyChanged(new PropertyChangedEventArgs(HeaderFontSizePropName));
            }
        }
        #endregion

        #region HeaderFontWeight
        /// <summary>
        /// Gets/Sets the title button font weight
        /// </summary>
        public static readonly DependencyProperty HeaderFontWeightProperty;

        public FontWeight HeaderFontWeight
        {
            get { return (FontWeight)this.GetValue(HeaderFontWeightProperty); }
            set { this.SetValue(HeaderFontWeightProperty, value); }
        }

        private static void OnHeaderFontWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = d as Calendar;
            DateButton btnTitle = (DateButton)vc.FindElement("Part_TitleButton");

            if (btnTitle != null)
            {
                btnTitle.FontWeight = (FontWeight)e.NewValue;
                vc.OnPropertyChanged(new PropertyChangedEventArgs(HeaderFontWeightPropName));
            }
        }
        #endregion

        #region HeaderHeight
        /// <summary>
        /// Gets/Sets the header height
        /// </summary>
        public static readonly DependencyProperty HeaderHeightProperty;

        public double HeaderHeight
        {
            get { return (double)this.GetValue(HeaderHeightProperty); }
            set { this.SetValue(HeaderHeightProperty, value); }
        }

        private static object CoerceHeaderHeight(DependencyObject d, object o)
        {
            Calendar vc = d as Calendar;
            double value = (double)o;

            if (value > 30)
            {
                return 30;
            }
            if (value < 18)
            {
                return 18;
            }

            return o;
        }

        private static void OnHeaderHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = d as Calendar;
            Border brdHeaderBorder = (Border)vc.FindElement("Part_HeaderBorder");

            if (brdHeaderBorder != null)
            {
                brdHeaderBorder.Height = (double)e.NewValue;
                vc.OnPropertyChanged(new PropertyChangedEventArgs(HeaderHeightPropName));
            }
        }
        #endregion

        #region IsAnimated
        /// <summary>
        /// Gets/Sets animations are used
        /// </summary>
        public static readonly DependencyProperty IsAnimatedProperty;

        public bool IsAnimated
        {
            get { return (bool)this.GetValue(IsAnimatedProperty); }
            set { this.SetValue(IsAnimatedProperty, value); }
        }

        private static void OnIsAnimatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = d as Calendar;
            vc.OnPropertyChanged(new PropertyChangedEventArgs(IsAnimatedPropName));
        }
        #endregion

        #region IsDesignTime
        /// <summary>
        /// Tests if control is in design enviroment
        /// </summary>
        public bool IsDesignTime
        {
            get { return DesignerProperties.GetIsInDesignMode(this); }
        }
        #endregion

        #region IsTodayHighlighted
        /// <summary>
        /// Gets/Sets current day is highlighted
        /// </summary>
        public static readonly DependencyProperty IsTodayHighlightedProperty;

        public bool IsTodayHighlighted
        {
            get { return (bool)GetValue(IsTodayHighlightedProperty); }
            set { SetValue(IsTodayHighlightedProperty, value); }
        }

        private static void OnTodayHighlightedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = d as Calendar;

            if (vc.DisplayMode == DisplayType.Month)
            {
                vc.SetMonthMode();
                vc.OnPropertyChanged(new PropertyChangedEventArgs(TodayHighlightedPropName));
            }
        }
        #endregion

        #region SelectedDate
        /// <summary>
        /// Gets/Sets the currently viewed date
        /// </summary>
        public static readonly DependencyProperty SelectedDateProperty;

        public DateTime SelectedDate
        {
            get { return (DateTime)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }

        static void OnSelectedDateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = (Calendar)obj;
            vc.OnDateChanged(vc.SelectedDate, (DateTime)e.OldValue);
            vc.OnPropertyChanged(new PropertyChangedEventArgs(CurrentlySelectedDatePropName));
        }

        private void OnDateChanged(DateTime newDate, DateTime oldDate)
        {
            SelectedDateChangedEventArgs args =
                new SelectedDateChangedEventArgs(SelectedDateChangedEvent);

            args.NewDate = newDate;
            args.OldDate = oldDate;
            RaiseEvent(args);
        }

        /// <summary>
        /// Event for the DateSelectionChanged raised when the date changes
        /// </summary>
        public static readonly RoutedEvent SelectedDateChangedEvent = EventManager.RegisterRoutedEvent("SelectedDateChanged",
            RoutingStrategy.Bubble, typeof(SelectedDateChangedEventHandler), typeof(Calendar));

        /// <summary>
        /// Event for the DateSelectionChanged raised when the date changes
        /// </summary>
        public event SelectedDateChangedEventHandler SelectedDateChanged
        {
            add { AddHandler(SelectedDateChangedEvent, value); }
            remove { RemoveHandler(SelectedDateChangedEvent, value); }
        }
        #endregion

        #region SelectedDates
        /// <summary>
        /// Gets/Sets a collection of selected dates
        /// </summary>
        public static readonly DependencyProperty SelectedDatesProperty;

        public ObservableCollection<DateTime> SelectedDates
        {
            get { return (ObservableCollection<DateTime>)GetValue(SelectedDatesProperty); }
            set { SetValue(SelectedDatesProperty, value); }
        }

        static void OnSelectedDatesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = (Calendar)obj;
            vc.OnDatesChanged((ObservableCollection<DateTime>)e.NewValue, (ObservableCollection<DateTime>)e.OldValue);
            vc.OnPropertyChanged(new PropertyChangedEventArgs(CurrentlySelectedDatePropName));
        }

        private static object CoerceDatesChanged(DependencyObject d, object o)
        {
            Calendar vc = d as Calendar;
            return o;
        }

        private void OnDatesChanged(ObservableCollection<DateTime> newDates, ObservableCollection<DateTime> oldDates)
        {
            SelectedDatesChangedEventArgs args = new SelectedDatesChangedEventArgs(SelectedDatesChangedEvent);

            args.NewDates = newDates;
            args.OldDates = oldDates;
            RaiseEvent(args);
        }

        /// <summary>
        /// Event for the DateSelectionChanged raised when the date changes
        /// </summary>
        public static readonly RoutedEvent SelectedDatesChangedEvent = EventManager.RegisterRoutedEvent("SelectedDatesChanged",
            RoutingStrategy.Bubble, typeof(SelectedDatesChangedEventHandler), typeof(Calendar));

        /// <summary>
        /// Event for the DateSelectionChanged raised when the date changes
        /// </summary>
        public event SelectedDatesChangedEventHandler SelectedDatesChanged
        {
            add { AddHandler(SelectedDatesChangedEvent, value); }
            remove { RemoveHandler(SelectedDatesChangedEvent, value); }
        }
        /// <summary>
        /// Gets/Sets a string that represents the selected date
        /// </summary>
        public string CurrentlySelectedDateString
        {
            get
            {
                if (SelectionMode != SelectionType.Single)
                {
                    if (SelectedDates.Count > 1)
                    {
                        return String.Format("{0} - {1}", SelectedDates[0].ToShortDateString(),
                            SelectedDates[SelectedDates.Count - 1].ToShortDateString());
                    }
                    else if (SelectedDates.Count == 1)
                    {
                        return SelectedDates[0].ToLongDateString();
                    }
                }
                return SelectedDate.ToLongDateString();
            }
        }
        #endregion

        #region SelectionMode
        /// <summary>
        /// Gets/Sets the selection mode
        /// </summary>
        public static readonly DependencyProperty SelectionModeProperty;

        public SelectionType SelectionMode
        {
            get { return (SelectionType)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        private static object CoerceSelectionMode(DependencyObject d, object o)
        {
            Calendar vc = d as Calendar;
            vc.ClearSelectedDates(false);
            
            return o;
        }
        #endregion

        #region ShowDaysOfAllMonths
        /// <summary>
        /// Gets/Sets days of all months written to grid
        /// </summary>
        public static readonly DependencyProperty ShowDaysOfAllMonthsProperty;

        public bool ShowDaysOfAllMonths
        {
            get { return (bool)GetValue(ShowDaysOfAllMonthsProperty); }
            set { SetValue(ShowDaysOfAllMonthsProperty, value); }
        }

        private static void OnShowDaysOfAllMonthsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = d as Calendar;
            Grid grdMonthContainer = (Grid)vc.FindElement("Part_MonthContainer");

            if (grdMonthContainer != null)
            {
                if (grdMonthContainer.Visibility == Visibility.Visible)
                {
                    vc.SetMonthMode();
                }
                vc.OnPropertyChanged(new PropertyChangedEventArgs(ShowDaysOfAllMonthsPropName));
            }
        }
        #endregion

        #region Theme
        /// <summary>
        /// Get/Sets the Calender theme
        /// </summary>
        public static DependencyProperty ThemeProperty;

        public string Theme
        {
            get { return (string)GetValue(ThemeProperty); }
            set { SetValue(ThemeProperty, value); }
        }

        private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = d as Calendar;
            
            // test args
            if (vc == null || e == null)
            {
                throw new ArgumentNullException("Invalid Theme property");
            }

            // current theme
            string curThemeName = e.OldValue as string;
            string curRegisteredThemeName = vc.GetRegistrationName(curThemeName, vc.GetType());

            if (vc._rdThemeDictionaries.ContainsKey(curRegisteredThemeName))
            {
                // remove current theme
                ResourceDictionary curThemeDictionary = vc._rdThemeDictionaries[curRegisteredThemeName];
                vc.Resources.MergedDictionaries.Remove(curThemeDictionary);
            }

            // new theme name
            string newThemeName = e.NewValue as string;
            string newRegisteredThemeName = vc.GetRegistrationName(newThemeName, vc.GetType());

            // add the resource
            if (!vc._rdThemeDictionaries.ContainsKey(newRegisteredThemeName))
            {
                  throw new ArgumentNullException("Invalid Theme property");
            }
            else
            {
                // reset display
                vc.DisplayMode = DisplayType.Month;
                vc.SetMonthMode();
                // add the dictionary
                ResourceDictionary newThemeDictionary = vc._rdThemeDictionaries[newRegisteredThemeName];
                vc.Resources.MergedDictionaries.Add(newThemeDictionary);
            }
            
            vc.OnPropertyChanged(new PropertyChangedEventArgs(ThemePropName));
        }

        private static object CoerceThemeChange(DependencyObject d, object o)
        {
            return o;
        }
        #endregion

        #region WeekColumnVisibility
        /// <summary>
        /// Gets/Sets the week column visibility
        /// </summary>
        public static readonly DependencyProperty WeekColumnVisibilityProperty;

        public Visibility WeekColumnVisibility
        {
            get { return (Visibility)GetValue(WeekColumnVisibilityProperty); }
            set { SetValue(WeekColumnVisibilityProperty, value); }
        }

        private static void OnWeekColumnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Calendar vc = d as Calendar;
            UniformGrid grdWeek = (UniformGrid)vc.FindElement("Part_WeekGrid");

            if (grdWeek != null)
            {
                if ((Visibility)e.NewValue == Visibility.Visible)
                {
                    grdWeek.Visibility = Visibility.Visible;
                    vc.UpdateCalendar();
                }
                else
                {
                    grdWeek.Visibility = Visibility.Collapsed;
                    vc.UpdateCalendar();
                }
            }
            vc.OnPropertyChanged(new PropertyChangedEventArgs(WeekColumnVisibilityPropName));
        }
        #endregion
        #endregion

        #region Theming
        /// <summary>
        /// Load the default theme
        /// </summary>
        private void LoadDefaultTheme()
        {
            string registrationName = GetRegistrationName(Themes.OfficeBlack.ToString(), typeof(Calendar));
            this.Resources.MergedDictionaries.Add(_rdThemeDictionaries[registrationName]);
        }

        /// <summary>
        /// Instance theme dictionary and add themes
        /// </summary>
        private void RegisterAttachedThemes()
        {
            _rdThemeDictionaries = new Dictionary<string, ResourceDictionary>();
            RegisterTheme(AeroNormalColorTheme, typeof(Calendar));
            RegisterTheme(ClassicTheme, typeof(Calendar));
            RegisterTheme(LunaHomesteadTheme, typeof(Calendar));
            RegisterTheme(LunaMetallicTheme, typeof(Calendar));
            RegisterTheme(LunaNormalTheme, typeof(Calendar));
            RegisterTheme(RoyaleTheme, typeof(Calendar));
            RegisterTheme(ZuneTheme, typeof(Calendar));
            RegisterTheme(OfficeBlack, typeof(Calendar));
            RegisterTheme(OfficeBlue, typeof(Calendar));
            RegisterTheme(OfficeSilver, typeof(Calendar));
        }

        /// <summary>
        /// Register a theme with internal dictionary
        /// </summary>
        public void RegisterTheme(ThemePath theme, Type ownerType)
        {
            // test args
            if ((theme.Name == null) || (theme.DictionaryPath == null))
            {
                throw new ArgumentNullException("Theme name/path is null");
            }
            if (ownerType == null)
            {
                throw new ArgumentNullException("Invalid ownerType");
            }

            // get registration name vhCalendar.Calendar;CustomTheme
            string registrationName = GetRegistrationName(theme.Name, ownerType);

            try
            {
                if (!_rdThemeDictionaries.ContainsKey(registrationName))
                {
                    // create the Uri
                    Uri themeUri = new Uri(theme.DictionaryPath, UriKind.Relative);
                    // register the new theme
                    _rdThemeDictionaries[registrationName] = Application.LoadComponent(themeUri) as ResourceDictionary;
                }
            }
            catch { }
        }

        /// <summary>
        /// Get themes formal registration name
        /// </summary>
        private string GetRegistrationName(string themeName, Type ownerType)
        {
            return string.Format("{0};{1}", ownerType.ToString(), themeName);
        }
        #endregion

        #region Control Methods
        /// <summary>
        /// Add handlers for adorned drag
        /// </summary>
        private void AddDragEventHandlers()
        {
            ParentWindow.PreviewDragOver += new DragEventHandler(ParentWindow_PreviewDragOver);
            ParentWindow.PreviewGiveFeedback += new GiveFeedbackEventHandler(ParentWindow_PreviewGiveFeedback);
            ParentWindow.PreviewQueryContinueDrag += new QueryContinueDragEventHandler(ParentWindow_PreviewQueryContinueDrag);
        }

        /// <summary>
        /// Capture an elements bitmap
        /// </summary>
        /// <param name="target">element</param>
        /// <param name="dpiX">screen dpi X</param>
        /// <param name="dpiY">screen dpi Y</param>
        /// <returns>bitmap</returns>
        private static BitmapSource CaptureScreen(Visual target, double dpiX, double dpiY)
        {
            if (target == null)
            {
                return null;
            }
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)(bounds.Width * dpiX / 96.0),
                                                            (int)(bounds.Height * dpiY / 96.0),
                                                            dpiX,
                                                            dpiY,
                                                            PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(target);
                ctx.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }
            rtb.Render(dv);
            return rtb;
        }

        /// <summary>
        /// Clear selected dates
        /// </summary>
        public void ClearSelectedDates(bool persist)
        {
            if (!persist)
            {
                SelectedDates.Clear();
            } 
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (_btnMonthMode[i, j] != null)
                    {
                        _btnMonthMode[i, j].IsSelected = false;
                    }
                }
            }
        }

        /// <summary>
        /// Create the adorned drag window
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        private DragAdorner CreateDragWindow(DateButton button)
        {
            // get the screen image
            BitmapSource screen = CaptureScreen(button, 96, 96);
            // create an imagebrush
            ImageBrush img = new ImageBrush();
            img.ImageSource = screen;
            // create the dragadorner
            Size sz = new Size(button.ActualWidth - 4, button.ActualHeight - 4);
            DragAdorner dragWindow = new DragAdorner(this, sz, img);
            // set opacity.		
            dragWindow.Opacity = 0.8;
            dragWindow.Visibility = Visibility.Visible;
            // add the adorner
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
            layer.Add(dragWindow);

            return dragWindow;
        }

        /// <summary>
        /// Visual tree helper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="current"></param>
        /// <returns></returns>
        private static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        /// <summary>
        /// Find element in the template
        /// </summary>
        private object FindElement(string name)
        {
            try 
            {
                if (HasInitialized)
                {
                    return this.Template.FindName(name, this);
                }
                else
                {
                    return null;
                }
            }
            catch 
            {
                return null; 
            }
        }

        /// <summary>
        /// Store parent window for adorned drag operation
        /// </summary>
        private void FindParentWindow()
        {
            try
            {
                Window window = FindAnchestor<Window>((DependencyObject)this);
                if (window != null)
                {
                    ParentWindow = window;
                }
                else
                {
                    AdornDrag = false;
                }
            }
            catch
            {
                AdornDrag = false;
            }
        }

        /// <summary>
        /// Get the week number from DateTime
        /// </summary>
        public int GetWeekNumber(DateTime date)
        {
            CultureInfo info = new CultureInfo("zh-CN");
            return info.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Sunday);
        }

        /// <summary>
        /// Initialize button elements and add to grid
        /// </summary>
        private void InitializeDecade()
        {
            UniformGrid grdDecade = (UniformGrid)FindElement("Part_DecadeGrid");

            if (grdDecade != null)
            {
                for (int j = 0; j < 10; j++)
                {
                    var element = NewDayControl();
                    element.Click += new RoutedEventHandler(decadeModeButton_Click);
                    element.Tag = j - 1;
                    this._btnDecadeMode[j] = element;
                    this._btnDecadeMode[j].FontSize = this.FontSize + 1;
                    this._btnDecadeMode[j].Margin = new Thickness(12, 6, 12, 6);
                    grdDecade.Children.Add(element);
                }
            }
        }

        /// <summary>
        /// Initialize button elements and add to grid
        /// </summary>
        private void InitializeMonth()
        {
            UniformGrid grdMonth = (UniformGrid)FindElement("Part_MonthGrid");
            UniformGrid grdDay = (UniformGrid)FindElement("Part_DayGrid");
            UniformGrid grdWeek = (UniformGrid)FindElement("Part_WeekGrid");

            if (grdMonth != null && grdDay != null && grdWeek != null)
            {
                // initialize day buttons
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        var element = NewDayControl();
                        element.Content = string.Format("{0},{1}", i, j);
                        element.Click += new RoutedEventHandler(monthModeButton_Click);
                        element.PreviewMouseDown += new MouseButtonEventHandler(element_PreviewMouseDown);
                        element.PreviewMouseMove += new MouseEventHandler(element_PreviewMouseMove);
                        grdMonth.Children.Add(element);
                        this._btnMonthMode[i, j] = element;
                    }
                }

                // initialize days
                string[] dayOfWeeks = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                for (int j = 0; j < 7; j++)
                {
                    var element = new Label
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Top,
                        Padding = new Thickness(0),
                        FontFamily = this.FontFamily,
                        FontSize = this.FontSize,
                        FontStyle = this.FontStyle,
                        FontWeight = FontWeights.Medium,
                    };
                    element.Tag = dayOfWeeks[j];
                    element.Style = (Style)this.FindResource("DayNameStyle");
                    grdDay.Children.Add(element);
                }

                // initialize week numbers
                for (int i = 0; i < 6; i++)
                {
                    var element = new Label
                    {
                        Background = Brushes.Transparent,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        FlowDirection = FlowDirection.LeftToRight,
                        Padding = new Thickness(0),
                        Content = "",
                        MinWidth = 20,
                        FontFamily = this.FontFamily,
                        FontSize = this.FontSize,
                        FontStyle = this.FontStyle,
                        FontWeight = this.FontWeight,
                    };
                    element.Style = (Style)this.FindResource("WeekNumberStyle");
                    grdWeek.Children.Add(element);
                }
            }
        }

        /// <summary>
        /// Initialize button elements and add to grid
        /// </summary>
        private void InitializeYear()
        {
            UniformGrid grdYear = (UniformGrid)FindElement("Part_YearGrid");

            if (grdYear != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var element = NewDayControl();
                        element.Content = ((MonthList)j + i * 3 + 1).ToString();
                        element.Click += new RoutedEventHandler(yearModeButton_Click);
                        element.Tag = j + i * 3 + 1;
                        FontSize = 11;
                        this._btnYearMode[i, j] = element;
                        this._btnYearMode[i, j].FontSize = this.FontSize + 1;
                        this._btnYearMode[i, j].Margin = new Thickness(8, 4, 8, 4);
                        grdYear.Children.Add(element);
                    }
                }
            }
        }

        /// <summary>
        /// Test for default date
        /// </summary>
        private void IsTodaysDate()
        {
            if (this.DisplayMode == DisplayType.Month)
            {
                int r, c;
                MonthModeDateToRowColumn(DateTime.Today, out r, out c);
                if (this.IsTodayHighlighted)
                {
                    this._btnMonthMode[r, c].IsTodaysDate = true;
                }
                else
                {
                    this._btnMonthMode[r, c].IsTodaysDate = false;
                }
            }
        }

        /// <summary>
        /// List all days in grid
        /// </summary>
        /// <param name="month">month</param>
        /// <param name="year">year</param>
        private void ListDaysOfAllMonths(int month, int year)
        {
            DateTime firstDay = new DateTime(year, month, 1);
            int fstCol = (int)firstDay.DayOfWeek;
            int newMonth = month;

            // adjust for year
            if (month == 1)
            {
                year--;
                newMonth = 12;
            }
            else
            {
                newMonth--;
            }
            int days = DateTime.DaysInMonth(year, newMonth);

            // previous days
            for (int d = fstCol - 1; d >= 0; d--)
            {
                DateTime date = new DateTime(year, newMonth, days);
                _btnMonthMode[0, d].Content = days.ToString();
                _btnMonthMode[0, d].Tag = date;
                _btnMonthMode[0, d].IsEnabled = false;
                days--;
            }

            // future days
            newMonth = month;
            if (month == 12)
            {
                year++;
                newMonth = 1;
            }
            else
            {
                newMonth++;
            }

            days = DateTime.DaysInMonth(year, month);
            int day = 1;
            for (int d = fstCol + days + 1; d <= 42; d++)
            {
                int c = (d - 1) % 7;
                int r = (d - 1) / 7;
                DateTime date = new DateTime(year, newMonth, day);
                _btnMonthMode[r, c].Content = day.ToString();
                _btnMonthMode[r, c].Tag = date;
                _btnMonthMode[r, c].IsEnabled = false;
                day++;
            }
        }

        /// <summary>
        /// List the week numbers
        /// </summary>
        private void ListWeekNumbers(int month, int year)
        {
            UniformGrid grdWeek = (UniformGrid)FindElement("Part_WeekGrid");

            if (grdWeek != null)
            {
                grdWeek.Visibility = Visibility.Visible;
                for (int i = 0; i < 6; i++)
                {
                    Label label = (Label)grdWeek.Children[i];
                    label.Content = "";

                    if (_btnMonthMode[i, 6].Tag != null)
                    {
                        DateTime date = (DateTime)_btnMonthMode[i, 6].Tag;
                        label.Content = GetWeekNumber(date).ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Return relative position of date within grid
        /// </summary>
        private void MonthModeDateToRowColumn(DateTime date, out int r, out int c)
        {
            int year = date.Year;
            int month = date.Month;
            DateTime firstDay = new DateTime(year, month, 1);
            int fstCol = (int)firstDay.DayOfWeek - 1;

            r = (date.Day + fstCol) / 7;
            c = (date.Day + fstCol) % 7;
        }

        /// <summary>
        /// Create a DateButton with default properties
        /// </summary>
        /// <returns>DateButton</returns>
        private DateButton NewDayControl()
        {
            var element = new DateButton
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Thickness(0),
                Style = (Style)this.FindResource("InsideButtonsStyle"),
                Background = Brushes.Transparent,
                FontFamily = this.FontFamily,
                FontSize = this.FontSize,
                FontStyle = this.FontStyle,
                FontWeight = this.FontWeight,
            };
            return element;
        }

        /// <summary>
        /// Remove handlers for adorned drag
        /// </summary>
        private void RemoveDragEventHandlers()
        {
            ParentWindow.PreviewDragOver -= new DragEventHandler(ParentWindow_PreviewDragOver);
            ParentWindow.PreviewGiveFeedback -= new GiveFeedbackEventHandler(ParentWindow_PreviewGiveFeedback);
            ParentWindow.PreviewQueryContinueDrag -= new QueryContinueDragEventHandler(ParentWindow_PreviewQueryContinueDrag);
        }

        /// <summary>
        /// Reset Drag operation data
        /// </summary>
        private void ResetDragData()
        {
            IsDragging = false;
            _dragData.Data = null;
            _dragData.Parent = null;
            this.AllowDrop = false;
            if (_dragData.Adorner != null)
            {
                RemoveDragEventHandlers();
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                adornerLayer.Remove( _dragData.Adorner );
                _dragData.Adorner = null;
            }
        }

        /// <summary>
        /// Start the animation sequence
        /// </summary>
        private void RunMonthTransition()
        {
            UniformGrid grdMonth = (UniformGrid)FindElement("Part_MonthGrid");
            Grid scrollGrid = (Grid)FindElement("Part_ScrollGrid");

            if (grdMonth != null && scrollGrid != null)
            {
                // not first run
                if (grdMonth.ActualWidth > 0)
                {
                    IsAnimating = true;
                    int width = (int)grdMonth.ActualWidth;
                    int height = (int)grdMonth.ActualHeight;
                    scrollGrid.Visibility = Visibility.Visible;

                    // alternative method
                    //RenderTargetBitmap bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default); 
                    //bitmap.Render(grdMonth);
                    //scrollGrid.Background = new ImageBrush(bitmap);

                    // get bitmap for the current state
                    BitmapSource screen = CaptureScreen(grdMonth, 96, 96);
                    scrollGrid.Background = new ImageBrush(screen);
                    // reset month grid
                    SetMonthMode();

                    // two animations one for image, other for month grid
                    ThicknessAnimation marginAnimation = new ThicknessAnimation();
                    marginAnimation.Duration = TimeSpan.FromMilliseconds(1000);
                    marginAnimation.Completed += new EventHandler(marginAnimation_Completed);

                    ThicknessAnimation marginAnimation2 = new ThicknessAnimation();
                    marginAnimation2.Duration = TimeSpan.FromMilliseconds(1000);

                    // expected direction of flow
                    if (IsMoveForward)
                    {
                        grdMonth.Margin = new Thickness(width, 0, width, 0);
                        marginAnimation.From = new Thickness(0);
                        marginAnimation.To = new Thickness(-width, 0, width, 0);
                        marginAnimation2.From = new Thickness(width, 0, -width, 0);
                        marginAnimation2.To = new Thickness(0);
                    }
                    else
                    {
                        grdMonth.Margin = new Thickness(-width, 0, width, 0);
                        marginAnimation.From = new Thickness(0);
                        marginAnimation.To = new Thickness(width, 0, -width, 0);
                        marginAnimation2.From = new Thickness(-width, 0, width, 0);
                        marginAnimation2.To = new Thickness(0);
                    }
                    // launch animations
                    scrollGrid.BeginAnimation(UniformGrid.MarginProperty, marginAnimation);
                    grdMonth.BeginAnimation(UniformGrid.MarginProperty, marginAnimation2);
                }
                else
                {
                    // first pass
                    SetMonthMode();
                }
            }
        }

        /// <summary>
        /// Start the animation sequence
        /// </summary>
        private void RunYearTransition()
        {
            Calendar c = (Calendar)this;
            Grid grdAnimationContainer = (Grid)FindElement("Part_AnimationContainer");

            if (grdAnimationContainer != null)
            {
                IsAnimating = true;
                // width animation
                double width = grdAnimationContainer.ActualWidth;
                double height = grdAnimationContainer.ActualHeight;
                DoubleAnimation widthAnimation = new DoubleAnimation();
                widthAnimation.From = (width * .5f);
                widthAnimation.To = width;
                widthAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(200));

                // height animation
                DoubleAnimation heightAnimation = new DoubleAnimation();
                heightAnimation.From = (height * .5f);
                heightAnimation.To = height;
                heightAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(200));

                // add width and height propertiy targets to animation
                Storyboard.SetTargetName(widthAnimation, grdAnimationContainer.Name);
                Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(Grid.WidthProperty));
                Storyboard.SetTargetName(heightAnimation, grdAnimationContainer.Name);
                Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(Grid.HeightProperty));

                Storyboard stbCalenderTransition = new Storyboard();
                // add to storyboard
                stbCalenderTransition.Children.Add(widthAnimation);
                stbCalenderTransition.Children.Add(heightAnimation);

                // resize grid
                grdAnimationContainer.Width = (this.Width * .1f);
                grdAnimationContainer.Height = (this.Height * .1f);
                stbCalenderTransition.Completed += new EventHandler(stbCalenderTransition_Completed);
                // run animation
                stbCalenderTransition.Begin(grdAnimationContainer);
            }
        }

        /// <summary>
        /// Add bindings and events to elements
        /// </summary>
        private void SetBindings()
        {
            // this.MouseMove += new MouseEventHandler(Calendar_MouseMove);
            DateButton btnTitle = (DateButton)FindElement("Part_TitleButton");
            if (btnTitle != null)
            {
                btnTitle.FontFamily = this.FontFamily;
                btnTitle.FontSize = HeaderFontSize;
                btnTitle.FontStyle = this.FontStyle;
                btnTitle.FontWeight = HeaderFontWeight;
                btnTitle.Click += new RoutedEventHandler(titleButton_Click);
            }

            TextBlock txtCurrentDate = (TextBlock)FindElement("Part_CurrentDateText");
            if (txtCurrentDate != null)
            {
                txtCurrentDate.FontFamily = this.FontFamily;
                txtCurrentDate.FontSize = this.FontSize;
                txtCurrentDate.FontStyle = this.FontStyle;
                txtCurrentDate.FontWeight = FontWeights.DemiBold;
            }

            RepeatButton btnNext = (RepeatButton)FindElement("Part_NextButton");
            if (btnNext != null)
            {
                btnNext.Click += new RoutedEventHandler(nextButton_Click);
            }

            RepeatButton btnPrevious = (RepeatButton)FindElement("Part_PreviousButton");
            if (btnPrevious != null)
            {
                btnPrevious.Click += new RoutedEventHandler(previousButton_Click);
            }
        }

        /// <summary>
        /// Switch date display mode
        /// </summary>
        private void SetCalendar()
        {
            ClearSelectedDates(true);
            UpdateCalendar();
        }

        /// <summary>
        /// Sets display to decade mode
        /// </summary>
        private void SetDecadeMode()
        {
            Grid grdMonthContainer = (Grid)FindElement("Part_MonthContainer");
            UniformGrid grdDecade = (UniformGrid)FindElement("Part_DecadeGrid");
            UniformGrid grdYear = (UniformGrid)FindElement("Part_YearGrid");

            if (grdMonthContainer != null && grdDecade != null && grdYear != null)
            {
                grdMonthContainer.Visibility = grdYear.Visibility = Visibility.Collapsed;
                grdDecade.Visibility = Visibility.Visible;

                // run the animation
                if (IsAnimated)
                {
                    RunYearTransition();
                }

                int decade = DisplayDate.Year - DisplayDate.Year % 10;
                for (int i = 0; i < 10; i++)
                {
                    int y = i + decade;
                    if (y >= DisplayDateStart.Year && y <= DisplayDateEnd.Year)
                    {
                        _btnDecadeMode[i].Content = decade + i;
                        _btnDecadeMode[i].Tag = decade + i;
                        _btnDecadeMode[i].IsEnabled = true;

                    }
                    else
                    {
                        _btnDecadeMode[i].Content = "";
                        _btnDecadeMode[i].Tag = null;
                        _btnDecadeMode[i].IsEnabled = false;
                    }
                }
                DateButton btnTitle = (DateButton)FindElement("Part_TitleButton");
                if (btnTitle != null)
                {
                    btnTitle.Content = decade.ToString() + "-" + (decade + 9).ToString();
                }
            }
        }

        /// <summary>
        /// Sets display to month mode
        /// </summary>
        private void SetMonthMode()
        {
            Grid grdMonthContainer = (Grid)FindElement("Part_MonthContainer");
            UniformGrid grdDecade = (UniformGrid)FindElement("Part_DecadeGrid");
            UniformGrid grdYear = (UniformGrid)FindElement("Part_YearGrid");

            if (grdMonthContainer != null && grdDecade != null && grdYear != null)
            {
                grdDecade.Visibility = grdYear.Visibility = Visibility.Collapsed;
                if (grdMonthContainer.Visibility != Visibility.Visible)
                {
                    grdMonthContainer.Visibility = Visibility.Visible;
                    if (IsAnimated)
                    {
                        RunYearTransition();
                    }
                }

                int year = DisplayDate.Year;
                int month = DisplayDate.Month;
                int days = DateTime.DaysInMonth(year, month);
                DateTime firstDay = new DateTime(year, month, 1);
                int fstCol = (int)firstDay.DayOfWeek;

                // clear buttons
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        _btnMonthMode[i, j].Content = "";
                        _btnMonthMode[i, j].IsEnabled = false;
                        _btnMonthMode[i, j].IsTodaysDate = false;
                        _btnMonthMode[i, j].IsCurrentMonth = false;
                        _btnMonthMode[i, j].IsBlackOut = false;
                    }
                }
                // write day numbers
                for (int d = 1; d <= days; d++)
                {
                    DateTime date = new DateTime(year, month, d);
                    if (date >= DisplayDateStart && date <= DisplayDateEnd)
                    {
                        int column, row;
                        row = (d + fstCol - 1) / 7;
                        column = (d + fstCol - 1) % 7;
                        _btnMonthMode[row, column].Content = d.ToString();
                        _btnMonthMode[row, column].IsEnabled = true;
                        _btnMonthMode[row, column].IsCurrentMonth = true;
                        _btnMonthMode[row, column].Tag = date;
                        // restore selected date(s)
                        if (SelectionMode == SelectionType.Single)
                        {
                            if (date == SelectedDate)
                            {
                                _btnMonthMode[row, column].IsSelected = true;
                            }
                        }
                        else
                        {
                            if (SelectedDates.Contains(date))
                            {
                                _btnMonthMode[row, column].IsSelected = true;
                            }
                        }
                        if (_blackoutDates.ContainsAny(new DateRangeHelper(date)))
                        {
                            _btnMonthMode[row, column].IsBlackOut = true;
                            _btnMonthMode[row, column].IsEnabled = false;
                        }
                    }
                }

                // show all days
                if (ShowDaysOfAllMonths)
                {
                    ListDaysOfAllMonths(month, year);
                }
                if (WeekColumnVisibility == Visibility.Visible)
                {
                    ListWeekNumbers(month, year);
                }
                //footer
                TextBlock txtCurrentDate = (TextBlock)FindElement("Part_CurrentDateText");
                if (txtCurrentDate != null)
                {
                    txtCurrentDate.Text = "Today: " + DateTime.Today.ToShortDateString();
                }
                // header title
                DateButton btnTitle = (DateButton)FindElement("Part_TitleButton");
                if (btnTitle != null)
                {
                    btnTitle.Content = ((MonthList)month).ToString() + " " + year.ToString();
                }
                // current selected
                if (DisplayDate.Month == DateTime.Today.Month)
                {
                    this.IsTodaysDate();
                }
            }
        }

        /// <summary>
        /// Sets display to year mode
        /// </summary>
        private void SetYearMode()
        {
            Grid grdMonthContainer = (Grid)FindElement("Part_MonthContainer");
            UniformGrid grdDecade = (UniformGrid)FindElement("Part_DecadeGrid");
            UniformGrid grdYear = (UniformGrid)FindElement("Part_YearGrid");

            if (grdMonthContainer != null && grdDecade != null && grdYear != null)
            {
                grdMonthContainer.Visibility = grdDecade.Visibility = Visibility.Collapsed;
                grdYear.Visibility = Visibility.Visible;

                // run the animation
                if (IsAnimated)
                {
                    RunYearTransition();
                }
                DateButton btnTitle = (DateButton)FindElement("Part_TitleButton");
                if (btnTitle != null)
                {
                    btnTitle.Content = this.DisplayDate.Year.ToString();
                }

                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        int month = j + i * 3 + 1;
                        if (new DateTime(DisplayDate.Year, month, DateTime.DaysInMonth(DisplayDate.Year, month)) >= DisplayDateStart &&
                            new DateTime(DisplayDate.Year, month, 1) <= DisplayDateEnd)
                        {
                            _btnYearMode[i, j].Content = ((MonthList)month).ToString();
                            _btnYearMode[i, j].IsEnabled = true;
                        }
                        else
                        {
                            _btnYearMode[i, j].Content = "";
                            _btnYearMode[i, j].IsEnabled = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Drag timer used to ensure timely clsure of secondary window
        /// </summary>
        private void StartDragTimer()
        {
            if (_dispatcherTimer == null)
            {
                _dispatcherTimer = new DispatcherTimer();
                _dispatcherTimer.Tick += new EventHandler(_dispatcherTimer_Tick);
                _dispatcherTimer.Interval = new TimeSpan(1000);
            }
            _dispatcherTimer.Start();
        }

        /// <summary>
        /// Halt drag timer and reset
        /// </summary>
        private void StopDragTimer()
        {
            _dispatcherTimer.Stop();
            ResetDragData();
        }

        /// <summary>
        /// Updates the calendar display
        /// </summary>
        internal void UpdateCalendar()
        {
            switch (this.DisplayMode)
            {
                case DisplayType.Month:
                    {
                        if (IsAnimating || IsDesignTime || !IsAnimated)
                        {
                            SetMonthMode();
                        }
                        else
                        {
                            RunMonthTransition();
                        }
                        break;
                    }
                case DisplayType.Year:
                    {
                        SetYearMode();
                        break;
                    }
                case DisplayType.Decade:
                    {
                        SetDecadeMode();
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException("The DisplayMode value is not in acceptable range");
                    }
            }
        }
        #endregion
    }
}