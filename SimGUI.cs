using UnityEngine;
using System.Collections.Generic;
using System;
using BepInEx;
using System.Linq;

namespace SimLibGUI
{
    public class SimGUI : MonoBehaviour
    {
        private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        private Color accentColor = new Color(0, 0.5f, 1f, 1f);
        private Color textColor = Color.white;
        private Color tabColor = new Color(0.15f, 0.15f, 1f, 1f);
        private Color activeTabColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        private GUIStyle windowStyle;
        private GUIStyle switchStyle;
        private GUIStyle sliderStyle;
        private GUIStyle buttonStyle;
        private GUIStyle headerStyle;
        private GUIStyle labelStyle;
        private GUIStyle tabStyle;
        private GUIStyle activeTabStyle;
        private GUIStyle titleStyle;
        private GUIStyle dropdownStyle;

        private KeyCode toggleKey = KeyCode.Period;

        private Rect windowRect = new Rect(20, 20, 300, 400);

        private Dictionary<string, bool> toggleStates = new Dictionary<string, bool>();
        private Dictionary<string, float> sliderValues = new Dictionary<string, float>();
        private Dictionary<string, int> dropdownSelections = new Dictionary<string, int>();
        private Dictionary<string, Vector2> minMaxSliderValues = new Dictionary<string, Vector2>();
        private Dictionary<string, string> textInputValues = new Dictionary<string, string>();
        private Dictionary<string, float> numberInputValues = new Dictionary<string, float>();
        private Dictionary<string, KeyCode> keybinds = new Dictionary<string, KeyCode>();

        private List<(string name, Action drawContent, Action<bool> update)> tabs = new List<(string name, Action drawContent, Action<bool> update)>();

        private bool isDropdownOpen = false;
        private string openDropdownId = "";

        private bool isDraggingMin = false;
        private bool isDraggingMax = false;
        private bool isVisible = false;
        private bool stylesInitialized = false;
        private string listeningForKeybind = null;
        private int currentTabIndex = 0;

        private string menuTitle = "Mod Menu";
        private int windowId;

        public SimGUI()
        {
            windowId = GetHashCode();
        }

        public void SetTitle(string title)
            => menuTitle = title;

        public void SetToggleKey(KeyCode key)
            => toggleKey = key;

        public void SetWindowSize(Vector2 size)
            => windowRect.size = size;

        public void AddTab(string tabName, Action drawContent, Action<bool> update = null)
            => tabs.Add((tabName, drawContent, update));

