
using HananokiEditor.Extensions;
using UnityEditor;
using UnityEngine;
using UnityReflection;

using singleton = HananokiEditor.AsmdefGraph.AsmdefGraphSingleton;
using SS = HananokiEditor.SharedModule.S;

namespace HananokiEditor.AsmdefGraph {

	public partial class AsmdefGraphWindow : HEditorWindow {

		[MenuItem( "Window/Hananoki/" + "Asmdef Graph", false, 11 )]
		public static void Open() {
			GetWindow<AsmdefGraphWindow>();
		}

		AsmdefGraphView graphView;

		readonly string kPath = System.Environment.CurrentDirectory + "/Library/Layout.wlt";
		const string kWLT = "dbef5ff65867dff428fa261aaf3073a6";



		protected override void ShowButton( Rect r ) {
			base.ShowButton( r );
			if( HEditorGUI.IconButton( r, EditorIcon.editicon_sml, "Save WindowLayout" ) ) {
				UnityEditorWindowLayout.SaveWindowLayout( kWLT.ToAssetPath() );
				EditorHelper.ShowMessagePop( SS._Saved );
			}

			var exist = kPath.IsExistsFile();
			r.x -= 16;
			ScopeChange.Begin();
			GUI.Toggle( r, exist, EditorHelper.TempContent( "", "Change WindowLayout" ) );
			if( ScopeChange.End() ) {
				if( exist ) {
					UnityEditorWindowLayout.LoadWindowLayout( kPath, false );
					fs.rm( kPath ); ;
				}
				else {
					UnityEditorWindowLayout.SaveWindowLayout( kPath );
					UnityEditorWindowLayout.LoadWindowLayout( kWLT.ToAssetPath(), false );
				}
			}
		}


		void OnEnable() {
			singleton.asmdefGraphWindow = this;
			m_disableShadeMode = true;
			SetTitle( "Asmdef Graph", EditorIcon.icons_processed_unityengine_ui_graphicraycaster_icon_asset );

			// view�̍쐬
			graphView = new AsmdefGraphView() {
				style = { flexGrow = 1 }
			};
			//graphView.transform
			rootVisualElement.Add( graphView );

			if( Find( typeof( CsFilesWindow ) ) == null ) {
				GetWindow<CsFilesWindow>();
			}
			if( Find( typeof( AsmdefFilesWindow ) ) == null ) {
				GetWindow<AsmdefFilesWindow>();
			}
		}


		void OnDisable() {
			singleton.asmdefGraphWindow = null;
			SettingsProject.Save();
		}


		public override void OnClose() {
			singleton.CloseWindow();
		}


	}
}
