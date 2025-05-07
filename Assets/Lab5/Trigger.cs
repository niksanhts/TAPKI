using UnityEngine;

namespace Lab5
{
    public class Trigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("TriggerEnter");
        }

    }
}
