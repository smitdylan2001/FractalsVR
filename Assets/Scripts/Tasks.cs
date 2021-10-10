using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Tasks
{
	public string Task;
	public floatTask ft;

	public enum Functions
	{
        IsBigger,
		IsBiggerOrEqual,
		IsSmaller,
		IsSmallerOrEqual,
		IsVector3Close,
		IsVector2Close
	}

	public Dictionary<string, Functions> Actions;
[System.Serializable]
	public struct floatTask
	{
		public Functions Function;
		public float value1, value2;
	}
	public struct Vector3Task
	{
		public Functions Function;
		public Vector3 value1, value2;
	}
	public struct Vector2Task
	{
		public Functions Function;
		public Vector2 value1, value2;
	}
}
