using Microsoft.CSharp;
using System.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Python.Runtime;
using System.IO;

public class PythonNumpyTest : MonoBehaviour
{
    void Start()
    {
        // Python�G���W���̏�����
        PythonEngine.Initialize();

        using (Py.GIL())
        {
            // Python�X�N���v�g�̃p�X��ݒ�
            string scriptPath = Path.Combine(Application.dataPath, "StreamingAssets", "myproject", "test.py");
            Debug.Log("Script Path: " + scriptPath);

            // �X�N���v�g�𓮓I�ɃC���|�[�g
            using (PyModule sys = (PyModule)Py.Import("sys"))
            {
                sys.Get("path").InvokeMethod("append", new PyString(Path.GetDirectoryName(scriptPath)));
            }

            using (PyModule npTest = (PyModule)Py.Import("test"))
            {
                if (npTest == null)
                {
                    Debug.LogError("Failed to import test.py");
                    return;
                }

                // �e�X�g�f�[�^
                float[] array1 = { 1.0f, 2.0f, 3.0f };
                float[] array2 = { 4.0f, 5.0f, 6.0f };

                // NumPy�̊֐����Ăяo��
                using (PyObject resultAddPy = npTest.InvokeMethod("add_arrays", new PyObject[] { array1.ToPython(), array2.ToPython() }))
                using (PyObject resultMultiplyPy = npTest.InvokeMethod("multiply_arrays", new PyObject[] { array1.ToPython(), array2.ToPython() }))
                {
                    // ���ʂ�C#�̔z��ɕϊ�
                    float[] resultAdd = resultAddPy.As<float[]>();
                    float[] resultMultiply = resultMultiplyPy.As<float[]>();

                    // ���ʂ�\��
                    Debug.Log("Addition Result: " + string.Join(", ", resultAdd));
                    Debug.Log("Multiplication Result: " + string.Join(", ", resultMultiply));
                }
            }
        }

        // Python�G���W���̏I��
        PythonEngine.Shutdown();
    }
}