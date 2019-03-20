using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI {

	public class Contour : Shadow {

		[Range (0.0f, 3.0f)]
		public float m_size = 1.0f;

		/*public bool glintEffect;


		[RangeAttribute (0, 5)]
		public int glintVertex = 0;
		[RangeAttribute (0, 3)]
		public int glintWidth = 0;
		public Color glintColor; */

		public override void ModifyMesh (VertexHelper vh) {
			if (!IsActive ())
				return;
			var verts = new List<UIVertex> ();

			vh.GetUIVertexStream (verts);

			var neededCpacity = verts.Count * 5;
			if (verts.Capacity < neededCpacity)
				verts.Capacity = neededCpacity;


			/*if (glintEffect) {
				for (int i = 0; i < verts.Count; i++) {
					UIVertex item = verts [i];

					for (int j = -glintWidth; j <= glintWidth; j++) {

						if (i % 6 == Mathf.Repeat (glintVertex + j, 6))
							item.color = glintColor;
					}

					verts [i] = item;
				}
			}*/

			Vector2 m_effectDistance = new Vector2 (m_size, m_size);
			var start = 0;
			var end = verts.Count;
			ApplyShadowZeroAlloc (verts, effectColor, start, verts.Count, m_effectDistance.x, m_effectDistance.y);

			start = end;
			end = verts.Count;
			ApplyShadowZeroAlloc (verts, effectColor, start, verts.Count, m_effectDistance.x, -m_effectDistance.y);

			start = end;
			end = verts.Count;
			ApplyShadowZeroAlloc (verts, effectColor, start, verts.Count, -m_effectDistance.x, m_effectDistance.y);

			start = end;
			end = verts.Count;
			ApplyShadowZeroAlloc (verts, effectColor, start, verts.Count, -m_effectDistance.x, -m_effectDistance.y);

			//////////////////////////////
			start = end;
			end = verts.Count;
			ApplyShadowZeroAlloc (verts, effectColor, start, verts.Count, 0, m_effectDistance.y * 1.5f);

			start = end;
			end = verts.Count;
			ApplyShadowZeroAlloc (verts, effectColor, start, verts.Count, m_effectDistance.x * 1.5f, 0);

			start = end;
			end = verts.Count;
			ApplyShadowZeroAlloc (verts, effectColor, start, verts.Count, -m_effectDistance.x * 1.5f, 0);

			start = end;
			end = verts.Count;
			ApplyShadowZeroAlloc (verts, effectColor, start, verts.Count, 0, -m_effectDistance.y * 1.5f);

			vh.Clear ();
			vh.AddUIVertexTriangleStream (verts);

		}

	}
}
