using UnityEngine;

// 迷彩指数 HUD：用 OnGUI 画百分比文字和操作提示（不再叠加红色滤镜）。
// 自动找到场景里的 CamoController，无需手动接线。
public class CamoHud : MonoBehaviour
{
    public CamoController camo;

    private GUIStyle titleStyle;
    private GUIStyle numberStyle;
    private GUIStyle hintStyle;

    private void EnsureInit()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 18;
            titleStyle.normal.textColor = new Color(1f, 1f, 1f, 0.85f);
        }
        if (numberStyle == null)
        {
            numberStyle = new GUIStyle(GUI.skin.label);
            numberStyle.fontSize = 40;
            numberStyle.fontStyle = FontStyle.Bold;
            numberStyle.normal.textColor = Color.white;
        }
        if (hintStyle == null)
        {
            hintStyle = new GUIStyle(GUI.skin.label);
            hintStyle.fontSize = 14;
            hintStyle.normal.textColor = new Color(1f, 1f, 1f, 0.75f);
        }
    }

    private void OnGUI()
    {
        EnsureInit();

        if (camo == null)
        {
            camo = FindObjectOfType<CamoController>();
        }
        if (camo == null)
        {
            return;
        }

        // 指数面板
        GUI.Box(new Rect(16, 16, 300, 108), GUIContent.none);
        GUI.Label(new Rect(30, 22, 280, 28), "迷彩指数  CAMO INDEX", titleStyle);
        GUI.Label(new Rect(30, 48, 280, 54), camo.CamoIndexPercent + "%", numberStyle);

        // 操作提示
        GUI.Label(new Rect(16, 130, 560, 24),
            "WASD 移动    空格 趴下/站起    鼠标左键(Fire1) 切换迷彩 / 复原", hintStyle);
    }
}
