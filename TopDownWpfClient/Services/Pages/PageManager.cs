using System;
using System.Collections.Generic;
using System.Windows.Controls;
using TopDownWpfClient.Models;

namespace TopDownWpfClient.Services.Pages {
    public class PageManager : IPageManager {
        private const string PAGE_TYPE_PREFIX = "TopDownWpfClient.Views.";
        private readonly Dictionary<object, Page> _openPages;

        public PageManager() {
            _openPages = new Dictionary<object, Page>();
        }

        /// <summary>
        /// Fires when a page opens.
        /// </summary>
        public event EventHandler<EventArgs<Type>> PageOpened;

        /// <summary>
        /// Creates the viewmodel as a page.
        /// </summary>
        /// <param name="viewModel">The page's viewmodel.</param>
        /// <returns>The created view for the given viewmodel.</returns>
        public Page CreatePage(object viewModel) {
            Page pageToOpen = CreatePageInternal(viewModel.GetType());

            pageToOpen.DataContext = viewModel;

            _openPages.Add(viewModel, pageToOpen);

            PageOpened?.Invoke(this, new EventArgs<Type>(viewModel.GetType()));

            return pageToOpen;
        }

        /// <summary>
        /// Gets the view for the given viewmodel.
        /// </summary>
        /// <param name="viewModel">The viewmodel.</param>
        /// <returns>The view for the given viewmodel, or null if the window could not be found.</returns>
        public Page GetView(object viewModel) {
            return _openPages.TryGetValue(viewModel, out Page page) ? page : null;
        }

        /// <summary>
        /// Creates a Page for the given viewmodel type.
        /// </summary>
        /// <param name="viewModelType">The type of viewmodel.</param>
        /// <returns>The created page.</returns>
        private static Page CreatePageInternal(Type viewModelType) {
            string typeName = viewModelType.Name;
            int backTickIndex = typeName.IndexOf('`');
            if (backTickIndex > 0) {
                typeName = typeName.Substring(0, backTickIndex);
            }

            string baseName;
            string suffix;

            if (typeName.EndsWith("PageViewModel", StringComparison.Ordinal)) {
                baseName = typeName.Substring(0, typeName.Length - "PageViewModel".Length);
                suffix = "Page";
            } else {
                throw new ArgumentException("Page viewmodel type's name must end in 'PageViewModel'");
            }

            Type pageType = Type.GetType(PAGE_TYPE_PREFIX + baseName + suffix) ??
                            Type.GetType(PAGE_TYPE_PREFIX + baseName);

            if (pageType == null) {
                throw new ArgumentException("Could not find Page for " + typeName);
            }

            return (Page) Activator.CreateInstance(pageType);
        }
    }
}