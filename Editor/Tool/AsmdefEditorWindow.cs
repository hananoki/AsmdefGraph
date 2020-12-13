

using HananokiEditor.Extensions;
using HananokiRuntime;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityReflection;


namespace HananokiEditor {
	public class AsmdefEditorWindow : HEditorWindow {

		UnityEditorSplitterState m_HorizontalSplitter;
		AsmdefEditorTreeView m_treeView;
		public Vector2 m_scroll;

		//[MenuItem( "EditorUtility/AssetLib" )]
		[MenuItem( "Window/Hananoki/" + "Asmdef Editor", false, 10 )]
		public static void Open() {
			GetWindow<AsmdefEditorWindow>();

		}

		public void Refresh() {
			if( m_treeView == null ) {
				Helper.New( ref m_treeView );
			}
			m_treeView?.RegisterFiles();
		}

		void OnEnable() {
			SetTitle( "Asmdef Editor", EditorIcon.assemblyDefinition );

			m_HorizontalSplitter = new UnityEditorSplitterState( 0.4f, 0.6f );

			Refresh();
		}


		void DrawLeftPane() {
			m_treeView.DrawLayoutGUI();
		}


		void DrawRightPane() {
			using( var sc = new GUILayout.ScrollViewScope( m_scroll ) ) {
				m_scroll = sc.scrollPosition;
				m_treeView.DrawItem();
			}
		}


		void DrawToolBar() {
			HGUIToolbar.Begin();
			if( HGUIToolbar.Button( EditorIcon.refresh ) ) {
				Refresh();
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
