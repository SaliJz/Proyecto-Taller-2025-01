using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CreadorColisionToro : MonoBehaviour
{
    [Header("Parámetros del toro (iniciales)")]
    public float radioMayor = 1f;
    public float radioMenor = 0.3f;
    public int segmentosMayores = 24;
    public int segmentosMenores = 12;

    private MeshFilter mf;
    private MeshCollider colliderNoConvexo;

    void Awake()
    {
        mf = GetComponent<MeshFilter>();

        // collider físico cóncavo
        colliderNoConvexo = gameObject.AddComponent<MeshCollider>();
        colliderNoConvexo.convex = false;

        // Crea hijos con CapsuleColliders para trigger
        CreaTriggersCapsula();
    }

    void Start()
    {
        RegeneraToro(radioMayor);
    }

    public void ActualizarRadioMayor(float nuevoRadio)
    {
        RegeneraToro(nuevoRadio);
        CreaTriggersCapsula();
    }

    private void RegeneraToro(float R)
    {
        var mesh = CrearMallaToro(R, radioMenor, segmentosMayores, segmentosMenores);
        mesh.name = "ToroProcedural";
        mf.sharedMesh = mesh;
        colliderNoConvexo.sharedMesh = mesh;
    }

    private Mesh CrearMallaToro(float R, float r, int segMay, int segMen)
    {
        Mesh mesh = new Mesh();
        int vCount = (segMay + 1) * (segMen + 1);
        var verts = new Vector3[vCount];
        var norms = new Vector3[vCount];
        var uvs = new Vector2[vCount];
        var tris = new int[segMay * segMen * 6];

        float TWO_PI = Mathf.PI * 2f;
        int vi = 0;
        for (int i = 0; i <= segMay; i++)
        {
            float u = i / (float)segMay * TWO_PI;
            Vector3 center = new Vector3(Mathf.Cos(u) * R, 0f, Mathf.Sin(u) * R);

            for (int j = 0; j <= segMen; j++)
            {
                float v = j / (float)segMen * TWO_PI;
                Vector3 n = new Vector3(
                    Mathf.Cos(u) * Mathf.Cos(v),
                    Mathf.Sin(v),
                    Mathf.Sin(u) * Mathf.Cos(v)
                );
                verts[vi] = center + n * r;
                norms[vi] = n;
                uvs[vi] = new Vector2(i / (float)segMay, j / (float)segMen);
                vi++;
            }
        }

        int ti = 0;
        int cols = segMen + 1;
        for (int i = 0; i < segMay; i++)
        {
            for (int j = 0; j < segMen; j++)
            {
                int curr = i * cols + j;
                int next = (i + 1) * cols + j;

                tris[ti++] = curr;
                tris[ti++] = next;
                tris[ti++] = curr + 1;

                tris[ti++] = curr + 1;
                tris[ti++] = next;
                tris[ti++] = next + 1;
            }
        }

        mesh.vertices = verts;
        mesh.normals = norms;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateBounds();
        return mesh;
    }

    private void CreaTriggersCapsula()
    {
        // elimina hijos previos
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        float TWO_PI = Mathf.PI * 2f;
        float paso = TWO_PI / segmentosMayores;
        float altura = paso * radioMayor + 2f * radioMenor;

        for (int i = 0; i < segmentosMayores; i++)
        {
            float u = i * paso;
            Vector3 center = new Vector3(Mathf.Cos(u) * radioMayor, 0f, Mathf.Sin(u) * radioMayor);
            Vector3 tangent = new Vector3(-Mathf.Sin(u), 0f, Mathf.Cos(u));

            var go = new GameObject("TriggerCapsula_" + i);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = center;
            go.transform.localRotation = Quaternion.LookRotation(tangent, Vector3.up);

            var cap = go.AddComponent<CapsuleCollider>();
            cap.direction = 2;           // eje Z local
            cap.isTrigger = true;
            cap.radius = radioMenor;
            cap.height = altura;
        }
    }
}








//using UnityEngine;

//[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
//public class CreadorColisionToro : MonoBehaviour
//{
//    [Header("Parámetros del toro (iniciales)")]
//    public float radioMayor = 1f;
//    public float radioMenor = 0.3f;
//    public int segmentosMayores = 24;
//    public int segmentosMenores = 12;

//    private MeshFilter mf;
//    private MeshCollider colliderNoConvexo;
//    private MeshCollider colliderTrigger;
//    private float radioMayorInicial;

//    void Awake()
//    {
//        mf = GetComponent<MeshFilter>();
//        // Collider físico (no convexo)
//        colliderNoConvexo = gameObject.AddComponent<MeshCollider>();
//        colliderNoConvexo.convex = false;
//        // Collider trigger (convexo)
//        colliderTrigger = gameObject.AddComponent<MeshCollider>();
//        colliderTrigger.convex = true;
//        colliderTrigger.isTrigger = true;
//        radioMayorInicial = radioMayor;
//    }

//    void Start()
//    {
//        RegeneraToro(radioMayorInicial);
//    }

//    public void ActualizarRadioMayor(float nuevoRadioMayor)
//    {
//        RegeneraToro(nuevoRadioMayor);
//    }

//    private void RegeneraToro(float R)
//    {
//        Mesh mallaToro = CrearMallaToro(R, radioMenor, segmentosMayores, segmentosMenores);

//        mf.sharedMesh = mallaToro;

//        // Actualizar collider físico
//        colliderNoConvexo.sharedMesh = mallaToro;

//        // Actualizar trigger collider
//        colliderTrigger.sharedMesh = mallaToro;
//    }

//    private Mesh CrearMallaToro(float R, float r, int segMay, int segMen)
//    {
//        Mesh mesh = new Mesh { name = "ToroProcedural" };
//        int vCount = (segMay + 1) * (segMen + 1);
//        Vector3[] vertices = new Vector3[vCount];
//        Vector3[] normales = new Vector3[vCount];
//        Vector2[] uvs = new Vector2[vCount];
//        int[] tris = new int[segMay * segMen * 6];

//        float TWO_PI = Mathf.PI * 2f;
//        int vi = 0;
//        for (int i = 0; i <= segMay; i++)
//        {
//            float u = (float)i / segMay * TWO_PI;
//            Vector3 center = new Vector3(Mathf.Cos(u) * R, 0f, Mathf.Sin(u) * R);

//            for (int j = 0; j <= segMen; j++)
//            {
//                float v = (float)j / segMen * TWO_PI;
//                Vector3 normal = new Vector3(
//                    Mathf.Cos(u) * Mathf.Cos(v),
//                    Mathf.Sin(v),
//                    Mathf.Sin(u) * Mathf.Cos(v)
//                );
//                vertices[vi] = center + normal * r;
//                normales[vi] = normal;
//                uvs[vi] = new Vector2((float)i / segMay, (float)j / segMen);
//                vi++;
//            }
//        }

//        int ti = 0;
//        int cols = segMen + 1;
//        for (int i = 0; i < segMay; i++)
//        {
//            for (int j = 0; j < segMen; j++)
//            {
//                int current = i * cols + j;
//                int next = (i + 1) * cols + j;

//                tris[ti++] = current;
//                tris[ti++] = next;
//                tris[ti++] = current + 1;
//                tris[ti++] = current + 1;
//                tris[ti++] = next;
//                tris[ti++] = next + 1;
//            }
//        }

//        mesh.vertices = vertices;
//        mesh.normals = normales;
//        mesh.uv = uvs;
//        mesh.triangles = tris;
//        mesh.RecalculateBounds();
//        return mesh;
//    }
//}










