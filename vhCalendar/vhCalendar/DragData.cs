namespace vhCalendar
{
    /// <summary></summary>
    internal struct DragData
    {
        internal DateButton Parent;
        internal object Data;
        internal DragAdorner Adorner;
        /// <summary>
        /// Stores drag data
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="data"></param>
        /// <param name="adorner"></param>
        internal DragData(DateButton parent, object data, DragAdorner adorner)
        {
            Parent = parent;
            Data = data;
            Adorner = adorner;
        }
    }
}
