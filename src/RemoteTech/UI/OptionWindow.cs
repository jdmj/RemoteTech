﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.Localization;


namespace RemoteTech.UI
{
    class OptionWindow : AbstractWindow
    {
        #region Member
        /// <summary>Defines the option window width</summary>
        const uint WINDOW_WIDTH = 430;
        /// <summary>Defines the option window height</summary>
        const uint WINDOW_HEIGHT = 320;

        /// <summary>Option menu items</summary>
        public enum OPTION_MENUS
        {
            Start = 0,
            Presets,
            WorldScale,
            AlternativeRules,
            VisualStyle,
            Miscellaneous,
            Cheats
        }
        
        /// <summary>Small gray hint text color</summary>
        private GUIStyle mGuiHintText;
        /// <summary>Small white running text color</summary>
        private GUIStyle mGuiRunningText;
        /// <summary>Textstyle for list entrys</summary>
        private GUIStyle mGuiListText;
        /// <summary>Button style for list entrys</summary>
        private GUIStyle mGuiListButton;
        /// <summary>Texture to represent the dish color</summary>
        private Texture2D mVSColorDish;
        /// <summary>Texture to represent the omni color</summary>
        private Texture2D mVSColorOmni;
        /// <summary>Texture to represent the active color</summary>
        private Texture2D mVSColorActive;
        /// <summary>Texture to represent the remote station color</summary>
        private Texture2D mVSColorRemoteStation;
        /// <summary>Toggles the color slider for the dish color</summary>
        private bool dishSlider = false;
        /// <summary>Toggles the color slider for the omni color</summary>
        private bool omniSlider = false;
        /// <summary>Toggles the color slider for the active color</summary>
        private bool activeSlider = false;
        /// <summary>Toggles the color slider for the remote station color</summary>
        private bool remoteStationSlider = false;
        /// <summary>HeadlineImage</summary>
        private Texture2D mTexHeadline;
        /// <summary>Positionvector for the content scroller</summary>
        private Vector2 mOptionScrollPosition;
        /// <summary>Reference to the RTSettings</summary>
        private Settings mSettings { get { return RTSettings.Instance; } }
        /// <summary>Current selected menu item</summary>
        private int mMenuValue;
        #endregion

        #region AbstractWindow-Definitions

        public OptionWindow()
            : base(new Guid("387AEB5A-D29C-485B-B96F-CA575E776940"), Localizer.Format("#RT_OptionWindow_title",RTUtil.Version),//"RemoteTech " +  + " Options"
                   new Rect(Screen.width / 2 - (OptionWindow.WINDOW_WIDTH / 2), Screen.height / 2 - (OptionWindow.WINDOW_HEIGHT / 2), OptionWindow.WINDOW_WIDTH, OptionWindow.WINDOW_HEIGHT), WindowAlign.Floating)
        {

            this.mMenuValue = (int)OPTION_MENUS.Start;
            this.mCloseButton = false;
            this.initalAssets();
        }

        public override void Hide()
        {
            RTSettings.Instance.Save();

            // Set the AppLauncherbutton to false
            if(RemoteTech.RTSpaceCentre.LauncherButton != null)
            {
                RemoteTech.RTSpaceCentre.LauncherButton.SetFalse();
            }

            base.Hide();
        }
        
        #endregion

        #region Base-drawing
        /// <summary>
        /// Draws the content of the window
        /// </summary>
        public override void Window(int uid)
        {
            // push the current GUI.skin
            var pushSkin = GUI.skin;
            GUI.skin = HighLogic.Skin;

            GUILayout.BeginVertical(GUILayout.Width(OptionWindow.WINDOW_WIDTH), GUILayout.Height(OptionWindow.WINDOW_HEIGHT));
            {
                // Header image
                GUI.DrawTexture(new Rect(16, 25, OptionWindow.WINDOW_WIDTH - 14, 70), this.mTexHeadline);
                GUILayout.Space(70);

                this.drawOptionMenu();
                this.drawOptionContent();
            }
            GUILayout.EndVertical();

            if (GUILayout.Button(Localizer.Format("#RT_OptionWindow_closebutton")))//"Close"
            {
                this.Hide();
                RTSettings.OnSettingsChanged.Fire();
            }

            base.Window(uid);

            // pop back the saved skin
            GUI.skin = pushSkin;
        }
        #endregion

