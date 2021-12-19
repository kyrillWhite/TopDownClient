using System;
using System.Windows.Controls;
using TopDownWpfClient.Models;

namespace TopDownWpfClient.Services.Pages {
    public interface IPageManager {
        /// <summary>
        /// Creates the viewmodel as a page.
        /// </summary>
        /// <param name="viewModel">The page's viewmodel.</param>
        /// <returns>The created view for the given viewmodel.</returns>
        Page CreatePage(object viewModel);

        /// <summary>
        /// Gets the view for the given viewmodel.
        /// </summary>
        /// <param name="viewModel">The viewmodel.</param>
        /// <returns>The view for the given viewmodel.</returns>
        Page GetView(object viewModel);

        /// <summary>
        /// Fires when a page opens.
        /// </summary>
        event EventHandler<EventArgs<Type>> PageOpened;
    }
}