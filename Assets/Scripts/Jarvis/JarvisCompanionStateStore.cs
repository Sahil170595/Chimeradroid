using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Chimeradroid.Jarvis
{
    public static class JarvisCompanionStateStore
    {
        private const string FileName = "jarvis-companion-state.json";

        private static string StatePath =>
            Path.Combine(Application.persistentDataPath, FileName);

        public static CompanionLocalState Load()
        {
            try
            {
                if (!File.Exists(StatePath))
                {
                    return new CompanionLocalState();
                }

                var json = File.ReadAllText(StatePath);
                var state = JsonConvert.DeserializeObject<CompanionLocalState>(json);
                return state ?? new CompanionLocalState();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load companion state: {e.Message}");
                return new CompanionLocalState();
            }
        }

        public static void Save(CompanionLocalState state)
        {
            try
            {
                var dir = Path.GetDirectoryName(StatePath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var json = JsonConvert.SerializeObject(state ?? new CompanionLocalState(), Formatting.Indented);
                File.WriteAllText(StatePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to save companion state: {e.Message}");
            }
        }

        public static void Delete()
        {
            try
            {
                if (File.Exists(StatePath))
                {
                    File.Delete(StatePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to delete companion state: {e.Message}");
            }
        }
    }
