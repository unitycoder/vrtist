﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace VRtist
{
    public class CommandSky : ICommand
    {
        SkySettings oldSky;
        SkySettings newSky;
        public CommandSky(SkySettings oldSky, SkySettings newSky)
        {
            this.oldSky = oldSky;
            this.newSky = newSky;
        }
        public override void Undo()
        {
            GlobalState.Instance.SkySettings = oldSky;
            MixerClient.GetInstance().SendEvent<SkySettings>(MessageType.Sky, oldSky);

        }
        public override void Redo()
        {
            GlobalState.Instance.SkySettings = newSky;
            MixerClient.GetInstance().SendEvent<SkySettings>(MessageType.Sky, newSky);
        }
        public override void Submit()
        {
            Redo();
            CommandManager.AddCommand(this);
        }

        public override void Serialize(SceneSerializer serializer)
        {

        }
    }
}