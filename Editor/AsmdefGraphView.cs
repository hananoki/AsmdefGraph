
using Hananoki.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Compilation;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using singleton = Hananoki.AsmdefGraph.AsmdefGraphSingleton;

namespace Hananoki.AsmdefGraph {
	public sealed class AsmdefGraphView : GraphView {

		//public new class UxmlFactory : UxmlFactory<AsmdefGraphView, UxmlTraits> { }

		Dictionary<string, AsmdefNode> m_nodes {
			get => singleton.instance.m_nodes;
			set => singleton.instance.m_nodes = value;
		}

		public AsmdefGraphView() {
			SettingsProject.Load();

			var USS = "f1a648a2e6dd154409abfd3d9c90bd22".LoadAsset();
			styleSheets.Add( (StyleSheet) USS );

			// .asmdefをすべて取得
			var asmdefs = CompilationPipeline.GetAssemblies();
			// 頭にUnityが付くものは無視
			var assemblies = asmdefs.Where( x => !x.name.StartsWith( "Unity" ) ).ToArray();

			// zoom可能に
			SetupZoom( ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale );

			// 背景を黒に
			Insert( 0, new GridBackground() );

			// ドラッグによる移動可能に
			this.AddManipulator( new SelectionDragger() );

			// ドラッグで描画範囲を動かせるように
			this.AddManipulator( new ContentDragger() );


			// ノードでMouseUpEventが取れなかったのでこっちで取る
			// 全ノード書き込むので非効率
			RegisterCallback( ( MouseUpEvent evt ) => {
				if( evt.button == 0 ) {
					foreach( var p in m_nodes ) {
						SettingsProject.i.Set( p.Key, p.Value.GetPosition() );
					}
				}
			} );


			// ノードの追加
			m_nodes = new Dictionary<string, AsmdefNode>();
			foreach( var asm in assemblies ) {
				var node = new AsmdefNode( asm, contentContainer );
				AddElement( node );
				var pos = SettingsProject.i.Get( asm.name );
				var r = node.GetPosition();
				node.SetPosition( new Rect( pos.x, pos.y, r.width, r.height ) );
				m_nodes.Add( asm.name, node );
			}

			//var aa = m_nodes[ "Hananoki.SharedModule.RuntimeEditor" ];
			//var bb = m_nodes[ "Hananoki.SharedModule.Runtime" ];

			//contentContainer.Add( aa.rightPort.ConnectTo( bb.leftPort ) );

			// 参照アセンブリに対して線を引いていく
			foreach( var asm in assemblies ) {
				if( !m_nodes.ContainsKey( asm.name ) ) continue;

				var portR = m_nodes[ asm.name ].rightPort;
				foreach( var depend in asm.assemblyReferences ) {
					if( !m_nodes.ContainsKey( depend.name ) ) continue;

					var portL = m_nodes[ depend.name ].leftPort;
					contentContainer.Add( portR.ConnectTo( portL ) );
				}
			}

			// ポートの名前に依存数を追記
			foreach( var node in m_nodes.Values ) {
				node.RefreshPortName();
			}

			//contentContainer.Add( node.rightPort.ConnectTo( node2.leftPort ) );
			//contentContainer.Add( node.rightPort.ConnectTo( node3.leftPort ) );
			//node.rightPort.portName = "接続キュ";

			//node.transform.position = new Vector3(0,0,0);
			//node2.transform.position = new Vector3( 10, 0, 0 );
			//node3.transform.position = new Vector3( 20, 0, 0 );

			singleton.instance.reload = true;
			singleton.instance.reloadNodeClick = true;
		}
	}
}
