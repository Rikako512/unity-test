using Python.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScatterPlotManager : MonoBehaviour
{
    public GameObject plotPrefab;
    public GameObject PlotContainer; // 多数の散布図を格納する場所
    public bool textDisplay =  true; // テキストオブジェクトの表示
    public float spacing = 2.0f; // 散布図間の間隔
    public float baseHeight = 50.0f; // 基準の高さ

    void Start()
    {
        Debug.Log("---------- ScatterPlotManager開始 ----------");

        // CSVData.pointList が準備できるまで待機
        StartCoroutine(WaitForCSVDataAndAnalyze());
    }

    private System.Collections.IEnumerator WaitForCSVDataAndAnalyze()
    {
        while (CSVData.pointList == null || CSVData.pointList.Count == 0)
        {
            yield return null;
        }

        PerformClusterAnalysis();
    }

    private void PerformClusterAnalysis()
    {
        PythonEngine.Initialize();

        using (Py.GIL())
        {
            // Pythonスクリプトのパスを設定
            string scriptPath = Path.Combine(Application.dataPath, "StreamingAssets", "myproject", "clustering.py");
            Debug.Log("Script Path: " + scriptPath);

            // スクリプトを動的にインポート
            using (PyModule sys = (PyModule)Py.Import("sys"))
            {
                sys.Get("path").InvokeMethod("append", new PyString(Path.GetDirectoryName(scriptPath)));
            }

            using (PyModule clusterModule = (PyModule)Py.Import("clustering"))
            {
                // CSVデータをPythonリストに変換
                using (PyList dataList = new PyList())
                {
                    foreach (var row in CSVData.pointList)
                    {
                        using (PyList pyRow = new PyList())
                        {
                            foreach (var key in row.Keys)
                            {
                                pyRow.Append(new PyFloat(float.Parse(row[key].ToString())));
                            }
                            dataList.Append(pyRow);
                        }
                    }

                    Debug.Log("DataList count: " + dataList.Length());
                    if (dataList.Length() == 0)
                    {
                        Debug.LogError("DataList is empty. Check CSV data.");
                        return;
                    }

                    // クラスタ分析を実行
                    using (PyObject resultPy = clusterModule.InvokeMethod("analyze_data", dataList))
                    {
                        Debug.Log("Python result: " + resultPy.ToString());

                        // 結果の取得
                        var order = ConvertToIntList(resultPy["order"]);
                        var features = ConvertToStringList(resultPy["features"]);
                        var featureTriplets = ConvertToStringTripletList(resultPy["feature_triplets"]);

                        Debug.Log("Order count: " + order.Count);
                        Debug.Log("First 5 elements of order: " + string.Join(", ", order.Take(5)));

                        Debug.Log("Feature triplets count: " + featureTriplets.Count);
                        Debug.Log("First 5 elements of feature triplets: " + string.Join(", ", featureTriplets.Take(5).Select(t => $"({t.Item1}, {t.Item2}, {t.Item3})")));

                        // 散布図の描画
                        DrawScatterPlots(order, featureTriplets);
                    }


                }
            }
        }

        // Pythonエンジンの終了
        PythonEngine.Shutdown();
        Debug.Log("---------- ScatterPlotManager終了 ----------");
    }

    private List<int> ConvertToIntList(PyObject pyOrder)
    {
        List<int> order = new List<int>();

        // PyListとしてpyOrderをラップ
        using (PyList list = new PyList(pyOrder))
        {
            // 各要素を整数に変換してリストに追加
            foreach (PyObject item in list)
            {
                order.Add(item.As<int>());
            }
        }

        return order;
    }

		private List<string> ConvertToStringList(PyObject pyFeatures)
    {
        List<string> features = new List<string>();
        using (PyList list = new PyList(pyFeatures))
        {
            foreach (PyObject item in list)
            {
                features.Add(item.ToString());
            }
        }
        return features;
    }
    
    private List<(string, string, string)> ConvertToStringTripletList(PyObject pyFeatureTriplets)
    {
        List<(string, string, string)> featureTriplets = new List<(string, string, string)>();
        // PyListとしてpyFeatureTripletsをラップ
        using (PyList list = new PyList(pyFeatureTriplets))
        {
            foreach (PyObject triplet in list)
            {
                string feature1 = triplet[0].ToString();
                string feature2 = triplet[1].ToString();
                string feature3 = triplet[2].ToString();
                featureTriplets.Add((feature1, feature2, feature3));
            }
        }

        return featureTriplets;
    }

    private void DrawScatterPlots(List<int> order, List<(string, string, string)> featureTriplets)
    {
        int totalPlots = order.Count;
        int rowCount = Mathf.CeilToInt(Mathf.Sqrt(totalPlots)); // 行数を計算

        for (int i = 0; i < totalPlots; i++)
        {
            int index = order[i];

            // キューブの位置を計算（正方形に近い形に配置、X軸とY軸を使用、、Y軸は上から下に）
            int row = i / rowCount;
            int col = i % rowCount;
            Vector3 position = new Vector3(col * spacing, baseHeight - (row * spacing), 0);
            GameObject plot = Instantiate(plotPrefab, position, Quaternion.identity);

            if (plot == null)
            {
                Debug.LogError($"Failed to instantiate plot at index {i}");
                continue;
            }

            // PlotContainerに格納
            plot.transform.parent = PlotContainer.transform;

            // plotに名称を設定
            string plotName = $"Plot {index}";
            plot.transform.name = plotName;

            var features = featureTriplets[index];
            //Debug.Log((features.Item1).GetType());

            // scatterplotのcolumn1,2,3に格納する
            PointRenderer scatterplotter = plot.transform.Find("GraphFrame/Plotter").gameObject.GetComponent<PointRenderer>();
            scatterplotter.column1 = int.Parse(features.Item1);
            scatterplotter.column2 = int.Parse(features.Item2);
            scatterplotter.column3 = int.Parse(features.Item3);

            if (textDisplay == true)
            {
                // テキストオブジェクトを生成
                GameObject textObject = new GameObject($"Text {index}");
                textObject.transform.SetParent(plot.transform);
                textObject.transform.localPosition = new Vector3(0, 1.5f, 0); // キューブの上に配置

                // TextMeshProコンポーネントを追加
                TextMeshPro textMesh = textObject.AddComponent<TextMeshPro>();
                if (textMesh != null)
                {
                    //textMesh.text = index.ToString();
                    string feature1Name = CSVData.pointList[0].Keys.ElementAt(int.Parse(features.Item1));
                    string feature2Name = CSVData.pointList[0].Keys.ElementAt(int.Parse(features.Item2));
                    string feature3Name = CSVData.pointList[0].Keys.ElementAt(int.Parse(features.Item3));
                
                    textMesh.text = $"{index}\n{feature1Name}\n{feature2Name}\n{feature3Name}";
                    textMesh.fontSize = 20; // 3D空間でのサイズを調整
                    textMesh.color = Color.black;
                    textMesh.alignment = TextAlignmentOptions.Center;
                    textMesh.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // テキストのスケールを調整
                }
                else
                {
                    Debug.LogError($"Failed to add TextMeshPro component to text object for plot {index}");
                }
            }

            Debug.Log(features.Item1 + ", " + features.Item2 + ", " + features.Item3);
        }
    }
}