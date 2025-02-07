using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager�� ����ϱ� ���� ����

public class ChangeScene : MonoBehaviour
{
    public void changeGameScene(){
        SceneManager.LoadScene("Jungin");
    }
    public void quitGame(){
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
