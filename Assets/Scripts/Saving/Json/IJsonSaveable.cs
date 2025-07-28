using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;


namespace RPG.Saving
{
    public interface IJsonSaveable
    {
        /// <summary>
        /// Override to return a JToken representing the state of the IJsonSaveable
        /// </summary>
        /// <returns>A JToken</returns>
        JToken CaptureAsJToken();
        /// <summary>
        /// Restore the state of the component using the information in JToken.
        /// </summary>
        /// <param name="state">A JToken object representing the state of the module</param>
        void RestoreFromJToken(JToken state);
    }
    public static class JsonStatics
    {
        /// <summary>
        /// Convenience helper to turn this Vector3 into a JToken for using with the JsonSavingSystem.
        /// </summary>
        /// <example><code>
        /// JToken position = transform.position.ToToken();
        /// </code></example>
        /// <param name="vector">this Vector3</param>
        /// <returns></returns>
        public static JToken ToToken(this Vector3 vector)
        {
            JObject state = new JObject();
            IDictionary<string, JToken> stateDict = state;
            stateDict["x"] = vector.x;
            stateDict["y"] = vector.y;
            stateDict["z"] = vector.z;
            return state;
        }

        /// <summary>
        /// Vector3 is not implicitly serializable due to the way serializers handle properties.  If you try to
        /// serialize a Vector3, you will get an error which is caused by the serializer getting caught in a 
        /// recursive cycle.  While there is a library you can add to help Newtonsoft Json convert Vector3 values, 
        /// there is an issue building projects which sometimes causes these library values to NOT be linked
        /// into the project.  This extension method converts a Vector3 to a JToken.
        /// </summary>
        /// <example><code>
        /// transform.position = state.ToVector3();
        /// </code></example>
        /// <param name="state">this JToken, created by Vector3.ToToken()</param>
        /// <returns>Vector3 value stored in the JToken or a zero Vector on failure</returns>
        public static Vector3 ToVector3(this JToken state)
        {
            Vector3 vector = new Vector3();
            if (state is JObject jObject)
            {
                IDictionary<string, JToken> stateDict = jObject;

                if (stateDict.TryGetValue("x", out JToken x))
                {
                    vector.x = x.ToObject<float>();
                }

                if (stateDict.TryGetValue("y", out JToken y))
                {
                    vector.y = y.ToObject<float>();
                }

                if (stateDict.TryGetValue("z", out JToken z))
                {
                    vector.z = z.ToObject<float>();
                }
            }
            return vector;
        }
    }

}
