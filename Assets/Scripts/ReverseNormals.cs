using UnityEngine;

public class ReverseNormals : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] normals = mesh.normals;

        // 例として最初の面の法線を反転
        for (int i = 0; i < normals.Length; i++)
        {
            if (i == 0)
            {
                normals[i] = -normals[i];
            }
        }

        mesh.normals = normals;
    }
}
