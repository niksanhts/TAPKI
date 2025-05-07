using UnityEngine;

namespace Lab2
{
    public class AnimationSwapper : MonoBehaviour
    {
        private bool uiia;

        private Animator _animator;
    
        private void Start()
        {
            _animator = GetComponent<Animator>();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _animator.SetBool("uiia", !uiia);
                uiia = !uiia;
            }
        }
    }
}
