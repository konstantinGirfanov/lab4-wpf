using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace lab4_wpf.Windows
{
    /// <summary>
    /// Логика взаимодействия для MultipathSortWindow.xaml
    /// </summary>
    public partial class MultipathSortWindow : Window
    {
        private static List<Step> Steps = new();
        private static ObservableCollection<Value> Data = new();
        private static int CurrentOperation = 0;
        private static List<int> DataForSort = new();
        private static List<ObservableCollection<Value>> TempData = new();

        public MultipathSortWindow()
        {
            InitializeComponent();

            Array.ItemsSource = Data;
            Array.SelectionUnit = DataGridSelectionUnit.Cell;
            Array.CellStyle = (Style)Resources["DataGridCellStyle1"];
        }

        private void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGridColumn CurrentColumn = null;
            var dataGrid = (DataGrid)sender;
            if (CurrentColumn != null)
                CurrentColumn.CellStyle = null;
            CurrentColumn = dataGrid.CurrentColumn;
            if (CurrentColumn != null)
                CurrentColumn.CellStyle = (Style)dataGrid.Resources["SelectedColumnStyle"];
        }

        private void NextStep(object sender, RoutedEventArgs e)
        {
            if (CurrentOperation != Steps.Count)
            {
                DoAction(Steps[CurrentOperation]);
                CurrentOperation++;
            }
        }

        private void GetSteps(object sender, RoutedEventArgs e)
        {
            string input = dataText.Text;
            for(int i = 0; i < int.Parse(input.Split(" ")[2]); i++)
            {
                TempData.Add(new ObservableCollection<Value>());

                DataGrid grid = new();
                grid.CellStyle = (Style)Resources["DataGridCellStyle1"];
                grid.SelectionUnit = DataGridSelectionUnit.Cell;
                grid.ItemsSource = TempData[i];
                dataGrids.Items.Add(grid);
            }

            var sort = new MultipathSort();
            sort.sortInfo = dataText.Text;
            sort.Sort();
            Steps = sort.Steps;
        }

        private void DoAction(Step step)
        {
            DescList.Items.Add(step.StepDecription);
            switch (step.Operation)
            {
                case Operations.Select:
                    foreach (DataGrid grid in dataGrids.Items)
                    {
                        grid.SelectedCells.Clear();
                    }
                    SelectItems(step.Indexes, 0);
                    break;

                case Operations.WriteToTemp:
                    WriteToTemp(step.DataInfo);
                    break;

                case Operations.WriteFromTemp:
                    WriteFromTemp(step.DataInfo);
                    break;

                case Operations.SelectInTemp:
                    SelectInTemp(step.SelectInfo);
                    break;

                default:
                    foreach (DataGrid grid in dataGrids.Items)
                    {
                        grid.SelectedCells.Clear();
                    }
                    Array.Items.Refresh();
                    break;
            }
        }

        private void SelectItems(int[] indexes, int dataGridIndex)
        {
            DataGrid grid;
            if (dataGridIndex == 0)
            {
                grid = Array;
            }
            else
            {
                grid = (DataGrid)dataGrids.Items[dataGridIndex - 1];
            }

            grid.SelectedCells.Clear();
            foreach (int index in indexes)
            {
                DataGridCellInfo newCell = new(grid.Items[index], grid.Columns[1]);
                Array.CellStyle = (Style)Resources["DataGridCellStyle1"];
                grid.SelectedCells.Add(newCell);
            }
        }

        private void WriteToTemp(SwapDataInformation info)
        {
            foreach (DataGrid grid in dataGrids.Items)
            {
                grid.SelectedCells.Clear();
            }

            if (Regex.IsMatch(Steps[CurrentOperation - 2].StepDecription, @"Длина сегментов умножается(\w*)"))
            {
                foreach (ObservableCollection<Value> tempArray in TempData)
                {
                    tempArray.Clear();
                }
            }

            SelectItems(new int[1] { info.SourceIndex }, 0);
            TempData[info.SourceFileNumber - 1].Add(info.Data);
            SelectItems(new int[1] { info.DestinationIndex }, info.SourceFileNumber);
        }

        private void WriteFromTemp(SwapDataInformation info)
        {
            foreach (DataGrid grid in dataGrids.Items)
            {
                grid.SelectedCells.Clear();
            }

            SelectItems(new int[1] { info.SourceIndex }, info.SourceFileNumber);
            Data[info.DestinationIndex] = info.Data;
            SelectItems(new int[1] { info.DestinationIndex }, 0);
        }

        private void SelectInTemp(List<SelectInfo> selectInfo)
        {
            foreach (DataGrid grid in dataGrids.Items)
            {
                grid.SelectedCells.Clear();
            }
            Array.SelectedCells.Clear();
            foreach (SelectInfo info in selectInfo)
            {
                SelectItems(new int[1] { info.IndexInFile }, info.FileNumber);
            }
        }
        private void EnterData(object sender, RoutedEventArgs e)
        {
            DescList.Items.Clear();
            CurrentOperation = 0;
            Data.Clear();
            DataForSort.Clear();
            Steps.Clear();
            dataGrids.Items.Clear();

            string fileName = $"../../../{dataText.Text.Split(" ")[0]}";
            int col = int.Parse(dataText.Text.Split(" ")[1]);
            foreach (string line in File.ReadAllLines(fileName))
            {
                Data.Add(new Value(line, line.Split(";")[col]));
            }


            GetSteps(null, null);
        }

        private void PrevStep(object sender, RoutedEventArgs e)
        {
            if (CurrentOperation - 1 != 0)
            {
                CurrentOperation--;
                if (Steps[CurrentOperation].Operation == Operations.Switch)
                {
                    ReswitchItems(Steps[CurrentOperation].Indexes);
                }
                DescList.Items.RemoveAt(DescList.Items.Count - 1);
                SelectItems(Steps[CurrentOperation - 1].Indexes, 0);
            }
        }

        private void ReswitchItems(int[] indexes)
        {
            int first = indexes[0];
            int second = indexes[1];
            (Data[second], Data[first]) = (Data[first], Data[second]);
            Array.Items.Refresh();
        }
    }
}
