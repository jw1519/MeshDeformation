using UnityEngine;

// This component requires a MeshFilter to be attached to the same GameObject.
// The MeshFilter provides the mesh that will be deformed.
[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    // The mesh to be deformed, its original vertices, their displaced positions, and velocities for vertex movement.
    private Mesh deformingMesh;
    private Vector3[] originalVertices, displacedVertices;
    private Vector3[] vertexVelocities;
    private MeshCollider meshCollider;

    // Spring force determines how strongly vertices return to their original positions.
    // Damping reduces oscillations and ensures the movement stabilizes over time.
    public float springForce = 50f;
    public float damping = 10f;
    public float colliderUpdateIntival = 0.1f;
    private float colliderUpdatetimer;

    void Start()
    {
        // Retrieve the mesh from the MeshFilter component.
        deformingMesh = GetComponent<MeshFilter>().mesh;

        meshCollider = GetComponent<MeshCollider>();

        // Store the original positions of the vertices.
        originalVertices = deformingMesh.vertices;

        // Initialize displaced vertices and their velocities to match the original positions and zero velocities.
        displacedVertices = new Vector3[originalVertices.Length];
        vertexVelocities = new Vector3[originalVertices.Length];

        // Copy the original vertex positions into the displaced array.
        for (int i = 0; i < originalVertices.Length; i++)
        {
            displacedVertices[i] = originalVertices[i];
        }
        colliderUpdatetimer = 0f;
    }

    // Adds a deforming force at a specific point in the mesh.
    // 'force' determines the intensity, and 'isInflate' controls whether the force inflates or compresses the vertices.
    public void AddDeformingForce(Vector3 point, float force, bool isInflate)
    {
        // Convert the world-space point to local-space coordinates relative to the mesh.
        point = transform.InverseTransformPoint(point);

        // Apply the deforming force to each vertex in the mesh.
        for (int i = 0; i < displacedVertices.Length; i++)
        {
            AddForceToVertex(i, point, force, isInflate);
        }
    }

    // Applies the deformation force to a single vertex.
    private void AddForceToVertex(int i, Vector3 point, float force, bool isInflate)
    {
        // Calculate the vector from the point of the force to the vertex.
        Vector3 pointToVertex = displacedVertices[i] - point;

        // Attenuate the force based on the square of the distance from the point to the vertex.
        float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);

        // Compute the velocity change due to the force.
        float velocity = attenuatedForce * Time.deltaTime;

        // Update the vertex's velocity in the appropriate direction (inflate or pinch).
        if (isInflate)
        {
            vertexVelocities[i] += pointToVertex.normalized * velocity;
        }
        else
        {
            vertexVelocities[i] -= pointToVertex.normalized * velocity;
        }
    }

    void Update()
    {
        // Update each vertex's position and velocity based on the physics simulation.
        for (int i = 0; i < displacedVertices.Length; i++)
        {
            UpdateVertex(i);
        }

        // Apply the updated vertex positions back to the mesh.
        deformingMesh.vertices = displacedVertices;

        // Recalculate the normals to ensure correct lighting on the deformed surface.
        deformingMesh.RecalculateNormals();

        colliderUpdatetimer += Time.deltaTime;

        if (colliderUpdatetimer >= colliderUpdateIntival)
        {
            colliderUpdatetimer = 0;
            UpdateCollisionMesh();
        }
        
    }

    // Updates the velocity and position of a single vertex based on spring and damping forces.
    private void UpdateVertex(int i)
    {
        // Retrieve the current velocity of the vertex.
        Vector3 velocity = vertexVelocities[i];

        // Calculate the displacement of the vertex from its original position.
        Vector3 displacement = displacedVertices[i] - originalVertices[i];

        // Apply a spring force that pulls the vertex back toward its original position.
        velocity -= displacement * springForce * Time.deltaTime;

        // Reduce the velocity over time using the damping factor to stabilise motion.
        velocity *= 1f - damping * Time.deltaTime;

        // Store the updated velocity for the vertex.
        vertexVelocities[i] = velocity;

        // Update the vertex's position based on its velocity.
        displacedVertices[i] += velocity * Time.deltaTime;
    }

    private void UpdateCollisionMesh()
    {
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = deformingMesh;
    }
}
