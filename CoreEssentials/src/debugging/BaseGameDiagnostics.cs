using System;
using System.Diagnostics;

namespace CoreEssentials.Debugging
{
    /// <summary>
    /// Provides performance diagnostics for game loops, tracking update, draw, and fixed update timing.
    /// This class helps monitor performance and display metrics like FPS and frame times.
    /// </summary>
    public class BaseGameDiagnostics
    {
        /// <summary>
        /// Stopwatches for measuring elapsed time in different game loop phases.
        /// </summary>
        private Stopwatch _updateStopwatch, _drawStopwatch, _fixedUpdateStopwatch;
        
        /// <summary>
        /// Arrays for storing recent timing samples to calculate averages.
        /// </summary>
        private float[] updateSample, drawSample, fixedUpdateSample;
        
        /// <summary>
        /// Current index positions in the sample arrays for storing new measurements.
        /// </summary>
        private int updateSampleIndex, drawSampleIndex, fixedUpdateIndex;

        /// <summary>
        /// Gets the average update time in milliseconds.
        /// </summary>
        public float UpdateAvg { private set; get; }
        
        /// <summary>
        /// Gets the average draw time in milliseconds.
        /// </summary>
        public float DrawAvg { private set; get; }

        /// <summary>
        /// Gets the average fixed update time in milliseconds.
        /// </summary>
        public float FixedUpdateAvg { private set; get; }

        /// <summary>
        /// Reference to the StickyLog used to display diagnostic information.
        /// </summary>
        private StickyLog stickyLog;

        /// <summary>
        /// Initializes a new instance of the BaseGameDiagnostics class.
        /// </summary>
        /// <param name="stickyLog">The StickyLog to use for displaying diagnostic information.</param>
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

        /// <summary>
        /// Begins measurement of update time by starting the update stopwatch.
        /// </summary>
        public void UpdateBegin()
        {
            _updateStopwatch.Restart();
        }

        /// <summary>
        /// Ends measurement of update time, calculates the average, and displays information in the StickyLog.
        /// </summary>
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

        /// <summary>
        /// Begins measurement of fixed update time by starting the fixed update stopwatch.
        /// </summary>
        public void FixedUpdateBegin()
        {
            _fixedUpdateStopwatch.Restart();
        }

        /// <summary>
        /// Ends measurement of fixed update time and calculates the average.
        /// </summary>
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

        /// <summary>
        /// Begins measurement of draw time by starting the draw stopwatch.
        /// </summary>
        public void DrawBegin()
        {
            _drawStopwatch.Restart();
        }

        /// <summary>
        /// Ends measurement of draw time and calculates the average.
        /// </summary>
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
