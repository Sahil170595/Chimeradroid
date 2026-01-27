using System;
using UnityEngine;

namespace Chimeradroid
{
    public sealed partial class JarvisOnGuiCompanion
    {
        private Vector2 _meshScroll;

        private void RenderMeshTab()
        {
            GUILayout.Label("whoami:");
            if (GUILayout.Button("Refresh whoami"))
            {
                StartCoroutine(LoadWhoami());
            }
            if (!string.IsNullOrEmpty(_whoami))
            {
                GUILayout.TextArea(_whoami, GUILayout.Height(60));
            }

            GUILayout.Space(8);
            GUILayout.Label("mesh push:");
            GUILayout.BeginHorizontal();
            GUILayout.Label("event_type", GUILayout.Width(80));
            _meshEventType = GUILayout.TextField(_meshEventType);
            GUILayout.EndHorizontal();
            GUILayout.Label("event_data (json):");
            _meshEventJson = GUILayout.TextArea(_meshEventJson, GUILayout.Height(60));
            if (GUILayout.Button("Push event"))
            {
                StartCoroutine(MeshPush());
            }

            GUILayout.Space(8);
            GUILayout.Label("mesh pull:");
            GUILayout.BeginHorizontal();
            GUILayout.Label("since_seq", GUILayout.Width(80));
            var sinceS = GUILayout.TextField(_meshSinceSeq.ToString());
            if (int.TryParse(sinceS, out var sinceParsed))
            {
                _meshSinceSeq = Math.Max(0, sinceParsed);
            }
            if (GUILayout.Button("Pull (exclude self)"))
            {
                StartCoroutine(MeshPull(includeSelf: false));
            }
            if (GUILayout.Button("Pull (include self)"))
            {
                StartCoroutine(MeshPull(includeSelf: true));
            }
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_meshStatus))
            {
                GUILayout.Label(_meshStatus);
            }

            _meshScroll = GUILayout.BeginScrollView(_meshScroll, GUILayout.Height(360));
            GUILayout.TextArea(string.IsNullOrEmpty(_meshLog) ? "(no mesh log)" : _meshLog);
            GUILayout.EndScrollView();
        }
    }
}

