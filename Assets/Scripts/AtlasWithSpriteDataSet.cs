namespace UnityEngine.U2D
{
    [System.Serializable]
    public struct AtlasWithSpriteDataSet
    {
        public SpriteAtlas atlas;
        public string spriteName;
        
        public Sprite sprite
        {
            get
            {
                if (atlas == null)
                {
                    return null;
                }
                return atlas.GetSprite(spriteName);
            }
        }
    }
}