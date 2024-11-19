using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab; // Cube.prefabをInspectorでアサインします
    public int numberOfCubes = 5; // 生成するキューブの数
    public float spacing = 2f; // キューブ間の間隔

    void Start()
    {
        SpawnCubes();
    }

    void SpawnCubes()
    {
        if (cubePrefab == null)
        {
            Debug.LogError("Cube prefab is not assigned!");
            return;
        }

        Vector3 spawnPosition = transform.position;

        for (int i = 0; i < numberOfCubes; i++)
        {
            // キューブを生成
            GameObject cube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);

            // 次のキューブの位置を設定（X軸方向に移動）
            spawnPosition.x += spacing;
        }
    }
}