using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab4_wpf
{
    public class SelectInfo
    {
        public int FileNumber { get; set; }
        public int IndexInFile { get; set; }
        public SelectInfo(int fileNumber, int indexInFile)
        {
            FileNumber = fileNumber;
            IndexInFile = indexInFile;
        }

        public static SelectInfo FindInfo(List<SelectInfo> infos, int fileNumber)
        {
            foreach(SelectInfo info in infos)
            {
                if(info.FileNumber == fileNumber)
                {
                    return info;
                }
            }

            return null;
        }
    }
}
