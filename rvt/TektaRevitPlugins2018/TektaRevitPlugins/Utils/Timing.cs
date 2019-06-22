namespace TektaRevitPlugins
{
    class Timing
    {
        #region Data fields
        System.TimeSpan startingTime;
        System.TimeSpan duration;
        #endregion

        #region Properties
        public System.TimeSpan Duration {
            get { return this.duration; }
        }
        #endregion

        #region Constructors
        public Timing() {
            startingTime = new System.TimeSpan(0);
            duration = new System.TimeSpan(0);
        }
        #endregion

        #region Methods
        public void StartTime() {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            startingTime = System.Diagnostics.Process
                            .GetCurrentProcess()
                            .Threads[0]
                            .UserProcessorTime;
        }

        public void StopTime() {
            duration = System.Diagnostics
                .Process.GetCurrentProcess()
                .Threads[0]
                .UserProcessorTime
                .Subtract(startingTime);
        }

        #endregion
    }
}
