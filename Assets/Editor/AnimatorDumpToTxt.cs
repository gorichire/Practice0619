// Assets/Editor/AnimatorDumpToTxt.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Text;
using System.IO;

public class AnimatorDumpToTxt
{
    [MenuItem("Tools/Animator ▶ Dump to TXT")]
    private static void DumpSelectedAnimators()
    {
        foreach (var obj in Selection.objects)
        {
            var controller = obj as AnimatorController;
            if (controller == null)
            {
                Debug.LogWarning($"{obj.name} 는 AnimatorController가 아닙니다.");
                continue;
            }

            string txt = BuildDump(controller);
            string path = Path.Combine(Application.dataPath, controller.name + "_AnimatorDump.txt");
            File.WriteAllText(path, txt, Encoding.UTF8);
            Debug.Log($"➡  {controller.name} 정보를 '{path}' 에 기록했습니다.");
        }
        AssetDatabase.Refresh();
    }

    private static string BuildDump(AnimatorController ac)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== Animator: {ac.name} ===");

        // [Parameters]
        sb.AppendLine("\n[Parameters]");
        foreach (var p in ac.parameters)
            sb.AppendLine($"- {p.type}  {p.name}");

        // [Layers / States / Transitions]
        for (int i = 0; i < ac.layers.Length; i++)
        {
            var layer = ac.layers[i];
            sb.AppendLine($"\n[L{i}] Layer \"{layer.name}\"  (default weight {layer.defaultWeight})");

            foreach (var smState in layer.stateMachine.states)
            {
                var s = smState.state;
                sb.AppendLine($"  • State: {s.name}");
                sb.AppendLine($"      Tag   : {s.tag}");
                sb.AppendLine($"      Motion: {(s.motion ? s.motion.name : "None")}");

                foreach (var t in s.transitions)
                {
                    sb.AppendLine($"      → {t.destinationState?.name ?? "<Exit>"}" +
                                  $"  (HasExitTime={t.hasExitTime}, Dur={t.duration:0.00}s)");
                }
            }
        }

        return sb.ToString(); 
    }
}
#endif
