using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CreadorColisionToro : MonoBehaviour
{
    [Header("Parametros del toro")]
    public float radioMayor = 1f;     // distancia desde el centro al centro del tubo
    public float radioMenor = 0.3f;   // radio del tubo
    public int segmentosMayores = 24; // subdivisiones alrededor del anillo principal
    public int segmentosMenores = 12; // subdivisiones del tubo

    [Header("Material")]
    public Material materialToro;     // asignar un material (por ejemplo rojo) en el Inspector

    void Start()
    {
        // Generar malla del toro
        Mesh mallaToro = CrearMallaToro(radioMayor, radioMenor, segmentosMayores, segmentosMenores);

        // Asignar a MeshFilter
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.sharedMesh = mallaToro;

        // Asignar material (se define en el Inspector)
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (materialToro != null)
        {
            mr.sharedMaterial = materialToro;
        }
        else
        {
            Debug.LogWarning("CreadorColisionToro: materialToro no asignado en el Inspector.");
        }

        // Asignar MeshCollider
        MeshCollider mc = GetComponent<MeshCollider>();
        mc.sharedMesh = mallaToro;
        mc.convex = false;  // conservar el agujero del toro
    }

    Mesh CrearMallaToro(float R, float r, int segMay, int segMen)
    {
        Mesh mesh = new Mesh { name = "ToroProcedural" };

        int cuentaVertices = (segMay + 1) * (segMen + 1);
        Vector3[] vertices = new Vector3[cuentaVertices];
        Vector3[] normales = new Vector3[cuentaVertices];
        Vector2[] uvs = new Vector2[cuentaVertices];
        int[] triangulos = new int[segMay * segMen * 6];

        float dosPi = Mathf.PI * 2f;
        int vi = 0;
        for (int i = 0; i <= segMay; i++)
        {
            float u = (float)i / segMay * dosPi;
            Vector3 centro = new Vector3(Mathf.Cos(u) * R, 0f, Mathf.Sin(u) * R);

            for (int j = 0; j <= segMen; j++)
            {
                float v = (float)j / segMen * dosPi;
                Vector3 normal = new Vector3(
                    Mathf.Cos(u) * Mathf.Cos(v),
                    Mathf.Sin(v),
                    Mathf.Sin(u) * Mathf.Cos(v)
                );
                vertices[vi] = centro + normal * r;
                normales[vi] = normal;
                uvs[vi] = new Vector2((float)i / segMay, (float)j / segMen);
                vi++;
            }
        }

        int ti = 0;
        int columnas = segMen + 1;
        for (int i = 0; i < segMay; i++)
        {
            for (int j = 0; j < segMen; j++)
            {
                int actual = i * columnas + j;
                int siguiente = (i + 1) * columnas + j;

                // Dos triangulos por cada cuadrilatero
                triangulos[ti++] = actual;
                triangulos[ti++] = siguiente;
                triangulos[ti++] = actual + 1;

                triangulos[ti++] = actual + 1;
                triangulos[ti++] = siguiente;
                triangulos[ti++] = siguiente + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangulos;
        mesh.RecalculateBounds();

        return mesh;
    }
}
