﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop.Project
{
	public enum SolutionFormatVersion // TODO: change IProject.MinimumSolutionFormatVersion to this enum type
	{
		VS2005 = 9,
		VS2008 = 10,
		VS2010 = 11,
		VS2012 = 12
	}
	
	/// <summary>
	/// Represents a solution.
	/// </summary>
	public interface ISolution : ISolutionFolder, ICanBeDirty, IConfigurable, IDisposable
	{
		Microsoft.Build.Evaluation.ProjectCollection MSBuildProjectCollection { get; }
		
		/// <summary>
		/// Gets the full path of the .sln file.
		/// </summary>
		FileName FileName { get; }
		
		event EventHandler FileNameChanged;
		
		/// <summary>
		/// Gets the full path of the directory containing the .sln file.
		/// </summary>
		DirectoryName Directory { get; }
		
		/// <summary>
		/// Gets/Sets the startup project.
		/// </summary>
		IProject StartupProject { get; set; }
		
		event EventHandler StartupProjectChanged;
		
		/// <summary>
		/// Gets all projects in the solution.
		/// </summary>
		IModelCollection<IProject> Projects { get; }
		
		/// <summary>
		/// Gets all items in the solution; including those nested within solution folders.
		/// </summary>
		/// <remarks>
		/// The enumerator performs a pre-order walk of the solution folder tree.
		/// </remarks>
		IEnumerable<ISolutionItem> AllItems { get; }
		
		/// <summary>
		/// Gets the list of global sections.
		/// These can be used to store additional data within the solution file.
		/// </summary>
		IList<SolutionSection> GlobalSections { get; }
		
		/// <summary>
		/// Finds the item with the specified <see cref="ISolutionItem.IdGuid"/>;
		/// or returns null if no such item exists.
		/// </summary>
		ISolutionItem GetItemByGuid(Guid guid);
		
		/// <summary>
		/// Gets a container that can be used to store data about the solution.
		/// This data is stored in SharpDevelop's config directory, not directly with the .sln file.
		/// </summary>
		Properties Preferences { get; }
		
		/// <summary>
		/// Saves the preferences for this solution; and also for any projects within this solution.
		/// </summary>
		void SavePreferences();
		
		/// <summary>
		/// Gets whether the solution is read-only.
		/// </summary>
		bool IsReadOnly { get; }
		
		/// <summary>
		/// Saves the solution.
		/// 
		/// This method will not save the project files. Use <see cref="ProjectService.SaveSolution"/> instead to save the solution
		/// and all open projects.
		/// </summary>
		void Save();
	}
	
	/// <summary>
	/// This is a HACK to make project creation work; I don't think we should expose this method.
	/// Maybe make IConfigurationMapping a concrete class and use an event for setting the solution's dirty flag?
	/// </summary>
	internal interface ISolutionInternal : ISolution
	{
		IConfigurationMapping CreateMappingForNewProject();
	}
}
