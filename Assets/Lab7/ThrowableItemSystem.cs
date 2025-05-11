using UnityEngine;

public class ThrowableItemSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform grabPoint;
    [SerializeField] private LayerMask pickupLayer;

    [Header("Settings")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float dropDistance = 0.5f;

    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private bool isHolding = false;
    private Vector3 rotationVector = Vector3.zero;

    void Update()
    {
        // Вращение удерживаемого предмета
        if (isHolding)
        {
            rotationVector.x += rotationSpeed * Time.deltaTime;
            rotationVector.y += rotationSpeed * Time.deltaTime;
            heldObject.transform.localRotation = Quaternion.Euler(rotationVector);
        }

        // Подбор предмета
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isHolding)
            {
                TryPickup();
            }
            else
            {
                DropItem();
            }
        }

        // Бросок предмета
        if (Input.GetMouseButtonDown(0) && isHolding)
        {
            ThrowItem();
        }
    }

    void TryPickup()
    {
        Debug.Log("TryPickup");
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, pickupRange, pickupLayer))
        {
            if (hit.collider.CompareTag("Throwable"))
            {
                // Подбираем предмет
                heldObject = hit.collider.gameObject;
                heldObjectRb = heldObject.GetComponent<Rigidbody>();
                
                // Физика
                heldObjectRb.isKinematic = true;
                heldObjectRb.interpolation = RigidbodyInterpolation.None;
                
                // Позиционирование
                heldObject.transform.position = grabPoint.position;
                heldObject.transform.SetParent(grabPoint);
                
                // Настройки коллайдера
                Collider col = heldObject.GetComponent<Collider>();
                if (col != null) col.enabled = false;
                
                isHolding = true;
            }
        }
        else
        {
            Debug.Log("TryPickup - :(");
        }
    }

    void DropItem()
    {
        if (!isHolding) return;

        // Сбрасываем родителя
        heldObject.transform.SetParent(null);
        
        // Включаем физику
        heldObjectRb.isKinematic = false;
        heldObjectRb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // Позиция перед игроком
        Vector3 dropPosition = playerCamera.position + playerCamera.forward * dropDistance;
        heldObject.transform.position = dropPosition;
        
        // Включаем коллайдер
        Collider col = heldObject.GetComponent<Collider>();
        if (col != null) col.enabled = true;
        
        // Сбрасываем ссылки
        heldObject = null;
        heldObjectRb = null;
        isHolding = false;
    }

    void ThrowItem()
    {
        if (!isHolding) return;

        // Сбрасываем родителя
        heldObject.transform.SetParent(null);
        
        // Включаем физику
        heldObjectRb.isKinematic = false;
        heldObjectRb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // Применяем силу броска
        Vector3 throwDirection = playerCamera.forward;
        heldObjectRb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        
        // Включаем коллайдер
        Collider col = heldObject.GetComponent<Collider>();
        if (col != null) col.enabled = true;
        
        // Сбрасываем ссылки
        heldObject = null;
        heldObjectRb = null;
        isHolding = false;
    }

    // Визуализация луча для дебага
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(playerCamera.position, playerCamera.position + playerCamera.forward * pickupRange);
    }
}