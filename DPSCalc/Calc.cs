using MelonLoader;
using HarmonyLib;
using System.Timers;

[assembly: MelonInfo(typeof(DPSCalc.Calc), "DPSCalc", "1.0.5", "Faelynox", null)]
[assembly: MelonGame("KisSoft", "ATLYSS")]

namespace DPSCalc
{
    public class Calc : MelonMod
    {
        public static Calc instance;
        public Player localPlayer;
        public static List <int> damageList = [];
        public static List <int> dpsList = [];
        public static List <int> dpsList2 = [];
        public static List <int> compList = [];
        public static int finalDPS;
        private System.Timers.Timer timer;
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg(">>> DPS numbers will show in here. <<<");
            Calc.instance = this;

            timer = new System.Timers.Timer(1000);
            timer.Elapsed += onTimerElapsed;
            timer.AutoReset = true;
            timer.Start();
        }
        public void onTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MelonLogger.Msg($"""
                            
                            [DEBUG] Timer elapsed!
                            [DEBUG] {String.Join(",", dpsList2)}
                            [DEBUG] {String.Join(",", compList)} 
                            """);
            if (dpsList.Count() > 0)
            {
                int avgdps = dpsList.Sum();
                dpsList2.Add(avgdps);

                if (dpsList2.Count() > 5 || compList.Count() > 5)
                {
                    dpsList2.RemoveAt(0);
                    compList.RemoveAt(0);
                }
                
                if (!compList.Contains(avgdps))
                {
                    compList.Add(avgdps);
                }

                if (!dpsList2.SequenceEqual(compList))
                {
                    MelonLogger.Msg($"[DEBUG] Something went wrong.. clearing both lists!");
                    dpsList2.Clear();
                    compList.Clear();
                }
                
                MelonLogger.Msg($"[DPS] Current DPS: {dpsList2.Average()}");
                MelonLogger.Msg($"[DEBUG] {String.Join(",", dpsList2)} {String.Join(",", compList)}");
                dpsList.Clear();
            }
            else
            {
                dpsList2.RemoveAt(0);
                compList.RemoveAt(0);
            }
        }


        [HarmonyPatch(typeof(CombatCollider), "Apply_Damage")]
        public class ApplyDamagePatch
        {
            public static void Postfix(ref ITakeDamage _damageable, ref int _damageValue)
            {
                if(_damageable is StatusEntity statusEntity && statusEntity._isPlayer == null)
                {
                    MelonLogger.Msg($"[DMG] {_damageValue} hit.");
                    damageList.Add(_damageValue);
                    dpsList.Add(_damageValue);
                    if(damageList.Count > 10)
                    {
                        damageList.RemoveAt(0);
                    }
                }
            }
        }
        
    }
}