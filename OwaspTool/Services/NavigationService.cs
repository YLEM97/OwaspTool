using Microsoft.AspNetCore.Components;

public class NavigationService
{
    private readonly NavigationManager _navigation;

    public bool IsNavigating { get; private set; } = false;
    public event Action? OnChange;

    public NavigationService(NavigationManager navigation)
    {
        _navigation = navigation;

        // Resetta l'overlay solo quando Blazor conferma la nuova location
        _navigation.LocationChanged += (_, _) =>
        {
            IsNavigating = false;
            OnChange?.Invoke();
        };
    }

    public async Task NavigateTo(string url, bool forceload = false)
    {
        IsNavigating = true;
        OnChange?.Invoke();

        await Task.Delay(800);

        _navigation.NavigateTo(url, forceload);
        // NON resettare qui — ci pensa LocationChanged
    }
}
