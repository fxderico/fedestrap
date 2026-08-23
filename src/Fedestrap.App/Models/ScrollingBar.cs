namespace Fedestrap.Helpers
{
    public static class SmoothScrollBehavior
    {
        static SmoothScrollBehavior()
        {
            Wpf.Ui.Controls.SmoothScroll.Register();
        }
    }
}
