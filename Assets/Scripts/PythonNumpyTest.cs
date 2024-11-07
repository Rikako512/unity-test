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
        // Pythonエンジンの初期化
        PythonEngine.Initialize();

        using (Py.GIL())
        {
            // Pythonスクリプトのパスを設定
            string scriptPath = Path.Combine(Application.dataPath, "StreamingAssets", "myproject", "test.py");
            Debug.Log("Script Path: " + scriptPath);

            // スクリプトを動的にインポート
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

                // テストデータ
                float[] array1 = { 1.0f, 2.0f, 3.0f };
                float[] array2 = { 4.0f, 5.0f, 6.0f };

                // NumPyの関数を呼び出し
                using (PyObject resultAddPy = npTest.InvokeMethod("add_arrays", new PyObject[] { array1.ToPython(), array2.ToPython() }))
                using (PyObject resultMultiplyPy = npTest.InvokeMethod("multiply_arrays", new PyObject[] { array1.ToPython(), array2.ToPython() }))
                {
                    // 結果をC#の配列に変換
                    float[] resultAdd = resultAddPy.As<float[]>();
                    float[] resultMultiply = resultMultiplyPy.As<float[]>();

                    // 結果を表示
                    Debug.Log("Addition Result: " + string.Join(", ", resultAdd));
                    Debug.Log("Multiplication Result: " + string.Join(", ", resultMultiply));
                }
            }
        }

        // Pythonエンジンの終了
        PythonEngine.Shutdown();
    }
}