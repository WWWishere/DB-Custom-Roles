using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using MelonLoader;
using HarmonyLib;
using System.Data;

namespace RolesCollection;

[RegisterTypeInIl2Cpp]
public class Lookout2 : Role
{
    public Il2CppSystem.Action actionPicked;
    public Il2CppSystem.Action actionPickedDrunk;
    public Il2CppSystem.Action actionStopPicked;
    public int baronSwap;
    public List<int>[] affectedByTown;
    public bool checking = false;
    public int trackingChar = 0;
    public override ActedInfo GetInfo(Character charRef)
    {
        return new ActedInfo("");
    }
    public override void Act(ETriggerPhase trigger, Character charRef)
    {
        if (trigger != ETriggerPhase.Day)
        {
            return;
        }
        CharacterPicker.Instance.StartPickCharacters(1);
        CharacterPicker.OnCharactersPicked += actionPicked;
        CharacterPicker.OnStopPick += actionStopPicked;
    }
    public void StopPick()
    {
        CharacterPicker.OnCharactersPicked -= actionPickedDrunk;
        CharacterPicker.OnCharactersPicked -= actionPicked;
        CharacterPicker.OnStopPick -= actionStopPicked;
    }
    public void CharacterPicked()
    {
        CharacterPicker.OnCharactersPicked -= actionPicked;
        CharacterPicker.OnStopPick -= actionStopPicked;
        string info = "";
        foreach (Character c in CharacterPicker.PickedCharacters)
        {
            List<int> affectedVals = affectedByTown[c.id - 1];
            if (affectedVals.Count == 0)
            {
                info = string.Format("#{0} did not affect anyone", c.id);
                break;
            }
            info = string.Format("#{0} has affected ", c.id);
            int i = 0;
            foreach (int affected in affectedVals)
            {
                string addition = ", #" + affected;
                if (i == 0)
                {
                    addition = "#" + affected;
                }
                info += addition;
                i++;
            }
        }
        Il2CppSystem.Collections.Generic.List<Character> chars = new Il2CppSystem.Collections.Generic.List<Character>();
        foreach (Character ch in CharacterPicker.PickedCharacters)
        {
            chars.Add(ch);
        }
        ActedInfo actedInfo = new ActedInfo(info, chars);
        this.onActed?.Invoke(actedInfo);
    }
    public void CharacterPickedDrunk()
    {
        CharacterPicker.OnCharactersPicked -= actionPickedDrunk;
        CharacterPicker.OnStopPick -= actionStopPicked;
        string falseInfo = "";
        foreach (Character c in CharacterPicker.PickedCharacters)
        {
            List<int> affectedVals = affectedByTown[c.id - 1];
            if (affectedVals.Count > 0)
            {
                falseInfo = string.Format("#{0} did not affect anyone", c.id);
                break;
            }
            List<List<int>> lists = new List<List<int>>();
            foreach (List<int> otherAffected in affectedByTown)
            {
                if (otherAffected.Count > 0)
                {
                    lists.Add(otherAffected);
                }
            }
            falseInfo = string.Format("#{0} has affected ", c.id);
            List<Character> nearby = new List<Character>();
            int random = UnityEngine.Random.RandomRangeInt(0, 2);
            Il2CppSystem.Collections.Generic.List<Character> list1 = CharactersHelper.GetSortedListWithCharacterFirst(Gameplay.CurrentCharacters, c);
            nearby.Add(list1[1]);
            nearby.Add(list1[list1.Count - 1]);
            if (lists.Count == 0 || random == 0)
            {
                Character randChar = Stats.RandomItem(nearby);
                falseInfo += "#" + randChar.id;
            }
            else
            {
                List<int> otherAffectedVals = Stats.RandomItem(lists);
                int i = 0;
                foreach (int affected in otherAffectedVals)
                {
                    string addition = ", #" + affected;
                    if (i == 0)
                    {
                        addition = "#" + affected;
                    }
                    falseInfo += addition;
                    i++;
                }
            }
        }
        Il2CppSystem.Collections.Generic.List<Character> chars = new Il2CppSystem.Collections.Generic.List<Character>();
        foreach (Character ch in CharacterPicker.PickedCharacters)
        {
            chars.Add(ch);
        }
        ActedInfo actedInfo = new ActedInfo(falseInfo, chars);
        this.onActed?.Invoke(actedInfo);
    }
    public override ActedInfo GetBluffInfo(Character charRef)
    {
        return new ActedInfo("");
    }
    public override void BluffAct(ETriggerPhase trigger, Character charRef)
    {
        if (trigger != ETriggerPhase.Day)
        {
            return;
        }
        CharacterPicker.Instance.StartPickCharacters(1);
        CharacterPicker.OnCharactersPicked += actionPickedDrunk;
        CharacterPicker.OnStopPick += actionStopPicked;
    }
    public void InitAffected(int charCount)
    {
        affectedByTown = new List<int>[charCount];
        for (int i = 0; i < charCount; i++)
        {
            affectedByTown[i] = new List<int>();
        }
        // MelonLogger.Msg("Reseting affected: size - " + charCount);
    }
    public void AddAffectedCharacter(int refId, int affectedId)
    {
        // MelonLogger.Msg("Character affected: " + affectedId + " <- " + refId);
        List<int> list = affectedByTown[refId - 1];
        if (!list.Contains(affectedId))
        {
            list.Add(affectedId);
        }
    }
    public void SwapAffecteds(int id1, int id2)
    {
        // MelonLogger.Msg("Swapping ids: " + id1 + ", " + id2);
        List<int> temp = affectedByTown[id1 - 1];
        affectedByTown[id1 - 1] = affectedByTown[id2 - 1];
        affectedByTown[id2 - 1] = temp;
        foreach (List<int> list in affectedByTown)
        {
            int index1 = -1;
            int index2 = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == id1)
                {
                    index1 = i;
                }
                if (list[i] == id2)
                {
                    index2 = i;
                }
                if (index1 >= 0)
                {
                    list[index1] = id2;
                }
                if (index2 >= 0)
                {
                    list[index2] = id1;
                }
            }
        }
    }
    public Lookout2() : base(ClassInjector.DerivedConstructorPointer<Lookout2>())
    {
        ClassInjector.DerivedConstructorBody((Il2CppObjectBase)this);
        actionPicked = new System.Action(CharacterPicked);
        actionPickedDrunk = new System.Action(CharacterPickedDrunk);
        actionStopPicked = new System.Action(StopPick);
        affectedByTown = new List<int>[] { };
    }
    public Lookout2(IntPtr ptr) : base(ptr)
    {

    }
}
// Good characters that can affect -> Alchemist, Baker, Plague Doctor, Slayer, 
[HarmonyPatch(typeof(Character), nameof(Character.Act))]
public static class TrackCharacter
{
    public static bool Prefix(Character __instance, ref ETriggerPhase trigger)
    {
        Lookout2 lookout = Stats.GetLookout();
        lookout.checking = true;
        lookout.trackingChar = __instance.id;
        return true;
    }
    public static void Postfix(Character __instance, ref ETriggerPhase trigger)
    {
        Lookout2 lookout = Stats.GetLookout();
        lookout.checking = false;
    }
}
[HarmonyPatch(typeof(Character), nameof(Character.InitWithNoReset))]
public static class CheckCharInit
{
    public static void Postfix(Character __instance, ref CharacterData character)
    {
        Lookout2 lookout = Stats.GetLookout();
        if (lookout.checking && character == Stats.StatsGetData("Baker"))
        {
            lookout.AddAffectedCharacter(lookout.trackingChar, __instance.id);
        }
    }
}
[HarmonyPatch(typeof(Slayer), nameof(Slayer.CharacterPicked))]
public static class TrackSlayer
{
    public static bool Prefix(Slayer __instance)
    {
        Lookout2 lookout = Stats.GetLookout();
        lookout.checking = true;
        lookout.trackingChar = __instance.chRef.id;
        return true;
    }
    public static void Postfix(Slayer __instance)
    {
        Lookout2 lookout = Stats.GetLookout();
        lookout.checking = false;
    }
}
[HarmonyPatch(typeof(Character), nameof(Character.KillAndReveal))]
public static class CheckSlayerKill
{
    public static void Postfix(Character __instance)
    {
        Lookout2 lookout = Stats.GetLookout();
        if (lookout.checking)
        {
            lookout.AddAffectedCharacter(lookout.trackingChar, __instance.id);
        }
    }
}
[HarmonyPatch(typeof(Character), nameof(Character.KillByDemon))]
public static class CheckDemonKill
{
    public static void Postfix(Character __instance, ref Character evilRef)
    {
        Lookout2 lookout = Stats.GetLookout();
        if (lookout.checking)
        {
            lookout.AddAffectedCharacter(evilRef.id, __instance.id);
        }
    }
}

