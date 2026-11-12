#if UNITY_EDITOR
using System.IO;

/*
 * VRSuya Core
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace VRSuya.Core {

	public static class AssetUtility {

		public static string GetAssetName(string TargetPath, bool OnlyFileName) {
			return OnlyFileName ? Path.GetFileNameWithoutExtension(TargetPath) : Path.GetFileName(TargetPath);
		}

		public static string GetDirectoryPath(string TargetAssetPath) {
			return Path.GetDirectoryName(TargetAssetPath).Replace('\\', '/');
		}
	}
}
#endif