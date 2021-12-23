using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using Microsoft.AnyContainer;
using TopDownWpfClient.Models;
using TopDownWpfClient.ViewModels;

namespace TopDownWpfClient.Services.Windows {
    public class WindowManager : IWindowManager {
        private static readonly Type _mainWindowViewModelType = typeof(MainWindowViewModel);
        private const string WINDOW_TYPE_PREFIX = "TopDownWpfClient.Views.";
        private object _mainViewModel;
        private readonly Dictionary<object, Window> _openWindows;

        static WindowManager() {
            Definitions = new List<WindowDefinition> {
                new() {
                    ViewModelType = typeof(MainWindowViewModel),
                    PlacementConfigKey = "MainWindowPlacement",
                    InitialSizeOverride = () => {
                        Rect workArea = SystemParameters.WorkArea;

                        double fillHeight = workArea.Height - 500;
                        double minHeight = 548;

                        double desiredHeight = Math.Max(fillHeight, minHeight);

                        return new Size(0, desiredHeight);
                    }
                },
            };
        }

        public WindowManager() {
            _openWindows = new Dictionary<object, Window>();
        }

        public static List<WindowDefinition> Definitions { get; private set; }

        /// <summary>
        /// Fires when a window opens.
        /// </summary>
        public event EventHandler<EventArgs<Type>> WindowOpened;

        /// <summary>
        /// Fires when a window closes.
        /// </summary>
        public event EventHandler<EventArgs<Type>> WindowClosed;

        /// <summary>
        /// Opens the viewmodel as a window.
        /// </summary>
        /// <param name="viewModel">The window's viewmodel.</param>
        /// <param name="ownerViewModel">The viewmodel of the owner window.</param>
        /// <param name="userInitiated">True if the user explicitly opened the window.</param>
        public void OpenWindow(object viewModel, object ownerViewModel = null, bool userInitiated = true) {
            if (viewModel.GetType() == _mainWindowViewModelType) {
                _mainViewModel = viewModel;
            } else if (ownerViewModel == null) {
                ownerViewModel = _mainViewModel;
            }

            Window windowToOpen = PrepareWindowForOpen(viewModel, ownerViewModel, userInitiated, isDialog: false);
            windowToOpen.Show();
        }

        /// <summary>
        /// Opens the viewmodel as a dialog.
        /// </summary>
        /// <param name="viewModel">The dialog's viewmodel.</param>
        /// <param name="ownerViewModel">The viewmodel of the owner window.</param>
        public void OpenDialog(object viewModel, object ownerViewModel = null) {
            ownerViewModel ??= _mainViewModel;

            Window windowToOpen =
                PrepareWindowForOpen(viewModel, ownerViewModel, userInitiated: true, isDialog: true);
            windowToOpen.ShowDialog();
        }

        /// <summary>
        /// Opens the viewmodel type as a dialog.
        /// </summary>
        /// <typeparam name="T">The type of the viewmodel.</typeparam>
        /// <param name="ownerViewModel">The viewmodel of the owner window.</param>
        public void OpenDialog<T>(object ownerViewModel = null)
            where T : class {
            OpenDialog(StaticResolver.Resolve<T>(), ownerViewModel);
        }

        /// <summary>
        /// Opens or focuses the viewmodel type's window.
        /// </summary>
        /// <param name="viewModelType">The type of the window viewmodel.</param>
        /// <param name="ownerViewModel">The owner view model (main view model).</param>
        public void OpenOrFocusWindow(Type viewModelType, object ownerViewModel = null) {
            object viewModel = FindOpenWindowViewModel(viewModelType);

            if (viewModel == null) {
                viewModel = StaticResolver.Resolve(viewModelType);
                ownerViewModel ??= _mainViewModel;

                Window window =
                    PrepareWindowForOpen(viewModel, ownerViewModel, userInitiated: true, isDialog: false);
                window.Show();
            } else {
                Focus(viewModel);
            }
        }

        /// <summary>
        /// Opens or focuses the viewmodel type's window.
        /// </summary>
        /// <typeparam name="T">The type of the window viewmodel.</typeparam>
        /// <param name="ownerViewModel">The owner view model (main view model).</param>
        /// <returns>The opened viewmodel.</returns>
        public T OpenOrFocusWindow<T>(object ownerViewModel = null) where T : class {
            if (FindOpenWindowViewModel(typeof(T)) is not T viewModel) {
                viewModel = StaticResolver.Resolve<T>();
                ownerViewModel ??= _mainViewModel;

                Window window =
                    PrepareWindowForOpen(viewModel, ownerViewModel, userInitiated: true, isDialog: false);
                window.Show();
            } else {
                Focus(viewModel);
            }

            return viewModel;
        }

