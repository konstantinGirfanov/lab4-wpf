using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace lab4_wpf
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    /// 
    public partial class Window1 : Window
    {
        private static List<Step> Steps;
        private static ObservableCollection<Value> Data;
        private static int CurrentOperation;
        private static List<int> DataForSort;
        private static bool Stop { get; set; } = false;
        private static bool Pause { get; set; } = false;

        public Window1()
        {
            Steps = new();
            Data = new();
            CurrentOperation = 0;
            DataForSort = new();

            InitializeComponent();
            Array.ItemsSource = Data;
            Array.SelectionUnit = DataGridSelectionUnit.Cell;
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
            InnerSorts.BubbleSort(DataForSort);
            Steps = InnerSorts.Steps;
        }

        private void DoAction(Step step)
        {
            DescList.Items.Add(step.StepDecription);
            switch (step.Operation)
            {
                case Operations.Select:
                    SelectItems(step.Indexes);
                    break;

                case Operations.Switch:
                    SwitchItems(step.Indexes);
                    break;

                default:
                    Array.Items.Refresh();
                    break;
            }
        }

        private void SelectItems(int[] indexes)
        {
            Array.SelectedCells.Clear();
            foreach (int index in indexes)
            {
                DataGridCellInfo newCell = new(Array.Items[index], Array.Columns[1]);
                Array.SelectedCells.Add(newCell);
            }
        }

        private void SwitchItems(int[] indexes)
        {
            int first = indexes[0];
            int second = indexes[1];
            (Data[second], Data[first]) = (Data[first], Data[second]);
            Array.Items.Refresh();
            SelectItems(indexes);
        }

        private void EnterData(object sender, RoutedEventArgs e)
        {
            Stop = false;
            DescList.Items.Clear();
            CurrentOperation = 0;
            Data.Clear();
            DataForSort.Clear();
            Steps.Clear();
            string[] data = dataText.Text.Split(" ");
            for (int i = 0; i < data.Length; i++)
            {
                if (int.TryParse(data[i], out int value))
                {
                    DataForSort.Add(value);
                    Data.Add(new Value(value.ToString(), value.ToString()));
                }
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
                SelectItems(Steps[CurrentOperation - 1].Indexes);
            }
        }

        private void ReswitchItems(int[] indexes)
        {
            int first = indexes[0];
            int second = indexes[1];
            (Data[second], Data[first]) = (Data[first], Data[second]);
            Array.Items.Refresh();
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
        }
    }
}