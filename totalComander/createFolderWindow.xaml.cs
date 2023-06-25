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
using System.Windows.Shapes;

namespace totalComander
{
    public partial class createFolderWindow : Window
    {
        public event EventHandler<createFolderEventArgs> CreateFolder;

        public createFolderWindow()
        {
            InitializeComponent();
        }

        private void cancleClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void createClicked(object sender, RoutedEventArgs e)
        {
            if (this.CreateFolder != null)
            {
                CreateFolder(this, new createFolderEventArgs(inputBox.Text));
            }
            this.Close();
        }
    }
}
