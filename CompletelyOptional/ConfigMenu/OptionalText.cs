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

            OpHoldButton_MouseTuto,
            OpHoldButton_NonMouseTuto,

            ConfigTabController_TabSelectButton_UnnamedTab,
            ConfigTabController_TabSelectButton_NamedTab
        }

        public static string[] engText;

        public static string GetText(ID id) => InternalTranslator.Translate(engText[(int)id]);
    }
}