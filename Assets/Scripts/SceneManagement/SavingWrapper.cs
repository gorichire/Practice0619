using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Saving;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

namespace RPG.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        const string defaultSaveFile = "save";
        [SerializeField] float fadeInTime = 0.2f;

        public void ContinueGame()
        {
            StartCoroutine(LoadLastScene());
        }

        IEnumerator LoadLastScene()
        {
            yield return GetComponent<JsonSavingSystem>().LoadLastScene(defaultSaveFile);

            Fader fader = FindObjectOfType<Fader>();
            fader.FadeOutImmediate();
            yield return fader.FadeIn(fadeInTime);

            yield return null;
            GetComponent<JsonSavingSystem>().Save(defaultSaveFile);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                Load();
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                Save();
            }
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Delete();
            }
        }

        public void Save()
        {
            GetComponent<JsonSavingSystem>().Save(defaultSaveFile);
        }

        public void Load()
        {
            GetComponent<JsonSavingSystem>().Load(defaultSaveFile);
        }
        public void Delete()
        {
            GetComponent<JsonSavingSystem>().Delete(defaultSaveFile);
        }
    }
}
