using System;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;


[assembly: MelonInfo(typeof(RolesCollection.Main), "Role Ideas Collection", "1.0", "SS122")]
[assembly: MelonGame("UmiArt", "Demon Bluff")]
namespace RolesCollection;

public class Main : MelonMod
{
    public override void OnInitializeMelon()
    {
        ClassInjector.RegisterTypeInIl2Cpp<Lookout>();
    }
    public override void OnLateInitializeMelon()
    {
        Application.runInBackground = true;
        Stats.GetStartingRoles();
        CharacterData lookout = Stats.createCharData("Lookout", "Witness", ECharacterType.Villager,
        EAlignment.Good, Stats.lookout, true, EAbilityUsage.ResetAfterNight);
        lookout.bluffable = true;
        lookout.description = "Pick a Character. Learn who they affected.";
        lookout.flavorText = "\"Could've been a sniper. She was too busy watching the Puppeteer.\"";
        lookout.characterId = "Lookout_RCol";
        foreach (CustomScriptData scriptData in ProjectContext.Instance.gameData.advancedAscension.possibleScriptsData)
        {
            ScriptInfo script = scriptData.scriptInfo;
            addRole(script.startingTownsfolks, lookout);
        }
    }
    public override void OnUpdate()
    {
        if (Stats.allDatas.Length == 0)
        {
            var loadedCharList = Resources.FindObjectsOfTypeAll(Il2CppType.Of<CharacterData>());
            if (loadedCharList != null)
            {
                Stats.allDatas = new CharacterData[loadedCharList.Length];
                for (int i = 0; i < loadedCharList.Length; i++)
                {
                    CharacterData data = loadedCharList[i]!.Cast<CharacterData>();
                    Stats.CheckAddRole(data);
                    Stats.allDatas[i] = data;
                }
            }
            if (Stats.allDatas.Length > 0)
            {
                Stats.OnFirstUpdate();
            }
        }
    }
    public void addDemonRole(AscensionsData advancedAscension, CharacterData? data, string oldScriptName, string newScriptName, int weight = 1)
    {
        if (data == null)
        {
            return;
        }
        foreach (CustomScriptData scriptData in advancedAscension.possibleScriptsData)
        {
            if (scriptData.name == oldScriptName)
            {
                CustomScriptData newScriptData = GameObject.Instantiate(scriptData);
                newScriptData.name = newScriptName;
                ScriptInfo newScript = new ScriptInfo();
                ScriptInfo script = scriptData.scriptInfo;
                newScriptData.scriptInfo = newScript;
                newScript.startingTownsfolks = script.startingTownsfolks;
                newScript.startingOutsiders = script.startingOutsiders;
                newScript.startingMinions = script.startingMinions;
                newScript.characterCounts = script.characterCounts;
                newScript.startingDemons = new Il2CppSystem.Collections.Generic.List<CharacterData>();
                newScript.startingDemons.Add(data);
                var newPSD = advancedAscension.possibleScriptsData.Append(newScriptData);
                for (int i = 0; i < weight - 1; i++)
                {
                    newPSD = newPSD.Append(newScriptData);
                }
                advancedAscension.possibleScriptsData = newPSD.ToArray();
                return;
            }
        }
    }
    public void addRole(Il2CppSystem.Collections.Generic.List<CharacterData> list, CharacterData? data)
    {
        if (data == null)
        {
            return;
        }
        if (list.Contains(data))
        {
            return;
        }
        list.Add(data);
    }
    public CharacterData? GetData(string name)
    {
        return Stats.StatsGetData(name);
    }
}
public static class Stats
{
    public static Dictionary<string, CharacterData> roles = new Dictionary<string, CharacterData>();
    public static CharacterData[] allDatas = Array.Empty<CharacterData>();
    public static ECharacterStatus phantom = (ECharacterStatus)151;
    public static Lookout lookout = new Lookout();
    public static CharacterData? StatsGetData(string name)
    {
        if (Stats.roles.ContainsKey(name))
        {
            return Stats.roles[name];
        }
        MelonLogger.Msg("Couldn't find CharacterData for " + name + "!");
        return null;
    }
    public static List<T> GetList<T>(Il2CppSystem.Collections.Generic.List<T> ilList)
    {
        List<T> list = new List<T>();
        foreach (T item in ilList)
        {
            list.Add(item);
        }
        return list;
    }
    public static T RandomItem<T>(Il2CppSystem.Collections.Generic.List<T> ilList)
    {
        return ilList[UnityEngine.Random.RandomRangeInt(0, ilList.Count)];
    }
    public static T RandomItem<T>(List<T> ilList)
    {
        return ilList[UnityEngine.Random.RandomRangeInt(0, ilList.Count)];
    }
    public static Lookout GetLookout()
    {
        return lookout;
    }
    public static void OnFirstUpdate()
    {
        int i = 0;
        foreach (var pair in roles)
        {
            // MelonLogger.Msg("#" + i + ": " + pair.Key + " -> " + pair.Value.characterId);
            i++;
        }
    }
    public static void GetStartingRoles()
    {
        AscensionsData allCharactersAscension = ProjectContext.Instance.gameData.allCharactersAscension;
        foreach (CharacterData data in allCharactersAscension.startingTownsfolks)
        {
            CheckAddRole(data);
        }
        foreach (CharacterData data in allCharactersAscension.startingOutsiders)
        {
            CheckAddRole(data);
        }
        foreach (CharacterData data in allCharactersAscension.startingMinions)
        {
            CheckAddRole(data);
        }
        foreach (CharacterData data in allCharactersAscension.startingDemons)
        {
            CheckAddRole(data);
        }
    }
    public static void CheckAddRole(CharacterData data)
    {
        string name = data.name;
        if (!roles.ContainsKey(name))
        {
            roles.Add(name, data);
        }
    }

