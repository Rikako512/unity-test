using UnityEngine;
using TMPro;

public class DataPointInfo : MonoBehaviour
{
    private TextMeshPro infoText;
    private Camera mainCamera;

    [SerializeField] private float textOffsetMultiplier = 1.0f; // テキストのオフセット乗数
    private void Start()
    {
        // TextMeshPro コンポーネントを持つ子オブジェクトを作成
        GameObject textObj = new GameObject("InfoText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = Vector3.zero; // 初期位置をオブジェクトの中心に設定

        infoText = textObj.AddComponent<TextMeshPro>();
        infoText.alignment = TextAlignmentOptions.Left; // 左揃えに変更
        infoText.fontSize = 2f; // フォントサイズを設定
        infoText.color = Color.black; // 黒色に変更
        infoText.gameObject.SetActive(false);

        // テキストのサイズを自動調整
        infoText.enableAutoSizing = true;
        infoText.fontSizeMin = 1f;
        infoText.fontSizeMax = 3f;

        // 背景の設定
        infoText.enableVertexGradient = true;
        infoText.colorGradient = new VertexGradient(
            new Color(1, 1, 1, 0.5f),
            new Color(1, 1, 1, 0.5f),
            new Color(1, 1, 1, 0.5f),
            new Color(1, 1, 1, 0.5f)
        );

        // テキストの幅と高さを設定
        infoText.rectTransform.sizeDelta = new Vector2(1f, 0.5f);

        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (infoText.gameObject.activeSelf)
        {
            // テキストをカメラの方向に向ける
            infoText.transform.rotation = Quaternion.LookRotation(infoText.transform.position - mainCamera.transform.position);

            // sphereの半径を取得（SphereColliderがアタッチされていると仮定）
            float sphereRadius = GetComponent<SphereCollider>().radius;

            // テキストの位置を設定（sphereの中心から半径×2.5分上に、半径×2.5分右に）
            Vector3 offset = (transform.up + transform.right) * (2.5f * sphereRadius * textOffsetMultiplier);
            infoText.transform.position = transform.position + offset;
        }
    }

    private void OnMouseEnter()
    {
        ShowInfo();
    }

    private void OnMouseExit()
    {
        //HideInfo();
    }

    private void ShowInfo()
    {
        string sphereName = gameObject.name;
        infoText.text = $"{sphereName}";
        infoText.gameObject.SetActive(true);
    }

    private void HideInfo()
    {
        infoText.gameObject.SetActive(false);
    }
}