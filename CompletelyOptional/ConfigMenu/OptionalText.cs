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

            engText[(int)ID.OpCheckBox_MouseTuto] = "Click to Check/Uncheck";
            engText[(int)ID.OpCheckBox_NonMouseTuto] = "Press JUMP to Check/Uncheck";
            engText[(int)ID.OpComboBox_MouseOpenTuto] = "Click to open the list";
            engText[(int)ID.OpComboBox_MouseUseTuto] = "Double click the main box to search";
            engText[(int)ID.OpComboBox_MouseSearchTuto] = "Type keyword or initial with Keyboard to search";
            engText[(int)ID.OpComboBox_NonMouseOpenTuto] = "Press JUMP to open the list";
            engText[(int)ID.OpComboBox_NonMouseUseTuto] = "Use Joystick to scroll, Press JUMP to select";
            engText[(int)ID.OpDragger_MouseTuto] = "Hold Mouse Button and Drag up/down to adjust value";
            engText[(int)ID.OpDragger_NonMouseGrabTuto] = "Press JUMP to grab";
            engText[(int)ID.OpDragger_NonMouseGrabTuto] = "Use Joystick to adjust, Press JUMP to set";
            engText[(int)ID.OpKeyBinder_ErrorConflictOtherModDefault] = "Conflicting Default Button with <ModID>";
            engText[(int)ID.OpKeyBinder_ErrorConflictVanilla] = "Conflicting Button with Vanilla Control Options";
            engText[(int)ID.OpKeyBinder_ErrorConflictOtherMod] = "Conflicting Button with <AnotherModID>";
            engText[(int)ID.OpKeyBinder_ErrorConflictCurrMod] = "[<ConflictButton>] Button is already in use";
            engText[(int)ID.OpKeyBinder_MouseBindTuto] = "Click and Press any Button to bind (ESC to unbind)";
            engText[(int)ID.OpKeyBinder_MouseJoystickBindTuto] = "Click and Press any Button to bind (ESC to unbind, Ctrl + No to set controller number)";
            engText[(int)ID.OpKeyBinder_NonMouseBindTuto] = "Press JUMP and Press any Button to bind (SELECT to unbind)";
            engText[(int)ID.OpKeyBinder_NonMouseJoystickBindTuto] = "Press JUMP and Press any Button to bind (SELECT to unbind, Ctrl + No to set controller number)";
            engText[(int)ID.OpRadioButton_MouseTuto] = "Click to choose this option";
            engText[(int)ID.OpRadioButton_NonMouseTuto] = "Press JUMP to choose this option";

            engText[(int)ID.OpHoldButton_MouseTuto] = "Hold Mouse Button to trigger";
            engText[(int)ID.OpHoldButton_NonMouseTuto] = "Hold JUMP Button to trigger";

            engText[(int)ID.ConfigTabController_TabSelectButton_UnnamedTab] = "Switch to Tab No <TabIndex>";
            engText[(int)ID.ConfigTabController_TabSelectButton_NamedTab] = "Switch to Tab <TabName>";
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
            OpKeyBinder_MouseBindTuto,
            OpKeyBinder_MouseJoystickBindTuto,
            OpKeyBinder_NonMouseBindTuto,
            OpKeyBinder_NonMouseJoystickBindTuto,
            OpRadioButton_MouseTuto,
            OpRadioButton_NonMouseTuto,

            OpHoldButton_MouseTuto,
            OpHoldButton_NonMouseTuto,

            ConfigTabController_TabSelectButton_UnnamedTab,
            ConfigTabController_TabSelectButton_NamedTab
        }

        public static string[] engText;

        public static string GetText(ID id) => InternalTranslator.Translate(engText[(int)id]);
    }
}