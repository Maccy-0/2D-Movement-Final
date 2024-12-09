using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController2D : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private Tilemap tilemap;

    [SerializeField] private float smoothing = 5f;
    [SerializeField] private Vector2 offset;

    private Camera followCamera;

    private Vector2 viewportHalfSize;
    private float leftBoundary;
    private float rightBoundary;
    private float bottomBoundary;

    private Vector3 shakeOffset;

    public void OnValidate()
    {
        if (followCamera == null)
        {
            followCamera = GetComponent<Camera>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        tilemap.CompressBounds();
        CalculateBounds();
    }

    private void CalculateBounds()
    {
        viewportHalfSize = new(followCamera.aspect * followCamera.orthographicSize, followCamera.orthographicSize);

        leftBoundary = tilemap.transform.position.x + tilemap.cellBounds.min.x + viewportHalfSize.x;
        rightBoundary = tilemap.transform.position.x + tilemap.cellBounds.max.x - viewportHalfSize.x;
        bottomBoundary = tilemap.transform.position.y + tilemap.cellBounds.min.y + viewportHalfSize.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Shake(3, 2);
        }
    }

    public void LateUpdate()
    {
        Vector3 desiredPosition = target.position + new Vector3(offset.x, offset.y, transform.position.z) + shakeOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, 1 - Mathf.Exp(-smoothing * Time.deltaTime));

        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, leftBoundary, rightBoundary);
        smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, bottomBoundary, smoothedPosition.y);

        transform.position = smoothedPosition;
    }


    public void Shake(float intensity, float duration)
    {
        StartCoroutine(ShakeCoroutine(intensity, duration));
    }

   private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            shakeOffset = Random.insideUnitCircle * intensity;
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        shakeOffset = Vector3.zero;
    }
}
