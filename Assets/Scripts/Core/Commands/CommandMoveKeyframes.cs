﻿using UnityEngine;

namespace VRtist
{
    /// <summary>
    /// Command to move a keyframe of an object.
    /// </summary>
    public class CommandMoveKeyframes : CommandGroup
    {
        readonly GameObject gObject;

        public CommandMoveKeyframes(GameObject obj, int frame, int newFrame) : base("Move Keyframes")
        {
            gObject = obj;
            AnimationSet animationSet = GlobalState.Animation.GetObjectAnimation(obj);
            if (null == animationSet)
                return;
            foreach (Curve curve in animationSet.curves.Values)
            {
                new CommandMoveKeyframe(gObject, curve.property, frame, newFrame).Submit();
            }
        }

        public override void Undo()
        {
            base.Undo();
        }

        public override void Redo()
        {
            base.Redo();

        }
        public override void Submit()
        {
            base.Submit();
        }
    }
}
