using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Key = Avalonia.Input.Key;
using MouseButton = Avalonia.Input.MouseButton;

namespace Avalonia.Veldrid.Sdl2
{
    public static class ExtensionMethods
    {
        public static MouseButton ToAvalonia(this global::Veldrid.MouseButton mouseButton)
        {
            switch (mouseButton)
            {
                case global::Veldrid.MouseButton.Left:
                    return MouseButton.Left;
                case global::Veldrid.MouseButton.Middle:
                    return MouseButton.Middle;
                case global::Veldrid.MouseButton.Right:
                    return MouseButton.Right;
                default:
                    return MouseButton.None;
            }
        }
        public static Key ToAvaloniaKey(this global::Veldrid.Key keyCode)
        {
            switch (keyCode)
            {
                case global::Veldrid.Key.Unknown: return Key.None;
                //case global::Veldrid.Key.Cancel: return Key.Cancel;
                case global::Veldrid.Key.Back: return Key.Back;
                case global::Veldrid.Key.Tab: return Key.Tab;
                //case global::Veldrid.Key.LineFeed: return Key.LineFeed;
                case global::Veldrid.Key.Clear: return Key.Clear;
                case global::Veldrid.Key.Enter: return Key.Return;
                case global::Veldrid.Key.Pause: return Key.Pause;
                case global::Veldrid.Key.CapsLock: return Key.CapsLock;
                //case global::Veldrid.Key.HangulMode: return Key.HangulMode;
                //case global::Veldrid.Key.JunjaMode: return Key.JunjaMode;
                //case global::Veldrid.Key.FinalMode: return Key.FinalMode;
                //case global::Veldrid.Key.KanjiMode: return Key.KanjiMode;
                case global::Veldrid.Key.Escape: return Key.Escape;
                //case global::Veldrid.Key.ImeConvert: return Key.ImeConvert;
                //case global::Veldrid.Key.ImeNonConvert: return Key.ImeNonConvert;
                //case global::Veldrid.Key.ImeAccept: return Key.ImeAccept;
                //case global::Veldrid.Key.ImeModeChange: return Key.ImeModeChange;
                case global::Veldrid.Key.Space: return Key.Space;
                //case global::Veldrid.Key.Prior: return Key.Prior;
                case global::Veldrid.Key.PageDown: return Key.PageDown;
                case global::Veldrid.Key.End: return Key.End;
                case global::Veldrid.Key.Home: return Key.Home;
                case global::Veldrid.Key.Left: return Key.Left;
                case global::Veldrid.Key.Up: return Key.Up;
                case global::Veldrid.Key.Right: return Key.Right;
                case global::Veldrid.Key.Down: return Key.Down;
                //case global::Veldrid.Key.Select: return Key.Select;
                //case global::Veldrid.Key.Print: return Key.Print;
                //case global::Veldrid.Key.Execute: return Key.Execute;
                case global::Veldrid.Key.PrintScreen: return Key.PrintScreen;
                case global::Veldrid.Key.Insert: return Key.Insert;
                case global::Veldrid.Key.Delete: return Key.Delete;
                //case global::Veldrid.Key.Help: return Key.Help;
                case global::Veldrid.Key.Number0: return Key.D0;
                case global::Veldrid.Key.Number1: return Key.D1;
                case global::Veldrid.Key.Number2: return Key.D2;
                case global::Veldrid.Key.Number3: return Key.D3;
                case global::Veldrid.Key.Number4: return Key.D4;
                case global::Veldrid.Key.Number5: return Key.D5;
                case global::Veldrid.Key.Number6: return Key.D6;
                case global::Veldrid.Key.Number7: return Key.D7;
                case global::Veldrid.Key.Number8: return Key.D8;
                case global::Veldrid.Key.Number9: return Key.D9;
                case global::Veldrid.Key.A: return Key.A;
                case global::Veldrid.Key.B: return Key.B;
                case global::Veldrid.Key.C: return Key.C;
                case global::Veldrid.Key.D: return Key.D;
                case global::Veldrid.Key.E: return Key.E;
                case global::Veldrid.Key.F: return Key.F;
                case global::Veldrid.Key.G: return Key.G;
                case global::Veldrid.Key.H: return Key.H;
                case global::Veldrid.Key.I: return Key.I;
                case global::Veldrid.Key.J: return Key.J;
                case global::Veldrid.Key.K: return Key.K;
                case global::Veldrid.Key.L: return Key.L;
                case global::Veldrid.Key.M: return Key.M;
                case global::Veldrid.Key.N: return Key.N;
                case global::Veldrid.Key.O: return Key.O;
                case global::Veldrid.Key.P: return Key.P;
                case global::Veldrid.Key.Q: return Key.Q;
                case global::Veldrid.Key.R: return Key.R;
                case global::Veldrid.Key.S: return Key.S;
                case global::Veldrid.Key.T: return Key.T;
                case global::Veldrid.Key.U: return Key.U;
                case global::Veldrid.Key.V: return Key.V;
                case global::Veldrid.Key.W: return Key.W;
                case global::Veldrid.Key.X: return Key.X;
                case global::Veldrid.Key.Y: return Key.Y;
                case global::Veldrid.Key.Z: return Key.Z;
                case global::Veldrid.Key.LWin: return Key.LWin;
                case global::Veldrid.Key.RWin: return Key.RWin;
                //case global::Veldrid.Key.Apps: return Key.Apps;
                case global::Veldrid.Key.Sleep: return Key.Sleep;
                case global::Veldrid.Key.Keypad0: return Key.NumPad0;
                case global::Veldrid.Key.Keypad1: return Key.NumPad1;
                case global::Veldrid.Key.Keypad2: return Key.NumPad2;
                case global::Veldrid.Key.Keypad3: return Key.NumPad3;
                case global::Veldrid.Key.Keypad4: return Key.NumPad4;
                case global::Veldrid.Key.Keypad5: return Key.NumPad5;
                case global::Veldrid.Key.Keypad6: return Key.NumPad6;
                case global::Veldrid.Key.Keypad7: return Key.NumPad7;
                case global::Veldrid.Key.Keypad8: return Key.NumPad8;
                case global::Veldrid.Key.Keypad9: return Key.NumPad9;
                case global::Veldrid.Key.KeypadMultiply: return Key.Multiply;
                case global::Veldrid.Key.KeypadAdd: return Key.Add;
                //case global::Veldrid.Key.Separator: return Key.Separator;
                case global::Veldrid.Key.KeypadSubtract: return Key.Subtract;
                case global::Veldrid.Key.KeypadDecimal: return Key.Decimal;
                case global::Veldrid.Key.KeypadDivide: return Key.Divide;
                case global::Veldrid.Key.F1: return Key.F1;
                case global::Veldrid.Key.F2: return Key.F2;
                case global::Veldrid.Key.F3: return Key.F3;
                case global::Veldrid.Key.F4: return Key.F4;
                case global::Veldrid.Key.F5: return Key.F5;
                case global::Veldrid.Key.F6: return Key.F6;
                case global::Veldrid.Key.F7: return Key.F7;
                case global::Veldrid.Key.F8: return Key.F8;
                case global::Veldrid.Key.F9: return Key.F9;
                case global::Veldrid.Key.F10: return Key.F10;
                case global::Veldrid.Key.F11: return Key.F11;
                case global::Veldrid.Key.F12: return Key.F12;
                case global::Veldrid.Key.F13: return Key.F13;
                case global::Veldrid.Key.F14: return Key.F14;
                case global::Veldrid.Key.F15: return Key.F15;
                case global::Veldrid.Key.F16: return Key.F16;
                case global::Veldrid.Key.F17: return Key.F17;
                case global::Veldrid.Key.F18: return Key.F18;
                case global::Veldrid.Key.F19: return Key.F19;
                case global::Veldrid.Key.F20: return Key.F20;
                case global::Veldrid.Key.F21: return Key.F21;
                case global::Veldrid.Key.F22: return Key.F22;
                case global::Veldrid.Key.F23: return Key.F23;
                case global::Veldrid.Key.F24: return Key.F24;
                case global::Veldrid.Key.NumLock: return Key.NumLock;
                case global::Veldrid.Key.ScrollLock: return Key.Scroll;
                case global::Veldrid.Key.ShiftLeft: return Key.LeftShift;
                case global::Veldrid.Key.ShiftRight: return Key.RightShift;
                case global::Veldrid.Key.ControlLeft: return Key.LeftCtrl;
                case global::Veldrid.Key.ControlRight: return Key.RightCtrl;
                case global::Veldrid.Key.AltLeft: return Key.LeftAlt;
                case global::Veldrid.Key.AltRight: return Key.RightAlt;
                    //case global::Veldrid.Key.BrowserBack: return Key.BrowserBack;
                    //case global::Veldrid.Key.BrowserForward: return Key.BrowserForward;
                    //case global::Veldrid.Key.BrowserRefresh: return Key.BrowserRefresh;
                    //case global::Veldrid.Key.BrowserStop: return Key.BrowserStop;
                    //case global::Veldrid.Key.BrowserSearch: return Key.BrowserSearch;
                    //case global::Veldrid.Key.BrowserFavorites: return Key.BrowserFavorites;
                    //case global::Veldrid.Key.BrowserHome: return Key.BrowserHome;
                    //case global::Veldrid.Key.VolumeMute: return Key.VolumeMute;
                    //case global::Veldrid.Key.VolumeDown: return Key.VolumeDown;
                    //case global::Veldrid.Key.VolumeUp: return Key.VolumeUp;
                    //case global::Veldrid.Key.MediaNextTrack: return Key.MediaNextTrack;
                    //case global::Veldrid.Key.MediaPreviousTrack: return Key.MediaPreviousTrack;
                    //case global::Veldrid.Key.MediaStop: return Key.MediaStop;
                    //case global::Veldrid.Key.MediaPlayPause: return Key.MediaPlayPause;
                    //case global::Veldrid.Key.LaunchMail: return Key.LaunchMail;
                    //case global::Veldrid.Key.SelectMedia: return Key.SelectMedia;
                    //case global::Veldrid.Key.LaunchApplication1: return Key.LaunchApplication1;
                    //case global::Veldrid.Key.LaunchApplication2: return Key.LaunchApplication2;
                    //case global::Veldrid.Key.OemSemicolon: return Key.OemSemicolon;
                    //case global::Veldrid.Key.OemSemicolon: return Key.OemSemicolon;
                    //case global::Veldrid.Key.OemPlus: return Key.OemPlus;
                    //case global::Veldrid.Key.OemComma: return Key.OemComma;
                    //case global::Veldrid.Key.OemMinus: return Key.OemMinus;
                    //case global::Veldrid.Key.OemPeriod: return Key.OemPeriod;
                    //case global::Veldrid.Key.Oem2: return Key.Oem2;
                    //case global::Veldrid.Key.Oem2: return Key.Oem2;
                    //case global::Veldrid.Key.OemTilde: return Key.OemTilde;
                    //case global::Veldrid.Key.OemTilde: return Key.OemTilde;
                    //case global::Veldrid.Key.AbntC1: return Key.AbntC1;
                    //case global::Veldrid.Key.AbntC2: return Key.AbntC2;
                    //case global::Veldrid.Key.Oem4: return Key.Oem4;
                    //case global::Veldrid.Key.Oem4: return Key.Oem4;
                    //case global::Veldrid.Key.OemPipe: return Key.OemPipe;
                    //case global::Veldrid.Key.OemPipe: return Key.OemPipe;
                    //case global::Veldrid.Key.OemCloseBrackets: return Key.OemCloseBrackets;
                    //case global::Veldrid.Key.OemCloseBrackets: return Key.OemCloseBrackets;
                    //case global::Veldrid.Key.OemQuotes: return Key.OemQuotes;
                    //case global::Veldrid.Key.OemQuotes: return Key.OemQuotes;
                    //case global::Veldrid.Key.Oem8: return Key.Oem8;
                    //case global::Veldrid.Key.OemBackslash: return Key.OemBackslash;
                    //case global::Veldrid.Key.OemBackslash: return Key.OemBackslash;
                    //case global::Veldrid.Key.ImeProcessed: return Key.ImeProcessed;
                    //case global::Veldrid.Key.System: return Key.System;
                    //case global::Veldrid.Key.DbeAlphanumeric: return Key.DbeAlphanumeric;
                    //case global::Veldrid.Key.DbeAlphanumeric: return Key.DbeAlphanumeric;
                    //case global::Veldrid.Key.OemFinish: return Key.OemFinish;
                    //case global::Veldrid.Key.OemFinish: return Key.OemFinish;
                    //case global::Veldrid.Key.DbeHiragana: return Key.DbeHiragana;
                    //case global::Veldrid.Key.DbeHiragana: return Key.DbeHiragana;
                    //case global::Veldrid.Key.DbeSbcsChar: return Key.DbeSbcsChar;
                    //case global::Veldrid.Key.DbeSbcsChar: return Key.DbeSbcsChar;
                    //case global::Veldrid.Key.DbeDbcsChar: return Key.DbeDbcsChar;
                    //case global::Veldrid.Key.DbeDbcsChar: return Key.DbeDbcsChar;
                    //case global::Veldrid.Key.DbeRoman: return Key.DbeRoman;
                    //case global::Veldrid.Key.DbeRoman: return Key.DbeRoman;
                    //case global::Veldrid.Key.DbeNoRoman: return Key.DbeNoRoman;
                    //case global::Veldrid.Key.DbeNoRoman: return Key.DbeNoRoman;
                    //case global::Veldrid.Key.CrSel: return Key.CrSel;
                    //case global::Veldrid.Key.CrSel: return Key.CrSel;
                    //case global::Veldrid.Key.DbeEnterImeConfigureMode: return Key.DbeEnterImeConfigureMode;
                    //case global::Veldrid.Key.DbeEnterImeConfigureMode: return Key.DbeEnterImeConfigureMode;
                    //case global::Veldrid.Key.DbeFlushString: return Key.DbeFlushString;
                    //case global::Veldrid.Key.DbeFlushString: return Key.DbeFlushString;
                    //case global::Veldrid.Key.DbeCodeInput: return Key.DbeCodeInput;
                    //case global::Veldrid.Key.DbeCodeInput: return Key.DbeCodeInput;
                    //case global::Veldrid.Key.DbeNoCodeInput: return Key.DbeNoCodeInput;
                    //case global::Veldrid.Key.DbeNoCodeInput: return Key.DbeNoCodeInput;
                    //case global::Veldrid.Key.NoName: return Key.NoName;
                    //case global::Veldrid.Key.NoName: return Key.NoName;
                    //case global::Veldrid.Key.Pa1: return Key.Pa1;
                    //case global::Veldrid.Key.Pa1: return Key.Pa1;
                    //case global::Veldrid.Key.OemClear: return Key.OemClear;
                    //case global::Veldrid.Key.DeadCharProcessed: return Key.DeadCharProcessed;
                    //case global::Veldrid.Key.FnLeftArrow: return Key.FnLeftArrow;
                    //case global::Veldrid.Key.FnRightArrow: return Key.FnRightArrow;
                    //case global::Veldrid.Key.FnUpArrow: return Key.FnUpArrow;
                    //case global::Veldrid.Key.FnDownArrow: return Key.FnDownArrow;
            }

            return Key.None;
        }
    }
}
