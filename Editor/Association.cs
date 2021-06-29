using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;

namespace HananokiEditor.AsmdefGraph {

	/////////////////////////////////////////
	public class Association {

		/////////////////////////////////////////
		[OnOpenAsset( 1 )] // OnOpenAsset��2�ȍ~�̓G�f�B�^�������ɂȂ�
		public static bool OnOpen( int instanceID, int line ) {

			var asset = EditorUtility.InstanceIDToObject( instanceID );

			if( asset.GetType() != typeof( AssemblyDefinitionAsset ) ) return false;
			
			AsmdefEditorWindow.OpenAsName( asset );

			return true;
		}
	}
}
