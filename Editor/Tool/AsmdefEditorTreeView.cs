
using HananokiEditor.Extensions;
using HananokiRuntime.Extensions;
using HananokiRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;
using UnityReflection;

using UnityObject = UnityEngine.Object;

namespace HananokiEditor {

	using Item = AsmdefEditorTreeViewItem;

	public class AsmdefEditorTreeViewItem : TreeViewItem {
		public AssemblyDefinitionAsset target;
		//public bool isMainAsset;
		//public AssetImporterEditor editor;
		public List<Ref> m_reference;

		public AsmdefAssetJson m_json;
		public bool isGUID;
		public bool isDIRTY;
		public string guid;
		public int changeFormat;
	}

	public class Ref {
		public string asmname;
		public string guid;
		public bool toggle;
	}



	public class AsmdefEditorTreeView : HTreeView<Item> {

		SessionStateString m_lastSelect = new SessionStateString( "m_lastSelect" );

		public AsmdefEditorTreeView() : base( new TreeViewState() ) {
			showAlternatingRowBackgrounds = true;
			//baseIndent = -8;

		}

		public void SelectLastItem() {
			var it = m_registerItems.Find( x => x.displayName == m_lastSelect );
			if( it != null ) SelectItem( it.id );
		}

		public void RegisterFiles() {
			InitID();
			m_registerItems = new List<Item>();

			var guids = AssetDatabase.FindAssets( $"t:AssemblyDefinitionAsset" );

			foreach( var x in guids/*.Select( x => x.ToAssetPath() )*/ ) {

				var path = x.ToAssetPath();
				if( path.StartWithPackage() ) continue;
				var packageName = path.FileNameWithoutExtension();
				if( packageName.StartsWith( "Unity" ) ) continue;

				//var imp = AssetImporter.GetAtPath( p );

				var _json = new AsmdefAssetJson( path );

				var _reference = new List<Ref>( 128 );
				//var dic = new IList();
				bool isGUID = false;

				if( _json.hasReferences ) {
					//var dic = (IList) _json[ "references" ];

					foreach( var e in _json.references ) {
						var name = (string) e;
						if( name.StartsWith( "GUID" ) ) {
							isGUID = true;
							name = name.Split( ':' )[ 1 ].ToAssetPath().FileNameWithoutExtension();
						}
						//Debug.Log( CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName( (string) e ) );
						var val = new Ref {
							asmname = name,
							guid = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName( (string) e ).ToGUID(),
							toggle = true,
						};
						_reference.Add( val );
					}
				}
				if( _json.hasReferencesBackup ) {
					//var dic = (IList) _json[ "references.backup" ];
					foreach( var e in _json.referencesBackup ) {
						//Debug.Log( CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName( (string) e ) );
						var val = new Ref {
							asmname = (string) e,
							guid = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName( (string) e ).ToGUID(),
							toggle = false,
						};
						_reference.Add( val );
					}
				}

				var item = new Item {
					displayName = packageName,
					id = GetID(),
					icon = UnityEditorEditorGUIUtility.LoadIcon( "icons/processed/unityeditorinternal/assemblydefinitionasset icon.asset" ),
					target = (AssemblyDefinitionAsset) path.LoadAsset(),
					//editor = (AssetImporterEditor) Editor.CreateEditor( imp ),
					m_json = _json,
					m_reference = _reference,
					isGUID = isGUID,
					guid = x,
				};

				m_registerItems.Add( item );
			}

			m_registerItems = m_registerItems.OrderBy( x => x.displayName ).ToList();

			ReloadAndSorting();
		}


		public void ReloadAndSorting() {
			Reload();

			SelectLastItem();
		}


		protected override void SelectionChanged( IList<int> selectedIds ) {
			m_lastSelect.Value = currentItem.displayName;
		}


