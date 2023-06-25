using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace totalComander
{
    internal enum fileType
    {
        file,
        directory,
        back
    }
    class file
    {
        public string name { get; set; }
        public DateTime time { get; set; }
        public string size { get; set; }
        public fileType fileType { get; set; }

        internal file(string _name, DateTime _time, string _size, fileType ft)
        {
            this.name = _name;
            this.time = _time;
            this.size = _size;
            this.fileType = ft;
            if (ft == fileType.directory)
                size = "<DIR>";
        }
        internal int remove(string path)
        {
            if (fileType == fileType.file)
            {
                try
                {
                    File.Delete(Path.Combine(path, name));
                    return 0;
                }
                catch (IOException e)
                {
                    throw new Exception($"{name} file is used by different application");
                }
                catch
                {
                    throw new Exception($"unable to delete file {name}");
                }
            }
            else if (fileType == fileType.directory)
            {
                try
                {
                    Directory.Delete(Path.Combine(path, name), true);
                    return 0;
                }
                catch (IOException e)
                {
                    throw e;
                }
                catch
                {
                    throw new Exception($"unable to delete directory {name}");
                }
            }
            return 0;
        }
    }
}
