using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrekVRApplication {

    /// <summary>
    ///     Not thread safe.
    /// </summary>
    public class ScrollTransformer {

        protected readonly Queue<Vector2> _inputSamples = new Queue<Vector2>();

        public Vector4 InputBounds { get; set; } = new Vector4(-1, -1, 1, 1);

        public Vector4 OutputBounds { get; set; } = new Vector4(-1, -1, 1, 1);

        public bool clampInput = true, enableX = true, enableY = true;

        public virtual bool OutputSampleReady {
            get => _inputSamples.Count > 0;
        }

        public virtual void AddInputSample(Vector2 sample) {
            _inputSamples.Enqueue(sample);
        }

        public virtual Vector2 GetOutputSample() {
            try {
                return CalculateOutput(_inputSamples.Count);
            }
            catch (Exception e) {
                throw e;
            }
        }

        protected Vector2 CalculateOutput(int sampleCount) {
            if (_inputSamples.Count == 0 || _inputSamples.Count < sampleCount) {
                throw new Exception("Not enough input samples are available.");
            }
            Vector2 sum = Vector2.zero;
            for (int i = 0; i < sampleCount; i++) {
                sum += _inputSamples.Dequeue();
            }
            Vector2 average = sum / sampleCount;
            Vector2 mapped = new Vector2(
                enableX ? MapInputOutput(average.x, InputBounds[0], InputBounds[2], OutputBounds[0], OutputBounds[2]) : 0,
                enableY ? MapInputOutput(average.y, InputBounds[1], InputBounds[3], OutputBounds[1], OutputBounds[3]) : 0
            );
            return mapped;
        }

        protected float MapInputOutput(float input, float inMin, float inMax, float outMin, float outMax) {
            if (clampInput) {
                input = Mathf.Clamp(input, inMin, inMax);
            }
            float position = (input - inMin) / (inMax - inMin);
            return position * (outMax - outMin) + outMin;
        }

    }

}
