using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using MelonLoader;
using HarmonyLib;
using Il2CppTMPro;
using UnityEngine;

namespace RolesCollection;

[RegisterTypeInIl2Cpp]
public class SoulCollector : Demon
{
    public int countdown = 1200;
    public bool runCountDown = false;
    public override ActedInfo GetInfo(Character charRef)
    {
        return base.GetInfo(charRef);
    }
    public override void Act(ETriggerPhase trigger, Character charRef)
    {
        this.runCountDown = false;
        this.countdown = 1200;
        Characters instance = Characters.Instance;
        List<Character> goods = Stats.GetList(instance.FilterAlignmentCharacters(Gameplay.CurrentCharacters, EAlignment.Good));
        List<Character> randoms = new List<Character>();
        Character randomGood = Stats.RandomItem(goods);
        randoms.Add(randomGood);
        goods.Remove(randomGood);
        randoms.Add(Stats.RandomItem(goods));
        foreach (Character good in randoms)
        {
            good.statuses.AddStatus(Stats.reaped, charRef);
        }
        this.charRef = charRef;
    }
    public override void ActOnDied(Character charRef)
    {
        StopCountdown();
    }
    public void BeginAttack()
    {
        if (charRef == null)
        {
            return;
        }
        if (charRef.dataRef.name != "Death")
        {
            return;
        }
        foreach (Character ch in Gameplay.CurrentCharacters)
        {
            if (ch.id != charRef.id)
            {
                Health health = PlayerController.PlayerInfo.health;
                health.Damage(5);
                ch.RevealAllReal();
                ch.KillByDemon(charRef);
            }
        }
    }
    public void Transform(Character charRef)
    {
        if (charRef.state == ECharacterState.Dead || charRef.state == ECharacterState.Revealed)
        {
            return;
        }
        charRef.dataRef = Stats.StatsGetData("Death");
        runCountDown = true;
        // Just in case
        this.charRef = charRef;
        MelonLogger.Msg("Soul Collector Transformed!");
        Stats.dethTimer.SetActive(true);
    }
    public void ReduceCountdown()
    {
        countdown--;
        if (countdown == 0)
        {
            BeginAttack();
            Stats.dethTimer.SetActive(false);
            runCountDown = false;
        }
    }
    public void StopCountdown()
    {
        runCountDown = false;
        countdown = 1200;
        Stats.dethTimer.SetActive(false);
    }

    public override int GetDamageToYou()
    {
        return 0;
    }
    public SoulCollector() : base(ClassInjector.DerivedConstructorPointer<SoulCollector>())
    {
        ClassInjector.DerivedConstructorBody((Il2CppObjectBase)this);
    }
    public SoulCollector(IntPtr ptr) : base(ptr)
    {

    }
}

[RegisterTypeInIl2Cpp]
public class CountdownTimer : MonoBehaviour
{
    public TextMeshProUGUI? timer;
    public void UpdateCountdown(int countdown)
    {
        if (timer == null)
        {
            return;
        }
        timer.text = "<align=\"center\"><b>TIME LEFT:\n0:" + FormatInt(countdown) + "</b></align>";
    }
    public string FormatInt(int num)
    {
        if (num < 10)
        {
            return "0" + num;
        }
        return num.ToString();
    }
    public void SetTextItem(TextMeshProUGUI newTimer)
    {
        timer = newTimer;
    }
    public CountdownTimer() : base(ClassInjector.DerivedConstructorPointer<CountdownTimer>())
    {
        ClassInjector.DerivedConstructorBody((Il2CppObjectBase)this);
    }
    public CountdownTimer(IntPtr ptr) : base(ptr)
    {

    }
}

[HarmonyPatch(typeof(Character), nameof(Character.Act))]
public static class CheckExecution
{
    public static void Postfix(Character __instance, ETriggerPhase trigger)
    {
        if (trigger == ETriggerPhase.OnExecuted && __instance.alignment == EAlignment.Good)
        {
            foreach (Character ch in Gameplay.CurrentCharacters)
            {
                if (ch.dataRef.characterId == "SoulCollector_RCol")
                {
                    SoulCollector sc = Stats.soulCollector;
                    sc.Transform(ch);
                }
            }
        }
    }
}

[HarmonyPatch(typeof(Character), nameof(Character.Reveal))]
public static class CheckReaped
{
    public static void Postfix(Character __instance)
    {
        if (__instance.statuses.Contains(Stats.reaped))
        {
            __instance.UpdateRegisterAsRole(Stats.StatsGetData("Soul Collector"));
        }
    }
}

[HarmonyPatch(typeof(Dreamer), nameof(Dreamer.CharacterPicked))]
public static class GetCabbageCollector
{
    public static bool Prefix(Dreamer __instance)
    {
        Character c = CharacterPicker.PickedCharacters[0];
        if (c.dataRef.name == "Wretch" && c.statuses.Contains(Stats.reaped))
        {
            Il2CppSystem.Collections.Generic.List<Character> picked = new Il2CppSystem.Collections.Generic.List<Character>();
            picked.Add(c);
            string info = string.Format("#{0} could be: A Cabbage Collector", c.id);
            __instance.onActed?.Invoke(new ActedInfo(info, picked));
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Characters), nameof(Characters.RevealAll))]
public static class StopOnGameEnd
{
    public static void Postfix(Characters __instance)
    {
        SoulCollector soulCollector = Stats.GetSoulCollector();
        soulCollector.StopCountdown();
    }
}