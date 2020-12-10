
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hananoki.AsmdefGraph {
	[Serializable]
	public class SettingsProject {

		static string projectSettingsPath => $"{Environment.CurrentDirectory}/ProjectSettings/AsmdefGraph.json";

		[Serializable]
		public class Data {
			public string asmName;
			public Vector2 position;
		}

		public static SettingsProject i;

		public List<Data> m_data;


		SettingsProject() {
			m_data = new List<Data>();
		}

		public Vector2 Get( string asmName ) {
			var data = m_data.Find( x => x.asmName == asmName );
			if( data == null ) return Vector2.zero;
			return data.position;
		}

		public void Set( string asmName, Rect position ) {
			var data = m_data.Find( x => x.asmName == asmName );
			if( data == null ) {
				m_data.Add( new Data() );
				data = m_data.Last();
				data.asmName = asmName;
			}
			data.position = new Vector2( position.x, position.y );
		}


		public static void Load() {
			if( i != null ) return;

			i = JsonUtility.FromJson<SettingsProject>( fs.ReadAllText( projectSettingsPath ) );
			if( i == null ) {
				i = new SettingsProject();
				Save();
			}
		}

		public static void Save() {
			fs.WriteAllText( projectSettingsPath, JsonUtility.ToJson( i, true ) );
		}
	}
}