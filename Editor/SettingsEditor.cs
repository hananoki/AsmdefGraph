using HananokiRuntime.Extensions;
using System;
using E = HananokiEditor.AsmdefGraph.SettingsEditor;


namespace HananokiEditor.AsmdefGraph {

	[Serializable]
	internal class SettingsEditor {
		public static E i;

		public string m_scriptTemplate = "";
		public int dockingIndex;

		//public int monoTypeAnalysisNum = 1;
		//public int csAnalysisNum = 100;

		public int flag;

		public const int ENABLE_PACKAGE_ASMDEF = 1 << 0;
		//public const int PB_DOUBLE_CLICK = 1 << 1;
		//public const int PB_CALLBACK_USE = 1 << 2;

		//public const int TOOLBAR_ICON_ONLY = 1 << 3;

		public bool enablePackageAsmdef {
			get => flag.Has( ENABLE_PACKAGE_ASMDEF );
			set => flag.Toggle( ENABLE_PACKAGE_ASMDEF, value );
		}
	
		//public bool projectBrowserDClick { get => flag.Has( PB_DOUBLE_CLICK ); set => flag.Toggle( PB_DOUBLE_CLICK, value ); }
		//public bool projectBrowserCallbackUse { get => flag.Has( PB_CALLBACK_USE ); set => flag.Toggle( PB_CALLBACK_USE, value ); }




		public static void Load() {
			if( i != null ) return;
			i = EditorPrefJson<E>.Get( Package.editorPrefName );
		}


		public static void Save() {
			EditorPrefJson<E>.Set( Package.editorPrefName, i );
		}
	}
}
