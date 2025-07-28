using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace RPG.Cinematics
{
    public class CinematicTrigger : MonoBehaviour
    {
        public bool alreadyTriggerd = false;
        private void OnTriggerEnter(Collider other)
        {
            if (!alreadyTriggerd && other.gameObject.tag == "Player")
            {
                alreadyTriggerd = true;
                GetComponent<PlayableDirector>().Play();
            } 
        }
    }
}
