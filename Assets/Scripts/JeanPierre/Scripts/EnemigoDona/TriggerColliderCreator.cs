using UnityEngine;

/// <summary>
/// Este script añade automáticamente un Collider (MeshCollider o BoxCollider) que cubra todo el GameObject
/// y lo configura como trigger.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class TriggerColliderCreator : MonoBehaviour
{
    void Awake()
    {
        // Intentar obtener un collider existente
        Collider col = GetComponent<Collider>();

        if (col == null)
        {
            // Si el objeto tiene MeshFilter, usar MeshCollider para ajustarse al mesh
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                MeshCollider mc = gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                mc.convex = true;         // Convexo necesario para triggers en MeshCollider
                mc.isTrigger = true;
            }
            else
            {
                // Si no hay mesh, usar BoxCollider ajustado a los bounds del Renderer
                BoxCollider bc = gameObject.AddComponent<BoxCollider>();

                Renderer rend = GetComponent<Renderer>();
                Bounds bounds = rend.bounds;

                // Convertir tamaño y centro al espacio local
                bc.size = transform.InverseTransformVector(bounds.size);
                bc.center = transform.InverseTransformPoint(bounds.center);
                bc.isTrigger = true;
            }
        }
        else
        {
            // Si ya existe un collider, simplemente convertirlo en trigger
            col.isTrigger = true;

            // Opcional: ajustar BoxCollider existente a los bounds
            BoxCollider bc = col as BoxCollider;
            if (bc != null)
            {
                Renderer rend = GetComponent<Renderer>();
                Bounds bounds = rend.bounds;
                bc.size = transform.InverseTransformVector(bounds.size);
                bc.center = transform.InverseTransformPoint(bounds.center);
            }

            // Si es MeshCollider, asegurar que sea convexo para triggers
            MeshCollider mc = col as MeshCollider;
            if (mc != null)
            {
                mc.convex = true;
            }
        }
    }
}
