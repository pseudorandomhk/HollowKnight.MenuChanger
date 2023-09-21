using MenuChanger.Extensions;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;

namespace MenuChanger
{
    public static class PrefabMenuObjects
    {
        internal static Dictionary<string, GameObject> indexedPrefabs = new(10);

        internal static GameObject ClassicModeButtonObject
        {
            get => Get();
            set => Set(value);
        }

        internal static GameObject SteelModeButtonObject
        {
            get => Get();
            set => Set(value);
        }

        internal static GameObject BackButtonObject
        {
            get => Get();
            set => Set(value);
        }

        internal static GameObject DescTextObject
        {
            get => Get();
            set => Set(value);
        }

        internal static GameObject Get([CallerMemberName] string name = null)
        {
            if (indexedPrefabs.TryGetValue(name, out GameObject prefab) && prefab != null)
            {
                prefab = UObject.Instantiate(prefab);
                prefab.SetActive(true);
                return prefab;
            }

            Log($"MenuChanger prefab {name} did not exist at time of request!");
            return null;
        }

        internal static void Set(GameObject value, [CallerMemberName] string name = null)
        {
            UObject.DontDestroyOnLoad(value);
            indexedPrefabs[name] = value;
            value.SetActive(false);
        }

        internal static void Setup()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Menu_Title") return;

            ClassicModeButtonObject = UObject.Instantiate(UIManager.instance.playModeMenuScreen.defaultHighlight.gameObject);
            SteelModeButtonObject = UObject.Instantiate(UIManager.instance.playModeMenuScreen.defaultHighlight.FindSelectableOnDown().gameObject);
            BackButtonObject = UObject.Instantiate(UIManager.instance.playModeMenuScreen.defaultHighlight
                .FindSelectableOnDown().FindSelectableOnDown().gameObject);
            DescTextObject = ConstructDescriptionText();
        }

        internal static void Dispose()
        {
            foreach (KeyValuePair<string, GameObject> kvp in indexedPrefabs)
            {
                if (kvp.Value != null) UObject.Destroy(kvp.Value);
            }
            indexedPrefabs.Clear();
        }

        internal static GameObject ConstructDescriptionText()
        {
            var go = new GameObject("DescriptionText", typeof(RectTransform));

            var rect = go.GetComponent<RectTransform>();
            var canvas = go.AddComponent<CanvasRenderer>();
            var text = go.AddComponent<Text>();
            var fitter = go.AddComponent<ContentSizeFitter>();

            rect.localScale = new Vector3(1, 1, 1);
            rect.localPosition = new Vector3(0, 0.0002f, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.anchorMin = new Vector2(0, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(0, 944.7246f);

            canvas.cull = false;
            canvas.DisableRectClipping();

            text.font = Modding.CanvasUtil.GetFont("Perpetua");
            text.fontSize = 46;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.lineSpacing = 0.85f;
            text.resizeTextMaxSize = 48;
            text.resizeTextMinSize = 20;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.text = "\nFREE CONTENT PACK 03\n\nORIGINAL RELEASE DATE: Early 2018\n\nDESCRIPTION: Take your place amongst the Gods in an epic celebration of Hallownest's Glory and the final chapter of the Knight's journey.\n\nNew Character and Quest - The Godseeker arrives. Break her chains and aid her in an ancient duty.\n\nNew Boss Fights - Hallownest's greatest warriors raise their blades.\n\nNew Game Mode - Long requested and a classic for the genre. Complete the Godseeker's quest to unlock the new mode.\n\nNew Music - All new soaring boss tracks and giant remixes of beloved classics.\n\n\n";

            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            //UObject.DontDestroyOnLoad(go);
            return go;
        }

        public static void Normalize(MenuPage page, params GameObject[] gs)
        {
            foreach (GameObject g in gs)
            {
                page.Add(g);
                g.transform.localPosition = Vector2.zero;
                g.transform.localScale = Vector2.one;
            }
        }

        public static (GameObject, Text, CanvasGroup) BuildDescText(MenuPage page, string text)
        {
            GameObject obj = DescTextObject;

            // set text and remove scrollbar mask
            Text t = obj.GetComponent<Text>();
            t.text = text;
            t.material = BackButtonObject.transform.Find("Text").GetComponent<Text>().material;

            // add to page and fix scale issues
            page.Add(obj);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);

            // allow clicking things placed behind the text
            CanvasGroup cg = obj.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;

            return (obj, t, cg);
        }

        public static (MenuButton button, Text titleText, Text descText) CloneBigButton(Mode mode = Mode.Classic)
        {
            MenuButton button;
            switch (mode)
            {
                default:
                case Mode.Classic:
                    button = ClassicModeButtonObject.GetComponent<MenuButton>();
                    break;
                case Mode.Steel:
                    button = SteelModeButtonObject.GetComponent<MenuButton>();
                    break;
            }
            button.buttonType = MenuButton.MenuButtonType.Proceed;
            var eventTrigger = button.gameObject.GetComponent<EventTrigger>();
            eventTrigger.triggers = eventTrigger.triggers.Where(t => t.eventID != EventTriggerType.PointerClick).ToList();
            button.cancelAction = GlobalEnums.CancelAction.CustomCancelAction;
            button.navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
            };

            Transform textTrans = button.transform.Find("Text");
            UObject.Destroy(textTrans.GetComponent<AutoLocalizeTextUI>());
            Text titleText = textTrans.GetComponent<Text>();
            textTrans.GetComponent<RectTransform>().sizeDelta = new Vector2(784f, 63f);

            Transform descTrans = button.transform.Find("DescriptionText");
            UObject.Destroy(descTrans.GetComponent<AutoLocalizeTextUI>());
            Text descText = descTrans.GetComponent<Text>();

            return (button, titleText, descText);
        }