		public void DrawItem() {
			if( currentItem == null ) return;

			using( new GUILayoutScope( 16, 4 ) ) {

				////////////////////////

				ScopeHorizontal.Begin();
				EditorGUILayout.LabelField( "General", EditorStyles.boldLabel );
				GUILayout.FlexibleSpace();
				if( GUILayout.Button( "Sort" ) ) Sort( currentItem );
				ScopeDisable.Begin( !currentItem.isDIRTY );
				if( GUILayout.Button( "Apply" ) ) ApplyAndSave( currentItem );
				ScopeDisable.End();
				ScopeHorizontal.End();

				ScopeVertical.Begin( GUI.skin.box );
				ScopeChange.Begin();
				currentItem.m_json.autoReferenced=EditorGUILayout.Toggle( "Auto Referenced", currentItem.m_json.autoReferenced );
				if( ScopeChange.End() ) {
					currentItem.isDIRTY = true;
				}
				ScopeVertical.End();

				////////////////////////

				ScopeHorizontal.Begin();
				EditorGUILayout.LabelField( "Assembly Definition References", EditorStyles.boldLabel );
				GUILayout.FlexibleSpace();
				//if( GUILayout.Button( "Sort" ) ) Sort( currentItem );
				//ScopeDisable.Begin( !currentItem.isDIRTY );
				//if( GUILayout.Button( "Apply" ) ) ApplyAndSave( currentItem );
				//ScopeDisable.End();
				ScopeHorizontal.End();

				////////////////////////

				Ref del = null;
				foreach( var e in currentItem.m_reference ) {
					ScopeHorizontal.Begin( EditorStyles.helpBox );
					ScopeChange.Begin();
					e.toggle = HEditorGUILayout.ToggleLeft( e.asmname, e.toggle );
					if( ScopeChange.End() ) {
						currentItem.isDIRTY = true;
					}
					GUILayout.FlexibleSpace();
					if( HEditorGUILayout.IconButton( EditorIcon.minus ) ) {
						del = e;
						currentItem.isDIRTY = true;
					}

					ScopeHorizontal.End();
				}
				if( del != null ) {
					currentItem.m_reference.Remove( del );
				}

				GUILayout.FlexibleSpace();
			}


			var dropRc = GUILayoutUtility.GetLastRect();
			DrawFileDragArea( dropRc, DD, DragAndDropVisualMode.Link );
			//HEditorGUI.DrawDebugRect( dropRc );
		}


		void DD() {
			foreach( var p in DragAndDrop.objectReferences ) {

				Debug.Log( $"{p.name}: {p.GetType().Name}" );
				if( typeof( AssemblyDefinitionAsset ) == p.GetType() ) {
					currentItem.m_reference.Add(
						new Ref {
							asmname = p.ToAssetPath().FileNameWithoutExtension(),
							toggle = true,
						} );
					currentItem.isDIRTY = true;
				}
			}
		}


		void DrawFileDragArea( Rect rc, Action dropCallback, DragAndDropVisualMode visualMode = DragAndDropVisualMode.Generic ) {
			Event evt = Event.current;
			//GUI.Box( dropArea, dropAreaMessage );

			switch( evt.type ) {
			// ドラッグ中.
			case EventType.DragUpdated:
			case EventType.DragPerform:
				if( !rc.Contains( evt.mousePosition ) ) break;

				// Dragされている間のカーソルの見た目を変更.
				DragAndDrop.visualMode = visualMode;

				if( evt.type == EventType.DragPerform ) {
					// オブジェクトを受け入れる.
					DragAndDrop.AcceptDrag();
					dropCallback();
					DragAndDrop.activeControlID = 0;
				}
				Event.current.Use();
				break;
			}
		}


		public void ApplyAndSave( Item item ) {
			Apply( item );
			AssetDatabase.Refresh();
		}

		public void Apply( Item item ) {
			string Format( Ref _ref, bool isGUID ) {

				if( isGUID && !_ref.guid.IsEmpty() ) {
					return $"GUID:{_ref.guid}";
				}
				return _ref.asmname;
			}
			try {
				//var refs = (IList) m_json[ "references" ];
				//if( item.m_json.hasReferences ) {
				var s = item.m_reference.Where( x => x.toggle ).Select( x => Format( x, item.isGUID ) ).ToArray();
				item.m_json.references = s;
				//}

				//if( item.m_json.hasReferencesBackup) {
				var ss = item.m_reference.Where( x => !x.toggle ).Select( x => Format( x, item.isGUID ) ).ToArray();
				item.m_json.referencesBackup = ss;
				//}

				item.m_json.Save();
				//AssetDatabase.ImportAsset( item.target.ToAssetPath() );

				item.isDIRTY = false;
			}
			catch( Exception e ) {
				Debug.LogException( e );
			}
		}


		public void Sort( Item item ) {
			var s = item.m_reference.Where( x => x.toggle ).OrderBy( x => x.asmname );
			var ss = item.m_reference.Where( x => !x.toggle ).OrderBy( x => x.asmname );

			//var _reference = item.m_reference.OrderBy( x => x.asmname ).ToList();
			var _reference = new List<Ref>();
			_reference.AddRange( s.ToArray() );
			_reference.AddRange( ss.ToArray() );
			if( _reference.SequenceEqual( item.m_reference ) ) {
				//Debug.Log("onaji");
				return;
			}
			item.m_reference = _reference;
			item.isDIRTY = true;
		}



		protected override void OnRowGUI( RowGUIArgs args ) {
			DefaultRowGUI( args );

			var item = (Item) args.item;

			var rc2 = args.rowRect.W( 16 );
			rc2.x += ( 16 * ( item.depth + 1 ) );
			rc2 = rc2.AlignCenter( 14, 14 );
			//EditorGUI.DrawRect( rc2,new Color(0,0,1,0.2f) );
			if( EditorHelper.HasMouseClick( rc2 ) ) {
				EditorApplication.delayCall += () => { EditorHelper.PingAndSelectObject( item.target ); };
			}

			if( item.isDIRTY ) {
				GUI.DrawTexture( args.rowRect.W( 16 ), EditorIcon.warning );
			}
			if( item.isGUID ) {
				HEditorGUI.MiniLabelR( args.rowRect, "GUID" );
				//EditorGUI.LabelField( args.rowRect.AlignR( 16 ), "G" );
			}
			else if( item.m_reference.Count == 0 ) {
				HEditorGUI.MiniLabelR( args.rowRect, "-" );
				//EditorGUI.LabelField( args.rowRect.AlignR( 16 ), "G" );
			}
		}



