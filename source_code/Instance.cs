using UnityEngine;

public class Instance : MonoBehaviour
{
    public GameObject prefabToInstantiate; // 인스턴스화할 프리팹을 가리키는 public 변수

    void Start()
    {
        // 프리팹을 인스턴스화하고 생성된 인스턴스를 변수에 저장
        GameObject instance = Instantiate(prefabToInstantiate);
    }
}