        private void Update()
        {
            if (UnityInput.Current.GetKeyDown(toggleKey) && listeningForKeybind == null)
            {
                isVisible = !isVisible;
            }

            if (listeningForKeybind != null)
            {
                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (UnityInput.Current.GetKeyDown(keyCode))
                    {
                        keybinds[listeningForKeybind] = keyCode;
                        listeningForKeybind = null;
                        break;
                    }
                }
            }

            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].update != null)
                    tabs[i].update(isVisible && i == currentTabIndex);
            }
        }

        private void OnGUI()
        {
            if (!stylesInitialized)
            {
                InitializeStyles();
                stylesInitialized = true;
            }

            if (isVisible)
            {
                windowRect = GUILayout.Window(windowId, windowRect, DrawWindow, menuTitle, windowStyle);
            }
        }

        private void InitializeStyles()
        {
            windowStyle = new GUIStyle(GUI.skin.window);
            // windowStyle.normal.background = MakeTexture(2, 2, backgroundColor);
            windowStyle.normal.textColor = textColor;
            windowStyle.fontSize = 16;
            windowStyle.fontStyle = FontStyle.Bold;

            switchStyle = new GUIStyle(GUI.skin.toggle);
            switchStyle.normal.textColor = textColor;

            sliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
            // sliderStyle.normal.background = MakeTexture(2, 2, new Color(0.3f, 0.3f, 0.3f, 1f));
            //GUI.skin.horizontalSliderThumb.normal.background = MakeTexture(2, 2, accentColor);

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.textColor = textColor;
            // buttonStyle.normal.background = MakeTexture(2, 2, new Color(0.2f, 0.2f, 0.2f, 1f));
            // buttonStyle.hover.background = MakeTexture(2, 2, accentColor);

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = textColor;
            labelStyle.fontSize = 12;

            dropdownStyle = new GUIStyle(GUI.skin.button);
            dropdownStyle.normal.textColor = textColor;
            // dropdownStyle.normal.background = MakeTexture(2, 2, new Color(0.2f, 0.2f, 0.2f, 1f));
            // dropdownStyle.hover.background = MakeTexture(2, 2, accentColor);
            dropdownStyle.alignment = TextAnchor.MiddleLeft;
            dropdownStyle.padding = new RectOffset(5, 5, 2, 2);

            tabStyle = new GUIStyle(GUI.skin.button);
            // tabStyle.normal.background = MakeTexture(2, 2, tabColor);
            tabStyle.normal.textColor = textColor;

            activeTabStyle = new GUIStyle(tabStyle);
            // activeTabStyle.normal.background = MakeTexture(2, 2, activeTabColor);

            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.normal.textColor = textColor;
            titleStyle.fontSize = 16;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.normal.textColor = textColor;
            headerStyle.fontSize = 14;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleLeft;
            headerStyle.padding = new RectOffset(5, 5, 5, 5);
        }

        private void DrawWindow(int windowID)
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            for (int i = 0; i < tabs.Count; i++)
            {
                if (GUILayout.Toggle(currentTabIndex == i, tabs[i].name, currentTabIndex == i ? activeTabStyle : tabStyle))
                {
                    currentTabIndex = i;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (currentTabIndex >= 0 && currentTabIndex < tabs.Count)
            {
                tabs[currentTabIndex].drawContent();
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        public bool AddSwitch(string label, string id, bool initialState = false)
        {
            if (!toggleStates.ContainsKey(id))
            {
                toggleStates[id] = initialState;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, labelStyle, GUILayout.Width(150));
            bool newState = GUILayout.Toggle(toggleStates[id], toggleStates[id] ? "ON" : "OFF", switchStyle);
            GUILayout.EndHorizontal();

            if (newState != toggleStates[id])
            {
                toggleStates[id] = newState;
            }

            return toggleStates[id];
        }

        public float AddSlider(string label, string id, float initialValue, float min, float max)
        {
            if (!sliderValues.ContainsKey(id))
            {
                sliderValues[id] = initialValue;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, labelStyle, GUILayout.Width(150));

            float sliderValue = GUILayout.HorizontalSlider(
                Mathf.Clamp(sliderValues[id], min, max),
                min,
                max,
                sliderStyle,
                GUI.skin.horizontalSliderThumb,
                GUILayout.Width(150)
            );

            // Update value if slider was moved
            if (sliderValue != Mathf.Clamp(sliderValues[id], min, max))
            {
                sliderValues[id] = sliderValue;
            }

            string input = GUILayout.TextField(sliderValues[id].ToString("F2"), GUILayout.Width(50));
            if (float.TryParse(input, out float inputValue))
            {
                // Allow input value to go beyond min and max
                sliderValues[id] = inputValue;
            }

            GUILayout.EndHorizontal();

            return sliderValues[id];
        }

        public int AddDropdown(string label, string id, string[] options, int initialIndex = 0)
        {
            if (!dropdownSelections.ContainsKey(id))
            {
                dropdownSelections[id] = initialIndex;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, labelStyle, GUILayout.Width(150));

            if (GUILayout.Button(options[dropdownSelections[id]], dropdownStyle))
            {
                if (isDropdownOpen && openDropdownId == id)
                {
                    isDropdownOpen = false;
                }
                else
                {
                    isDropdownOpen = true;
                    openDropdownId = id;
                }
            }

            GUILayout.EndHorizontal();

            if (isDropdownOpen && openDropdownId == id)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    if (GUILayout.Button(options[i], dropdownStyle))
                    {
                        dropdownSelections[id] = i;
                        isDropdownOpen = false;
                    }
                }
            }

            return dropdownSelections[id];
        }

        public Vector2 AddMinMaxSlider(string label, string id, float initialMin, float initialMax, float absoluteMin, float absoluteMax)
        {
            if (!minMaxSliderValues.ContainsKey(id))
            {
                minMaxSliderValues[id] = new Vector2(initialMin, initialMax);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, labelStyle, GUILayout.Width(150));
            GUILayout.BeginVertical();

            Vector2 values = minMaxSliderValues[id];
            float minValue = values.x;
            float maxValue = values.y;

            GUILayout.BeginHorizontal(GUILayout.Height(20));
            Rect sliderRect = GUILayoutUtility.GetRect(200, 20);

            GUI.Box(sliderRect, "", GUI.skin.horizontalSlider);

            float minPos = Mathf.Clamp01((Mathf.Clamp(minValue, absoluteMin, absoluteMax) - absoluteMin) / (absoluteMax - absoluteMin)) * sliderRect.width;
            float maxPos = Mathf.Clamp01((Mathf.Clamp(maxValue, absoluteMin, absoluteMax) - absoluteMin) / (absoluteMax - absoluteMin)) * sliderRect.width;

            GUI.Box(new Rect(sliderRect.x + minPos, sliderRect.y, maxPos - minPos, sliderRect.height), "", GUI.skin.GetStyle("SelectionRect"));

            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && sliderRect.Contains(e.mousePosition))
            {
                float mousePos = e.mousePosition.x - sliderRect.x;
                if (Mathf.Abs(mousePos - minPos) < Mathf.Abs(mousePos - maxPos))
                {
                    isDraggingMin = true;
                }
                else
                {
                    isDraggingMax = true;
                }
                e.Use();
            }
            else if (e.type == EventType.MouseUp)
            {
                isDraggingMin = false;
                isDraggingMax = false;
            }
            else if (e.type == EventType.MouseDrag && (isDraggingMin || isDraggingMax))
            {
                float mousePos = e.mousePosition.x - sliderRect.x;
                float newValue = (mousePos / sliderRect.width) * (absoluteMax - absoluteMin) + absoluteMin;

                if (isDraggingMin)
                {
                    minValue = Mathf.Clamp(newValue, absoluteMin, maxValue);
                }
                else if (isDraggingMax)
                {
                    maxValue = Mathf.Clamp(newValue, minValue, absoluteMax);
                }
                e.Use();
            }

            GUI.Box(new Rect(sliderRect.x + minPos - 5, sliderRect.y, 10, sliderRect.height), "", GUI.skin.horizontalSliderThumb);
            GUI.Box(new Rect(sliderRect.x + maxPos - 5, sliderRect.y, 10, sliderRect.height), "", GUI.skin.horizontalSliderThumb);

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            string minInput = GUILayout.TextField(minValue.ToString("F2"), GUILayout.Width(50));
            GUILayout.FlexibleSpace();
            string maxInput = GUILayout.TextField(maxValue.ToString("F2"), GUILayout.Width(50));
            GUILayout.EndHorizontal();

            if (float.TryParse(minInput, out float inputMinValue))
            {
                minValue = inputMinValue;
                if (minValue > maxValue)
                {
                    maxValue = minValue;
                }
            }
            if (float.TryParse(maxInput, out float inputMaxValue))
            {
                maxValue = inputMaxValue;
                if (maxValue < minValue)
                {
                    minValue = maxValue;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            minMaxSliderValues[id] = new Vector2(minValue, maxValue);
            return minMaxSliderValues[id];
        }

        public void AddLabel(string text)
            => GUILayout.Label(text, labelStyle);

        public bool AddButton(string label)
            => GUILayout.Button(label, buttonStyle);

        public void AddHeader(string text)
        {
            GUILayout.Space(10);
            GUILayout.Label(text, headerStyle);
            GUILayout.Space(5);
        }

        public string AddTextInput(string label, string id, string initialValue = "")
        {
            if (!textInputValues.ContainsKey(id))
            {
                textInputValues[id] = initialValue;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, labelStyle, GUILayout.Width(150));
            textInputValues[id] = GUILayout.TextField(textInputValues[id], GUILayout.Width(150));
            GUILayout.EndHorizontal();

            return textInputValues[id];
        }

        public float AddNumberInput(string label, string id, float initialValue = 0f)
        {
            if (!numberInputValues.ContainsKey(id))
            {
                numberInputValues[id] = initialValue;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, labelStyle, GUILayout.Width(150));
            string input = GUILayout.TextField(numberInputValues[id].ToString(), GUILayout.Width(150));
            GUILayout.EndHorizontal();

            if (float.TryParse(input, out float result))
            {
                numberInputValues[id] = result;
            }

            return numberInputValues[id];
        }

        public KeyCode AddKeybindButton(string label, string id, KeyCode defaultKey)
        {
            if (!keybinds.ContainsKey(id))
            {
                keybinds[id] = defaultKey;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, labelStyle, GUILayout.Width(150));

            string buttonText = (listeningForKeybind == id) ? "Press any key..." : keybinds[id].ToString();
            if (GUILayout.Button(buttonText, buttonStyle, GUILayout.Width(150)))
            {
                if (listeningForKeybind == id)
                {
                    listeningForKeybind = null;
                }
                else
                {
                    listeningForKeybind = id;
                }
            }

            GUILayout.EndHorizontal();

            return keybinds[id];
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = Enumerable.Repeat(color, width * height).ToArray();
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }
    }
}