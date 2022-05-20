using System;
using UnityEngine;

namespace RO.UI
{
    [Flags]
    public enum CanvasFilter
    {
        ItemDrag = 1 << 0,
        SkillDrag = 1 << 1,
        DragPanel = 1 << 2,
        DragSubChat = 1 << 3,
        NpcDialog = 1 << 4,
        ModalMsgDialog = 1 << 5,
        DisconnectDialog = 1 << 6
    }

    public partial class UIController : MonoBehaviour
    {
        //This is to encapsulate the panels interface
        public abstract partial class Panel : MonoBehaviour, ICanvasRaycastFilter
        {
            // intrusive linked list for panel order
            private Panel _nextPanel;
            private Panel _prevPanel;

            public static CanvasFilter CanvasFilter { get; private set; } = 0;
            public static void SetCanvasFilterFlags(CanvasFilter flags)
            {
                CanvasFilter |= flags;
            }

            public static void ClearCanvasFilterFlags(CanvasFilter flags)
            {
                CanvasFilter &= ~flags;
            }

            [SerializeField]
            private EscAction escAction = EscAction.ClosePanel;

            [SerializeField]
            private UIController _uiController = default;
            protected UIController UiController => _uiController;
            protected EscHandler EscController => _uiController._escHandler;
            protected DragIconController DragIconController => _uiController._dragIconController;
            protected LabelController LabelController => _uiController._labelController;
            protected MessageDialogController MessageDialogController => _uiController._messageDialogController;

            protected class GrayscaleShader
            {
                public static Color Red(float alpha) { return new Color(206 / 255f, 0, 0, alpha); }
                public static Color Green(float alpha) { return new Color(0, 206 / 255f, 0, alpha); }
                public static Color Blue(float alpha) { return new Color(0, 0, 206 / 255f, alpha); }
                public static Color Yellow(float alpha) { return new Color(0, 0, 49 / 255f, alpha); }
                public static Color Purple(float alpha) { return new Color(0, 49 / 255f, 0, alpha); }
                public static Color Grey(float alpha) { return new Color(0, 0, 0, alpha); }
                public static Color Original(float alpha) { return new Color(1, 1, 1, alpha); }
            }

            public enum MaterialType
            {
                Default,
                SpritePallete,
                Grayscale,
                TextOutline,
                BattleModeSlot,
                TextLabel,
                Mask,
                MaskedTextLabel
            }


            public static T Instantiate<T>(UIController uiController, Transform parent, string assetName)
                where T : Panel
            {
                var panel = AssetBundleProvider.LoadUiBundleAsset<GameObject>(assetName);
                panel = Instantiate(panel, parent, false);

                var controller = panel.GetComponentInChildren<T>();
                controller._uiController = uiController;

                return controller;
            }

            // Api for panel and its components

            public Material GetTransparentMaterial(MaterialType type)
            {
                return UiController._uiMaterialsTransparent[(int)type];
            }

            public Material GetMaterial(MaterialType type)
            {
                return UiController._uiMaterials[(int)type];
            }

            // Override if it's undesirable
            public virtual void BringToFront()
            {
                gameObject.transform.SetAsLastSibling();
            }

            protected void OnEnable()
            {
                // auto bring to front all panels that are closable
                if (escAction == EscAction.ClosePanel)
                    BringToFront();

                _uiController._escHandler.OnPanelEnabled(this);
            }

            protected void TriggerDisconnect()
            {
                _uiController.TriggerLobbyDisconnect();
            }

            protected void OnDisable()
            {
                _uiController._escHandler.OnPanelDisabled(this);
                RO.Media.CursorAnimator.UnsetAnimation(RO.Media.CursorAnimator.Animations.Click);
            }

            // Workaround if panel is not meant to be instantiated from asset bundle
            protected void ForceSetUiController(UIController controller)
            {
                _uiController = controller;
            }

            protected void SetSelectedCharacter(CharacterInfo info)
            {
                UiController._selectedCharacter = info;
            }

            protected CharacterInfo[] GetCharInfos()
            {
                return UiController._availableCharacters;
            }

            protected void LoginPanelSetActive(bool active)
            {
                UiController.LoginPanelSetActive(active);
            }

            protected void CharacterSelectPanelSetActive(bool active)
            {
                UiController.CharacterSelectPanelSetActive(active);
            }

            protected void CharacterMakePanelSetActive(bool active)
            {
                UiController.CharacterMakePanelSetActive(active);
            }

            protected void SkillPanelSetActive(bool active)
            {
                UiController.SkillPanelSetActive(active);
            }

            protected void SoundPanelSetActive(bool active)
            {
                UiController.SoundPanelSetActive(active);
            }

            protected void ShortcutPanelSetActive(bool active)
            {
                UiController._shortcutPanelController.gameObject.SetActive(active);
            }

            public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
            {
                const CanvasFilter mask = ~CanvasFilter.NpcDialog;

                return (mask & CanvasFilter) == 0;
            }
        };
    }
}