using Python.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScatterPlotManager : MonoBehaviour
{
    public GameObject plotPrefab;

    void Start()
    {
        Debug.Log("---------- ScatterPlotManager�J�n ----------");

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
            string scriptPath = Path.Combine(Application.dataPath, "StreamingAssets", "myproject", "clustering.py");
            Debug.Log("Script Path: " + scriptPath);

            // �X�N���v�g�𓮓I�ɃC���|�[�g
            using (PyModule sys = (PyModule)Py.Import("sys"))
            {
                sys.Get("path").InvokeMethod("append", new PyString(Path.GetDirectoryName(scriptPath)));
            }

            using (PyModule clusterModule = (PyModule)Py.Import("clustering"))
            {
                // CSV�f�[�^��Python���X�g�ɕϊ�
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

                    // �N���X�^���͂����s
                    using (PyObject resultPy = clusterModule.InvokeMethod("analyze_data", dataList))
                    {
                        Debug.Log("Python result: " + resultPy.ToString());

                        // ���ʂ̎擾
                        var order = ConvertToIntList(resultPy["order"]);
                        var features = ConvertToStringList(resultPy["features"]);
                        var featureTriplets = ConvertToStringTripletList(resultPy["feature_triplets"]);

                        Debug.Log("Order count: " + order.Count);
                        Debug.Log("First 5 elements of order: " + string.Join(", ", order.Take(5)));

                        Debug.Log("Feature triplets count: " + featureTriplets.Count);
                        Debug.Log("First 5 elements of feature triplets: " + string.Join(", ", featureTriplets.Take(5).Select(t => $"({t.Item1}, {t.Item2}, {t.Item3})")));

                        // �U�z�}�̕`��
                        DrawScatterPlots(order, featureTriplets);
                    }


                }
            }
        }

        // Python�G���W���̏I��
        PythonEngine.Shutdown();
        Debug.Log("---------- ScatterPlotManager�I�� ----------");
    }

    private List<int> ConvertToIntList(PyObject pyOrder)
    {
        List<int> order = new List<int>();

        // PyList�Ƃ���pyOrder�����b�v
        using (PyList list = new PyList(pyOrder))
        {
            // �e�v�f�𐮐��ɕϊ����ă��X�g�ɒǉ�
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
        // PyList�Ƃ���pyFeatureTriplets�����b�v
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
        float spacing = 2.0f; // �L���[�u�Ԃ̊Ԋu

        for (int i = 0; i < order.Count; i++)
        {
            int index = order[i];

            // �L���[�u�𐶐�
            Vector3 position = new Vector3(i * spacing, 0, 0);
            GameObject cube = Instantiate(plotPrefab, position, Quaternion.identity);

            if (cube == null)
            {
                Debug.LogError($"Failed to instantiate cube at index {i}");
                continue;
            }

            // �L���[�u�ɖ��̂�ݒ�
            cube.name = $"Cube {index}";

            // �e�L�X�g�I�u�W�F�N�g�𐶐�
            GameObject textObject = new GameObject($"Text {index}");
            textObject.transform.SetParent(cube.transform);
            textObject.transform.localPosition = new Vector3(0, 1.5f, 0); // �L���[�u�̏�ɔz�u

            // TextMeshPro�R���|�[�l���g��ǉ�
            TextMeshPro textMesh = textObject.AddComponent<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = index.ToString();
                //textMesh.text = $"{index}\n{features.Item1}\n{features.Item2}\n{features.Item3}";
                textMesh.fontSize = 20; // 3D��Ԃł̃T�C�Y�𒲐�
                textMesh.color = Color.black;
                textMesh.alignment = TextAlignmentOptions.Center;
                textMesh.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // �e�L�X�g�̃X�P�[���𒲐�
            }
            else
            {
                Debug.LogError($"Failed to add TextMeshPro component to text object for cube {index}");
            }

            // �e�L�X�g�����₷�����邽�߂ɁA�e�L�X�g�I�u�W�F�N�g���J�����̕����Ɍ�����
            textObject.transform.LookAt(Camera.main.transform);
            textObject.transform.Rotate(0, 180, 0); // �e�L�X�g�����]����̂�h��

            /*
            var featureTriplet = featureTriplets[index];
            Debug.Log($"Drawing scatter plot for: {featureTriplet.Item1} vs {featureTriplet.Item2}");

            // ������3D�I�u�W�F�N�g�Ƃ��ĎU�z�}��`�悷�鏈����ǉ����܂��B
            */
        }


    }
}