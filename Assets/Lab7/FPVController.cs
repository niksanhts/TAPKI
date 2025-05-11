using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FPVController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float crouchSpeed = 1.75f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float crouchHeight = 0.65f;
    [SerializeField] private float standHeight = 1.8f;
    [SerializeField] private float crouchTransitionSpeed = 5f;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 90f;

    [Header("Head Bobbing")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float runBobSpeed = 18f;
    [SerializeField] private float runBobAmount = 0.1f;
    [SerializeField] private float crouchBobSpeed = 10f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    [SerializeField] private Transform cameraHolder;

    [Header("Audio")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioMixerGroup footstepMixerGroup;
    [SerializeField] private float minTimeBetweenSteps = 0.3f;
    [SerializeField] private float maxTimeBetweenSteps = 0.6f;

    [Header("Breathing")]
    [SerializeField] private AudioClip[] breathingSounds;
    [SerializeField] private float breathInterval = 10f;
    [SerializeField] private float runBreathMultiplier = 0.5f;

    private CharacterController characterController;
    private AudioSource audioSource;
    private AudioSource breathAudioSource;
    private float verticalVelocity;
    private bool isJumping;
    private bool isCrouching;
    private float defaultYPos;
    private float timer;
    private float verticalLookRotation = 0f;
    private float nextFootstepTime;
    private float nextBreathTime;
    private float currentSpeed;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        
        breathAudioSource = gameObject.AddComponent<AudioSource>();
        breathAudioSource.outputAudioMixerGroup = footstepMixerGroup;
        breathAudioSource.spatialBlend = 0f;
        
        audioSource.outputAudioMixerGroup = footstepMixerGroup;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        defaultYPos = cameraHolder.localPosition.y;
    }

    private void Start()
    {
        characterController.height = standHeight;
        characterController.center = new Vector3(0, standHeight / 2, 0); 
    
        Vector3 camLocalPos = cameraHolder.localPosition;
        camLocalPos.y = defaultYPos; 
        cameraHolder.localPosition = camLocalPos;
    }

    private void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleCrouch();
        HandleHeadBob();
        HandleFootsteps();
        HandleBreathing();
    }

    private void HandleMovement()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
        currentSpeed = isCrouching ? crouchSpeed : 
                      Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        
        Vector3 moveDirection = (transform.forward * input.y + transform.right * input.x).normalized;
        moveDirection *= currentSpeed;

        if (characterController.isGrounded)
        {
            verticalVelocity = -gravity * Time.deltaTime;
            
            if (Input.GetButtonDown("Jump") && !isCrouching)
            {
                verticalVelocity = jumpForce;
                audioSource.PlayOneShot(jumpSound);
                isJumping = true;
            }
            else if (isJumping)
            {
                audioSource.PlayOneShot(landSound);
                isJumping = false;
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        moveDirection.y = verticalVelocity;
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);

        cameraHolder.localEulerAngles = Vector3.right * verticalLookRotation;
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
        }

        float targetHeight = isCrouching ? crouchHeight : standHeight;
        float currentHeight = Mathf.Lerp(characterController.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        
        characterController.height = currentHeight;
        characterController.center = new Vector3(0, currentHeight / 2, 0);
    }

    private void HandleHeadBob()
    {
        if (!characterController.isGrounded) return;

        float speed = new Vector2(characterController.velocity.x, characterController.velocity.z).magnitude;
        
        if (speed < 0.1f)
        {
            Vector3 c = cameraHolder.localPosition;
            c.y = Mathf.Lerp(c.y, defaultYPos, Time.deltaTime * 3f);
            cameraHolder.localPosition = c;
            return;
        }

        float bobSpeed = isCrouching ? crouchBobSpeed : 
                        currentSpeed == runSpeed ? runBobSpeed : walkBobSpeed;
        float bobAmount = isCrouching ? crouchBobAmount : 
                         currentSpeed == runSpeed ? runBobAmount : walkBobAmount;

        timer += Time.deltaTime * bobSpeed;
        float waveSlice = Mathf.Sin(timer);
        float totalBob = waveSlice * bobAmount;

        Vector3 camLocalPos = cameraHolder.localPosition;
        camLocalPos.y = defaultYPos + totalBob;
        cameraHolder.localPosition = camLocalPos;
    }

    private void HandleFootsteps()
    {
        if (!characterController.isGrounded || characterController.velocity.magnitude < 0.1f)
        {
            nextFootstepTime = Time.time + Random.Range(minTimeBetweenSteps, maxTimeBetweenSteps);
            return;
        }

        if (Time.time > nextFootstepTime)
        {
            PlayFootstepSound();
            
            float speedFactor = currentSpeed / walkSpeed;
            nextFootstepTime = Time.time + Mathf.Lerp(
                maxTimeBetweenSteps, 
                minTimeBetweenSteps, 
                Mathf.Clamp01(speedFactor)
            );
        }
    }

    private void PlayFootstepSound()
    {
        if (footstepSounds.Length == 0) return;
        
        int n = Random.Range(1, footstepSounds.Length);
        audioSource.clip = footstepSounds[n];
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(audioSource.clip);
        
        footstepSounds[n] = footstepSounds[0];
        footstepSounds[0] = audioSource.clip;
    }

    private void HandleBreathing()
    {
        if (Time.time > nextBreathTime && breathingSounds.Length > 0 && characterController.isGrounded)
        {
            float breathRate = currentSpeed > walkSpeed ? breathInterval * runBreathMultiplier : breathInterval;
            nextBreathTime = Time.time + breathRate + Random.Range(-2f, 2f);
            
            AudioClip breathClip = breathingSounds[Random.Range(0, breathingSounds.Length)];
            breathAudioSource.PlayOneShot(breathClip);
        }
    }

    public void ApplyCameraShake(float duration, float magnitude)
    {
        StartCoroutine(CameraShake(duration, magnitude));
    }

    private IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = cameraHolder.localPosition;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            cameraHolder.localPosition = new Vector3(
                originalPos.x + x,
                originalPos.y + y,
                originalPos.z
            );
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        cameraHolder.localPosition = originalPos;
    }
}