#nullable enable

using MovieApp.UI.ViewModels;
using System.Windows;

namespace MovieApp.UI;

/// <summary>
/// Main application shell.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The main view model.</param>
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
