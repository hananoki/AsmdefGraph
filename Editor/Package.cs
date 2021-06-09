
namespace HananokiEditor.AsmdefGraph {
  public static class Package {
    public const string reverseDomainName = "com.hananoki.asmdef-graph";
    public const string name = "AsmdefGraph";
    public const string nameNicify = "Asmdef Graph";
    public const string editorPrefName = "Hananoki.AsmdefGraph";
    public const string version = "0.2.3";
		[HananokiEditorMDViewerRegister]
		public static string MDViewerRegister() {
			return "2510bf44735f1aa419689bbb7ebf6400";
		}
  }
}
