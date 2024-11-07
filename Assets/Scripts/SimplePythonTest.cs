using UnityEngine;
using Python.Runtime;
using System.IO;
using System.Collections.Generic;

public class SimplePythonTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("---------- SimplePythonTest �J�n ----------");
        // CSVData �X�N���v�g���f�[�^��ǂݍ��ނ܂őҋ@
        StartCoroutine(WaitForCSVData());
    }

    System.Collections.IEnumerator WaitForCSVData()
    {
        // CSVData.pointList �� null �łȂ��Ȃ�܂őҋ@
        while (CSVData.pointList == null)
        {
            yield return null;
        }

        // �f�[�^���ǂݍ��܂ꂽ��Python�������J�n
        ProcessDataWithPython();
    }

    void ProcessDataWithPython()
    {
        PythonEngine.Initialize();

        using (Py.GIL())
        {
            try
            {
                string scriptPath = Path.Combine(Application.dataPath, "StreamingAssets", "myproject", "simple_add.py");

                using (PyModule sys = (PyModule)Py.Import("sys"))
                {
                    sys.Get("path").InvokeMethod("append", Path.GetDirectoryName(scriptPath).ToPython());
                }

                using (PyModule simpleAdd = (PyModule)Py.Import("simple_add"))
                {
                    if (simpleAdd == null)
                    {
                        Debug.LogError("Failed to import simple_add.py");
                        return;
                    }

                    // CSVData.pointList ����f�[�^���擾���APython �ɓn��
                    using (PyList dataList = new PyList())
                    {
                        foreach (var row in CSVData.pointList)
                        {
                            float sepalLength = System.Convert.ToSingle(row["Sepal.Length"]);
                            float sepalWidth = System.Convert.ToSingle(row["Sepal.Width"]);
                            float petalLength = System.Convert.ToSingle(row["Petal.Length"]);
                            string species = row["Species"].ToString();

                            using (PyTuple pyRow = new PyTuple(new PyObject[] {
                            new PyFloat(sepalLength),
                            new PyFloat(sepalWidth),
                            new PyFloat(petalLength),
                            new PyString(species)
                        }))
                            {
                                dataList.Append(pyRow);
                            }
                        }

                        if (dataList.Length() == 0)
                        {
                            Debug.LogError("No valid data to process");
                            return;
                        }

                        using (PyObject results = simpleAdd.InvokeMethod("sum_iris_features", dataList))
                        {
                            long length = results.Length();
                            for (long i = 0; i < length; i++)
                            {
                                using (PyObject item = results[i.ToPython()])
                                {
                                    float sum = item[0].As<float>();
                                    string species = item[1].ToString();
                                    Debug.Log($"Row {i + 1}: Sum of features: {sum:F2}, Species: {species}");
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in Python processing: {e.Message}");
            }
        }

        PythonEngine.Shutdown();
        Debug.Log("---------- SimplePythonTest �I�� ----------");

    }
}