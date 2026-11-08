
using UnityEngine;

public class FreezeIndicator : MonoBehaviour
{
    [Header("Indicator")]
    [SerializeField] private int fontSize = 24;

    private GUIStyle style;

    private void OnGUI ()
    {
        if ( TimeFreezeManager.Instance == null ) return;
        if ( style == null )
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = fontSize;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
        }

        bool frozen = TimeFreezeManager.Instance.IsFrozen;

        style.normal.textColor = frozen ? new Color(0.75f, 0.45f, 1f) : Color.white;

        string message = frozen ? "TIME FROZEN" : "TIME ACTIVE";

        Rect area = new Rect(Screen.width / 2f - 150f, 20f, 300f, 45f);

        GUI.Label(area, message, style);
    }
}
