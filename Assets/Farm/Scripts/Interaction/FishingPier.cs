using UnityEngine;
namespace NongTrai {
 public sealed class FishingPier:MonoBehaviour,IInteractable {
 public string InteractionHint=>"[CHUỘT PHẢI] Câu cá • 3 nhịp đúng để bắt cá";
 public bool CanInteract(FarmPlayer source)=>!source.Paused;
 public void Interact(PlayerInteraction actor){FarmRestaurant.Ensure();FarmFishing.Instance?.Open();}
 public void SetHighlighted(bool selected)=>InteractionOutline.Set(this,selected);
 }}
