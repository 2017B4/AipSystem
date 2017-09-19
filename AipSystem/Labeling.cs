using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AipSystem
{
    class Labeling
    {
        public static void ExecLabeling()
        {
            string name = $"C:\\Users\\yuuki\\Desktop\\Pre_Data01_t019_page_0011.tif";
            Mat origin = Cv2.ImRead(name, ImreadModes.Color);
            Mat tmp = Cv2.ImRead(name, 0);
            Mat bin = new Mat();
            Cv2.Threshold(tmp, bin, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);

            // ラベリング
            Mat status = new Mat();
            Mat center = new Mat();
            Mat labelTmp = new Mat();
            int nLabels = Cv2.ConnectedComponentsWithStats(bin, labelTmp, status, center, PixelConnectivity.Connectivity8, MatType.CV_32SC1);

            var param = status.GetGenericIndexer<int>();
            for (int label = 0; label < nLabels; label++)
            {
                int x = param[label, 0];
                int y = param[label, 1];
                int width = param[label, 2];
                int height = param[label, 3];
                int area = param[label, 4];
                double occupancy = (double)area / (height * width);
                double aspect = (double)height / width;
                if(CheckInRange(width, height, area, occupancy, aspect))
                {
                    origin.Rectangle(new Rect(x, y, width, height), new Scalar(0, 0, 255), 1);
                }
                
            }
            Cv2.NamedWindow("結果", WindowMode.AutoSize);
            Cv2.ImShow("結果", origin);
            Cv2.WaitKey(50);
            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();
            Cv2.ImWrite(@"C:\\Users\\yuuki\\Desktop\\2.tif", origin);
        }

        // 閾値以内か確認
        static bool CheckInRange(int width, int height, int area, double occupancy, double aspect)
        {
            int tmp = 0;
            if (100 <= area && area <= 400) tmp++;  //面積
            if (0.5 <= aspect && aspect <= 1.5) tmp++; //縦横比
            //if (10 <= height && height <= 50 && 10 <= width && width <= 50) tmp++; //縦幅横幅
            if (0.1 <= occupancy && occupancy <= 0.8) tmp++; //占有率

            if (tmp == 3) return true;
            return false;
        }

    }
}
