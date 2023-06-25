using System;
using System.IO;
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
using totalComander;
using System.Diagnostics;
using System.Numerics;

namespace totalComander
{
    public enum sortType
    {
        name,
        size
    }
    public partial class fileListView : UserControl
    {
        List<file> files;
        List<string> drives;
        bool isSelected = false;

        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }
        public string path { get; set; }


        public fileListView()
        {
            InitializeComponent();
            
            contentView.ItemsSource = files;
            
            drives = new List<string>();
            foreach(DriveInfo drive in DriveInfo.GetDrives())
            {
                drives.Add(drive.Name);
            }
            drivesList.ItemsSource = drives;
            drivesList.SelectedIndex = 0;
        }
        public void setPath(string path_, sortType st = sortType.name)
        {
            if (!Directory.Exists(path_))
                return;
            path = path_;

            string drive = Path.GetPathRoot(path);
            drivesList.SelectedIndex = drives.FindIndex(x => x == drive);

            files = new List<file>();
            files.AddRange(fileGenerator.getDirsFromPath(path) ?? new List<file>());
            List<file> filesT = fileGenerator.getFilesFromPath(path) ?? new List<file>();

            if (filesT.Count > 0)
            {
                switch (st)
                {
                    case sortType.name:
                        {
                            filesT.Sort(delegate (file x, file y)
                            {
                                return x.name.CompareTo(y.name);
                            });
                            break;
                        }
                    case sortType.size:
                        {
                            filesT.Sort(delegate (file x, file y)
                            {
                                return long.Parse(x.size).CompareTo(long.Parse(y.size));
                            });
                            break;
                        }
                }
                files.AddRange(filesT);
            }

            setItemSource(files);
            pathLabel.Content = path.Remove(0, 3);
            try
            {
                Decorator border = VisualTreeHelper.GetChild(contentView, 0) as Decorator;
                ScrollViewer scroll = border.Child as ScrollViewer;
                scroll.ScrollToTop();
            }
            catch (Exception) { }
        }

        private void doubleClick(object sender, MouseButtonEventArgs e)
        {
            file item = ((FrameworkElement)e.OriginalSource).DataContext as file;
            if (item == null)
                return;
            if (item.fileType == fileType.directory)
            {
                setPath(Path.Combine(path, item.name));
            }
            else if (item.fileType == fileType.back)
            {
                if (Directory.GetParent(path) != null)
                    setPath(Directory.GetParent(path).FullName);
            }
            else if (item.fileType == fileType.file)
            {
                Process.Start(new ProcessStartInfo(Path.Combine(path, item.name)) { UseShellExecute = true });
            }
        }
        private void setItemSource(List <file> files_)
        {
            files_.Insert(0, new file("[..]back", DateTime.Now, "", fileType.back));
            contentView.ItemsSource = files_;
        }

