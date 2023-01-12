using Fusion.XR.Host.Rig;
using UnityEngine;
using UnityEngine.Events;


namespace Fusion.XR.Host
{
    /**
     * 
     * Script to display an overlay UI to select desktop or VR mode, and active the associated rig, alongside the connexion component
     * 
     **/

    public class RigSelection : MonoBehaviour
    {
        public UnityEvent OnSelectRig;

        public GameObject connexionHandler;
        public HardwareRig vrRig;
        public HardwareRig desktopRig;

        public bool forceVROnAndroid = true;

        public enum Mode
        {
            SelectedByUIAndPref,
            ForceVR,
            ForceDesktop
        }
        public Mode mode = Mode.SelectedByUIAndPref;

        private void Awake()
        {
            connexionHandler.gameObject.SetActive(false);
            vrRig.gameObject.SetActive(false);
            desktopRig.gameObject.SetActive(false);

#if !UNITY_EDITOR && UNITY_ANDROID
            if (forceVROnAndroid)
            {
                EnableVRRig();
                return;
            }
#endif
            if (mode == Mode.ForceVR)
            {
                EnableVRRig();
                return;
            }
            if (mode == Mode.ForceDesktop)
            {
                EnableDesktopRig();
                return;
            }


            var prefMode = PlayerPrefs.GetString("RigMode");
            if (prefMode == "VR") EnableVRRig();
            if (prefMode == "Desktop") EnableDesktopRig();
            if (prefMode != "")
            {
                PlayerPrefs.SetString("RigMode", "");
                PlayerPrefs.Save();
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(5, 5, Screen.width - 10, Screen.height - 10));
            {
                GUILayout.BeginVertical(GUI.skin.window);
                {

                    if (GUILayout.Button("VR", GUILayout.Height(500)))
                    {
                        EnableVRRig();
                    }
                    if (GUILayout.Button("Desktop",GUILayout.Height(100)))
                    {
                        EnableDesktopRig();
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndArea();
        }

        void EnableVRRig()
        {
            gameObject.SetActive(false);
            vrRig.gameObject.SetActive(true);
            connexionHandler.gameObject.SetActive(true);
            if (OnSelectRig != null) OnSelectRig.Invoke();
        }
        void EnableDesktopRig()
        {
            gameObject.SetActive(false);
            desktopRig.gameObject.SetActive(true);
            connexionHandler.gameObject.SetActive(true);
            if (OnSelectRig != null) OnSelectRig.Invoke();
        }
    }
}