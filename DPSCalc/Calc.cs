using MelonLoader;
using HarmonyLib;
using System.Timers;
using System.Linq.Expressions;
using JetBrains.Annotations;
using UnityEngine;

[assembly: MelonInfo(typeof(DPSCalc.Calc), "DPSCalc", "1.1.0", "Faelynox", null)]
[assembly: MelonGame("KisSoft", "ATLYSS")]

namespace DPSCalc
{
    public class Calc : MelonMod
    {
        public static Calc instance;
        public Player localPlayer;
        public static List<int> damageList = [];
        public static List<int> dpsList = [];
        public static List<int> dpsList2 = [];
        public static int finalDPS;
        private System.Timers.Timer timer;
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg(">>> DPS numbers will show in here. <<<");
            Calc.instance = this;

            timer = new System.Timers.Timer(1000);
            timer.Elapsed += onTimerElapsed;
            timer.AutoReset = true;
            if (timer == null)
            {
                MelonLogger.Error("[ERROR] Timer is null!");
            }
            else
            {
                timer.Start();
                MelonLogger.Msg("[DEBUG] Timer started!");
            }
        }

        [HarmonyPatch(typeof(Player), "OnGameConditionChange")]
        public class playerSpawn
        {
            public static void Postfix(ref Player __instance)
            {
                if (__instance._currentGameCondition == GameCondition.IN_GAME && __instance.isLocalPlayer)
                {
                    Calc.instance.localPlayer = __instance;
                    MelonLogger.Msg($"[DEBUG] localPlayer:{Calc.instance.localPlayer}");
                }
            }
        }
        public override void OnUpdate()
        {
            if (localPlayer == null) { return; }

            if (localPlayer._inChat || localPlayer._inUI || HostConsole._current._isOpen || localPlayer._bufferingStatus) { return; }

            else if (Input.GetKeyDown(KeyCode.U))
            {
                dpsList.Clear();
                dpsList2.Clear();
                MelonLogger.Msg("[INPUT] Lists cleared!");
            }
        }
        public void onTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int maxValue = 0;
            void removeMaximum()
            {
                if (maxValue != 0)
                {
                    dpsList2.Remove(maxValue);
                }
                else if (dpsList2.Count() > 0)
                {
                    dpsList2.RemoveAt(0);
                }
            }
            if (dpsList.Count > 0)
            {
                try
                {

                    double averageDPSoverTime;
                    int avgdps = dpsList.Sum();
                    dpsList2.Add(avgdps);
                    averageDPSoverTime = dpsList2.Average();

                    if (dpsList2.Count() > 10 || dpsList.Count() == 0)
                    {
                        removeMaximum();
                    }

                    if (dpsList2.Count() > 0 || averageDPSoverTime != 0)
                    {
                        MelonLogger.Msg($"""

                                        [DPS] Average DPS: {averageDPSoverTime}
                                        [DPS] Raw DPS: {dpsList.Sum()}
                                        """);
                    }

                    dpsList.Clear();
                    if (dpsList2.Count > 0)
                    {
                        maxValue = dpsList2.Max();
                    }
                }
                catch (Exception exp)
                {
                    MelonLogger.Error($"[ERROR] {exp}");
                }
            }
            else
            {
                removeMaximum();
            }
        }

        [HarmonyPatch(typeof(CombatCollider), "Apply_Damage")]
        public class ApplyDamagePatch
        {
            public static void Postfix(ref ITakeDamage _damageable, ref int _damageValue)
            {
                if (_damageable is StatusEntity statusEntity && statusEntity._isPlayer == null)
                {
                    MelonLogger.Msg($"[DMG] {_damageValue} hit.");
                    damageList.Add(_damageValue);
                    dpsList.Add(_damageValue);
                    if (damageList.Count > 10)
                    {
                        damageList.RemoveAt(0);
                    }
                }
            }
        }
    }
}