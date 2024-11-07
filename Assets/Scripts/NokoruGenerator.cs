using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NokoruGenerator : MonoBehaviour
{
    public Material toumeiMaterial; // Inspectorで設定するための変数
    public int cubesPerSide = 3; // 1辺あたりのCube数を設定する変数
    public bool useToumeiMaterial = true; // toumeiマテリアルを使用するかどうかのフラグ
    private HashSet<string> uniqueCombinations = new HashSet<string>();

    void Start()
    {
        GenerateNokoru();
    }

    void GenerateNokoru()
    {
        GameObject nokoru = new GameObject("Nokoru");

        for (int x = 1; x <= cubesPerSide; x++)
        {
            for (int y = 1; y <= cubesPerSide; y++)
            {
                for (int z = 1; z <= cubesPerSide; z++)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.SetParent(nokoru.transform);
                    cube.transform.position = new Vector3(x - 1, y - 1, z - 1);

                    string cubeName = $"({x},{y},{z})";
                    cube.name = cubeName;

                    if (ShouldApplySpecialTreatment(x, y, z))
                    {
                        if (useToumeiMaterial)
                        {
                            ApplyToumeiMaterial(cube);
                        }
                        else
                        {
                            cube.SetActive(false);
                        }
                    }
                    else
                    {
                        uniqueCombinations.Add(GetSortedCombination(x, y, z));
                    }
                }
            }
        }
    }

    bool ShouldApplySpecialTreatment(int x, int y, int z)
    {
        if (HasDuplicateDigits(x, y, z))
        {
            return true;
        }

        string sortedCombination = GetSortedCombination(x, y, z);
        return uniqueCombinations.Contains(sortedCombination);
    }

    string GetSortedCombination(int x, int y, int z)
    {
        List<int> digits = new List<int> { x, y, z };
        digits.Sort();
        return string.Join(",", digits);
    }

    bool HasDuplicateDigits(int x, int y, int z)
    {
        List<int> digits = new List<int> { x, y, z };
        return digits.Distinct().Count() != digits.Count;
    }

    void ApplyToumeiMaterial(GameObject cube)
    {
        Renderer renderer = cube.GetComponent<Renderer>();
        if (toumeiMaterial != null)
        {
            renderer.material = toumeiMaterial;
        }
        else
        {
            Debug.LogWarning("toumeiMaterial is not assigned in the Inspector!");
        }
    }
}