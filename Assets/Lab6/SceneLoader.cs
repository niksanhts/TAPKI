using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Название сцены для загрузки (укажите в инспекторе)
    public string sceneNameToLoad;

    // Метод для загрузки сцены (вызывается при нажатии кнопки)
    public void LoadTargetScene()
    {
        // Проверяем, что название сцены не пустое
        if (!string.IsNullOrEmpty(sceneNameToLoad))
        {
            // Загружаем указанную сцену
            SceneManager.LoadScene(sceneNameToLoad);
        }
        else
        {
            Debug.LogError("Scene name is not specified!");
        }
    }

    // Метод для перезагрузки текущей сцены
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Метод для выхода из игры (работает в билде)
    public void QuitGame()
    {
        Application.Quit();
        
        // Для теста в редакторе
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}