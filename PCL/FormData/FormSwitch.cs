﻿using System;
namespace PCL
{
	public class FormSwitch: FormElement
	{
		public bool DefaultValue;
		public bool Value;
		public string Text;

		public FormSwitch()
		{
			DefaultValue = false;
			Value = true;
			Text = "";
		}
	}
}