[HarmonyPatch(typeof(CharacterStatuses), nameof(CharacterStatuses.AddStatus))]
public static class CheckNewStatus
{
    public static void Postfix(CharacterStatuses __instance, ref ECharacterStatus newStatus, ref Character sourceRef)
    {
        if (sourceRef == null)
        {
            return;
        }
        if (newStatus == ECharacterStatus.CorruptionResistant || newStatus == ECharacterStatus.HealthyBluff || newStatus == ECharacterStatus.UnkillableByDemon)
        {
            return;
        }
        foreach (Character ch in Gameplay.CurrentCharacters)
        {
            if (ch.statuses == __instance)
            {
                Lookout2 lookout = Stats.GetLookout();
                lookout.AddAffectedCharacter(sourceRef.id, ch.id);
            }
        }
    }
}

[HarmonyPatch(typeof(CharacterStatuses), nameof(CharacterStatuses.CheckIfCanCurePoisonAndCure))]
public static class CheckCure
{
    public static void Postfix(CharacterStatuses __instance, ref bool __result)
    {
        Lookout2 lookout = Stats.GetLookout();
        if (lookout.checking && __result)
        {
            foreach (Character ch in Gameplay.CurrentCharacters)
            {
                if (ch.statuses == __instance)
                {
                    lookout.AddAffectedCharacter(lookout.trackingChar, ch.id);
                }
            }
        }
    }
}

[HarmonyPatch(typeof(Characters), nameof(Characters.ManageCharacters))]
public static class ResetAffected
{
    public static bool Prefix(Characters __instance, ref Il2CppSystem.Collections.Generic.List<CharacterData> charactersList)
    {
        Lookout2 lookout = Stats.GetLookout();
        lookout.InitAffected(charactersList.Count);
        return true;
    }
}

[HarmonyPatch(typeof(Character), nameof(Character.Init))]
public static class SwapIndex
{
    public static void Postfix(Character __instance, ref CharacterData character)
    {
        Lookout2 lookout = Stats.GetLookout();
        if (lookout.checking && character == Stats.StatsGetData("Counsellor"))
        {
            lookout.baronSwap = __instance.id;
        }
    }
}

[HarmonyPatch(typeof(Baron), nameof(Baron.SitNextToOutsider))]
public static class CheckSwap
{
    public static void Postfix(Baron __instance, ref Character charRef)
    {
        Lookout2 lookout = Stats.GetLookout();
        if (lookout.checking)
        {
            lookout.SwapAffecteds(lookout.baronSwap, charRef.id);
        }
    }
}