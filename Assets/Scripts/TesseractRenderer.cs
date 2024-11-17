using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesseractRenderer : MonoBehaviour
{
    public float size = 1f;
    public bool rotate = false;
    public bool showWhiteEdges = true; // 白い線を表示するかどうか
    public float rotationSpeed = 30f;
    public Material lineMaterial;
    public Material transparentMaterial;

    private Vector4[] vertices;
    private int[,] edges;
    private LineRenderer[] lineRenderers;

    // 4次元回転用の角度
    /*
    public float angleXY = 0f;
    public float angleXZ = 0f;
    public float angleYW = 0f;
    public float angleZW = 0f;
    */

    void Start()
    {
        InitializeVertices();
        InitializeEdges();
        CreateLineRenderers();
        //CreateFaces();
    }

    void Update()
    {
        /*
        if (rotate)
        {
            // 回転量を計算
            float deltaAngleXY = rotationSpeed * Time.deltaTime;
            float deltaAngleXZ = rotationSpeed * Time.deltaTime;

            // 回転処理
            Quaternion rotationXY = Quaternion.Euler(0, deltaAngleXY, 0);
            Quaternion rotationXZ = Quaternion.Euler(deltaAngleXZ, 0, 0);

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = RotateVertex(vertices[i], rotationXY);
                vertices[i] = RotateVertex(vertices[i], rotationXZ);
            }
        }
        */
        if (rotate)
        {
            RotateTesseract();
        }
        UpdateLineRenderers();
        //UpdateFaces();
    }

    // 正八胞体の16個の頂点を定義
    void InitializeVertices()
    {
        vertices = new Vector4[16];
        for (int i = 0; i < 16; i++)
        {
            vertices[i] = new Vector4(
                ((i & 1) * 2 - 1) * size,
                (((i >> 1) & 1) * 2 - 1) * size,
                (((i >> 2) & 1) * 2 - 1) * size,
                (((i >> 3) & 1) * 2 - 1) * size
            );
        }
    }

    // 32本の辺を定義
    void InitializeEdges()
    {
        edges = new int[32, 2];
        int edgeCount = 0;
        for (int i = 0; i < 16; i++)
        {
            for (int j = i + 1; j < 16; j++)
            {
                if (CountDifferences(vertices[i], vertices[j]) == 1)
                {
                    edges[edgeCount, 0] = i;
                    edges[edgeCount, 1] = j;
                    edgeCount++;
                }
            }
        }
    }

    void CreateLineRenderers()
    {
        lineRenderers = new LineRenderer[32];
        for (int i = 0; i < 32; i++)
        {
            GameObject lineObj = new GameObject("Edge " + i);
            lineObj.transform.SetParent(transform);
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = lineMaterial;
            line.startWidth = 0.05f;
            line.endWidth = 0.05f;
            line.positionCount = 2;
            lineRenderers[i] = line;

            // 特定のエッジに色を設定
            switch (i)
            {
                case 0:
                    line.startColor = Color.red;
                    line.endColor = Color.red;
                    break;
                case 1:
                    line.startColor = Color.yellow;
                    line.endColor = Color.yellow;
                    break;
                case 2:
                    line.startColor = Color.green;
                    line.endColor = Color.green;
                    break;
                case 3:
                    line.startColor = Color.blue;
                    line.endColor = Color.blue;
                    break;
                case 4:
                case 7:
                    line.startColor = Color.magenta;
                    line.endColor = Color.magenta;
                    break;
                case 5:
                case 8:
                case 10:
                case 12:
                case 13:
                case 15:
                case 17:
                    line.startColor = Color.black;
                    line.endColor = Color.black;
                    break;
                case 6:
                case 9:
                case 11:
                case 20:
                case 21:
                case 23:
                case 25:
                    line.startColor = new Color(0.3f, 0.3f, 0.3f);
                    line.endColor = new Color(0.3f, 0.3f, 0.3f);
                    break;
                default:
                    // showWhiteEdgesがtrueの場合は白、falseの場合はデフォルトの色（白など）を設定
                    if (showWhiteEdges)
                    {
                        line.startColor = Color.white; // 白色
                        line.endColor = Color.white; // 白色
                    }
                    else
                    {
                        line.startColor = Color.clear; // 非表示（透明）
                        line.endColor = Color.clear; // 非表示（透明）
                    }
                    break;
            }
        }
       }

    /*
    void CreateFaces()
    {
        int[,] faceIndices = new int[,]
        {
        {0, 1, 3, 2}, {0, 1, 5, 4}, {0, 2, 6, 4}, {0, 3, 7, 5},
        {1, 3, 7, 5}, {2, 3, 7, 6}, {4, 5, 7, 6}, {4, 5, 1, 0},
        {6, 7, 3, 2}, {6, 4, 0, 2}, {6, 4, 5, 1}, {7, 5, 1, 3}
        };

        for (int i = 0; i < faceIndices.GetLength(0); i++)
        {
            GameObject faceObj = new GameObject("Face " + i);
            faceObj.transform.SetParent(transform);

            // MeshFilterとMeshRendererを追加
            MeshFilter meshFilter = faceObj.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = faceObj.AddComponent<MeshRenderer>();
            meshRenderer.material = transparentMaterial;

            Mesh mesh = new Mesh();
            Vector3[] faceVertices = new Vector3[4];

            for (int j = 0; j < faceIndices.GetLength(1); j++)
            {
                faceVertices[j] = Project(vertices[faceIndices[i, j]]);
            }

            int[] triangles = {0, 1, 2,
                           0, 2, 3}; // 四角形を二つの三角形に分割

            mesh.vertices = faceVertices;
            mesh.triangles = triangles;

            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;
        }
    }
    */

    // 4次元回転
    void RotateTesseract()
    {
        float angle = rotationSpeed * Time.deltaTime * Mathf.Deg2Rad;

        for (int i = 0; i < vertices.Length; i++)
        {
            float x = vertices[i].x;
            float y = vertices[i].y;
            float z = vertices[i].z;
            float w = vertices[i].w;

            // xy平面での回転
            vertices[i].x = x * Mathf.Cos(angle) - y * Mathf.Sin(angle);
            vertices[i].y = x * Mathf.Sin(angle) + y * Mathf.Cos(angle);

            // zw平面での回転
            vertices[i].z = z * Mathf.Cos(angle) - w * Mathf.Sin(angle);
            vertices[i].w = z * Mathf.Sin(angle) + w * Mathf.Cos(angle);
        }
    }

    /*
    // 4次元回転
    void RotateTesseract()
    {
        // 回転処理
        Quaternion rotationXY = Quaternion.Euler(0, angleXY * Time.deltaTime, 0); // Y軸周りの回転
        Quaternion rotationXZ = Quaternion.Euler(angleXZ * Time.deltaTime, 0, 0); // X軸周りの回転

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = RotateVertex(vertices[i], rotationXY);
            vertices[i] = RotateVertex(vertices[i], rotationXZ);
        }
    }


    Vector4 RotateVertex(Vector4 vertex, Quaternion rotation)
    {
        Vector3 v3Vertex = new Vector3(vertex.x, vertex.y, vertex.z);

        // クォータニオンによる回転
        v3Vertex = rotation * v3Vertex;

        return new Vector4(v3Vertex.x, v3Vertex.y, v3Vertex.z, vertex.w); // W成分はそのまま
    }
    */


    void UpdateLineRenderers()
    {
        for (int i = 0; i < 32; i++)
        {
            Vector3 start = Project(vertices[edges[i, 0]]);
            Vector3 end = Project(vertices[edges[i, 1]]);
            lineRenderers[i].SetPosition(0, start);
            lineRenderers[i].SetPosition(1, end);
        }
    }

    /*
    void UpdateFaces()
    {
        int[,] faceIndices = new int[,]
        {
        {0, 1, 3, 2}, {0, 1, 5, 4}, {0, 2, 6, 4}, {0, 3, 7, 5},
        {1, 3, 7, 5}, {2, 3, 7, 6}, {4, 5, 7, 6}, {4, 5, 1, 0},
        {6, 7, 3, 2}, {6, 4, 0, 2}, {6, 4, 5, 1}, {7, 5, 1, 3}
        };

        for (int i = 0; i < faceIndices.GetLength(0); i++)
        {
            GameObject faceObj = transform.GetChild(i).gameObject; // 各面のオブジェクトを取得
            MeshFilter meshFilter = faceObj.GetComponent<MeshFilter>();

            if (meshFilter != null) // MeshFilterが存在するか確認
            {
                Mesh mesh = meshFilter.mesh;
                Vector3[] faceVertices = new Vector3[4];

                for (int j = 0; j < faceIndices.GetLength(1); j++)
                {
                    // 回転後の頂点を使用
                    faceVertices[j] = Project(vertices[faceIndices[i, j]]);
                }

                int[] triangles = {0, 1, 2,
                               0, 2, 3}; // 四角形を二つの三角形に分割

                mesh.vertices = faceVertices;
                mesh.triangles = triangles;

                mesh.RecalculateNormals(); // 法線を再計算
            }
        }
    }
    */

    // 4次元から3次元への投影
    Vector3 Project(Vector4 v)
    {
        float distance = 5f; // 視点からの距離
        return new Vector3(
            v.x / (v.w + distance) * distance,
            v.y / (v.w + distance) * distance,
            v.z / (v.w + distance) * distance
        );
    }

    // 頂点間の差異を数える補助関数
    int CountDifferences(Vector4 a, Vector4 b)
    {
        int count = 0;
        if (a.x != b.x) count++;
        if (a.y != b.y) count++;
        if (a.z != b.z) count++;
        if (a.w != b.w) count++;
        return count;
    }
}