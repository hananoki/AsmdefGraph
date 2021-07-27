
using HananokiEditor.Extensions;
using HananokiRuntime;
using HananokiRuntime.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;
using E = HananokiEditor.AsmdefGraph.SettingsEditor;
using UnityObject = UnityEngine.Object;


namespace HananokiEditor {

	using Item = AsmdefEditorTreeViewItem;

	public class Ref {
		public string asmDefの名前;
		public string guid;
		public bool toggle;
		public bool バックアップされている;
	}



	public class AsmdefEditorTreeView : HTreeView<Item> {

		SessionStateString m_lastSelect = new SessionStateString( "m_lastSelect" );
		public readonly string k_GenericDragID = $"{typeof( AsmdefEditorTreeView ).Name}.GenericData";

		public List<Item> m_asmdefItems;


		public AsmdefEditorTreeView() : base( new TreeViewState() ) {
			showAlternatingRowBackgrounds = true;
			//baseIndent = -8;

		}

		public void SelectLastItem() {
			var it = m_root.displayNameで探す( m_lastSelect );
			if( it != null ) SelectItem( it.id );
		}


		public void RegisterFiles() {
			InitID();
			MakeRoot();
			m_asmdefItems = new List<Item>();

			var guids = AssetDatabase.FindAssets( $"t:AssemblyDefinitionAsset" );

			foreach( var guid in guids/*.Select( x => x.ToAssetPath() )*/ ) {

				var assetPath = guid.ToAssetPath();

				if( !E.i.enablePackageAsmdef ) {
					if( assetPath.StartWithPackage() ) continue;
				}
				var packageName = assetPath.FileNameWithoutExtension();
				//if( packageName.StartsWith( "Unity" ) ) continue;


				var asmdefJson = new AsmdefAssetJson( assetPath );

				var 参照しているasmdefのリスト = new List<Ref>( 128 );

				bool isGUID = false;

				if( asmdefJson.hasReferences ) {
					foreach( var e in asmdefJson.references ) {
						var name = (string) e;
						if( name.StartsWith( "GUID" ) ) {
							isGUID = true;
							name = name.Split( ':' )[ 1 ].ToAssetPath().FileNameWithoutExtension();
						}
						var val = new Ref {
							asmDefの名前 = name,
							guid = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName( (string) e ).ToGUID(),
							toggle = true,
						};
						参照しているasmdefのリスト.Add( val );
					}
				}
				if( asmdefJson.hasReferencesBackup ) {
					foreach( var e in asmdefJson.referencesBackup ) {
						var val = new Ref {
							asmDefの名前 = (string) e,
							guid = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName( (string) e ).ToGUID(),
							toggle = false,
							バックアップされている = true,
						};
						参照しているasmdefのリスト.Add( val );
					}
				}
				var parent = m_root;
				if( assetPath.StartWithPackage() ) {
					parent = (Item) m_root.children.FindSafe( x => x.displayName == "Package" );
					if( parent == null ) {
						parent = new Item {
							displayName = "Package",
							id = GetID(),
							icon = EditorIcon.folder,
							folder = true,
						};
						m_root.AddChild( parent );
					}
				}
				else {
					parent = (Item) m_root.children.FindSafe( x => x.displayName == "Assets" );
					if( parent == null ) {
						parent = new Item {
							displayName = "Assets",
							id = GetID(),
							icon = EditorIcon.folder,
							folder = true,
						};
						m_root.AddChild( parent );
					}
				}


				var item = new Item {
					displayName = packageName,
					id = GetID(),
					icon = EditorIcon.icons_processed_unityeditorinternal_assemblydefinitionasset_icon_asset,

					target = (AssemblyDefinitionAsset) assetPath.LoadAsset(),
					//editor = (AssetImporterEditor) Editor.CreateEditor( imp ),
					m_json = asmdefJson,
					参照しているasmdefのリスト = 参照しているasmdefのリスト,
					isGUID = isGUID,
					guid = guid,
				};

				if( 0 < 参照しているasmdefのリスト.Where( x => x.guid.IsEmpty() ).Where( x => !x.バックアップされている ).Count() ) {
					item.missingなasmdefを検知 = true;
				}
				parent.AddChild( item );
				m_asmdefItems.Add( item );
			}
			m_root.displayNameでアルファベット順にする();
			UpdateAllDepth();

			ReloadAndSorting();

		}


