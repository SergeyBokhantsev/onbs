namespace Interfaces.UI
{
    public interface IUIHost
    {
		void Run(bool fullscreen);
        void ShowPage(IPageModel model);
        void ShowDialog(IDialogModel dialog);
        void Shutdown();
    }
}
