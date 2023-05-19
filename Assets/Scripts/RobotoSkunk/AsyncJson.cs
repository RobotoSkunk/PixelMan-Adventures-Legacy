/*
	PixelMan Adventures, an open source platformer game.
	Copyright (C) 2022  RobotoSkunk <contact@robotoskunk.com>

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Affero General Public License as published
	by the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Affero General Public License for more details.

	You should have received a copy of the GNU Affero General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using UnityEngine;

using Cysharp.Threading.Tasks;


namespace RobotoSkunk {
	public static class AsyncJson {
		public static async UniTask<T> FromJson<T>(string json) {
			return await UniTask.RunOnThreadPool(() => JsonUtility.FromJson<T>(json));
		}

		public static async UniTask<string> ToJson<T>(T obj) {
			return await UniTask.RunOnThreadPool(() => JsonUtility.ToJson(obj));
		}

		public static async UniTask FromJsonOverwrite<T>(string json, T obj) {
			await UniTask.RunOnThreadPool(() => JsonUtility.FromJsonOverwrite(json, obj));
		}
	}
}
