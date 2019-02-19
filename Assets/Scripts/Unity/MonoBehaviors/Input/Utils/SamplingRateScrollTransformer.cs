
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrekVRApplication {

    /// <summary>
    ///     Not thread safe.
    /// </summary>
    class SamplingRateScrollTransformer : ScrollTransformer {

        private readonly Queue<Vector2> _outputSamples = new Queue<Vector2>();

        public int SamplingRate { get; set; } = 3;

        public override bool OutputSampleReady {
            get { return _outputSamples.Count > 0; }
        }

        public override void AddInputSample(Vector2 sample) {
            base.AddInputSample(sample);
            if (_inputSamples.Count >= SamplingRate) {
                Vector2 output = CalculateOutput(SamplingRate);
                //Debug.Log($"Scroll: ({output.x}, {output.y})");
                _outputSamples.Enqueue(output);
            }
        }

        public override Vector2 GetOutputSample() {
            if (!OutputSampleReady) {
                throw new Exception("No output samples are available.");
            }
            return _outputSamples.Dequeue();
        }

    }

}
