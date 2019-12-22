using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TaskManager.OperateDll;
using TaskManager.TabsModels;

namespace TaskManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private ObservableCollection<AppsModel> myAppsList;
        //private int index;
        private DispatcherTimer at;
        private DispatcherTimer pt;
        private DispatcherTimer st;
        private DispatcherTimer stat_t;
        public string AppNameToDelete { get; set; }
        public string ProcessToDelete { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();
            at = new System.Windows.Threading.DispatcherTimer();
            at.Interval = new TimeSpan(0, 0, 0, 0, 100); // 100 Milliseconds
            at.Tick += new EventHandler(at_Tick);
            AppsDgd.CurrentCellChanged += AppsDgd_CurrentCellChanged;

            pt = new System.Windows.Threading.DispatcherTimer();
            pt.Interval = new TimeSpan(0, 0, 0, 0, 3000); // 1500 Milliseconds
            pt.Tick += new EventHandler(pt_Tick);

            st = new System.Windows.Threading.DispatcherTimer();
            st.Interval = new TimeSpan(0, 0, 0, 0, 3000); // 1500 Milliseconds
            st.Tick += new EventHandler(st_Tick);

            stat_t = new System.Windows.Threading.DispatcherTimer();
            stat_t.Interval = new TimeSpan(0, 0, 0, 0, 2000); // 2000 Milliseconds
            stat_t.Tick += new EventHandler(statBar_Tick);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProcessTabCtrlPage.IsSelected)
            {
                pt.Start();
                at.Stop();
                st.Stop();
            }
            if (ServicesTabCtrlPage.IsSelected)
            {
                st.Start();
                at.Stop();
                pt.Stop();
            }
            if (AppsTabCtrlPage.IsSelected)
            {
                stat_t.Start();
                at.Start();
                st.Stop();
                pt.Stop();
            }
        }

        private async void pt_Tick(object sender, EventArgs e)
        {
            List<ProcessModel> myprossList = new List<ProcessModel>();
            myprossList = await Task.Run(() => { return MyProcesses.GetProcesses(); });
            if (CancelTaskProcessCbx.IsChecked == true)
            {
                ProcessesDgd.ItemsSource = myprossList;
            }
            else
            {
                ProcessesDgd.ItemsSource = MyProcesses.SortedList("Администратор", myprossList);
            }
        }

        private async void at_Tick(object sender, EventArgs e)
        {
            ObservableCollection<AppsModel> temp = new ObservableCollection<AppsModel>();
            temp = await Task.Run(() => { return user32.GetHandles(); });
            AppsDgd.ItemsSource = temp;
        }

        private async void st_Tick(object sender, EventArgs e)
        {
            List<ServicesModel> myServicesList = new List<ServicesModel>();

            //object myServicesList = null; // Used to store the return value
            //var thread = new Thread(
            //  () =>
            //  {
            //      myServicesList = MyProcesses.GetServices(); // Publish the return value
            //  });
            //thread.Start();
            //thread.Join();

            //myServicesList = MyProcesses.GetServices();
            //myServicesList = await Task.Run(() => { return MyProcesses.GetServices(); });
            
            myServicesList = await MyProcesses.GetServices1();
            ServicesDgd.ItemsSource = myServicesList;

        }

        private void statBar_Tick(object sender, EventArgs e)
        {
            ShowStatistic();
        }

        void AppsDgd_CurrentCellChanged(object sender, EventArgs e)
        {
            AppsModel temp = new AppsModel();
            temp = AppsDgd.CurrentItem as AppsModel;
            AppNameToDelete = temp.AppsDescription;
        }

        void ProcessesDgd_CurrentCellChanged(object sender, EventArgs e)
        {
            try
            {
                ProcessModel temp = new ProcessModel();
                temp = ProcessesDgd.CurrentItem as ProcessModel;
                ProcessToDelete = temp.ProcessId;
            }
            catch (Exception ex) { }

        }

        private void SelectionChangedMyEvent(object sender, SelectionChangedEventArgs e)

        {

        }

        private void CancelTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AppNameToDelete != "")
            {
                try
                {
                    user32.closeWindow1(AppNameToDelete);
                }
                catch (Exception ex) { }
            }
            AppNameToDelete = "";

        }


        private void NewTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            NewTask myTask = new NewTask();
            if (myTask.ShowDialog() == true)
            {
                try
                {
                    using (Process myProcess = new Process())
                    {
                        myProcess.StartInfo.UseShellExecute = false;
                        myProcess.StartInfo.FileName = myTask.getTaskCbx.SelectedItem.ToString();
                        myProcess.StartInfo.CreateNoWindow = true;
                        myProcess.Start();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }

        private void OpenTask_Click(object sender, RoutedEventArgs e)
        {
            NewTask myTask = new NewTask();
            if (myTask.ShowDialog() == true)
            {
                try
                {
                    using (Process myProcess = new Process())
                    {
                        myProcess.StartInfo.UseShellExecute = false;
                        myProcess.StartInfo.FileName = myTask.getTaskCbx.SelectedItem.ToString();
                        myProcess.StartInfo.CreateNoWindow = true;
                        myProcess.Start();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public async void ShowStatistic()
        {
            try
            {
                processLoadTblkData.Text = await Task.Run(() => { return MyProcesses.getCurrentCpuUsage();});
                procesMemoryTblkData.Text = await Task.Run(() => { return MyProcesses.getAvailableRAM(); });
                processQtyTblkData.Text = await Task.Run(() => { return MyProcesses.getCurrentProcessQty(); });

            }
            catch (Exception ex) { };
        }

        private void CancelProccBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessToDelete != "")
            {
                try
                {   
                    MyProcesses.KillProcessAndChildren(Convert.ToInt32(ProcessToDelete));
                }
                catch (Exception ex) { }
            }
            ProcessToDelete = "";

        }


    }
}
