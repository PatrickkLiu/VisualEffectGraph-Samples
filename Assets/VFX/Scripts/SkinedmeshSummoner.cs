using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX; // note that The VisualEffect component has been moved from the namespace UnityEngine.Experimental.VFX to UnityEngine.VFX with Unity version 2019.1.

public class SkinedmeshSummoner : MonoBehaviour
{
    public GameObject Earth;
    public GameObject Mars;
    public GameObject VFX;
    public float dissoveTime =3f;
    public float butterflyTime=1f;
    public float assembleTime = 2f;
    private GameObject VFXInstance;

    private Texture2D pointCache;
    private float size;

    private bool summonning;

    void Start()
    {
        this.summonning = false;
        this.Mars.transform.position = this.Earth.transform.position;
        this.Mars.transform.rotation = this.Earth.transform.rotation;
        this.Mars.SetActive(false);
    }
    
    void Update()
    {
        if(!this.summonning && Input.GetKeyDown(KeyCode.Space) == true)
        {
            StartCoroutine(Summon());
        }
    }

    private IEnumerator Summon()
    {
        this.summonning = true;
        float minClippingLevel = 0f;
        //float maxClippingLevel = 2.25f;
        float maxClippingLevel = 2.75f;
        float clippingLevel = maxClippingLevel;
        this.Earth.SetActive(true);
        this.VFXInstance = Instantiate(this.VFX);
        this.VFXInstance.transform.position = this.Earth.transform.position;
        this.VFXInstance.transform.rotation = this.Earth.transform.rotation;
        while (clippingLevel > minClippingLevel)
        {
            this.UpdateSize(this.Earth);
            this.UpdateCachePoint(this.Earth);
            clippingLevel -= Mathf.Abs(maxClippingLevel - minClippingLevel) / dissoveTime * Time.deltaTime;
            Debug.Log ("CalculatedclippingLevel="+ clippingLevel);
            SkinnedMeshRenderer[] renderers = this.Earth.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {                    
                    material.SetFloat("_ClippingLevel", clippingLevel);
                }
            }

            this.VFXInstance.GetComponent<VisualEffect>().SetTexture("PointCache", this.pointCache);
            this.VFXInstance.GetComponent<VisualEffect>().SetFloat("Size", this.size);
            this.VFXInstance.GetComponent<VisualEffect>().SetFloat("ClippingLevel", clippingLevel-0.5f);
            this.VFXInstance.GetComponent<VisualEffect>().SetBool("Emit", true);
   
            yield return 0;

        }
        this.Earth.SetActive(false);
        Debug.Log("Earthsetinvisible");

        yield return new WaitForSeconds(butterflyTime);
        minClippingLevel = -1;
        maxClippingLevel = 3;
        this.Mars.SetActive(true);
        while (clippingLevel < maxClippingLevel)
        {
            this.UpdateSize(this.Mars);
            this.UpdateCachePoint(this.Mars);
            clippingLevel += Mathf.Abs(maxClippingLevel - minClippingLevel) / 10f * Time.deltaTime;
            SkinnedMeshRenderer[] renderers = this.Mars.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    Debug.Log("beforeSet");
                    material.SetFloat("_ClippingLevel", clippingLevel);
                    Debug.Log("afterSet");
                }
            }

            this.VFXInstance.GetComponent<VisualEffect>().SetTexture("PointCache", this.pointCache);
            this.VFXInstance.GetComponent<VisualEffect>().SetFloat("Size", this.size);
            this.VFXInstance.GetComponent<VisualEffect>().SetFloat("ClippingLevel", clippingLevel);
            this.VFXInstance.GetComponent<VisualEffect>().SetBool("Emit", false);

            yield return 0;
        }
        yield return new WaitForSeconds(1);
        this.summonning = false;
    }

   void UpdateSize(GameObject character)
    {
        SkinnedMeshRenderer[] renderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();
        Bounds bound = new Bounds();
        foreach(SkinnedMeshRenderer renderer in renderers)
        {
            Mesh baked = new Mesh();
            renderer.BakeMesh(baked);
            //Mesh baked = character.GetComponent<MeshFilter>().sharedMesh; 
            ///note this is for non-SkinedMeshRenderer -- and comment the two lines above out
            bound.Encapsulate(baked.bounds);
        }
        this.size = Mathf.Max(bound.extents.x * 2, bound.extents.y * 2, bound.extents.z * 2);
    }

    void UpdateCachePoint(GameObject character)
    {
        Mesh baked;
        Vector3[] vertices;
        Transform parent;
        SkinnedMeshRenderer[] renderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();
        List<Color> normalizedVertices = new List<Color>();
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            parent = renderer.gameObject.transform.parent;
            baked = new Mesh();
            renderer.BakeMesh(baked);
            //baked = character.GetComponent<MeshFilter>().sharedMesh;
            ///note this is for non-SkinedMeshRenderer -- and comment the two lines above out
            vertices = baked.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = (character.gameObject.transform.InverseTransformPoint(renderer.gameObject.transform.TransformPoint(vertices[i])) + new Vector3(size * 0.5f, 0, size * 0.5f)) / size;
                normalizedVertices.Add(new Color(vertices[i].x, vertices[i].y, vertices[i].z));
            }
        }
        if(this.pointCache == null || this.pointCache.width != normalizedVertices.Count)
        {
            this.pointCache = new Texture2D(1, normalizedVertices.Count, TextureFormat.RGBA32, false, true);
            this.pointCache.filterMode = FilterMode.Point;
        }
        this.pointCache.SetPixels(normalizedVertices.ToArray());
        this.pointCache.Apply();
    } 
}
