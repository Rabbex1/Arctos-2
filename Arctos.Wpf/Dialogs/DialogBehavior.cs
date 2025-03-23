using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using Application = System.Windows.Application;

//https://www.codeproject.com/Articles/820324/Implementing-Dialog-Boxes-in-MVVM

namespace Arctos.Wpf.Dialogs
{
    public static class DialogBehavior
    {
        private static readonly Dictionary<IDialogBaseVM, Window> DialogBoxes = new();
        private static readonly Dictionary<Window, NotifyCollectionChangedEventHandler> ChangeNotificationHandlers = new();
        private static readonly Dictionary<ObservableCollection<IDialogBaseVM>, List<IDialogBaseVM>> DialogBoxViewModels = new();

        public static readonly DependencyProperty ClosingProperty = DependencyProperty.RegisterAttached(
          "Closing",
          typeof(bool),
          typeof(DialogBehavior),
          new PropertyMetadata(false));

        public static readonly DependencyProperty ClosedProperty = DependencyProperty.RegisterAttached(
          "Closed",
          typeof(bool),
          typeof(DialogBehavior),
          new PropertyMetadata(false));

        public static readonly DependencyProperty DialogViewModelsProperty = DependencyProperty.RegisterAttached(
          "DialogViewModels",
          typeof(object),
          typeof(DialogBehavior),
          new PropertyMetadata(null, OnDialogViewModelsChange));

        public static void SetDialogViewModels(DependencyObject source, object value)
        {
            source.SetValue(DialogViewModelsProperty, value);
        }

        public static object GetDialogViewModels(DependencyObject source)
        {
            return source.GetValue(DialogViewModelsProperty);
        }

        private static void OnDialogViewModelsChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is not Window  parent) 
                return;

            // when the parent closes we don't need to track it anymore
            parent.Closed += (s, a) => ChangeNotificationHandlers.Remove(parent);

            // otherwise create a handler for it that responds to changes to the supplied collection
            if (!ChangeNotificationHandlers.ContainsKey(parent))
                ChangeNotificationHandlers[parent] = (sender, args) =>
                {
                    if (sender is ObservableCollection<IDialogBaseVM> collection)
                    {
                        if (args.Action == NotifyCollectionChangedAction.Add ||
                            args.Action == NotifyCollectionChangedAction.Remove ||
                            args.Action == NotifyCollectionChangedAction.Replace)
                        {
                            if (args.NewItems != null)
                                foreach (IDialogBaseVM viewModel in args.NewItems)
                                {
                                    if (!DialogBoxViewModels.ContainsKey(collection))
                                        DialogBoxViewModels[collection] = new List<IDialogBaseVM>();
                                    DialogBoxViewModels[collection].Add(viewModel);
                                    AddDialog(viewModel, collection, parent);
                                }
                            if (args.OldItems != null)
                                foreach (IDialogBaseVM viewModel in args.OldItems)
                                {
                                    RemoveDialog(viewModel);
                                    DialogBoxViewModels[collection].Remove(viewModel);
                                    if (DialogBoxViewModels[collection].Count == 0)
                                        DialogBoxViewModels.Remove(collection);
                                }
                        }
                        else if (args.Action == NotifyCollectionChangedAction.Reset)
                        {
                            // a reset event is typically generated in response to clearing the collection.
                            // unfortunately the framework doesn't provide us with the list of items being
                            // removed which is why we have to keep a mirror in DialogBoxViewModels
                            if (DialogBoxViewModels.TryGetValue(collection, out var value))
                            {
                                foreach (var viewModel in value)
                                    RemoveDialog(viewModel);
                                DialogBoxViewModels.Remove(collection);
                            }
                        }
                    }
                };

            // when the collection is first bound to this property we should create any initial
            // dialogs the user may have added in the main view model's constructor
            if (e.NewValue is ObservableCollection<IDialogBaseVM> newCollection)
            {
                newCollection.CollectionChanged += ChangeNotificationHandlers[parent];
                foreach (var viewModel in newCollection.ToList())
                    AddDialog(viewModel, newCollection, (Window)d);
            }

            // when we remove the binding we need to shut down any dialogs that have been left open
            if (e.OldValue is ObservableCollection<IDialogBaseVM> oldCollection)
            {
                oldCollection.CollectionChanged -= ChangeNotificationHandlers[parent];
                foreach (IDialogBaseVM viewModel in oldCollection.ToList())
                    RemoveDialog(viewModel);
            }
        }

        private static void AddDialog(IDialogBaseVM viewModel, ObservableCollection<IDialogBaseVM> collection, Window owner)
        {
            // find the global resource that has been keyed to this view model type
            //try
            //{
            var resource = Application.Current.TryFindResource(viewModel.GetType());
            if (resource == null)
            {
                collection.Remove(viewModel);
                return;
            }

            // is this resource a presenter?
            if (IsAssignableToGenericType(resource.GetType(), typeof(IDialogPresenter<>)))
            {
                resource.GetType().GetMethod("Show")?.Invoke(resource, new object[] { viewModel });
                collection.Remove(viewModel);
            }

            // is this resource a dialog box window?
            else if (resource is Window dialog)
            {
                if (viewModel is not IDialogVM userViewModel)
                    return;
                //ColorHelper.SetAccentColor(dialog, Color.FromArgb(0xFF, 0x66, 0x66, 0xff));
                dialog.DataContext = userViewModel;
                DialogBoxes[userViewModel] = dialog;
                userViewModel.DialogClosing += (sender, args) =>
                {
                    if (sender is not IDialogVM dialogVM)
                        return;
                    collection.Remove(dialogVM);
                };
                dialog.Closing += (sender, args) =>
                {
                    if (!(bool)dialog.GetValue(ClosingProperty))
                    {
                        dialog.SetValue(ClosingProperty, true);
                        userViewModel.RequestClose();
                        if (!(bool)dialog.GetValue(ClosedProperty))
                        {
                            args.Cancel = true;
                            dialog.SetValue(ClosingProperty, false);
                        }
                    }
                };
                dialog.Closed += (sender, args) =>
                {
                    Debug.Assert(DialogBoxes.ContainsKey(userViewModel));
                    DialogBoxes.Remove(userViewModel);
                };
                dialog.Owner = owner;
                userViewModel.OnOpen();
                if (userViewModel.IsModal)
                    dialog.ShowDialog();
                else
                    dialog.Show();
            }

            //}
            //catch (Exception e)
            //{
            //  Console.WriteLine(e);
            //  throw;
            //}
        }

        private static void RemoveDialog(IDialogBaseVM viewModel)
        {
            if (DialogBoxes.TryGetValue(viewModel, out var value))
            {
                if (!(bool)value.GetValue(ClosingProperty))
                {
                    value.SetValue(ClosingProperty, true);
                    value.Close();
                }
                value.SetValue(ClosedProperty, true);
            }
        }


        // courtesy James Fraumeni/StackOverflow: http://stackoverflow.com/questions/74616/how-to-detect-if-type-is-another-generic-type/1075059#1075059
        private static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            var baseType = givenType.BaseType;
            return baseType != null && IsAssignableToGenericType(baseType, genericType);
        }
    }
}
