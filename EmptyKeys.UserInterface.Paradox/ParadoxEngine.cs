﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmptyKeys.UserInterface.Input;
using EmptyKeys.UserInterface.Media;
using EmptyKeys.UserInterface.Renderers;
using SiliconStudio.Paradox.Games;

namespace EmptyKeys.UserInterface
{
    /// <summary>
    /// Implements Paradox specific engine
    /// </summary>
    public class ParadoxEngine : Engine
    {
        private Renderer renderer;
        private AudioDevice audioDevice = new ParadoxAudioDevice();
        private AssetManager assetManager = new ParadoxAssetManager();
        private InputDeviceBase inputDevice = new ParadoxInputDevice();

        /// <summary>
        /// Gets the renderer.
        /// </summary>
        /// <value>
        /// The renderer.
        /// </value>
        public override Renderer Renderer
        {
            get { return renderer; }
        }

        /// <summary>
        /// Gets the audio device.
        /// </summary>
        /// <value>
        /// The audio device.
        /// </value>
        public override AudioDevice AudioDevice
        {
            get { return audioDevice; }
        }

        /// <summary>
        /// Gets the asset manager.
        /// </summary>
        /// <value>
        /// The asset manager.
        /// </value>
        public override AssetManager AssetManager
        {
            get { return assetManager; }
        }

        /// <summary>
        /// Gets the input device.
        /// </summary>
        /// <value>
        /// The input device.
        /// </value>
        public override InputDeviceBase InputDevice
        {
            get { return inputDevice; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParadoxEngine"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="nativeScreenWidth">Width of the native screen.</param>
        /// <param name="nativeScreenHeight">Height of the native screen.</param>
        public ParadoxEngine(GraphicsDeviceManager manager, int nativeScreenWidth, int nativeScreenHeight)
            : base()
        {
            renderer = new ParadoxRenderer(manager);
        }        
    }
}