    public static CharacterData createCharData(string name, string getBGfrom, ECharacterType type, EAlignment alignment, Role role, bool picking = false, EAbilityUsage abilityUsage = EAbilityUsage.Once)
    {
        // unadded: tags, description, notes, bluffable
        CharacterData newData = new CharacterData();
        newData.name = name;
        newData.abilityUsage = abilityUsage;
        CharacterData? bgData = StatsGetData(getBGfrom);
        if (bgData != null)
        {
            newData.backgroundArt = bgData.backgroundArt;
        }
        newData.bundledCharacters = new Il2CppSystem.Collections.Generic.List<CharacterData>();
        newData.canAppearIf = new Il2CppSystem.Collections.Generic.List<CharacterData>();
        switch (type)
        {
            case ECharacterType.Villager:
                newData.artBgColor = new Color(0.111f, 0.0833f, 0.1415f);
                newData.cardBgColor = new Color(0.26f, 0.1519f, 0.3396f);
                newData.cardBorderColor = new Color(0.7133f, 0.339f, 0.8679f);
                newData.color = new Color(1f, 0.935f, 0.7302f);
                break;
            case ECharacterType.Outcast:
                newData.artBgColor = new Color(0.3679f, 0.2014f, 0.1541f);
                newData.cardBgColor = new Color(0.102f, 0.0667f, 0.0392f);
                newData.cardBorderColor = new Color(0.7843f, 0.6471f, 0f);
                newData.color = new Color(0.9659f, 1f, 0.4472f);
                break;
            case ECharacterType.Minion:
                newData.artBgColor = new Color(1f, 0f, 0f);
                newData.cardBgColor = new Color(0.0941f, 0.0431f, 0.0431f);
                newData.cardBorderColor = new Color(0.8208f, 0f, 0.0241f);
                newData.color = new Color(0.8491f, 0.4555f, 0f);
                break;
            case ECharacterType.Demon:
                newData.artBgColor = new Color(1f, 0f, 0f);
                newData.cardBgColor = new Color(0.0941f, 0.0431f, 0.0431f);
                newData.cardBorderColor = new Color(0.8208f, 0f, 0.0241f);
                newData.color = new Color(1f, 0.3811f, 0.3811f);
                break;
        }
        newData.descriptionCHN = "";
        newData.descriptionPL = "";
        newData.hints = "";
        newData.ifLies = "";
        newData.notes = "";
        newData.picking = picking;
        newData.role = role;
        newData.skins = new Il2CppSystem.Collections.Generic.List<SkinData>();
        newData.startingAlignment = alignment;
        newData.type = type;
        return newData;
    }
}
