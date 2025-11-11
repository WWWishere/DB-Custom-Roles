using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using MelonLoader;

namespace RolesCollection;

[RegisterTypeInIl2Cpp]
public class OutcastShepherd : Role
{
    public CharacterData sheep;
    public override ActedInfo GetInfo(Character charRef)
    {
        return new ActedInfo("");
    }
    public override void Act(ETriggerPhase trigger, Character charRef)
    {
        if (trigger == ETriggerPhase.Start)
        {
            Il2CppSystem.Collections.Generic.List<Character> list = CharactersHelper.GetSortedListWithCharacterFirst(Gameplay.CurrentCharacters, charRef);
            List<Character> neighbors = new List<Character>() { list[1], list[list.Count - 1] };
            foreach (Character ch in neighbors)
            {
                if (ch.dataRef.type == ECharacterType.Villager && ch.alignment == EAlignment.Good)
                {
                    ch.InitWithNoReset(sheep);
                    ch.statuses.AddStatus(ECharacterStatus.AlteredCharacter, charRef);
                }
            }
        }
        if (trigger != ETriggerPhase.OnExecuted)
        {
            return;
        }
        if (charRef.alignment != EAlignment.Evil)
        {
            Health health = PlayerController.PlayerInfo.health;
            health.Damage(4);
        }
    }
    public override void BluffAct(ETriggerPhase trigger, Character charRef)
    {

    }
    public OutcastShepherd(CharacterData sheepData) : base(ClassInjector.DerivedConstructorPointer<OutcastShepherd>())
    {
        ClassInjector.DerivedConstructorBody((Il2CppObjectBase)this);
        this.sheep = sheepData;
    }
    public OutcastShepherd(IntPtr ptr) : base(ptr)
    {

    }
}

[RegisterTypeInIl2Cpp]
public class Sheep : Role
{
    public override void Act(ETriggerPhase trigger, Character charRef)
    {

    }
    public override ActedInfo GetInfo(Character charRef)
    {
        return new ActedInfo("");
    }
    public Sheep() : base(ClassInjector.DerivedConstructorPointer<Sheep>())
    {
        ClassInjector.DerivedConstructorBody((Il2CppObjectBase)this);
    }
    public Sheep(IntPtr ptr) : base(ptr)
    {

    }
}