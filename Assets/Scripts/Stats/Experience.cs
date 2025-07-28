using RPG.Saving;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;

namespace RPG.Stats
{
    public class Experience : MonoBehaviour, ISaveable/* , IJsonSaveable*/
    {
        [SerializeField] float experiencePoints = 0;

        public event Action onExperienceGained;
        public void GainExperience(float experience)
        {
            experiencePoints += experience;
            onExperienceGained();
        }
        public float GetPoints()
        {
            return experiencePoints;
        }

        public object CaptureState()
        {
            return experiencePoints;
        }

        public void RestoreState(object state)
        {
            experiencePoints = (float)state;
        }
        //public JToken CaptureAsJToken()
        //{
        //    return JToken.FromObject(experiencePoints);
        //}

        //public void RestoreFromJToken(JToken state)
        //{
        //    experiencePoints = state.ToObject<float>();
        //}

    }
}