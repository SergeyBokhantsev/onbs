namespace Interfaces.UI
{
    public interface IUIHost
    {
        int UserIdleMinutes { get; }

        void Run(bool fullscreen);
        void ShowPage(IPageModel model);
        void ShowDialog(IDialogModel dialog);
        void Shutdown();
    }
}
