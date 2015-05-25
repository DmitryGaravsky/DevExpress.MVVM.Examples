namespace Mvvm.Examples {
    public class ViewModelForSimpleBinding {
        public ViewModelForSimpleBinding() {
            Title = "MVVM Practices: Simple Binding";
        }
        // Bindable property will be created from this property.
        public virtual string Title { get; set; }
        // Command property will be created from this method.
        public void ResetTitle() {
            Title = "MVVM Practices: Simple Binding";
        }
    }
}