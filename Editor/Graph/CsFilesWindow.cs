
using HananokiRuntime;

using singleton = HananokiEditor.AsmdefGraph.AsmdefGraphSingleton;

namespace HananokiEditor.AsmdefGraph {

	public sealed class CsFilesWindow : HEditorWindow {

		void Refresh() {
		}

		void OnEnable() {
			singleton.csFilesWindow = this;
			m_disableShadeMode = true;
			SetTitle( "Cs Files", EditorIcon.cs_script );

			Refresh();
			Helper.New( ref singleton.instance.m_TreeViewCs );
		}

		void OnDisable() {
			singleton.csFilesWindow = null;
		}

		public override void OnClose() {
			singleton.CloseWindow();
		}

		public override void OnDefaultGUI() {
			singleton.treeViewCs.DrawItem();
		}
	}
}
