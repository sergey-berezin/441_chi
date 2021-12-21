using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Library;
using Ookii.Dialogs.Wpf;

namespace Lab_GUI
{


    public partial class MainWindow : Window
    {
        public static ObservableCollection<Iofo> result = new ObservableCollection<Iofo>();

        private static async Task main()
        {
            string type;
            string image;

            while (true)
            {
                (type, image) = await Program.bufferblock.ReceiveAsync();
               

                bool flag = true;
                foreach (Iofo r in result)
                {
                    if (r.Info == type)
                    {
                        r.list.Add(image);
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    result.Add(new Iofo(type, image));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = result;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            if ((bool)dialog.ShowDialog())
                Path.Text = dialog.SelectedPath;
        }

        private async void Strat_Click(object sender, RoutedEventArgs e)
        {
            Strat.IsEnabled = false;
            result.Clear();
            Program.cancelTokenSource = new CancellationTokenSource();
            Program.cancel = Program.cancelTokenSource.Token;

            await Task.WhenAll(Program.ImageRecog(Path.Text), main());
            Strat.IsEnabled = true;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Program.cancelTokenSource.Cancel();
            Stop.IsEnabled = true;
        }
    }
}
