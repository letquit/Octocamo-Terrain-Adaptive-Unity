using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CamoController : MonoBehaviour
{
    public Terrain terrain;
    public Material camoMaterial;
    public Text camoIndexText;
    public Image camoIndexImage;

    private float camoIndex = 0.0f;
    private Texture2D currentCamo;
    private float camoIndexUpdateTime = 0.0f;
    private Coroutine setCamoRoutine = null;
    private bool isCamoApplied = false;

    private const float camoSwitchTime = 0.5f;
    private const float camoThreshold = 0.5f;
    private const float camoIndexUpdateThreshold = 1.0f;

    // 供 HUD 读取
    public float CamoIndex01 { get { return camoIndex; } }
    public int CamoIndexPercent { get { return Mathf.RoundToInt(camoIndex * 100f); } }

    private void Start()
    {
        if (terrain == null)
        {
            terrain = FindObjectOfType<Terrain>();
        }

        // 自动查找教程同款 UGUI 元素（CamoController 挂在预制体上，UI 在场景里）
        if (camoIndexText == null)
        {
            var textGo = GameObject.Find("CamoIndex");
            if (textGo != null) camoIndexText = textGo.GetComponent<Text>();
        }
        if (camoIndexImage == null)
        {
            var imageGo = GameObject.Find("CamoIcon");
            if (imageGo != null) camoIndexImage = imageGo.GetComponent<Image>();
        }

        if (camoMaterial == null)
        {
            return;
        }

        currentCamo = camoMaterial.GetTexture("_CamoTexture") as Texture2D;

        var mainTexture = camoMaterial.GetTexture("_MainTexture") as Texture2D;
        if (mainTexture != null)
        {
            CamoDatabase.Instance.AddCamoColour(mainTexture);
        }
        if (currentCamo != null)
        {
            CamoDatabase.Instance.AddCamoColour(currentCamo);
        }

        // 初始状态：显示原始外观（_CamoAmount = 0 显示 Main Texture）
        camoMaterial.SetFloat("_CamoAmount", 0f);
        isCamoApplied = false;
        currentCamo = mainTexture;

        UpdateCamoIndex();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1") && setCamoRoutine == null)
        {
            setCamoRoutine = StartCoroutine(ToggleCamo());
        }
        else if (camoIndexUpdateTime > camoIndexUpdateThreshold)
        {
            UpdateCamoIndex();
        }

        camoIndexUpdateTime += Time.deltaTime;
    }

    // 按一次 = 变成地形迷彩；再按一次 = 复原到原始外观。
    private IEnumerator ToggleCamo()
    {
        if (terrain == null)
        {
            setCamoRoutine = null;
            yield break;
        }

        if (!isCamoApplied)
        {
            var camoTexture = GetTerrainTexture();
            if (camoTexture == null)
            {
                setCamoRoutine = null;
                yield break;
            }

            // 先淡出到原色，换贴图，再淡入到地形迷彩
            float start = camoMaterial.GetFloat("_CamoAmount");
            for (float t = 0; t < camoSwitchTime; t += Time.deltaTime)
            {
                camoMaterial.SetFloat("_CamoAmount", Mathf.Lerp(start, 0f, t / camoSwitchTime));
                yield return null;
            }
            camoMaterial.SetFloat("_CamoAmount", 0f);

            camoMaterial.SetTexture("_CamoTexture", camoTexture);
            CamoDatabase.Instance.GetCamoColour(camoTexture);
            currentCamo = camoTexture;

            for (float t = 0; t < camoSwitchTime; t += Time.deltaTime)
            {
                camoMaterial.SetFloat("_CamoAmount", Mathf.Lerp(0f, 1f, t / camoSwitchTime));
                yield return null;
            }
            camoMaterial.SetFloat("_CamoAmount", 1f);
            isCamoApplied = true;
        }
        else
        {
            // 复原：淡出回原始 Main Texture 并保持（不更换 _CamoTexture）
            float start = camoMaterial.GetFloat("_CamoAmount");
            for (float t = 0; t < camoSwitchTime; t += Time.deltaTime)
            {
                camoMaterial.SetFloat("_CamoAmount", Mathf.Lerp(start, 0f, t / camoSwitchTime));
                yield return null;
            }
            camoMaterial.SetFloat("_CamoAmount", 0f);
            isCamoApplied = false;
            currentCamo = camoMaterial.GetTexture("_MainTexture") as Texture2D;
        }

        UpdateCamoIndex();
        setCamoRoutine = null;
    }

    private Texture2D GetTerrainTexture()
    {
        if (terrain == null)
        {
            return null;
        }

        TerrainData tData = terrain.terrainData;

        Vector3 position;
        position.x = ((transform.position.x - terrain.transform.position.x) / tData.size.x) * tData.alphamapWidth;
        position.z = ((transform.position.z - terrain.transform.position.z) / tData.size.z) * tData.alphamapHeight;

        int px = Mathf.Clamp((int)position.x, 0, tData.alphamapWidth - 1);
        int pz = Mathf.Clamp((int)position.z, 0, tData.alphamapHeight - 1);

        int textureID = 0;
        float[,,] alphamaps = tData.GetAlphamaps(px, pz, 1, 1);

        for (int i = 0; i < alphamaps.GetLength(2); ++i)
        {
            if (camoThreshold < alphamaps[0, 0, i])
            {
                textureID = i;
            }
        }

        return tData.terrainLayers[textureID].diffuseTexture;
    }

    private void UpdateCamoIndex()
    {
        var terrainTexture = GetTerrainTexture();
        if (terrainTexture == null || currentCamo == null)
        {
            return;
        }

        var terrainColor = CamoDatabase.Instance.GetCamoColour(terrainTexture);
        var camoColor = CamoDatabase.Instance.GetCamoColour(currentCamo);

        camoIndex = 1.0f - Mathf.Sqrt(Mathf.Pow(terrainColor.r - camoColor.r, 2) +
                                      Mathf.Pow(terrainColor.g - camoColor.g, 2) +
                                      Mathf.Pow(terrainColor.b - camoColor.b, 2));
        camoIndex = Mathf.Clamp01(camoIndex);

        if (camoIndexText != null)
        {
            camoIndexText.text = Mathf.RoundToInt(camoIndex * 100) + "%";
        }
        if (camoIndexImage != null)
        {
            camoIndexImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f - camoIndex);
        }

        camoIndexUpdateTime = 0.0f;
    }
}