        private void drivesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            setPath(text);
        }
        public void setAsActive()
        {
            activeIndicator.Visibility = Visibility.Visible;
            isSelected = true;
        }
        public void deactivate()
        {
            activeIndicator.Visibility = Visibility.Hidden;
            isSelected = false;
        }
        public void createFolderView()
        {
            if (isSelected)
            {
                createFolderWindow cw = new createFolderWindow();
                cw.CreateFolder += createFolder;
                cw.ShowDialog();
            }
        }
        public void removeItemsProcedure()
        {
            if (isSelected && contentView.SelectedItems.Count > 0)
            {
                if (MessageBox.Show($"Are you sure, you want to remove {contentView.SelectedItems.Count} elements?",
                    "Remove file",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var items = contentView.SelectedItems;
                    foreach (file item in items)
                    {
                        try
                        {
                            item.remove(path);
                            files.Remove(item);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "delete fail", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    setPath(path);
                }
            }
        }
        
        void createFolder(object sender, createFolderEventArgs e)
        {
            try
            {
                if (Directory.Exists(Path.Combine(path, e.folderName)))
                {
                    throw new Exception($"folder {e.folderName} already exist");
                }
                Directory.CreateDirectory(Path.Combine(path, e.folderName));
                setPath(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Create folder failed");
            }
        }

        private void headerClick(object sender, RoutedEventArgs e)
        {
            string column = ((GridViewColumnHeader)e.OriginalSource).Column.Header.ToString();
            switch (column)
            {
                case "Nazwa":
                    {
                        setPath(path, sortType.name);
                        break;
                    }
                case "Roz":
                    {
                        setPath(path, sortType.size);
                        break;
                    }
            }    
        }

        bool isDragDrop = false;
        private void ListViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is ListViewItem listViewItem)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    isDragDrop = true;
                    //file data = listViewItem.DataContext as file;
                    //string sourceFile = Path.Combine(path, data.name);
                    //DragDrop.DoDragDrop(this, sourceFile, DragDropEffects.Copy);
                }
            }
        }
        private void ListViewItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is ListViewItem listViewItem)
            {
                if (isDragDrop)
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        try
                        {
                            file data = listViewItem.DataContext as file;
                            if (data == null) return;
                            string sourceFile = Path.Combine(path, data.name);
                            DragDrop.DoDragDrop(this, sourceFile, DragDropEffects.Copy);
                            isDragDrop = false;
                        }
                        catch { }
                        
                    }
                    else
                    {
                        isDragDrop = false;
                    }
                }
            }
        }

        private void contentView_Drop(object sender, DragEventArgs e)
        {
            string sourcePath = (string)e.Data.GetData(DataFormats.Text);

            CopyFilesRecursively(sourcePath, path);
            setPath(path);
        }
        public static void CopyFilesRecursively(string source, string target)
        {
            string checkIfExist;
            int lenOfSource = Path.GetDirectoryName(source).Length;
            
            FileAttributes attr = File.GetAttributes(source);

            if (attr.HasFlag(FileAttributes.Directory))
            {
                checkIfExist = Path.Combine(target, source.Remove(0, lenOfSource));
                if (Directory.Exists(checkIfExist))
                    return;

                //MessageBox.Show("Its a directory");
                Directory.CreateDirectory(Path.Combine(target,Path.GetFileName(source)));

                string[] files = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
                string[] directories = Directory.GetDirectories(source, "*", SearchOption.AllDirectories);

                //int lenOfSource = Path.GetDirectoryName(source).Length;

                int i = 0;
                foreach (string item in directories)
                {
                    directories[i] = item.Remove(0, lenOfSource);
                    i++;
                }

                foreach (string dir in directories)
                {
                    Directory.CreateDirectory(Path.Combine(target, dir));
                }
                foreach (string file in files)
                {
                    //File.Copy(file, Path.Combine(target, Path.GetDirectoryName(file).Remove(0, lenOfSource)));
                    File.Copy(file, Path.Combine(target, file.Remove(0, lenOfSource)));
                }
            }
            else
            {
                //Path.
                //MessageBox.Show("Its a file");
                //int lenOfSource = Path.GetDirectoryName(source).Length;
                checkIfExist = Path.Combine(target, source.Remove(0, lenOfSource));
                if (File.Exists(checkIfExist))
                    return;
                File.Copy(source, Path.Combine(target, source.Remove(0, lenOfSource)));
            }

            //foreach (DirectoryInfo dir in source.GetDirectories())
            //    CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            //foreach (FileInfo file in source.GetFiles())
            //    file.CopyTo(Path.Combine(target.FullName, file.Name));
        }

        
    }

    public class createFolderEventArgs : EventArgs
    {
        public string folderName { get; set; }
        public createFolderEventArgs(string folderName_)
        {
            this.folderName = folderName_;
        }
    }

    internal static class fileGenerator
    {
        internal static List<file> getFilesFromPath(string path)
        {
            string[] filesDir;
            try
            {
                filesDir = Directory.GetFiles(path);
            }
            catch (Exception ex)
            {
                return null;
            }
            List<file> files = new List<file>();
            foreach(string file in filesDir)
            {
                FileInfo fi = new FileInfo(file);
                files.Add(new file(fi.Name, fi.CreationTime, fi.Length.ToString(), fileType.file));
            }
            return files;
        }
        internal static List<file> getDirsFromPath(string path)
        {
            string[] dirs;
            try
            {
                dirs = Directory.GetDirectories(path);
            }
            catch(Exception ex)
            {
                return null;
            }
            List<file> files = new List<file>();
            foreach (string dir in dirs)
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                files.Add(new file(di.Name, di.CreationTime, "", fileType.directory));
            }
            return files;
        }
    }
}
