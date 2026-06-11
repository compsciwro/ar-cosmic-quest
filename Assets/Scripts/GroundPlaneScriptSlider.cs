using UnityEngine;
using Vuforia;
using UnityEngine.UI;

public class GroundPlaneScriptSlider : MonoBehaviour
{
    public GameObject prefab;
    public float rotationSpeed = 0.2f;
    public float scaleSpeed = 0.01f;
    public float minScale = 0.5f;
    public float maxScale = 3f;

    public Slider rotationSlider;

    public Slider scaleSlider;

    private GameObject placedObject;
    private PlaneFinderBehaviour planeFinder;

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

        if (rotationSlider != null)
        {
            rotationSlider.minValue = 0f;
            rotationSlider.maxValue = 360f;
            rotationSlider.onValueChanged.AddListener(RotateWithSlider);
        }

        if (scaleSlider != null)
        {
            scaleSlider.minValue = minScale;
            scaleSlider.maxValue = maxScale;
            scaleSlider.onValueChanged.AddListener(ScaleWithSlider);
        }
    }

    void OnDestroy()
    {
        if (planeFinder != null)
            planeFinder.OnInteractiveHitTest.RemoveListener(OnPlaneTap);

        if (rotationSlider != null)
            rotationSlider.onValueChanged.RemoveListener(RotateWithSlider);

        if (scaleSlider != null)
            scaleSlider.onValueChanged.RemoveListener(ScaleWithSlider);
    }

    void Update()
    {
        if (placedObject == null) return;

        RotateWithSlider(rotationSlider.value);

        ScaleWithSlider(scaleSlider.value);
    }

    void OnPlaneTap(HitTestResult result)
    {
        if (Input.touchCount == 1 && !isTouchingObject)
        {
            Vector3 position = result.Position;

            if (placedObject == null)
                placedObject = Instantiate(prefab, position, Quaternion.identity);
            else
                placedObject.transform.position = position;
        }
    }

    public void RotateWithSlider(float value)
    {
        if (placedObject == null) return;

        placedObject.transform.rotation = Quaternion.Euler(0f, value, 0f);
    }

    public void ScaleWithSlider(float value)
    {
        if (placedObject != null)
        {
            placedObject.transform.localScale = new Vector3(value, value, value);
        }
    }

}

