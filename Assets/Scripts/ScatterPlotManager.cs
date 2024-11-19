using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Python.Runtime;
using TMPro;

public class ScatterPlotManager : MonoBehaviour
{
    public GameObject plotPrefab;

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
                        // 結果の取得
                        var order = ConvertToIntList(resultPy["order"]);
                        var featurePairs = ConvertToStringTupleList(resultPy["feature_pairs"]);

                        // 散布図の描画
                        DrawScatterPlots(order, featurePairs);
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

    private List<(string, string)> ConvertToStringTupleList(PyObject pyFeaturePairs)
    {
        List<(string, string)> featurePairs = new List<(string, string)>();

        // PyListとしてpyFeaturePairsをラップ
        using (PyList list = new PyList(pyFeaturePairs))
        {
            // 各ペアをタプルとしてリストに追加
            foreach (PyObject pair in list)
            {
                string feature1 = pair[0].ToString();
                string feature2 = pair[1].ToString();
                featurePairs.Add((feature1, feature2));
            }
        }

        return featurePairs;
    }

    private void DrawScatterPlots(List<int> order, List<(string, string)> featurePairs)
    {
        float spacing = 2.0f; // キューブ間の間隔
        Vector3 cubePosition = transform.position;

        for (int i = 0; i < order.Count; i++)
        {
            int index = order[i];

            // キューブを生成
            Vector3 position = new Vector3(i * spacing, 0, 0);
            GameObject cube = Instantiate(plotPrefab, position, Quaternion.identity);

            if (cube == null)
            {
                Debug.LogError($"Failed to instantiate cube at index {i}");
                continue;
            }

            // キューブに名称を設定
            cube.name = $"Cube {index}";

            // テキストオブジェクトを生成
            GameObject textObject = new GameObject($"Text {index}");
            textObject.transform.SetParent(cube.transform);
            textObject.transform.localPosition = new Vector3(0, 1.5f, 0); // キューブの上に配置

            // TextMeshProコンポーネントを追加
            TextMeshPro textMesh = textObject.AddComponent<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = index.ToString();
                textMesh.fontSize = 20; // 3D空間でのサイズを調整
                textMesh.color = Color.black;
                textMesh.alignment = TextAlignmentOptions.Center;
                textMesh.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // テキストのスケールを調整
            }
            else
            {
                Debug.LogError($"Failed to add TextMeshPro component to cube {index}");
            }

            /*
            var featurePair = featurePairs[index];
            Debug.Log($"Drawing scatter plot for: {featurePair.Item1} vs {featurePair.Item2}");

            // ここで3Dオブジェクトとして散布図を描画する処理を追加します。
            */
        }
        
        
    }
}