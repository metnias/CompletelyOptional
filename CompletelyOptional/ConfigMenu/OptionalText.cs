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
            engText[(int)ID.OpComboBox_MouseSearchTuto] = "Type keyword or initial with Keyboard to search";
            engText[(int)ID.OpComboBox_NonMouseOpenTuto] = "Press JUMP to open the list";
            engText[(int)ID.OpComboBox_NonMouseUseTuto] = "Use Joystick to scroll, Press JUMP to select";
            engText[(int)ID.OpDragger_MouseTuto] = "Hold Mouse Button and Drag up/down to adjust value";
            engText[(int)ID.OpDragger_NonMouseGrabTuto] = "Press JUMP to interact";
            engText[(int)ID.OpDragger_NonMouseGrabTuto] = "Use Joystick to adjust, Press JUMP to set";
            engText[(int)ID.OpKeyBinder_ErrorConflictOtherModDefault] = "Conflicting Default Button with <ModID>";
            engText[(int)ID.OpKeyBinder_ErrorConflictVanilla] = "Conflicting Button with Vanilla Control Options";
            engText[(int)ID.OpKeyBinder_ErrorConflictOtherMod] = "Conflicting Button with <AnotherModID>";
            engText[(int)ID.OpKeyBinder_ErrorConflictCurrMod] = "[<ConflictButton>] Button is already in use";
            engText[(int)ID.OpKeyBinder_MouseSelectTuto] = "Click to change Button binding";
            engText[(int)ID.OpKeyBinder_MouseBindTuto] = "Press a Button to bind (ESC to unbind)";
            engText[(int)ID.OpKeyBinder_MouseJoystickBindTuto] = "Press a Button to bind (ESC to unbind, Ctrl + No to set controller number)";
            engText[(int)ID.OpKeyBinder_NonMouseSelectTuto] = "Press JUMP to change Button binding";
            engText[(int)ID.OpKeyBinder_NonMouseBindTuto] = "Press a Button to bind (SELECT to unbind)";
            engText[(int)ID.OpKeyBinder_NonMouseJoystickBindTuto] = "Press a Button to bind (SELECT to unbind, Ctrl + No to set controller number)";
            engText[(int)ID.OpRadioButton_MouseTuto] = "Click to choose this option";
            engText[(int)ID.OpRadioButton_NonMouseTuto] = "Press JUMP to choose this option";
            engText[(int)ID.OpSlider_MouseTutoHrzt] = "Hold your Mouse Button and Drag left/right to adjust value";
            engText[(int)ID.OpSlider_MouseTutoVrtc] = "Hold your Mouse Button and Drag up/down to adjust value";
            engText[(int)ID.OpSlider_NonMouseGrabTuto] = "Press JUMP to interact";
            engText[(int)ID.OpSlider_NonMouseAdjustTuto] = "Use Joystick to adjust, Press JUMP to apply";
            engText[(int)ID.OpTextBox_MouseTutoGrab] = "Click to begin typing";
            engText[(int)ID.OpTextBox_MouseTutoType] = "Use Keyboard to type, Press Enter to apply";
            engText[(int)ID.OpTextBox_NonMouseTuto] = "This cannot be interacted with a Joystick";

            engText[(int)ID.OpScrollBox_MouseTuto] = "Use Scroll Wheel to see more";
            engText[(int)ID.OpScrollBox_MouseTutoSlidebar] = "Use Scroll Wheel or Scrollbar to see more";
            engText[(int)ID.OpScrollBox_NonMouseTuto] = "Press JUMP and use Joystick to see more";
            engText[(int)ID.OpSimpleButton_MouseTuto] = "Click Mouse Button to trigger";
            engText[(int)ID.OpSimpleButton_MouseTutoHold] = "Press or Hold Mouse Button to trigger";
            engText[(int)ID.OpSimpleButton_NonMouseTuto] = "Press JUMP Button to trigger";
            engText[(int)ID.OpSimpleButton_NonMouseTutoHold] = "Press or Hold JUMP Button to trigger";
            engText[(int)ID.OpHoldButton_MouseTuto] = "Hold Mouse Button to trigger";
            engText[(int)ID.OpHoldButton_NonMouseTuto] = "Hold JUMP Button to trigger";

            engText[(int)ID.ConfigTabController_TabSelectButton_UnnamedTab] = "Switch to Tab No <TabIndex>";
            engText[(int)ID.ConfigTabController_TabSelectButton_NamedTab] = "Switch to Tab <TabName>";

            engText[(int)ID.ConfigMenuTab_Back] = "BACK";
            engText[(int)ID.ConfigMenuTab_Apply] = "APPLY";
            engText[(int)ID.ConfigMenuTab_Reset] = "RESET CONFIG";
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

            OpScrollBox_MouseTuto,
            OpScrollBox_MouseTutoSlidebar,
            OpScrollBox_NonMouseTuto,
            OpSimpleButton_MouseTuto,
            OpSimpleButton_MouseTutoHold,
            OpSimpleButton_NonMouseTuto,
            OpSimpleButton_NonMouseTutoHold,
            OpHoldButton_MouseTuto,
            OpHoldButton_NonMouseTuto,

            ConfigTabController_TabSelectButton_UnnamedTab,
            ConfigTabController_TabSelectButton_NamedTab,

            ConfigMenuTab_Back,
            ConfigMenuTab_Apply,
            ConfigMenuTab_Reset
        }

        public static string[] engText;

        public static string GetText(ID id) => InternalTranslator.Translate(engText[(int)id]);
    }
}