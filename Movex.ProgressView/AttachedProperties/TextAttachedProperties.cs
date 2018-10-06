using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Movex.ProgressView
{
    /// <summary>
    /// Focuses (keyboard focus) this element on load
    /// </summary>
    public class IsFocusedProperty : BaseAttachedProperty<IsFocusedProperty, bool>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // If we don't have a control, return
            if (!(sender is Control control))
                return;

            // Focus this control once loaded
            control.Loaded += (s, se) => control.Focus();
        }

    }


        /// <summary>
        /// Focuses (keyboard focus) this element in true
        /// </summary>
    public class FocusProperty : BaseAttachedProperty<FocusProperty, bool>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // If we don't have a control, return
            if (!(sender is Control control))
                return;

            if ((bool)e.NewValue)
                // Focus this control
                control.Focus();
        }
    }

    /// <summary>
    /// Focuses (keyboard focus) and select all the text in this element in true
    /// </summary>
    public class FocusAndSelectProperty : BaseAttachedProperty<FocusAndSelectProperty, bool>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // If we don't have a control, return
            if (!(sender is TextBoxBase control))
                return;

            if ((bool)e.NewValue) { 

                // Focus this control
                control.Focus();

                // Select all the text
                control.SelectAll();

            }
        }
    }   
}
