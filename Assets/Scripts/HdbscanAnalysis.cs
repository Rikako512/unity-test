using UnityEngine;
using Python.Runtime;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class HdbscanAnalysis : MonoBehaviour
{
    // �g�p��������ʂ̗�C���f�b�N�X���w��
    private static readonly int[] featureIndices = new int[] { 1, 2, 3 }; // ��: 2�ԖځA3�ԖځA4�Ԗڂ̗���g�p

    void Start()
    {
        Debug.Log("---------- HdbscanAnalysis �J�n ----------");

        // CSVData.pointList �������ł���܂őҋ@
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
            // Python�X�N���v�g�̃p�X��ݒ�
            string scriptPath = Path.Combine(Application.dataPath, "StreamingAssets", "myproject", "hdbscan_analysis.py");
            Debug.Log("Script Path: " + scriptPath);

            // �X�N���v�g�𓮓I�ɃC���|�[�g
            using (PyModule sys = (PyModule)Py.Import("sys"))
            {
                sys.Get("path").InvokeMethod("append", new PyString(Path.GetDirectoryName(scriptPath)));
            }

            using (PyModule clusterModule = (PyModule)Py.Import("hdbscan_analysis"))
            {

                // CSV�f�[�^��Python���X�g�ɕϊ�
                using (PyList dataList = new PyList())
                {
                    foreach (var row in CSVData.pointList)
                    {
                        using (PyList pyRow = new PyList())
                        {
                            foreach (int index in featureIndices)
                            {
                                string columnName = CSVData.pointList[0].Keys.ElementAt(index);
                                pyRow.Append(new PyFloat(float.Parse(row[columnName].ToString())));
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

                    // �N���X�^���͂����s
                    using (PyObject resultPy = clusterModule.InvokeMethod("perform_hdbscan_analysis", dataList))
                    {
                      DisplayResults(resultPy);
                    }

                }
            }
        }

        // Python�G���W���̏I��
        PythonEngine.Shutdown();
        Debug.Log("---------- HdbscanAnalysis �I�� ----------");
    }

    private List<int> ConvertPyListToIntList(PyObject pyList)
    {
        var result = new List<int>();
        using (PyList list = new PyList(pyList))
        {
            foreach (PyObject item in list)
            {
                result.Add(item.As<int>());
            }
        }
        return result;
    }

    private List<double> ConvertPyListToDoubleList(PyObject pyList)
    {
        var result = new List<double>();
        using (PyList list = new PyList(pyList))
        {
            foreach (PyObject item in list)
            {
                result.Add(item.As<double>());
            }
        }
        return result;
    }

    private List<List<double>> ConvertPyListToListOfLists(PyObject pyList)
    {
        var result = new List<List<double>>();
        using (PyList list = new PyList(pyList))
        {
            foreach (PyObject item in list)
            {
                result.Add(ConvertPyListToDoubleList(item));
            }
        }
        return result;
    }

    private void DisplayResults(PyObject resultPy)
    {
        using (PyDict pyDict = new PyDict(resultPy))
        {
            int numberOfClusters = pyDict["number_of_clusters"].As<int>();
            List<int> clusterSizes = ConvertPyListToIntList(pyDict["cluster_sizes"]);
            List<List<double>> clusterCenters = ConvertPyListToListOfLists(pyDict["cluster_centers"]);
            int noisePoints = pyDict["noise_points"].As<int>();
            List<double> markerSizes = ConvertPyListToDoubleList(pyDict["marker_sizes"]);

            Debug.Log($"�N���X�^��: {numberOfClusters}");
            Debug.Log($"�N���X�^�T�C�Y: {string.Join(", ", clusterSizes)}");
            Debug.Log($"�N���X�^���S�_:\n{string.Join("\n", clusterCenters.Select(center => $"({string.Join(", ", center)})"))}");
            Debug.Log($"�m�C�Y�|�C���g��: {noisePoints}");
            Debug.Log($"�}�[�J�[�T�C�Y: {string.Join(", ", markerSizes)}");
        }
    }


}
