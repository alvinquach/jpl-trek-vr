
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrekVRApplication {

    /// <summary>
    ///     Not thread safe.
    /// </summary>
    public sealed class ScrollTransformer {

        private readonly Queue<Vector2> _inputSamples = new Queue<Vector2>();

        private readonly Queue<Vector2> _outputSamples = new Queue<Vector2>();

        public int SamplingRate { get; set; } = 10;

        public Vector4 InputBounds { get; set; } = new Vector4(-1, -1, 1, 1);

        public Vector4 OutputBounds { get; set; } = new Vector4(-1, -1, 1, 1);

        public bool clampInput = true, enableX = true, enableY = true;

        public bool HasOutputSamples {
            get { return _outputSamples.Count > 0; }
        }

        public void AddInputSample(Vector2 sample) {
            _inputSamples.Enqueue(sample);
            if (_inputSamples.Count >= SamplingRate) {
                Vector2 sum = Vector2.zero;
                for (int i = 0; i < SamplingRate; i++) {
                    sum += _inputSamples.Dequeue();
                }
                Vector2 average = sum / SamplingRate;

                // TODO Perform input/output mapping

                _outputSamples.Enqueue(average);
            }
        }

        public Vector2 PopOutputSample() {
            if (!HasOutputSamples) {
                throw new Exception("No output samples are available.");
            }
            return _outputSamples.Dequeue();
        }

        private float MapInputOutput(float input, float inMin, float inMax, float outMin, float outMax) {
            if (clampInput) {
                input = Mathf.Clamp(input, inMin, inMax);
            }
            float position = (input - inMin) / (inMax - inMin);
            return position * (outMax - outMin) + outMin;
        }

    }

}