        /// <summary>
        /// Initializes the styles and assets
        /// </summary>
        private void initalAssets()
        {
            // initial styles
            this.mGuiHintText = new GUIStyle(HighLogic.Skin.label)
            {
                fontSize = 11,
                normal = { textColor = XKCDColors.Grey }
            };

            this.mGuiRunningText = new GUIStyle(HighLogic.Skin.label)
            {
                fontSize = 13,
                normal = { textColor = Color.white }
            };

            this.mGuiListText = new GUIStyle(HighLogic.Skin.label)
            {
                fontSize = 12,
            };

            this.mGuiListButton = new GUIStyle(HighLogic.Skin.button)
            {
                fontSize = 12,
                normal = { textColor = Color.white }
            };

            // initial Textures
            mTexHeadline = RTUtil.LoadImage("headline");
            // Visual style colors
            this.loadColorTexture(out this.mVSColorDish, this.mSettings.DishConnectionColor);
            this.loadColorTexture(out this.mVSColorOmni, this.mSettings.OmniConnectionColor);
            this.loadColorTexture(out this.mVSColorActive, this.mSettings.ActiveConnectionColor);
            this.loadColorTexture(out this.mVSColorRemoteStation, this.mSettings.RemoteStationColorDot);
        }

        /// <summary>
        /// Draws the option menu
        /// </summary>
        private void drawOptionMenu()
        {
            GUILayout.BeginHorizontal();
            {
                // push the font size of buttons
                var pushFontsize = GUI.skin.button.fontSize;
                GUI.skin.button.fontSize = 11;

                foreach (OPTION_MENUS menu in Enum.GetValues(typeof(OPTION_MENUS)))
                {
                    RTUtil.FakeStateButton(new GUIContent(menu.ToString()), () => this.mMenuValue = (int)menu, this.mMenuValue, (int)menu, GUILayout.Height(16));
                }

                // pop the saved button size back
                GUI.skin.button.fontSize = pushFontsize;
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw current GUI content based on the user selection
        /// </summary>
        private void drawOptionContent()
        {
            GUILayout.BeginHorizontal();
            {
                this.mOptionScrollPosition = GUILayout.BeginScrollView(this.mOptionScrollPosition);
                {
                    switch ((OPTION_MENUS)this.mMenuValue)
                    {
                        case OPTION_MENUS.WorldScale:
                            {
                                this.drawWorldScaleContent();
                                break;
                            }
                        case OPTION_MENUS.AlternativeRules:
                            {
                                this.drawAlternativeRulesContent();
                                break;
                            }
                        case OPTION_MENUS.VisualStyle:
                            {
                                this.drawVisualStyleContent();
                                break;
                            }
                        case OPTION_MENUS.Miscellaneous:
                            {
                                this.drawMiscellaneousContent();
                                break;
                            }
                        case OPTION_MENUS.Presets:
                            {
                                this.drawPresetsContent();
                                break;
                            }
                        case OPTION_MENUS.Cheats:
                            {
                                this.drawCheatContent();
                                break;
                            }
                        case OPTION_MENUS.Start:
                        default:
                            {
                                this.drawStartContent();
                                break;
                            }
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the content of the Start section
        /// </summary>
        private void drawStartContent()
        {
            GUILayout.Space(10);
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_start_Text"), this.mGuiRunningText);//"Use the small menu buttons above to navigate through the different options."

            /* Commented out because there is an issue of conflicts between RT and CommNet (stock and RT modules in each antenna) and RT is moving into CommNet inevitably
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(90);
                this.mSettings.RemoteTechEnabled = GUILayout.Toggle(this.mSettings.RemoteTechEnabled, (this.mSettings.RemoteTechEnabled) ? "RemoteTech enabled" : "RemoteTech disabled");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(90);
                this.mSettings.CommNetEnabled = (HighLogic.fetch.currentGame.Parameters.Difficulty.EnableCommNet = GUILayout.Toggle(this.mSettings.CommNetEnabled, (this.mSettings.CommNetEnabled) ? "CommNet enabled" : "CommNet disabled"));
            }
            GUILayout.EndHorizontal();
            */

            GUILayout.Space(90);
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_start_Text2"), this.mGuiRunningText);//"Need some help with RemoteTech?  Check out the online manual and tutorials.  If you can't find your answer, post in the forum thread.\n(Browser opens on click)"
            GUILayout.BeginHorizontal();
            {
                if(GUILayout.Button(Localizer.Format("#RT_OptionWindow_start_button1")))//"Online Manual and Tutorials"
                {
                    Application.OpenURL("http://remotetechnologiesgroup.github.io/RemoteTech/");
                }
                if(GUILayout.Button(Localizer.Format("#RT_OptionWindow_start_button2")))//"KSP Forum"
                {
                    Application.OpenURL("http://forum.kerbalspaceprogram.com/threads/83305");
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the content of the WorldScale section
        /// </summary>
        private void drawWorldScaleContent()
        {
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_WorldScale_ConsMulti_head",this.mSettings.ConsumptionMultiplier), GUILayout.Width(OptionWindow.WINDOW_WIDTH * 0.75f));//"Consumption Multiplier: (" +  + ")"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_WorldScale_ConsMulti_text"), this.mGuiHintText);//"If set to a value other than 1, the power consumption of all antennas will be increased or decreased by this factor.\nDoes not affect energy consumption for science transmissions."
            this.mSettings.ConsumptionMultiplier = (float)Math.Round(GUILayout.HorizontalSlider(this.mSettings.ConsumptionMultiplier, 0, 2), 2);

            // Re-scaling Kerbin by 10.625x to create Earth-like planet size requires Multipliers of 10 for orbital satellite network with full coverage to be practical
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_WorldScale_RangeMulti_head", this.mSettings.RangeMultiplier), GUILayout.Width(OptionWindow.WINDOW_WIDTH * 0.75f));//"Antennas Range Multiplier: (" +  + ")"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_WorldScale_RangeMulti_text"), this.mGuiHintText);//"If set to a value other than 1, the range of all <b><color=#bada55>antennas</color></b> will be increased or decreased by this factor.\nDoes not affect Mission Control range."
            mSettings.RangeMultiplier = (float)Math.Round(GUILayout.HorizontalSlider(mSettings.RangeMultiplier, 0, 10), 2);

            GUILayout.Label(Localizer.Format("#RT_OptionWindow_WorldScale_MissionControlRangeMulti_head",mSettings.MissionControlRangeMultiplier), GUILayout.Width(OptionWindow.WINDOW_WIDTH * 0.75f));//"Mission Control Range Multiplier: (" +  + ")"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_WorldScale_MissionControlRangeMulti_text"), this.mGuiHintText);//"If set to a value other than 1, the range of all <b><color=#bada55>Mission Controls</color></b> will be increased or decreased by this factor.\nDoes not affect antennas range."
            mSettings.MissionControlRangeMultiplier = (float)Math.Round(GUILayout.HorizontalSlider(mSettings.MissionControlRangeMultiplier, 0, 10), 2);
        }

        /// <summary>
        /// Draws the content of the AlternativeRules section
        /// </summary>
        private void drawAlternativeRulesContent()
        {
            GUILayout.Space(10);
            this.mSettings.EnableSignalDelay = GUILayout.Toggle(this.mSettings.EnableSignalDelay, (this.mSettings.EnableSignalDelay) ? Localizer.Format("#RT_OptionWindow_AlternativeRules_EnableSignalDelay") : Localizer.Format("#RT_OptionWindow_AlternativeRules_disabledSignalDelay"));//"Signal delay enabled""Signal delay disabled"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_AlternativeRules_SignalDelaydesc"), this.mGuiHintText);//"ON: All commands sent to RemoteTech-compatible probe cores are limited by the speed of light and have a delay before executing, based on distance.\nOFF: All commands will be executed immediately, although a working connection to Mission Control is still required."

            GUILayout.Label(Localizer.Format("#RT_OptionWindow_AlternativeRules_RangeModelMode_head"), GUILayout.Width(OptionWindow.WINDOW_WIDTH * 0.75f));//"Range Model Mode"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_AlternativeRules_RangeModelMode_text"), this.mGuiHintText);//"This setting controls how the game determines whether two antennas are in range of each other.\nRead more on our online manual about the difference for each rule."
            GUILayout.BeginHorizontal();
            {
                RTUtil.FakeStateButton(new GUIContent(Localizer.Format("#RT_OptionWindow_AlternativeRules_RangeModelMode_Standard")), () => this.mSettings.RangeModelType = RangeModel.RangeModel.Standard, (int)this.mSettings.RangeModelType, (int)RangeModel.RangeModel.Standard, GUILayout.Height(20));//"Standard"
                RTUtil.FakeStateButton(new GUIContent(Localizer.Format("#RT_OptionWindow_AlternativeRules_RangeModelMode_Root")), () => this.mSettings.RangeModelType = RangeModel.RangeModel.Additive, (int)this.mSettings.RangeModelType, (int)RangeModel.RangeModel.Additive, GUILayout.Height(20));//"Root"
            }
            GUILayout.EndHorizontal();

            GUILayout.Label(Localizer.Format("#RT_OptionWindow_AlternativeRules_MultipleAntennaMulti_head",this.mSettings.MultipleAntennaMultiplier), GUILayout.Width(OptionWindow.WINDOW_WIDTH * 0.75f));//"Multiple Antenna Multiplier : (" +  + ")"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_AlternativeRules_MultipleAntennaMulti_text"), this.mGuiHintText);//"Multiple omnidirectional antennas on the same craft work together.\nThe default value of 0 means this is disabled.\nThe largest value of 1.0 sums the range of all omnidirectional antennas to provide a greater effective range.\nThe effective range scales linearly and this option works with both the Standard and Root range models."
            this.mSettings.MultipleAntennaMultiplier = Math.Round(GUILayout.HorizontalSlider((float)mSettings.MultipleAntennaMultiplier, 0, 1), 2);
        }

        /// <summary>
        /// Draws the content of the VisualStyle section
        /// </summary>
        private void drawVisualStyleContent()
        {
            
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Localizer.Format("#RT_OptionWindow_VisualStyle_DishColor"), GUILayout.Width(OptionWindow.WINDOW_WIDTH * 0.75f));//"Dish Connection Color:"
                if(GUILayout.Button(this.mVSColorDish, GUILayout.Width(18)))
                {
                    this.dishSlider = !this.dishSlider;
                }
            }
            GUILayout.EndHorizontal();

            if (this.dishSlider)
            {
                this.mSettings.DishConnectionColor = this.drawColorSlider(this.mSettings.DishConnectionColor);
                this.loadColorTexture(out this.mVSColorDish, this.mSettings.DishConnectionColor);
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Localizer.Format("#RT_OptionWindow_VisualStyle_OmniColor"), GUILayout.Width(OptionWindow.WINDOW_WIDTH * 0.75f));//"Omni Connection Color:"
                if(GUILayout.Button(this.mVSColorOmni, GUILayout.Width(18)))
                {
                    this.omniSlider = !this.omniSlider;
                }
            }
            GUILayout.EndHorizontal();

            if (this.omniSlider)
            {
                this.mSettings.OmniConnectionColor = this.drawColorSlider(this.mSettings.OmniConnectionColor);
                this.loadColorTexture(out this.mVSColorOmni, this.mSettings.OmniConnectionColor);
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Localizer.Format("#RT_OptionWindow_VisualStyle_ActiveColor"), GUILayout.Width(OptionWindow.WINDOW_WIDTH * 0.75f));//"Active Connection Color:"
                if(GUILayout.Button(this.mVSColorActive, GUILayout.Width(18)))
                {
                    this.activeSlider = !this.activeSlider;
                }
            }
            GUILayout.EndHorizontal();

            if (this.activeSlider)
            {
                this.mSettings.ActiveConnectionColor = this.drawColorSlider(this.mSettings.ActiveConnectionColor);
                this.loadColorTexture(out this.mVSColorActive, this.mSettings.ActiveConnectionColor);
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Localizer.Format("#RT_OptionWindow_VisualStyle_RemoteStationColor"), GUILayout.Width(OptionWindow.WINDOW_WIDTH * 0.75f));//"Remote Station Color:"
                if (GUILayout.Button(this.mVSColorRemoteStation, GUILayout.Width(18)))
                {
                    this.remoteStationSlider = !this.remoteStationSlider;
                }
            }
            GUILayout.EndHorizontal();

            if (this.remoteStationSlider)
            {
                this.mSettings.RemoteStationColorDot = this.drawColorSlider(this.mSettings.RemoteStationColorDot);
                this.loadColorTexture(out this.mVSColorRemoteStation, this.mSettings.RemoteStationColorDot);
            }

            GUILayout.Space(10);
            GUILayout.BeginScrollView(new Vector2(), false, false, GUILayout.Height(10));
            { }
            GUILayout.EndScrollView();
            GUILayout.Space(10);

            this.mSettings.HideGroundStationsBehindBody = GUILayout.Toggle(this.mSettings.HideGroundStationsBehindBody, (this.mSettings.HideGroundStationsBehindBody) ?  Localizer.Format("#RT_OptionWindow_VisualStyle_StationsBehindBodyHide") : Localizer.Format("#RT_OptionWindow_VisualStyle_StationsBehindBodyShow"));//"Ground Stations are hidden behind bodies""Ground Stations always shown"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_VisualStyle_StationsBehindBodytext"), this.mGuiHintText);//"ON: Ground Stations are occluded by the planet or body, and are not visible behind it.\nOFF: Ground Stations are always shown (see range option below)."

            this.mSettings.HideGroundStationsOnDistance = GUILayout.Toggle(this.mSettings.HideGroundStationsOnDistance, (this.mSettings.HideGroundStationsOnDistance) ? Localizer.Format("#RT_OptionWindow_VisualStyle_StationsOnDistancehide") : Localizer.Format("#RT_OptionWindow_VisualStyle_StationsOnDistanceShow"));//"Ground Stations are hidden at a defined distance""Ground Stations always shown"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_VisualStyle_StationsOnDistanceText"), this.mGuiHintText);//"ON: Ground Stations will not be shown past a defined distance to the mapview camera.\nOFF: Ground Stations are shown regardless of distance."

            this.mSettings.ShowMouseOverInfoGroundStations = GUILayout.Toggle(this.mSettings.ShowMouseOverInfoGroundStations, (this.mSettings.ShowMouseOverInfoGroundStations) ? Localizer.Format("#RT_OptionWindow_VisualStyle_MouseOverInfoGroundStationsE") : Localizer.Format("#RT_OptionWindow_VisualStyle_MouseOverInfoGroundStationsD"));//"Mouseover of Ground Stations enabled""Mouseover of Ground Stations disabled"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_VisualStyle_MouseOverInfoGroundStationsDesc"), this.mGuiHintText);//"ON: Some useful information is shown when you mouseover a Ground Station on the map view or Tracking Station.\nOFF: Information isn't shown during mouseover."
        }

