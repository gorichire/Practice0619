using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class PersistentObjectSpawner : MonoBehaviour
    {
        [SerializeField] GameObject persistentObjectPrefeb;

        static bool hasSpawned = false;

        private void Awake()
        {
            if(hasSpawned) { return; }

            SpawnPersistentObjects();

            hasSpawned = true;
        }

        private void SpawnPersistentObjects()
        {
            GameObject persistenObject = Instantiate(persistentObjectPrefeb);
            DontDestroyOnLoad(persistenObject);
        }
    }
}