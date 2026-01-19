using UnityEngine;
using UnityEngine.UIElements;

public class HealthHUD : MonoBehaviour
{
    public SlantedHealthBar bar;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        if (bar == null)
        {
            bar = new SlantedHealthBar();
            bar.name = "ship-health-bar"; // 👈 IMPORTANT

            bar.style.width = 500;
            bar.style.height = 60;
            bar.style.position = Position.Absolute;
            bar.style.left = Length.Percent(50);
            bar.style.bottom = 20;
            bar.style.translate = new Translate(Length.Percent(-50), 0);

            root.Add(bar);
        }
    }
}
