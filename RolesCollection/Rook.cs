using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using MelonLoader;
using HarmonyLib;
using UnityEngine;

namespace RolesCollection;

[RegisterTypeInIl2Cpp]
public class Rook : Role
{
    public Il2CppSystem.Action actionSwap;
    public List<Character> charRefs;
    public List<RookSwap> swapOrder;
    public Transform? swappedCircle;
    public List<int> possibleDistances;
    public Dictionary<int, RookData> rookDatas = new Dictionary<int, RookData>();
    public RookData? GetRookData(Character charRef)
    {
        int key = charRef.id;
        if (rookDatas.ContainsKey(key))
        {
            return rookDatas[key];
        }
        return null;
    }
    public override ActedInfo GetInfo(Character charRef)
    {
        RookData? rookData = GetRookData(charRef);
        if (rookData == null)
        {
            return new ActedInfo("I couldn't swap anyone");
        }
        string line = string.Format("I swapped two characters {0} cards apart", rookData.swapDistance);
        return new ActedInfo(line);
    }
    public override void Act(ETriggerPhase trigger, Character charRef)
    {
        if (trigger == ETriggerPhase.Start)
        {
            GameplayEvents.OnDeckShuffled += actionSwap;
            charRefs.Add(charRef);
        }
        if (trigger != ETriggerPhase.Day)
        {
            return;
        }
        this.onActed.Invoke(this.GetInfo(charRef));
    }
    public override ActedInfo GetBluffInfo(Character charRef)
    {
        int rand = Stats.RandomItem(possibleDistances);
        string line = string.Format("I swapped two characters {0} cards apart", rand);
        return new ActedInfo(line);
    }
    public override void BluffAct(ETriggerPhase trigger, Character charRef)
    {
        if (trigger != ETriggerPhase.Day)
        {
            return;
        }
        this.onActed.Invoke(this.GetBluffInfo(charRef));
    }
    public void StartSwap()
    {
        GameplayEvents.OnDeckShuffled -= actionSwap;
        Character nextSwapper = charRefs[0];
        int swapperId = nextSwapper.id;
        List<Character> listDisguised = new List<Character>();
        foreach (Character ch in Gameplay.CurrentCharacters)
        {
            if (ch.id != swapperId && ch.GetCharacterBluffIfAble().name != "Rook" && ch.bluff != null)
            {
                listDisguised.Add(ch);
            }
        }
        if (listDisguised.Count < 2)
        {
            return;
        }
        Character rand1 = Stats.RandomItem(listDisguised);
        listDisguised.Remove(rand1);
        Character rand2 = Stats.RandomItem(listDisguised);
        CharacterData r1Data = rand1.dataRef;
        CharacterData r2Data = rand2.dataRef;
        CharacterStatuses r1Statuses = rand1.statuses;
        CharacterStatuses r2Statuses = rand2.statuses;
        RuntimeCharacterData r1CData = rand1.GetRuntimeData();
        RuntimeCharacterData r2CData = rand2.GetRuntimeData();
        rand1.dataRef = r2Data;
        rand1.statuses = r2Statuses;
        rand1.runtimeData = r2CData;
        rand2.dataRef = r1Data;
        rand2.statuses = r1Statuses;
        rand2.runtimeData = r1CData;
        int s1 = Math.Min(rand1.id, rand2.id);
        int s2 = Math.Max(rand1.id, rand2.id);
        int count = Gameplay.CurrentCharacters.Count;
        int diff1 = s2 - s1;
        int diff2 = s1 + count - s2;
        int distance = Math.Min(diff1, diff2);
        rookDatas.Add(swapperId, new RookData(rand1, rand2, distance));
        possibleDistances.Remove(distance);
        swapOrder.Add(new RookSwap(rand1.id, rand2.id));
        charRefs.Remove(nextSwapper);
    }
    // Just in case function
    public void CheckForSwappedRook(int id1, int id2)
    {
        if (rookDatas.ContainsKey(id1))
        {
            RookData rookData = rookDatas[id1];
            if (rookDatas.ContainsKey(id2))
            {
                RookData rookData2 = rookDatas[id2];
                rookDatas.Remove(id1);
                rookDatas.Remove(id2);
                rookDatas.Add(id1, rookData2);
                rookDatas.Add(id2, rookData);
            }
            else
            {
                rookDatas.Remove(id1);
                rookDatas.Add(id2, rookData);
            }
        }
        else if (rookDatas.ContainsKey(id2))
        {
            RookData rookData = rookDatas[id2];
            rookDatas.Remove(id2);
            rookDatas.Add(id1, rookData);
        }
    }
    public void SwapIds(int id1, int id2)
    {
        Character? ch1 = null;
        Character? ch2 = null;
        foreach (Character ch in Gameplay.CurrentCharacters)
        {
            if (ch.id == id1)
            {
                ch1 = ch;
            }
            else if (ch.id == id2)
            {
                ch2 = ch;
            }
        }
        if (ch1 == null || ch2 == null)
        {
            return;
        }
        ch1.id = id2;
        ch2.id = id1;
    }
    public void fixCircle()
    {
        Transform? circle = swappedCircle;
        if (circle == null)
        {
            return;
        }
        int childCount = circle.childCount;
        int i = 0;
        for (int j = 0; j < childCount; j++)
        {
            Transform card = circle.GetChild(j);
            Character ch = card.GetComponent<Character>();
            if (ch != null)
            {
                ch.id = childCount - i;
                i++;
            }
        }
    }
    // Activate when a new game starts, just before each character begins start phase
    public void ResetAllLists()
    {
        rookDatas.Clear();
        swapOrder.Clear();
        charRefs.Clear();
        possibleDistances.Clear();
        for (int i = 1; i <= Gameplay.CurrentCharacters.Count / 2; i++)
        {
            possibleDistances.Add(i);
        }
    }
    public Rook() : base(ClassInjector.DerivedConstructorPointer<Rook>())
    {
        ClassInjector.DerivedConstructorBody((Il2CppObjectBase)this);
        actionSwap = new System.Action(StartSwap);
        charRefs = new List<Character>();
        swapOrder = new List<RookSwap>();
        swappedCircle = null;
        rookDatas = new Dictionary<int, RookData>();
        possibleDistances = new List<int>();
    }
    public Rook(IntPtr ptr) : base(ptr)
    {

    }
}

