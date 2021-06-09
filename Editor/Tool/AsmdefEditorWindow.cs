

using HananokiEditor.Extensions;
using HananokiRuntime;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityReflection;


namespace HananokiEditor {
	public class AsmdefEditorWindow : HNEditorWindow<AsmdefEditorWindow> {

		UnityEditorSplitterState m_HorizontalSplitter;
		internal AsmdefEditorTreeView m_treeView;
		public Vector2 m_scroll;


		//[MenuItem( "EditorUtility/AssetLib" )]
		[MenuItem( "Window/Hananoki/" + "Asmdef Editor", false, 10 )]
		public static void Open() {
			GetWindow<AsmdefEditorWindow>();
		}
		public static void OpenAsName( string asmdefName ) {
			//Debug.Log( asmdefName );
			var lasctSelect = new SessionStateString( "m_lastSelect" );
			lasctSelect.Value = asmdefName;
			var window = EditorWindowUtils.Find<AsmdefEditorWindow>();
			if( window ) {
				window.m_treeView.SelectLastItem();
				window.Repaint();
				return;
			}
			Open();
		}

		public void Refresh() {
			if( m_treeView == null ) {
				Helper.New( ref m_treeView );
			}
			m_treeView?.RegisterFiles();
		}



		void OnEnable() {
			SetTitle( "Asmdef Editor", EditorIcon.assetIcon_AssemblyDefinition );

			m_HorizontalSplitter = new UnityEditorSplitterState( 0.4f, 0.6f );

			Refresh();
		}


		void DrawLeftPane() {
			HGUIToolbar.Begin();
			if( HGUIToolbar.Button( EditorIcon.refresh ) ) {
				Refresh();
				EditorHelper.ShowMessagePop( "Refresh OK." );
			}

			bool isDirty = m_treeView.m_registerItems.Where( x => x.isDIRTY ).Count() != 0;
			ScopeDisable.Begin( !isDirty );
			if( HGUIToolbar.Button( "Apply All" ) ) {
				m_treeView.SaveAssetDirty();
			}
			ScopeDisable.End();

			if( HGUIToolbar.DropDown( "Change Format" ) ) {
				var m = new GenericMenu();
				m.AddItem( "Assembly Name", () => m_treeView.ChangeAsmName() );
				m.AddItem( "GUID", () => m_treeView.ChangeGUID() );
				m.DropDownPopupRect( HEditorGUI.lastRect );
				//
			}

			GUILayout.FlexibleSpace();
			HGUIToolbar.End();

			m_treeView.DrawLayoutGUI();
		}


		void DrawRightPane() {
			HGUIToolbar.Begin();
			if( HGUIToolbar.Button( "ソースコード整形" ) ) {
				ShellUtils.Start( "dotnet-format", $"-v diag {m_treeView.currentItem.m_json.name}.csproj" );
			}
			//GUILayout.Space(1);

			GUILayout.FlexibleSpace();
			if( HGUIToolbar.Button( "Sort" ) ) m_treeView.Sort( m_treeView.currentItem );
			ScopeDisable.Begin( m_treeView.currentItem == null ? true : !m_treeView.currentItem.isDIRTY );
			if( HGUIToolbar.Button( "Apply" ) ) m_treeView.ApplyAndSave( m_treeView.currentItem );
			ScopeDisable.End();
			HGUIToolbar.End();

			using( var sc = new GUILayout.ScrollViewScope( m_scroll ) ) {
				m_scroll = sc.scrollPosition;
				m_treeView.DrawItem();
			}
		}


		void DrawToolBar() {

		}


		public override void OnDefaultGUI() {

			ScopeDisable.BeginIsCompiling();
			DrawToolBar();

			UnityEditorSplitterGUILayout.BeginHorizontalSplit( m_HorizontalSplitter );
			ScopeVertical.Begin();
			DrawLeftPane();
			ScopeVertical.End();
			ScopeVertical.Begin( HEditorStyles.dopesheetBackground );
			DrawRightPane();
			ScopeVertical.End();
			UnityEditorSplitterGUILayout.EndHorizontalSplit();

			ScopeDisable.End();

		}
	}
}