        /// <summary>
        /// Draws the content of the Miscellaneous section
        /// </summary>
        private void drawMiscellaneousContent()
        {
            GUILayout.Space(10);
            this.mSettings.ThrottleTimeWarp = GUILayout.Toggle(this.mSettings.ThrottleTimeWarp, (this.mSettings.ThrottleTimeWarp) ? Localizer.Format("#RT_OptionWindow_Miscellaneous_ThrottleTimeWarp") : Localizer.Format("#RT_OptionWindow_Miscellaneous_NoThrottleTimeWarp"));//"RemoteTech will throttle time warp""RemoteTech will not throttle time warp"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_Miscellaneous_ThrottleTimeWarp_text"), this.mGuiHintText);//"ON: The flight computer will automatically stop time warp a few seconds before executing a queued command.\nOFF: The player is responsible for controlling time warp during scheduled actions."

            this.mSettings.ThrottleZeroOnNoConnection = GUILayout.Toggle(this.mSettings.ThrottleZeroOnNoConnection, (this.mSettings.ThrottleZeroOnNoConnection) ? Localizer.Format("#RT_OptionWindow_Miscellaneous_ThrottleZeroOnNoConnection") : Localizer.Format("#RT_OptionWindow_Miscellaneous_NoThrottleZeroOnNoConnection"));//"Throttle to zero on loss of connection""Throttle unaffected by loss of connection"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_Miscellaneous_ThrottleZeroOnNoConnection_text"), this.mGuiHintText);//"ON: The flight computer cuts the thrust if you lose connection to Mission Control.\nOFF: The throttle is not adjusted automatically."

            this.mSettings.StopTimeWrapOnReConnection = GUILayout.Toggle(this.mSettings.StopTimeWrapOnReConnection, (this.mSettings.StopTimeWrapOnReConnection) ? Localizer.Format("#RT_OptionWindow_Miscellaneous_StopTimeWrapOnReConnection") : Localizer.Format("#RT_OptionWindow_Miscellaneous_NoStopTimeWrapOnReConnection"));//"Stop time wrap on reconnection""Time wrap uninterrupted by reconnection"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_Miscellaneous_StopTimeWrapOnReConnection_text"), this.mGuiHintText);//"ON: The flight computer will automatically stop time warp when a connection is found. \nOFF: The time wrap will not be stopped when a connection is found."

            this.mSettings.UpgradeableMissionControlAntennas = GUILayout.Toggle(this.mSettings.UpgradeableMissionControlAntennas, (this.mSettings.UpgradeableMissionControlAntennas) ? Localizer.Format("#RT_OptionWindow_Miscellaneous_UpgradeableMissionControl") : Localizer.Format("#RT_OptionWindow_Miscellaneous_NoUpgradeableMissionControl"));//"Mission Control antennas are upgradeable""Mission Control antennas are not upgradeable"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_Miscellaneous_UpgradeableMissionControlText"), this.mGuiHintText);//"ON: Mission Control antenna range is upgraded when the Tracking Center is upgraded.\nOFF: Mission Control antenna range isn't upgradeable."

            this.mSettings.AutoInsertKaCAlerts = GUILayout.Toggle(this.mSettings.AutoInsertKaCAlerts, (this.mSettings.AutoInsertKaCAlerts) ? Localizer.Format("#RT_OptionWindow_Miscellaneous_AutoInsertKaCAlerts") : Localizer.Format("#RT_OptionWindow_Miscellaneous_NotAutoInsertKaCAlerts"));//"Alarms added to Kerbal Alarm Clock""No alarms added to Kerbal Alarm Clock"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_Miscellaneous_AutoInsertKaCAlertsText"), this.mGuiHintText);//"ON: The flight computer will automatically add alarms to the Kerbal Alarm Clock mod for burn and maneuver commands.  The alarm goes off 3 minutes before the command executes.\nOFF: No alarms are added to Kerbal Alarm Clock"
        }

