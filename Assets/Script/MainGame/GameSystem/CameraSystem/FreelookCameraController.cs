using UnityEngine;
using Unity.Cinemachine;

public class FreelookCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera freeLookCamera;
    [SerializeField] private bool enableDragPanMove = false;

    [SerializeField] private float minFieldOfView = 20f;
    [SerializeField] private float maxFieldOfView = 80f;

    private bool dragPanMoveActive;
    private Vector2 lastMousePosition;
    private float targetFieldOfView = 50;

    private void Update()
    {
        HandleCameraMovement();
        if(enableDragPanMove)
        {
            HandleCameraMovementDrag();
        }

        HandleCameraRotation();

        HandleCameraZoom();
    }


    #region Camera Controls
    private void HandleCameraMovement()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) inputDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) inputDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) inputDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) inputDir.x = +1f;

        Vector3 moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;
        float moveSpeed = 50f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void HandleCameraMovementDrag()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);

        if(Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftControl))
        {
            dragPanMoveActive = true;
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
        {
            dragPanMoveActive = false;
        }

        if (dragPanMoveActive)
        {
            Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - lastMousePosition;
            
            float dragSpeed = 2f;
            inputDir.x = mouseMovementDelta.x * dragSpeed;
            inputDir.z = mouseMovementDelta.y * dragSpeed;
            lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleCameraRotation()
    {
        float rotateDir = 0f;
        if (Input.GetKey(KeyCode.Q)) rotateDir = +1f;
        if (Input.GetKey(KeyCode.E)) rotateDir = -1f;

        float rotateSpeed = 100f;
        transform.eulerAngles += new Vector3(0, rotateDir * rotateSpeed * Time.deltaTime, 0);
    }

    private void HandleCameraZoom()
    {
        if (freeLookCamera == null)
        {
            Debug.LogWarning("freeLookCamera 或 m_Lens 未初始化");
            return;
        }

        if (Input.mouseScrollDelta.y > 0)
        {
            targetFieldOfView -= 5f;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            targetFieldOfView += 5f;
        }
        targetFieldOfView = Mathf.Clamp(targetFieldOfView, minFieldOfView, maxFieldOfView);

        float zoomSpeed = 10f;
        freeLookCamera.Lens.FieldOfView = 
            Mathf.Lerp(freeLookCamera.Lens.FieldOfView, targetFieldOfView, zoomSpeed * Time.deltaTime);
    }
    #endregion
}
