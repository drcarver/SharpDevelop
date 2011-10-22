﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ICSharpCode.Core;
using ICSharpCode.Core.Presentation;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Editor.Search;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Widgets.Resources;

namespace SearchAndReplace
{
	/// <summary>
	/// Implements ISearchResult and provides the ResultsTreeView.
	/// </summary>
	public class DefaultSearchResult : ISearchResult
	{
		IList<SearchResultMatch> matches;
		SearchRootNode rootNode;
		
		public DefaultSearchResult(string title, IEnumerable<SearchResultMatch> matches)
		{
			if (title == null)
				throw new ArgumentNullException("title");
			if (matches == null)
				throw new ArgumentNullException("matches");
			this.matches = matches.ToArray();
			rootNode = new SearchRootNode(title, this.matches);
		}
		
		public string Text {
			get {
				return rootNode.Title + " (" + SearchRootNode.GetOccurrencesString(rootNode.Occurrences) + ")";
			}
		}
		
		static ResultsTreeView resultsTreeViewInstance;
		
		public object GetControl()
		{
			WorkbenchSingleton.AssertMainThread();
			if (resultsTreeViewInstance == null)
				resultsTreeViewInstance = new ResultsTreeView();
			rootNode.GroupResultsByFile(ResultsTreeView.GroupResultsByFile);
			resultsTreeViewInstance.ItemsSource = new object[] { rootNode };
			return resultsTreeViewInstance;
		}
		
		static IList toolbarItems;
		static MenuItem flatItem, perFileItem;
		
		public IList GetToolbarItems()
		{
			WorkbenchSingleton.AssertMainThread();
			if (toolbarItems == null) {
				toolbarItems = new List<object>();
				DropDownButton perFileDropDown = new DropDownButton();
				perFileDropDown.Content = new Image { Height = 16, Source = PresentationResourceService.GetBitmapSource("Icons.16x16.FindIcon") };
				perFileDropDown.SetValueToExtension(DropDownButton.ToolTipProperty, new LocalizeExtension("MainWindow.Windows.SearchResultPanel.SelectViewMode.ToolTip"));
				
				flatItem = new MenuItem();
				flatItem.SetValueToExtension(MenuItem.HeaderProperty, new LocalizeExtension("MainWindow.Windows.SearchResultPanel.Flat"));
				flatItem.Click += delegate { SetPerFile(false); };
				
				perFileItem = new MenuItem();
				perFileItem.SetValueToExtension(MenuItem.HeaderProperty, new LocalizeExtension("MainWindow.Windows.SearchResultPanel.PerFile"));
				perFileItem.Click += delegate { SetPerFile(true); };
				
				perFileDropDown.DropDownMenu = new ContextMenu();
				perFileDropDown.DropDownMenu.Items.Add(flatItem);
				perFileDropDown.DropDownMenu.Items.Add(perFileItem);
				toolbarItems.Add(perFileDropDown);
				toolbarItems.Add(new Separator());
				
				Button expandAll = new Button();
				expandAll.SetValueToExtension(Button.ToolTipProperty, new LocalizeExtension("MainWindow.Windows.SearchResultPanel.ExpandAll.ToolTip"));
				expandAll.Content = new Image { Height = 16, Source = PresentationResourceService.GetBitmapSource("Icons.16x16.OpenAssembly") };
				expandAll.Click += delegate { ExpandCollapseAll(true); };
				toolbarItems.Add(expandAll);
				
				Button collapseAll = new Button();
				collapseAll.SetValueToExtension(Button.ToolTipProperty, new LocalizeExtension("MainWindow.Windows.SearchResultPanel.CollapseAll.ToolTip"));
				collapseAll.Content = new Image { Height = 16, Source = PresentationResourceService.GetBitmapSource("Icons.16x16.Assembly") };
				collapseAll.Click += delegate { ExpandCollapseAll(false); };
				toolbarItems.Add(collapseAll);
			}
			return toolbarItems;
		}
		
		static void ExpandCollapseAll(bool newIsExpanded)
		{
			if (resultsTreeViewInstance != null) {
				foreach (SearchNode node in resultsTreeViewInstance.ItemsSource.OfType<SearchNode>().Flatten(n => n.Children)) {
					node.IsExpanded = newIsExpanded;
				}
			}
		}
		
		static void SetPerFile(bool perFile)
		{
			ResultsTreeView.GroupResultsByFile = perFile;
			if (resultsTreeViewInstance != null) {
				foreach (SearchRootNode node in resultsTreeViewInstance.ItemsSource.OfType<SearchRootNode>()) {
					node.GroupResultsByFile(perFile);
				}
			}
		}
	}
	
	public class DefaultSearchResultFactory : ISearchResultFactory
	{
		public ISearchResult CreateSearchResult(string title, IEnumerable<SearchResultMatch> matches)
		{
			return new DefaultSearchResult(title, matches);
		}
		
		public ISearchResult CreateSearchResult(string title, IObservable<SearchResultMatch> matches)
		{
			var osr = new ObserverSearchResult(title);
			osr.Registration = matches.ObserveOnUIThread().Subscribe(osr);
			return osr;
		}
	}
	
