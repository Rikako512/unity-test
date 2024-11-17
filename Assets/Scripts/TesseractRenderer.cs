using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesseractRenderer : MonoBehaviour
{
    public float size = 1f;
    public bool rotate = false;
    public bool showWhiteEdges = true; // ��������\�����邩�ǂ���
    public float rotationSpeed = 30f;
    public Material lineMaterial;
    public Material transparentMaterial;

    private Vector4[] vertices;
    private int[,] edges;
    private LineRenderer[] lineRenderers;

    // 4������]�p�̊p�x
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
            // ��]�ʂ��v�Z
            float deltaAngleXY = rotationSpeed * Time.deltaTime;
            float deltaAngleXZ = rotationSpeed * Time.deltaTime;

            // ��]����
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

    // �����E�̂�16�̒��_���`
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

    // 32�{�̕ӂ��`
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

            // ����̃G�b�W�ɐF��ݒ�
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
                    // showWhiteEdges��true�̏ꍇ�͔��Afalse�̏ꍇ�̓f�t�H���g�̐F�i���Ȃǁj��ݒ�
                    if (showWhiteEdges)
                    {
                        line.startColor = Color.white; // ���F
                        line.endColor = Color.white; // ���F
                    }
                    else
                    {
                        line.startColor = Color.clear; // ��\���i�����j
                        line.endColor = Color.clear; // ��\���i�����j
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

            // MeshFilter��MeshRenderer��ǉ�
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
                           0, 2, 3}; // �l�p�`���̎O�p�`�ɕ���

            mesh.vertices = faceVertices;
            mesh.triangles = triangles;

            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;
        }
    }
    */

    // 4������]
    void RotateTesseract()
    {
        float angle = rotationSpeed * Time.deltaTime * Mathf.Deg2Rad;

        for (int i = 0; i < vertices.Length; i++)
        {
            float x = vertices[i].x;
            float y = vertices[i].y;
            float z = vertices[i].z;
            float w = vertices[i].w;

            // xy���ʂł̉�]
            vertices[i].x = x * Mathf.Cos(angle) - y * Mathf.Sin(angle);
            vertices[i].y = x * Mathf.Sin(angle) + y * Mathf.Cos(angle);

            // zw���ʂł̉�]
            vertices[i].z = z * Mathf.Cos(angle) - w * Mathf.Sin(angle);
            vertices[i].w = z * Mathf.Sin(angle) + w * Mathf.Cos(angle);
        }
    }

    /*
    // 4������]
    void RotateTesseract()
    {
        // ��]����
        Quaternion rotationXY = Quaternion.Euler(0, angleXY * Time.deltaTime, 0); // Y������̉�]
        Quaternion rotationXZ = Quaternion.Euler(angleXZ * Time.deltaTime, 0, 0); // X������̉�]

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = RotateVertex(vertices[i], rotationXY);
            vertices[i] = RotateVertex(vertices[i], rotationXZ);
        }
    }


    Vector4 RotateVertex(Vector4 vertex, Quaternion rotation)
    {
        Vector3 v3Vertex = new Vector3(vertex.x, vertex.y, vertex.z);

        // �N�H�[�^�j�I���ɂ���]
        v3Vertex = rotation * v3Vertex;

        return new Vector4(v3Vertex.x, v3Vertex.y, v3Vertex.z, vertex.w); // W�����͂��̂܂�
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
            GameObject faceObj = transform.GetChild(i).gameObject; // �e�ʂ̃I�u�W�F�N�g���擾
            MeshFilter meshFilter = faceObj.GetComponent<MeshFilter>();

            if (meshFilter != null) // MeshFilter�����݂��邩�m�F
            {
                Mesh mesh = meshFilter.mesh;
                Vector3[] faceVertices = new Vector3[4];

                for (int j = 0; j < faceIndices.GetLength(1); j++)
                {
                    // ��]��̒��_���g�p
                    faceVertices[j] = Project(vertices[faceIndices[i, j]]);
                }

                int[] triangles = {0, 1, 2,
                               0, 2, 3}; // �l�p�`���̎O�p�`�ɕ���

                mesh.vertices = faceVertices;
                mesh.triangles = triangles;

                mesh.RecalculateNormals(); // �@�����Čv�Z
            }
        }
    }
    */

    // 4��������3�����ւ̓��e
    Vector3 Project(Vector4 v)
    {
        float distance = 5f; // ���_����̋���
        return new Vector3(
            v.x / (v.w + distance) * distance,
            v.y / (v.w + distance) * distance,
            v.z / (v.w + distance) * distance
        );
    }

    // ���_�Ԃ̍��ق𐔂���⏕�֐�
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