using CsvHelper;
using Syncfusion.Maui.Data;
using Syncfusion.Maui.DataGrid;
using System.Globalization;
using windiff.Extensions;
using MenuItem = Syncfusion.Maui.DataGrid.MenuItem;

namespace windiff
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            RestoreColumn();
            BindingContext = new ItemInfoRepository();
        }

        void RestoreColumn()
        {
            foreach (var column in grid.Columns)
            {
                ColumnWidth(column);
                ColumnVisible(column);
            }
            foreach (var item in grid.HeaderContextMenu.Items)
            {
                MenuItemIcon(item);
            }
        }
        static void ColumnWidth(DataGridColumn column)
        {
            var key = $"ColumnWidth:{column.MappingName}";
            if (Preferences.ContainsKey(key))
            {
                column.Width = Preferences.Get(key, column.Width);
            }
        }
        static void ColumnVisible(DataGridColumn column)
        {
            var key = $"ColumnVisible:{column.MappingName}";
            if (Preferences.ContainsKey(key))
            {
                column.Visible = Preferences.Get(key, column.Visible);
            }
        }
        static void MenuItemIcon(MenuItem item)
        {
            var key = $"ColumnVisible:{item.Text}";
            if (Preferences.ContainsKey(key))
            {
                var visible = Preferences.Get(key, false);
                if (item.Icon is Label label)
                {
                    label.Text = visible ? "\uf00c" : string.Empty;
                }
            }
        }

        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && BindingContext is ItemInfoRepository vm)
            {
                picker.IsEnabled = false;

                var item = picker.SelectedItem.ToString();
                if (item != null)
                {
                    grid.GroupColumnDescriptions.Clear();
                    if (string.IsNullOrEmpty(item) == false)
                    {
                        grid.GroupColumnDescriptions.Add(new GroupColumnDescription
                        {
                            ColumnName = item,
                        });
                    }
                }

                picker.IsEnabled = true;
            }
        }
        private void Grid_ColumnResizing(object? sender, DataGridColumnResizingEventArgs e)
        {
            if (e.ResizingState == DataGridProgressState.Completed && sender is SfDataGrid grid)
            {
                grid.Columns.ForEach(column =>
                {
                    var key = $"ColumnWidth:{column.MappingName}";
                    Preferences.Set(key, column.Width);
                });
            }
        }
        private void Grid_ContextMenuItemClicked(object? sender, ContextMenuItemClickedEventArgs e)
        {
            if (sender is SfDataGrid grid)
            {
                var column = grid.Columns.FirstOrDefault(item => item.MappingName == e.MenuItem.Text);
                if (column != null)
                {
                    var visible = !column.Visible;
                    var key = $"ColumnVisible:{e.MenuItem.Text}";
                    Preferences.Set(key, visible);
                    column.Visible = visible;
                    if (e.MenuItem.Icon is Label label)
                    {
                        label.Text = visible ? "\uf00c" : string.Empty;
                    }
                }
            }
        }
        private void Button_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is ItemInfoRepository vm)
            {
                var searching = new DirectoryInfo(vm.Searching);
                var snapshots = new DirectoryInfo("snapshots");
                if (snapshots.Exists == false)
                {
                    snapshots.Create();
                }

                using (var writer = new StreamWriter(Path.Combine(snapshots.FullName, $"{DateTime.Now.ToString("yyyyMMddHHmmss")}-{searching.Name}.csv")))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(vm.Items);
                }
            }
        }
    }
}
