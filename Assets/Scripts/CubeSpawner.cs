using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab; // Cube.prefab��Inspector�ŃA�T�C�����܂�
    public int numberOfCubes = 5; // ��������L���[�u�̐�
    public float spacing = 2f; // �L���[�u�Ԃ̊Ԋu

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
            // �L���[�u�𐶐�
            GameObject cube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);

            // ���̃L���[�u�̈ʒu��ݒ�iX�������Ɉړ��j
            spawnPosition.x += spacing;
        }
    }
}