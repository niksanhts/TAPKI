using UnityEngine;

namespace Lab3
{
    [RequireComponent(typeof(UnityEngine.CharacterController))]
    public class CharacterMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float jumpHeight = 2f;
        public float gravity = -9.81f;

        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundDistance = 0.4f;
        public LayerMask groundMask;

        private UnityEngine.CharacterController _controller;
        private Vector3 _velocity;
        private bool _isGrounded;

        private void Start()
        {
            if (_controller == null)
                _controller = GetComponent<UnityEngine.CharacterController>();
        }

        private void Update()
        {
            // Проверка, находится ли персонаж на земле
            _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f; // Небольшая сила прижимающая к земле
            }

            // Получение ввода с клавиатуры
            var x = Input.GetAxis("Horizontal");
            var z = Input.GetAxis("Vertical");

            // Движение персонажа
            var move = transform.right * x + transform.forward * z;
            _controller.Move(move * (moveSpeed * Time.deltaTime));

            // Прыжок
            if (Input.GetButtonDown("Jump") && _isGrounded)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Гравитация
            _velocity.y += gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }
    }
}
