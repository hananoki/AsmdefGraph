using HananokiEditor.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.IMGUI.Controls;

namespace HananokiEditor.AsmdefGraph {

	using Item = CsFileItem;


	public class CsFileItem : TreeViewItem {
		public string assetPath;
	}


	public class TreeViewCs : HTreeView<Item> {

		public TreeViewCs() : base( new TreeViewState() ) {
			showAlternatingRowBackgrounds = true;
		}

		public void RegisterFiles( Assembly assembly ) {

			InitID();
			m_registerItems = new List<Item>();

			foreach( var p in assembly.sourceFiles ) {
				var mainItem = new Item {
					displayName = p.FileNameWithoutExtension(),
					id = GetID(),
					assetPath = p,
					//isMainAsset = true,
					//editor = Editor.CreateEditor( main[ 0 ] ),
					icon = EditorIcon.cs_script,
				};
				m_registerItems.Add( mainItem );
			}
			m_registerItems = m_registerItems.OrderBy( x => x.displayName ).ToList();

			ReloadAndSorting();
		}


		public void ReloadAndSorting() {
			Reload();
		}



		protected override void OnSingleClickedItem( Item item ) {
			Selection.activeObject = item.assetPath.LoadAsset();
		}


		public void DrawItem() {
			if( rootItem != null  ) {
				DrawLayoutGUI();
			}
			else {
				EditorGUILayout.HelpBox( "Select Asmdef File ...", MessageType.Warning );
			}
		}


		protected override void OnRowGUI( RowGUIArgs args ) {
			//DefaultRowGUI( args );

			var item = (Item) args.item;

			var rc = args.rowRect;
			rc.x += 4;
			EditorGUI.LabelField( rc, EditorHelper.TempContent( item.displayName, item.icon ) );
		}
	}
}
