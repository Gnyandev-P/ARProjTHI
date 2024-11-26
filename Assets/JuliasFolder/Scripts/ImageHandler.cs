using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ImageHandler : MonoBehaviour
{
    [SerializeField]
    private ARTrackedImageManager m_TrackedImageManager;

    [SerializeField]
    private List<ReferencePrefabMapping> referencePrefabMappings;

    private Dictionary<string, ReferencePrefabMapping> prefabDictionary = new Dictionary<string, ReferencePrefabMapping>();

    private GameObject activePrefab = null; // The current active prefab
    private ARTrackedImage currentTrackedImage = null; // The currently tracked image

    // [SerializeField]
    // private GameObject debugPosMarking;

    [Serializable]
    public struct ReferencePrefabMapping
    {
        public string imageName; // Name of the reference image
        public GameObject prefab; // Prefab associated with the image
        public Vector3 offset; // Offset to apply to the prefab
    }

    private void Awake()
    {
        // Initialize the prefab dictionary
        foreach (var mapping in referencePrefabMappings)
        {
            if (!prefabDictionary.ContainsKey(mapping.imageName))
            {
                prefabDictionary[mapping.imageName] = mapping;
            }
        }
    }

    private void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Handle newly added or updated tracked images
        foreach (var trackedImage in eventArgs.added)
        {
            HandleTrackedImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            HandleTrackedImage(trackedImage);
        }
    }

    private void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        // Ignore images that are not actively tracked
        if (trackedImage.trackingState != UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            return;

        // If the image is already the currently tracked one, just update the prefab's transform
        if (currentTrackedImage == trackedImage)
        {
            UpdatePrefabTransform(trackedImage);
            return;
        }

        // If the image is different, switch to the new one
        Debug.Log($"Switching prefab to match image: {trackedImage.referenceImage.name}");

        // Destroy the current prefab
        DestroyActivePrefab();

        // Set the new tracked image
        currentTrackedImage = trackedImage;

        // Instantiate the prefab for the new image
        if (prefabDictionary.TryGetValue(trackedImage.referenceImage.name, out ReferencePrefabMapping mapping))
        {
            Vector3 positionWithOffset = trackedImage.transform.position + mapping.offset;
            activePrefab = Instantiate(mapping.prefab, positionWithOffset, trackedImage.transform.rotation);
            // Debug.Log($"Prefab {activePrefab.name} instantiated for image: {trackedImage.referenceImage.name}");

            //visual debug
            // Instantiate(debugPosMarking,positionWithOffset, trackedImage.transform.rotation);

            // Animator animator = activePrefab.GetComponent<Animator>();
            // if (animator != null)
            // {
            //     animator.Play("JumpOut");
            //     Debug.Log($"Prefab {activePrefab.name} animated with openAnim");
            // }
            
            
        }
    }

    private void UpdatePrefabTransform(ARTrackedImage trackedImage)
    {
        if (activePrefab != null && prefabDictionary.TryGetValue(trackedImage.referenceImage.name, out ReferencePrefabMapping mapping))
        {
            activePrefab.transform.position = trackedImage.transform.position + mapping.offset;
            activePrefab.transform.rotation = trackedImage.transform.rotation;
        }
    }

    private void DestroyActivePrefab()
    {
        if (activePrefab != null)
        {
            // Debug.Log($"Destroying prefab: {activePrefab.name}");
            Destroy(activePrefab);
            activePrefab = null;
        }
    }
}
