﻿namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
	public class BeginTransform : IModelMapInstruction
	{
		public IDynamicValue Name { get; set; }

		public void Accept(IModelMapVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}