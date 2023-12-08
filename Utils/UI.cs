using UnityEngine;

namespace HideMUI.Utils
{
    internal class UI
    {
        public static void UpdateHud(Hud hudInstance)
        {
            setActiveCrosshair(hudInstance, Plugin.m_crosshairActive.Value);
            setActiveStealthHud(hudInstance, Plugin.m_stealthGUIActive.Value);
        }

        public static void setActiveCrosshair(Hud hudInstance, bool active)
        {
            GameObject crosshair = hudInstance.m_crosshair ? hudInstance.m_crosshair.gameObject : null;
            if (!crosshair || (crosshair && active == crosshair.activeInHierarchy)) { return; }

            crosshair.SetActive(active);
        }

        public static void setActiveStealthHud(Hud hudInstance, bool active)
        {
            GameObject[] stealthHudElements = getStealthHudGameObjects(hudInstance);
            foreach (GameObject element in stealthHudElements) {
                if (!element || (element && active == element.activeInHierarchy)) { continue; }
                
                element.SetActive(active);
            }
        }

        private static GameObject[] getStealthHudGameObjects(Hud hudInstance)
        {
            return new GameObject[] {
                hudInstance.m_targeted,
                hudInstance.m_hidden,
                hudInstance.m_targetedAlert,
                hudInstance.m_stealthBar ? hudInstance.m_stealthBar.gameObject : null
            };
        }
    }
}
