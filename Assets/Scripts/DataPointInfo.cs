using UnityEngine;
using TMPro;

public class DataPointInfo : MonoBehaviour
{
    private TextMeshPro infoText;
    private Camera mainCamera;

    [SerializeField] private float textOffsetMultiplier = 1.0f; // �e�L�X�g�̃I�t�Z�b�g�搔
    private void Start()
    {
        // TextMeshPro �R���|�[�l���g�����q�I�u�W�F�N�g���쐬
        GameObject textObj = new GameObject("InfoText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = Vector3.zero; // �����ʒu���I�u�W�F�N�g�̒��S�ɐݒ�

        infoText = textObj.AddComponent<TextMeshPro>();
        infoText.alignment = TextAlignmentOptions.Left; // �������ɕύX
        infoText.fontSize = 2f; // �t�H���g�T�C�Y��ݒ�
        infoText.color = Color.black; // ���F�ɕύX
        infoText.gameObject.SetActive(false);

        // �e�L�X�g�̃T�C�Y����������
        infoText.enableAutoSizing = true;
        infoText.fontSizeMin = 1f;
        infoText.fontSizeMax = 3f;

        // �w�i�̐ݒ�
        infoText.enableVertexGradient = true;
        infoText.colorGradient = new VertexGradient(
            new Color(1, 1, 1, 0.5f),
            new Color(1, 1, 1, 0.5f),
            new Color(1, 1, 1, 0.5f),
            new Color(1, 1, 1, 0.5f)
        );

        // �e�L�X�g�̕��ƍ�����ݒ�
        infoText.rectTransform.sizeDelta = new Vector2(1f, 0.5f);

        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (infoText.gameObject.activeSelf)
        {
            // �e�L�X�g���J�����̕����Ɍ�����
            infoText.transform.rotation = Quaternion.LookRotation(infoText.transform.position - mainCamera.transform.position);

            // sphere�̔��a���擾�iSphereCollider���A�^�b�`����Ă���Ɖ���j
            float sphereRadius = GetComponent<SphereCollider>().radius;

            // �e�L�X�g�̈ʒu��ݒ�isphere�̒��S���甼�a�~2.5����ɁA���a�~2.5���E�Ɂj
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