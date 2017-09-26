using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AipSystem
{
    class FileUtil
    {
        // データ名
        string fileName;
        // T:時間総数　 Z:Z軸総数
        int T, Z;

        public static void init()
        {
            File.Delete(@"output.csv");
            using (FileStream hStream = File.Create(@"output.csv"))
            {
                // 作成時に返される FileStream を利用して閉じる
                if (hStream != null)
                {
                    hStream.Close();
                }
            }
        }

        public string[] ReadInputCSV()
        {
            string line;
            int index = 0;
            string[] ary = new string[3];
            using (StreamReader file = new StreamReader(@"input.csv"))
            {
                while ((line = file.ReadLine()) != null)
                {
                    ary[index++] = line;
                    Console.WriteLine(line);
                    if (index == 3) break;
                }
            }
            fileName = ary[0];
            if (!int.TryParse(ary[1], out T))
            {
                Console.WriteLine("時間総数の取得に失敗しました");
            }

            if (!int.TryParse(ary[2], out Z))
            {
                Console.WriteLine("Z軸総数の取得に失敗しました");
            }

            using (StreamWriter sw = new StreamWriter(@"output.csv", true))
            {
                sw.WriteLine(T);
            }

            Console.WriteLine("データ名:" + fileName);
            Console.WriteLine("時間:" + T);
            Console.WriteLine("Z軸:" + Z);
            return ary;
        }

        public static List<string> getImageList(string path)
        {
            List<string> list = new List<string>();
            string[] files = System.IO.Directory.GetFiles(path);
            Regex regex = new Regex("^[a-zA-Z0-9].*\\.(jpg|bmp)$", RegexOptions.Compiled);
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Remove(0, 7);
                if (regex.IsMatch(files[i]))
                {
                    list.Add(files[i]);
                }
            }
            return list;
        }

        public static void OutputCSV(List<CellState> outputData)
        {
            if (outputData.Count < 20) return;
            using (StreamWriter sw = new StreamWriter(@"output.csv", true))
            {
                sw.WriteLine();
                for(int i=outputData.Count-1; i >= 0; i--)
                {
                    sw.WriteLine(outputData[i].x + " " + outputData[i].y + " " + outputData[i].z);
                }
            }
        }

        public static void OutputCSV2(List<List<CellState>> outputData)
        {
            using (StreamWriter sw = new StreamWriter(@"output.csv", true))
            {
                sw.WriteLine(outputData.Count);
                sw.WriteLine();
                int start = 0, last = 0, t = 0;
                outputData.ForEach(item =>
                {
                    int lastIdx = item.Count - 1;
                    sw.WriteLine(item[lastIdx].x + " " + item[lastIdx].y + " " + item[lastIdx].z + " " + item[0].x + " " + item[0].y + " " + item[0].z);
                    for(int i = 0; i < item.Count; i++)
                    {
                        if(i == 0)
                        {
                            t = item[i].t;
                            continue;
                        }
                        if(t != item[i].t)
                        {
                            if(item[start].z < item[last].z)
                            {
                                sw.WriteLine(item[start].x + " " + item[start].y + " " + item[start].z + " " + item[last].x + " " + item[last].y + " " + item[last].z);
                            } else
                            {
                                sw.WriteLine(item[last].x + " " + item[last].y + " " + item[last].z + " " + item[start].x + " " + item[start].y + " " + item[start].z);
                            }
                            
                            t = item[i].t;
                            start = i;
                        } else
                        {
                            last = i;
                        }
                    }
                    start = 0;
                    last = 0;
                    sw.WriteLine();
                });
                
            }
        }
    }
}