	public class ObserverSearchResult : ISearchResult, IObserver<SearchResultMatch>
	{
		static Button stopButton;
		SearchRootNode rootNode;
		
		public ObserverSearchResult(string title)
		{
			Text = title;
			rootNode = new SearchRootNode(title, new List<SearchResultMatch>());
		}
		
		public string Text { get; private set; }
		public IDisposable Registration { get; set; }
		
		static ResultsTreeView resultsTreeViewInstance;
		
		public object GetControl()
		{
			WorkbenchSingleton.AssertMainThread();
			if (resultsTreeViewInstance == null)
				resultsTreeViewInstance = new ResultsTreeView();
			rootNode.GroupResultsByFile(ResultsTreeView.GroupResultsByFile);
			resultsTreeViewInstance.ItemsSource = new object[] { rootNode };
			return resultsTreeViewInstance;
		}
		
		static IList toolbarItems;
		static MenuItem flatItem, perFileItem;
		
		public IList GetToolbarItems()
		{
			WorkbenchSingleton.AssertMainThread();
			if (toolbarItems == null) {
				toolbarItems = new List<object>();
				DropDownButton perFileDropDown = new DropDownButton();
				perFileDropDown.Content = new Image { Height = 16, Source = PresentationResourceService.GetBitmapSource("Icons.16x16.FindIcon") };
				perFileDropDown.SetValueToExtension(DropDownButton.ToolTipProperty, new LocalizeExtension("MainWindow.Windows.SearchResultPanel.SelectViewMode.ToolTip"));
				
				flatItem = new MenuItem();
				flatItem.SetValueToExtension(MenuItem.HeaderProperty, new LocalizeExtension("MainWindow.Windows.SearchResultPanel.Flat"));
				flatItem.Click += delegate { SetPerFile(false); };
				
				perFileItem = new MenuItem();
				perFileItem.SetValueToExtension(MenuItem.HeaderProperty, new LocalizeExtension("MainWindow.Windows.SearchResultPanel.PerFile"));
				perFileItem.Click += delegate { SetPerFile(true); };
				
				perFileDropDown.DropDownMenu = new ContextMenu();
				perFileDropDown.DropDownMenu.Items.Add(flatItem);
				perFileDropDown.DropDownMenu.Items.Add(perFileItem);
				toolbarItems.Add(perFileDropDown);
				toolbarItems.Add(new Separator());
				
				Button expandAll = new Button();
				expandAll.SetValueToExtension(Button.ToolTipProperty, new LocalizeExtension("MainWindow.Windows.SearchResultPanel.ExpandAll.ToolTip"));
				expandAll.Content = new Image { Height = 16, Source = PresentationResourceService.GetBitmapSource("Icons.16x16.OpenAssembly") };
				expandAll.Click += delegate { ExpandCollapseAll(true); };
				toolbarItems.Add(expandAll);
				
				Button collapseAll = new Button();
				collapseAll.SetValueToExtension(Button.ToolTipProperty, new LocalizeExtension("MainWindow.Windows.SearchResultPanel.CollapseAll.ToolTip"));
				collapseAll.Content = new Image { Height = 16, Source = PresentationResourceService.GetBitmapSource("Icons.16x16.Assembly") };
				collapseAll.Click += delegate { ExpandCollapseAll(false); };
				toolbarItems.Add(collapseAll);
				
				stopButton = new Button { Content = new Image { Height = 16, Source = PresentationResourceService.GetBitmapSource("Icons.16x16.Debug.StopProcess") } };
				stopButton.Click += StopButtonClick;
				toolbarItems.Add(stopButton);
			}
			stopButton.Visibility = Visibility.Visible;
			return toolbarItems;
		}

		void StopButtonClick(object sender, RoutedEventArgs e)
		{
			stopButton.Visibility = Visibility.Hidden;
			if (Registration != null) Registration.Dispose();
		}
		
		static void ExpandCollapseAll(bool newIsExpanded)
		{
			if (resultsTreeViewInstance != null) {
				foreach (SearchNode node in resultsTreeViewInstance.ItemsSource.OfType<SearchNode>().Flatten(n => n.Children)) {
					node.IsExpanded = newIsExpanded;
				}
			}
		}
		
		static void SetPerFile(bool perFile)
		{
			ResultsTreeView.GroupResultsByFile = perFile;
			if (resultsTreeViewInstance != null) {
				foreach (SearchRootNode node in resultsTreeViewInstance.ItemsSource.OfType<SearchRootNode>()) {
					node.GroupResultsByFile(perFile);
				}
			}
		}
		
		void IObserver<SearchResultMatch>.OnNext(SearchResultMatch value)
		{
			rootNode.Add(value);
		}
		
		void IObserver<SearchResultMatch>.OnError(Exception error)
		{
			MessageService.ShowException(error);
			OnCompleted();
		}
		
		void OnCompleted()
		{
			stopButton.Visibility = Visibility.Collapsed;
			if (Registration != null)
				Registration.Dispose();
		}
		
		void IObserver<SearchResultMatch>.OnCompleted()
		{
			OnCompleted();
		}
	}
}
