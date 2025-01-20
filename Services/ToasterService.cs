using EuroTrail.Models;

namespace EuroTrail.Services
{
    public class ToasterService
    {
        private static ToasterService? currentInstance;
        private static readonly object currentLock = new object();

        public static ToasterService Instance
        {
            get
            {
                if (currentInstance == null)
                {
                    lock (currentLock)
                    {
                        currentInstance ??= new ToasterService();
                    }
                }
                return currentInstance;
            }
        }

        private Action? NotifyUI;
        public event Action? OnToastsUpdated;
        private readonly List<Toast> toasts = new();
        public IReadOnlyList<Toast> Toasts => toasts.AsReadOnly();

        private ToasterService() { }

        public static void ShowGlobalToast(string message, string description, string type = "info")
        {
            Instance.ShowToast(message, description, type);
        }
        public void ShowToast(string message, string description, string type = "info")
        {
            var Id = Guid.NewGuid();

            toasts.Add(new Toast
            {
                Id = Id,
                Message = message,
                Description = description,
                Type = type
            });

            OnToastsUpdated?.Invoke();

            ForceUpdate();
            AutoRemoveToast(Id);
        }

        public void RemoveToast(Guid id)
        {
            var toast = toasts.FirstOrDefault(t => t.Id == id);
            if (toast != null)
            {
                toasts.Remove(toast);
                OnToastsUpdated?.Invoke();
            }
        }

        private async void AutoRemoveToast(Guid id)
        {
            await Task.Delay(5000);

            var toast = toasts.FirstOrDefault(t => t.Id == id);
            if (toast != null)
            {
                toasts.Remove(toast);
                OnToastsUpdated?.Invoke();
            }
        }

        public void RegisterUI(Action notifyUI)
        {
            NotifyUI = notifyUI;
        }

        private void ForceUpdate()
        {
            NotifyUI?.Invoke();
        }
    }
}
