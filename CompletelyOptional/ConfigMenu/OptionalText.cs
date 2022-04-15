using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompletelyOptional
{
    /// <summary>
    /// Storing texts for translation
    /// </summary>
    public static class OptionalText
    {
        internal static void Initialize()
        {
            if (engText != null) { return; }
            engText = new string[Enum.GetNames(typeof(ID)).Length];

            engText[(int)ID.OpCheckBox_MouseTuto] = "Click to toggle";
            engText[(int)ID.OpCheckBox_NonMouseTuto] = "Press JUMP to toggle";
            engText[(int)ID.OpComboBox_MouseOpenTuto] = "Click to open the list";
            engText[(int)ID.OpComboBox_MouseUseTuto] = "Double click the main box to search";
            engText[(int)ID.OpComboBox_MouseSearchTuto] = "Type keyword or initial with keyboard to search";
            engText[(int)ID.OpComboBox_NonMouseOpenTuto] = "Press JUMP to open the list";
            engText[(int)ID.OpComboBox_NonMouseUseTuto] = "Use joystick to scroll, Press JUMP to select";
            engText[(int)ID.OpDragger_MouseTuto] = "Hold mouse button and drag up or down to adjust value";
            engText[(int)ID.OpDragger_NonMouseGrabTuto] = "Press JUMP to interact";
            engText[(int)ID.OpDragger_NonMouseUseTuto] = "Use joystick to adjust, press JUMP to set";
            engText[(int)ID.OpKeyBinder_ErrorConflictOtherModDefault] = "Conflicting default button with <ModID>";
            engText[(int)ID.OpKeyBinder_ErrorConflictVanilla] = "Conflicting button with vanilla control options";
            engText[(int)ID.OpKeyBinder_ErrorConflictOtherMod] = "Conflicting button with <AnotherModID>";
            engText[(int)ID.OpKeyBinder_ErrorConflictCurrMod] = "[<ConflictButton>] button is already in use";
            engText[(int)ID.OpKeyBinder_MouseSelectTuto] = "Click to change button binding";
            engText[(int)ID.OpKeyBinder_MouseBindTuto] = "Press a button to bind (ESC to unbind)";
            engText[(int)ID.OpKeyBinder_MouseJoystickBindTuto] = "Press a button to bind (ESC to unbind, CTRL + no to set controller number)";
            engText[(int)ID.OpKeyBinder_NonMouseSelectTuto] = "Press JUMP to change Button binding";
            engText[(int)ID.OpKeyBinder_NonMouseBindTuto] = "Press a button to bind (SELECT to unbind)";
            engText[(int)ID.OpKeyBinder_NonMouseJoystickBindTuto] = "Press a button to bind (SELECT to unbind, CTRL + No to set controller number)";
            engText[(int)ID.OpRadioButton_MouseTuto] = "Click to choose this option";
            engText[(int)ID.OpRadioButton_NonMouseTuto] = "Press JUMP to choose this option";
            engText[(int)ID.OpSlider_MouseTutoHrzt] = "Hold your mouse button and drag left or right to adjust value";
            engText[(int)ID.OpSlider_MouseTutoVrtc] = "Hold your mouse button and drag up or down to adjust value";
            engText[(int)ID.OpSlider_NonMouseGrabTuto] = "Press JUMP to interact";
            engText[(int)ID.OpSlider_NonMouseAdjustTuto] = "Use joystick to adjust, press JUMP to apply";
            engText[(int)ID.OpTextBox_MouseTutoGrab] = "Click to begin typing";
            engText[(int)ID.OpTextBox_MouseTutoType] = "Use keyboard to type, press Enter to apply";
            engText[(int)ID.OpTextBox_NonMouseTuto] = "This cannot be interacted with a joystick";
            engText[(int)ID.OpColorPicker_MouseTypeTuto] = "Type hex code with keyboard for desired color";
            engText[(int)ID.OpColorPicker_MouseRGBTuto] = "Select color with red, green, blue slider";
            engText[(int)ID.OpColorPicker_MouseHSLTuto] = "Select color with hue saturation square and lightness slider";
            engText[(int)ID.OpColorPicker_MousePLTTuto] = "Select a color from palette";
            engText[(int)ID.OpColorPicker_NonMouseModeSelect] = "Press JUMP to select mode";
            engText[(int)ID.OpColorPicker_NonMouseRGBSliders] = "With joystick, up or down to choose, left or right to adjust RGB. Press JUMP to apply";

            engText[(int)ID.OpScrollBox_MouseTuto] = "Use scroll wheel to see more";
            engText[(int)ID.OpScrollBox_MouseTutoSlidebar] = "Use scroll wheel or scrollbar to see more";
            engText[(int)ID.OpScrollBox_NonMouseTuto] = "Press JUMP and use joystick to see more";
            engText[(int)ID.OpSimpleButton_MouseTuto] = "Click mouse button to trigger";
            engText[(int)ID.OpSimpleButton_NonMouseTuto] = "Press JUMP button to trigger";
            engText[(int)ID.OpHoldButton_MouseTuto] = "Hold mouse button to trigger";
            engText[(int)ID.OpHoldButton_NonMouseTuto] = "Hold JUMP button to trigger";

            engText[(int)ID.OptionsMenu_ConfigButton_Label] = "MOD CONFIG";
            engText[(int)ID.OptionsMenu_ConfigButton_Desc] = "Configure settings for installed mods";
            engText[(int)ID.ConfigMenuTab_BackButton_Label] = "BACK";
            engText[(int)ID.ConfigMenuTab_BackButton_Desc] = "Return to options menu";
            engText[(int)ID.ConfigMenuTab_SaveButton_Label] = "APPLY";
            engText[(int)ID.ConfigMenuTab_SaveButton_Desc] = "Save current mod's configurations";
            engText[(int)ID.ConfigMenuTab_ResetButton_Label] = "RESET CONFIG";
            engText[(int)ID.ConfigMenuTab_ResetButton_Desc] = "Reset current mod's configurations";
            engText[(int)ID.MenuModList_ModButton_Configure] = "Configure <ModID> by <ModAuthor>";
            engText[(int)ID.MenuModList_ModButton_ConfigureAnonymous] = "Configure <ModID>";
            engText[(int)ID.MenuModList_ModButton_Display] = "Display <ModID> by <ModAuthor>";
            engText[(int)ID.MenuModList_ModButton_DisplayAnonymous] = "Display <ModID>";
            engText[(int)ID.MenuModList_ModButton_Blank] = "<ModID> by <ModAuthor>";
            engText[(int)ID.MenuModList_ModButton_Blankest] = "<ModID>";
            engText[(int)ID.MenuModList_ListButton_Stat] = "Display mods statistics";
            engText[(int)ID.MenuModList_ListButton_ScrollUp] = "Press or hold mouse button to scroll the mod List up";
            engText[(int)ID.MenuModList_ListButton_ScrollDw] = "Press or hold mouse button to scroll the mod List down";
            engText[(int)ID.MenuModList_AlphabetButton_MouseDesc] = "Click to jump to mods start with the alphabet <Letter>";
            engText[(int)ID.MenuModList_AlphabetButton_NonMouseDesc] = "Press JUMP to jump to mods start with the alphabet <Letter>";
            engText[(int)ID.MenuModList_ListSlider_Desc] = "Hold your mouse button on the circle and drag up or down to scroll the mod list";
            engText[(int)ID.ConfigTabController_TabSelectButton_UnnamedTab] = "Switch to tab no. <TabIndex>";
            engText[(int)ID.ConfigTabController_TabSelectButton_NamedTab] = "Switch to tab <TabName>";
            engText[(int)ID.ConfigTabController_TabScrollButton_Up] = "Press or hold mouse button to scroll the tab list up";
            engText[(int)ID.ConfigTabController_TabScrollButton_Dw] = "Press or hold mouse button to scroll the tab list down";
        }

        public enum ID : int
        {
            OpCheckBox_MouseTuto,
            OpCheckBox_NonMouseTuto,
            OpComboBox_MouseOpenTuto,
            OpComboBox_MouseUseTuto,
            OpComboBox_MouseSearchTuto,
            OpComboBox_NonMouseOpenTuto,
            OpComboBox_NonMouseUseTuto,
            OpDragger_MouseTuto,
            OpDragger_NonMouseGrabTuto,
            OpDragger_NonMouseUseTuto,
            OpKeyBinder_ErrorConflictOtherModDefault,
            OpKeyBinder_ErrorConflictVanilla,
            OpKeyBinder_ErrorConflictOtherMod,
            OpKeyBinder_ErrorConflictCurrMod,
            OpKeyBinder_MouseSelectTuto,
            OpKeyBinder_MouseBindTuto,
            OpKeyBinder_MouseJoystickBindTuto,
            OpKeyBinder_NonMouseSelectTuto,
            OpKeyBinder_NonMouseBindTuto,
            OpKeyBinder_NonMouseJoystickBindTuto,
            OpRadioButton_MouseTuto,
            OpRadioButton_NonMouseTuto,
            OpSlider_MouseTutoHrzt,
            OpSlider_MouseTutoVrtc,
            OpSlider_NonMouseGrabTuto,
            OpSlider_NonMouseAdjustTuto,
            OpTextBox_MouseTutoGrab,
            OpTextBox_MouseTutoType,
            OpTextBox_NonMouseTuto,
            OpColorPicker_MouseTypeTuto,
            OpColorPicker_MouseRGBTuto,
            OpColorPicker_MouseHSLTuto,
            OpColorPicker_MousePLTTuto,
            OpColorPicker_NonMouseModeSelect,
            OpColorPicker_NonMouseRGBSliders,

            OpScrollBox_MouseTuto,
            OpScrollBox_MouseTutoSlidebar,
            OpScrollBox_NonMouseTuto,
            OpSimpleButton_MouseTuto,
            OpSimpleButton_NonMouseTuto,
            OpHoldButton_MouseTuto,
            OpHoldButton_NonMouseTuto,

            OptionsMenu_ConfigButton_Label,
            OptionsMenu_ConfigButton_Desc,
            ConfigMenuTab_BackButton_Label,
            ConfigMenuTab_BackButton_Desc,
            ConfigMenuTab_SaveButton_Label,
            ConfigMenuTab_SaveButton_Desc,
            ConfigMenuTab_ResetButton_Label,
            ConfigMenuTab_ResetButton_Desc,
            MenuModList_ModButton_Configure,
            MenuModList_ModButton_ConfigureAnonymous,
            MenuModList_ModButton_Display,
            MenuModList_ModButton_DisplayAnonymous,
            MenuModList_ModButton_Blank,
            MenuModList_ModButton_Blankest,
            MenuModList_ListButton_Stat,
            MenuModList_ListButton_ScrollUp,
            MenuModList_ListButton_ScrollDw,
            MenuModList_AlphabetButton_MouseDesc,
            MenuModList_AlphabetButton_NonMouseDesc,
            MenuModList_ListSlider_Desc,
            ConfigTabController_TabSelectButton_UnnamedTab,
            ConfigTabController_TabSelectButton_NamedTab,
            ConfigTabController_TabScrollButton_Up,
            ConfigTabController_TabScrollButton_Dw,
        }

        public static string[] engText;

        public static string GetText(ID id) => InternalTranslator.Translate(engText[(int)id]);
    }
}