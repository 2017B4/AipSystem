using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AipSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            FileUtil fileUtil = new FileUtil();
            string[] inputData = fileUtil.ReadInputCSV();
            ImageProc imageProc = new ImageProc(inputData);
            imageProc.Exec();
            imageProc.ShowImage();

            //Labeling.ExecLabeling();
        }
    }
}
