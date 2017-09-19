using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AipSystem
{
    class ImageProc
    {
        // データ名
        string fileName;
        // T:時間総数　 Z:Z軸総数
        int T, Z;

        Mat[][] Result;

        //分裂候補情報を全て保存するリスト
        List<CellState> Candidates = new List<CellState>();

        public ImageProc(string[] ary)
        {
            fileName = ary[0];
            T = int.Parse(ary[1]);
            Z = int.Parse(ary[2]);
            Result = new Mat[Z][];
            for (int z = 0; z < Z; z++)
            {
                Result[z] = new Mat[T];
            }
        }

        public void Exec()
        {
            //分裂候補を探すための配列
            List<int> count_x_1 = new List<int>();
            List<int> count_y_1 = new List<int>();

            List<CellState> countList_1 = new List<CellState>();

            for (int z = 0; z < Z; z++)
            {
                for (int t = 0; t < T; t++)
                {
                    string name = $"{fileName}/t{t + 1:000}/{fileName}_t{t + 1:000}_page_{z + 1:0000}.tif";
                    Result[z][t] = Cv2.ImRead(name, ImreadModes.Color);
                    Mat tmp = Cv2.ImRead(name, 0);
                    Mat bin = new Mat();
                    Cv2.Threshold(tmp, bin, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);
                    Detect(bin, ref countList_1, t, z);
                }
            }
        }

        public void Detect(Mat bin, ref List<CellState> countList_1, int t, int z)
        {

            List<CellState> countList = new List<CellState>();

            // ラベリング
            Mat status = new Mat();
            Mat center = new Mat();
            Mat labelTmp = new Mat();
            int nLabels = Cv2.ConnectedComponentsWithStats(bin, labelTmp, status, center, PixelConnectivity.Connectivity8, MatType.CV_32SC1);
            List<Scalar> colors = new List<Scalar>();

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

                //ラべリング情報による閾値設定(閾値以内なら分裂候補、以外ならノイズ)
                if (CheckInRange(width, height, area, occupancy, aspect))
                {
                    //現在フレームにおける分裂候補の情報を保存
                    countList.Add(new CellState(x, y, z, width, height, t));
                    //分裂候補なので白で描画
                    colors.Add(Scalar.White);
                }
                else
                {
                    //分裂候補でないので黒で描画（ノイズ）
                    colors.Add(Scalar.Black);
                }
            }

            int count = countList.Count;
            int count_1 = countList_1.Count;
            for (int c = 0; c < count; c++)
            {
                for (int v = 0; v < count_1; v++)
                {
                    //候補座標同士の差が絶対値10px以下ならば分裂候補として再確定
                    if (Math.Abs(countList[c].x - countList_1[v].x) <= 10 && Math.Abs(countList[c].y - countList_1[v].y) <= 10 && countList[c].y != 0)
                    {
                        Console.WriteLine("時間：{0}、Z軸：{1}、(x,y)=({2},{3})", countList[count - 1].t, countList[count - 1].z, countList[c].x, countList[c].y);
                        //分裂候補の情報を保存する
                        Candidates.Add(new CellState(countList[c].x, countList[c].y, countList[c].z, countList[c].w, countList[c].h, countList[c].t));
                        Result[z][t].Rectangle(new Rect(countList[c].x, countList[c].y, countList[c].w, countList[c].h), new Scalar(0, 0, 255), 1);
                    }
                }
            }

            //1フレームごとに適用させるために配列の初期化を行う
            //過去候補配列を初期化
            countList_1.Clear();

            //過去候補配列に現在フレームの分裂候補結果を保存
            countList_1 = countList;
        }

        // 閾値以内か確認
        bool CheckInRange(int width, int height, int area, double occupancy, double aspect)
        {
            int tmp = 0;
            if (200 <= area && area <= 400) tmp++;  //面積
            if (0.75 <= aspect && aspect <= 1.25) tmp++; //縦横比
            //if (10 <= height && height <= 50 && 10 <= width && width <= 50) tmp++; //縦幅横幅
            if (0.1 <= occupancy && occupancy <= 0.8) tmp++; //占有率

            if (tmp == 3) return true;
            return false;
        }

        public void ShowImage()
        {
            Cv2.NamedWindow("結果", WindowMode.AutoSize);
            for(int z = 0; z < Z; z++)
            {
                for(int t = 0; t < T; t++)
                {
                    Cv2.ImShow("結果", Result[z][t]);
                    Cv2.WaitKey(50);
                }
            }
            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();
        }

    }
}
