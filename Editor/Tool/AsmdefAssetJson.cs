
using HananokiRuntime.Extensions;
using HananokiEditor.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HananokiEditor {
	public class AsmdefAssetJson {
		public TextAsset m_object;
		Dictionary<string, object> m_data;

		public AsmdefAssetJson( string assetPath ) {
			Load( assetPath );
		}

		public string name {
			get => (string) m_data[ "name" ];
			set => m_data[ "name" ] = (string) value;
		}

		public bool hasReferences => m_data.ContainsKey( "references" );
		public bool hasReferencesBackup => m_data.ContainsKey( "references.backup" );

		public IList this[ string _name ] {
			get {
				return (IList) m_data[ _name ];
			}
			set {
				//if( value == null || value.Count == 0 ) return;

				if( m_data.ContainsKey( _name ) ) {
					m_data[ _name ] = value;
				}
				else {
					m_data.Add( _name, value );
				}
			}
		}

		public IList references {
			get => this[ "references" ];
			set => this[ "references" ] = value;
		}
		public IList referencesBackup {
			get => this[ "references.backup" ];
			set => this[ "references.backup" ] = value;

		}

		public void Load( string _assetPath ) {
			Load( _assetPath.LoadAsset<TextAsset>() );
		}

		public void Load( TextAsset _object ) {
			m_object = _object;
			m_data = (Dictionary<string, object>) EditorJson.Deserialize( m_object.text );
		}

		public void Save( string newName = null ) {
			var fpath = m_object.ToAssetPath();
			bool rename = false;
			if( !newName.IsEmpty() ) {
				if( fpath.FileNameWithoutExtension() != newName ) {
					name = newName;
					rename = true;
				}
			}
			fs.WriteAllText( m_object.ToAssetPath(), EditorJson.Serialize( m_data, true ) );
			if( rename ) {
				AssetDatabase.RenameAsset( fpath, name );
			}
		}

	}
}