        /// <summary>
        /// Finds an open window with the given viewmodel type.
        /// </summary>
        /// <typeparam name="T">The viewmodel type.</typeparam>
        /// <returns>The open window viewmodel, or null if is not open.</returns>
        public T Find<T>() where T : class {
            return FindOpenWindowViewModel(typeof(T)) as T;
        }

        /// <summary>
        /// Gets the view for the given viewmodel.
        /// </summary>
        /// <param name="viewModel">The viewmodel.</param>
        /// <returns>The view for the given viewmodel, or null if the window could not be found.</returns>
        public Window GetView(object viewModel) {
            return _openWindows.TryGetValue(viewModel, out Window window) ? window : null;
        }

        /// <summary>
        /// Gets the view for the given viewmodel type.
        /// </summary>
        /// <typeparam name="T">The viewmodel type.</typeparam>
        /// <returns>The view for the given viewmodel.</returns>
        public Window GetView<T>() where T : class {
            var tempVM = Find<T>();
            return GetView(tempVM);
        }

        /// <summary>
        /// Focuses the window.
        /// </summary>
        /// <param name="viewModel">The viewmodel of the window to focus.</param>
        public void Focus(object viewModel) {
            _openWindows[viewModel].Focus();
        }

        /// <summary>
        /// Activates the window.
        /// </summary>
        /// <param name="viewModel">The viewmodel of the window to activate.</param>
        public void Activate(object viewModel) {
            _openWindows[viewModel].Activate();
        }

