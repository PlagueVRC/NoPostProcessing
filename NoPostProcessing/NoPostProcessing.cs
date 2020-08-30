using System.Collections;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NoPostProcessing
{
    public class NoPostProcessing : MelonMod
    {
        #region Data Types
            public static bool DisablePostProcessing = true;

            public static bool WorldWasChanged = false;
        #endregion

        /// <summary>
        /// Called When The Game Starts
        /// </summary>
        public override void OnApplicationStart()
        {
            //Register Config Category And Toggle Boolean If Not Present
            MelonPrefs.RegisterCategory("NoPostProcessing", "NoPostProcessing");
            MelonPrefs.RegisterBool("NoPostProcessing", "DisablePostProcessing", true, "Disable Post Processing");

            //Set Initial Post Processing State To The One In Config (Or Default: True)
            DisablePostProcessing = MelonPrefs.GetBool("NoPostProcessing", "DisablePostProcessing");
        }

        /// <summary>
        /// Called When UIExpansionKit Applies Mod Settings
        /// </summary>
        public override void OnModSettingsApplied()
        {
            //Update Local Boolean: ConfigValue To The State Of The Boolean In Config So We Don't Need To Keep Pulling It From Config, Which Would Cause Unnecessary Disk Reads
            bool ConfigValue = MelonPrefs.GetBool("NoPostProcessing", "DisablePostProcessing");

            //If Post Processing State Was Changed
            if (DisablePostProcessing != ConfigValue)
            {
                //Update Global Boolean To New ConfigValue
                DisablePostProcessing = MelonPrefs.GetBool("NoPostProcessing", "DisablePostProcessing");

                //Enumerate All Cameras
                foreach (Camera cam in Camera.allCameras)
                {
                    //If The Camera Has Post Processing Applied To It
                    if (cam.GetComponent<PostProcessLayer>() != null)
                    {
                        //If The Camera's Post Processing Layer Has Not Been Toggled Previously
                        if (!DisablePostProcessing != cam.GetComponent<PostProcessLayer>().enabled)
                        {
                            //Variably Log If The Post Processing Layer Was Removed Or Added
                            MelonLogger.Log(DisablePostProcessing
                                ? "Removed Post Processing From World!"
                                : "Re-Added Post Processing From World!");

                            //Set The Post Processing Layer On The Camera To The Opposite State Of DisablePostProcessing
                            cam.GetComponent<PostProcessLayer>().enabled = !DisablePostProcessing;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called When A Level Has Loaded And Initialised
        /// </summary>
        /// <param name="level">The ID Of The Level Loaded</param>
        public override void OnLevelWasInitialized(int level)
        {
            //If World Was Initialized (Not Login/Loading)
            if (level == -1)
            {
                //Define Boolean Stating The World Was Changed
                WorldWasChanged = true;

                //Define Delay Offset To Use In OnUpdate()
                MelonCoroutines.Start(DelayedAction());
            }
        }

        /// <summary>
        /// An IEnumerator Used To Delay Setting Of Post Processing On World Load To When The World Applies It
        /// </summary>
        /// <returns></returns>
        IEnumerator DelayedAction()
        {
            yield return new WaitForSeconds(5);

            //Do Not Run Any Code Below Unless The Delay Has Passed & The World Has Changed
            if (WorldWasChanged)
            {
                //Define WorldWasChanged As False Until Next World Change
                WorldWasChanged = false;

                //Enumerate All Cameras
                foreach (Camera cam in Camera.allCameras)
                {
                    //If The Camera Has Post Processing Applied To It
                    if (cam.GetComponent<PostProcessLayer>() != null)
                    {
                        //If The Camera's Post Processing Layer Has Not Been Toggled Previously
                        if (!DisablePostProcessing != cam.GetComponent<PostProcessLayer>().enabled)
                        {
                            //Variably Log If The Post Processing Layer Was Removed Or Added
                            MelonLogger.Log(DisablePostProcessing ? "Removed Post Processing From World!" : "Re-Added Post Processing To World!");

                            //Set The Post Processing Layer On The Camera To The Opposite State Of DisablePostProcessing
                            cam.GetComponent<PostProcessLayer>().enabled = !DisablePostProcessing;
                        }
                    }
                }
            }
        }
    }
}
