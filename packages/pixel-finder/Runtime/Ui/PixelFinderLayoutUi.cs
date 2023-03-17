using UnityEngine;

namespace Sasaki.Unity.Ui
{
	public class PixelFinderLayoutUi : MonoBehaviour, ILinkableUi<IPixelLayout>
	{
		[SerializeField] GameObject _pixelFinderUiPrefab;
		
		
		public void Link(IPixelLayout obj)
		{
			if(obj == default)return;
			
			foreach (var finder in obj.Finders)
			{
				if (finder == null) return;

				var finderUi = Instantiate(_pixelFinderUiPrefab, transform).GetComponent<PixelFinderUi>();

				finderUi.Link(finder);
			}
			
		}
	}
}