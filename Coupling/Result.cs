namespace ideal.coupling
{
    /// <summary>
    /// Represents the result of an interaction with a unique label.
    /// </summary>
    public class Result
    {
        private readonly string _label;

        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class.
        /// </summary>
        /// <param name="label">The unique label for the result.</param>
        public Result(string label)
        {
            if (string.IsNullOrEmpty(label))
                throw new System.ArgumentNullException("label");
            _label = label;
        }

        /// <summary>
        /// Gets the label of the result.
        /// </summary>
        /// <returns>The result's label.</returns>
        public string GetLabel()
        {
            return _label;
        }
    }
}
