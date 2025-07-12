using System.Collections.Generic;
using UnityEngine;

public class GlitchTime_Effect : MonoBehaviour
{
    [Header("Objetos a duplicar")]
    public List<Transform> Duplicate_Objects;

    [Header("Material del glitch")]
    public Material glitchMaterial;

    [Header("Parámetros del efecto")]
    public int ghostCount = 3;
    public Vector3 offsetPerGhost = new Vector3(0.01f, 0f, 0f);
    public float ghostLifetime;
    public float spawnInterval;

    private float timer = 0f;
    [SerializeField] float Scale_Multiplier;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnGhosts();
            timer = 0f;
        }
    }

    void SpawnGhosts()
    {
        foreach (var target in Duplicate_Objects)
        {
            if (!target) continue;

            Mesh mesh = null;
            MeshFilter mf = target.GetComponent<MeshFilter>();
            SkinnedMeshRenderer smr = target.GetComponent<SkinnedMeshRenderer>();

            if (smr)
            {
                mesh = new Mesh();
                smr.BakeMesh(mesh);
            }
            else if (mf)
            {
                mesh = mf.sharedMesh;
            }

            if (mesh == null) continue;

            for (int i = 0; i < ghostCount; i++)
            {
                GameObject ghost = new GameObject($"Ghost_{target.name}_{i}");
                ghost.transform.position = target.position + offsetPerGhost * (i - ghostCount / 2f);
                ghost.transform.rotation = target.rotation;
                ghost.transform.localScale = target.lossyScale * Scale_Multiplier;

                MeshFilter ghostMF = ghost.AddComponent<MeshFilter>();
                ghostMF.mesh = mesh;

                MeshRenderer ghostMR = ghost.AddComponent<MeshRenderer>();
                ghostMR.material = glitchMaterial;

                ghost.transform.SetParent(this.transform); // opcional: para agruparlos
                Destroy(ghost, ghostLifetime);
            }
        }
    }
}