		public void ReloadAndSorting() {
			ReloadRoot();
			ExpandAll();

			SelectLastItem();
		}


		protected override void OnSelectionChanged( Item[] items ) {
			m_lastSelect.Value = currentItem.displayName;
		}


		/////////////////////////////////////////
		protected override void OnDoubleClickedItem( Item item ) {
#if UNITY_EDITOR_WIN
			ShellUtils.Start( "explorer.exe", $"{fs.currentDirectory}/{item.guid.ToAssetPath()}".separatorToOS() );
#endif
		}

		void DD() {
			foreach( var p in DragAndDrop.objectReferences ) {

				Debug.Log( $"{p.name}: {p.GetType().Name}" );
				if( typeof( AssemblyDefinitionAsset ) == p.GetType() ) {
					currentItem.参照しているasmdefのリスト.Add(
						new Ref {
							asmDefの名前 = p.ToAssetPath().FileNameWithoutExtension(),
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
				return _ref.asmDefの名前;
			}
			try {
				//var refs = (IList) m_json[ "references" ];
				//if( item.m_json.hasReferences ) {
				var s = item.参照しているasmdefのリスト.Where( x => x.toggle ).Select( x => Format( x, item.isGUID ) ).ToArray();
				item.m_json.references = s;
				//}

				//if( item.m_json.hasReferencesBackup) {
				var ss = item.参照しているasmdefのリスト.Where( x => !x.toggle ).Select( x => Format( x, item.isGUID ) ).ToArray();
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
			var s = item.参照しているasmdefのリスト.Where( x => x.toggle ).OrderBy( x => x.asmDefの名前 );
			var ss = item.参照しているasmdefのリスト.Where( x => !x.toggle ).OrderBy( x => x.asmDefの名前 );

			//var _reference = item.m_reference.OrderBy( x => x.asmname ).ToList();
			var _reference = new List<Ref>();
			_reference.AddRange( s.ToArray() );
			_reference.AddRange( ss.ToArray() );
			if( _reference.SequenceEqual( item.参照しているasmdefのリスト ) ) {
				//Debug.Log("onaji");
				return;
			}
			item.参照しているasmdefのリスト = _reference;
			item.isDIRTY = true;
		}



		/////////////////////////////////////////
		protected override void OnRowGUI( Item item, RowGUIArgs args ) {
			if( item.missingなasmdefを検知 ) {
				EditorGUI.DrawRect( args.rowRect, ColorUtils.RGBA( Color.red, 0.2f ) );
			}
			DefaultRowGUI( args );

			var rc2 = args.rowRect.W( 16 );
			rc2.x += ( 16 * ( item.depth + 1 ) );
			rc2 = rc2.AlignCenter( 14, 14 );

			if( EditorHelper.HasMouseClick( rc2 ) ) {
				EditorApplication.delayCall += () => { EditorHelper.PingAndSelectObject( item.target ); };
			}

			if( item.isDIRTY ) {
				GUI.DrawTexture( args.rowRect.W( 16 ), EditorIcon.warning );
			}
			if( item.isGUID ) {
				HEditorGUI.MiniLabelR( args.rowRect, "GUID" );
			}
		}


		/////////////////////////////////////////
		public void DrawItem() {
			if( currentItem == null ) return;
			if( currentItem.folder ) return;

			using( new GUILayoutScope( 16, 4 ) ) {

				////////////////////////

				ScopeHorizontal.Begin();
				EditorGUILayout.LabelField( "General", EditorStyles.boldLabel );
				GUILayout.FlexibleSpace();
				ScopeHorizontal.End();

				ScopeVertical.Begin( GUI.skin.box );
				ScopeChange.Begin();
				currentItem.m_json.autoReferenced = EditorGUILayout.Toggle( "Auto Referenced", currentItem.m_json.autoReferenced );
				if( ScopeChange.End() ) {
					currentItem.isDIRTY = true;
				}
				ScopeVertical.End();

				////////////////////////

				ScopeHorizontal.Begin();
				EditorGUILayout.LabelField( "Assembly Definition References", EditorStyles.boldLabel );
				GUILayout.FlexibleSpace();
				ScopeHorizontal.End();

				////////////////////////

				Ref del = null;

				foreach( var e in currentItem.参照しているasmdefのリスト ) {
					using( new GUIBackgroundColorScope() ) {
						if( e.guid.IsEmpty() ) {
							if( e.バックアップされている ) {
								GUI.backgroundColor = Color.yellow;
							}
							else {
								GUI.backgroundColor = Color.red;
							}
						}
						ScopeHorizontal.Begin( EditorStyles.helpBox );
						ScopeChange.Begin();
						e.toggle = HEditorGUILayout.ToggleLeft( e.asmDefの名前, e.toggle );
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
				}
				if( del != null ) {
					currentItem.参照しているasmdefのリスト.Remove( del );
				}

				//ScopeHorizontal.Begin();
				//GUILayout.FlexibleSpace();
				//if( GUILayout.Button( "整形") ) {
				//	Debug.Log( currentItem.m_json.name );
				//	ShellUtils.Start( "dotnet-format", $"-v diag {currentItem.m_json.name}.csproj" );
				//	//ShellUtils.Start( "cmd.exe", "/k dir" );
				//	//System.Diagnostics.Process p = new System.Diagnostics.Process();
				//	//p.StartInfo.FileName = "cmd.exe";
				//	//p.StartInfo.Arguments = "/k dotnet-format -v diag com.unity.cinemachine.Editor.csproj";
				//	//p.SynchronizingObject = this;
				//	//p.StartInfo.UseShellExecute = true;
				//	//p.Exited += ( sender, e ) => {
				//	//	EditorApplication.delayCall += () => {
				//	//		exitHandler( sender, e );
				//	//	};
				//	//};
				//	//p.Start();
				//	//p.WaitForExit();
				//}
				//ScopeHorizontal.End();

				GUILayout.FlexibleSpace();
			}


			var dropRc = GUILayoutUtility.GetLastRect();
			DrawFileDragArea( dropRc, DD, DragAndDropVisualMode.Link );
			//HEditorGUI.DrawDebugRect( dropRc );
		}



		public void SaveAssetDirty() {
			using( var prog = new ProgressBarScope( "Save - AssetName", m_root.children.Count ) ) {
				foreach( var p in m_asmdefItems ) {
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
			for( int i = 0; i < m_asmdefItems.Count; i++ ) {
				var p = m_asmdefItems[ i ];
				if( !p.isGUID ) continue;

				p.changeFormat = 1;
				p.isDIRTY = true;
			}
		}

		public void ChangeGUID() {
			for( int i = 0; i < m_asmdefItems.Count; i++ ) {
				var p = m_asmdefItems[ i ];
				if( p.isGUID ) continue;

				p.changeFormat = 2;
				p.isDIRTY = true;
			}
		}


		#region Rename

		protected override bool CanRename( TreeViewItem item ) {
			if( item == null ) return false;
			return true;
		}


		protected override void RenameEnded( RenameEndedArgs args ) {
			base.RenameEnded( args );

			if( args.newName.Length <= 0 ) goto err;
			if( args.newName == args.originalName ) goto err;

			args.acceptedRename = true;

			var item = ToItem( args.itemID );

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
			using( var prog = new ProgressBarScope( "Save - AssetName", m_root.children.Count + 1 ) ) {
				foreach( var p in m_asmdefItems ) {
					prog.Next( p.displayName );
					bool dirty = false;
					if( p.id == renameItem.id ) continue;

					foreach( var pp in p.参照しているasmdefのリスト ) {
						if( renameItem.guid == pp.guid ) {
							//Debug.Log( $"{p.displayName} : {pp.asmname}" );
							pp.asmDefの名前 = renameItem.displayName;
							dirty = true;
						}
					}
					if( dirty ) {
						var s = p.参照しているasmdefのリスト.Where( x => x.toggle ).Select( x => x.asmDefの名前 ).ToArray();
						p.m_json.references = s;
						var ss = p.参照しているasmdefのリスト.Where( x => !x.toggle ).Select( x => x.asmDefの名前 ).ToArray();
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

		protected override void OnSetupDragAndDrop( Item[] items ) {
			DragAndDrop.PrepareStartDrag();

			DragAndDrop.objectReferences = new UnityObject[] { items[ 0 ].target };
			DragAndDrop.paths = null;
			DragAndDrop.SetGenericData( k_GenericDragID, items );
			DragAndDrop.visualMode = DragAndDropVisualMode.Move;
			DragAndDrop.StartDrag( $"{typeof( AsmdefEditorTreeView ).Name}.StartDrag" );
		}

		protected override bool OnCanStartDrag( Item item, CanStartDragArgs args ) => true;

		#endregion
	}
}

