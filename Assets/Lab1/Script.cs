using UnityEngine;

public class Script : MonoBehaviour
{
    [SerializeField] private GameObject[] o;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var obj in o)
            {
                obj.transform.position = new Vector3(obj.transform.position.x, 2, obj.transform.position.z);
            }
        }
    }
}
