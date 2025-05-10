using UnityEngine;

public class CamSwitch : MonoBehaviour
{
    [SerializeField] private Camera[] cameras;
    [SerializeField] private int defaultCameraIndex = 0;

    private int currentCameraIndex;
    private int previousCameraIndex;

    private void Start()
    {
        if (cameras == null || cameras.Length == 0)
        {
            Debug.LogError("No cameras assigned in CamSwitch!");
            return;
        }

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null)
                cameras[i].gameObject.SetActive(i == defaultCameraIndex);
        }

        currentCameraIndex = defaultCameraIndex;
        previousCameraIndex = defaultCameraIndex;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && cameras.Length > 0)
            SwitchCamera(0);

        if (Input.GetKeyDown(KeyCode.Alpha2) && cameras.Length > 1)
            SwitchCamera(1);

        if (Input.GetKeyDown(KeyCode.Alpha3) && cameras.Length > 2)
            SwitchCamera(2);
    }

    private void SwitchCamera(int newCameraIndex)
    {
        if (newCameraIndex < 0 || newCameraIndex >= cameras.Length || cameras[newCameraIndex] == null)
        {
            Debug.LogWarning($"Camera index {newCameraIndex} is out of range or null!");
            return;
        }

        if (cameras[currentCameraIndex] != null)
            cameras[currentCameraIndex].gameObject.SetActive(false);

        previousCameraIndex = currentCameraIndex;
        currentCameraIndex = newCameraIndex;

        cameras[currentCameraIndex].gameObject.SetActive(true);
    }

    public void SwitchToPreviousCamera()
    {
        SwitchCamera(previousCameraIndex);
    }
}