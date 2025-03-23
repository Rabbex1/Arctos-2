using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Arctos.Wpf.Helpers
{
    public class DataGridCellHelpers
    {
        public static DataGridCell? GetCell(DataGridCellInfo dataGridCellInfo)
        {
            if (!dataGridCellInfo.IsValid)
                return null;
            var cellContent = dataGridCellInfo.Column.GetCellContent(dataGridCellInfo.Item);
            return cellContent?.Parent as DataGridCell;
        }

        public static int GetRowIndex(DataGridCell dataGridCell)
        {
            if (dataGridCell == null)
                return -1;
            // Use reflection to get DataGridCell.RowDataItem property value.
            var rowDataItemProperty = dataGridCell.GetType().GetProperty("RowDataItem", BindingFlags.Instance | BindingFlags.NonPublic);
            if (rowDataItemProperty == null)
                return -1;

            var dataGrid = GetDataGridFromChild(dataGridCell);
            var item = rowDataItemProperty.GetValue(dataGridCell, null);
            if (item == null)
                return -1;
            return dataGrid.Items.IndexOf(item);
        }

        public static DataGrid GetDataGridFromChild(DependencyObject dataGridPart)
        {
            if (dataGridPart == null)
                throw new NullReferenceException("Control is null.");
            if (VisualTreeHelper.GetParent(dataGridPart) == null)
            {
                throw new NullReferenceException("Control is null.");
            }
            if (VisualTreeHelper.GetParent(dataGridPart) is DataGrid)
            {
                return (DataGrid)VisualTreeHelper.GetParent(dataGridPart);
            }
            else
            {
                return GetDataGridFromChild(VisualTreeHelper.GetParent(dataGridPart));
            }
        }

        public static void SelectRowByIndex(DataGrid dataGrid, int rowIndex)
        {
            if (!dataGrid.SelectionUnit.Equals(DataGridSelectionUnit.FullRow))
                throw new ArgumentException("The SelectionUnit of the DataGrid must be set to FullRow.");

            if (rowIndex < 0 || rowIndex > (dataGrid.Items.Count - 1))
                throw new ArgumentException($"{rowIndex} is an invalid row index.");

            dataGrid.SelectedItems.Clear();
            /* set the SelectedItem property */
            object item = dataGrid.Items[rowIndex]; // = Product X
            dataGrid.SelectedItem = item;

            DataGridRow? row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            if (row == null)
            {
                /* bring the data item (Product object) into view
                 * in case it has been virtualized away */
                dataGrid.ScrollIntoView(item);
                row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            }
            //TODO: Retrieve and focus a DataGridCell object
        }
    }
}
