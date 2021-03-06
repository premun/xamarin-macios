﻿using System;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Xamarin.Localization.MSBuild;
using Xamarin.Utils;

namespace Xamarin.MacDev.Tasks
{
	public abstract class CompileAppManifestTaskBase : XamarinTask
	{
		#region Inputs

		[Required]
		public string AppBundleName { get; set; }

		[Required]
		public string AppManifest { get; set; }

		[Required]
		public string AppManifestBundleDirectory { get; set; }

		[Required]
		public string AssemblyName { get; set; }

		[Required]
		public string BundleIdentifier { get; set; }

		[Required]
		public bool Debug { get; set; }

		public string DebugIPAddresses { get; set; }

		[Required]
		public string DefaultSdkVersion { get; set; }

		[Required]
		public bool IsAppExtension { get; set; }

		public bool IsXPCService { get; set; }

		public bool IsWatchApp { get; set; }

		public bool IsWatchExtension { get; set; }

		[Required]
		public string MinimumOSVersion { get; set; }

		public ITaskItem[] PartialAppManifests { get; set; }

		public string ResourceRules { get; set; }

		[Required]
		public string SdkPlatform { get; set; }

		[Required]
		public bool SdkIsSimulator { get; set; }

		public string TargetArchitectures { get; set; }
		#endregion

		#region Outputs

		[Output]
		public ITaskItem CompiledAppManifest { get; set; }

		#endregion

		protected TargetArchitecture architectures;

		public override bool Execute ()
		{
			PDictionary plist;

			try {
				plist = PDictionary.FromFile (AppManifest);
			} catch (Exception ex) {
				LogAppManifestError (MSBStrings.E0010, AppManifest, ex.Message);
				return false;
			}

			plist.SetIfNotPresent (PlatformFrameworkHelper.GetMinimumOSVersionKey (Platform), MinimumOSVersion);

			if (!string.IsNullOrEmpty (TargetArchitectures) && !Enum.TryParse (TargetArchitectures, out architectures)) {
				LogAppManifestError (MSBStrings.E0012, TargetArchitectures);
				return false;
			}

			plist.SetIfNotPresent (ManifestKeys.CFBundleIdentifier, BundleIdentifier);
			plist.SetIfNotPresent (ManifestKeys.CFBundleInfoDictionaryVersion, "6.0");
			plist.SetIfNotPresent (ManifestKeys.CFBundlePackageType, IsAppExtension ? "XPC!" : "APPL");
			plist.SetIfNotPresent (ManifestKeys.CFBundleSignature, "????");
			plist.SetIfNotPresent (ManifestKeys.CFBundleVersion, "1.0");
			plist.SetIfNotPresent (ManifestKeys.CFBundleExecutable, AssemblyName);

			if (!Compile (plist))
				return false;

			// Merge with any partial plists generated by the Asset Catalog compiler...
			MergePartialPlistTemplates (plist);

			CompiledAppManifest = new TaskItem (Path.Combine (AppManifestBundleDirectory, "Info.plist"));
			plist.Save (CompiledAppManifest.ItemSpec, true, true);

			return !Log.HasLoggedErrors;
		}

		protected abstract bool Compile (PDictionary plist);

		protected void LogAppManifestError (string format, params object[] args)
		{
			// Log an error linking to the Info.plist file
			Log.LogError (null, null, null, AppManifest, 0, 0, 0, 0, format, args);
		}

		protected void LogAppManifestWarning (string format, params object[] args)
		{
			// Log a warning linking to the Info.plist file
			Log.LogWarning (null, null, null, AppManifest, 0, 0, 0, 0, format, args);
		}

		protected void SetValue (PDictionary dict, string key, string value)
		{
			if (dict.ContainsKey (key))
				return;

			if (string.IsNullOrEmpty (value))
				LogAppManifestWarning (MSBStrings.W0106, key);
			else
				dict[key] = value;
		}

		protected void MergePartialPlistDictionary (PDictionary plist, PDictionary partial)
		{
			foreach (var property in partial) {
				if (plist.ContainsKey (property.Key)) {
					var value = plist[property.Key];

					if (value is PDictionary && property.Value is PDictionary) {
						MergePartialPlistDictionary ((PDictionary) value, (PDictionary) property.Value);
					} else {
						plist[property.Key] = property.Value.Clone ();
					}
				} else {
					plist[property.Key] = property.Value.Clone ();
				}
			}
		}

		protected void MergePartialPlistTemplates (PDictionary plist)
		{
			if (PartialAppManifests == null)
				return;

			foreach (var template in PartialAppManifests) {
				PDictionary partial;

				try {
					partial = PDictionary.FromFile (template.ItemSpec);
				} catch (Exception ex) {
					Log.LogError (MSBStrings.E0107, template.ItemSpec, ex.Message);
					continue;
				}

				MergePartialPlistDictionary (plist, partial);
			}
		}
	}
}
