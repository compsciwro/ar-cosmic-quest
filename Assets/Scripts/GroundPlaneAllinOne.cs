using UnityEngine;
using Vuforia;

public class GroundPlaneAllinOne : MonoBehaviour
{
    public GameObject prefab;
    public float rotationSpeed = 0.2f;
    public float scaleSpeed = 0.01f;
    public float minScale = 0.1f;
    public float maxScale = 3f;

    private GameObject placedObject;
    private PlaneFinderBehaviour planeFinder;

    public GameObject floorPlane;

    public ParticleSystem particleSystem;

    public float dropHeight = 0.3f;

    private bool isTouchingObject = false;

    void Start()
    {
        planeFinder = FindObjectOfType<PlaneFinderBehaviour>();

        if (planeFinder == null)
        {
            Debug.LogError("PlaneFinderBehaviour not found!");
            return;
        }

        planeFinder.HitTestMode = HitTestMode.AUTOMATIC;
        planeFinder.OnInteractiveHitTest.AddListener(OnPlaneTap);

        Debug.Log("Ground Plane script started.");
    }

    void OnDestroy()
    {
        if (planeFinder != null)
            planeFinder.OnInteractiveHitTest.RemoveListener(OnPlaneTap);
    }

    void Update()
    {
        if (placedObject == null) return;

        // STOP PARTICLES IF NO TOUCH
        if (Input.touchCount == 0)
        {
            isTouchingObject = false;

            if (particleSystem != null && particleSystem.isPlaying)
                particleSystem.Stop();

            return;
        }

        // ROTATE
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isTouchingObject = IsTouchingObject(touch.position);
            }

            if (touch.phase == TouchPhase.Moved && isTouchingObject)
            {
                float rotY = touch.deltaPosition.x * rotationSpeed;
                placedObject.transform.Rotate(Vector3.up, -rotY, Space.World);

                if (particleSystem != null)
                {
                    particleSystem.transform.position = placedObject.transform.position;

                    if (!particleSystem.isPlaying)
                        particleSystem.Play();
                }
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isTouchingObject = false;

                if (particleSystem != null && particleSystem.isPlaying)
                    particleSystem.Stop();
            }
        }

        // SCALE
        if (Input.touchCount == 2)
        {
            if (particleSystem != null && particleSystem.isPlaying)
                particleSystem.Stop();

            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            float prevDist = Vector2.Distance(
                t1.position - t1.deltaPosition,
                t2.position - t2.deltaPosition
            );

            float currentDist = Vector2.Distance(t1.position, t2.position);

            float delta = currentDist - prevDist;

            float scaleFactor = 1 + delta * scaleSpeed;

            Vector3 newScale = placedObject.transform.localScale * scaleFactor;

            float clamped = Mathf.Clamp(newScale.x, minScale, maxScale);
            placedObject.transform.localScale = Vector3.one * clamped;
        }
    }

    void OnPlaneTap(HitTestResult result)
    {
        Debug.Log("Plane tapped.");

        if (prefab == null)
        {
            Debug.LogError("Prefab is missing. Drag your prefab into the script slot.");
            return;
        }

        Vector3 groundPosition = result.Position;
        Vector3 position = result.Position + Vector3.up * dropHeight;

        if (floorPlane != null)
        {
            floorPlane.transform.position = groundPosition + Vector3.down * 0.02f;
            floorPlane.transform.rotation = result.Rotation;
        }

        if (placedObject == null)
        {
            placedObject = Instantiate(prefab, position, Quaternion.identity);

            if (particleSystem != null)
                particleSystem.Stop();

            Rigidbody rb = placedObject.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.WakeUp();
            }

            Debug.Log("Object placed.");
        }
        else
        {
            placedObject.transform.position = position;

            Rigidbody rb = placedObject.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.WakeUp();
            }

            Debug.Log("Object moved.");
        }
    }

    bool IsTouchingObject(Vector2 screenPos)
    {
        if (Camera.main == null)
        {
            Debug.LogError("Camera.main is missing. Make sure ARCamera is tagged MainCamera.");
            return false;
        }

        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        // This checks ALL raycast hits, not only the first one.
        // This helps if the FloorPlane is hit before the object.
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit hit in hits)
        {
            if (placedObject != null &&
                (hit.transform == placedObject.transform || hit.transform.IsChildOf(placedObject.transform)))
            {
                return true;
            }
        }

        return false;
    }
}