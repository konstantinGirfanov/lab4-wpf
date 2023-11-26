using System.IO;

namespace lab4_wpf
{
    class FileWriter
    {
        public StreamWriter Writer { get; set; }
        public string FileName { get; set; }
        public FileWriter(string filename)
        {
            FileName = filename;
            Writer = new StreamWriter(filename);
        }
    }
}
