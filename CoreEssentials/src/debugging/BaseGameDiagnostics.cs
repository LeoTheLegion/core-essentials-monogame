using System;
using System.Diagnostics;

namespace CoreEssentials.Debugging
{
    public class BaseGameDiagnostics
    {
        private Stopwatch _updateStopwatch, _drawStopwatch, _fixedUpdateStopwatch;
        private float[] updateSample, drawSample,fixedUpdateSample;
        private int updateSampleIndex, drawSampleIndex, fixedUpdateIndex;

        public float UpdateAvg { private set; get; }
        public float DrawAvg { private set; get; }

        public float FixedUpdateAvg { private set; get; }


        private StickyLog stickyLog;

        public BaseGameDiagnostics(StickyLog stickyLog)
        {
            _updateStopwatch = new Stopwatch();
            updateSample = new float[20];
            updateSampleIndex = 0;

            _drawStopwatch = new Stopwatch();
            drawSample = new float[20];
            drawSampleIndex = 0;
            
            _fixedUpdateStopwatch = new Stopwatch();
            fixedUpdateSample = new float[20];
            fixedUpdateIndex = 0;

            this.stickyLog = stickyLog;
        }

        public void UpdateBegin()
        {
            _updateStopwatch.Restart();
        }

        public void UpdateEnd()
        {
            _updateStopwatch.Stop();

            updateSample[updateSampleIndex] = (float)_updateStopwatch.Elapsed.TotalMilliseconds;
            updateSampleIndex = ++updateSampleIndex % updateSample.Length;

            float time = 0;

            for (int i = 0; i < updateSample.Length; i++)
            {
                time += updateSample[i];
            }

            time /= updateSample.Length;

            this.UpdateAvg = time;


            this.stickyLog.Log("Update Time", String.Format("{0}ms", Math.Round(UpdateAvg,2)));
            this.stickyLog.Log("Fixed Update Time", String.Format("{0}ms", Math.Round(FixedUpdateAvg, 2)));
            this.stickyLog.Log("Draw Time", String.Format("{0}ms", Math.Round(DrawAvg, 2)));
            this.stickyLog.Log("FPS", String.Format("{0}fps", Math.Round(1000 / DrawAvg)));
        }

        public void FixedUpdateBegin()
        {
            _fixedUpdateStopwatch.Restart();
        }

        public void FixedUpdateEnd()
        {
            _fixedUpdateStopwatch.Stop();

            fixedUpdateSample[fixedUpdateIndex] = (float)_fixedUpdateStopwatch.Elapsed.TotalMilliseconds;
            fixedUpdateIndex = ++fixedUpdateIndex % fixedUpdateSample.Length;

            float time = 0;

            for (int i = 0; i < fixedUpdateSample.Length; i++)
            {
                time += fixedUpdateSample[i];
            }

            time /= fixedUpdateSample.Length;

            this.FixedUpdateAvg = time;
        }

        public void DrawBegin()
        {
            _drawStopwatch.Restart();
        }

        public void DrawEnd()
        {
            _drawStopwatch.Stop();

            drawSample[drawSampleIndex] = (float)_drawStopwatch.Elapsed.TotalMilliseconds;
            drawSampleIndex = ++drawSampleIndex % drawSample.Length;

            float time = 0;

            for (int i = 0; i < drawSample.Length; i++)
            {
                time += drawSample[i];
            }

            time /= drawSample.Length;

            this.DrawAvg = time;
        }
    }
}
