// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version value="$version"/>
// </file>

using System;
using System.Collections.Generic;

namespace ICSharpCode.SharpDevelop.Dom
{
	public interface IReturnType
	{
		string FullyQualifiedName {
			get;
		}
		
		string Name {
			get;
		}
		
		string Namespace {
			get;
		}
		
		string DotNetName {
			get;
		}
		
		/// <summary>
		/// Gets the array ranks of the return type.
		/// When the return type is not an array, this property returns null.
		/// </summary>
		int[] ArrayDimensions {
			get;
		}
		
		List<IMethod>   GetMethods();
		List<IProperty> GetProperties();
		List<IField>    GetFields();
		List<IEvent>    GetEvents();
		List<IIndexer>  GetIndexers();
	}
}
