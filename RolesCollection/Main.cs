using System;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


[assembly: MelonInfo(typeof(RolesCollection.Main), "Role Ideas Collection", "1.3", "SS122")]
[assembly: MelonGame("UmiArt", "Demon Bluff")]
namespace RolesCollection;

public class Main : MelonMod
{
    bool disableRook = false;
    public override void OnInitializeMelon()
    {
        ClassInjector.RegisterTypeInIl2Cpp<Lookout2>();
        ClassInjector.RegisterTypeInIl2Cpp<SoulCollector>();
        ClassInjector.RegisterTypeInIl2Cpp<Rook>();
        ClassInjector.RegisterTypeInIl2Cpp<Sheep>();
        ClassInjector.RegisterTypeInIl2Cpp<OutcastShepherd>();
        ClassInjector.RegisterTypeInIl2Cpp<CountdownTimer>();
    }
    public override void OnLateInitializeMelon()
    {
        Application.runInBackground = true;
        var loadedFonts = Resources.FindObjectsOfTypeAll(Il2CppSystem.Type.GetTypeFromHandle(RuntimeReflectionHelper.GetRuntimeTypeHandle<TMP_FontAsset>()));
        Stats.fonts = new TMP_FontAsset[loadedFonts.Length];
        for (int i = 0; i < loadedFonts.Length; i++)
        {
            TMP_FontAsset asset = loadedFonts[i]!.Cast<TMP_FontAsset>();
            // MelonLogger.Msg("Font asset: " + asset.name);
            Stats.fonts[i] = asset;
        }
        Stats.GetStartingRoles();
        Stats.CreateApocalypseTag();
        Stats.CreateDeathTimer(Stats.dethTimer);
        CharacterData lookout = Stats.createCharData("Lookout", "Witness", ECharacterType.Villager,
        EAlignment.Good, Stats.lookout, true, EAbilityUsage.ResetAfterNight);
        lookout.bluffable = true;
        lookout.description = "Pick a Character. Learn who they affected.";
        lookout.flavorText = "\"Could've been a sniper. She was too busy watching the Puppeteer.\"";
        lookout.characterId = "Lookout_RCol";
        CharacterData soulCollector = Stats.createCharData("Soul Collector", "Baa", ECharacterType.Demon,
        EAlignment.Evil, Stats.soulCollector);
        soulCollector.bluffable = false;
        soulCollector.description = "<b>Game Start:</b>\nReap 2 characters.\nReaped characters Register as a Soul Collector.\n\nWhen you execute a good character, I become <b>Death.</b>";
        soulCollector.flavorText = "\"Is afraid of the dark so they carry a lantern with them\"";
        soulCollector.characterId = "SoulCollector_RCol";
        CharacterData death = Stats.createCharData("Death", "Baa", ECharacterType.Demon,
        EAlignment.Evil, Stats.soulCollector);
        death.bluffable = false;
        death.description = "A 20-second timer starts.\nIf time runs out, I will kill all other characters, dealing 5 damage to you for each character.";
        death.flavorText = "\"Your time is almost up\"";
        death.characterId = "Death_RCol";
        soulCollector.type = Stats.apocalypse;
        death.type = Stats.apocalypse;
        CharacterData rook = Stats.createCharData("Rook", "Bombardier", ECharacterType.Outcast,
        EAlignment.Good, Stats.rook);
        rook.bluffable = true;
        rook.description = "Swaps around two random disguised characters, making any information mentioning their number and position obsolete.";
        rook.flavorText = "\"And he sacrifices..... THE ROOK!\"";
        rook.characterId = "Rook_RCol";
        // Funny shepherd role
        /*
        CharacterData sheep = Stats.createCharData("Sheep", "Bombardier", ECharacterType.Villager,
        EAlignment.Good, new Sheep());
        sheep.bluffable = false;
        sheep.description = "A sheep.";
        sheep.flavorText = "\"Baaaaaaa...\"";
        sheep.characterId = "Sheep_RCol";
        CharacterData shepherd = Stats.createCharData("Shepherd", "Bombardier", ECharacterType.Outcast,
        EAlignment.Good, new OutcastShepherd(sheep));
        shepherd.bluffable = true;
        shepherd.description = "Adjacent Good Villagers become Sheep.";
        shepherd.hints = "If I am Executed while Good:\nI will take away 4 additional health from you.";
        shepherd.flavorText = "\"Baa?\"";
        shepherd.characterId = "Shepherd_RCol";
        Characters.Instance.startGameActOrder = insertAfterAct("Shaman", shepherd);
        */
        Characters.Instance.startGameActOrder = insertAfterAct("Chancellor", rook);
        Characters.Instance.startGameActOrder = insertAfterAct("Lilis", soulCollector);

        foreach (CustomScriptData scriptData in ProjectContext.Instance.gameData.advancedAscension.possibleScriptsData)
        {
            ScriptInfo script = scriptData.scriptInfo;
            addRole(script.startingTownsfolks, lookout);
            if (!disableRook)
            {
                addRole(script.startingOutsiders, rook);
            }
        }
        CharactersCount sc10 = new CharactersCount(10, 7, 1, 2, 0);
        CharactersCount sc9 = new CharactersCount(9, 6, 1, 2, 0);
        addDemonRole(ProjectContext.Instance.gameData.advancedAscension, soulCollector, new List<CharactersCount>() { sc9, sc10, sc10 }, "SC_Test", 2);
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
                    if (data.name == "Pestilence")
                    {
                        // MelonLogger.Msg("Found Pestilence! in " + data.characterId);
                        data.type = Stats.apocalypse;
                    }
                    if (data.name == "Rook" && data.characterId != "Rook_RCol")
                    {
                        disableRook = true;
                    }
                    Stats.CheckAddRole(data);
                    Stats.allDatas[i] = data;
                }
            }
            if (Stats.allDatas.Length > 0)
            {
                Stats.OnFirstUpdate();
            }
        }
        SoulCollector sc = Stats.soulCollector;
        if (sc.runCountDown)
        {
            if (sc.countdown % 60 == 0)
            {
                int seconds = sc.countdown / 60;
                MelonLogger.Msg("Time left: " + seconds + " seconds");
                CountdownTimer timer = Stats.dethTimer.GetComponent<CountdownTimer>();
                timer.UpdateCountdown(seconds);
            }
            sc.ReduceCountdown();
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

    public void addDemonRole(AscensionsData advancedAscension, CharacterData? data, List<CharactersCount> counts, string newScriptName, int weight = 1)
    {
        if (data == null)
        {
            return;
        }
        CustomScriptData newScriptData = new CustomScriptData();
        newScriptData.name = newScriptName;
        ScriptInfo newScript = new ScriptInfo();
        ScriptInfo script = ProjectContext.Instance.gameData.advancedAscension.possibleScriptsData[0].scriptInfo;
        newScriptData.scriptInfo = newScript;
        newScript.startingTownsfolks = script.startingTownsfolks;
        newScript.startingOutsiders = script.startingOutsiders;
        newScript.startingMinions = script.startingMinions;
        foreach (CharactersCount count in counts)
        {
            newScript.characterCounts.Add(count);
        }
        newScript.startingDemons = new Il2CppSystem.Collections.Generic.List<CharacterData>();
        newScript.startingDemons.Add(data);
        var newPSD = advancedAscension.possibleScriptsData.Append(newScriptData);
        for (int i = 0; i < weight - 1; i++)
        {
            newPSD = newPSD.Append(newScriptData);
        }
        advancedAscension.possibleScriptsData = newPSD.ToArray();
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
    public CharacterData[] insertAfterAct(string previous, CharacterData data)
    {
        CharacterData[] actList = Characters.Instance.startGameActOrder;
        int actSize = actList.Length;
        CharacterData[] newActList = new CharacterData[actSize + 1];
        bool inserted = false;
        for (int i = 0; i < actSize; i++)
        {
            if (inserted)
            {
                newActList[i + 1] = actList[i];
            }
            else
            {
                newActList[i] = actList[i];
                if (actList[i].name == previous)
                {
                    newActList[i + 1] = data;
                    inserted = true;
                }
            }
        }
        if (!inserted)
        {
            LoggerInstance.Msg("");
        }
        return newActList;
    }
}
public static class Stats
{
    public static Dictionary<string, CharacterData> roles = new Dictionary<string, CharacterData>();
    public static CharacterData[] allDatas = Array.Empty<CharacterData>();
    public static ECharacterStatus reaped = (ECharacterStatus)205;
    public static Lookout2 lookout = new Lookout2();
    public static Rook rook = new Rook();
    public static SoulCollector soulCollector = new SoulCollector();
    public static ECharacterType apocalypse = (ECharacterType)101;
    public static GenericHint hint = FindGeneralHint();
    public static GameObject dethTimer = new GameObject();
    public static TMP_FontAsset[] fonts = Array.Empty<TMP_FontAsset>();
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
    public static T RandomItem<T>(List<T> list)
    {
        return list[UnityEngine.Random.RandomRangeInt(0, list.Count)];
    }
    public static Lookout2 GetLookout()
    {
        return lookout;
    }
    public static Rook GetRook()
    {
        return rook;
    }
    public static SoulCollector GetSoulCollector()
    {
        return soulCollector;
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

    public static void CreateApocalypseTag()
    {
        GameObject info = GameObject.Find("Game/MoreInfo");
        GameObject ruleBook = GameObject.Find("Game/Gameplay/Content/RuleBook/Panel");
        // Path 1: Game/MoreInfo/Skins_Change/Panel/CharacterHint (1)/Tags/Layout/
        // Path 2: Game/MoreInfo/CardExplanationUI/Panel/Tags/Tags/Layout/View (4)/
        // Path 3: Game/MoreInfo/SkinUnlocked_Details/Panel/CharacterHint (1)/Tags/Layout/
        // Path 4: Game/Gameplay/Content/RuleBook/Panel/CharacterHint (1)/Tags/Layout/
        // Path 5: Game/Gameplay/Content/RuleBook/Panel/CharacterHint (2)/Tags/Layout/
        // Path 6: Game/MoreInfo/CardExplanationUI/Panel/CharacterHint (1)/Tags/Layout/
        // Path 7: Game/MoreInfo/CharacterHint/Tags/Layout/
        // Path 8: Game/MoreInfo/AchievementDetails/Panel/CharacterHint (1)/Tags/Layout/
        // Path 9: Game/MoreInfo/CardExplanationUI/Panel/Tags/Tags/Layout/
        GameObject ch1 = info.transform.Find("Skins_Change/Panel/CharacterHint (1)").gameObject;
        GameObject ch2 = info.transform.Find("CardExplanationUI/Panel/Tags").gameObject;
        GameObject ch3 = info.transform.Find("SkinUnlocked_Details/Panel/CharacterHint (1)").gameObject;
        GameObject ch4 = ruleBook.transform.Find("CharacterHint (1)").gameObject;
        GameObject ch5 = ruleBook.transform.Find("CharacterHint (2)").gameObject;
        GameObject ch6 = info.transform.Find("CardExplanationUI/Panel/CharacterHint (1)").gameObject;
        GameObject ch7 = info.transform.Find("CharacterHint").gameObject;
        GameObject ch8 = info.transform.Find("AchievementDetails/Panel/CharacterHint (1)").gameObject;
        AddApocalypseTag(ch1);
        AddApocalypseTag(ch3);
        AddApocalypseTag(ch4);
        AddApocalypseTag(ch5);
        AddApocalypseTag(ch6);
        AddApocalypseTag(ch7);
        AddApocalypseTag(ch8);
        CharacterHint ch2Hint = ch2.GetComponent<CharacterHint>();
        GameObject ch2View1 = ch2.transform.Find("Tags/Layout/View (4)").gameObject;
        Transform parent = ch2View1.transform.GetParent();
        GameObject apocHint = GameObject.Instantiate(ch2View1, parent);
        apocHint.transform.SetSiblingIndex(4);
        apocHint.name = "View (Apoc Tag)";
        CharacterTag apocHintTag = apocHint.GetComponent<CharacterTag>();
        apocHintTag.alignment = EAlignment.None;
        apocHintTag.type = apocalypse;
        GameObject apoc = apocHint.transform.GetChild(0).gameObject;
        apoc.name = "View (Apoc Tag)";
        CharacterTag apocTag = apoc.GetComponent<CharacterTag>();
        apocTag.alignment = EAlignment.None;
        apocTag.type = apocalypse;
        GameObject text = apoc.transform.GetChild(3).gameObject;
        TextMeshProUGUI apocText = text.GetComponent<TextMeshProUGUI>();
        apocText.text = "Apocalypse";
        var postAppend = ch2Hint.tags.Append(apocHintTag);
        ch2Hint.tags = postAppend.ToArray();
        EventTrigger eventTrigger = apocHint.GetComponent<EventTrigger>();
        UnityEngine.Object.Destroy(eventTrigger);
        ApocHint newTrigger = apocHint.AddComponent<ApocHint>();
        SimpleUIInfo uiInfo = GameObject.FindObjectOfType<SimpleUIInfo>();
        Transform apocHintPivot = apocHint.transform.GetChild(1);
        newTrigger.SetGeneralHint(hint.gameObject, uiInfo, apocHintPivot);
    }
    public static void AddApocalypseTag(GameObject tag)
    {
        GameObject view = tag.transform.Find("Tags/Layout/View (4)").gameObject;
        CharacterHint characterHint = tag.GetComponent<CharacterHint>();
        Transform parent = view.transform.GetParent();
        GameObject apoc = GameObject.Instantiate(view, parent);
        apoc.transform.SetSiblingIndex(4);
        apoc.name = "View (Apoc Tag)";
        CharacterTag apocTag = apoc.GetComponent<CharacterTag>();
        apocTag.alignment = EAlignment.None;
        apocTag.type = apocalypse;
        GameObject text = apoc.transform.GetChild(3).gameObject;
        TextMeshProUGUI apocText = text.GetComponent<TextMeshProUGUI>();
        apocText.text = "Apocalypse";
        var postAppend = characterHint.tags.Append(apocTag);
        characterHint.tags = postAppend.ToArray();
    }
    public static GenericHint FindGeneralHint()
    {
        SimpleUIInfo allHints = GameObject.FindObjectOfType<SimpleUIInfo>();
        GenericHint genericHint = allHints.genericHint;
        return genericHint;
    }
    public static void SetFont(TextMeshProUGUI textItem, string name)
    {
        foreach (TMP_FontAsset font in fonts)
        {
            if (font != null)
            {
                if (font.name == name)
                {
                    textItem.font = font;
                }
            }
        }
    }
    public static void CreateDeathTimer(GameObject timer)
    {
        // Missing parts: set the parent object of timer, set the local pos of timer
        timer.name = "Countdown";
        GameObject gameplay = GameObject.Find("Game/Gameplay/Content/Canvas/Characters");
        if (gameplay != null)
        {
            timer.transform.SetParent(gameplay.transform);
        }
        CountdownTimer cd = timer.AddComponent<CountdownTimer>();
        GameObject timerTM = new GameObject("Text (TMP)");
        timerTM.transform.SetParent(timer.transform);
        TextMeshProUGUI timerText = timerTM.AddComponent<TextMeshProUGUI>();
        cd.SetTextItem(timerText);
        timer.SetActive(false);
        timer.transform.localScale = new Vector3(1f, 1f, 1f);
        timerTM.transform.localScale = new Vector3(1f, 1f, 1f);
        timer.transform.localPosition = new Vector3(0f, 400f, 0f);
        SetFont(timerText, "Alata-Regular SDF");
    }
}