        /// <summary>
        /// Draws the content of the Presets section
        /// </summary>
        private void drawPresetsContent()
        {
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_Presets_HelpText"), this.mGuiRunningText);//"You can revert your current settings to the starting settings, constructed from installed mods' MM patches. Also, you can reload your current settings with a third-party mod's own RemoteTech settings (the fallback in the event of no MM patch).\n\nHere you can see what presets are available:"
            GUILayout.Space(15);

            List<String> presetList = this.mSettings.PreSets;

            if(this.mSettings.PreSets.Count <= 0)
            {
                GUILayout.Label(Localizer.Format("#RT_OptionWindow_Presets_NotFound"), this.mGuiRunningText);//"No presets are found"
            }

            for(int i = presetList.Count - 1; i >= 0; --i)
            {
                GUILayout.BeginHorizontal("box", GUILayout.MaxHeight(15));
                {
                    string folderName = presetList[i];

                    //remove the node name
                    int index = folderName.LastIndexOf("/RemoteTechSettings");
                    folderName = folderName.Substring(0, index) + folderName.Substring(index).Replace("/RemoteTechSettings", "").Trim();

                    //change default name to better name for MM-patched settings
                    index = folderName.LastIndexOf("/Default_Settings");
                    if(index>=0)
                        folderName = folderName.Substring(0, index) + " " + folderName.Substring(index).Replace("/Default_Settings", "starting settings");

                    GUILayout.Space(15);
                    GUILayout.Label(folderName, this.mGuiListText, GUILayout.ExpandWidth(true));
                    if(GUILayout.Button(Localizer.Format("#RT_OptionWindow_Presets_Reload"), this.mGuiListButton, GUILayout.Width(70), GUILayout.Height(20)))//"Reload"
                    {
                        RTSettings.ReloadSettings(this.mSettings, presetList[i]);
                        ScreenMessages.PostScreenMessage(Localizer.Format("#RT_OptionWindow_Presets_msg1",folderName), 10);//string.Format("Your RemoteTech settings are set to {0}", )
                        RTLog.Notify("Overwrote current settings with this cfg {0}", RTLogLevel.LVL3, presetList[i]);
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Draws the content of the Cheat section
        /// </summary>
        private void drawCheatContent()
        {
            GUILayout.Space(10);
            this.mSettings.ControlAntennaWithoutConnection = GUILayout.Toggle(this.mSettings.ControlAntennaWithoutConnection, (this.mSettings.ControlAntennaWithoutConnection) ? Localizer.Format("#RT_OptionWindow_Cheat_ControlAntennaWithoutConnection") : Localizer.Format("#RT_OptionWindow_Cheat_ControlAntennaNeedConnection"));//"No Connection needed to control antennas""Connection is needed to control antennas"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_Cheat_ControlAntennaWithoutConnectionText"), this.mGuiHintText);//"ON: antennas can be activated, deactivated and targeted without a connection.\nOFF: No control without a working connection."

            GUILayout.Space(10);
            this.mSettings.IgnoreLineOfSight = GUILayout.Toggle(this.mSettings.IgnoreLineOfSight, (this.mSettings.IgnoreLineOfSight) ? Localizer.Format("#RT_OptionWindow_Cheat_IgnoreLineOfSight") : Localizer.Format("#RT_OptionWindow_Cheat_NoIgnoreLineOfSight"));//"Planets and moons will not block a signal""Planets and moons will block a signal"
            GUILayout.Label(Localizer.Format("#RT_OptionWindow_Cheat_IgnoreLineOfSightText"), this.mGuiHintText);//"ON: Antennas and dishes will not need line-of-sight to maintain a connection, as long as they have adequate range and power.\nOFF: Antennas and dishes need line-of-sight to maintain a connection."
        }

        /// <summary>
        /// Paint the texture with given color
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="colorTex"></param>
        private void loadColorTexture(out Texture2D tex, Color colorTex)
        {
            tex = new Texture2D(16, 16);
            tex.SetPixels32(Enumerable.Repeat((Color32)colorTex, 16 * 16).ToArray());
            tex.Apply();
        }

        /// <summary>
        /// Draw RBG color sliders
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private Color drawColorSlider(Color value)
        {
            GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                    GUILayout.Label(Localizer.Format("#RT_OptionWindow_ColorSlider_Red",(int)(value.r * 255)), this.mGuiHintText, GUILayout.Width(OptionWindow.WINDOW_WIDTH * 0.25f));//"Red: ("+ +")"
                    value.r = GUILayout.HorizontalSlider(value.r, 0, 1);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                    GUILayout.Label(Localizer.Format("#RT_OptionWindow_ColorSlider_Green",(int)(value.g * 255)), this.mGuiHintText, GUILayout.Width(OptionWindow.WINDOW_WIDTH * 0.25f));//"Green: (" +  + ")"
                    value.g = GUILayout.HorizontalSlider(value.g, 0, 1);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                    GUILayout.Label(Localizer.Format("#RT_OptionWindow_ColorSlider_Blue", (int)(value.b * 255)), this.mGuiHintText, GUILayout.Width(OptionWindow.WINDOW_WIDTH * 0.25f));//"Blue: (" +  + ")"
                    value.b = GUILayout.HorizontalSlider(value.b, 0, 1);
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            return value;
        }
    }
}