        /// <summary>
        /// Activates the window.
        /// </summary>
        /// <typeparam name="T">The viewmodel type of the window to activate.</typeparam>
        public void Activate<T>() where T : class {
            var tempVM = Find<T>();
            _openWindows[tempVM].Activate();
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <param name="viewModel">The viewmodel of the window to close.</param>
        public void Close(object viewModel) {
            CloseInternal(viewModel, userInitiated: true);
        }

        /// <summary>
        /// Closes the window of the given type.
        /// </summary>
        /// <typeparam name="T">The viewmodel type of the window to close.</typeparam>
        /// <param name="userInitiated">True if the user specifically asked this window to close.</param>
        public void Close<T>(bool userInitiated) where T : class {
            object viewModel = FindOpenWindowViewModel(typeof(T));
            if (viewModel != null) {
                CloseInternal(viewModel, userInitiated);
            }
        }

        /// <summary>
        /// Gets a list of window positions.
        /// </summary>
        /// <param name="excludeWindow">The window to exclude.</param>
        /// <returns>A list of open window positions.</returns>
        public List<WindowPosition> GetOpenedWindowPositions(Window excludeWindow = null) {
            return (from definition in Definitions.Where(d => d.PlacementConfigKey != null)
                    let windowVM = FindOpenWindowViewModel(definition.ViewModelType)
                    where windowVM != null
                    let window = GetView(windowVM)
                    where window != null && window != excludeWindow
                    select new WindowPosition {
                        Position = new Rect((int) window.Left, (int) window.Top, (int) window.ActualWidth,
                                            (int) window.ActualHeight),
                        ViewModelType = definition.ViewModelType
                    }).ToList();
        }

        /// <summary>
        /// Suspends the AllowDrop property on all windows (used when a smaller drag/drop operation is starting).
        /// </summary>
        public void SuspendDropOnWindows() {
            foreach (Window window in _openWindows.Values) {
                window.AllowDrop = false;
            }
        }

        /// <summary>
        /// Resumes the AllowDrop property on all windows (used when a smaller drag/drop operation is finished).
        /// </summary>
        public void ResumeDropOnWindows() {
            foreach (Window window in _openWindows.Values) {
                window.AllowDrop = true;
            }
        }

        /// <summary>
        /// Prepares a window for opening.
        /// </summary>
        /// <param name="viewModel">The window viewmodel to use.</param>
        /// <param name="ownerViewModel">The owner viewmodel.</param>
        /// <param name="userInitiated">True if the user specifically asked this window to open,
        /// false if it being re-opened automatically on app start.</param>
        /// <param name="isDialog">True if the window is being opened as a dialog, false if it's being opened
        /// as a window.</param>
        /// <returns>The prepared window.</returns>
        private Window PrepareWindowForOpen(object viewModel, object ownerViewModel, bool userInitiated,
                                            bool isDialog) {
            Window windowToOpen = CreateWindow(viewModel.GetType());
            if (ownerViewModel != null) {
                windowToOpen.Owner = _openWindows[ownerViewModel];
            }

            windowToOpen.DataContext = viewModel;
            windowToOpen.Closing += OnClosingHandler;

            WindowDefinition windowDefinition = GetWindowDefinition(viewModel);
            windowToOpen.SourceInitialized += (o, e) => {
                // Add hook for WndProc messages
                ((HwndSource) PresentationSource.FromVisual(windowToOpen)).AddHook(WindowPlacement.HookProc);

                // Restore placement from Config - deleted
            };

            _openWindows.Add(viewModel, windowToOpen);

            if (userInitiated) {
                // Save window in Config as opened - deleted
            }

            WindowOpened?.Invoke(this, new EventArgs<Type>(viewModel.GetType()));

            if (!isDialog) {
                // windowToOpen.RegisterGlobalHotkeys(); TODO: сделать hotkeys
                windowToOpen.AllowDrop = true;
            }

            return windowToOpen;
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <param name="viewModel">The viewmodel of the window to close.</param>
        /// <param name="userInitiated">True if the user specifically asked this window to close.</param>
        private void CloseInternal(object viewModel, bool userInitiated) {
            if (!_openWindows.ContainsKey(viewModel)) {
                return;
            }

            Window window = _openWindows[viewModel];

            if (!userInitiated) {
                window.Closing -= OnClosingHandler;
                if (!OnClosing(window, userInitiated: false)) {
                    return;
                }
            }

            window.Close();
        }

        /// <summary>
        /// Fires when a window is closing.
        /// </summary>
        /// <param name="sender">The sending window.</param>
        /// <param name="e">The cancellation event args.</param>
        /// <remarks>This should only fire when the user has specifically asked for the window to
        /// close.</remarks>
        private void OnClosingHandler(object sender, CancelEventArgs e) {
            var closingWindow = (Window) sender;
            if (!OnClosing(closingWindow, userInitiated: true)) {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Fires when a window is closing.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="userInitiated">True if the close was initiated by the user, false if this
        /// was initiated by the system as part of app shutdown.</param>
        /// <returns>True if the window closed, false if it was stopped by the user.</returns>
        private bool OnClosing(Window window, bool userInitiated) {
            object viewModel = window.DataContext;
            if (viewModel is IClosableWindow closableWindow) {
                if (!closableWindow.OnClosing()) {
                    return false;
                }
            }

            WindowDefinition windowDefinition = GetWindowDefinition(viewModel);
            if (windowDefinition != null) {
                // Save window placement and mark it as closed in Config - deleted
            }

            _openWindows.Remove(viewModel);

            if (userInitiated) {
                window.Owner?.Activate();
            }

            WindowClosed?.Invoke(this, new EventArgs<Type>(viewModel.GetType()));

            return true;
        }

        /// <summary>
        /// Finds the viewmodel for an open window.
        /// </summary>
        /// <param name="viewModelType">The viewmodel's type.</param>
        /// <returns>The viewmodel, or null if it was not open.</returns>
        private object FindOpenWindowViewModel(Type viewModelType) {
            return _openWindows.Keys.FirstOrDefault(k => k.GetType() == viewModelType);
        }

        /// <summary>
        /// Gets a tracked window definition from the given viewmodel.
        /// </summary>
        /// <param name="viewModel">The window viewmodel.</param>
        /// <returns>The tracked window definition for the viewmodel.</returns>
        private static WindowDefinition GetWindowDefinition(object viewModel) {
            Type viewModelType = viewModel.GetType();
            return Definitions.FirstOrDefault(d => d.ViewModelType == viewModelType);
        }

        /// <summary>
        /// Creates a Window for the given viewmodel type.
        /// </summary>
        /// <param name="viewModelType">The type of viewmodel.</param>
        /// <returns>The created window.</returns>
        private static Window CreateWindow(Type viewModelType) {
            string typeName = viewModelType.Name;
            int backTickIndex = typeName.IndexOf('`');
            if (backTickIndex > 0) {
                typeName = typeName.Substring(0, backTickIndex);
            }

            string baseName;
            string suffix;

            if (typeName.EndsWith("DialogViewModel", StringComparison.Ordinal)) {
                baseName = typeName.Substring(0, typeName.Length - "DialogViewModel".Length);
                suffix = "Dialog";
            } else if (typeName.EndsWith("WindowViewModel", StringComparison.Ordinal)) {
                baseName = typeName.Substring(0, typeName.Length - "WindowViewModel".Length);
                suffix = "Window";
            } else if (typeName.EndsWith("ViewModel", StringComparison.Ordinal)) {
                baseName = typeName.Substring(0, typeName.Length - "ViewModel".Length);
                suffix = string.Empty;
            } else {
                throw new ArgumentException("Window viewmodel type's name must end in 'ViewModel'");
            }

            Type windowType = Type.GetType(WINDOW_TYPE_PREFIX + baseName + suffix) ??
                              Type.GetType(WINDOW_TYPE_PREFIX + baseName);

            if (windowType == null) {
                throw new ArgumentException("Could not find Window for " + typeName);
            }

            return (Window) Activator.CreateInstance(windowType);
        }
    }
}