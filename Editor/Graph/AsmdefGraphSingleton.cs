using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityReflection;

namespace HananokiEditor.AsmdefGraph {
	internal class AsmdefGraphSingleton : ScriptableSingleton<AsmdefGraphSingleton> {

		//public Texture2D _asmDefIcon;
		//public static Texture2D asmDefIcon {
		//	get {
		//		if( instance._asmDefIcon == null ) {
		//			instance._asmDefIcon = UnityEditorEditorGUIUtility.LoadIcon( "icons/processed/unityeditorinternal/assemblydefinitionasset icon.asset" );
		//		}
		//		return instance._asmDefIcon;
		//	}
		//}

		// Key: アセンブリ名
		// Value: ノード
		public Dictionary<string, AsmdefNode> m_nodes = new Dictionary<string, AsmdefNode>();
		public static Dictionary<string, AsmdefNode> nodes => instance.m_nodes;


		public bool reload;
		public bool reloadNodeClick;

		public static void CloseWindow() {
			instance.m_AsmdefGraphWindow?.Close();
			instance.m_CsFilesWindow?.Close();
			instance.m_AsmdefFilesWindow?.Close();
		}

		public AsmdefGraphWindow m_AsmdefGraphWindow;
		public static AsmdefGraphWindow asmdefGraphWindow {
			get => instance.m_AsmdefGraphWindow;
			set => instance.m_AsmdefGraphWindow = value;
		}

		public CsFilesWindow m_CsFilesWindow;
		public static CsFilesWindow csFilesWindow {
			get => instance.m_CsFilesWindow;
			set => instance.m_CsFilesWindow = value;
		}

		public AsmdefFilesWindow m_AsmdefFilesWindow;
		public static AsmdefFilesWindow asmdefFilesWindow {
			get => instance.m_AsmdefFilesWindow;
			set => instance.m_AsmdefFilesWindow = value;
		}



		public TreeViewCs m_TreeViewCs;
		public static TreeViewCs treeViewCs {
			get => instance.m_TreeViewCs;
			set => instance.m_TreeViewCs = value;
		}

		public TreeViewAsmdef m_TreeViewAsmdef;
		public static TreeViewAsmdef treeViewAsmdef {
			get => instance.m_TreeViewAsmdef;
			set => instance.m_TreeViewAsmdef = value;
		}
	}
}

