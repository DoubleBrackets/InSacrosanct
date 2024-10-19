using Timeline.Samples;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace Timeline.ImageSprite
{
    [System.Serializable]
    public class ImagePlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        [NoFoldOut]
        [NotKeyable] 
        public ImagePlayableBehaviour Template = new ImagePlayableBehaviour();

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<ImagePlayableBehaviour>.Create(graph, Template);
        }
    }

}
