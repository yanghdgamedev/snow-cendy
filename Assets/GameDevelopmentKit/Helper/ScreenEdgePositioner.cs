using UnityEngine;

public class ScreenEdgePositioner : MonoBehaviour
{
    public enum ScreenEdge
    {
        Left,
        Right
    }

    [Header("Screen Edge Settings")]
    [SerializeField] private ScreenEdge targetEdge = ScreenEdge.Left;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float edgeOffset = 0f;
    
    [Header("Position Settings")]
    [SerializeField] private bool maintainYPosition = true;
    [SerializeField] private bool maintainZPosition = true;
    [SerializeField] private float customYPosition = 0f;
    [SerializeField] private float customZPosition = 0f;
    
    [Header("Update Settings")]
    // [SerializeField] private bool updateInRealtime = false;
    [SerializeField] private bool updateOnStart = true;
    
    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        
        if (updateOnStart)
        {
            UpdatePosition();
        }
    }
    
    // private void LateUpdate()
    // {
    //     if (updateInRealtime)
    //     {
    //         UpdatePosition();
    //     }
    // }
    
    private void OnValidate()
    {
        if (Application.isPlaying && targetCamera != null)
        {
            UpdatePosition();
        }
    }
    
    public void UpdatePosition()
    {
        if (targetCamera == null)
        {
            Logger.LogWarning("ScreenEdgePositioner: No camera assigned!");
            return;
        }
        
        float yPosition = maintainYPosition ? transform.position.y : customYPosition;
        float zPosition = maintainZPosition ? transform.position.z : customZPosition;
        
        Vector3 targetWorldPos = new Vector3(0, yPosition, zPosition);
        
        // Create a plane at the target Z position, facing the camera
        Plane targetPlane = new Plane(-targetCamera.transform.forward, targetWorldPos);
        
        // Calculate viewport edges (0,0.5) for left, (1,0.5) for right at middle height
        Vector3 viewportPoint = targetEdge == ScreenEdge.Left ? 
            new Vector3(0, 0.5f, 0) : 
            new Vector3(1, 0.5f, 0);
        
        // Create a ray from the camera through the viewport point
        Ray ray = targetCamera.ViewportPointToRay(viewportPoint);
        
        // Find where the ray intersects with the plane at target Z
        float distance;
        if (targetPlane.Raycast(ray, out distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);
            
            // Apply offset (inward from edge)
            float offsetDirection = targetEdge == ScreenEdge.Left ? 1f : -1f;
            Vector3 cameraRight = targetCamera.transform.right;
            worldPoint += cameraRight * (edgeOffset * offsetDirection);
            
            // Use calculated X, but maintain Y and Z
            transform.position = new Vector3(worldPoint.x, yPosition, zPosition);
        }
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (targetCamera == null) return;
        
        float y = maintainYPosition ? transform.position.y : customYPosition;
        float z = maintainZPosition ? transform.position.z : customZPosition;
        
        Vector3 targetWorldPos = new Vector3(0, y, z);
        Plane targetPlane = new Plane(-targetCamera.transform.forward, targetWorldPos);
        
        // Calculate left edge
        Ray leftRay = targetCamera.ViewportPointToRay(new Vector3(0, 0.5f, 0));
        float leftDistance;
        Vector3 leftEdge = targetWorldPos;
        if (targetPlane.Raycast(leftRay, out leftDistance))
        {
            leftEdge = leftRay.GetPoint(leftDistance);
        }
        
        // Calculate right edge
        Ray rightRay = targetCamera.ViewportPointToRay(new Vector3(1, 0.5f, 0));
        float rightDistance;
        Vector3 rightEdge = targetWorldPos;
        if (targetPlane.Raycast(rightRay, out rightDistance))
        {
            rightEdge = rightRay.GetPoint(rightDistance);
        }
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(leftEdge + Vector3.down * 5, leftEdge + Vector3.up * 5);
        Gizmos.DrawLine(rightEdge + Vector3.down * 5, rightEdge + Vector3.up * 5);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(leftEdge, rightEdge);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    #endif
}