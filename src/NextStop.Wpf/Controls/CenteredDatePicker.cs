using System.Windows;
using System.Windows.Controls;

namespace NextStop.Wpf.Controls;

public sealed class CenteredDatePicker : DatePicker
{
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_Button") is Button calendarButton)
        {
            calendarButton.VerticalAlignment = VerticalAlignment.Center;
        }
    }
}
