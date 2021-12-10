using MelonLoader;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NoPostProcessing
{
    public class NoPostProcessing : MelonMod
    {
        public static bool DisablePostProcessing = true;

        /// <summary>
        /// Called When The Game Starts
        /// </summary>
        public override void OnApplicationStart()
        {
            //Register Config Category And Toggle Boolean If Not Present
            MelonPreferences.CreateCategory("NoPostProcessing", "NoPostProcessing");
            MelonPreferences.CreateEntry("NoPostProcessing", "DisablePostProcessing", true, "Disable Post Processing");

            //Set Initial Post Processing State To The One In Config (Or Default: True)
            DisablePostProcessing = MelonPreferences.GetEntry<bool>("NoPostProcessing", "DisablePostProcessing").Value;
        }

        /// <summary>
        /// Method To Toggle Post Processing
        /// </summary>
        /// <param name="state"></param>
        private void SetPostProcessing(bool state, bool force = false)
        {
            //If Post Processing State Was Changed Or Is To Be Forced
            if (DisablePostProcessing != state || force)
            {
                //Update Global Boolean To New ConfigValue
                DisablePostProcessing = state;

                //Variable For Log After
                var DidToggle = false;

                //Enumerate All Cameras
                foreach (var cam in Camera.allCameras)
                {
                    //If The Camera Has Post Processing Applied To It
                    if (cam.GetComponent<PostProcessLayer>() != null)
                    {
                        //If The Camera's Post Processing Layer Has Not Been Toggled Previously
                        if (cam.GetComponent<PostProcessLayer>().enabled != !DisablePostProcessing)
                        {
                            //Set For Logging After
                            DidToggle = true;

                            //Set The Post Processing Layer On The Camera To The Opposite State Of DisablePostProcessing
                            cam.GetComponent<PostProcessLayer>().enabled = !DisablePostProcessing;
                        }
                    }
                }

                foreach (var Volume in Resources.FindObjectsOfTypeAll<PostProcessVolume>())
                {
                    // -_-
                    if (Volume != null)
                    {
                        //If The Post Processing Volume Has Not Been Toggled Previously
                        if (Volume.enabled != !DisablePostProcessing)
                        {
                            //Set For Logging After
                            DidToggle = true;

                            //Set The Post Processing Layer On The Camera To The Opposite State Of DisablePostProcessing
                            Volume.enabled = !DisablePostProcessing;
                        }
                    }
                }

                if (DidToggle)
                {
                    //Variably Log If The Post Processing Was Removed Or Added
                    MelonLogger.Msg(DisablePostProcessing
                        ? "Removed Post Processing From World!"
                        : "Re-Added Post Processing To World!");
                }
            }
        }

        /// <summary>
        /// Called When UIExpansionKit Applies Mod Settings
        /// </summary>
        public override void OnModSettingsApplied()
        {
            //Update Local Boolean: ConfigValue To The State Of The Boolean In Config So We Don't Need To Keep Pulling It From Config, Which Would Cause Unnecessary Disk Reads
            bool ConfigValue = MelonPreferences.GetEntry<bool>("NoPostProcessing", "DisablePostProcessing").Value;

            SetPostProcessing(ConfigValue);
        }

        /// <summary>
        /// Called When A Level Has Loaded And Initialised
        /// </summary>
        /// <param name="level">The ID Of The Level Loaded</param>
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            //If World Was Initialized (Not Login/Loading)
            if (buildIndex == -1)
            {
                //Define Delay Offset To Use In OnUpdate()
                MelonCoroutines.Start(DelayedAction());
            }
        }

        /// <summary>
        /// An IEnumerator Used To Delay Setting Of Post Processing On World Load To When The World Applies It
        /// </summary>
        /// <returns></returns>
        private IEnumerator DelayedAction()
        {
            yield return new WaitForSeconds(5);

            SetPostProcessing(DisablePostProcessing);

            yield break;
        }
    }
}