        public static MenuButton BuildBigButtonOneTextNoSprite(string title)
        {
            (MenuButton button, Text titleText, Text descText) = CloneBigButton();
            UObject.Destroy(button.transform.Find("Image").GetComponent<Image>());
            titleText.text = title;
            titleText.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -5f);
            descText.text = string.Empty;

            return button;
        }

        public static MenuButton BuildBigButtonTwoTextNoSprite(string title, string desc)
        {
            (MenuButton button, Text titleText, Text descText) = CloneBigButton();
            UObject.Destroy(button.transform.Find("Image").GetComponent<Image>());
            titleText.text = title;
            descText.text = desc;
            titleText.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 28f);
            descText.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -53f);

            return button;
        }

        public static MenuButton BuildBigButtonTwoTextAndSprite(Sprite sprite, string title, string desc)
        {
            MenuButton button = ClassicModeButtonObject.GetComponent<MenuButton>();
            button.buttonType = MenuButton.MenuButtonType.Proceed;
            button.cancelAction = GlobalEnums.CancelAction.CustomCancelAction;
            var eventTrigger = button.gameObject.GetComponent<EventTrigger>();
            eventTrigger.triggers = eventTrigger.triggers.Where(t => t.eventID != EventTriggerType.PointerClick).ToList();

            Transform textTrans = button.transform.Find("Text");
            UObject.Destroy(textTrans.GetComponent<AutoLocalizeTextUI>());
            textTrans.GetComponent<Text>().text = title ?? string.Empty;
            if (string.IsNullOrEmpty(desc))
            {
                textTrans.GetComponent<RectTransform>().anchoredPosition = new Vector2(130.5f, -5f);
            }
            // scaling issues with title text
            textTrans.GetComponent<RectTransform>().sizeDelta = new Vector2(784f, 63f);

            Transform descTrans = button.transform.Find("DescriptionText");
            UObject.Destroy(descTrans.GetComponent<AutoLocalizeTextUI>());
            descTrans.GetComponent<Text>().text = desc ?? string.Empty;

            if (sprite != null)
            {
                button.transform.Find("Image").GetComponent<Image>().sprite = sprite;
            }

            return button;
        }

        public static MenuButton BuildBigButtonOneTextAndSprite(Sprite sprite, string title)
        {
            MenuButton button = ClassicModeButtonObject.GetComponent<MenuButton>();
            button.buttonType = MenuButton.MenuButtonType.Proceed;
            var eventTrigger = button.gameObject.GetComponent<EventTrigger>();
            eventTrigger.triggers = eventTrigger.triggers.Where(t => t.eventID != EventTriggerType.PointerClick).ToList();
            button.cancelAction = GlobalEnums.CancelAction.CustomCancelAction;

            Transform textTrans = button.transform.Find("Text");
            UObject.Destroy(textTrans.GetComponent<AutoLocalizeTextUI>());
            textTrans.GetComponent<Text>().text = title ?? string.Empty;
            textTrans.GetComponent<RectTransform>().anchoredPosition = new Vector2(130.5f, -5f);

            // scaling issues with title text
            textTrans.GetComponent<RectTransform>().sizeDelta = new Vector2(784f, 63f);

            Transform descTrans = button.transform.Find("DescriptionText");
            UObject.Destroy(descTrans.GetComponent<AutoLocalizeTextUI>());
            descTrans.GetComponent<Text>().text = string.Empty;

            if (sprite != null)
            {
                button.transform.Find("Image").GetComponent<Image>().sprite = sprite;
            }

            return button;
        }

        public static MenuButton BuildBigButtonSpriteOnly(Sprite sprite)
        {
            MenuButton button = ClassicModeButtonObject.GetComponent<MenuButton>();
            button.buttonType = MenuButton.MenuButtonType.Proceed;
            button.cancelAction = GlobalEnums.CancelAction.CustomCancelAction;
            var eventTrigger = button.gameObject.GetComponent<EventTrigger>();
            eventTrigger.triggers = eventTrigger.triggers.Where(t => t.eventID != EventTriggerType.PointerClick).ToList();

            UObject.Destroy(button.transform.Find("Text").GetComponent<Text>());
            UObject.Destroy(button.transform.Find("DescriptionText").GetComponent<Text>());
            Image i = button.transform.Find("Image").GetComponent<Image>();
            i.sprite = sprite;
            i.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            i.rectTransform.localScale = new Vector2(2.8f, 2.8f);

            return button;
        }

        public static (GameObject, Text, CanvasGroup) BuildLabel(MenuPage page, string label)
        {
            GameObject obj = BackButtonObject;
            MenuButton button = obj.GetComponent<MenuButton>();
            button.name = label + " Label";
            button.buttonType = MenuButton.MenuButtonType.Activate;

            // Change text on the button
            Transform textTrans = button.transform.Find("Text");
            UObject.Destroy(textTrans.GetComponent<AutoLocalizeTextUI>());
            textTrans.GetComponent<Text>().text = label;

            UObject.Destroy(obj.GetComponent<EventTrigger>());
            UObject.Destroy(obj.GetComponent<MenuButton>());

            page.Add(obj);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

            CanvasGroup cg = obj.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;

            return (obj, obj.transform.Find("Text").GetComponent<Text>(), cg);
        }

        public static MenuButton BuildNewButton(string text)
        {
            GameObject buttonObj = BackButtonObject;
            MenuButton button = buttonObj.GetComponent<MenuButton>();
            button.name = text + " Button";
            button.buttonType = MenuButton.MenuButtonType.Activate;

            // Change text on the button
            Transform textTrans = button.transform.Find("Text");
            UObject.Destroy(textTrans.GetComponent<AutoLocalizeTextUI>());
            textTrans.GetComponent<Text>().text = text;

            button.ClearEvents();
            button.cancelAction = GlobalEnums.CancelAction.CustomCancelAction;
            button.navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
            };
            return button;
        }

        public static (GameObject, InputField) BuildEntryField()
        {
            GameObject obj = BackButtonObject;
            MenuButton button = obj.GetComponent<MenuButton>();
            button.name = "EntryField";
            button.buttonType = MenuButton.MenuButtonType.Activate;

            UObject.DestroyImmediate(obj.GetComponent<MenuButton>());
            UObject.DestroyImmediate(obj.GetComponent<EventTrigger>());
            UObject.DestroyImmediate(obj.transform.Find("Text").GetComponent<AutoLocalizeTextUI>());
            UObject.DestroyImmediate(obj.transform.Find("Text").GetComponent<FixVerticalAlign>());
            UObject.DestroyImmediate(obj.transform.Find("Text").GetComponent<ContentSizeFitter>());

            RectTransform textRT = obj.transform.Find("Text").GetComponent<RectTransform>();
            textRT.anchorMin = textRT.anchorMax = new Vector2(0.5f, 0.5f);
            textRT.sizeDelta = new Vector2(337, 63.2f);

            InputField inputField = obj.AddComponent<CustomInputField>();

            inputField.textComponent = obj.transform.Find("Text").GetComponent<Text>();

            inputField.caretColor = Color.white;
            inputField.contentType = InputField.ContentType.Standard;
            inputField.navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
            };
            inputField.caretWidth = 8;
            inputField.characterLimit = 9;
            inputField.text = string.Empty;

            return (obj, inputField);
        }

        public static (GameObject, InputField) BuildMultiLineEntryField(MenuPage page)
        {
            GameObject obj = DescTextObject;

            // remove scrollbar mask
            Text t = obj.GetComponent<Text>();
            t.material = BackButtonObject.transform.Find("Text").GetComponent<Text>().material;

            // add to page and fix scale issues
            page.Add(obj);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
            try
            {
                UObject.DestroyImmediate(obj.GetComponent<AutoLocalizeTextUI>());
                UObject.DestroyImmediate(obj.GetComponent<FixVerticalAlign>());
                UObject.DestroyImmediate(obj.GetComponent<ContentSizeFitter>());
            }
            catch (Exception e)
            {
                MenuChangerMod.instance.LogError(e);
            }

            // befuddling
            RectTransform textRT = obj.GetComponent<RectTransform>();
            textRT.anchorMin = textRT.anchorMax = new Vector2(0.5f, 0.5f);
            textRT.sizeDelta = new Vector2(450f, 800f);

            InputField inputField = obj.AddComponent<CustomInputField>();

            inputField.textComponent = t;

            inputField.caretColor = Color.white;
            inputField.contentType = InputField.ContentType.Standard;
            inputField.navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
            };
            inputField.caretWidth = 8;
            inputField.characterLimit = 600;
            inputField.text = string.Empty;
            inputField.lineType = InputField.LineType.MultiLineSubmit;

            return (obj, inputField);
        }
    }
}
