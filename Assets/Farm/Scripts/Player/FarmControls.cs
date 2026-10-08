using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace NongTrai
{
    // © TriForge. Desktop and touch share the same gameplay input edges.
    public static class FarmControls
    {
        public sealed class ButtonState
        {
            readonly Key key; readonly int pointer;
            bool held,requested,pressed,released;int snapshotFrame=-1,pressSequence,releaseSequence,seenPress,seenRelease;
            public ButtonState(Key key = Key.None, int pointer = 0) { this.key = key; this.pointer = pointer; }
            ButtonControl Desktop => pointer == 1 ? Mouse.current?.leftButton : pointer == 2 ? Mouse.current?.rightButton : key == Key.None ? null : Keyboard.current?[key];
            void Snapshot(){if(snapshotFrame==Time.frameCount)return;snapshotFrame=Time.frameCount;held=requested;pressed=pressSequence!=seenPress;released=releaseSequence!=seenRelease;seenPress=pressSequence;seenRelease=releaseSequence;}
            public bool isPressed {get{Snapshot();return held || (!Mobile && Desktop != null && Desktop.isPressed);}}
            public bool wasPressedThisFrame {get{Snapshot();return pressed || (!Mobile && Desktop != null && Desktop.wasPressedThisFrame);}}
            public bool wasReleasedThisFrame {get{Snapshot();return released || (!Mobile && Desktop != null && Desktop.wasReleasedThisFrame);}}
            public void Set(bool value) { if(requested==value)return;requested=value;if(value)pressSequence++;else releaseSequence++; }
            public void Reset() {held=requested=pressed=released=false;snapshotFrame=-1;pressSequence=releaseSequence=seenPress=seenRelease=0;}
        }
        public sealed class VectorState
        {
            readonly bool wheel;
            public VectorState(bool wheel = false) { this.wheel = wheel; }
            public Vector2 ReadValue() => wheel ? (Mobile ? Vector2.zero : Mouse.current?.scroll.ReadValue() ?? Vector2.zero) : (Mobile ? TouchPosition : Mouse.current?.position.ReadValue() ?? Vector2.zero);
        }
        public sealed class PointerState
        {
            public readonly ButtonState leftButton = new ButtonState(pointer:1), rightButton = new ButtonState(pointer:2);
            public readonly VectorState position = new VectorState(), scroll = new VectorState(true);
        }
        public sealed class KeyStates
        {
            readonly Dictionary<Key, ButtonState> values = new Dictionary<Key, ButtonState>();
            public ButtonState this[Key key] { get { if(!values.TryGetValue(key,out var state))values[key]=state=new ButtonState(key);return state; } }
            public ButtonState aKey=>this[Key.A]; public ButtonState bKey=>this[Key.B]; public ButtonState dKey=>this[Key.D];
            public ButtonState eKey=>this[Key.E]; public ButtonState fKey=>this[Key.F]; public ButtonState iKey=>this[Key.I];
            public ButtonState mKey=>this[Key.M]; public ButtonState nKey=>this[Key.N]; public ButtonState pKey=>this[Key.P];
            public ButtonState rKey=>this[Key.R]; public ButtonState sKey=>this[Key.S]; public ButtonState wKey=>this[Key.W];
            public ButtonState xKey=>this[Key.X]; public ButtonState tabKey=>this[Key.Tab]; public ButtonState spaceKey=>this[Key.Space];
            public ButtonState hKey=>this[Key.H];
            public ButtonState leftShiftKey=>this[Key.LeftShift]; public ButtonState rightShiftKey=>this[Key.RightShift];
            public void Reset() { foreach(var value in values.Values)value.Reset(); }
        }
        public static bool Mobile => Application.isMobilePlatform || ForceTouch;
        public static bool ForceTouch { get; set; }
        public static readonly PointerState Pointer = new PointerState();
        public static readonly KeyStates Keys = new KeyStates();
        public static Vector2 Move, TouchPosition;
        static Vector2 look,pendingLook;static int lookFrame=-1;
        public static void AddLook(Vector2 delta)=>pendingLook+=delta;
        public static Vector2 Look {get{if(lookFrame!=Time.frameCount){lookFrame=Time.frameCount;look=pendingLook;pendingLook=Vector2.zero;}return look;}}
        public static void ReleaseAll() { Move=look=pendingLook=Vector2.zero;lookFrame=-1;Pointer.leftButton.Reset();Pointer.rightButton.Reset();Keys.Reset(); }
        public static string DisplayHint(string text)
        {
            if(!Mobile||string.IsNullOrEmpty(text))return text;
            return text.Replace("CHUỘT TRÁI","DÙNG").Replace("Chuột trái","Dùng").Replace("chuột trái","Dùng")
                .Replace("Chuột phải","Tương tác").Replace("chuột phải","Tương tác").Replace("click trái","Dùng").Replace("click","Dùng")
                .Replace("[N]","[Menu → Mở đất]").Replace("[B]","[Túi]").Replace("[R]","[Xoay]").Replace("[F]","[Menu → Cho thú ăn]");
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { ForceTouch=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-farmTouch")>=0;ReleaseAll(); }
    }
}
