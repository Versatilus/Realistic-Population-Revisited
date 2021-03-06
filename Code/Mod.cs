﻿using ICities;
using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    public class RealPopMod : IUserMod
    {
        public static string ModName => "Realistic Population Revisited";
        public static string Version => "1.4.3";

        public string Name => ModName + " " + Version;

        public string Description => Translations.Translate("RPR_DESC");


        /// <summary>
        /// Called by the game when the mod options panel is setup.
        /// </summary>
        public void OnSettingsUI(UIHelperBase helper)
        {
            // Setup options panel reference.
            OptionsPanel.optionsPanel = ((UIHelper)helper).self as UIScrollablePanel;
            OptionsPanel.optionsPanel.autoLayout = false;
        }


        /// <summary>
        /// Adds the options panel event handler for the start screen (to enable/disable options panel based on visibility).
        /// </summary>
        public void OnEnabled()
        {
            // Load settings file.
            SettingsUtils.LoadSettings();

            // Populate Datastore from configuration file.
            XMLUtilsWG.ReadFromXML();

            // Check to see if UIView is ready.
            if (UIView.GetAView() != null)
            {
                // It's ready - attach the hook now.
                OptionsPanel.OptionsEventHook();
            }
            else
            {
                // Otherwise, queue the hook for when the intro's finished loading.
                LoadingManager.instance.m_introLoaded += OptionsPanel.OptionsEventHook;
            }
        }
    }
}
