using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace Timeline.ImageSprite
{
    [Serializable]
    public class ImagePlayableBehaviour : PlayableBehaviour
    {
        public Sprite Image;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            Image image = playerData as Image;
            if (image == null)
                return;

            image.sprite = Image;
        }
    }

}
