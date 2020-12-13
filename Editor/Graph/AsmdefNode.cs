
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor.Compilation;
using HananokiEditor.Extensions;
using UnityEditor;
using System.Linq;

using singleton = HananokiEditor.AsmdefGraph.AsmdefGraphSingleton;

namespace HananokiEditor.AsmdefGraph {
	public class AsmdefNode : Node/*, IAsmdefNodeView */{
		public Port leftPort;
		public Port rightPort;

		public Assembly assembly;
		static Port selectPort;
		static Port selectPortR;


		public string GetAssetPath() {
			return CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName( assembly.name );
		}

		public AsmdefNode( Assembly assembly, VisualElement parentContentContainer ) {
			title = assembly.name;
			this.assembly = assembly;
			
			leftPort = Port.Create<Edge>( Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof( Port ) );
			inputContainer.Add( leftPort );

			rightPort= Port.Create<Edge>( Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof( Port ) );
			outputContainer.Add( rightPort );

			RegisterCallback( ( MouseDownEvent evt ) => {
				// 左クリック
				if( evt.button == 0 ) {
					//focus = true;  // 選択
					if( selectPort != null ) {
						selectPort.portColor = Color.white;
						foreach( var p in selectPort.connections ) {
							p.UpdateEdgeControl();
						}
					}
					if( selectPortR != null ) {
						selectPortR.portColor = Color.white;
						foreach( var p in selectPortR.connections ) {
							p.UpdateEdgeControl();
						}
					}
					leftPort.portColor = Color.magenta;
					rightPort.portColor = Color.cyan;
					foreach( var p in leftPort.connections ) {
						p.UpdateEdgeControl();
					}
					foreach( var p in rightPort.connections ) {
						p.UpdateEdgeControl();
					}
					selectPort = leftPort;
					selectPortR = rightPort;

					var path = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName( title );
					Selection.activeObject = path.LoadAsset();

					singleton.treeViewAsmdef?.SelectAsmdef( assembly );
					singleton.treeViewAsmdef?.SetFocusAndEnsureSelectedItem();
					singleton.asmdefFilesWindow?.Repaint();

					var refBy = leftPort.connections.Select( x => ( (AsmdefNode) x.output.node ) ).ToArray();
					var refTo = rightPort.connections.Select( x => ( (AsmdefNode) x.input.node ) ).ToArray();
					singleton.treeViewAsmdef?.RegisterFiles( refBy, refTo );
					singleton.asmdefFilesWindow?.Repaint();
				}
			} );
			RegisterCallback( ( MouseUpEvent evt ) => {
				if( evt.button == 0 ) {
					//Debug.Log( $"{title}: {GetPosition()}" );
					SettingsProject.Save();
				}
			} );
		}


		public void RefreshPortName() {
			leftPort.portName = $"Ref By: ({leftPort.connections.Count()})";
			rightPort.portName = $"Ref To: ({rightPort.connections.Count()})";
		}
	}
}
