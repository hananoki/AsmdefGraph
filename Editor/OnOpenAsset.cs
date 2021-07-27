using UnityEditorInternal;
using UnityEngine;

namespace HananokiEditor.AsmdefGraph {

	/////////////////////////////////////////
	public class OnOpenAsset {

		/////////////////////////////////////////
		[Hananoki_OnOpenAsset( typeof( AssemblyDefinitionAsset ) )]
		public static bool OpenAsmdefEditor( Object asset, int line ) {

			AsmdefEditorWindow.OpenAsName( asset );
			return true;
		}
	}
}
