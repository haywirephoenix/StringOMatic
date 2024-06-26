﻿using UnityEngine;

namespace SOM
{
	/// <summary>
	/// Base class for every module
	/// </summary>
	public abstract class SOMModule
	{
		//============================
		//Properties
		//============================
		/// <summary>
		/// The short name of the module
		/// </summary>
		public abstract string moduleName { get; }

		/// <summary>
		/// The key name used on SOMPreferences to check if this module needs to be refreshed
		/// </summary>
		public string needsRefreshingKey
		{
			get { return moduleName + " Needs Refreshing"; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this module needs refreshing.
		/// </summary>
		/// <value><c>true</c> if needs refreshing; otherwise, <c>false</c>.</value>
		public virtual bool needsRefreshing
		{
			get
			{
				return SOMPreferences.bools[needsRefreshingKey];
			}
			set
			{
				SOMPreferences.bools[needsRefreshingKey] = value;
			}
		}

		//============================
		//Methods
		//============================
		/// <summary>
		/// Provides the base functionality for the module
		/// </summary>
		public abstract void Refresh();

		const string FORCE_REFRESH = "Force Refresh";

		/// <summary>
		/// This is called to draw custom preferences for the module
		/// </summary>
		public virtual void DrawPreferences()
		{
			//Pressing this button forces this module to refresh
			// if (GUILayout.Button(FORCE_REFRESH))
			// {
			// 	SOMManager.Refresh(moduleName, false);
			// 	SOMDataHandler.Save();
			// 	SOMCSHarpHandler.Compile();
			// }
		}
	}
}
