using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace TaskManager
{
    /// <summary>
    /// Логика взаимодействия для NewTask.xaml
    /// </summary>
    public partial class NewTask : Window, INotifyPropertyChanged
    {
        private string _selectedItem;
        private string sSelectedFile;
        private ObservableCollection<string> _items = new ObservableCollection<string>()
        { "calc.exe", "notepad.exe", "mspaint.exe"};
        public NewTask()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog choofdlog = new OpenFileDialog();
                choofdlog.Filter = "All Files (*.*)|*.*";
                choofdlog.FilterIndex = 1;
                choofdlog.Multiselect = true;

                if (choofdlog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    sSelectedFile = choofdlog.FileName;
                    _items.Add(sSelectedFile);
                    getTaskCbx.SelectedIndex = _items.Count - 1;
                }
                else
                    sSelectedFile = string.Empty;
            }
            catch (Exception ex) { }

        }

        public IEnumerable Items
        {
            get { return _items; }
        }

        public string SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public string NewItem
        {
            set
            {
                if (SelectedItem != null)
                {
                    return;
                }
                if (!string.IsNullOrEmpty(value))
                {
                    _items.Add(value);
                    SelectedItem = value;
                }
            }
            get { return SelectedItem; }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
