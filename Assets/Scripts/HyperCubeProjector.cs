using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HyperCubeProjector : MonoBehaviour
{
    public float k = 16f;
    public Vector4 cameraRay = new Vector4(1f, 0f, 0f, 1f);
    public Vector4 cubePosition = new Vector4(4f, 0f, 0f, 0f);

    // 4次元立方体の頂点
    private Vector4[] v = new Vector4[]
    {
    new Vector4(+1f, +1f, +1f, +1f),
    new Vector4(+1f, +1f, +1f, -1f),
    new Vector4(+1f, +1f, -1f, +1f),
    new Vector4(+1f, +1f, -1f, -1f),
    new Vector4(+1f, -1f, +1f, +1f),
    new Vector4(+1f, -1f, +1f, -1f),
    new Vector4(+1f, -1f, -1f, +1f),
    new Vector4(+1f, -1f, -1f, -1f),
    new Vector4(-1f, +1f, +1f, +1f),
    new Vector4(-1f, +1f, +1f, -1f),
    new Vector4(-1f, +1f, -1f, +1f),
    new Vector4(-1f, +1f, -1f, -1f),
    new Vector4(-1f, -1f, +1f, +1f),
    new Vector4(-1f, -1f, +1f, -1f),
    new Vector4(-1f, -1f, -1f, +1f),
    new Vector4(-1f, -1f, -1f, -1f)
    };

    void OnGUI()
    {
        // 4次元立方体を画面に出す
        Vector3[] vertices = Projection(this.k, this.cameraRay, this.v, this.cubePosition);
        DrawHyperCube(vertices);
    }

    private void DrawLine(Vector3 p1, Vector3 p2, Color color)
    {
        Debug.DrawLine(this.transform.position + p1, this.transform.position + p2, color);
    }

    private void DrawHyperCube(Vector3[] vertices)
    {
        Color colorX = Color.red;
        Color colorY = Color.green;
        Color colorZ = Color.blue;
        Color colorW = Color.yellow;

        // X軸に平行な辺
        DrawLine(vertices[0], vertices[8], colorX);
        DrawLine(vertices[1], vertices[9], colorX);
        DrawLine(vertices[2], vertices[10], colorX);
        DrawLine(vertices[3], vertices[11], colorX);
        DrawLine(vertices[4], vertices[12], colorX);
        DrawLine(vertices[5], vertices[13], colorX);
        DrawLine(vertices[6], vertices[14], colorX);
        DrawLine(vertices[7], vertices[15], colorX);

        // Y軸に平行な辺
        DrawLine(vertices[0], vertices[4], colorY);
        DrawLine(vertices[1], vertices[5], colorY);
        DrawLine(vertices[2], vertices[6], colorY);
        DrawLine(vertices[3], vertices[7], colorY);
        DrawLine(vertices[8], vertices[12], colorY);
        DrawLine(vertices[9], vertices[13], colorY);
        DrawLine(vertices[10], vertices[14], colorY);
        DrawLine(vertices[11], vertices[15], colorY);

        // Z軸に平行な辺
        DrawLine(vertices[0], vertices[2], colorZ);
        DrawLine(vertices[1], vertices[3], colorZ);
        DrawLine(vertices[4], vertices[6], colorZ);
        DrawLine(vertices[5], vertices[7], colorZ);
        DrawLine(vertices[8], vertices[10], colorZ);
        DrawLine(vertices[9], vertices[11], colorZ);
        DrawLine(vertices[12], vertices[14], colorZ);
        DrawLine(vertices[13], vertices[15], colorZ);

        // W軸に平行な辺
        DrawLine(vertices[0], vertices[1], colorW);
        DrawLine(vertices[2], vertices[3], colorW);
        DrawLine(vertices[4], vertices[5], colorW);
        DrawLine(vertices[6], vertices[7], colorW);
        DrawLine(vertices[8], vertices[9], colorW);
        DrawLine(vertices[10], vertices[11], colorW);
        DrawLine(vertices[12], vertices[13], colorW);
        DrawLine(vertices[14], vertices[15], colorW);
    }

    // 4次元立方体の3次元空間への写像を作る
    // 4次元立方体の3次元空間への写像を作る
    private static Vector3[] Projection(float k, Vector4 n, Vector4[] v, Vector4 cubePos)
    {
        // X-W成分を回転させる行列
        Matrix4x4 rot = Matrix4x4.identity;
        rot.m00 = +Mathf.Cos(Time.time);
        rot.m03 = -Mathf.Sin(Time.time);
        rot.m30 = +Mathf.Sin(Time.time);
        rot.m33 = +Mathf.Cos(Time.time);

        Vector3[] p = new Vector3[v.Length]; // 3次元空間とレイの交点

        for (int i = 0; i < v.Length; i++)
        {
            Vector4 ray = rot * v[i] + cubePos;

            float L = k * n.sqrMagnitude / Vector4.Dot(n, ray);
            p[i] = L * ray;
        }

        return p;
    }


}
