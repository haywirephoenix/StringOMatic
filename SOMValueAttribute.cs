using System;
using UnityEngine;

public class SOMValueAttribute : PropertyAttribute {
	public readonly Type module;

	public SOMValueAttribute (Type module) {
		this.module = module;
	}
	public SOMValueAttribute(){
		this.module = Type.GetType("StringOMatic");
	}
}