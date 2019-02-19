using NUnit.Framework;
using System.Reflection;
using TrekVRApplication;

namespace Tests {

    public class ScrollTransformerTests {

        private ScrollTransformer _scrollTransformer;

        [SetUp]
        public void Setup() {
            _scrollTransformer = new ScrollTransformer();
        }

        [Test]
        public void MapInputOutput_OneToOneA_CorrectResults() {
            float result = InvokeMapInputOutput(0.5f, -1, 1, -1, 1);
            Assert.AreEqual(0.5f, result);
        }

        [Test]
        public void MapInputOutput_OneToOneB_CorrectResults() {
            float result = InvokeMapInputOutput(1.5f, 1, 2, 1, 2);
            Assert.AreEqual(1.5f, result);
        }

        [Test]
        public void MapInputOutput_OneToOneAClamped_CorrectResults() {
            float result = InvokeMapInputOutput(3, -1, 1, -1, 1);
            Assert.AreEqual(1.0f, result);
        }

        [Test]
        public void MapInputOutput_OneToOneBClamped_CorrectResults() {
            float result = InvokeMapInputOutput(0.5f, 1, 2, 1, 2);
            Assert.AreEqual(1.0f, result);
        }

        [Test]
        public void MapInputOutput_OneToOneAUnclamped_CorrectResults() {
            _scrollTransformer.clampInput = false;
            float result = InvokeMapInputOutput(3, -1, 1, -1, 1);
            Assert.AreEqual(3.0f, result);
        }

        [Test]
        public void MapInputOutput_OneToOneBUnclamped_CorrectResults() {
            _scrollTransformer.clampInput = false;
            float result = InvokeMapInputOutput(0.5f, 1, 2, 1, 2);
            Assert.AreEqual(0.5f, result);
        }

        [Test]
        public void MapInputOutput_MappingA_CorrectResults() {
            float result = InvokeMapInputOutput(0.5f, -1, 1, 0, 2);
            Assert.AreEqual(1.5f, result);
        }

        [Test]
        public void MapInputOutput_MappingB_CorrectResults() {
            float result = InvokeMapInputOutput(0.5f, -1, 1, 2, -2);
            Assert.AreEqual(-1.0f, result);
        }

        private float InvokeMapInputOutput(float input,float inMin, float inMax, float outMin, float outMax) {
            MethodInfo method = _scrollTransformer.GetType().GetMethod("MapInputOutput", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] args = new object[] { input, inMin, inMax, outMin, outMax };
            object result = method.Invoke(_scrollTransformer, args);
            return (float)result;
        }


    }

}
