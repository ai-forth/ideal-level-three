namespace ideal.coupling
{
    /// <summary>
    /// Represents an experiment with a unique label.
    /// </summary>
    public class Experiment
    {
        private readonly string _label;

        /// <summary>
        /// Initializes a new instance of the <see cref="Experiment"/> class.
        /// </summary>
        /// <param name="label">The unique label for the experiment.</param>
        public Experiment(string label)
        {
            if (string.IsNullOrEmpty(label))
                throw new System.ArgumentNullException("label");
            _label = label;
        }

        /// <summary>
        /// Gets the label of the experiment.
        /// </summary>
        /// <returns>The experiment's label.</returns>
        public string GetLabel()
        {
            return _label;
        }
    }
}
