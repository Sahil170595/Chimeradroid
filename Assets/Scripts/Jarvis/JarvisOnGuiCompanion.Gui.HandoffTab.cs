using UnityEngine;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private void RenderHandoffTab()
        {
            GUILayout.Label("Handoff token (redeem from desktop):");
            _handoffToken = GUILayout.TextField(_handoffToken);
            if (GUILayout.Button("Redeem Handoff"))
            {
                StartCoroutine(RedeemHandoff(_handoffToken));
            }

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Timeline"))
            {
                StartCoroutine(LoadTimeline());
            }
            if (GUILayout.Button("Connect Stream"))
            {
                StartStream();
            }
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_timeline))
            {
                GUILayout.Space(8);
                GUILayout.Label("Timeline:");
                _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(520));
                GUILayout.TextArea(_timeline);
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("(no timeline loaded)");
            }
        }
    }
}

