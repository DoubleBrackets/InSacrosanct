using Timeline.Samples;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace Timeline.ImageSprite
{
    [TrackClipType(typeof(ImagePlayableAsset))]
    [TrackBindingType(typeof(Image))]
    public class ImageTrack : TrackAsset
    {

        
        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            Image trackBinding = director.GetGenericBinding(this) as Image;
            if (trackBinding == null)
                return;
        
            driver.AddFromName<Image>(trackBinding.gameObject, "m_Sprite");

            base.GatherProperties(director, driver);
        }
    }

}
