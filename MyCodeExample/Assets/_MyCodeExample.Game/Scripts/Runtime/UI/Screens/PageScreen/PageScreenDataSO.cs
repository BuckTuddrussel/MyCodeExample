using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyCodeExample.Game.UI.Screens
{
    [CreateAssetMenu(fileName = nameof(PageScreenDataSO), menuName = "Custom/Create New " + nameof(PageScreenDataSO), order = 0)]
    public sealed class PageScreenDataSO : ScriptableObject
    {
        [SerializeField] private ImageData[] _imagesData = Array.Empty<ImageData>();

        private Dictionary<DataItem.CategoryType, Sprite> _spritesMap;

        public void Init()
        {
            _spritesMap = new Dictionary<DataItem.CategoryType, Sprite>();

            foreach (var entry in _imagesData)
            {
                _spritesMap.Add(entry.CategoryType, entry.Sprite);
            }
        }

        public Sprite GetCategorySprite(DataItem.CategoryType categoryType)
        {
            return _spritesMap[categoryType];
        }

        [Serializable]
        private class ImageData
        {
            public DataItem.CategoryType CategoryType;
            public Sprite Sprite;
        }
    }
}