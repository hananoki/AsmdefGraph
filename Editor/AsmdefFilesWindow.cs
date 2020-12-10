
using singleton = Hananoki.AsmdefGraph.AsmdefGraphSingleton;

namespace Hananoki.AsmdefGraph {

	public sealed class AsmdefFilesWindow : HEditorWindow {

		public int m_selectIndex;

		void OnEnable() {
			singleton.asmdefFilesWindow = this;
			m_disableShadeMode = true;
			SetTitle( "Asmdef Files", singleton.asmDefIcon );

			Helper.New( ref singleton.instance.m_TreeViewAsmdef );
		}


		void OnDisable() {
			singleton.asmdefFilesWindow = null;
		}


		public override void OnClose() {
			singleton.CloseWindow();
		}


		public override void OnDefaultGUI() {
			if( singleton.instance.reload ) {
				singleton.treeViewAsmdef.RegisterAll();
				singleton.treeViewAsmdef.Show( m_selectIndex );
				singleton.instance.reload = false;
			}
			if( singleton.instance.reloadNodeClick ) {
				singleton.treeViewAsmdef.Show( m_selectIndex );
				singleton.instance.reload = false;
			}

			HGUIToolbar.Begin();
			if( HGUIToolbar.Toggle( m_selectIndex == 0, "All" ) ) m_selectIndex = 0;
			if( HGUIToolbar.Toggle( m_selectIndex == 1, "Ref By" ) ) m_selectIndex = 1;
			if( HGUIToolbar.Toggle( m_selectIndex == 2, "Ref To" ) ) m_selectIndex = 2;
			HGUIToolbar.End();

			singleton.treeViewAsmdef.DrawLayoutGUI();
		}

	}
}
