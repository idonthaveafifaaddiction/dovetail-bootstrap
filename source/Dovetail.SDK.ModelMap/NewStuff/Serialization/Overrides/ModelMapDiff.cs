﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization.Overrides
{
	public class ModelMapDiff : IModelMapDiff
	{
		private static readonly List<Type> Offsets = new List<Type>
		{
			typeof(BeginRelation),
			typeof(BeginTable),
			typeof(BeginView),
			typeof(BeginAdHocRelation),
			typeof(BeginMappedProperty),
			typeof(BeginMappedCollection),
		};

		private static readonly List<PropertyContext> PropertyContexts = new List<PropertyContext>
		{
			new PropertyContext(typeof(BeginProperty), typeof(EndProperty)),
			new PropertyContext(typeof(BeginMappedProperty), typeof(EndMappedProperty)),
			new PropertyContext(typeof(BeginMappedCollection), typeof(EndMappedCollection)),
		};

		public void Diff(ModelMap map, ModelMap overrides)
		{
			removeProperties(map, overrides);
			addProperties(map, overrides);
		}

		private void removeProperties(ModelMap map, ModelMap overrides)
		{
			var removals = overrides.Instructions.OfType<Instructions.RemoveProperty>().ToArray();
			foreach (var removal in removals)
			{
				var instructionsToRemove = new List<IModelMapInstruction>();
				int count = 0;
				var removing = false;
				foreach (var instruction in map.Instructions)
				{
					var beginProp = instruction as BeginProperty;
					if (beginProp != null && beginProp.Key.EqualsIgnoreCase(removal.Key))
					{
						removing = true;
					}

					if (removing)
					{
						instructionsToRemove.Add(instruction);

						var endProp = instruction as EndProperty;
						if (endProp != null)
						{
							--count;
						}
						else if (beginProp != null)
						{
							++count;
						}

						if (count == 0)
							break;
					}
				}

				instructionsToRemove.Each(map.RemoveInstruction);
			}

			var prunedInstructions = new List<IModelMapInstruction>();
			var contexts = new Stack<IModelMapInstruction>();
			foreach (var instruction in map.Instructions)
			{
				if (contexts.Count != 0)
				{
					var previous = contexts.Peek();
					var previousType = previous.GetType();

					if ((previousType == typeof(BeginRelation) && instruction.GetType() == typeof(EndRelation))
					    || (previousType == typeof(BeginAdHocRelation) && instruction.GetType() == typeof(EndRelation)))
					{
						prunedInstructions.Add(previous);
						prunedInstructions.Add(instruction);
						contexts.Pop();
						continue;
					}
				}

				contexts.Push(instruction);
			}

			prunedInstructions.Each(map.RemoveInstruction);
		}

		private void addProperties(ModelMap map, ModelMap overrides)
		{
			var targetIndex = -1;
			var instructionsToAdd = new List<IModelMapInstruction>();
			var contexts = new Stack<IModelMapInstruction>();
			var mapInstructions = map.Instructions.ToList();
			foreach (var instruction in overrides.Instructions.Where(_ => _.GetType() != typeof(Instructions.RemoveProperty)).ToArray())
			{
				if (shouldOffset(instruction))
				{
					var i = mapInstructions.IndexOf(instruction);
					if (i != -1)
					{
						targetIndex = i + 1;
					}
					else
					{
						targetIndex += 1;
					}
				}

				if (targetIndex == -1)
					continue;

				var context = PropertyContexts.SingleOrDefault(_ => _.Matches(instruction.GetType()));
				if (context != null)
				{
					contexts.Push(context.WaitFor());
				}

				if (contexts.Count != 0)
					instructionsToAdd.Add(instruction);

				if (contexts.Count != 0 && contexts.Peek().GetType() == instruction.GetType())
					contexts.Pop();

				if (contexts.Count == 0 && instructionsToAdd.Count != 0)
				{
					map.InsertInstructions(targetIndex, instructionsToAdd);
					targetIndex += instructionsToAdd.Count;
					instructionsToAdd.Clear();
				}
			}
		}

		private static bool shouldPushContext(IModelMapInstruction instruction)
		{
			return PropertyContexts.Any(_ => _.Matches(instruction.GetType()));
		}

		private static bool shouldOffset(IModelMapInstruction instruction)
		{
			return Offsets.Contains(instruction.GetType());
		}

		public class PropertyContext
		{
			private readonly Type _current;
			private readonly Type _waitFor;

			public PropertyContext(Type current, Type waitFor)
			{
				_current = current;
				_waitFor = waitFor;
			}

			public bool Matches(Type current)
			{
				return _current == current;
			}

			public IModelMapInstruction WaitFor()
			{
				return (IModelMapInstruction) FastYetSimpleTypeActivator.CreateInstance(_waitFor);
			}
		}
	}
}