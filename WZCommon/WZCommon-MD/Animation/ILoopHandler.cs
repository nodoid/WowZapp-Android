// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Collections.Generic;

namespace WZCommon
{
	public interface ILoopHandler
	{

		void OnStart();

		void OnUpdate(double timestamp, IEnumerable<LayerInfo> layers, IEnumerable<AnimationAudioInfo> audioItems);

		void OnStop(double timestamp);

		void PrepareAudioBuffers();
	}
}

