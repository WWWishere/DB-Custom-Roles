using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RolesCollection;

[RegisterTypeInIl2Cpp]
public class ApocHint : EventTrigger
{
    public GameObject generalHint;
    public GenericHint hint;
    public SimpleUIInfo ui;
    public Transform pivot;
    public override void OnPointerEnter(PointerEventData eventData)
    {
        generalHint.SetActive(true);
        TextMeshProUGUI text = hint.text;
        TextMeshProUGUI title = hint.title;
        title.gameObject.SetActive(true);
        title.text = "Apocalypse";
        text.text = "Boss level demon.\n\nIs normally the only evil in the village. Has the power to make you lose instantly if you're not careful.";
        ui.currentPivot = pivot;
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        generalHint.SetActive(false);
    }
    public void SetGeneralHint(GameObject generalHint, SimpleUIInfo ui, Transform pivot)
    {
        this.pivot = pivot;
        this.ui = ui;
        this.generalHint = generalHint;
        this.hint = generalHint.GetComponent<GenericHint>();
    }
    public ApocHint(GameObject generalHint) : base(ClassInjector.DerivedConstructorPointer<ApocHint>())
    {
        ClassInjector.DerivedConstructorBody((Il2CppObjectBase)this);
    }
    public ApocHint(IntPtr ptr) : base(ptr)
    {

    }
}