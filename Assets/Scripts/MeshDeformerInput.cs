using UnityEngine;

// This script listens for user input and applies deformation forces to a mesh at runtime.
public class MeshDeformerInput : MonoBehaviour
{
    // The magnitude of the force to apply when deforming the mesh.
    public float force = 1f;

    void Update()
    {
        // Check if the left mouse button (0) is pressed.
        // This triggers an inflating deformation effect.
        if (Input.GetMouseButton(0))
        {
            ApplyDeformingForce(true); // Inflate
        }

        // Check if the right mouse button (1) is pressed.
        // This triggers a pinching deformation effect.
        if (Input.GetMouseButton(1))
        {
            ApplyDeformingForce(false); // Pinch
        }
    }

    // Sends a raycast from the mouse cursor's position and applies a deforming force if it hits a mesh.
    private void ApplyDeformingForce(bool isInflate)
    {
        // Generate a ray from the main camera through the mouse's screen position.
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform a raycast to detect any collider along the ray.
        if (Physics.Raycast(inputRay, out hit))
        {
            // Attempt to find a MeshDeformer component on the hit object.
            MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();

            // If the hit object has a MeshDeformer, apply the deforming force.
            if (deformer != null)
            {
                // Call the AddDeformingForce method on the MeshDeformer, passing the hit point, force, and deformation type (inflate/pinch).
                deformer.AddDeformingForce(hit.point, force, isInflate);
            }
        }
    }
}