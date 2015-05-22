using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm {
    public enum MessageResult {
        None, OK, Cancel, Yes, No,
#if NETFX_CORE
        Abort, Retry, Ignore, Close,
#endif
    }
    public enum MessageButton {
        OK, OKCancel, YesNoCancel, YesNo,
#if NETFX_CORE
        AbortRetryIgnore, RetryCancel, Close,
#endif
    }
#if !NETFX_CORE
#if !SILVERLIGHT
    public enum MessageIcon {
        None, Error, Question, Warning, Information,
        Hand = Error, Stop = Error, Exclamation = Warning, Asterisk = Information
    }
#endif
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public static class MessageBoxEnumsConverter {
#if !MONO
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool? ToBool(this MessageBoxResult result) {
            switch(result) {
                case MessageBoxResult.Cancel: return null;
                case MessageBoxResult.No: return false;
                default: return true;
            }
        }
#endif
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool? ToBool(this MessageResult result) {
            switch(result) {
                case MessageResult.Cancel: return null;
                case MessageResult.No: return false;
                default: return true;
            }
        }
#if!MONO
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MessageBoxResult ToMessageBoxResult(this MessageResult result) {
            switch(result) {
                case MessageResult.Cancel: return MessageBoxResult.Cancel;
                case MessageResult.No: return MessageBoxResult.No;
                case MessageResult.Yes: return MessageBoxResult.Yes;
                case MessageResult.OK: return MessageBoxResult.OK;
                default: return MessageBoxResult.None;
            }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MessageResult ToMessageResult(this MessageBoxResult result) {
            switch(result) {
                case MessageBoxResult.Cancel: return MessageResult.Cancel;
                case MessageBoxResult.No: return MessageResult.No;
                case MessageBoxResult.Yes: return MessageResult.Yes;
                case MessageBoxResult.OK: return MessageResult.OK;
                default: return MessageResult.None;
            }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MessageBoxButton ToMessageBoxButton(this MessageButton button) {
            switch(button) {
                case MessageButton.OKCancel: return MessageBoxButton.OKCancel;
#if !SILVERLIGHT
                case MessageButton.YesNo: return MessageBoxButton.YesNo;
                case MessageButton.YesNoCancel: return MessageBoxButton.YesNoCancel;
#endif
                default: return MessageBoxButton.OK;
            }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MessageButton ToMessageButton(this MessageBoxButton button) {
            switch(button) {
                case MessageBoxButton.OKCancel: return MessageButton.OKCancel;
#if !SILVERLIGHT
                case MessageBoxButton.YesNo: return MessageButton.YesNo;
                case MessageBoxButton.YesNoCancel: return MessageButton.YesNoCancel;
#endif
                default: return MessageButton.OK;
            }
        }
#if SILVERLIGHT
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MessageButton ToMessageButton(this DXMessageBoxButton button) {
            switch(button) {
                case DXMessageBoxButton.OKCancel: return MessageButton.OKCancel;
                case DXMessageBoxButton.YesNo: return MessageButton.YesNo;
                case DXMessageBoxButton.YesNoCancel: return MessageButton.YesNoCancel;
                default: return MessageButton.OK;
            }
        }
#else
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MessageBoxImage ToMessageBoxImage(this MessageIcon icon) {
            switch(icon) {
                case MessageIcon.Error: return MessageBoxImage.Error;
                case MessageIcon.Information: return MessageBoxImage.Information;
                case MessageIcon.Question: return MessageBoxImage.Question;
                case MessageIcon.Warning: return MessageBoxImage.Warning;
                default: return MessageBoxImage.None;
            }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MessageIcon ToMessageIcon(this MessageBoxImage icon) {
            switch(icon) {
                case MessageBoxImage.Error: return MessageIcon.Error;
                case MessageBoxImage.Information: return MessageIcon.Information;
                case MessageBoxImage.Question: return MessageIcon.Question;
                case MessageBoxImage.Warning: return MessageIcon.Warning;
                default: return MessageIcon.None;
            }
        }
#endif
#endif
    }
#endif
}