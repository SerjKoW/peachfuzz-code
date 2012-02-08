﻿
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Reflection;
using System.Runtime.Serialization;

namespace Peach.Core.Dom
{
	/// <summary>
	/// Choice allows the selection of a single
	/// data element based on the current data set.
	/// 
	/// The other options in the choice are available
	/// for mutation by the mutators.
	/// </summary>
	[DataElement("Choice")]
	[DataElementChildSupportedAttribute(DataElementTypes.Any)]
	[Serializable]
	public class Choice : DataElementContainer
	{
		public OrderedDictionary<string, DataElement> choiceElements = new OrderedDictionary<string, DataElement>();
		DataElement _selectedElement = null;

		public Choice()
		{
		}

		public Choice(string name)
		{
			this.name = name;
		}

		public void SelectDefault()
		{
			this.Clear();
			this.Add(choiceElements[0]);
			_selectedElement = this[0];
		}

		public DataElement SelectedElement
		{
			get
			{
				//if (_selectedElement == null && choiceElements.Count > 0)
				//{
				//    this.Clear();
				//    this.Add(choiceElements[0]);
				//    _selectedElement = this[0];
				//}

				return _selectedElement;
			}
			set
			{
				if (!choiceElements.Values.Contains(value))
					throw new KeyNotFoundException("value was not found");

				this.Clear();
				this.Add(value);
				_selectedElement = value;
				Invalidate();
			}
		}

		public override IEnumerable<DataElement> EnumerateAllElements(List<DataElement> knownParents)
		{
			// First our children
			foreach (DataElement child in this)
				yield return child;

			// Next our children's children
			foreach (DataElement child in this)
			{
				if (!knownParents.Contains(child))
				{
					foreach (DataElement subChild in child.EnumerateAllElements(knownParents))
						yield return subChild;
				}
			}

			if (_selectedElement == null)
			{
				foreach (DataElement child in choiceElements.Values)
					yield return child;

				// Next our children's children
				foreach (DataElement child in choiceElements.Values)
				{
					if (!knownParents.Contains(child))
					{
						foreach (DataElement subChild in child.EnumerateAllElements(knownParents))
							yield return subChild;
					}
				}
			}
		}

		public override Variant GenerateInternalValue()
		{
			Variant value;

			// 1. Default value

			if (_selectedElement == null)
				SelectDefault();

			if (_mutatedValue == null)
				value = new Variant(SelectedElement.Value);

			else
				value = MutatedValue;

			// 2. Relations

			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_RELATIONS) != 0)
			{
				_internalValue = MutatedValue;
				return MutatedValue;
			}

			foreach (Relation r in _relations)
			{
				if (r.Of == this)
					value = r.CalculateFromValue();
			}

			// 3. Fixup

			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_FIXUP) != 0)
			{
				_internalValue = MutatedValue;
				return MutatedValue;
			}

			if (_fixup != null)
				value = _fixup.fixup(this);

			_internalValue = value;
			return value;
		}
	}
}

// end
