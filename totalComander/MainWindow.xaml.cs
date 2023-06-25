using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace totalComander
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            leftList.setPath("C:\\");
            rightList.setPath("D:\\");
        }

        private void selectView(object sender, MouseButtonEventArgs e)
        {
            leftList.deactivate();
            rightList.deactivate();
            fileListView selView = (fileListView)sender;
            selView.setAsActive();
        }
        private void windowLoaded(object sender, RoutedEventArgs e)
        {
            this.KeyDown += HandleKeyPress;
        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F7)
            {
                leftList.createFolderView();
                rightList.createFolderView();
            }
            else if (e.Key == Key.F8)
            {
                leftList.removeItemsProcedure();
                rightList.removeItemsProcedure();
            }
        }
    }
}
