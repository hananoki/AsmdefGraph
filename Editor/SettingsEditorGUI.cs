using HananokiEditor.Extensions;
using HananokiEditor.SharedModule;
using UnityEngine;
using E = HananokiEditor.AsmdefGraph.SettingsEditor;
using SS = HananokiEditor.SharedModule.S;


namespace HananokiEditor.AsmdefGraph {

	public class SettingsEditorGUI {

		[HananokiSettingsRegister]
		public static SettingsItem RegisterSettings() {
			return new SettingsItem() {
				displayName = Package.nameNicify,
				version = Package.version,
				gui = DrawGUI,
				mode = 0,
			};
		}



		/////////////////////////////////////////
		public static void DrawGUI() {
			E.Load();

			ScopeChange.Begin();

			E.i.enablePackageAsmdef = HEditorGUILayout.ToggleLeft( "enablePackageAsmdef".nicify(), E.i.enablePackageAsmdef );

			if( ScopeChange.End() ) {
				//s_changed = true;
				E.Save();
			}

		}
	}
}

