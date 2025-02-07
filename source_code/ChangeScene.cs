using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager를 사용하기 위한 참조

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
