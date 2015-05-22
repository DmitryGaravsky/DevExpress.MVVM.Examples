namespace Mvvm.Utils {
    using System.ComponentModel;
    using Mvvm.Utils.Behaviors;

    public enum ConfirmationButtons {
        OKCancel = 1,
        YesNoCancel = 3,
        YesNo = 4,
    }
    public class ConfirmationBehavior<TEventArgs> : EventTriggerBase<TEventArgs>
        where TEventArgs : CancelEventArgs {
        public ConfirmationBehavior(string eventName)
            : base(eventName) {
            Text = "Please confirm your action.";
            Caption = "Need confirmation";
            Buttons = ConfirmationButtons.YesNo;
            ShowQuestionIcon = true;
        }
        public string Text { get; set; }
        public string Caption { get; set; }
        public ConfirmationButtons Buttons { get; set; }
        public bool ShowQuestionIcon { get; set; }
        protected virtual bool Confirm() {
            return true;
        }
        protected sealed override void OnEvent() {
            Args.Cancel = !Confirm();
        }
    }
}