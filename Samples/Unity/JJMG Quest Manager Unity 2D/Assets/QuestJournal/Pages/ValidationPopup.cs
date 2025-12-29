using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace QuestJournal.UI
{
    internal static class ValidationPopup
    {
        public static void Show(VisualElement host, List<string> errors)
        {
            if (host == null) return;

            // Remove existing overlay if any
            var existing = host.Q<VisualElement>("validation-overlay");
            if (existing != null)
            {
                existing.RemoveFromHierarchy();
            }

            // Overlay - full window
            var overlay = new VisualElement { name = "validation-overlay" };
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.top = 0;
            overlay.style.width = new Length(100, LengthUnit.Percent);
            overlay.style.height = new Length(100, LengthUnit.Percent);
            overlay.style.display = DisplayStyle.Flex;
            overlay.style.flexDirection = FlexDirection.Column;
            overlay.style.alignItems = Align.Center;
            overlay.style.justifyContent = Justify.Center;
            overlay.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.5f));

            // Dialog
            var dialog = new VisualElement { name = "validation-dialog" };
            dialog.style.width = new Length(560, LengthUnit.Pixel);
            dialog.style.maxWidth = new Length(95, LengthUnit.Percent);
            dialog.style.paddingTop = 12;
            dialog.style.paddingBottom = 12;
            dialog.style.paddingLeft = 14;
            dialog.style.paddingRight = 14;
            dialog.style.backgroundColor = new StyleColor(Color.white);
            dialog.style.borderTopWidth = 1;
            dialog.style.borderBottomWidth = 1;
            dialog.style.borderLeftWidth = 1;
            dialog.style.borderRightWidth = 1;
            dialog.style.borderTopColor = new StyleColor(Color.black);
            dialog.style.borderLeftColor = new StyleColor(Color.black);
            dialog.style.borderRightColor = new StyleColor(Color.black);
            dialog.style.borderBottomColor = new StyleColor(Color.black);
            dialog.style.display = DisplayStyle.Flex;
            dialog.style.flexDirection = FlexDirection.Column;

            // Title
            var title = new Label("Validation Errors") { name = "validation-title" };
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 8;
            try { title.style.color = new StyleColor(Color.black); } catch { title.style.color = Color.black; }
            dialog.Add(title);

            // Message area (ScrollView)
            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            scroll.style.maxHeight = new Length(220, LengthUnit.Pixel);
            scroll.style.marginBottom = 6;
            foreach (var e in errors)
            {
                var lbl = new Label(e) { name = "validation-item" };
                lbl.style.marginBottom = 4;
                try { lbl.style.color = new StyleColor(Color.black); } catch { lbl.style.color = Color.black; }
                scroll.Add(lbl);
            }
            dialog.Add(scroll);

            // Buttons
            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.justifyContent = Justify.FlexEnd;
            buttons.style.marginTop = 8;

            var ok = new Button(() => { overlay.RemoveFromHierarchy(); }) { text = "OK" };
            ok.style.width = 80;
            ok.style.height = 28;
            buttons.Add(ok);

            dialog.Add(buttons);

            overlay.Add(dialog);

            host.Add(overlay);
            try { overlay.BringToFront(); } catch { }
        }
    }
}