		public void SaveAssetDirty() {
			using( var prog = new ProgressBarScope( "Save - AssetName", m_registerItems.Count ) ) {
				foreach( var p in m_registerItems ) {
					prog.Next( p.displayName );
					if( !p.isDIRTY ) continue;
					if( p.changeFormat == 1 ) {
						p.isGUID = false;
					}
					else if( p.changeFormat == 2 ) {
						p.isGUID = true;
					}
					Apply( p );
					//}
				}
				AssetDatabase.Refresh();
			}
		}


		public void ChangeAsmName() {
			for( int i = 0; i < m_registerItems.Count; i++ ) {
				var p = m_registerItems[ i ];
				if( !p.isGUID ) continue;

				p.changeFormat = 1;
				p.isDIRTY = true;
			}
		}

		public void ChangeGUID() {
			for( int i = 0; i < m_registerItems.Count; i++ ) {
				var p = m_registerItems[ i ];
				if( p.isGUID ) continue;

				p.changeFormat = 2;
				p.isDIRTY = true;
			}
		}


		#region Rename

		protected override bool CanRename( TreeViewItem item ) {
			if( item == null ) return false;
			//if( item.displayName.IsEmpty() ) return false;

			var _it = (Item) item;
			//if( _it.hasSubAsset ) return false;

			return true;
		}

		protected override void RenameEnded( RenameEndedArgs args ) {
			base.RenameEnded( args );

			if( args.newName.Length <= 0 ) goto err;
			if( args.newName == args.originalName ) goto err;

			args.acceptedRename = true;

			var item = FindItem( args.itemID, rootItem ) as Item;

			//AssetDatabase.RenameAsset( item.target .ToAssetPath(), args.newName );
			//item.target.name = args.newName;
			item.displayName = args.newName;
			item.isDIRTY = true;
			renameItem = item;
			EditorApplication.delayCall += RenameAssets;
			//var a = P.i.GetFolders( item.node.ID );
			//a.name = args.newName;
			//P.Save();
			//AssetDatabase.SaveAssets(); // AssetDatabase余分に呼ばれるのでリフレッシュのみ
			//AssetDatabase.Refresh(); // リフレッシュしないとAssetPostprocessorが動かない

			Reload();
			//throw new ExitGUIException();
			GUIUtility.ExitGUI();
			return;

			err:
			args.acceptedRename = false;
		}

		#endregion

		Item renameItem;


		void RenameAssets() {
			using( var prog = new ProgressBarScope( "Save - AssetName", m_registerItems.Count + 1 ) ) {
				foreach( var p in m_registerItems ) {
					prog.Next( p.displayName );
					bool dirty = false;
					if( p.id == renameItem.id ) continue;

					foreach( var pp in p.m_reference ) {
						if( renameItem.guid == pp.guid ) {
							//Debug.Log( $"{p.displayName} : {pp.asmname}" );
							pp.asmname = renameItem.displayName;
							dirty = true;
						}
					}
					if( dirty ) {
						var s = p.m_reference.Where( x => x.toggle ).Select( x => x.asmname ).ToArray();
						p.m_json.references = s;
						var ss = p.m_reference.Where( x => !x.toggle ).Select( x => x.asmname ).ToArray();
						p.m_json.referencesBackup = ss;
						p.m_json.Save();
					}
				}

				prog.Next( renameItem.displayName );

				renameItem.m_json.Save( renameItem.displayName );
			}
			AssetDatabase.Refresh();
		}



		#region DragAndDrop

		public const string k_GenericDragID = "AAA.GenericData";

		protected override void SetupDragAndDrop( SetupDragAndDropArgs args ) {
			if( args.draggedItemIDs == null ) return;

			DragAndDrop.PrepareStartDrag();

			var selected = new List<Item>();
			foreach( var id in args.draggedItemIDs ) {
				var item = FindItem( id, rootItem ) as Item;
				selected.Add( item );
			}
			//var ss = selected.Select( x => x.guid.ToAssetPath() ).ToArray(); ;
			DragAndDrop.objectReferences = new UnityObject[] { selected[ 0 ].target };
			DragAndDrop.paths = null;
			//DragAndDrop.paths = new string[10];
			//DragAndDrop.paths[ 0 ] = "aaa";
			DragAndDrop.SetGenericData( k_GenericDragID, selected );
			DragAndDrop.visualMode = DragAndDropVisualMode.Move;
			DragAndDrop.StartDrag( "ScriptableObjectTreeView" );
		}

		protected override bool CanStartDrag( CanStartDragArgs args ) {
			return true;
		}

		#endregion
	}
}

