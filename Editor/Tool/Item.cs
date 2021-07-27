using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;

namespace HananokiEditor {
	public class AsmdefEditorTreeViewItem : TreeViewItem {
		public AssemblyDefinitionAsset target;

		public List<Ref> �Q�Ƃ��Ă���asmdef�̃��X�g;

		public AsmdefAssetJson m_json;
		public bool isGUID;
		public bool isDIRTY;
		public string guid;
		public int changeFormat;

		public bool missing��asmdef�����m;
		public bool folder;
	}
}
