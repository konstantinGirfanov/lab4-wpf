using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private static bool Pause { get; set; } = false;
        private static Stack<Value>? PreviousValues { get; set; } = new();
        private static Stack<ObservableCollection<Value>>? PreviousTempArrays = new();
        private static bool Stop { get; set; } = false;

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
            for (int i = 0; i < int.Parse(input.Split(" ")[2]); i++)
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
                    var copy = new ObservableCollection<Value>();
                    foreach (Value value in tempArray)
                    {
                        copy.Add(value);
                    }
                    PreviousTempArrays.Push(copy);

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
            PreviousValues.Push(Data[info.DestinationIndex]);
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
            Stop = false;
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
                if (Steps[CurrentOperation].Operation == Operations.WriteToTemp)
                {
                    Step currentStep = Steps[CurrentOperation];
                    int tempFile = currentStep.DataInfo.SourceFileNumber - 1;
                    TempData[tempFile].RemoveAt(TempData[tempFile].Count - 1);

                }
                else if (Steps[CurrentOperation].Operation == Operations.WriteFromTemp)
                {
                    foreach (DataGrid grid in dataGrids.Items)
                    {
                        grid?.SelectedCells.Clear();
                    }

                    Step currentStep = Steps[CurrentOperation];
                    Data[currentStep.DataInfo.DestinationIndex] = PreviousValues.Pop();
                    Array.SelectedCells.Clear();
                    Array.Items.Refresh();
                }
                else if (Steps[CurrentOperation].Operation == Operations.SelectInTemp)
                {
                    foreach (DataGrid grid in dataGrids.Items)
                    {
                        grid?.SelectedCells.Clear();
                    }
                    Array.SelectedCells.Clear();
                }
                else if (Steps[CurrentOperation].Operation == Operations.Select)
                {
                    for (int i = 0; i < TempData.Count; i++)
                    {
                        TempData[TempData.Count - 1] = PreviousTempArrays.Pop();
                    }

                    for (int i = 0; i < TempData.Count; i++)
                    {
                        DataGrid grid = (DataGrid)dataGrids.Items[i];
                        grid.ItemsSource = TempData[i];
                    }

                    SelectItems(Steps[CurrentOperation].Indexes, 0);
                }
                else if (Regex.IsMatch(Steps[CurrentOperation].StepDecription, @"Длина сегментов умножается(\w*)"))
                {
                    for (int i = 0; i < TempData.Count; i++)
                    {
                        TempData[TempData.Count - 1 - i] = PreviousTempArrays.Pop();
                    }

                    for (int i = 0; i < TempData.Count; i++)
                    {
                        DataGrid grid = (DataGrid)dataGrids.Items[i];
                        grid.ItemsSource = TempData[i];
                    }
                }


                if (CurrentOperation - 1 != 0)
                {
                    if (Steps[CurrentOperation - 1].Operation == Operations.SelectInTemp)
                    {
                        Step prevStep = Steps[CurrentOperation - 1];
                        SelectInTemp(prevStep.SelectInfo);
                    }
                    else if (Steps[CurrentOperation - 1].Operation == Operations.WriteFromTemp)
                    {
                        Array.SelectedCells.Clear();
                        var info = Steps[CurrentOperation - 1].DataInfo;
                        SelectItems(new int[1] { info.SourceIndex }, info.SourceFileNumber);
                        SelectItems(new int[1] { info.DestinationIndex }, 0);
                    }
                    else if (Steps[CurrentOperation - 1].Operation == Operations.WriteToTemp)
                    {
                        Array.SelectedCells.Clear();
                        Step prevStep = Steps[CurrentOperation - 1];
                        var info = prevStep.DataInfo;
                        SelectItems(new int[1] { info.SourceIndex }, 0);
                        SelectItems(new int[1] { info.DestinationIndex }, info.SourceFileNumber);
                    }
                    else if (Steps[CurrentOperation - 1].Operation == Operations.None)
                    {
                        Array.SelectedCells.Clear();
                        foreach (DataGrid grid in dataGrids.Items)
                        {
                            grid?.SelectedCells.Clear();
                        }
                    }
                }

                DescList.Items.RemoveAt(DescList.Items.Count - 1);
            }
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                if (CurrentOperation != Steps.Count && !Pause)
                {
                    NextStep(null, null);
                }

                await Task.Delay(int.Parse(Delay.Text));

                if (Stop)
                {
                    break;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Pause = !Pause;
        }

        private void ClearWindow(object sender, CancelEventArgs e)
        {
            Stop = true;
            Steps.Clear();
            Data.Clear();
            DataForSort.Clear();
            CurrentOperation = 0;
            PreviousTempArrays.Clear();
            PreviousValues.Clear();
            foreach (var temp in TempData)
            {
                temp.Clear();
            }
        }
    }
}
