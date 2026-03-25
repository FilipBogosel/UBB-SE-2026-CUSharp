#nullable enable

namespace MovieApp.UI.ViewModels;

/// <summary>
/// Coordinates the top-level application view models.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private int _selectedTabIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="catalogViewModel">The catalog view model.</param>
    /// <param name="movieDetailViewModel">The movie detail view model.</param>
    /// <param name="battleViewModel">The battle view model.</param>
    /// <param name="forumViewModel">The forum view model.</param>
    /// <param name="profileViewModel">The profile view model.</param>
    public MainViewModel(
        CatalogViewModel catalogViewModel,
        MovieDetailViewModel movieDetailViewModel,
        BattleViewModel battleViewModel,
        ForumViewModel forumViewModel,
        ProfileViewModel profileViewModel)
    {
        CatalogViewModel = catalogViewModel;
        MovieDetailViewModel = movieDetailViewModel;
        BattleViewModel = battleViewModel;
        ForumViewModel = forumViewModel;
        ProfileViewModel = profileViewModel;
    }

    /// <summary>
    /// Gets the catalog view model.
    /// </summary>
    public CatalogViewModel CatalogViewModel { get; }

    /// <summary>
    /// Gets the movie detail view model.
    /// </summary>
    public MovieDetailViewModel MovieDetailViewModel { get; }

    /// <summary>
    /// Gets the battle view model.
    /// </summary>
    public BattleViewModel BattleViewModel { get; }

    /// <summary>
    /// Gets the forum view model.
    /// </summary>
    public ForumViewModel ForumViewModel { get; }

    /// <summary>
    /// Gets the profile view model.
    /// </summary>
    public ProfileViewModel ProfileViewModel { get; }

    /// <summary>
    /// Gets or sets the selected tab index.
    /// </summary>
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }
}
