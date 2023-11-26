using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
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
    /// Логика взаимодействия для NaturalSortWindow.xaml
    /// </summary>
    public partial class NaturalSortWindow : Window
    {
        private static List<Step> Steps = new();
        private static ObservableCollection<Value> Data = new();
        private static int CurrentOperation = 0;
        private static List<int> DataForSort = new();
        private static List<ObservableCollection<Value>> TempData = new() { new ObservableCollection<Value>(), new ObservableCollection<Value>() };
        public NaturalSortWindow()
        {
            InitializeComponent();

            Array.ItemsSource = Data;
            Array.SelectionUnit = DataGridSelectionUnit.Cell;
            Array.CellStyle = (Style)Resources["DataGridCellStyle1"];

            TempArray.ItemsSource = TempData[0];
            TempArray.CellStyle = (Style)Resources["DataGridCellStyle1"];
            TempArray.SelectionUnit = DataGridSelectionUnit.Cell;
            TempArray1.ItemsSource = TempData[1];
            TempArray1.SelectionUnit = DataGridSelectionUnit.Cell;
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
            var sort = new NaturalSort();
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
                Style st = new();
                Array.CellStyle = (Style)Resources["DataGridCellStyle1"];
                grid.SelectedCells.Add(newCell);
            }
        }

        /*private void SwitchItems(int[] indexes)
        {
            int first = indexes[0];
            int second = indexes[1];
            (Data[second], Data[first]) = (Data[first], Data[second]);
            Array.Items.Refresh();
            SelectItems(indexes);
        }*/

        private void WriteToTemp(SwapDataInformation info)
        {
            foreach (DataGrid grid in dataGrids.Items)
            {
                grid.SelectedCells.Clear();
            }
            if (Steps[CurrentOperation - 4].StepDecription == "Слияние завершено.")
            {
                foreach (ObservableCollection<Value> tempArray in TempData)
                {
                    tempArray.Clear();
                }
            }


            /*foreach (ObservableCollection<Value> tempArray in TempData)
            {
                tempArray.Clear();
            }*/

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
