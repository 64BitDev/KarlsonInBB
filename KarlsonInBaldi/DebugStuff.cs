using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KarlsonInBaldi
{
    public class DebugStuff
    //Remove this class in Rel
    {
        [HarmonyPatch(typeof(Navigator), "Awake")]
        class NavPointStart
        {
            [HarmonyPostfix]
            static void Postfix(Navigator __instance) //add vars to access them
            {
                var line = __instance.gameObject.AddComponent<LineRenderer>();
                line.positionCount = 2;
                line.SetPosition(0, Vector3.zero);
                line.SetPosition(1, new Vector3(0, 1000, 1000));
                line.startWidth = 0.1f;
                line.endWidth = 0.1f;
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = Color.red;
                line.endColor = Color.red;

            }
        }

        [HarmonyPatch(typeof(Navigator), "LateUpdate")]
        class NavPointUpdate
        {
            [HarmonyPostfix]
            static void Postfix(Navigator __instance) //add vars to access them
            {
                var line = __instance.gameObject.GetComponent<LineRenderer>();
                line.SetPosition(0, __instance.transform.position);
                line.SetPosition(1, __instance.NextPoint);
            }
        }
    }
}
