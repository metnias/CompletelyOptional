using CompletelyOptional;
using Menu;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Contains all UIelement, connects UIelements and Rain World Menu
    /// </summary>
    public class ConfigContainer : Menu.MenuObject
    {
        public ConfigContainer(Menu.Menu menu, MenuObject owner) : base(menu, owner)
        {
            menuTab = new MenuTab();
            _soundFill = 0;
            lastFocusedElement = menuTab.backButton;
            focusedElement = menuTab.backButton;
            history = new Stack<ConfigHistory>();
        }

        internal MenuTab menuTab;
        internal OpTab activeTab;
        private int scrollInitDelay;
        private int scrollDelay;

        public bool holdElement;

        private List<UIelement> focusables
        {
            get
            {
                List<UIelement> list = new List<UIelement>();
                foreach (UIelement item in this.menuTab.focusables)
                { if (!item.isInactive) { list.Add(item); } }
                if (this.activeTab != null)
                {
                    foreach (UIelement item in this.activeTab.focusables)
                    { if (!item.isInactive) { list.Add(item); } }
                }

                List<UIelement> res = new List<UIelement>();
                if (menu.manager.menuesMouseMode)
                {
                    foreach (UIelement item in list)
                    { if (!(item is OpScrollBox)) { res.Add(item); } }
                }
                else
                {
                    if (this.focusedElement == null || !this.focusedElement.inScrollBox)
                    {// Candidates are items that aren't in scrollBox
                        foreach (UIelement item in list)
                        { if (!item.inScrollBox) { res.Add(item); } }
                    }
                    else
                    {
                        foreach (UIelement item in list)
                        { if (item.inScrollBox && item.scrollBox == this.focusedElement.scrollBox) { res.Add(item); } }
                    }
                }
                return res;
            }
        }

        // Called by ModConfigMenu.GrafUpdate
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            menuTab.GrafUpdate(timeStacker);
            if (activeTab != null) { activeTab.GrafUpdate(timeStacker); }
        }

        // Called by ModConfigMenu.Update
        public override void Update()
        {
            base.Update();
            _soundFill = _soundFill > 0 ? _soundFill - 1 : 0;
            if (menu.ForceNoMouseMode) { this.focusedElement = null; return; } // == FreezeMenuFunctions
            if (menu.manager.menuesMouseMode)
            { // Mouse Mode
                if (!holdElement)
                {
                    UIelement lastFocus = this.focusedElement;
                    this.focusedElement = null;
                    List<UIelement> list = this.focusables;
                    for (int j = 0; j < list.Count; j++)
                    {
                        if ((list[j] as FocusableUIelement).CurrentlyFocusableMouse && list[j].MouseOver)
                        {
                            this.focusedElement = list[j];
                            if (this.focusedElement != lastFocus)
                            {
                                PlaySound((this.focusedElement as FocusableUIelement).GreyedOut
                                    ? SoundID.MENU_Greyed_Out_Button_Select_Mouse : SoundID.MENU_Button_Select_Mouse);
                            }
                            break;
                        }
                    }
                }
            }
            else
            { // Controller/Keyboard Mode
                if (!this.holdElement)
                {
                    if (menu.input.pckp && !menu.lastInput.pckp) // Hold Element
                    { this.holdElement = true; }
                    else // Switch Focus
                    {
                        if (menu.input.y != 0 && menu.lastInput.y != menu.input.y)
                        { this.FocusNewElementInDirection(new IntVector2(0, menu.input.y)); }
                        else if (menu.input.x != 0 && menu.lastInput.x != menu.input.x)
                        { this.FocusNewElementInDirection(new IntVector2(menu.input.x, 0)); }
                        if (menu.input.y != 0 && menu.lastInput.y == menu.input.y && menu.input.x == 0)
                        { this.scrollInitDelay++; }
                        else if (menu.input.x != 0 && menu.lastInput.x == menu.input.y && menu.input.y == 0)
                        { this.scrollInitDelay++; }
                        else
                        { this.scrollInitDelay = 0; }
                        if (this.scrollInitDelay > 20)
                        {
                            this.scrollDelay++;
                            if (this.scrollDelay > 6)
                            {
                                this.scrollDelay = 0;
                                if (menu.input.y != 0 && menu.lastInput.y == menu.input.y)
                                { this.FocusNewElementInDirection(new IntVector2(0, menu.input.y)); }
                                else if (menu.input.x != 0 && menu.lastInput.x == menu.input.x)
                                { this.FocusNewElementInDirection(new IntVector2(menu.input.x, 0)); }
                            }
                        }
                        else { this.scrollDelay = 0; }
                    }
                }
                else
                {
                    if (menu.input.thrw && !menu.lastInput.thrw && focusedElement != null) // Move Upward
                    {
                        bool moved = false;
                        if (focusedElement.inScrollBox) // Move to ScrollBox
                        {
                            focusedElement = focusedElement.scrollBox;
                        }
                        else if (!(focusedElement.tab is MenuTab)) // Move to TabController
                        {
                            if (this.menuTab.tabCtrler.tabCount > 1) { focusedElement = this.menuTab.tabCtrler; }
                            else { focusedElement = this.menuTab.modList; }
                        }
                        else if (focusedElement is ConfigTabController) // Move to Mod List
                        {
                            focusedElement = this.menuTab.modList;
                        }
                        else // Move to Save button
                        {
                            focusedElement = this.menuTab.saveButton;
                        }
                        if (moved)
                        {
                            PlaySound((focusedElement as FocusableUIelement).GreyedOut
                                ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
                        }
                    }
                    if (menu.input.mp && !menu.lastInput.mp && focusedElement is MenuModList) // Starred
                    {
                    }
                }
            }
            menuTab.Update();
            if (activeTab != null) { activeTab.Update(); }
        }

        private UIelement focusedElement;
        private UIelement lastFocusedElement;

        /// <summary>
        /// Change <see cref="focusedElement"/>
        /// </summary>
        /// <param name="element"><see cref="FocusableUIelement"/> for new focus</param>
        public void FocusNewElement(UIelement element)
        {
            if (!(element is FocusableUIelement))
            { ComOptPlugin.LogWarning($"{element.GetType()} is not FocusableUIelement. FocusNewElement ignored."); return; }
            if (element != null && element != this.focusedElement)
            {
                this.focusedElement = element;
                PlaySound((this.focusedElement as FocusableUIelement).GreyedOut
                    ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
                // Always play Gamepad sound even in Mouse mode, as this is called by either Gamepad or Modder
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="direction"></param>
        internal void FocusNewElementInDirection(IntVector2 direction)
        {
            UIelement element = this.FocusCandidate(direction);
            this.FocusNewElement(element);
        }

        private UIelement FocusCandidate(IntVector2 direction)
        {
            if (this.focusedElement == null)
            { // current mod button
                return this.activeTab.focusables[0];
            }
            if (!(this.focusedElement is FocusableUIelement))
            {
                return this.lastFocusedElement;
            }
            UIelement result = this.lastFocusedElement;
            Vector2 curCenter = this.focusedElement.CenterPos();
            List<UIelement> candidates = this.focusables;
            float likelihood = float.MaxValue;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i] is FocusableUIelement
                    && (candidates[i] as FocusableUIelement).CurrentlyFocusableNonMouse
                    && (candidates[i] as FocusableUIelement) != this.focusedElement)
                {
                    Vector2 cndCenter = candidates[i].CenterPos();
                    // if (direction.y == 0 || cndCenter.y < curCenter.y != direction.y < 0)
                    // {
                    //     bool flag = direction.x != 0 && cndCenter.x < curCenter.x == direction.x < 0;
                    // }
                    float dist = Vector2.Distance(curCenter, cndCenter);
                    Vector2 dir = Custom.DirVec(curCenter, cndCenter);
                    float angle = 0.5f - 0.5f * Vector2.Dot(dir, direction.ToVector2().normalized);
                    if (angle > 0.5f)
                    {
                        if (angle > 0.8f)
                        {
                            angle = 0.5f - 0.5f * Vector2.Dot(-dir, direction.ToVector2().normalized);
                            dist = Vector2.Distance(curCenter, cndCenter + direction.ToVector2() * ((direction.x == 0) ? 1800f : 2400f));
                        }
                        else { dist += 100000f; }
                    }
                    angle *= 50f;
                    float cndLikelihood = (1f + dist) * (1f + angle);
                    if (cndLikelihood < likelihood)
                    {
                        result = candidates[i];
                        likelihood = cndLikelihood;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Notify config change to Menu
        /// </summary>
        /// <param name="config">Changed <see cref="UIconfig"/></param>
        /// <param name="oldValue">Original <see cref="UIconfig.value"/> before the change</param>
        /// <param name="value">New <see cref="UIconfig.value"/></param>
        internal void NotifyConfigChange(UIconfig config, string oldValue, string value)
        {
            if (history.Count > 0)
            {
                ConfigHistory last = history.Peek();
                if (last.config.key == config.key)
                {
                    oldValue = history.Pop().origValue;
                    if (oldValue == value) { return; } // User-reverted config; Remove history
                }
            }
            history.Push(new ConfigHistory() { config = config, origValue = oldValue });
            // configChanged = true; == history.Count > 0
            // cfgContainer.menuTab.saveButton.text = InternalTranslator.Translate("APPLY");
        }

        /// <summary>
        /// History for undo
        /// </summary>
        private Stack<ConfigHistory> history;

        private struct ConfigHistory
        {
            public UIconfig config;
            public string origValue;
        }

        #region Sound

        /// <summary>
        /// Restricted <see cref="Menu.Menu.PlaySound(SoundID)"/> to prevent sound glitch
        /// </summary>
        public static void PlaySound(SoundID soundID)
        {
            if (_soundFilled || ModConfigMenu.instance.manager.rainWorld.options.soundEffectsVolume == 0f) { return; }
            _soundFill += GetSoundFill(soundID);
            ModConfigMenu.instance.PlaySound(soundID);
        }

        /// <summary>
        /// Restricted <see cref="Menu.Menu.PlaySound(SoundID, float, float, float)"/> to prevent sound glitch
        /// </summary>
        public static void PlaySound(SoundID soundID, float pan, float vol, float pitch)
        {
            if (_soundFilled || ModConfigMenu.instance.manager.rainWorld.options.soundEffectsVolume == 0f) { return; }
            _soundFill += GetSoundFill(soundID);
            ModConfigMenu.instance.PlaySound(soundID, pan, vol, pitch);
        }

        private static int GetSoundFill(SoundID soundID)
        {
            SoundLoader.SoundData soundData = ModConfigMenu.instance.manager.menuMic.GetSoundData(soundID, -1);
            AudioClip clip = ModConfigMenu.instance.manager.menuMic.soundLoader.GetAudioClip(soundData.audioClip);
            if (clip == null) { return 0; }
            return Mathf.CeilToInt(clip.length * 60.0f) + 1;
        }

        /// <summary>
        /// Add number (in proportion with sound effect's length) to this whenever you're playing soundeffect. See also <seealso cref="_soundFilled"/>
        /// </summary>
        private static int _soundFill;

        /// <summary>
        /// Whether the sound engine is full or not. See also <seealso cref="_soundFill"/>
        /// </summary>
        private static bool _soundFilled => _soundFill > UIelement.FrameMultiply(80) || mute;

        /// <summary>
        /// Whether to play sound or not
        /// </summary>
        public static bool mute { get; internal set; }

        #endregion Sound
    }
}