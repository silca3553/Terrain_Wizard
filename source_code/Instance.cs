using UnityEngine;

public class Instance : MonoBehaviour
{
    public GameObject prefabToInstantiate; // �ν��Ͻ�ȭ�� �������� ����Ű�� public ����

    void Start()
    {
        // �������� �ν��Ͻ�ȭ�ϰ� ������ �ν��Ͻ��� ������ ����
        GameObject instance = Instantiate(prefabToInstantiate);
    }
}