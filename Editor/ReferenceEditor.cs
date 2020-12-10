
using Hananoki.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
//using UnityReflection;

namespace Hananoki.AsmdefGraph {

	public partial class ReferenceEditor : HEditorWindow {

		public class Ref {
			public string asmname;
			public bool toggle;
		}

		static string arg;
		public string assetPath;

		List<Ref> m_reference;

		Dictionary<string, object> m_json;



		public static void Open( string assetPath ) {
			arg = assetPath;
			GetWindow<ReferenceEditor>( true, $"{ arg.FileName()}" );
		}



		void OnEnable() {
			SetTitle( $"{assetPath.FileNameWithoutExtension()}" );
			if( arg != null ) {
				assetPath = arg;
				arg = null;
			}
			var asmdef = assetPath.LoadAsset<AssemblyDefinitionAsset>();
			m_json = (Dictionary<string, object>) EditorJson.Deserialize( asmdef.text );

			var dic = (IList) m_json[ "references" ];
			m_reference = new List<Ref>( 128 );
			foreach( var e in dic ) {
				var val = new Ref {
					asmname = (string) e,
					toggle = true,
				};
				m_reference.Add( val );
			}

			if( m_json.ContainsKey( "references.backup" ) ) {
				dic = (IList) m_json[ "references.backup" ];
				foreach( var e in dic ) {
					var val = new Ref {
						asmname = (string) e,
						toggle = false,
					};
					m_reference.Add( val );
				}
			}
		}


		public override void OnDefaultGUI() {
			//////////////////////
			EditorGUILayout.LabelField( "Assembly Definition References", EditorStyles.boldLabel );
			HGUIScope.Vertical( EditorStyles.helpBox );
			foreach( var e in m_reference ) {
				e.toggle = HEditorGUILayout.ToggleLeft( e.asmname, e.toggle );
			}
			HGUIScope.End();


			//////////////////////
			GUILayout.FlexibleSpace();
			HGUIScope.Horizontal();
			GUILayout.FlexibleSpace();
			if( GUILayout.Button( "Apply" ) ) Apply();
			GUILayout.Space( 8 );
			HGUIScope.End();
			GUILayout.Space( 8 );
		}



		public void Apply() {
			try {
				//var refs = (IList) m_json[ "references" ];
				if( m_json.ContainsKey( "references" ) ) {
					var s = m_reference.Where( x => x.toggle ).Select( x => x.asmname ).ToArray();
					m_json[ "references" ] = s;
				}

				if( m_json.ContainsKey( "references.backup" ) ) {
					var s = m_reference.Where( x => !x.toggle ).Select( x => x.asmname ).ToArray();
					m_json[ "references.backup" ] = s;
				}
				else {
					var s = m_reference.Where( x => !x.toggle ).Select( x => x.asmname ).ToArray();
					m_json.Add( "references.backup", s );
				}

				fs.WriteAllText( assetPath, EditorJson.Serialize( m_json, true ), new UTF8Encoding( false ) );
				AssetDatabase.ImportAsset( assetPath );
			}
			catch( Exception e ) {
				Debug.LogException( e );
			}
		}

	}
}