public class RookData
{
    public int swapped1;
    public int swapped2;
    public int swapDistance;
    public Il2CppSystem.Collections.Generic.List<Character> targets;
    public RookData(Character swap1, Character swap2, int distance)
    {
        this.swapped1 = swap1.id;
        this.swapped2 = swap2.id;
        this.targets = new Il2CppSystem.Collections.Generic.List<Character>();
        this.targets.Add(swap1);
        this.targets.Add(swap2);
        this.swapDistance = distance;
    }
}

public class RookSwap
{
    public int swap1;
    public int swap2;
    public RookSwap(int swap1, int swap2)
    {
        this.swap1 = swap1;
        this.swap2 = swap2;
    }
}

// Awful implementation
[HarmonyPatch(typeof(Character), nameof(Character.Act))]
public static class ModifyCircleValues
{
    public static bool Prefix(Character __instance, ETriggerPhase trigger)
    {
        if (trigger == ETriggerPhase.Day)
        {
            Rook rook = Stats.GetRook();
            foreach (RookSwap swap in rook.swapOrder)
            {
                rook.SwapIds(swap.swap1, swap.swap2);
            }
            rook.swappedCircle = __instance.gameObject.transform.GetParent();
        }
        return true;
    }
    public static void Postfix(Character __instance, ETriggerPhase trigger)
    {
        Rook rook = Stats.GetRook();
        if (rook.swappedCircle != null)
        {
            rook.fixCircle();
            rook.swappedCircle = null;
        }
    }
}