using UnityEngine;
using Python.Runtime;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class ClusterAnalysis : MonoBehaviour
{
    void Start()
    {
        Debug.Log("---------- ClusterAnalysis 開始 ----------");

        // Pythonエンジンの初期化
        PythonEngine.Initialize();

        using (Py.GIL())
        {
            // Pythonスクリプトのパスを設定
            string scriptPath = Path.Combine(Application.dataPath, "StreamingAssets", "myproject", "cluster_analysis.py");
            Debug.Log("Script Path: " + scriptPath);

            // スクリプトを動的にインポート
            using (PyModule sys = (PyModule)Py.Import("sys"))
            {
                sys.Get("path").InvokeMethod("append", new PyString(Path.GetDirectoryName(scriptPath)));
            }

            using (PyModule clusterModule = (PyModule)Py.Import("cluster_analysis"))
            {
                if (clusterModule == null)
                {
                    Debug.LogError("Failed to import cluster_analysis.py");
                    return;
                }

                // クラスタ分析を実行
                using (PyObject resultPy = clusterModule.InvokeMethod("perform_cluster_analysis"))
                {
                    // 結果をC#のデータ構造に変換
                    var result = new Dictionary<string, object>();
                    using (PyDict pyDict = new PyDict(resultPy))
                    {
                        foreach (PyObject key in pyDict.Keys())
                        {
                            string keyStr = key.ToString();
                            PyObject value = pyDict[key];

                            if (keyStr == "number_of_clusters")
                            {
                                result[keyStr] = value.As<int>();
                            }
                            else if (keyStr == "cluster_centers")
                            {
                                result[keyStr] = ConvertPyListToList(value);
                            }
                            else if (keyStr == "cluster_sizes" || keyStr == "marker_sizes")
                            {
                                result[keyStr] = ConvertPyListToDoubleList(value);
                            }
                        }
                    }

                    // 結果を表示
                    DisplayResults(result);
                }
            }
        }

        // Pythonエンジンの終了
        PythonEngine.Shutdown();
        Debug.Log("---------- ClusterAnalysis 終了 ----------");
    }

    private List<string> ConvertPyListToList(PyObject pyList)
    {
        var result = new List<string>();
        using (PyList list = new PyList(pyList))
        {
            foreach (PyObject item in list)
            {
                if (item.GetPythonType().Name == "list")
                {
                    result.Add($"({string.Join(", ", ConvertPyListToDoubleList(item))})");
                }
                else
                {
                    result.Add(item.ToString());
                }
            }
        }
        return result;
    }

    private List<double> ConvertPyListToDoubleList(PyObject pyList)
    {
        var result = new List<double>();
        if (pyList.GetPythonType().Name == "list")
        {
            using (PyList list = new PyList(pyList))
            {
                foreach (PyObject item in list)
                {
                    if (double.TryParse(item.ToString(), out double value))
                    {
                        result.Add(value);
                    }
                }
            }
        }
        else if (double.TryParse(pyList.ToString(), out double value))
        {
            result.Add(value);
        }
        return result;
    }

    private void DisplayResults(Dictionary<string, object> result)
    {
        if (result.ContainsKey("number_of_clusters"))
        {
            Debug.Log($"Number of clusters: {result["number_of_clusters"]}");
        }
        else if (result.ContainsKey("cluster_sizes"))
        {
            int numberOfClusters = ((List<double>)result["cluster_sizes"]).Count;
            Debug.Log($"Number of clusters (derived from cluster_sizes): {numberOfClusters}");
        }
        else
        {
            Debug.LogWarning("Unable to determine the number of clusters. Neither 'number_of_clusters' nor 'cluster_sizes' found in the result dictionary.");
        }

        if (result.ContainsKey("cluster_centers"))
        {
            Debug.Log($"Cluster centers: {string.Join(", ", (List<string>)result["cluster_centers"])}");
        }
        else
        {
            Debug.LogWarning("'cluster_centers' key not found in the result dictionary.");
        }

        if (result.ContainsKey("cluster_sizes"))
        {
            Debug.Log($"Cluster sizes: {string.Join(", ", (List<double>)result["cluster_sizes"])}");
        }
        else
        {
            Debug.LogWarning("'cluster_sizes' key not found in the result dictionary.");
        }

        if (result.ContainsKey("marker_sizes"))
        {
            Debug.Log($"Marker sizes: {string.Join(", ", (List<double>)result["marker_sizes"])}");
        }
        else
        {
            Debug.LogWarning("'marker_sizes' key not found in the result dictionary.");
        }

        // デバッグ用：すべてのキーを表示
        Debug.Log("All keys in the result dictionary:");
        foreach (var key in result.Keys)
        {
            Debug.Log(key);
        }
    }

}
