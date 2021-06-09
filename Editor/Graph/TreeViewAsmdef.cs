
using HananokiEditor.Extensions;
using HananokiRuntime.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.IMGUI.Controls;

using singleton = HananokiEditor.AsmdefGraph.AsmdefGraphSingleton;

namespace HananokiEditor.AsmdefGraph {

	using Item = DependItem;


	public class DependItem : TreeViewItem {
		public string assetPath;
		public AsmdefNode node;
	}


	public class TreeViewAsmdef : HTreeView<Item> {

		List<Item> m_itemAll;
		List<Item> m_itemRefBy;
		List<Item> m_itemRefTo;

		public TreeViewAsmdef() : base( new TreeViewState() ) {
			baseIndent = -8;
			showAlternatingRowBackgrounds = true;
			m_registerItems = new List<Item>();
		}


		public void Show( int m ) {
			var lst = new object[] { m_itemAll, m_itemRefBy, m_itemRefTo };

			if( lst[ m ] != null ) {
				m_registerItems = new List<Item>( (List<Item>) lst[ m ] );
			}
			else {
				m_registerItems = new List<Item>();
			}
			m_registerItems = m_registerItems.OrderBy( x => x.displayName ).ToList();
			ReloadAndSorting();
		}

		public void RegisterAll() {

			InitID();
			m_itemAll = new List<Item>();

			foreach( var p in singleton.nodes.Values ) {
				var mainItem = new Item {
					displayName = p.assembly.name,
					id = GetID(),
					node = p,
					assetPath = p.GetAssetPath(),
					icon = EditorIcon.assetIcon_AssemblyDefinition,
				};
				m_itemAll.Add( mainItem );
			}
		}


		public void RegisterFiles( AsmdefNode[] refBy, AsmdefNode[] refTo ) {

			InitID();
			m_itemRefBy = new List<Item>();
			foreach( var p in refBy ) {
				var mainItem = new Item {
					displayName = p.assembly.name,
					id = GetID(),
					node = p,
					assetPath = p.GetAssetPath(),
					icon = EditorIcon.assetIcon_AssemblyDefinition,
				};
				m_itemRefBy.Add( mainItem );
			}

			InitID();
			m_itemRefTo = new List<Item>();
			foreach( var p in refTo ) {
				var mainItem = new Item {
					displayName = p.assembly.name,
					id = GetID(),
					node = p,
					assetPath = p.GetAssetPath(),
					icon = EditorIcon.assetIcon_AssemblyDefinition,
				};
				m_itemRefTo.Add( mainItem );
			}
		}


		public void ReloadAndSorting() {
			Reload();
		}


		public void SelectAsmdef( Assembly assembly ) {
			var item = m_registerItems.Find( x => x.node.assembly == assembly );
			if( item == null ) return;

			SelectItem( item.id );
		}



		protected override void SingleClickedItem( int id ) {
			var item = FindItem( id );

			singleton.treeViewCs.RegisterFiles( item.node.assembly );

			var path = item.node.GetAssetPath();
			Selection.activeObject = path.LoadAsset();
			singleton.csFilesWindow.Repaint();

			//item.node.selected=true;
		}


		protected override void OnRowGUI( RowGUIArgs args ) {
			DefaultRowGUI( args );

			var item = (Item) args.item;

			if( !IsSelected( item.id ) ) return;

			if( item.assetPath.IsEmpty() ) return;

			if( HEditorGUI.IconButton( args.rowRect.AlignR( 16 ), EditorIcon.editicon_sml ) ) {
				ReferenceEditor.Open( item.assetPath );
			}
		}
	}
}
